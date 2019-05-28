using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;
using Vector2 = OpenBveApi.Math.Vector2;
using Vector3 = OpenBveApi.Math.Vector3;

namespace OpenBve
{
	internal static partial class Renderer
	{
		private static Texture BrightnessChangeTexture;
		private static Texture BackgroundChangeTexture;
		private static Texture StationStartTexture;
		private static Texture StationEndTexture;
		private static Texture LimitTexture;
		private static Texture SectionTexture;
		private static Texture TransponderTexture;
		private static Texture SoundTexture;
		private static Texture BufferTexture;
		private static Texture StopTexture;
		private static Texture PointSoundTexture;

		private static bool Initialized = false;

		private static void Init()
		{
			Initialized = true;
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
			Textures.RegisterTexture(OpenBveApi.Path.CombineFile(Folder, "switchsound.png"), out PointSoundTexture);
		}

		/// <summary>Renders a graphical visualisation of any events within camera range</summary>
		/// <param name="Camera">The absolute camera position</param>
		private static void RenderEvents(Vector3 Camera)
		{
			if (Interface.CurrentOptions.ShowEvents == false || TrackManager.Tracks[0].Elements == null)
			{
				return;
			}
			
			if (!Initialized)
			{
				Init();
				Initialized = true;
			}
			GL.Enable(EnableCap.CullFace); LibRender.Renderer.CullEnabled = true;
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask(true);
			if (LibRender.Renderer.LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting);
				LibRender.Renderer.LightingEnabled = false;
			}
			if (LibRender.Renderer.AlphaTestEnabled)
			{
				GL.Disable(EnableCap.AlphaTest);
				LibRender.Renderer.AlphaTestEnabled = false;
			}
			double da = -World.BackwardViewingDistance - World.ExtraViewingDistance;
			double db = World.ForwardViewingDistance + World.ExtraViewingDistance;
			bool[] sta = new bool[Game.Stations.Length];
			// events
			for (int i = 0; i < TrackManager.Tracks[0].Elements.Length; i++)
			{
				double p = TrackManager.Tracks[0].Elements[i].StartingTrackPosition;
				double d = p - World.CameraTrackFollower.TrackPosition;
				if (d >= da & d <= db)
				{
					for (int j = 0; j < TrackManager.Tracks[0].Elements[i].Events.Length; j++)
					{
						TrackManager.GeneralEvent e = TrackManager.Tracks[0].Elements[i].Events[j];
						double dy, dx = 0.0, dz = 0.0;
						double s; Texture t;
						if (e is TrackManager.BrightnessChangeEvent)
						{
							s = 0.15;
							dy = 4.0;
							t = BrightnessChangeTexture;
						}
						else if (e is TrackManager.BackgroundChangeEvent)
						{
							s = 0.25;
							dy = 3.5;
							t = BackgroundChangeTexture;
						}
						else if (e is TrackManager.StationStartEvent)
						{
							s = 0.25;
							dy = 1.6;
							t = StationStartTexture;
							TrackManager.StationStartEvent f = (TrackManager.StationStartEvent)e;
							sta[f.StationIndex] = true;
						}
						else if (e is TrackManager.StationEndEvent)
						{
							s = 0.25;
							dy = 1.6;
							t = StationEndTexture;
							TrackManager.StationEndEvent f = (TrackManager.StationEndEvent)e;
							sta[f.StationIndex] = true;
						}
						else if (e is TrackManager.LimitChangeEvent)
						{
							s = 0.2;
							dy = 1.1;
							t = LimitTexture;
						}
						else if (e is TrackManager.SectionChangeEvent)
						{
							s = 0.2;
							dy = 0.8;
							t = SectionTexture;
						}
						else if (e is TrackManager.TransponderEvent)
						{
							s = 0.15;
							dy = 0.4;
							t = TransponderTexture;
						}
						else if (e is TrackManager.SoundEvent)
						{
							TrackManager.SoundEvent f = (TrackManager.SoundEvent)e;
							s = 0.2;
							dx = f.Position.X;
							dy = f.Position.Y < 0.1 ? 0.1 : f.Position.Y;
							dz = f.Position.Z;
							t = SoundTexture;
						}
						else if (e is TrackManager.PointSoundEvent)
						{
							s = 0.2;
							dx = 0;
							dy = 0.2;
							dz = 0;
							t = PointSoundTexture;
						}
						else
						{
							s = 0.2;
							dy = 1.0;
							t = null;
						}
						if (t != null)
						{
							TrackManager.TrackFollower f = new TrackManager.TrackFollower();
							f.TriggerType = TrackManager.EventTriggerType.None;
							f.TrackPosition = p;
							f.Update(p + e.TrackPositionDelta, true, false);
							f.WorldPosition.X += dx * f.WorldSide.X + dy * f.WorldUp.X + dz * f.WorldDirection.X;
							f.WorldPosition.Y += dx * f.WorldSide.Y + dy * f.WorldUp.Y + dz * f.WorldDirection.Y;
							f.WorldPosition.Z += dx * f.WorldSide.Z + dy * f.WorldUp.Z + dz * f.WorldDirection.Z;
							LibRender.Renderer.DrawCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera, t);
						}
					}
				}
			}
			// stops
			for (int i = 0; i < sta.Length; i++)
			{
				if (sta[i])
				{
					for (int j = 0; j < Game.Stations[i].Stops.Length; j++)
					{
						const double dy = 1.4;
						const double s = 0.2;
						double p = Game.Stations[i].Stops[j].TrackPosition;
						TrackManager.TrackFollower f = new TrackManager.TrackFollower();
						f.TriggerType = TrackManager.EventTriggerType.None;
						f.TrackPosition = p;
						f.Update(p, true, false);
						f.WorldPosition.X += dy * f.WorldUp.X;
						f.WorldPosition.Y += dy * f.WorldUp.Y;
						f.WorldPosition.Z += dy * f.WorldUp.Z;
						LibRender.Renderer.DrawCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera, StopTexture);
					}
				}
			}
			// buffers
			for (int i = 0; i < Game.BufferTrackPositions.Length; i++)
			{
				double p = Game.BufferTrackPositions[i];
				double d = p - World.CameraTrackFollower.TrackPosition;
				if (d >= da & d <= db)
				{
					const double dy = 2.5;
					const double s = 0.25;
					TrackManager.TrackFollower f = new TrackManager.TrackFollower();
					f.TriggerType = TrackManager.EventTriggerType.None;
					f.TrackPosition = p;
					f.Update(p, true, false);
					f.WorldPosition.X += dy * f.WorldUp.X;
					f.WorldPosition.Y += dy * f.WorldUp.Y;
					f.WorldPosition.Z += dy * f.WorldUp.Z;
					LibRender.Renderer.DrawCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera, BufferTexture);
				}
			}
		}
	}
}
