using OpenBveShared;
using OpenBveApi.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenBveApi.Objects;

namespace OpenBve
{
	internal static partial class Renderer
	{
		
		internal static OutputMode CurrentOutputMode = OutputMode.Default;
		internal static OutputMode PreviousOutputMode = OutputMode.Default;

		// interface options
		/// <summary>Whether the clock overlay is currently displayed</summary>
		internal static bool OptionClock = false;

		internal enum GradientDisplayMode
		{
			Percentage, UnitOfChange, Permil, None
		}
		/// <summary>Whether the gradient overlay is currently displayed</summary>
		internal static GradientDisplayMode OptionGradient = GradientDisplayMode.None;
		internal enum SpeedDisplayMode
		{
			None, Kmph, Mph
		}
		internal static SpeedDisplayMode OptionSpeed = SpeedDisplayMode.None;
		internal enum DistanceToNextStationDisplayMode
		{
			None, Km, Mile
		}
		internal static DistanceToNextStationDisplayMode OptionDistanceToNextStation = DistanceToNextStationDisplayMode.None;
		internal static bool OptionFrameRates = false;
		internal static bool OptionBrakeSystems = false;

		// fade to black
		private static double FadeToBlackDueToChangeEnds = 0.0;

		// constants
		private const float inv255 = 1.0f / 255.0f;

		// render scene
		
		internal static void RenderScene(double TimeElapsed)
		{
			// initialize
			OpenBveShared.Renderer.ResetOpenGlState();
			int OpenGlTextureIndex = 0;

			if (OpenBveShared.Renderer.OptionWireframe | OpenGlTextureIndex == 0)
			{
				if (OpenBveShared.Renderer.CurrentFog.Start < OpenBveShared.Renderer.CurrentFog.End)
				{
					const float fogdistance = 600.0f;
					float n = (fogdistance - OpenBveShared.Renderer.CurrentFog.Start) / (OpenBveShared.Renderer.CurrentFog.End - OpenBveShared.Renderer.CurrentFog.Start);
					float cr = n * inv255 * (float)OpenBveShared.Renderer.CurrentFog.Color.R;
					float cg = n * inv255 * (float)OpenBveShared.Renderer.CurrentFog.Color.G;
					float cb = n * inv255 * (float)OpenBveShared.Renderer.CurrentFog.Color.B;
					GL.ClearColor(cr, cg, cb, 1.0f);
				}
				else
				{
					GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
				}
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			}
			else
			{
				GL.Clear(ClearBufferMask.DepthBufferBit);
			}
			GL.PushMatrix();
			OpenBveShared.Renderer.UpdateViewport(OpenBveShared.Renderer.ViewPortChangeMode.ChangeToScenery);
			// set up camera
			double dx = Camera.AbsoluteCameraDirection.X;
			double dy = Camera.AbsoluteCameraDirection.Y;
			double dz = Camera.AbsoluteCameraDirection.Z;
			double ux = Camera.AbsoluteCameraUp.X;
			double uy = Camera.AbsoluteCameraUp.Y;
			double uz = Camera.AbsoluteCameraUp.Z;
			Matrix4d lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);
			GL.Light(LightName.Light0, LightParameter.Position, new float[] { (float)OpenBveShared.Renderer.OptionLightPosition.X, (float)OpenBveShared.Renderer.OptionLightPosition.Y, (float)OpenBveShared.Renderer.OptionLightPosition.Z, 0.0f });
			// fog
			double fd = OpenBveShared.Renderer.NextFog.TrackPosition - OpenBveShared.Renderer.PreviousFog.TrackPosition;
			if (fd != 0.0)
			{
				float fr = (float)((World.CameraTrackFollower.TrackPosition - OpenBveShared.Renderer.PreviousFog.TrackPosition) / fd);
				float frc = 1.0f - fr;
				OpenBveShared.Renderer.CurrentFog.Start = OpenBveShared.Renderer.PreviousFog.Start * frc + OpenBveShared.Renderer.NextFog.Start * fr;
				OpenBveShared.Renderer.CurrentFog.End = OpenBveShared.Renderer.PreviousFog.End * frc + OpenBveShared.Renderer.NextFog.End * fr;
				OpenBveShared.Renderer.CurrentFog.Color.R = (byte)((float)OpenBveShared.Renderer.PreviousFog.Color.R * frc + (float)OpenBveShared.Renderer.NextFog.Color.R * fr);
				OpenBveShared.Renderer.CurrentFog.Color.G = (byte)((float)OpenBveShared.Renderer.PreviousFog.Color.G * frc + (float)OpenBveShared.Renderer.NextFog.Color.G * fr);
				OpenBveShared.Renderer.CurrentFog.Color.B = (byte)((float)OpenBveShared.Renderer.PreviousFog.Color.B * frc + (float)OpenBveShared.Renderer.NextFog.Color.B * fr);
			}
			else
			{
				OpenBveShared.Renderer.CurrentFog = OpenBveShared.Renderer.PreviousFog;
			}
			// render background
			if (OpenBveShared.Renderer.FogEnabled)
			{
				GL.Disable(EnableCap.Fog); OpenBveShared.Renderer.FogEnabled = false;
			}
			GL.Disable(EnableCap.DepthTest);
			OpenBveShared.Renderer.UpdateBackground(Game.SecondsSinceMidnight, TimeElapsed);
			RenderEvents(Camera.AbsoluteCameraPosition);
			// fog
			float aa = OpenBveShared.Renderer.CurrentFog.Start;
			float bb = OpenBveShared.Renderer.CurrentFog.End;
			if (aa < bb & aa < OpenBveShared.Renderer.BackgroundImageDistance)
			{
				if (!OpenBveShared.Renderer.FogEnabled)
				{
					GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
				}
				GL.Fog(FogParameter.FogStart, aa);
				GL.Fog(FogParameter.FogEnd, bb);
				GL.Fog(FogParameter.FogColor, new float[] { inv255 * (float)OpenBveShared.Renderer.CurrentFog.Color.R, inv255 * (float)OpenBveShared.Renderer.CurrentFog.Color.G, inv255 * (float)OpenBveShared.Renderer.CurrentFog.Color.B, 1.0f });
				if (!OpenBveShared.Renderer.FogEnabled)
				{
					GL.Enable(EnableCap.Fog); OpenBveShared.Renderer.FogEnabled = true;
				}
			}
			else if (OpenBveShared.Renderer.FogEnabled)
			{
				GL.Disable(EnableCap.Fog); OpenBveShared.Renderer.FogEnabled = false;
			}
			// world layer
			bool optionLighting = OpenBveShared.Renderer.OptionLighting;
			OpenBveShared.Renderer.LastBoundTexture = null;
			if (OpenBveShared.Renderer.OptionLighting)
			{
				if (!OpenBveShared.Renderer.LightingEnabled)
				{
					GL.Enable(EnableCap.Lighting); OpenBveShared.Renderer.LightingEnabled = true;
				}
				if (Camera.CameraRestriction == CameraRestrictionMode.NotAvailable)
				{
					GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { inv255 * (float)OpenBveShared.Renderer.OptionAmbientColor.R, inv255 * (float)OpenBveShared.Renderer.OptionAmbientColor.G, inv255 * (float)OpenBveShared.Renderer.OptionAmbientColor.B, 1.0f });
					GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { inv255 * (float)OpenBveShared.Renderer.OptionDiffuseColor.R, inv255 * (float)OpenBveShared.Renderer.OptionDiffuseColor.G, inv255 * (float)OpenBveShared.Renderer.OptionDiffuseColor.B, 1.0f });
				}
			}
			else if (OpenBveShared.Renderer.LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting); OpenBveShared.Renderer.LightingEnabled = false;
			}
			// static opaque
			if (Interface.CurrentOptions.DisableDisplayLists)
			{
				OpenBveShared.Renderer.ResetOpenGlState();
				for (int i = 0; i < OpenBveShared.Renderer.StaticOpaque.Length; i++)
				{
					if (OpenBveShared.Renderer.StaticOpaque[i] != null)
					{
						if (OpenBveShared.Renderer.StaticOpaque[i].List != null)
						{
							for (int j = 0; j < OpenBveShared.Renderer.StaticOpaque[i].List.FaceCount; j++)
							{
								if (OpenBveShared.Renderer.StaticOpaque[i].List.Faces[j] != null)
								{
									OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.StaticOpaque[i].List.Faces[j], Camera.AbsoluteCameraPosition);
								}
							}
						}
					}
				}
			}
			else
			{
				for (int i = 0; i < OpenBveShared.Renderer.StaticOpaque.Length; i++)
				{
					if (OpenBveShared.Renderer.StaticOpaque[i] != null)
					{
						if (OpenBveShared.Renderer.StaticOpaque[i].Update | OpenBveShared.Renderer.StaticOpaqueForceUpdate)
						{
							OpenBveShared.Renderer.StaticOpaque[i].Update = false;
							if (OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable)
							{
								GL.DeleteLists(OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayList, 1);
								OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable = false;
							}
							if (OpenBveShared.Renderer.StaticOpaque[i].List.FaceCount != 0)
							{
								OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayList = GL.GenLists(1);
								OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable = true;
								OpenBveShared.Renderer.ResetOpenGlState();
								GL.NewList(OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayList, ListMode.Compile);
								for (int j = 0; j < OpenBveShared.Renderer.StaticOpaque[i].List.FaceCount; j++)
								{
									if (OpenBveShared.Renderer.StaticOpaque[i].List.Faces[j] != null)
									{
										OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.StaticOpaque[i].List.Faces[j], Camera.AbsoluteCameraPosition);
									}
								}
								GL.EndList();
							}

							OpenBveShared.Renderer.StaticOpaque[i].WorldPosition = Camera.AbsoluteCameraPosition;
						}
					}
				}

				OpenBveShared.Renderer.StaticOpaqueForceUpdate = false;
				for (int i = 0; i < OpenBveShared.Renderer.StaticOpaque.Length; i++)
				{
					if (OpenBveShared.Renderer.StaticOpaque[i] != null && OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable)
					{
						OpenBveShared.Renderer.ResetOpenGlState();
						GL.PushMatrix();
						GL.Translate(OpenBveShared.Renderer.StaticOpaque[i].WorldPosition.X - Camera.AbsoluteCameraPosition.X, OpenBveShared.Renderer.StaticOpaque[i].WorldPosition.Y - Camera.AbsoluteCameraPosition.Y, OpenBveShared.Renderer.StaticOpaque[i].WorldPosition.Z - Camera.AbsoluteCameraPosition.Z);
						GL.CallList(OpenBveShared.Renderer.StaticOpaque[i].OpenGlDisplayList);
						GL.PopMatrix();
					}
				}
				//Update bounding box positions now we've rendered the objects
				int currentBox = 0;
				for (int i = 0; i < OpenBveShared.Renderer.StaticOpaque.Length; i++)
				{
					if (OpenBveShared.Renderer.StaticOpaque[i] != null)
					{
						currentBox++;

					}
				}
			}
			// dynamic opaque
			OpenBveShared.Renderer.ResetOpenGlState();
			for (int i = 0; i < OpenBveShared.Renderer.DynamicOpaque.FaceCount; i++)
			{
				OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.DynamicOpaque.Faces[i], Camera.AbsoluteCameraPosition);
			}
			// dynamic alpha
			OpenBveShared.Renderer.ResetOpenGlState();
			SortPolygons(OpenBveShared.Renderer.DynamicAlpha);
			if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
			{
				GL.Enable(EnableCap.Blend); OpenBveShared.Renderer.BlendEnabled = true;
				GL.DepthMask(false);
				OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
				for (int i = 0; i < OpenBveShared.Renderer.DynamicAlpha.FaceCount; i++)
				{
					OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.DynamicAlpha.Faces[i], Camera.AbsoluteCameraPosition);
				}
			}
			else
			{
				GL.Disable(EnableCap.Blend); OpenBveShared.Renderer.BlendEnabled = false;
				OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				GL.DepthMask(true);
				for (int i = 0; i < OpenBveShared.Renderer.DynamicAlpha.FaceCount; i++)
				{
					int r = (int)GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Faces[OpenBveShared.Renderer.DynamicAlpha.Faces[i].FaceIndex].Material;
					if (GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].GlowAttenuationData == 0)
					{
						if (GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].Color.A == 255)
						{
							OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.DynamicAlpha.Faces[i], Camera.AbsoluteCameraPosition);
						}
					}
				}
				GL.Enable(EnableCap.Blend); OpenBveShared.Renderer.BlendEnabled = true;
				OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
				GL.DepthMask(false);
				bool additive = false;
				for (int i = 0; i < OpenBveShared.Renderer.DynamicAlpha.FaceCount; i++)
				{
					int r = (int)GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Faces[OpenBveShared.Renderer.DynamicAlpha.Faces[i].FaceIndex].Material;
					if (GameObjectManager.Objects[OpenBveShared.Renderer.DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
					{
						if (!additive)
						{
							OpenBveShared.Renderer.UnsetAlphaFunc();
							additive = true;
						}

						OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.DynamicAlpha.Faces[i], Camera.AbsoluteCameraPosition);
					}
					else
					{
						if (additive)
						{
							OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
							additive = false;
						}

						OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.DynamicAlpha.Faces[i], Camera.AbsoluteCameraPosition);
					}
				}
			}
			// motion blur
			GL.Disable(EnableCap.DepthTest);
			GL.DepthMask(false);
			OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
			if (Interface.CurrentOptions.MotionBlur != Interface.MotionBlurMode.None)
			{
				if (OpenBveShared.Renderer.LightingEnabled)
				{
					GL.Disable(EnableCap.Lighting);
					OpenBveShared.Renderer.LightingEnabled = false;
				}
				RenderFullscreenMotionBlur();
			}
			// overlay layer
			if (OpenBveShared.Renderer.FogEnabled)
			{
				GL.Disable(EnableCap.Fog); OpenBveShared.Renderer.FogEnabled = false;
			}
			GL.LoadIdentity();
			OpenBveShared.Renderer.UpdateViewport(OpenBveShared.Renderer.ViewPortChangeMode.ChangeToCab);
			lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);
			if (Camera.CameraRestriction == CameraRestrictionMode.NotAvailable)
			{
				// 3d cab
				OpenBveShared.Renderer.ResetOpenGlState(); // TODO: inserted
				GL.DepthMask(true);
				GL.Enable(EnableCap.DepthTest);
				GL.Clear(ClearBufferMask.DepthBufferBit);
				if (!OpenBveShared.Renderer.LightingEnabled)
				{
					GL.Enable(EnableCap.Lighting); OpenBveShared.Renderer.LightingEnabled = true;
				}
				OpenBveShared.Renderer.OptionLighting = true;
				GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.7f, 0.7f, 0.7f, 1.0f });
				GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 0.7f, 0.7f, 0.7f, 1.0f });
				// overlay opaque
				OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.9f);
				for (int i = 0; i < OpenBveShared.Renderer.OverlayOpaque.FaceCount; i++)
				{
					OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.OverlayOpaque.Faces[i], Camera.AbsoluteCameraPosition);
				}
				// overlay alpha
				SortPolygons(OpenBveShared.Renderer.OverlayAlpha);
				if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
				{
					GL.Enable(EnableCap.Blend); OpenBveShared.Renderer.BlendEnabled = true;
					GL.DepthMask(false);
					OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
					for (int i = 0; i < OpenBveShared.Renderer.OverlayAlpha.FaceCount; i++)
					{
						OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.OverlayAlpha.Faces[i], Camera.AbsoluteCameraPosition);
					}
				}
				else
				{
					GL.Disable(EnableCap.Blend); OpenBveShared.Renderer.BlendEnabled = false;
					OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
					GL.DepthMask(true);
					for (int i = 0; i < OpenBveShared.Renderer.OverlayAlpha.FaceCount; i++)
					{
						int r = (int)GameObjectManager.Objects[OpenBveShared.Renderer.OverlayAlpha.Faces[i].ObjectIndex].Mesh.Faces[OpenBveShared.Renderer.OverlayAlpha.Faces[i].FaceIndex].Material;
						if (GameObjectManager.Objects[OpenBveShared.Renderer.OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & GameObjectManager.Objects[OpenBveShared.Renderer.OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].GlowAttenuationData == 0)
						{
							if (GameObjectManager.Objects[OpenBveShared.Renderer.OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].Color.A == 255)
							{
								OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.OverlayAlpha.Faces[i], Camera.AbsoluteCameraPosition);
							}
						}
					}
					GL.Enable(EnableCap.Blend); OpenBveShared.Renderer.BlendEnabled = true;
					OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
					GL.DepthMask(false);
					bool additive = false;
					for (int i = 0; i < OpenBveShared.Renderer.OverlayAlpha.FaceCount; i++)
					{
						int r = (int)GameObjectManager.Objects[OpenBveShared.Renderer.OverlayAlpha.Faces[i].ObjectIndex].Mesh.Faces[OpenBveShared.Renderer.OverlayAlpha.Faces[i].FaceIndex].Material;
						if (GameObjectManager.Objects[OpenBveShared.Renderer.OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
						{
							if (!additive)
							{
								OpenBveShared.Renderer.UnsetAlphaFunc();
								additive = true;
							}

							OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.OverlayAlpha.Faces[i], Camera.AbsoluteCameraPosition);
						}
						else
						{
							if (additive)
							{
								OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
								additive = false;
							}

							OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.OverlayAlpha.Faces[i], Camera.AbsoluteCameraPosition);
						}
					}
				}
			}
			else
			{
				/*
                 * Render 2D Cab
                 * This is actually an animated object generated on the fly and held in memory
                 */
				if (OpenBveShared.Renderer.LightingEnabled)
				{
					GL.Disable(EnableCap.Lighting); OpenBveShared.Renderer.LightingEnabled = false; // TODO: was 'true' before
				}
				OpenBveShared.Renderer.OptionLighting = false;
				if (!OpenBveShared.Renderer.BlendEnabled)
				{
					GL.Enable(EnableCap.Blend); OpenBveShared.Renderer.BlendEnabled = true;
				}
				GL.DepthMask(false);
				GL.Disable(EnableCap.DepthTest);
				OpenBveShared.Renderer.UnsetAlphaFunc();
				SortPolygons(OpenBveShared.Renderer.OverlayAlpha);
				for (int i = 0; i < OpenBveShared.Renderer.OverlayAlpha.FaceCount; i++)
				{
					OpenBveShared.Renderer.RenderFace(ref OpenBveShared.Renderer.OverlayAlpha.Faces[i], Camera.AbsoluteCameraPosition);
				}

			}
			// render overlays
			OpenBveShared.Renderer.OptionLighting = optionLighting;
			if (OpenBveShared.Renderer.LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting); OpenBveShared.Renderer.LightingEnabled = false;
			}
			if (OpenBveShared.Renderer.FogEnabled)
			{
				GL.Disable(EnableCap.Fog); OpenBveShared.Renderer.FogEnabled = false;
			}
			if (OpenBveShared.Renderer.BlendEnabled)
			{
				GL.Disable(EnableCap.Blend); OpenBveShared.Renderer.BlendEnabled = false;
			}
			OpenBveShared.Renderer.UnsetAlphaFunc();
			GL.Disable(EnableCap.DepthTest);
			RenderOverlays(TimeElapsed);
			// finalize rendering
			GL.PopMatrix();
		}
	}
}
