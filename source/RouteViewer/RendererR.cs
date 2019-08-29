// ╔═════════════════════════════════════════════════════════════╗
// ║ Renderer.cs for the Route Viewer                            ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using System.Drawing;
using OpenBve.RouteManager;
using LibRender;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Vector3 = OpenBveApi.Math.Vector3;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using static LibRender.CameraProperties;

namespace OpenBve {
	internal static partial class Renderer {

		// first frame behavior
		internal enum LoadTextureImmediatelyMode { NotYet, Yes, NoLonger }
		internal static LoadTextureImmediatelyMode LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;

		
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

		//Stats
		internal static bool RenderStatsOverlay = true;
		
		// current opengl data
		internal static bool TransparentColorDepthSorting = false;

		// textures
		private static Texture BackgroundChangeTexture = null;
		private static Texture BrightnessChangeTexture = null;
		private static Texture TransponderTexture = null;
		private static Texture SectionTexture = null;
		private static Texture LimitTexture = null;
		private static Texture StationStartTexture = null;
		private static Texture StationEndTexture = null;
		private static Texture StopTexture = null;
		private static Texture BufferTexture = null;
		private static Texture SoundTexture = null;
		private static Texture PointSoundTexture = null;

		// options
		internal static bool OptionEvents = false;
		internal static bool OptionInterface = true;

		// constants
		private const float inv255 = 1.0f / 255.0f;

		// reset
		internal static void Reset() {
			LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;
			LibRender.Renderer.Objects = new RendererObject[256];
			LibRender.Renderer.ObjectCount = 0;
			StaticOpaque = new ObjectGroup[] { };
			StaticOpaqueForceUpdate = true;
			DynamicOpaque = new ObjectList();
			DynamicAlpha = new ObjectList();
			OverlayOpaque = new ObjectList();
			OverlayAlpha = new ObjectList();
			LibRender.Renderer.OptionLighting = true;
			LibRender.Renderer.OptionAmbientColor = new Color24(160, 160, 160);
			LibRender.Renderer.OptionDiffuseColor = new Color24(160, 160, 160);
			LibRender.Renderer.OptionLightPosition = new Vector3(0.215920077052065f, 0.875724044222352f, -0.431840154104129f);
			LibRender.Renderer.OptionLightingResultingAmount = 1.0f;
			GL.Disable(EnableCap.Fog); LibRender.Renderer.FogEnabled = false;
		}

		// initialize
		internal static void Initialize() {
			LibRender.Renderer.Initialize();
			string Folder = OpenBveApi.Path.CombineDirectory(Program.FileSystem.GetDataFolder(), "RouteViewer");
			TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "background.png"), out BackgroundChangeTexture);
			TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "brightness.png"), out BrightnessChangeTexture);
			TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "transponder.png"), out TransponderTexture);
			TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "section.png"), out SectionTexture);
			TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "limit.png"), out LimitTexture);
			TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "station_start.png"), out StationStartTexture);
			TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "station_end.png"), out StationEndTexture);
			TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "stop.png"), out StopTexture);
			TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "buffer.png"), out BufferTexture);
			TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "sound.png"), out SoundTexture);
			TextureManager.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "switchsound.png"), out PointSoundTexture);
			TransparentColorDepthSorting = Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality& Interface.CurrentOptions.Interpolation != InterpolationMode.NearestNeighbor & Interface.CurrentOptions.Interpolation != InterpolationMode.Bilinear;
		}

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
					float cr = n * inv255 * (float) CurrentRoute.CurrentFog.Color.R;
					float cg = n * inv255 * (float) CurrentRoute.CurrentFog.Color.G;
					float cb = n * inv255 * (float) CurrentRoute.CurrentFog.Color.B;
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
			GL.Light(LightName.Light0, LightParameter.Position, new float[] {(float) LibRender.Renderer.OptionLightPosition.X, (float) LibRender.Renderer.OptionLightPosition.Y, (float) LibRender.Renderer.OptionLightPosition.Z, 0.0f});
			// fog
			double fd = CurrentRoute.NextFog.TrackPosition - CurrentRoute.PreviousFog.TrackPosition;
			if (fd != 0.0)
			{
				float fr = (float) ((World.CameraTrackFollower.TrackPosition - CurrentRoute.PreviousFog.TrackPosition) / fd);
				float frc = 1.0f - fr;
				CurrentRoute.CurrentFog.Start = CurrentRoute.PreviousFog.Start * frc + CurrentRoute.NextFog.Start * fr;
				CurrentRoute.CurrentFog.End = CurrentRoute.PreviousFog.End * frc + CurrentRoute.NextFog.End * fr;
				CurrentRoute.CurrentFog.Color.R = (byte) ((float) CurrentRoute.PreviousFog.Color.R * frc + (float) CurrentRoute.NextFog.Color.R * fr);
				CurrentRoute.CurrentFog.Color.G = (byte) ((float) CurrentRoute.PreviousFog.Color.G * frc + (float) CurrentRoute.NextFog.Color.G * fr);
				CurrentRoute.CurrentFog.Color.B = (byte) ((float) CurrentRoute.PreviousFog.Color.B * frc + (float) CurrentRoute.NextFog.Color.B * fr);
			}
			else
			{
				CurrentRoute.CurrentFog = CurrentRoute.PreviousFog;
			}
			// render background

			GL.Disable(EnableCap.DepthTest);
			CurrentRoute.UpdateBackground(TimeElapsed, false);
			bool optionLighting = LibRender.Renderer.OptionLighting;
			LibRender.Renderer.LastBoundTexture = null;
			// fog
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
									RenderFace(ref StaticOpaque[i].List.Faces[j], Camera.AbsolutePosition);
								}
							}

							GL.EndList();
						}

						StaticOpaque[i].WorldPosition = Camera.AbsolutePosition;
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
					GL.Translate(StaticOpaque[i].WorldPosition.X - Camera.AbsolutePosition.X, StaticOpaque[i].WorldPosition.Y - Camera.AbsolutePosition.Y, StaticOpaque[i].WorldPosition.Z - Camera.AbsolutePosition.Z);
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

			LibRender.Renderer.ResetOpenGlState();
			if (OptionEvents) RenderEvents(Camera.AbsolutePosition);
			LibRender.Renderer.ResetOpenGlState();
			// dynamic opaque
			LibRender.Renderer.ResetOpenGlState();
			for (int i = 0; i < DynamicOpaque.FaceCount; i++)
			{
				RenderFace(ref DynamicOpaque.Faces[i], Camera.AbsolutePosition);
			}

			// dynamic alpha
			LibRender.Renderer.ResetOpenGlState();
			DynamicAlpha.SortPolygons();
			if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Performance)
			{
				GL.Enable(EnableCap.Blend);
				LibRender.Renderer.BlendEnabled = true;
				GL.DepthMask(false);
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.0f);
				for (int i = 0; i < DynamicAlpha.FaceCount; i++)
				{
					RenderFace(ref DynamicAlpha.Faces[i], Camera.AbsolutePosition);
				}
			}
			else
			{
				GL.Disable(EnableCap.Blend);
				LibRender.Renderer.BlendEnabled = false;
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Equal, 1.0f);
				GL.DepthMask(true);
				for (int i = 0; i < DynamicAlpha.FaceCount; i++)
				{
					int r = (int) DynamicAlpha.Faces[i].ObjectReference.Mesh.Faces[DynamicAlpha.Faces[i].FaceIndex].Material;
					if (DynamicAlpha.Faces[i].ObjectReference.Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Normal & DynamicAlpha.Faces[i].ObjectReference.Mesh.Materials[r].GlowAttenuationData == 0)
					{
						if (DynamicAlpha.Faces[i].ObjectReference.Mesh.Materials[r].Color.A == 255)
						{
							RenderFace(ref DynamicAlpha.Faces[i], Camera.AbsolutePosition);
						}
					}
				}

				GL.Enable(EnableCap.Blend);
				LibRender.Renderer.BlendEnabled = true;
				LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
				GL.DepthMask(false);
				bool additive = false;
				for (int i = 0; i < DynamicAlpha.FaceCount; i++)
				{
					int r = (int) DynamicAlpha.Faces[i].ObjectReference.Mesh.Faces[DynamicAlpha.Faces[i].FaceIndex].Material;
					if (DynamicAlpha.Faces[i].ObjectReference.Mesh.Materials[r].BlendMode == MeshMaterialBlendMode.Additive)
					{
						if (!additive)
						{
							LibRender.Renderer.UnsetAlphaFunc();
							additive = true;
						}

						RenderFace(ref DynamicAlpha.Faces[i], Camera.AbsolutePosition);
					}
					else
					{
						if (additive)
						{
							LibRender.Renderer.SetAlphaFunc(AlphaFunction.Less, 1.0f);
							additive = false;
						}

						RenderFace(ref DynamicAlpha.Faces[i], Camera.AbsolutePosition);
					}
				}
			}


			lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);

			// render overlays

			LibRender.Renderer.OptionLighting = optionLighting;
			if (LibRender.Renderer.LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting);
				LibRender.Renderer.LightingEnabled = false;
			}

			if (LibRender.Renderer.FogEnabled)
			{
				GL.Disable(EnableCap.Fog);
				LibRender.Renderer.FogEnabled = false;
			}

			if (LibRender.Renderer.BlendEnabled)
			{
				GL.Disable(EnableCap.Blend);
				LibRender.Renderer.BlendEnabled = false;
			}

			LibRender.Renderer.UnsetAlphaFunc();
			GL.Disable(EnableCap.DepthTest);
			GL.PushMatrix();
			if(ObjectManager.ObjectsUsed == 0) GL.Clear(ClearBufferMask.ColorBufferBit);
			if(OptionInterface) RenderOverlays(TimeElapsed);
			GL.PopMatrix();
			// finalize rendering
			GL.PopMatrix();
		}


		// render face
		private static void RenderFace(ref ObjectFace Face, Vector3 Camera) {
			if (LibRender.Renderer.CullEnabled) {
				if (!LibRender.Renderer.OptionBackfaceCulling || (Face.ObjectReference.Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) != 0) {
					GL.Disable(EnableCap.CullFace);
					LibRender.Renderer.CullEnabled = false;
				}
			} else if (LibRender.Renderer.OptionBackfaceCulling) {
				if ((Face.ObjectReference.Mesh.Faces[Face.FaceIndex].Flags & MeshFace.Face2Mask) == 0) {
					GL.Enable(EnableCap.CullFace);
					LibRender.Renderer.CullEnabled = true;
				}
			}
			int r = (int)Face.ObjectReference.Mesh.Faces[Face.FaceIndex].Material;
			LibRender.Renderer.RenderFace(ref Face.ObjectReference.Mesh.Materials[r], Face.ObjectReference.Mesh.Vertices, Face.Wrap, ref Face.ObjectReference.Mesh.Faces[Face.FaceIndex], Camera);
		}
		
		// render events
		private static void RenderEvents(Vector3 Camera) {
			if (CurrentRoute.Tracks[0].Elements == null) {
				return;
			}
			LibRender.Renderer.LastBoundTexture = null;
			if (LibRender.Renderer.LightingEnabled) {
				GL.Disable(EnableCap.Lighting);
				LibRender.Renderer.LightingEnabled = false;
			}
			if (LibRender.Renderer.AlphaTestEnabled) {
				GL.Disable(EnableCap.AlphaTest);
				LibRender.Renderer.AlphaTestEnabled = false;
			}
			double da = -CameraProperties.Camera.BackwardViewingDistance - CameraProperties.Camera.ExtraViewingDistance;
			double db = CameraProperties.Camera.ForwardViewingDistance + CameraProperties.Camera.ExtraViewingDistance;
			bool[] sta = new bool[CurrentRoute.Stations.Length];
			// events
			for (int i = 0; i < CurrentRoute.Tracks[0].Elements.Length; i++) {
				double p = CurrentRoute.Tracks[0].Elements[i].StartingTrackPosition;
				double d = p - World.CameraTrackFollower.TrackPosition;
				if (d >= da & d <= db) {
					for (int j = 0; j < CurrentRoute.Tracks[0].Elements[i].Events.Length; j++) {
						dynamic e = CurrentRoute.Tracks[0].Elements[i].Events[j];
						double dy, dx = 0.0, dz = 0.0;
						double s; Texture t;
						if (e is BrightnessChangeEvent) {
							s = 0.15;
							dy = 4.0;
							t = BrightnessChangeTexture;
						} else if (e is BackgroundChangeEvent) {
							s = 0.25;
							dy = 3.5;
							t = BackgroundChangeTexture;
						} else if (e is StationStartEvent) {
							s = 0.25;
							dy = 1.6;
							t = StationStartTexture;
							StationStartEvent f = (StationStartEvent)e;
							sta[f.StationIndex] = true;
						} else if (e is TrackManager.StationEndEvent) {
							s = 0.25;
							dy = 1.6;
							t = StationEndTexture;
							TrackManager.StationEndEvent f = (TrackManager.StationEndEvent)e;
							sta[f.StationIndex] = true;
						} else if (e is LimitChangeEvent) {
							s = 0.2;
							dy = 1.1;
							t = LimitTexture;
						} else if (e is SectionChangeEvent) {
							s = 0.2;
							dy = 0.8;
							t = SectionTexture;
						} else if (e is TransponderEvent) {
							s = 0.15;
							dy = 0.4;
							t = TransponderTexture;
						} else if (e is TrackManager.SoundEvent) {
							TrackManager.SoundEvent f = (TrackManager.SoundEvent)e;
							s = 0.2;
							dx = f.Position.X;
							dy = f.Position.Y < 0.1 ? 0.1 : f.Position.Y;
							dz = f.Position.Z;
							t = f.SoundBuffer == null ? PointSoundTexture : SoundTexture;
						} else {
							s = 0.2;
							dy = 1.0;
							t = null;
						}
						if (t != null) {
							TrackFollower f = new TrackFollower(CurrentRoute.Tracks);
							f.TriggerType = EventTriggerType.None;
							f.TrackPosition = p;
							f.UpdateAbsolute(p + e.TrackPositionDelta, true, false);
							f.WorldPosition += dx * f.WorldSide + dy * f.WorldUp + dz * f.WorldDirection;
							LibRender.Renderer.DrawCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera, t);
						}
					}
				}
			}
			// stops
			for (int i = 0; i < sta.Length; i++) {
				if (sta[i]) {
					for (int j = 0; j < CurrentRoute.Stations[i].Stops.Length; j++) {
						const double dy = 1.4;
						const double s = 0.2;
						double p = CurrentRoute.Stations[i].Stops[j].TrackPosition;
						TrackFollower f = new TrackFollower(CurrentRoute.Tracks);
						f.TriggerType = EventTriggerType.None;
						f.TrackPosition = p;
						f.UpdateAbsolute(p, true, false);
						f.WorldPosition += dy * f.WorldUp;
						LibRender.Renderer.DrawCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera, StopTexture);
					}
				}
			}
			// buffers
			for (int i = 0; i < Game.BufferTrackPositions.Length; i++) {
				double p = Game.BufferTrackPositions[i];
				double d = p - World.CameraTrackFollower.TrackPosition;
				if (d >= da & d <= db) {
					const double dy = 2.5;
					const double s = 0.25;
					TrackFollower f = new TrackFollower(CurrentRoute.Tracks);
					f.TriggerType = EventTriggerType.None;
					f.TrackPosition = p;
					f.UpdateAbsolute(p, true, false);
					f.WorldPosition += dy * f.WorldUp;
					LibRender.Renderer.DrawCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera, BufferTexture);
				}
			}

			LibRender.Renderer.TexturingEnabled = true; //Set by the LibRender function
		}
		
		// render overlays
		private static void RenderOverlays(double TimeElapsed) {
			// initialize
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			GL.Enable(EnableCap.Blend); LibRender.Renderer.BlendEnabled = true;
			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.Ortho(0.0, (double)LibRender.Screen.Width, (double)LibRender.Screen.Height, 0.0, -1.0, 1.0);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.PushMatrix();
			GL.LoadIdentity();
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// marker
			if (OptionInterface)
			{
				int y = 150;
				for (int i = 0; i < LibRender.Renderer.MarkerTextures.Length; i++)
				{
					if (Program.CurrentHost.LoadTexture(LibRender.Renderer.MarkerTextures[i], OpenGlTextureWrapMode.ClampClamp)) {
						int w = LibRender.Renderer.MarkerTextures[i].Width;
						int h = LibRender.Renderer.MarkerTextures[i].Height;
						GL.Color4(1.0, 1.0, 1.0, 1.0);
						LibRender.Renderer.DrawRectangle(LibRender.Renderer.MarkerTextures[i], new Point(Screen.Width - w - 8, y), new Size(w,h));
						y += h + 8;
					}
				}
			}
			// render
			if (!Program.CurrentlyLoading) {
				if (ObjectManager.ObjectsUsed == 0) {
					string[][] Keys = { new string[] { "F7" }, new string[] { "F8" } };
					LibRender.Renderer.RenderKeys(4, 4, 24, Fonts.SmallFont, Keys);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Open route", new Point(32,4), TextAlignment.TopLeft, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the options window", new Point(32, 24), TextAlignment.TopLeft, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "v" + System.Windows.Forms.Application.ProductVersion, new Point(Screen.Width - 8, Screen.Height - 8), TextAlignment.BottomRight, Color128.White);
				} else if (OptionInterface) {
					// keys
					string[][] Keys = { new string[] { "F5" }, new string[] { "F7" }, new string[] { "F8" } };
					LibRender.Renderer.RenderKeys(4, 4, 24, Fonts.SmallFont, Keys);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Reload route", new Point(32, 4), TextAlignment.TopLeft, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Open route", new Point(32, 24), TextAlignment.TopLeft, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the options window", new Point(32, 44), TextAlignment.TopLeft, Color128.White, true);
					Keys = new string[][] { new string[] { "F" }, new string[] { "N" }, new string[] { "E" }, new string[] { "C" }, new string[] { "M" }, new string[] { "I" }};
					LibRender.Renderer.RenderKeys(Screen.Width - 20, 4, 16, Fonts.SmallFont, Keys);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Wireframe:", new Point(Screen.Width -32, 4), TextAlignment.TopRight, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Normals:", new Point(Screen.Width - 32, 24), TextAlignment.TopRight, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Events:", new Point(Screen.Width - 32, 44), TextAlignment.TopRight, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "CPU:", new Point(Screen.Width - 32, 64), TextAlignment.TopRight, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Mute:", new Point(Screen.Width - 32, 84), TextAlignment.TopRight, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Hide interface:", new Point(Screen.Width - 32, 104), TextAlignment.TopRight, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, (RenderStatsOverlay ? "Hide" : "Show") + " renderer statistics", new Point(Screen.Width - 32, 124), TextAlignment.TopRight, Color128.White, true);
					Keys = new string[][] { new string[] { "F10" } };
					LibRender.Renderer.RenderKeys(Screen.Width - 32, 124, 30, Fonts.SmallFont, Keys);
					Keys = new string[][] { new string[] { null, "W", null }, new string[] { "A", "S", "D" } };
					LibRender.Renderer.RenderKeys(4, Screen.Height - 40, 16, Fonts.SmallFont, Keys);
					Keys = new string[][] { new string[] { null, "↑", null }, new string[] { "←", "↓", "→" } };
					LibRender.Renderer.RenderKeys(0 * Screen.Width - 48, Screen.Height - 40, 16, Fonts.SmallFont, Keys);
					Keys = new string[][] { new string[] { "P↑" }, new string[] { "P↓" } };
					LibRender.Renderer.RenderKeys((int)(0.5 * Screen.Width + 32), Screen.Height - 40, 24, Fonts.SmallFont, Keys);
					Keys = new string[][] { new string[] { null, "/", "*" }, new string[] { "7", "8", "9" }, new string[] { "4", "5", "6" }, new string[] { "1", "2", "3" }, new string[] { null, "0", "." } };
					LibRender.Renderer.RenderKeys(Screen.Width - 60, Screen.Height - 100, 16, Fonts.SmallFont, Keys);
					if (Program.JumpToPositionEnabled) {
						LibRender.Renderer.DrawString(Fonts.SmallFont, "Jump to track position:", new Point(4, 80),TextAlignment.TopLeft, Color128.White, true);
						double distance;
						if (Double.TryParse(Program.JumpToPositionValue, out distance))
						{
							if (distance < Program.MinimumJumpToPositionValue - 100)
							{
								LibRender.Renderer.DrawString(Fonts.SmallFont, (Environment.TickCount % 1000 <= 500 ? Program.JumpToPositionValue + "_" : Program.JumpToPositionValue), new Point(4, 100), TextAlignment.TopLeft, Color128.Red, true);
							}
							else
							{
								LibRender.Renderer.DrawString(Fonts.SmallFont, (Environment.TickCount % 1000 <= 500 ? Program.JumpToPositionValue + "_" : Program.JumpToPositionValue), new Point(4, 100), TextAlignment.TopLeft, distance > CurrentRoute.Tracks[0].Elements[CurrentRoute.Tracks[0].Elements.Length - 1].StartingTrackPosition + 100
								? Color128.Red : Color128.Yellow, true);
							}
							
						}
					}
					// info
					double x = 0.5 * (double) Screen.Width - 256.0;
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Position: " + GetLengthString(Camera.Alignment.TrackPosition) + " (X=" + GetLengthString(Camera.Alignment.Position.X) + ", Y=" + GetLengthString(Camera.Alignment.Position.Y) + "), Orientation: (Yaw=" + (Camera.Alignment.Yaw * 57.2957795130824).ToString("0.00", Culture) + "°, Pitch=" + (Camera.Alignment.Pitch * 57.2957795130824).ToString("0.00", Culture) + "°, Roll=" + (Camera.Alignment.Roll * 57.2957795130824).ToString("0.00", Culture) + "°)", new Point((int)x, 4), TextAlignment.TopLeft, Color128.White, true);
					LibRender.Renderer.DrawString(Fonts.SmallFont, "Radius: " + GetLengthString(World.CameraTrackFollower.CurveRadius) + ", Cant: " + (1000.0 * World.CameraTrackFollower.CurveCant).ToString("0", Culture) + " mm, Adhesion=" + (100.0 * World.CameraTrackFollower.AdhesionMultiplier).ToString("0", Culture), new Point((int)x, 20), TextAlignment.TopLeft, Color128.White, true);
					if (Program.CurrentStation >= 0) {
						System.Text.StringBuilder t = new System.Text.StringBuilder();
						t.Append(CurrentRoute.Stations[Program.CurrentStation].Name);
						if (CurrentRoute.Stations[Program.CurrentStation].ArrivalTime >= 0.0) {
							t.Append(", Arrival: " + GetTime(CurrentRoute.Stations[Program.CurrentStation].ArrivalTime));
						}
						if (CurrentRoute.Stations[Program.CurrentStation].DepartureTime >= 0.0) {
							t.Append(", Departure: " + GetTime(CurrentRoute.Stations[Program.CurrentStation].DepartureTime));
						}
						if (CurrentRoute.Stations[Program.CurrentStation].OpenLeftDoors & CurrentRoute.Stations[Program.CurrentStation].OpenRightDoors) {
							t.Append(", [L][R]");
						} else if (CurrentRoute.Stations[Program.CurrentStation].OpenLeftDoors) {
							t.Append(", [L][-]");
						} else if (CurrentRoute.Stations[Program.CurrentStation].OpenRightDoors) {
							t.Append(", [-][R]");
						} else {
							t.Append(", [-][-]");
						}
						switch (CurrentRoute.Stations[Program.CurrentStation].StopMode) {
							case StationStopMode.AllStop:
								t.Append(", Stop");
								break;
							case StationStopMode.AllPass:
								t.Append(", Pass");
								break;
							case StationStopMode.PlayerStop:
								t.Append(", Player stops - others pass");
								break;
							case StationStopMode.PlayerPass:
								t.Append(", Player passes - others stop");
								break;
						}
						if (CurrentRoute.Stations[Program.CurrentStation].Type == StationType.ChangeEnds) {
							t.Append(", Change ends");
						}
						t.Append(", Ratio=").Append((100.0 * CurrentRoute.Stations[Program.CurrentStation].PassengerRatio).ToString("0", Culture)).Append("%");
						LibRender.Renderer.DrawString(Fonts.SmallFont, t.ToString(), new Point((int)x, 36), TextAlignment.TopLeft, Color128.White, true);
					}
					if (Interface.MessageCount == 1) {
						Keys = new string[][] { new string[] { "F9" } };
						LibRender.Renderer.RenderKeys(4, 72, 24, Fonts.SmallFont, Keys);
						if (Interface.LogMessages[0].Type != MessageType.Information)
						{
							LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the 1 error message recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.Red, true);
						}
						else
						{
							//If all of our messages are information, then print the message text in grey
							LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the 1 message recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.White, true);
						}
					} else if (Interface.MessageCount > 1) {
						LibRender.Renderer.RenderKeys(4, 72, 24, Fonts.SmallFont, new string[][] { new string[] { "F9" } });
						bool error = false;
						for (int i = 0; i < Interface.MessageCount; i++)
						{
							if (Interface.LogMessages[i].Type != MessageType.Information)
							{
								error = true;
								break;
							}

						}
						if (error)
						{
							LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the " + Interface.MessageCount + " error messages recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.Red, true);
						}
						else
						{
							LibRender.Renderer.DrawString(Fonts.SmallFont, "Display the " + Interface.MessageCount + " messages recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.White, true);
						}
					}
					if (RenderStatsOverlay)
					{
						LibRender.Renderer.RenderKeys(4, Screen.Height - 126, 116, Fonts.SmallFont, new string[][] { new string[] { "Renderer Statistics" } });
						LibRender.Renderer.DrawString(Fonts.SmallFont, "Total static objects: " + ObjectManager.ObjectsUsed, new Point(4, Screen.Height - 112), TextAlignment.TopLeft, Color128.White, true);
						LibRender.Renderer.DrawString(Fonts.SmallFont, "Total animated objects: " + ObjectManager.AnimatedWorldObjectsUsed, new Point(4, Screen.Height - 100), TextAlignment.TopLeft, Color128.White, true);
						LibRender.Renderer.DrawString(Fonts.SmallFont, "Current framerate: " + LibRender.Renderer.FrameRate.ToString("0.0", Culture) + "fps", new Point(4, Screen.Height - 88), TextAlignment.TopLeft, Color128.White, true);
						LibRender.Renderer.DrawString(Fonts.SmallFont, "Total opaque faces: " + LibRender.Renderer.InfoStaticOpaqueFaceCount, new Point(4, Screen.Height - 76), TextAlignment.TopLeft, Color128.White, true);
						LibRender.Renderer.DrawString(Fonts.SmallFont, "Total alpha faces: " + DynamicAlpha.FaceCount, new Point(4, Screen.Height - 64), TextAlignment.TopLeft, Color128.White, true);
					}
				}

			}
			GL.PopMatrix();
			GL.LoadIdentity();
			// finalize
			GL.PopMatrix();
			GL.MatrixMode(MatrixMode.Projection);
			GL.PopMatrix();
			GL.MatrixMode(MatrixMode.Modelview);
			GL.Disable(EnableCap.Blend);
		}
		private static string GetTime(double Time) {
			int h = (int)Math.Floor(Time / 3600.0);
			Time -= (double)h * 3600.0;
			int m = (int)Math.Floor(Time / 60.0);
			Time -= (double)m * 60.0;
			int s = (int)Math.Floor(Time);
			return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
		}

		// get length string 
		private static string GetLengthString(double Value)
		{
			System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
			if (Game.RouteUnitOfLength.Length == 1 && Game.RouteUnitOfLength[0] == 1.0)
			{
				return Value.ToString("0.00", culture);
			}
			else
			{
				double[] values = new double[Game.RouteUnitOfLength.Length];
				for (int i = 0; i < Game.RouteUnitOfLength.Length - 1; i++)
				{
					values[i] = Math.Floor(Value/Game.RouteUnitOfLength[i]);
					Value -= values[i]*Game.RouteUnitOfLength[i];
				}
				values[Game.RouteUnitOfLength.Length - 1] = Value/Game.RouteUnitOfLength[Game.RouteUnitOfLength.Length - 1];
				System.Text.StringBuilder builder = new System.Text.StringBuilder();
				for (int i = 0; i < values.Length - 1; i++)
				{
					builder.Append(values[i].ToString(culture) + ":");
				}
				builder.Append(values[values.Length - 1].ToString("0.00", culture));
				return builder.ToString();
			}
		}
		
		private static void ReAddObjects()
        {
	        RendererObject[] list = new RendererObject[LibRender.Renderer.ObjectCount];
	        for (int i = 0; i < LibRender.Renderer.ObjectCount; i++)
	        {
		        list[i] = LibRender.Renderer.Objects[i];
	        }
	        for (int i = 0; i < list.Length; i++)
	        {
		        HideObject(ref list[i].InternalObject);
	        }
	        for (int i = 0; i < list.Length; i++)
	        {
		        ShowObject(list[i].InternalObject, list[i].Type);
	        }
        }

		/// <summary>Makes an object visible within the world</summary>
		/// <param name="objectToShow">The object to show</param>
		/// <param name="Type">Whether this is a static or dynamic object</param>
		internal static void ShowObject(StaticObject objectToShow, ObjectType Type)
		{
			if (objectToShow == null)
			{
				return;
			}
			if (objectToShow.RendererIndex == 0)
			{
				if (LibRender.Renderer.ObjectCount >= LibRender.Renderer.Objects.Length)
				{
					Array.Resize<RendererObject>(ref LibRender.Renderer.Objects, LibRender.Renderer.Objects.Length << 1);
				}

				LibRender.Renderer.Objects[LibRender.Renderer.ObjectCount] = new RendererObject(objectToShow, Type);
				int f = objectToShow.Mesh.Faces.Length;
				LibRender.Renderer.Objects[LibRender.Renderer.ObjectCount].FaceListReferences = new ObjectListReference[f];
				for (int i = 0; i < f; i++)
				{
					bool alpha = false;
					int k = objectToShow.Mesh.Faces[i].Material;
					OpenGlTextureWrapMode wrap = OpenGlTextureWrapMode.ClampClamp;
					if (objectToShow.Mesh.Materials[k].DaytimeTexture != null | objectToShow.Mesh.Materials[k].NighttimeTexture != null)
					{
						if (objectToShow.Mesh.Materials[k].WrapMode == null)
						{
							// If the object does not have a stored wrapping mode, determine it now
							for (int v = 0; v < objectToShow.Mesh.Vertices.Length; v++)
							{
								if (objectToShow.Mesh.Vertices[v].TextureCoordinates.X < 0.0f |
								    objectToShow.Mesh.Vertices[v].TextureCoordinates.X > 1.0f)
								{
									wrap |= OpenGlTextureWrapMode.RepeatClamp;
								}
								if (objectToShow.Mesh.Vertices[v].TextureCoordinates.Y < 0.0f |
								    objectToShow.Mesh.Vertices[v].TextureCoordinates.Y > 1.0f)
								{
									wrap |= OpenGlTextureWrapMode.ClampRepeat;
								}
							}							
						}
						else
						{
							//Yuck cast, but we need the null, as otherwise requires rewriting the texture indexer
							wrap = (OpenGlTextureWrapMode)objectToShow.Mesh.Materials[k].WrapMode;
						}
						if (objectToShow.Mesh.Materials[k].DaytimeTexture != null)
						{
							if (Program.CurrentHost.LoadTexture(objectToShow.Mesh.Materials[k].DaytimeTexture, wrap))
							{
								TextureTransparencyType type =
									objectToShow.Mesh.Materials[k].DaytimeTexture.Transparency;
								if (type == TextureTransparencyType.Alpha)
								{
									alpha = true;
								}
								else if (type == TextureTransparencyType.Partial &&
								         Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality)
								{
									alpha = true;
								}
							}
						}
						if (objectToShow.Mesh.Materials[k].NighttimeTexture != null)
						{
							if (Program.CurrentHost.LoadTexture(objectToShow.Mesh.Materials[k].NighttimeTexture, wrap))
							{
								TextureTransparencyType type =
									objectToShow.Mesh.Materials[k].NighttimeTexture.Transparency;
								if (type == TextureTransparencyType.Alpha)
								{
									alpha = true;
								}
								else if (type == TextureTransparencyType.Partial &
								         Interface.CurrentOptions.TransparencyMode == TransparencyMode.Quality)
								{
									alpha = true;
								}
							}
						}
					}
					if (Type == ObjectType.Overlay & CameraProperties.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable)
					{
						alpha = true;
					}
					else if (objectToShow.Mesh.Materials[k].Color.A != 255)
					{
						alpha = true;
					}
					else if (objectToShow.Mesh.Materials[k].BlendMode == MeshMaterialBlendMode.Additive)
					{
						alpha = true;
					}
					else if (objectToShow.Mesh.Materials[k].GlowAttenuationData != 0)
					{
						alpha = true;
					}
					ObjectListType listType;
					switch (Type)
					{
						case ObjectType.Static:
							listType = alpha ? ObjectListType.DynamicAlpha : ObjectListType.StaticOpaque;
							break;
						case ObjectType.Dynamic:
							listType = alpha ? ObjectListType.DynamicAlpha : ObjectListType.DynamicOpaque;
							break;
						case ObjectType.Overlay:
							listType = alpha ? ObjectListType.OverlayAlpha : ObjectListType.OverlayOpaque;
							break;
						default:
							throw new InvalidOperationException();
					}
					if (listType == ObjectListType.StaticOpaque)
					{
						/*
						 * For the static opaque list, insert the face into
						 * the first vacant position in the matching group's list.
						 * */
						int groupIndex = (int)objectToShow.GroupIndex;
						if (groupIndex >= StaticOpaque.Length)
						{
							if (StaticOpaque.Length == 0)
							{
								StaticOpaque = new ObjectGroup[16];
							}
							while (groupIndex >= StaticOpaque.Length)
							{
								Array.Resize<ObjectGroup>(ref StaticOpaque, StaticOpaque.Length << 1);
							}
						}
						if (StaticOpaque[groupIndex] == null)
						{
							StaticOpaque[groupIndex] = new ObjectGroup();
						}
						ObjectList list = StaticOpaque[groupIndex].List;
						int newIndex = list.FaceCount;
						for (int j = 0; j < list.FaceCount; j++)
						{
							if (list.Faces[j] == null)
							{
								newIndex = j;
								break;
							}
						}
						if (newIndex == list.FaceCount)
						{
							if (list.FaceCount == list.Faces.Length)
							{
								Array.Resize<ObjectFace>(ref list.Faces, list.Faces.Length << 1);
							}
							list.FaceCount++;
						}
						list.Faces[newIndex] = new ObjectFace
						{
							ObjectListIndex = LibRender.Renderer.ObjectCount,
							ObjectReference = objectToShow,
							FaceIndex = i,
							Wrap = wrap
						};

						// HACK: Let's store the wrapping mode.

						StaticOpaque[groupIndex].Update = true;
						LibRender.Renderer.Objects[LibRender.Renderer.ObjectCount].FaceListReferences[i] = new ObjectListReference(listType, newIndex);
						LibRender.Renderer.InfoStaticOpaqueFaceCount++;

						/*
						 * Check if the given object has a bounding box, and insert it to the end of the list of bounding boxes if required
						 */
						if (objectToShow.Mesh.BoundingBox != null)
						{
							int Index = list.BoundingBoxes.Length;
							for (int j = 0; j < list.BoundingBoxes.Length; j++)
							{
								if (list.Faces[j] == null)
								{
									Index = j;
									break;
								}
							}
							if (Index == list.BoundingBoxes.Length)
							{
								Array.Resize<BoundingBox>(ref list.BoundingBoxes, list.BoundingBoxes.Length << 1);
							}
							list.BoundingBoxes[Index].Upper = objectToShow.Mesh.BoundingBox[0];
							list.BoundingBoxes[Index].Lower = objectToShow.Mesh.BoundingBox[1];
						}
					}
					else
					{
						/*
						 * For all other lists, insert the face at the end of the list.
						 * */
						ObjectList list;
						switch (listType)
						{
							case ObjectListType.DynamicOpaque:
								list = DynamicOpaque;
								break;
							case ObjectListType.DynamicAlpha:
								list = DynamicAlpha;
								break;
							case ObjectListType.OverlayOpaque:
								list = OverlayOpaque;
								break;
							case ObjectListType.OverlayAlpha:
								list = OverlayAlpha;
								break;
							default:
								throw new InvalidOperationException();
						}
						if (list.FaceCount == list.Faces.Length)
						{
							Array.Resize<ObjectFace>(ref list.Faces, list.Faces.Length << 1);
						}
						list.Faces[list.FaceCount] = new ObjectFace
						{
							ObjectListIndex = LibRender.Renderer.ObjectCount,
							ObjectReference = objectToShow,
							FaceIndex = i,
							Wrap = wrap
						};

						// HACK: Let's store the wrapping mode.

						LibRender.Renderer.Objects[LibRender.Renderer.ObjectCount].FaceListReferences[i] = new ObjectListReference(listType, list.FaceCount);
						list.FaceCount++;
					}

				}
				objectToShow.RendererIndex = LibRender.Renderer.ObjectCount + 1;
				LibRender.Renderer.ObjectCount++;
			}
		}

		/// <summary>Hides an object within the world</summary>
		/// <param name="Object">The object to hide</param>
		internal static void HideObject(ref StaticObject Object)
		{
			if (Object == null)
			{
				return;
			}

			int k = Object.RendererIndex - 1;
			if (k >= 0)
			{
				// remove faces
				for (int i = 0; i < LibRender.Renderer.Objects[k].FaceListReferences.Length; i++)
				{
					ObjectListType listType = LibRender.Renderer.Objects[k].FaceListReferences[i].Type;
					if (listType == ObjectListType.StaticOpaque)
					{
						/*
						 * For static opaque faces, set the face to be removed
						 * to a null reference. If there are null entries at
						 * the end of the list, update the number of faces used
						 * accordingly.
						 * */
						int groupIndex = (int) LibRender.Renderer.Objects[k].InternalObject.GroupIndex;
						ObjectList list = StaticOpaque[groupIndex].List;
						int listIndex = LibRender.Renderer.Objects[k].FaceListReferences[i].Index;
						list.Faces[listIndex] = null;
						if (listIndex == list.FaceCount - 1)
						{
							int count = 0;
							for (int j = list.FaceCount - 2; j >= 0; j--)
							{
								if (list.Faces[j] != null)
								{
									count = j + 1;
									break;
								}
							}

							list.FaceCount = count;
						}

						StaticOpaque[groupIndex].Update = true;
						LibRender.Renderer.InfoStaticOpaqueFaceCount--;
					}
					else
					{
						/*
						 * For all other kinds of faces, move the last face into place
						 * of the face to be removed and decrement the face counter.
						 * */
						ObjectList list;
						switch (listType)
						{
							case ObjectListType.DynamicOpaque:
								list = DynamicOpaque;
								break;
							case ObjectListType.DynamicAlpha:
								list = DynamicAlpha;
								break;
							case ObjectListType.OverlayOpaque:
								list = OverlayOpaque;
								break;
							case ObjectListType.OverlayAlpha:
								list = OverlayAlpha;
								break;
							default:
								throw new InvalidOperationException();
						}

						int listIndex = LibRender.Renderer.Objects[k].FaceListReferences[i].Index;
						if (list.FaceCount > 0)
						{
							list.Faces[listIndex] = list.Faces[list.FaceCount - 1];
						}

						LibRender.Renderer.Objects[list.Faces[listIndex].ObjectListIndex].FaceListReferences[list.Faces[listIndex].FaceIndex].Index = listIndex;
						if (list.FaceCount > 0)
						{
							list.FaceCount--;
						}
					}
				}

				// remove object
				if (k == LibRender.Renderer.ObjectCount - 1)
				{
					LibRender.Renderer.ObjectCount--;
				}
				else if (LibRender.Renderer.ObjectCount == 0)
				{
					return; //Outside the world?
				}
				else
				{
					LibRender.Renderer.Objects[k] = LibRender.Renderer.Objects[LibRender.Renderer.ObjectCount - 1];
					LibRender.Renderer.ObjectCount--;
					for (int i = 0; i < LibRender.Renderer.Objects[k].FaceListReferences.Length; i++)
					{
						ObjectListType listType = LibRender.Renderer.Objects[k].FaceListReferences[i].Type;
						ObjectList list;
						switch (listType)
						{
							case ObjectListType.StaticOpaque:
							{
								int groupIndex = (int) LibRender.Renderer.Objects[k].InternalObject.GroupIndex;
								list = StaticOpaque[groupIndex].List;
							}
								break;
							case ObjectListType.DynamicOpaque:
								list = DynamicOpaque;
								break;
							case ObjectListType.DynamicAlpha:
								list = DynamicAlpha;
								break;
							case ObjectListType.OverlayOpaque:
								list = OverlayOpaque;
								break;
							case ObjectListType.OverlayAlpha:
								list = OverlayAlpha;
								break;
							default:
								throw new InvalidOperationException();
						}

						int listIndex = LibRender.Renderer.Objects[k].FaceListReferences[i].Index;
						if (list.Faces[listIndex] == null)
						{
							continue;
						}

						list.Faces[listIndex].ObjectListIndex = k;
					}

					LibRender.Renderer.Objects[k].InternalObject.RendererIndex = k + 1;
				}

				Object.RendererIndex = 0;
			}
		}
	}
}
