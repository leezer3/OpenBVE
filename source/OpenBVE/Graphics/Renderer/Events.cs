using OpenTK.Graphics.OpenGL;
using Vector2 = OpenBveApi.Math.Vector2;
using Vector3 = OpenBveApi.Math.Vector3;

namespace OpenBve
{
	internal static partial class Renderer
	{
		private static Textures.Texture BrightnessChangeTexture;
		private static Textures.Texture BackgroundChangeTexture;
		private static Textures.Texture StationStartTexture;
		private static Textures.Texture StationEndTexture;
		private static Textures.Texture LimitTexture;
		private static Textures.Texture SectionTexture;
		private static Textures.Texture TransponderTexture;
		private static Textures.Texture SoundTexture;
		private static Textures.Texture BufferTexture;
		private static Textures.Texture StopTexture;
		private static Textures.Texture PointSoundTexture;

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
		internal static void RenderEvents(Vector3 Camera)
		{
			if (Interface.CurrentOptions.ShowEvents == false || TrackManager.CurrentTrack.Elements == null)
			{
				return;
			}
			
			if (!Initialized)
			{
				Init();
				Initialized = true;
			}
			GL.Enable(EnableCap.CullFace); CullEnabled = true;
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask(true);
			if (LightingEnabled)
			{
				GL.Disable(EnableCap.Lighting);
				LightingEnabled = false;
			}
			if (AlphaTestEnabled)
			{
				GL.Disable(EnableCap.AlphaTest);
				AlphaTestEnabled = false;
			}
			double da = -World.BackwardViewingDistance - World.ExtraViewingDistance;
			double db = World.ForwardViewingDistance + World.ExtraViewingDistance;
			bool[] sta = new bool[Game.Stations.Length];
			// events
			for (int i = 0; i < TrackManager.CurrentTrack.Elements.Length; i++)
			{
				double p = TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
				double d = p - World.CameraTrackFollower.TrackPosition;
				if (d >= da & d <= db)
				{
					for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++)
					{
						TrackManager.GeneralEvent e = TrackManager.CurrentTrack.Elements[i].Events[j];
						double dy, dx = 0.0, dz = 0.0;
						double s; Textures.Texture t;
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
							RenderCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera.X, Camera.Y, Camera.Z, t);
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
						RenderCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera.X, Camera.Y, Camera.Z, StopTexture);
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
					RenderCube(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera.X, Camera.Y, Camera.Z, BufferTexture);
				}
			}
		}
		private static void RenderCube(Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, double Size, double CameraX, double CameraY, double CameraZ, Textures.Texture TextureIndex)
		{
			
			Vector3[] v = new Vector3[8];
			v[0] = new Vector3(Size, Size, -Size);
			v[1] = new Vector3(Size, -Size, -Size);
			v[2] = new Vector3(-Size, -Size, -Size);
			v[3] = new Vector3(-Size, Size, -Size);
			v[4] = new Vector3(Size, Size, Size);
			v[5] = new Vector3(Size, -Size, Size);
			v[6] = new Vector3(-Size, -Size, Size);
			v[7] = new Vector3(-Size, Size, Size);
			for (int i = 0; i < 8; i++)
			{
				World.Rotate(ref v[i].X, ref v[i].Y, ref v[i].Z, Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z, Side.X, Side.Y, Side.Z);
				v[i].X += Position.X - CameraX;
				v[i].Y += Position.Y - CameraY;
				v[i].Z += Position.Z - CameraZ;
			}
			int[][] Faces = new int[6][];
			Faces[0] = new int[] { 0, 1, 2, 3 };
			Faces[1] = new int[] { 0, 4, 5, 1 };
			Faces[2] = new int[] { 0, 3, 7, 4 };
			Faces[3] = new int[] { 6, 5, 4, 7 };
			Faces[4] = new int[] { 6, 7, 3, 2 };
			Faces[5] = new int[] { 6, 2, 1, 5 };
			if (TextureIndex == null || !Textures.LoadTexture(TextureIndex, Textures.OpenGlTextureWrapMode.ClampClamp))
			{
				if (TexturingEnabled)
				{
					GL.Disable(EnableCap.Texture2D);
					TexturingEnabled = false;
				}
				for (int i = 0; i < 6; i++)
				{
					GL.Begin(PrimitiveType.Quads);
					GL.Color3(1.0, 1.0, 1.0);
					for (int j = 0; j < 4; j++)
					{
						GL.Vertex3(v[Faces[i][j]].X, v[Faces[i][j]].Y, v[Faces[i][j]].Z);
					}
					GL.End();
				}
				return;
			}
			else
			{
				TexturingEnabled = true;
				GL.Enable(EnableCap.Texture2D);
			}
			GL.BindTexture(TextureTarget.Texture2D, TextureIndex.OpenGlTextures[(int)Textures.OpenGlTextureWrapMode.ClampClamp].Name);
			Vector2[][] t = new Vector2[6][];
				t[0] = new Vector2[] { new Vector2(1.0, 0.0), new Vector2(1.0, 1.0), new Vector2(0.0, 1.0), new Vector2(0.0, 0.0) };
				t[1] = new Vector2[] { new Vector2(0.0, 0.0), new Vector2(1.0, 0.0), new Vector2(1.0, 1.0), new Vector2(0.0, 1.0) };
				t[2] = new Vector2[] { new Vector2(1.0, 1.0), new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0) };
				t[3] = new Vector2[] { new Vector2(1.0, 1.0), new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0) };
				t[4] = new Vector2[] { new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0), new Vector2(1.0, 1.0) };
				t[5] = new Vector2[] { new Vector2(0.0, 1.0), new Vector2(0.0, 0.0), new Vector2(1.0, 0.0), new Vector2(1.0, 1.0) };
			for (int i = 0; i < 6; i++)
			{
				GL.Begin(PrimitiveType.Quads);
				GL.Color3(1.0, 1.0, 1.0);
				for (int j = 0; j < 4; j++)
				{
					GL.TexCoord2(t[i][j].X, t[i][j].Y);
					GL.Vertex3(v[Faces[i][j]].X, v[Faces[i][j]].Y, v[Faces[i][j]].Z);
				}
				GL.End();
			}
		}
	}
}
