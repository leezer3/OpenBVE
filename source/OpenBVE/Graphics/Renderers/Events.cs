using LibRender2;
using OpenBveApi;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;
using RouteManager2.Events;
using RouteManager2.Tracks;
using System;
using System.Collections.Generic;

namespace OpenBve.Graphics.Renderers
{
	internal class Events
	{
		private readonly BaseRenderer renderer;

		private Texture BrightnessChangeTexture;
		private Texture BackgroundChangeTexture;
		private Texture StationStartTexture;
		private Texture StationEndTexture;
		private Texture LimitTexture;
		private Texture SectionTexture;
		private Texture TransponderTexture;
		private Texture SoundTexture;
		private Texture BufferTexture;
		private Texture StopTexture;
		private Texture PointSoundTexture;
		private Texture RunSoundTexture;
		private Texture LightingEventTexture;
		private Texture WeatherEventTexture;
		private Texture SwitchEventTexture;
		private readonly List<Guid> renderedSwitches = new List<Guid>();

		internal double LastTrackPosition;

		internal Events(BaseRenderer renderer)
		{
			this.renderer = renderer;
			Init();
		}

		private void Init()
		{
			string Folder = Path.CombineDirectory(Program.FileSystem.GetDataFolder(), "RouteViewer");
			renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "background.png"), out BackgroundChangeTexture);
			renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "brightness.png"), out BrightnessChangeTexture);
			renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "transponder.png"), out TransponderTexture);
			renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "section.png"), out SectionTexture);
			renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "limit.png"), out LimitTexture);
			renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "station_start.png"), out StationStartTexture);
			renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "station_end.png"), out StationEndTexture);
			renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "stop.png"), out StopTexture);
			renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "buffer.png"), out BufferTexture);
			renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "sound.png"), out SoundTexture);
			renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "switchsound.png"), out PointSoundTexture);
			renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "runsound.png"), out RunSoundTexture);
			renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "lighting.png"), out LightingEventTexture);
			renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "weather.png"), out WeatherEventTexture);
			renderer.TextureManager.RegisterTexture(Path.CombineFile(Folder, "switchevent.png"), out SwitchEventTexture);
		}

		/// <summary>Finds visible events on a rail index</summary>
		/// <param name="railIndex">The rail index</param>
		internal void FindEvents(int railIndex)
		{
			if (Program.CurrentRoute.Tracks[railIndex].Elements == null)
			{
				return;
			}

			GL.Enable(EnableCap.CullFace);
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask(true);

			double da = -renderer.Camera.BackwardViewingDistance - renderer.Camera.ExtraViewingDistance;
			double db = renderer.Camera.ForwardViewingDistance + renderer.Camera.ExtraViewingDistance;
			bool[] sta = new bool[Program.CurrentRoute.Stations.Length];

			// events
			for (int i = 0; i < Program.CurrentRoute.Tracks[railIndex].Elements.Length; i++)
			{
				double p = Program.CurrentRoute.Tracks[railIndex].Elements[i].StartingTrackPosition;
				double d = p - renderer.CameraTrackFollower.TrackPosition;

				if (d >= da & d <= db)
				{
					foreach (GeneralEvent e in Program.CurrentRoute.Tracks[railIndex].Elements[i].Events)
					{
						double dy, dx = 0.0, dz = 0.0;
						Texture t;

						if (e is BrightnessChangeEvent)
						{
							dy = 4.0;
							t = BrightnessChangeTexture;
						}
						else if (e is BackgroundChangeEvent)
						{
							dy = 3.5;
							t = BackgroundChangeTexture;
						}
						else if (e is StationStartEvent startEvent)
						{
							dy = 1.6;
							t = StationStartTexture;
							sta[startEvent.StationIndex] = true;
						}
						else if (e is StationEndEvent endEvent)
						{
							dy = 1.6;
							t = StationEndTexture;
							sta[endEvent.StationIndex] = true;
						}
						else if (e is LimitChangeEvent)
						{
							dy = 1.1;
							t = LimitTexture;
						}
						else if (e is SectionChangeEvent)
						{
							dy = 0.8;
							t = SectionTexture;
						}
						else if (e is TransponderEvent transponderEvent)
						{
							dy = 0.4;
							// beacon type 21 is reserved for legacy weather events
							t = transponderEvent.Type == 21 ? WeatherEventTexture : TransponderTexture;

						}
						else if (e is SoundEvent soundEvent)
						{
							dx = soundEvent.Position.X;
							dy = soundEvent.Position.Y < 0.1 ? 0.1 : soundEvent.Position.Y;
							dz = soundEvent.Position.Z;
							t = SoundTexture;
						}
						else if (e is PointSoundEvent)
						{
							dx = 0;
							dy = 0.2;
							dz = 0;
							t = PointSoundTexture;
						}
						else if (e is RailSoundsChangeEvent)
						{
							dy = 0.8;
							t = RunSoundTexture;
						}
						else if (e is LightingChangeEvent)
						{
							dy = 1.5;
							t = LightingEventTexture;
						}
						else
						{
							dy = 1.0;
							t = null;
						}

						if (e is SwitchEvent sw && !renderedSwitches.Contains(sw.Index))
						{
							dy = 0.8;
							t = SwitchEventTexture;
							renderedSwitches.Add(sw.Index); // as otherwise we'll render the cube once for each track and z-fight
						}

						if (t != null)
						{
							TrackFollower f = new TrackFollower(Program.CurrentHost)
							{
								TriggerType = EventTriggerType.None,
								TrackPosition = p,
								TrackIndex = railIndex
							};

							f.UpdateAbsolute(p + e.TrackPositionDelta, true, false);
							Vector3 cubePos = new Vector3(f.WorldPosition);
							cubePos.X += dx * f.WorldSide.X + dy * f.WorldUp.X + dz * f.WorldDirection.X;
							cubePos.Y += dx * f.WorldSide.Y + dy * f.WorldUp.Y + dz * f.WorldDirection.Y;
							cubePos.Z += dx * f.WorldSide.Z + dy * f.WorldUp.Z + dz * f.WorldDirection.Z;

							if (!renderer.CubesToDraw.ContainsKey(t))
							{
								renderer.CubesToDraw.Add(t, new HashSet<Vector3>());
							}

							renderer.CubesToDraw[t].Add(cubePos);
						}
					}
				}

				if (d > db)
				{
					break;
				}
			}

			if (railIndex != 0)
			{
				return;
			}

			// stops
			for (int i = 0; i < sta.Length; i++)
			{
				if (sta[i])
				{
					for (int j = 0; j < Program.CurrentRoute.Stations[i].Stops.Length; j++)
					{
						const double dy = 1.4;
						const double s = 0.2;
						double p = Program.CurrentRoute.Stations[i].Stops[j].TrackPosition;
						TrackFollower f = new TrackFollower(Program.CurrentHost)
						{
							TriggerType = EventTriggerType.None,
							TrackPosition = p
						};
						f.UpdateAbsolute(p, true, false);
						f.WorldPosition.X += dy * f.WorldUp.X;
						f.WorldPosition.Y += dy * f.WorldUp.Y;
						f.WorldPosition.Z += dy * f.WorldUp.Z;

						renderer.Cube.Draw(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, renderer.Camera.AbsolutePosition, StopTexture);
					}
				}
			}

			// buffers
			foreach (BufferStop stop in Program.CurrentRoute.BufferTrackPositions)
			{
				double d = stop.TrackPosition - Program.Renderer.CameraTrackFollower.TrackPosition;

				if (d >= da & d <= db)
				{
					const double dy = 2.5;
					const double s = 0.25;
					TrackFollower f = new TrackFollower(Program.CurrentHost)
					{
						TriggerType = EventTriggerType.None,
						TrackPosition = stop.TrackPosition,
						TrackIndex = stop.TrackIndex
					};
					f.UpdateAbsolute(stop.TrackPosition, true, false);
					f.WorldPosition.X += dy * f.WorldUp.X;
					f.WorldPosition.Y += dy * f.WorldUp.Y;
					f.WorldPosition.Z += dy * f.WorldUp.Z;

					renderer.Cube.Draw(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, renderer.Camera.AbsolutePosition, BufferTexture);
				}
			}
		}
	}
}
