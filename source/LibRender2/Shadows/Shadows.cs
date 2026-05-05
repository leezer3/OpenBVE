using System;
using System.Collections.Generic;
using LibRender2.Objects;
using LibRender2.Shaders;
using OpenBveApi.Graphics;
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
		internal CascadedShadowMap Map;
		/// <summary>The math engine for computing cascade frustums and matrices.</summary>
		internal CascadedShadowCaster Caster;
		/// <summary>The shader used for rendering the shadow depth pass.</summary>
		internal ShadowDepthShader DepthShader;

		/// <summary>Whether shadows are currently active.</summary>
		public bool Enabled;
		/// <summary>The current darkness of the shadows (0.0 to 1.0).</summary>
		public float Strength;

		public Shadows(BaseRenderer renderer)
		{
			this.renderer = renderer;
		}

		/// <summary>
		/// Initializes (or reinitializes) shadow mapping from current options.
		/// </summary>
		public void Initialize()
		{
			var opts = renderer.currentOptions;

			if (opts.ShadowResolution == ShadowMapResolution.Off)
			{
				Dispose();
				Enabled = false;
				renderer.fileSystem.AppendToLogFile("[CSM] Shadows disabled by user setting.");
				return;
			}

			int resolution = Math.Max(1, (int)opts.ShadowResolution);
			int cascadeCount = (int)opts.ShadowCascades;
			double shadowDistance = opts.ShadowDrawDistance == ShadowDistance.ViewingDistance ? opts.ViewingDistance : (double)(int)opts.ShadowDrawDistance;
			shadowDistance = Math.Max(1.0, shadowDistance);
			Strength = (float)opts.ShadowStrength;

			try
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

				Caster.ShadowDistance = shadowDistance;
				Caster.Resolution = resolution;
				Caster.SplitLambda = 0.75;
				Caster.DepthMargin = 150.0;

				if (DepthShader == null)
				{
					DepthShader = new ShadowDepthShader(renderer, "shadow_depth", "shadow_depth", true);
				}

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

		/// <summary>
		/// Performs the shadow depth pass for all cascades.
		/// </summary>
		public void RenderPass()
		{
			if (!Enabled || Map == null || Caster == null || DepthShader == null)
			{
				return;
			}

			// 1. Get light direction pointing FROM the sun TOWARD the scene
			// Sun position is in OpenBVE coordinates (X: right, Y: up, Z: backward)
			// Light direction needs to be negated for some components to match the shadow math.
			Vector3 lightDir = new Vector3(
				-renderer.Lighting.OptionLightPosition.X,
				-renderer.Lighting.OptionLightPosition.Y,
				renderer.Lighting.OptionLightPosition.Z
			);

			if (lightDir.IsNullVector())
			{
				return;
			}

			// 2. Update cascade matrices
			// NOTE: We pass renderer.CurrentViewMatrix here which reflects the camera's rotation.
			// The Caster will use this to align the shadow frustums with the view direction.
			Caster.Resolution = Map.Resolution;
			if (renderer.currentOptions.ShadowDrawDistance == ShadowDistance.ViewingDistance)
			{
				Caster.ShadowDistance = renderer.currentOptions.ViewingDistance;
			}
			Caster.Update(lightDir, renderer.CurrentViewMatrix, renderer.CurrentProjectionMatrix, 0.1, renderer.Camera.VerticalViewingAngle, renderer.Screen.AspectRatio);

			// 3. Setup state for depth pass
			renderer.CurrentShader?.Deactivate();
			DepthShader.Activate();
			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);
			GL.Disable(EnableCap.CullFace);
			GL.DepthMask(true);
			DepthShader.SetTexture(0);

			for (int i = 0; i < Caster.CascadeCount; i++)
			{
				Map.BindCascadeForWriting(i);
				GL.Clear(ClearBufferMask.DepthBufferBit);
				DepthShader.SetLightSpaceMatrix(Caster.LightSpaceMatrices[i]);

				lock (renderer.VisibleObjects.LockObject)
				{
					int lastVAO = -1;
					// Render both opaque and alpha-tested geometry (AlphaFaces).
					// Alpha-tested objects (like trees) need to cast shadows for realism.
					RenderFaces(renderer.VisibleObjects.OpaqueFaces, ref lastVAO);
					RenderFaces(renderer.VisibleObjects.AlphaFaces, ref lastVAO); 
				}
				Map.Unbind();
			}

			// 4. Restore state
			GL.DepthFunc(DepthFunction.Lequal);
			GL.CullFace(CullFaceMode.Front);
			GL.Viewport(0, 0, renderer.Screen.Width, renderer.Screen.Height);
			// Shadow pass corrupts the GL texture state, so ensure we null it out
			// so the next render pass re-binds what it needs.
			renderer.LastBoundTexture = null;
		}

		/// <summary>
		/// Renders a collection of faces while minimizing VAO and texture state switches.
		/// </summary>
		/// <param name="faces">The faces to render.</param>
		/// <param name="lastVAO">A reference to the last bound VAO handle, to avoid redundant binds.</param>
		private void RenderFaces(IEnumerable<FaceState> faces, ref int lastVAO)
		{
			foreach (var face in faces)
			{
				if (face.Object.Prototype.Mesh.VAO == null || face.Object.DisableShadowCasting)
				{
					continue;
				}

				ObjectState state = face.Object;
				DepthShader.SetModelMatrix(state.ModelMatrix * renderer.Camera.TranslationMatrix);
				DepthShader.SetTextureMatrix(state.TextureTranslation);

				var material = face.Object.Prototype.Mesh.Materials[face.Face.Material];
				if ((material.Flags & MaterialFlags.NoShadow) != 0 || material.BlendMode == MeshMaterialBlendMode.Additive)
				{
					continue;
				}
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
				PrimitiveType drawMode = renderer.GetPrimitiveType(face.Face.Flags);
				vao.Draw(drawMode, face.Face.IboStartIndex, face.Face.Vertices.Length);
			}
		}

		/// <summary>
		/// Binds shadow data to the main scene shader.
		/// </summary>
		public void Bind(Shader shader)
		{
			if (!Enabled || Map == null || Caster == null)
			{
				shader.SetShadowEnabled(false);
				// To satisfy strict OpenGL drivers, always bind something to the shadow map units
				// even if shadow is disabled, to avoid "sampler collision" errors.
				for (int i = 0; i < 4; i++)
				{
					GL.ActiveTexture(TextureUnit.Texture4 + i);
					GL.BindTexture(TextureTarget.Texture2D, renderer.nullDepthMap);
				}
				GL.ActiveTexture(TextureUnit.Texture0);
				return;
			}

			shader.Activate();
			shader.SetShadowEnabled(true);
			shader.SetShadowStrength((float)renderer.currentOptions.ShadowStrength);
			shader.SetCurrentViewMatrix(renderer.CurrentViewMatrix);

			Map.BindAllCascadesForReading(TextureUnit.Texture4);

			int cascadeCount = Caster.CascadeCount;
			for (int i = 0; i < cascadeCount; i++)
			{
				shader.SetCascadeLightSpaceMatrix(i, Caster.LightSpaceMatrices[i]);
				shader.SetCascadeShadowMapUnit(i, 4 + i);
				// Split distance = the view-space Z where this cascade ends.
				shader.SetShadowSplitDistance(i, (float)Caster.SplitDistances[i]);
				shader.SetCascadeBias(i, Caster.CascadeBiases[i] + (float)renderer.currentOptions.ShadowBias);
				shader.SetNormalBias(i, (float)renderer.currentOptions.ShadowNormalBias);
			}

			for (int i = cascadeCount; i < 4; i++)
			{
				shader.SetShadowSplitDistance(i, 0.0f);
			}
			shader.SetShadowCascadeCount(cascadeCount);
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
