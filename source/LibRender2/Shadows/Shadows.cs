using System;
using System.Collections.Generic;
using LibRender2.Objects;
using LibRender2.Shaders;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.ShadowMapping
{
	/// <summary>
	/// Manages the Cascaded Shadow Mapping (CSM) system, including depth passes and shader binding.
	/// </summary>
	public class Shadows
	{
		private readonly BaseRenderer renderer;

		/// <summary>The shadow map textures and framebuffers.</summary>
		internal CascadedShadowMap Map { get; private set; }
		/// <summary>The math engine for computing cascade frustums and matrices.</summary>
		internal CascadedShadowCaster Caster { get; private set; }
		/// <summary>The shader used for rendering the shadow depth pass.</summary>
		internal ShadowDepthShader DepthShader { get; private set; }

		/// <summary>Whether shadows are currently active.</summary>
		public bool Enabled { get; private set; }
		/// <summary>The current darkness of the shadows (0.0 to 1.0).</summary>
		public float Strength { get; private set; }

		/// <summary>Whether shadows actually render this frame: enabled by the user AND the sun is up.</summary>
		/// <remarks>Every reader (depth pass, shader bind, background restore) must use this, never Enabled directly.</remarks>
		internal bool EffectiveEnabled => Enabled && IsSunUp();

		// Reference tuning for ComputeEffectiveFilterRadius: Medium preset (300m / 3 cascades) with 2048px maps.
		private const double ReferenceDistPerCascade = 100.0;
		private const double ReferenceResolution = 2048.0;
		private const double MinDistFactor = 0.7;
		private const double MaxDistFactor = 1.8;
		private const double MinResFactor = 0.7;
		private const double MaxResFactor = 1.4;
		private const double MinFilterRadius = 0.5;
		private const double MaxFilterRadius = 3.0;
		private const float MinNormalBiasTexels = 0.0f;
		private const float MaxNormalBiasTexels = 4.0f;

		/// <summary>Extra ortho depth around a cascade sphere to catch tall occluders.</summary>
		private const double CasterDepthMargin = 150.0;
		/// <summary>Per-cascade cull margin so long shadows from tall objects are kept.</summary>
		/// <remarks>Tuned to the same value as <see cref="CasterDepthMargin"/> but acts on view-space distance, not ortho depth.</remarks>
		private const double CascadeCullMargin = 150.0;
		/// <summary>Near clip used when fitting cascade frustums.</summary>
		private const double CascadeNearClip = 0.1;

		public Shadows(BaseRenderer renderer)
		{
			this.renderer = renderer;
		}

		/// <summary>
		/// Initializes (or reinitializes) shadow mapping from current options.
		/// </summary>
		public void Initialize()
		{
			if (!TryResolveSettings(out int resolution, out int cascadeCount, out double shadowDistance))
			{
				return;
			}

			try
			{
				AllocateResources(resolution, cascadeCount);
				ApplyTuning(resolution, shadowDistance);

				Enabled = true;
				renderer.fileSystem.AppendToLogFile($"[CSM] Initialized: {cascadeCount} cascades, {resolution}×{resolution}, distance={shadowDistance}m, strength={Strength:P0}");
			}
			catch (Exception ex)
			{
				renderer.fileSystem.AppendToLogFile($"[CSM] Init failed: {ex.Message}");
				Enabled = false;
				GL.GetError();
			}
		}

		/// <summary>Reads shadow settings from current options.</summary>
		/// <returns>False when shadows are disabled and no resources should be allocated.</returns>
		private bool TryResolveSettings(out int resolution, out int cascadeCount, out double shadowDistance)
		{
			var opts = renderer.currentOptions;

			if (opts.ShadowResolution == ShadowMapResolution.Off)
			{
				Dispose();
				Enabled = false;
				renderer.fileSystem.AppendToLogFile("[CSM] Shadows disabled by user setting.");
				resolution = 0;
				cascadeCount = 0;
				shadowDistance = 0.0;
				return false;
			}

			resolution = Math.Max(1, (int)opts.ShadowResolution);
			cascadeCount = (int)opts.ShadowCascades;
			shadowDistance = opts.ShadowDrawDistance == ShadowDistance.ViewingDistance ? opts.ViewingDistance : (double)(int)opts.ShadowDrawDistance;
			shadowDistance = Math.Max(1.0, shadowDistance);
			Strength = (float)opts.ShadowStrength;
			return true;
		}

		/// <summary>Creates or resizes GPU resources for the given settings.</summary>
		private void AllocateResources(int resolution, int cascadeCount)
		{
			if (Map == null)
			{
				Map = new CascadedShadowMap(cascadeCount, resolution);
			}
			else
			{
				Map.Resize(cascadeCount, resolution);
			}

			if (Caster == null || cascadeCount != Caster.CascadeCount)
			{
				Caster = new CascadedShadowCaster(cascadeCount);
			}

			if (DepthShader == null)
			{
				DepthShader = new ShadowDepthShader(renderer, "shadow_depth", "shadow_depth", true);
			}
		}

		/// <summary>Applies distance, resolution and bias tuning to the caster.</summary>
		private void ApplyTuning(int resolution, double shadowDistance)
		{
			Caster.ShadowDistance = shadowDistance;
			Caster.Resolution = resolution;
			Caster.SplitLambda = CascadedShadowCaster.DefaultSplitLambda;
			Caster.DepthMargin = CasterDepthMargin;
		}

		/// <summary>
		/// Performs the shadow depth pass for all cascades.
		/// </summary>
		public void RenderPass()
		{
			if (!EffectiveEnabled || Map == null || Caster == null || DepthShader == null)
			{
				return;
			}

			if (!TryGetLightDirection(out Vector3 lightDir))
			{
				return;
			}

			// Update cascade matrices using the camera's current view.
			// The Caster aligns the shadow frustums with the view direction.
			Caster.Resolution = Map.Resolution;
			if (renderer.currentOptions.ShadowDrawDistance == ShadowDistance.ViewingDistance)
			{
				Caster.ShadowDistance = renderer.currentOptions.ViewingDistance;
			}
			Caster.Update(lightDir, renderer.CurrentViewMatrix, renderer.CurrentProjectionMatrix, CascadeNearClip, renderer.Camera.VerticalViewingAngle, renderer.Screen.AspectRatio);

			SetupDepthState();

			for (int i = 0; i < Caster.CascadeCount; i++)
			{
				RenderCascade(i);
			}

			RestoreDepthState();
		}

		/// <summary>
		/// Whether the sun is above the horizon and can cast directional shadows.
		/// </summary>
		/// <remarks>
		/// Below the horizon there is no sunlight, so the depth pass is skipped
		/// and stale cascade matrices are never bound to the scene shader.
		/// </remarks>
		private bool IsSunUp()
		{
			return renderer.Lighting.OptionLightPosition.Y > 0;
		}

		/// <summary>
		/// Resolves the light direction pointing FROM the sun TOWARD the scene.
		/// </summary>
		/// <returns>False when the sun position is degenerate and no pass should run.</returns>
		private bool TryGetLightDirection(out Vector3 lightDir)
		{
			// Sun position is in OpenBVE coordinates (X: right, Y: up, Z: backward).
			// Light direction needs to be negated for some components to match the shadow math.
			lightDir = new Vector3(
				-renderer.Lighting.OptionLightPosition.X,
				-renderer.Lighting.OptionLightPosition.Y,
				renderer.Lighting.OptionLightPosition.Z
			);

			return !lightDir.IsNullVector();
		}

		/// <summary>Sets up GL state for the depth pass.</summary>
		private void SetupDepthState()
		{
			renderer.CurrentShader?.Deactivate();
			DepthShader.Activate();
			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);
			if (renderer.OptionBackFaceCulling)
			{
				GL.Enable(EnableCap.CullFace);
				GL.CullFace(CullFaceMode.Front); // OpenBVE culls Front faces by default
			}
			else
			{
				GL.Disable(EnableCap.CullFace);
			}
			GL.DepthMask(true);
			DepthShader.SetTexture(0);
		}

		/// <summary>Renders all visible faces into a single cascade's depth target.</summary>
		private void RenderCascade(int cascadeIndex)
		{
			Map.BindCascadeForWriting(cascadeIndex);
			GL.Clear(ClearBufferMask.DepthBufferBit);
			DepthShader.SetLightSpaceMatrix(Caster.LightSpaceMatrices[cascadeIndex]);

			lock (renderer.VisibleObjects.LockObject)
			{
				int lastVAO = -1;
				/*
				 * Culling Per-Cascade:
				 * Distant objects don't need to be rendered into near-field high-res shadow maps.
				 * We use a safety margin to catch long shadows from tall objects.
				 */
				double maxDistance = renderer.currentOptions.ShadowFilterCascades ? Caster.SplitDistances[cascadeIndex] + CascadeCullMargin : double.MaxValue;
				double maxDistanceSquared = maxDistance * maxDistance;

				RenderFacesFiltered(renderer.VisibleObjects.OpaqueFaces, ref lastVAO, maxDistanceSquared);
				RenderFacesFiltered(renderer.VisibleObjects.AlphaFaces, ref lastVAO, maxDistanceSquared);
			}
			Map.Unbind();
		}

		/// <summary>Restores GL state after the depth pass.</summary>
		private void RestoreDepthState()
		{
			GL.DepthFunc(DepthFunction.Lequal);
			GL.CullFace(CullFaceMode.Front);
			GL.Viewport(0, 0, renderer.Screen.Width, renderer.Screen.Height);
			// Shadow pass corrupts the GL texture state, so ensure we null it out
			// so the next render pass re-binds what it needs.
			renderer.LastBoundTexture = null;
		}

		/// <summary>
		/// Renders a collection of faces filtered by distance to optimize per-cascade draw calls.
		/// </summary>
		/// <param name="faces">The faces to render.</param>
		/// <param name="lastVAO">A reference to the last bound VAO handle, to avoid redundant binds.</param>
		/// <param name="maxDistanceSquared">The squared maximum distance from camera for an object to cast shadows into this cascade.</param>
		private void RenderFacesFiltered(IEnumerable<FaceState> faces, ref int lastVAO, double maxDistanceSquared)
		{
			Vector3 cameraPos = renderer.Camera.AbsolutePosition;

			foreach (var face in faces)
			{
				if (!IsShadowCaster(face, cameraPos, maxDistanceSquared, out ObjectState state, out MeshMaterial material))
				{
					continue;
				}

				BindDepthMaterial(state, material);
				DrawDepthFace(face, state, ref lastVAO);
			}
		}

		/// <summary>Checks VAO, shadow flags and per-cascade distance culling.</summary>
		/// <returns>False when the face must not cast shadows into this cascade.</returns>
		private static bool IsShadowCaster(FaceState face, Vector3 cameraPos, double maxDistanceSquared, out ObjectState state, out MeshMaterial material)
		{
			state = face.Object;
			material = default(MeshMaterial);

			if (state.Prototype.Mesh.VAO == null || state.DisableShadowCasting)
			{
				return false;
			}

			if (maxDistanceSquared < double.MaxValue)
			{
				double dx = state.WorldPosition.X - cameraPos.X;
				double dy = state.WorldPosition.Y - cameraPos.Y;
				double dz = state.WorldPosition.Z - cameraPos.Z;

				if (dx * dx + dy * dy + dz * dz > maxDistanceSquared)
				{
					return false;
				}
			}

			material = state.Prototype.Mesh.Materials[face.Face.Material];
			if ((material.Flags & MaterialFlags.NoShadow) != 0 || material.BlendMode == MeshMaterialBlendMode.Additive)
			{
				return false;
			}

			return true;
		}

		/// <summary>Binds model, texture and alpha state for the depth pass.</summary>
		private void BindDepthMaterial(ObjectState state, MeshMaterial material)
		{
			DepthShader.SetModelMatrix(state.ModelMatrix * renderer.Camera.TranslationMatrix);
			DepthShader.SetTextureMatrix(state.TextureTranslation);

			if (material.DaytimeTexture != null && renderer.currentHost.LoadTexture(ref material.DaytimeTexture, (OpenGlTextureWrapMode)(material.WrapMode ?? OpenGlTextureWrapMode.ClampClamp)))
			{
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, material.DaytimeTexture.OpenGlTextures[(int)(material.WrapMode ?? OpenGlTextureWrapMode.ClampClamp)].Name);
				DepthShader.SetHasTexture(true);
			}
			else
			{
				DepthShader.SetHasTexture(false);
			}

			DepthShader.SetAlphaCutoff(0.5f);
			DepthShader.SetMaterialAlpha(material.Color.A / 255.0f);
			DepthShader.SetMaterialFlags(material.Flags);
		}

		/// <summary>Draws a single face into the depth target, minimizing VAO switches.</summary>
		private void DrawDepthFace(FaceState face, ObjectState state, ref int lastVAO)
		{
			if (state.Matricies != null && state.Matricies.Length > 0)
			{
				DepthShader.SetCurrentAnimationMatricies(state);
				GL.BindBufferBase(BufferTarget.UniformBuffer, 0, state.MatrixBufferIndex);
			}

			VertexArrayObject vao = (VertexArrayObject)face.Object.Prototype.Mesh.VAO;
			if (vao.handle != lastVAO)
			{
				vao.Bind();
				lastVAO = vao.handle;
			}
			if (renderer.OptionBackFaceCulling)
			{
				if ((face.Face.Flags & FaceFlags.Face2Mask) != 0)
				{
					// Double-sided faces (Face2) must not be culled to ensure they cast shadows from both sides
					GL.Disable(EnableCap.CullFace);
				}
				else
				{
					GL.Enable(EnableCap.CullFace);
				}
			}
			PrimitiveType drawMode = renderer.GetPrimitiveType(face.Face.Flags);
			vao.Draw(drawMode, face.Face.IboStartIndex, face.Face.Vertices.Length);
		}

		/// <summary>
		/// Binds shadow data to the main scene shader.
		/// </summary>
		public void Bind(Shader shader)
		{
			if (!EffectiveEnabled || Map == null || Caster == null)
			{
				shader.SetShadowEnabled(false);
				BindNullDepthMaps();
				return;
			}

			shader.Activate();
			shader.SetShadowEnabled(true);
			shader.SetShadowStrength((float)renderer.currentOptions.ShadowStrength);
			shader.SetShadowSmooth(renderer.currentOptions.ShadowSmooth);
			shader.SetShadowFilterRadius(ComputeEffectiveFilterRadius());
			shader.SetCurrentViewMatrix(renderer.CurrentViewMatrix);

			Map.BindAllCascadesForReading(TextureUnit.Texture4);

			int cascadeCount = Caster.CascadeCount;
			// Normal bias is in Unity-style texel units (typical 0.3-1.0). Clamp so a stale
			// config (old 2.0x multiplier default) can't detach shadows catastrophically.
			float normalBiasTexels = Math.Max(MinNormalBiasTexels, Math.Min(MaxNormalBiasTexels, (float)renderer.currentOptions.ShadowNormalBias));
			for (int i = 0; i < cascadeCount; i++)
			{
				shader.SetCascadeLightSpaceMatrix(i, Caster.LightSpaceMatrices[i]);
				shader.SetCascadeShadowMapUnit(i, ShadowSamplerUnit(i));
				// Split distance = the view-space Z where this cascade ends.
				shader.SetShadowSplitDistance(i, (float)Caster.SplitDistances[i]);
				shader.SetCascadeBias(i, Caster.CascadeBiases[i] + (float)renderer.currentOptions.ShadowBias);
				shader.SetNormalBias(i, normalBiasTexels);
				shader.SetTexelWorldSize(i, Caster.TexelWorldSizes[i]);
			}

			// Clear unused cascade slots so stale data from a previous cascade count can't leak in.
			for (int i = cascadeCount; i < ShadowConstants.MaxCascadeCount; i++)
			{
				shader.SetShadowSplitDistance(i, 0.0f);
				shader.SetTexelWorldSize(i, cascadeCount > 0 ? Caster.TexelWorldSizes[cascadeCount - 1] : ShadowConstants.FallbackTexelWorldSize);
			}
			shader.SetShadowCascadeCount(cascadeCount);
		}

		/// <summary>Resolves the texture unit reserved for a shadow cascade.</summary>
		private static TextureUnit ShadowTextureUnit(int cascadeIndex)
		{
			return ShadowConstants.FirstShadowUnit + cascadeIndex;
		}

		/// <summary>Resolves the sampler index matching a shadow cascade's texture unit.</summary>
		private static int ShadowSamplerUnit(int cascadeIndex)
		{
			return ShadowConstants.FirstShadowSamplerIndex + cascadeIndex;
		}

		/// <summary>
		/// Binds the dummy depth texture to all shadow units so strict drivers
		/// never see colliding samplers when shadows are disabled.
		/// </summary>
		private void BindNullDepthMaps()
		{
			for (int i = 0; i < ShadowConstants.MaxCascadeCount; i++)
			{
				GL.ActiveTexture(ShadowTextureUnit(i));
				GL.BindTexture(TextureTarget.Texture2D, renderer.nullDepthMap);
			}
			GL.ActiveTexture(TextureUnit.Texture0);
		}

		/// <summary>
		/// Auto-scales the filter radius by cascade count, shadow distance and resolution
		/// so softness stays perceptually consistent: far distance / few cascades / high res → slightly larger texel radius.
		/// </summary>
		private float ComputeEffectiveFilterRadius()
		{
			double baseRadius = renderer.currentOptions.ShadowFilterRadius;

			double shadowDist = renderer.currentOptions.ShadowDrawDistance == ShadowDistance.ViewingDistance
				? renderer.currentOptions.ViewingDistance
				: (double)(int)renderer.currentOptions.ShadowDrawDistance;
			int optCascadeCount = Math.Max(1, (int)renderer.currentOptions.ShadowCascades);
			int res = Math.Max(512, (int)renderer.currentOptions.ShadowResolution);

			double actualDistPerCascade = shadowDist / optCascadeCount;
			double distFactor = Math.Sqrt(actualDistPerCascade / ReferenceDistPerCascade);
			distFactor = Math.Max(MinDistFactor, Math.Min(MaxDistFactor, distFactor));

			double resFactor = Math.Sqrt((double)res / ReferenceResolution);
			resFactor = Math.Max(MinResFactor, Math.Min(MaxResFactor, resFactor));

			double effectiveRadius = baseRadius * distFactor * resFactor;
			effectiveRadius = Math.Max(MinFilterRadius, Math.Min(MaxFilterRadius, effectiveRadius));
			return (float)effectiveRadius;
		}

		/// <summary>
		/// Disposes shadow resources.
		/// </summary>
		public void Dispose()
		{
			Map?.Dispose();
			Map = null;
			DepthShader?.Dispose();
			DepthShader = null;
			Caster = null;
			Enabled = false;
		}
	}
}
