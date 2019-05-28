using LibRender;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Vector3 = OpenBveApi.Math.Vector3;
using OpenBveApi.Objects;

namespace OpenBve
{
	internal static partial class Renderer
	{
		private static ViewPortMode CurrentViewPortMode = ViewPortMode.Scenery;
		internal static OutputMode CurrentOutputMode = OutputMode.Default;
		internal static OutputMode PreviousOutputMode = OutputMode.Default;

		// the static opaque lists
		/// <summary>The list of static opaque face groups. Each group contains only objects that are associated the respective group index.</summary>
		private static ObjectGroup[] StaticOpaque = new ObjectGroup[] { };
		/// <summary>Whether to enforce updating all display lists.</summary>
		internal static bool StaticOpaqueForceUpdate = true;

		// all other lists
		/// <summary>The list of dynamic opaque faces to be rendered.</summary>
		private static ObjectList DynamicOpaque = new ObjectList();
		/// <summary>The list of dynamic alpha faces to be rendered.</summary>
		private static ObjectList DynamicAlpha = new ObjectList();
		/// <summary>The list of overlay opaque faces to be rendered.</summary>
		private static ObjectList OverlayOpaque = new ObjectList();
		/// <summary>The list of overlay alpha faces to be rendered.</summary>
		private static ObjectList OverlayAlpha = new ObjectList();

		// options
		internal static Color24 OptionAmbientColor = new Color24(160, 160, 160);
		internal static Color24 OptionDiffuseColor = new Color24(160, 160, 160);
		internal static Vector3 OptionLightPosition = new Vector3(0.223606797749979f, 0.86602540378444f, -0.447213595499958f);
		internal static bool OptionBackfaceCulling = true;

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
				if (Game.CurrentFog.Start < Game.CurrentFog.End)
				{
					const float fogdistance = 600.0f;
					float n = (fogdistance - Game.CurrentFog.Start) / (Game.CurrentFog.End - Game.CurrentFog.Start);
					float cr = n * inv255 * (float)Game.CurrentFog.Color.R;
					float cg = n * inv255 * (float)Game.CurrentFog.Color.G;
					float cb = n * inv255 * (float)Game.CurrentFog.Color.B;
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
			double dx = World.AbsoluteCameraDirection.X;
			double dy = World.AbsoluteCameraDirection.Y;
			double dz = World.AbsoluteCameraDirection.Z;
			double ux = World.AbsoluteCameraUp.X;
			double uy = World.AbsoluteCameraUp.Y;
			double uz = World.AbsoluteCameraUp.Z;
			Matrix4d lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);
			GL.Light(LightName.Light0, LightParameter.Position, new float[] { (float)OptionLightPosition.X, (float)OptionLightPosition.Y, (float)OptionLightPosition.Z, 0.0f });
			// fog
			double fd = Game.NextFog.TrackPosition - Game.PreviousFog.TrackPosition;
			if (fd != 0.0)
			{
				float fr = (float)((World.CameraTrackFollower.TrackPosition - Game.PreviousFog.TrackPosition) / fd);
				float frc = 1.0f - fr;
				Game.CurrentFog.Start = Game.PreviousFog.Start * frc + Game.NextFog.Start * fr;
				Game.CurrentFog.End = Game.PreviousFog.End * frc + Game.NextFog.End * fr;
				Game.CurrentFog.Color.R = (byte)((float)Game.PreviousFog.Color.R * frc + (float)Game.NextFog.Color.R * fr);
				Game.CurrentFog.Color.G = (byte)((float)Game.PreviousFog.Color.G * frc + (float)Game.NextFog.Color.G * fr);
				Game.CurrentFog.Color.B = (byte)((float)Game.PreviousFog.Color.B * frc + (float)Game.NextFog.Color.B * fr);
			}
			else
			{
				Game.CurrentFog = Game.PreviousFog;
			}
			// render background
			if (LibRender.Renderer.FogEnabled)
			{
				GL.Disable(EnableCap.Fog); LibRender.Renderer.FogEnabled = false;
			}
			GL.Disable(EnableCap.DepthTest);
			UpdateBackground(TimeElapsed);
			RenderEvents(World.AbsoluteCameraPosition);
			// fog
			float aa = Game.CurrentFog.Start;
			float bb = Game.CurrentFog.End;
			if (aa < bb & aa < World.BackgroundImageDistance)
			{
				if (!LibRender.Renderer.FogEnabled)
				{
					GL.Fog(FogParameter.FogMode, (int)FogMode.Linear);
				}
				GL.Fog(FogParameter.FogStart, aa);
				GL.Fog(FogParameter.FogEnd, bb);
				GL.Fog(FogParameter.FogColor, new float[] { inv255 * (float)Game.CurrentFog.Color.R, inv255 * (float)Game.CurrentFog.Color.G, inv255 * (float)Game.CurrentFog.Color.B, 1.0f });
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
				if (World.CameraRestriction == CameraRestrictionMode.NotAvailable)
				{
					GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { inv255 * (float)OptionAmbientColor.R, inv255 * (float)OptionAmbientColor.G, inv255 * (float)OptionAmbientColor.B, 1.0f });
					GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { inv255 * (float)OptionDiffuseColor.R, inv255 * (float)OptionDiffuseColor.G, inv255 * (float)OptionDiffuseColor.B, 1.0f });
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
				for (int i = 0; i < StaticOpaque.Length; i++)
				{
					if (StaticOpaque[i] != null)
					{
						if (StaticOpaque[i].List != null)
						{
							for (int j = 0; j < StaticOpaque[i].List.FaceCount; j++)
							{
								if (StaticOpaque[i].List.Faces[j] != null)
								{
									RenderFace(ref StaticOpaque[i].List.Faces[j], World.AbsoluteCameraPosition);
								}
							}
						}
					}
				}
			}
			else
			{
				for (int i = 0; i < StaticOpaque.Length; i++)
				{
					if (StaticOpaque[i] != null)
					{
						if (StaticOpaque[i].Update | StaticOpaqueForceUpdate)
						{
							StaticOpaque[i].Update = false;
							if (StaticOpaque[i].OpenGlDisplayListAvailable)
							{
								GL.DeleteLists(StaticOpaque[i].OpenGlDisplayList, 1);
								StaticOpaque[i].OpenGlDisplayListAvailable = false;
							}
							if (StaticOpaque[i].List.FaceCount != 0)
							{
								StaticOpaque[i].OpenGlDisplayList = GL.GenLists(1);
								StaticOpaque[i].OpenGlDisplayListAvailable = true;
								LibRender.Renderer.ResetOpenGlState();
								GL.NewList(StaticOpaque[i].OpenGlDisplayList, ListMode.Compile);
								for (int j = 0; j < StaticOpaque[i].List.FaceCount; j++)
								{
									if (StaticOpaque[i].List.Faces[j] != null)
									{
										RenderFace(ref StaticOpaque[i].List.Faces[j], World.AbsoluteCameraPosition);
									}
								}
								GL.EndList();
							}
							StaticOpaque[i].WorldPosition = World.AbsoluteCameraPosition;
						}
					}
				}
				StaticOpaqueForceUpdate = false;
				for (int i = 0; i < StaticOpaque.Length; i++)
				{
					if (StaticOpaque[i] != null && StaticOpaque[i].OpenGlDisplayListAvailable)
					{
						LibRender.Renderer.ResetOpenGlState();
						GL.PushMatrix();
						GL.Translate(StaticOpaque[i].WorldPosition.X - World.AbsoluteCameraPosition.X, StaticOpaque[i].WorldPosition.Y - World.AbsoluteCameraPosition.Y, StaticOpaque[i].WorldPosition.Z - World.AbsoluteCameraPosition.Z);
						GL.CallList(StaticOpaque[i].OpenGlDisplayList);
						GL.PopMatrix();
					}
				}
				//Update bounding box positions now we've rendered the objects
				int currentBox = 0;
				for (int i = 0; i < StaticOpaque.Length; i++)
				{
					if (StaticOpaque[i] != null)
					{
						currentBox++;

					}
				}
			}
			// dynamic opaque
			LibRender.Renderer.ResetOpenGlState();
			for (int i = 0; i < DynamicOpaque.FaceCount; i++)
			{
				RenderFace(ref DynamicOpaque.Faces[i], World.AbsoluteCameraPosition);
			}
			// dynamic alpha
			LibRender.Renderer.ResetOpenGlState();
			DynamicAlpha.SortPolygons();
			if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
			{
				GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
				GL.DepthMask(false);
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
				for (int i = 0; i < DynamicAlpha.FaceCount; i++)
				{
					RenderFace(ref DynamicAlpha.Faces[i], World.AbsoluteCameraPosition);
				}
			}
			else
			{
				GL.Disable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = false;
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				GL.DepthMask(true);
				for (int i = 0; i < DynamicAlpha.FaceCount; i++)
				{
					int r = (int)ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Faces[DynamicAlpha.Faces[i].FaceIndex].Material;
					if (ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].GlowAttenuationData == 0)
					{
						if (ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].Color.A == 255)
						{
							RenderFace(ref DynamicAlpha.Faces[i], World.AbsoluteCameraPosition);
						}
					}
				}
				GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
				GL.DepthMask(false);
				bool additive = false;
				for (int i = 0; i < DynamicAlpha.FaceCount; i++)
				{
					int r = (int)ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Faces[DynamicAlpha.Faces[i].FaceIndex].Material;
					if (ObjectManager.Objects[DynamicAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
					{
						if (!additive)
						{
							LibRender.Renderer.UnsetAlphaFunc();
							additive = true;
						}
						RenderFace(ref DynamicAlpha.Faces[i], World.AbsoluteCameraPosition);
					}
					else
					{
						if (additive)
						{
							LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
							additive = false;
						}
						RenderFace(ref DynamicAlpha.Faces[i], World.AbsoluteCameraPosition);
					}
				}
			}
			// motion blur
			GL.Disable(EnableCap.DepthTest);
			GL.DepthMask(false);
			LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
			if (Interface.CurrentOptions.MotionBlur != Interface.MotionBlurMode.None)
			{
				if (LibRender.Renderer.LightingEnabled)
				{
					GL.Disable(EnableCap.Lighting);
					LibRender.Renderer.LightingEnabled = false;
				}
				RenderFullscreenMotionBlur();
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
			if (World.CameraRestriction == CameraRestrictionMode.NotAvailable)
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
				for (int i = 0; i < OverlayOpaque.FaceCount; i++)
				{
					RenderFace(ref OverlayOpaque.Faces[i], World.AbsoluteCameraPosition);
				}
				// overlay alpha
				OverlayAlpha.SortPolygons();
				if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
				{
					GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
					GL.DepthMask(false);
					LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
					for (int i = 0; i < OverlayAlpha.FaceCount; i++)
					{
						RenderFace(ref OverlayAlpha.Faces[i], World.AbsoluteCameraPosition);
					}
				}
				else
				{
					GL.Disable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = false;
					LibRender.Renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
					GL.DepthMask(true);
					for (int i = 0; i < OverlayAlpha.FaceCount; i++)
					{
						int r = (int)ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Faces[OverlayAlpha.Faces[i].FaceIndex].Material;
						if (ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].GlowAttenuationData == 0)
						{
							if (ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].Color.A == 255)
							{
								RenderFace(ref OverlayAlpha.Faces[i], World.AbsoluteCameraPosition);
							}
						}
					}
					GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
					LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
					GL.DepthMask(false);
					bool additive = false;
					for (int i = 0; i < OverlayAlpha.FaceCount; i++)
					{
						int r = (int)ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Faces[OverlayAlpha.Faces[i].FaceIndex].Material;
						if (ObjectManager.Objects[OverlayAlpha.Faces[i].ObjectIndex].Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
						{
							if (!additive)
							{
								LibRender.Renderer.UnsetAlphaFunc();
								additive = true;
							}
							RenderFace(ref OverlayAlpha.Faces[i], World.AbsoluteCameraPosition);
						}
						else
						{
							if (additive)
							{
								LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
								additive = false;
							}
							RenderFace(ref OverlayAlpha.Faces[i], World.AbsoluteCameraPosition);
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
				OverlayAlpha.SortPolygons();
				for (int i = 0; i < OverlayAlpha.FaceCount; i++)
				{
					RenderFace(ref OverlayAlpha.Faces[i], World.AbsoluteCameraPosition);
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
				if (!OptionBackfaceCulling || (ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) != 0)
				{
					GL.Disable(EnableCap.CullFace);
					LibRender.Renderer.CullEnabled = false;
				}
			}
			else if (OptionBackfaceCulling)
			{
				if ((ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) == 0)
				{
					GL.Enable(EnableCap.CullFace);
					LibRender.Renderer.CullEnabled = true;
				}
			}
			int r = (int)ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Material;
			LibRender.Renderer.RenderFace(ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Materials[r], ObjectManager.Objects[Face.ObjectIndex].Mesh.Vertices, Face.Wrap, ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex], Camera, IsDebugTouchMode);
		}
		
		
	}
}
