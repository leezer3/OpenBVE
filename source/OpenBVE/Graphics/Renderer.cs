using System;
using LibRender;
using OpenBve.RouteManager;
using OpenBveApi.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Vector3 = OpenBveApi.Math.Vector3;
using OpenBveApi.Objects;
using static LibRender.CameraProperties;

namespace OpenBve
{
	internal static partial class Renderer
	{
		

		// the static opaque lists
		
		/// <summary>Whether to enforce updating all display lists.</summary>
		internal static bool StaticOpaqueForceUpdate = true;

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
			LibRender.Renderer.ResetOpenGlState();

			if (LibRender.Renderer.OptionWireframe)
			{
				if (CurrentRoute.CurrentFog.Start < CurrentRoute.CurrentFog.End)
				{
					const float fogdistance = 600.0f;
					float n = (fogdistance - CurrentRoute.CurrentFog.Start) / (CurrentRoute.CurrentFog.End - CurrentRoute.CurrentFog.Start);
					float cr = n * inv255 * (float)CurrentRoute.CurrentFog.Color.R;
					float cg = n * inv255 * (float)CurrentRoute.CurrentFog.Color.G;
					float cb = n * inv255 * (float)CurrentRoute.CurrentFog.Color.B;
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
			UpdateViewport(ViewPortChangeMode.ChangeToScenery);
			// set up camera
			double dx = Camera.AbsoluteDirection.X;
			double dy = Camera.AbsoluteDirection.Y;
			double dz = Camera.AbsoluteDirection.Z;
			double ux = Camera.AbsoluteUp.X;
			double uy = Camera.AbsoluteUp.Y;
			double uz = Camera.AbsoluteUp.Z;
			Matrix4d lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);
			GL.Light(LightName.Light0, LightParameter.Position, new float[] { (float)LibRender.Renderer.OptionLightPosition.X, (float)LibRender.Renderer.OptionLightPosition.Y, (float)LibRender.Renderer.OptionLightPosition.Z, 0.0f });
			// fog
			double fd = CurrentRoute.NextFog.TrackPosition - CurrentRoute.PreviousFog.TrackPosition;
			if (fd != 0.0)
			{
				float fr = (float)((World.CameraTrackFollower.TrackPosition - CurrentRoute.PreviousFog.TrackPosition) / fd);
				float frc = 1.0f - fr;
				CurrentRoute.CurrentFog.Start = CurrentRoute.PreviousFog.Start * frc + CurrentRoute.NextFog.Start * fr;
				CurrentRoute.CurrentFog.End = CurrentRoute.PreviousFog.End * frc + CurrentRoute.NextFog.End * fr;
				CurrentRoute.CurrentFog.Color.R = (byte)((float)CurrentRoute.PreviousFog.Color.R * frc + (float)CurrentRoute.NextFog.Color.R * fr);
				CurrentRoute.CurrentFog.Color.G = (byte)((float)CurrentRoute.PreviousFog.Color.G * frc + (float)CurrentRoute.NextFog.Color.G * fr);
				CurrentRoute.CurrentFog.Color.B = (byte)((float)CurrentRoute.PreviousFog.Color.B * frc + (float)CurrentRoute.NextFog.Color.B * fr);
			}
			else
			{
				CurrentRoute.CurrentFog = CurrentRoute.PreviousFog;
			}
			// render background

			GL.Disable(EnableCap.DepthTest);
			CurrentRoute.UpdateBackground(TimeElapsed, Game.CurrentInterface != Game.InterfaceType.Normal);
			RenderEvents(Camera.AbsolutePosition);
			// fog
			float aa = CurrentRoute.CurrentFog.Start;
			float bb = CurrentRoute.CurrentFog.End;
			if (aa < bb & aa < Backgrounds.BackgroundImageDistance)
			{
				if (!LibRender.Renderer.FogEnabled)
				{
					GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
				}
				GL.Fog(FogParameter.FogStart, aa);
				GL.Fog(FogParameter.FogEnd, bb);
				GL.Fog(FogParameter.FogColor, new float[] { inv255 * (float)CurrentRoute.CurrentFog.Color.R, inv255 * (float)CurrentRoute.CurrentFog.Color.G, inv255 * (float)CurrentRoute.CurrentFog.Color.B, 1.0f });
				if (!LibRender.Renderer.FogEnabled)
				{
					GL.Enable(EnableCap.Fog); LibRender.Renderer.FogEnabled = true;
				}
			}
			else if (LibRender.Renderer.FogEnabled)
			{
				GL.Disable(EnableCap.Fog); LibRender.Renderer.FogEnabled = false;
			}
			// world layer
			bool optionLighting = LibRender.Renderer.OptionLighting;
			LibRender.Renderer.LastBoundTexture = null;
			if (LibRender.Renderer.OptionLighting)
			{
				if (!LibRender.Renderer.LightingEnabled)
				{
					GL.Enable(EnableCap.Lighting); LibRender.Renderer.LightingEnabled = true;
				}
				if (Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable)
				{
					GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { inv255 * (float) LibRender.Renderer.OptionAmbientColor.R, inv255 * (float) LibRender.Renderer.OptionAmbientColor.G, inv255 * (float) LibRender.Renderer.OptionAmbientColor.B, 1.0f });
					GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { inv255 * (float) LibRender.Renderer.OptionDiffuseColor.R, inv255 * (float) LibRender.Renderer.OptionDiffuseColor.G, inv255 * (float) LibRender.Renderer.OptionDiffuseColor.B, 1.0f });
				}
			}
			else if (LibRender.Renderer.LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting); LibRender.Renderer.LightingEnabled = false;
			}
			// static opaque
			if (Interface.CurrentOptions.DisableDisplayLists)
			{
				LibRender.Renderer.ResetOpenGlState();
				for (int i = 0; i < LibRender.Renderer.StaticOpaque.Length; i++)
				{
					if (LibRender.Renderer.StaticOpaque[i] != null)
					{
						if (LibRender.Renderer.StaticOpaque[i].List != null)
						{
							for (int j = 0; j < LibRender.Renderer.StaticOpaque[i].List.FaceCount; j++)
							{
								if (LibRender.Renderer.StaticOpaque[i].List.Faces[j] != null)
								{
									RenderFace(ref LibRender.Renderer.StaticOpaque[i].List.Faces[j], Camera.AbsolutePosition);
								}
							}
						}
					}
				}
			}
			else
			{
				for (int i = 0; i < LibRender.Renderer.StaticOpaque.Length; i++)
				{
					if (LibRender.Renderer.StaticOpaque[i] != null)
					{
						if (LibRender.Renderer.StaticOpaque[i].Update | StaticOpaqueForceUpdate)
						{
							LibRender.Renderer.StaticOpaque[i].Update = false;
							if (LibRender.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable)
							{
								GL.DeleteLists(LibRender.Renderer.StaticOpaque[i].OpenGlDisplayList, 1);
								LibRender.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable = false;
							}
							if (LibRender.Renderer.StaticOpaque[i].List.FaceCount != 0)
							{
								LibRender.Renderer.StaticOpaque[i].OpenGlDisplayList = GL.GenLists(1);
								LibRender.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable = true;
								LibRender.Renderer.ResetOpenGlState();
								GL.NewList(LibRender.Renderer.StaticOpaque[i].OpenGlDisplayList, ListMode.Compile);
								for (int j = 0; j < LibRender.Renderer.StaticOpaque[i].List.FaceCount; j++)
								{
									if (LibRender.Renderer.StaticOpaque[i].List.Faces[j] != null)
									{
										RenderFace(ref LibRender.Renderer.StaticOpaque[i].List.Faces[j], Camera.AbsolutePosition);
									}
								}
								GL.EndList();
							}

							LibRender.Renderer.StaticOpaque[i].WorldPosition = Camera.AbsolutePosition;
						}
					}
				}
				StaticOpaqueForceUpdate = false;
				for (int i = 0; i < LibRender.Renderer.StaticOpaque.Length; i++)
				{
					if (LibRender.Renderer.StaticOpaque[i] != null && LibRender.Renderer.StaticOpaque[i].OpenGlDisplayListAvailable)
					{
						LibRender.Renderer.ResetOpenGlState();
						GL.PushMatrix();
						GL.Translate(LibRender.Renderer.StaticOpaque[i].WorldPosition.X - Camera.AbsolutePosition.X, LibRender.Renderer.StaticOpaque[i].WorldPosition.Y - Camera.AbsolutePosition.Y, LibRender.Renderer.StaticOpaque[i].WorldPosition.Z - Camera.AbsolutePosition.Z);
						GL.CallList(LibRender.Renderer.StaticOpaque[i].OpenGlDisplayList);
						GL.PopMatrix();
					}
				}
				//Update bounding box positions now we've rendered the objects
				int currentBox = 0;
				for (int i = 0; i < LibRender.Renderer.StaticOpaque.Length; i++)
				{
					if (LibRender.Renderer.StaticOpaque[i] != null)
					{
						currentBox++;

					}
				}
			}
			// dynamic opaque
			LibRender.Renderer.ResetOpenGlState();
			for (int i = 0; i < LibRender.Renderer.DynamicOpaque.FaceCount; i++)
			{
				RenderFace(ref LibRender.Renderer.DynamicOpaque.Faces[i], Camera.AbsolutePosition);
			}
			// dynamic alpha
			LibRender.Renderer.ResetOpenGlState();
			LibRender.Renderer.DynamicAlpha.SortPolygons();
			if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
			{
				GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
				GL.DepthMask(false);
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
				for (int i = 0; i < LibRender.Renderer.DynamicAlpha.FaceCount; i++)
				{
					RenderFace(ref LibRender.Renderer.DynamicAlpha.Faces[i], Camera.AbsolutePosition);
				}
			}
			else
			{
				GL.Disable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = false;
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				GL.DepthMask(true);
				for (int i = 0; i < LibRender.Renderer.DynamicAlpha.FaceCount; i++)
				{
					int r = (int) LibRender.Renderer.DynamicAlpha.Faces[i].ObjectReference.Mesh.Faces[LibRender.Renderer.DynamicAlpha.Faces[i].FaceIndex].Material;
					if (LibRender.Renderer.DynamicAlpha.Faces[i].ObjectReference.Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & LibRender.Renderer.DynamicAlpha.Faces[i].ObjectReference.Mesh.Materials[r].GlowAttenuationData == 0)
					{
						if (LibRender.Renderer.DynamicAlpha.Faces[i].ObjectReference.Mesh.Materials[r].Color.A == 255)
						{
							RenderFace(ref LibRender.Renderer.DynamicAlpha.Faces[i], Camera.AbsolutePosition);
						}
					}
				}
				GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
				GL.DepthMask(false);
				bool additive = false;
				for (int i = 0; i < LibRender.Renderer.DynamicAlpha.FaceCount; i++)
				{
					int r = (int) LibRender.Renderer.DynamicAlpha.Faces[i].ObjectReference.Mesh.Faces[LibRender.Renderer.DynamicAlpha.Faces[i].FaceIndex].Material;
					if (LibRender.Renderer.DynamicAlpha.Faces[i].ObjectReference.Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
					{
						if (!additive)
						{
							LibRender.Renderer.UnsetAlphaFunc();
							additive = true;
						}
						RenderFace(ref LibRender.Renderer.DynamicAlpha.Faces[i], Camera.AbsolutePosition);
					}
					else
					{
						if (additive)
						{
							LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
							additive = false;
						}
						RenderFace(ref LibRender.Renderer.DynamicAlpha.Faces[i], Camera.AbsolutePosition);
					}
				}
			}
			// motion blur
			GL.Disable(EnableCap.DepthTest);
			GL.DepthMask(false);
			LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
			if (Interface.CurrentOptions.MotionBlur != MotionBlurMode.None)
			{
				if (LibRender.Renderer.LightingEnabled)
				{
					GL.Disable(EnableCap.Lighting);
					LibRender.Renderer.LightingEnabled = false;
				}
				LibRender.MotionBlur.RenderFullscreen(Interface.CurrentOptions.MotionBlur, LibRender.Renderer.FrameRate, Math.Abs(Camera.CurrentSpeed));
			}
			// overlay layer
			if (LibRender.Renderer.FogEnabled)
			{
				GL.Disable(EnableCap.Fog); LibRender.Renderer.FogEnabled = false;
			}
			GL.LoadIdentity();
			UpdateViewport(ViewPortChangeMode.ChangeToCab);
			lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);
			if (Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable)
			{
				// 3d cab
				LibRender.Renderer.ResetOpenGlState(); // TODO: inserted
				GL.DepthMask(true);
				GL.Enable(EnableCap.DepthTest);
				GL.Clear(ClearBufferMask.DepthBufferBit);
				if (!LibRender.Renderer.LightingEnabled)
				{
					GL.Enable(EnableCap.Lighting); LibRender.Renderer.LightingEnabled = true;
				}
				LibRender.Renderer.OptionLighting = true;
				GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.7f, 0.7f, 0.7f, 1.0f });
				GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 0.7f, 0.7f, 0.7f, 1.0f });
				// overlay opaque
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.9f);
				for (int i = 0; i < LibRender.Renderer.OverlayOpaque.FaceCount; i++)
				{
					RenderFace(ref LibRender.Renderer.OverlayOpaque.Faces[i], Camera.AbsolutePosition);
				}
				// overlay alpha
				LibRender.Renderer.OverlayAlpha.SortPolygons();
				if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
				{
					GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
					GL.DepthMask(false);
					LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
					for (int i = 0; i < LibRender.Renderer.OverlayAlpha.FaceCount; i++)
					{
						RenderFace(ref LibRender.Renderer.OverlayAlpha.Faces[i], Camera.AbsolutePosition);
					}
				}
				else
				{
					GL.Disable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = false;
					LibRender.Renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
					GL.DepthMask(true);
					for (int i = 0; i < LibRender.Renderer.OverlayAlpha.FaceCount; i++)
					{
						int r = (int) LibRender.Renderer.OverlayAlpha.Faces[i].ObjectReference.Mesh.Faces[LibRender.Renderer.OverlayAlpha.Faces[i].FaceIndex].Material;
						if (LibRender.Renderer.OverlayAlpha.Faces[i].ObjectReference.Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & LibRender.Renderer.OverlayAlpha.Faces[i].ObjectReference.Mesh.Materials[r].GlowAttenuationData == 0)
						{
							if (LibRender.Renderer.OverlayAlpha.Faces[i].ObjectReference.Mesh.Materials[r].Color.A == 255)
							{
								RenderFace(ref LibRender.Renderer.OverlayAlpha.Faces[i], Camera.AbsolutePosition);
							}
						}
					}
					GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
					LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
					GL.DepthMask(false);
					bool additive = false;
					for (int i = 0; i < LibRender.Renderer.OverlayAlpha.FaceCount; i++)
					{
						int r = (int) LibRender.Renderer.OverlayAlpha.Faces[i].ObjectReference.Mesh.Faces[LibRender.Renderer.OverlayAlpha.Faces[i].FaceIndex].Material;
						if (LibRender.Renderer.OverlayAlpha.Faces[i].ObjectReference.Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
						{
							if (!additive)
							{
								LibRender.Renderer.UnsetAlphaFunc();
								additive = true;
							}
							RenderFace(ref LibRender.Renderer.OverlayAlpha.Faces[i], Camera.AbsolutePosition);
						}
						else
						{
							if (additive)
							{
								LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
								additive = false;
							}
							RenderFace(ref LibRender.Renderer.OverlayAlpha.Faces[i], Camera.AbsolutePosition);
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
				if (LibRender.Renderer.LightingEnabled)
				{
					GL.Disable(EnableCap.Lighting); LibRender.Renderer.LightingEnabled = false; // TODO: was 'true' before
				}
				LibRender.Renderer.OptionLighting = false;
				if (!LibRender.Renderer.BlendEnabled)
				{
					GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
				}
				GL.DepthMask(false);
				GL.Disable(EnableCap.DepthTest);
				LibRender.Renderer.UnsetAlphaFunc();
				LibRender.Renderer.OverlayAlpha.SortPolygons();
				for (int i = 0; i < LibRender.Renderer.OverlayAlpha.FaceCount; i++)
				{
					RenderFace(ref LibRender.Renderer.OverlayAlpha.Faces[i], Camera.AbsolutePosition);
				}

			}
			// render overlays
			LibRender.Renderer.OptionLighting = optionLighting;
			if (LibRender.Renderer.LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting); LibRender.Renderer.LightingEnabled = false;
			}
			if (LibRender.Renderer.FogEnabled)
			{
				GL.Disable(EnableCap.Fog); LibRender.Renderer.FogEnabled = false;
			}
			if (LibRender.Renderer.BlendEnabled)
			{
				GL.Disable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = false;
			}
			LibRender.Renderer.UnsetAlphaFunc();
			GL.Disable(EnableCap.DepthTest);
			RenderOverlays(TimeElapsed);
			// finalize rendering
			GL.PopMatrix();
		}

		// set alpha func

		private static void RenderFace(ref ObjectFace Face, Vector3 Camera, bool IsDebugTouchMode = false)
		{
			if (LibRender.Renderer.CullEnabled)
			{
				if (!LibRender.Renderer.OptionBackfaceCulling || (Face.ObjectReference.Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) != 0)
				{
					GL.Disable(EnableCap.CullFace);
					LibRender.Renderer.CullEnabled = false;
				}
			}
			else if (LibRender.Renderer.OptionBackfaceCulling)
			{
				if ((Face.ObjectReference.Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) == 0)
				{
					GL.Enable(EnableCap.CullFace);
					LibRender.Renderer.CullEnabled = true;
				}
			}
			int r = (int)Face.ObjectReference.Mesh.Faces[Face.FaceIndex].Material;
			LibRender.Renderer.RenderFace(ref Face.ObjectReference.Mesh.Materials[r], Face.ObjectReference.Mesh.Vertices, Face.Wrap, ref Face.ObjectReference.Mesh.Faces[Face.FaceIndex], Camera, IsDebugTouchMode);
		}
		
		
	}
}
