// ╔═════════════════════════════════════════════════════════════╗
// ║ Renderer.cs for the Route Viewer                            ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using System.Drawing;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Vector3 = OpenBveApi.Math.Vector3;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenBveShared;
using TrackManager;

namespace OpenBve {
	internal static partial class Renderer {
		//Stats
		internal static bool RenderStatsOverlay = true;

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

		// options
		internal static bool OptionEvents = false;
		internal static bool OptionInterface = true;

		// constants
		private const float inv255 = 1.0f / 255.0f;

		// reset
		internal static void Reset() {
			OpenBveShared.Renderer.Objects = new RendererObject[256];
			OpenBveShared.Renderer.ObjectCount = 0;
			OpenBveShared.Renderer.StaticOpaque = new ObjectGroup[] { };
			OpenBveShared.Renderer.StaticOpaqueForceUpdate = true;
			OpenBveShared.Renderer.DynamicOpaque = new ObjectList();
			OpenBveShared.Renderer.DynamicAlpha = new ObjectList();
			OpenBveShared.Renderer.OverlayOpaque = new ObjectList();
			OpenBveShared.Renderer.OverlayAlpha = new ObjectList();
			OpenBveShared.Renderer.OptionLighting = true;
			OpenBveShared.Renderer.OptionAmbientColor = new Color24(160, 160, 160);
			OpenBveShared.Renderer.OptionDiffuseColor = new Color24(160, 160, 160);
			OpenBveShared.Renderer.OptionLightPosition = new Vector3(0.223606797749979f, 0.86602540378444f, -0.447213595499958f);
			OpenBveShared.Renderer.OptionLightingResultingAmount = 1.0f;
			GL.Disable(EnableCap.Fog); OpenBveShared.Renderer.FogEnabled = false;
		}

		// initialize
		internal static void LoadEventTextures() {
			string Folder = OpenBveApi.Path.CombineDirectory(Program.FileSystem.GetDataFolder(), "RouteViewer");
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "background.png"), out BackgroundChangeTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "brightness.png"), out BrightnessChangeTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "transponder.png"), out TransponderTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "section.png"), out SectionTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "limit.png"), out LimitTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "station_start.png"), out StationStartTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "station_end.png"), out StationEndTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "stop.png"), out StopTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "buffer.png"), out BufferTexture);
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "sound.png"), out SoundTexture);
			Textures.LoadTexture(BackgroundChangeTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(BrightnessChangeTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(TransponderTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(SectionTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(LimitTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(StationStartTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(StationEndTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(StopTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(BufferTexture, OpenGlTextureWrapMode.ClampClamp);
			Textures.LoadTexture(SoundTexture, OpenGlTextureWrapMode.ClampClamp);
		}

		internal static void RenderScene(double TimeElapsed) {
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
					if (string.IsNullOrEmpty(Program.CurrentRoute))
					{
						GL.ClearColor(0.67f, 0.67f, 0.67f, 1.0f);
					}
					else
					{
						GL.ClearColor(cr, cg, cb, 1.0f);
					}
					
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
				if (OpenBveShared.Camera.CameraRestriction == CameraRestrictionMode.NotAvailable)
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
			bool f = false;
			if (f) //TODO: Implement display list disabling
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
			OpenBveShared.Renderer.DynamicAlpha.SortPolygons();
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
			GL.LoadIdentity();
			lookat = Matrix4d.LookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);
			// render overlays
			OpenBveShared.Renderer.BlendEnabled = false; GL.Disable(EnableCap.Blend);
			OpenBveShared.Renderer.SetAlphaFunc(AlphaFunction.Greater, 0.9f);
			OpenBveShared.Renderer.AlphaTestEnabled = false; GL.Disable(EnableCap.AlphaTest);
			GL.Disable(EnableCap.DepthTest);
			if (OpenBveShared.Renderer.LightingEnabled) {
				GL.Disable(EnableCap.Lighting);
				OpenBveShared.Renderer.LightingEnabled = false;
			}
			RenderOverlays(TimeElapsed);
			// finalize rendering
			GL.PopMatrix();
		}
		
		// render events
		private static void RenderEvents(Vector3 CameraPosition) {
			if (OptionEvents == false)
			{
				return;
			}
			if (TrackManager.CurrentTrack.Elements == null) {
				return;
			}
			GL.Enable(EnableCap.CullFace); OpenBveShared.Renderer.CullEnabled = true;
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask(true);
			if (OpenBveShared.Renderer.LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting);
				OpenBveShared.Renderer.LightingEnabled = false;
			}
			if (OpenBveShared.Renderer.AlphaTestEnabled)
			{
				GL.Disable(EnableCap.AlphaTest);
				OpenBveShared.Renderer.AlphaTestEnabled = false;
			}
			double da = -OpenBveShared.World.BackwardViewingDistance - OpenBveShared.World.ExtraViewingDistance;
			double db = OpenBveShared.World.ForwardViewingDistance + OpenBveShared.World.ExtraViewingDistance;
			bool[] sta = new bool[Game.Stations.Length];
			// events
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++) {
				double p = TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
				double d = p - World.CameraTrackFollower.TrackPosition;
				if (d >= da & d <= db) {
					for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++) {
						GeneralEvent e = TrackManager.CurrentTrack.Elements[i].Events[j];
						double dy, dx = 0.0, dz = 0.0;
						double s; Texture t;
						if (e is TrackManager.BrightnessChangeEvent) {
							s = 0.15;
							dy = 4.0;
							t = BrightnessChangeTexture;
						} else if (e is BackgroundChangeEvent) {
							s = 0.25;
							dy = 3.5;
							t = BackgroundChangeTexture;
						} else if (e is TrackManager.StationStartEvent) {
							s = 0.25;
							dy = 1.6;
							t = StationStartTexture;
							TrackManager.StationStartEvent f = (TrackManager.StationStartEvent)e;
							sta[f.StationIndex] = true;
						} else if (e is TrackManager.StationEndEvent) {
							s = 0.25;
							dy = 1.6;
							t = StationEndTexture;
							TrackManager.StationEndEvent f = (TrackManager.StationEndEvent)e;
							sta[f.StationIndex] = true;
						} else if (e is TrackManager.LimitChangeEvent) {
							s = 0.2;
							dy = 1.1;
							t = LimitTexture;
						} else if (e is TrackManager.SectionChangeEvent) {
							s = 0.2;
							dy = 0.8;
							t = SectionTexture;
						} else if (e is TrackManager.TransponderEvent) {
							s = 0.15;
							dy = 0.4;
							t = TransponderTexture;
						} else if (e is TrackManager.SoundEvent) {
							TrackManager.SoundEvent f = (TrackManager.SoundEvent)e;
							s = 0.2;
							dx = f.Position.X;
							dy = f.Position.Y < 0.1 ? 0.1 : f.Position.Y;
							dz = f.Position.Z;
							t = SoundTexture;
						} else {
							s = 0.2;
							dy = 1.0;
							t = null;
						}
						if (t != null) {
							TrackFollower f = new TrackFollower();
							f.TriggerType = EventTriggerType.None;
							f.TrackPosition = p;
							f.Update(TrackManager.CurrentTrack, p + e.TrackPositionDelta, true, false);
							f.WorldPosition.X += dx * f.WorldSide.X + dy * f.WorldUp.X + dz * f.WorldDirection.X;
							f.WorldPosition.Y += dx * f.WorldSide.Y + dy * f.WorldUp.Y + dz * f.WorldDirection.Y;
							f.WorldPosition.Z += dx * f.WorldSide.Z + dy * f.WorldUp.Z + dz * f.WorldDirection.Z;
							OpenBveShared.Renderer.DrawCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, CameraPosition, t);
						}
					}
				}
			}
			// stops
			for (int i = 0; i < sta.Length; i++) {
				if (sta[i]) {
					for (int j = 0; j < Game.Stations[i].Stops.Length; j++) {
						const double dy = 1.4;
						const double s = 0.2;
						double p = Game.Stations[i].Stops[j].TrackPosition;
						TrackFollower f = new TrackFollower();
						f.TriggerType = EventTriggerType.None;
						f.TrackPosition = p;
						f.Update(TrackManager.CurrentTrack, p, true, false);
						f.WorldPosition.X += dy * f.WorldUp.X;
						f.WorldPosition.Y += dy * f.WorldUp.Y;
						f.WorldPosition.Z += dy * f.WorldUp.Z;
						OpenBveShared.Renderer.DrawCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, CameraPosition, StopTexture);
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
					TrackFollower f = new TrackFollower();
					f.TriggerType = EventTriggerType.None;
					f.TrackPosition = p;
					f.Update(TrackManager.CurrentTrack, p, true, false);
					f.WorldPosition.X += dy * f.WorldUp.X;
					f.WorldPosition.Y += dy * f.WorldUp.Y;
					f.WorldPosition.Z += dy * f.WorldUp.Z;
					OpenBveShared.Renderer.DrawCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, CameraPosition, BufferTexture);
				}
			}
		}
		
		// render overlays
		private static void RenderOverlays(double TimeElapsed) {
			// initialize
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			GL.Enable(EnableCap.Blend);
			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.MatrixMode(MatrixMode.Modelview);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.Ortho(0.0, (double)OpenBveShared.Renderer.Width, (double)OpenBveShared.Renderer.Height, 0.0, -1.0, 1.0);
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// marker
			if (OptionInterface)
			{
				int y = 150;
				for (int i = 0; i < Game.MarkerTextures.Length; i++)
				{
					Textures.LoadTexture(Game.MarkerTextures[i], OpenGlTextureWrapMode.ClampClamp);
					if (Game.MarkerTextures[i] != null) {
						int w = Game.MarkerTextures[i].Width;
						int h = Game.MarkerTextures[i].Height;
						GL.Color4(1.0, 1.0, 1.0, 1.0);
						OpenBveShared.Renderer.DrawRectangle(Game.MarkerTextures[i], new Point(OpenBveShared.Renderer.Width - w - 8, y), new Size(w,h), null);
						y += h + 8;
					}
				}
			}
			// render
			if (!Program.CurrentlyLoading) {
				if (GameObjectManager.ObjectsUsed == 0) {
					string[][] Keys = { new string[] { "F7" }, new string[] { "F8" } };
					OpenBveShared.Renderer.RenderKeys(4, 4, 24, Keys);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Open route", new Point(32,4), TextAlignment.TopLeft, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the options window", new Point(32, 24), TextAlignment.TopLeft, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "v" + System.Windows.Forms.Application.ProductVersion, new Point(OpenBveShared.Renderer.Width - 8, OpenBveShared.Renderer.Height - 8), TextAlignment.BottomRight, Color128.White);
				} else if (OptionInterface) {
					// keys
					string[][] Keys = { new string[] { "F5" }, new string[] { "F7" }, new string[] { "F8" } };
					OpenBveShared.Renderer.RenderKeys(4, 4, 24, Keys);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Reload route", new Point(32, 4), TextAlignment.TopLeft, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Open route", new Point(32, 24), TextAlignment.TopLeft, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the options window", new Point(32, 44), TextAlignment.TopLeft, Color128.White, true);
					Keys = new string[][] { new string[] { "F" }, new string[] { "N" }, new string[] { "E" }, new string[] { "C" }, new string[] { "M" }, new string[] { "I" }};
					OpenBveShared.Renderer.RenderKeys(OpenBveShared.Renderer.Width - 20, 4, 16, Keys);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Wireframe:", new Point(OpenBveShared.Renderer.Width -32, 4), TextAlignment.TopRight, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Normals:", new Point(OpenBveShared.Renderer.Width - 32, 24), TextAlignment.TopRight, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Events:", new Point(OpenBveShared.Renderer.Width - 32, 44), TextAlignment.TopRight, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "CPU:", new Point(OpenBveShared.Renderer.Width - 32, 64), TextAlignment.TopRight, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Mute:", new Point(OpenBveShared.Renderer.Width - 32, 84), TextAlignment.TopRight, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Hide interface:", new Point(OpenBveShared.Renderer.Width - 32, 104), TextAlignment.TopRight, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, (RenderStatsOverlay ? "Hide" : "Show") + " renderer statistics", new Point(OpenBveShared.Renderer.Width - 32, 124), TextAlignment.TopRight, Color128.White, true);
					Keys = new string[][] { new string[] { "F10" } };
					OpenBveShared.Renderer.RenderKeys(OpenBveShared.Renderer.Width - 32, 124, 30, Keys);
					Keys = new string[][] { new string[] { null, "W", null }, new string[] { "A", "S", "D" } };
					OpenBveShared.Renderer.RenderKeys(4, OpenBveShared.Renderer.Height - 40, 16, Keys);
					Keys = new string[][] { new string[] { null, "↑", null }, new string[] { "←", "↓", "→" } };
					OpenBveShared.Renderer.RenderKeys(0 * OpenBveShared.Renderer.Width - 48, OpenBveShared.Renderer.Height - 40, 16, Keys);
					Keys = new string[][] { new string[] { "P↑" }, new string[] { "P↓" } };
					OpenBveShared.Renderer.RenderKeys((int)(0.5 * OpenBveShared.Renderer.Width + 32), OpenBveShared.Renderer.Height - 40, 24, Keys);
					Keys = new string[][] { new string[] { null, "/", "*" }, new string[] { "7", "8", "9" }, new string[] { "4", "5", "6" }, new string[] { "1", "2", "3" }, new string[] { null, "0", "." } };
					OpenBveShared.Renderer.RenderKeys(OpenBveShared.Renderer.Width - 60, OpenBveShared.Renderer.Height - 100, 16, Keys);
					if (Program.JumpToPositionEnabled) {
						OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Jump to track position:", new Point(4, 80),TextAlignment.TopLeft, Color128.White, true);
						double distance;
						if (Double.TryParse(Program.JumpToPositionValue, out distance))
						{
							if (distance < Program.MinimumJumpToPositionValue - 100)
							{
								OpenBveShared.Renderer.DrawString(Fonts.SmallFont, (Environment.TickCount % 1000 <= 500 ? Program.JumpToPositionValue + "_" : Program.JumpToPositionValue), new Point(4, 100), TextAlignment.TopLeft, Color128.Red, true);
							}
							else
							{
								OpenBveShared.Renderer.DrawString(Fonts.SmallFont, (Environment.TickCount % 1000 <= 500 ? Program.JumpToPositionValue + "_" : Program.JumpToPositionValue), new Point(4, 100), TextAlignment.TopLeft, distance > TrackManager.CurrentTrack.Elements[TrackManager.CurrentTrack.Elements.Length - 1].StartingTrackPosition + 100
								? Color128.Red : Color128.Yellow, true);
							}
							
						}
					}
					// info
					double x = 0.5 * (double)OpenBveShared.Renderer.Width - 256.0;
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Position: " + GetLengthString(OpenBveShared.Camera.CameraCurrentAlignment.TrackPosition) + " (X=" + GetLengthString(OpenBveShared.Camera.CameraCurrentAlignment.Position.X) + ", Y=" + GetLengthString(OpenBveShared.Camera.CameraCurrentAlignment.Position.Y) + "), Orientation: (Yaw=" + (OpenBveShared.Camera.CameraCurrentAlignment.Yaw * 57.2957795130824).ToString("0.00", Culture) + "°, Pitch=" + (OpenBveShared.Camera.CameraCurrentAlignment.Pitch * 57.2957795130824).ToString("0.00", Culture) + "°, Roll=" + (OpenBveShared.Camera.CameraCurrentAlignment.Roll * 57.2957795130824).ToString("0.00", Culture) + "°)", new Point((int)x, 4), TextAlignment.TopLeft, Color128.White, true);
					OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Radius: " + GetLengthString(World.CameraTrackFollower.CurveRadius) + ", Cant: " + (1000.0 * World.CameraTrackFollower.CurveCant).ToString("0", Culture) + " mm, Adhesion=" + (100.0 * World.CameraTrackFollower.AdhesionMultiplier).ToString("0", Culture), new Point((int)x, 20), TextAlignment.TopLeft, Color128.White, true);
					if (Program.CurrentStation >= 0) {
						System.Text.StringBuilder t = new System.Text.StringBuilder();
						t.Append(Game.Stations[Program.CurrentStation].Name);
						if (Game.Stations[Program.CurrentStation].ArrivalTime >= 0.0) {
							t.Append(", Arrival: " + GetTime(Game.Stations[Program.CurrentStation].ArrivalTime));
						}
						if (Game.Stations[Program.CurrentStation].DepartureTime >= 0.0) {
							t.Append(", Departure: " + GetTime(Game.Stations[Program.CurrentStation].DepartureTime));
						}
						if (Game.Stations[Program.CurrentStation].OpenLeftDoors & Game.Stations[Program.CurrentStation].OpenRightDoors) {
							t.Append(", [L][R]");
						} else if (Game.Stations[Program.CurrentStation].OpenLeftDoors) {
							t.Append(", [L][-]");
						} else if (Game.Stations[Program.CurrentStation].OpenRightDoors) {
							t.Append(", [-][R]");
						} else {
							t.Append(", [-][-]");
						}
						switch (Game.Stations[Program.CurrentStation].StopMode) {
							case Game.StationStopMode.AllStop:
								t.Append(", Stop");
								break;
							case Game.StationStopMode.AllPass:
								t.Append(", Pass");
								break;
							case Game.StationStopMode.PlayerStop:
								t.Append(", Player stops - others pass");
								break;
							case Game.StationStopMode.PlayerPass:
								t.Append(", Player passes - others stop");
								break;
						}
						if (Game.Stations[Program.CurrentStation].StationType == Game.StationType.ChangeEnds) {
							t.Append(", Change ends");
						}
						t.Append(", Ratio=").Append((100.0 * Game.Stations[Program.CurrentStation].PassengerRatio).ToString("0", Culture)).Append("%");
						OpenBveShared.Renderer.DrawString(Fonts.SmallFont, t.ToString(), new Point((int)x, 36), TextAlignment.TopLeft, Color128.White, true);
					}
					if (Interface.MessageCount == 1) {
						Keys = new string[][] { new string[] { "F9" } };
						OpenBveShared.Renderer.RenderKeys(4, 72, 24, Keys);
						if (Interface.LogMessages[0].Type != MessageType.Information)
						{
							OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the 1 error message recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.Red, true);
						}
						else
						{
							//If all of our messages are information, then print the message text in grey
							OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the 1 message recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.White, true);
						}
					} else if (Interface.MessageCount > 1) {
						Keys = new string[][] { new string[] { "F9" } };
						OpenBveShared.Renderer.RenderKeys(4, 72, 24, Keys);
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
							OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the " + Interface.MessageCount + " error messages recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.Red, true);
						}
						else
						{
							OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Display the " + Interface.MessageCount + " messages recently generated.", new Point(32, 72), TextAlignment.TopLeft, Color128.White, true);
						}
					}
					if (RenderStatsOverlay)
					{
						OpenBveShared.Renderer.RenderKeys(4, OpenBveShared.Renderer.Height - 126, 116, new string[][] { new string[] { "Renderer Statistics" } });
						OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Total static objects: " + GameObjectManager.ObjectsUsed, new Point(4, OpenBveShared.Renderer.Height - 112), TextAlignment.TopLeft, Color128.White, true);
						OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Total animated objects: " + GameObjectManager.AnimatedWorldObjectsUsed, new Point(4, OpenBveShared.Renderer.Height - 100), TextAlignment.TopLeft, Color128.White, true);
						OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Current framerate: " + Game.InfoFrameRate.ToString("0.0", Culture) + "fps", new Point(4, OpenBveShared.Renderer.Height - 88), TextAlignment.TopLeft, Color128.White, true);
						OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Total opaque faces: " + Game.InfoStaticOpaqueFaceCount, new Point(4, OpenBveShared.Renderer.Height - 76), TextAlignment.TopLeft, Color128.White, true);
						OpenBveShared.Renderer.DrawString(Fonts.SmallFont, "Total alpha faces: " + (OpenBveShared.Renderer.DynamicAlpha.FaceCount), new Point(4, OpenBveShared.Renderer.Height - 64), TextAlignment.TopLeft, Color128.White, true);
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
	}
}
