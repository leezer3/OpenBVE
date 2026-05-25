using System;
using System.Collections.Generic;
using LibRender2.Objects;
using LibRender2.Pipeline;
using LibRender2.Shaders;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Passes
{
	/// <summary>
	/// Performs the Cascaded Shadow Map (CSM) shadow depth rendering pass.
	/// </summary>
	public class ShadowPass : IRenderPass
	{
		public void Execute(RenderContext context)
		{
			BaseRenderer renderer = context.Renderer;

			if (!renderer.ShadowsEnabled || renderer.CSMShadowMaps == null || renderer.CSMCaster == null || renderer.ShadowDepthShaderProgram == null)
				return;

			// 1. Get light direction pointing FROM the sun TOWARD the scene
			Vector3 lightDir = new Vector3(
				-renderer.Lighting.OptionLightPosition.X,
				-renderer.Lighting.OptionLightPosition.Y,
				-renderer.Lighting.OptionLightPosition.Z
			);

			if (lightDir.IsNullVector())
			{
				return;
			}

			// 2. Update cascade matrices
			renderer.CSMCaster.Resolution = renderer.CSMShadowMaps.Resolution;
			if (renderer.currentOptions.ShadowDrawDistance == ShadowDistance.ViewingDistance)
			{
				renderer.CSMCaster.ShadowDistance = renderer.currentOptions.ViewingDistance;
			}
			else
			{
				renderer.CSMCaster.ShadowDistance = (double)renderer.currentOptions.ShadowDrawDistance;
			}
			renderer.CSMCaster.Update(lightDir, context.ViewMatrix, context.ProjectionMatrix, 0.1, context.Camera.VerticalViewingAngle, renderer.Screen.AspectRatio);

			// 3. Setup rendering state
			renderer.CurrentShader?.Deactivate();
			renderer.ShadowDepthShaderProgram.Activate();
			
			renderer.SetDepthTest(true, DepthFunction.Less);
			renderer.SetCullFace(false);
			renderer.SetDepthMask(true);
			
			renderer.ShadowDepthShaderProgram.SetTexture(0); // always use texture unit 0

			for (int cascade = 0; cascade < renderer.CSMCaster.CascadeCount; cascade++)
			{
				renderer.CSMShadowMaps.BindCascadeForWriting(cascade);
				GL.Clear(ClearBufferMask.DepthBufferBit);
				renderer.ShadowDepthShaderProgram.SetLightSpaceMatrix(renderer.CSMCaster.LightSpaceMatrices[cascade]);

				lock (renderer.Scene.VisibilityUpdateLock)
				{
					int lastVAOHandle = -1;
					foreach (var face in renderer.Scene.VisibleObjects.OpaqueFaces)
					{
						renderer.RenderFace(renderer.ShadowDepthShaderProgram, face.Object, face.Face);
					}

					foreach (var face in renderer.Scene.VisibleObjects.AlphaFaces)
					{
						renderer.RenderFace(renderer.ShadowDepthShaderProgram, face.Object, face.Face);
					}

					// Dynamic objects (trains) are rendered regardless of camera visibility to ensure they always cast shadows
					foreach (var state in renderer.Scene.DynamicObjectStates)
					{
						foreach (var face in state.Prototype.Mesh.Faces)
						{
							renderer.RenderFace(renderer.ShadowDepthShaderProgram, state, face);
						}
					}
				}
				renderer.CSMShadowMaps.Unbind();
			}

			// 4. Restore viewport
			renderer.SetViewport(0, 0, renderer.Screen.Width, renderer.Screen.Height);

			renderer.LastBoundTexture = null;
		}

		private PrimitiveType GetPrimitiveType(FaceFlags flags)
		{
			switch (flags & FaceFlags.FaceTypeMask)
			{
				case FaceFlags.Triangles: return PrimitiveType.Triangles;
				case FaceFlags.TriangleStrip: return PrimitiveType.TriangleStrip;
				case FaceFlags.Quads: return PrimitiveType.Quads;
				case FaceFlags.QuadStrip: return PrimitiveType.QuadStrip;
				default: return PrimitiveType.Polygon;
			}
		}
	}
}
