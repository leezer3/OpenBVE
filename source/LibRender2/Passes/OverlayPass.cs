using System;
using System.Collections.Generic;
using System.Linq;
using LibRender2.Cameras;
using LibRender2.Objects;
using LibRender2.Pipeline;
using LibRender2.Viewports;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Passes
{
	/// <summary>
	/// Renders the overlays, including the train cab (2D/3D) and the user interface.
	/// </summary>
	public class OverlayPass : IRenderPass
	{
		private readonly Action<RenderContext> renderUiAction;

		/// <summary>
		/// Initializes a new OverlayPass.
		/// </summary>
		/// <param name="renderUiAction">A callback to render the application-specific UI.</param>
		public OverlayPass(Action<RenderContext> renderUiAction)
		{
			this.renderUiAction = renderUiAction;
		}

		public void Execute(RenderContext context)
		{
			BaseRenderer renderer = context.Renderer;

			// 1. Setup for Cab/Interior layer
			renderer.Fog.Enabled = false;
			renderer.UpdateViewport(ViewportChangeMode.ChangeToCab);

			if (renderer.AvailableNewRenderer)
			{
				renderer.DefaultShader.Activate();
				renderer.ResetShader(renderer.DefaultShader);
				renderer.DefaultShader.SetCurrentProjectionMatrix(renderer.CurrentProjectionMatrix);
			}

			renderer.CurrentViewMatrix = Matrix4D.LookAt(Vector3.Zero, new Vector3(context.Camera.AbsoluteDirection.X, context.Camera.AbsoluteDirection.Y, -context.Camera.AbsoluteDirection.Z), new Vector3(context.Camera.AbsoluteUp.X, context.Camera.AbsoluteUp.Y, -context.Camera.AbsoluteUp.Z));
			context.ViewMatrix = renderer.CurrentViewMatrix;

			if (renderer.AvailableNewRenderer)
			{
				renderer.DefaultShader.SetCurrentViewMatrix(renderer.CurrentViewMatrix);
			}

			List<FaceState> overlayOpaqueFaces, overlayAlphaFaces;
			lock (renderer.Scene.VisibleObjects.LockObject)
			{
				overlayOpaqueFaces = renderer.Scene.VisibleObjects.OverlayOpaqueFaces.ToList();
				overlayAlphaFaces = renderer.Scene.VisibleObjects.GetSortedPolygons(true);
			}

			if (context.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable || context.Camera.CurrentRestriction == CameraRestrictionMode.Restricted3D)
			{
				// 3D Cab Rendering
				renderer.ResetOpenGlState();
				GL.Clear(ClearBufferMask.DepthBufferBit);
				renderer.OptionLighting = true;

				Color24 prevOptionAmbientColor = renderer.Lighting.OptionAmbientColor;
				Color24 prevOptionDiffuseColor = renderer.Lighting.OptionDiffuseColor;
				renderer.Lighting.OptionAmbientColor = Color24.LightGrey;
				renderer.Lighting.OptionDiffuseColor = Color24.LightGrey;

				if (renderer.AvailableNewRenderer)
				{
					renderer.DefaultShader.SetIsLight(true);
					Vector3 lightPos = new Vector3(renderer.Lighting.OptionLightPosition.X, renderer.Lighting.OptionLightPosition.Y, -renderer.Lighting.OptionLightPosition.Z);
					renderer.DefaultShader.SetLightPosition(lightPos);
					renderer.DefaultShader.SetLightAmbient(renderer.Lighting.OptionAmbientColor);
					renderer.DefaultShader.SetLightDiffuse(renderer.Lighting.OptionDiffuseColor);
					renderer.DefaultShader.SetLightSpecular(renderer.Lighting.OptionSpecularColor);
					renderer.DefaultShader.SetLightModel(renderer.Lighting.LightModel);
				}

				// Render opaque faces
				foreach (FaceState face in overlayOpaqueFaces)
				{
					face.Draw();
				}

				// Render alpha faces
				renderer.ResetOpenGlState();
				if (renderer.currentOptions.TransparencyMode == TransparencyMode.Performance)
				{
					renderer.SetBlendFunc();
					renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
					renderer.SetDepthMask(false);

					foreach (FaceState face in overlayAlphaFaces)
					{
						face.Draw();
					}
				}
				else
				{
					renderer.UnsetBlendFunc();
					renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
					renderer.SetDepthMask(true);

					foreach (FaceState face in overlayAlphaFaces)
					{
						var material = face.Object.Prototype.Mesh.Materials[face.Face.Material];
						if (material.BlendMode == MeshMaterialBlendMode.Normal && material.GlowAttenuationData == 0)
						{
							if (material.Color.A == 255)
							{
								face.Draw();
							}
						}
					}

					renderer.SetBlendFunc();
					renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
					renderer.SetDepthMask(false);
					bool additive = false;

					foreach (FaceState face in overlayAlphaFaces)
					{
						var material = face.Object.Prototype.Mesh.Materials[face.Face.Material];
						if (material.BlendMode == MeshMaterialBlendMode.Additive)
						{
							if (!additive)
							{
								renderer.UnsetAlphaFunc();
								additive = true;
							}
						}
						else
						{
							if (additive)
							{
								renderer.SetAlphaFunc();
								additive = false;
							}
						}
						face.Draw();
					}
				}

				renderer.Lighting.OptionAmbientColor = prevOptionAmbientColor;
				renderer.Lighting.OptionDiffuseColor = prevOptionDiffuseColor;
				renderer.Lighting.Initialize();
			}
			else
			{
				// 2D Cab Rendering
				renderer.ResetOpenGlState();
				renderer.OptionLighting = false;
				if (renderer.AvailableNewRenderer)
				{
					renderer.DefaultShader.SetIsLight(false);
				}

				renderer.SetBlendFunc();
				renderer.UnsetAlphaFunc();
				renderer.SetDepthTest(true);
				renderer.SetDepthMask(true);

				foreach (FaceState face in overlayOpaqueFaces)
				{
					face.Draw();
				}

				renderer.SetDepthTest(false);
				renderer.SetDepthMask(false);

				foreach (FaceState face in overlayAlphaFaces)
				{
					face.Draw();
				}
			}

			// 2. Render UI and other overlays
			renderer.OptionLighting = false;
			renderer.ResetOpenGlState();
			renderer.SetBlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
			renderer.SetDepthTest(false);

			if (renderer.AvailableNewRenderer)
			{
				renderer.CurrentViewMatrix = Matrix4D.Identity;
				renderer.DefaultShader.SetCurrentViewMatrix(renderer.CurrentViewMatrix);
			}

			renderUiAction?.Invoke(context);
		}
	}
}
