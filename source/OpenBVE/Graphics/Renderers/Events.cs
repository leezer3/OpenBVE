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
using System.Linq;

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

		private bool Initialized;

		internal Events(BaseRenderer renderer)
		{
			this.renderer = renderer;
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
			Initialized = true;
		}

		/// <summary>Renders a graphical visualisation of any events within camera range</summary>
		/// <param name="Camera">The absolute camera position</param>
		internal void Render(Vector3 Camera)
		{
			if (Interface.CurrentOptions.ShowEvents == false || Program.CurrentRoute.Tracks[0].Elements == null)
			{
				return;
			}

			if (!Initialized)
			{
				Init();
			}

			renderedSwitches.Clear();
			GL.Enable(EnableCap.CullFace);
			GL.Enable(EnableCap.DepthTest);
			GL.DepthMask(true);
			renderer.OptionLighting = false;

			double da = -renderer.Camera.BackwardViewingDistance - renderer.Camera.ExtraViewingDistance;
			double db = renderer.Camera.ForwardViewingDistance + renderer.Camera.ExtraViewingDistance;
			bool[] sta = new bool[Program.CurrentRoute.Stations.Length];

			// events
			for (int i = 0; i < Program.CurrentRoute.Tracks[0].Elements.Length; i++)
			{
				double p = Program.CurrentRoute.Tracks[0].Elements[i].StartingTrackPosition;
				double d = p - Program.Renderer.CameraTrackFollower.TrackPosition;

				if (d >= da & d <= db)
				{
					for (int j = 0; j < Program.CurrentRoute.Tracks.Count; j++)
					{
						int railKey = Program.CurrentRoute.Tracks.ElementAt(j).Key;
						// check that element is valid and drivable
						if (i < Program.CurrentRoute.Tracks[railKey].Elements.Length && Program.CurrentRoute.Tracks[railKey].Elements[i].IsDriveable)
						{
							foreach (GeneralEvent e in Program.CurrentRoute.Tracks[railKey].Elements[i].Events)
							{
								double dy, dx = 0.0, dz = 0.0;
								double s;
								Texture t;

								if (e is BrightnessChangeEvent)
								{
									s = 0.15;
									dy = 4.0;
									t = BrightnessChangeTexture;
								}
								else if (e is BackgroundChangeEvent)
								{
									s = 0.25;
									dy = 3.5;
									t = BackgroundChangeTexture;
								}
								else if (e is StationStartEvent startEvent)
								{
									s = 0.25;
									dy = 1.6;
									t = StationStartTexture;
									sta[startEvent.StationIndex] = true;
								}
								else if (e is StationEndEvent endEvent)
								{
									s = 0.25;
									dy = 1.6;
									t = StationEndTexture;
									sta[endEvent.StationIndex] = true;
								}
								else if (e is LimitChangeEvent)
								{
									s = 0.2;
									dy = 1.1;
									t = LimitTexture;
								}
								else if (e is SectionChangeEvent)
								{
									s = 0.2;
									dy = 0.8;
									t = SectionTexture;
								}
								else if (e is TransponderEvent transponderEvent)
								{
									s = 0.15;
									dy = 0.4;
									/*
									 * NOTE:
									 * Beacon 21 is used by legacy beacon based BVE4 weather events
									 * e.g. OS_ATS, UK* family of plugins etc.
									 * Use the weather event texture in this case
									 */
									t = transponderEvent.Type == 21 ? WeatherEventTexture : TransponderTexture;

								}
								else if (e is SoundEvent soundEvent)
								{
									s = 0.2;
									dx = soundEvent.Position.X;
									dy = soundEvent.Position.Y < 0.1 ? 0.1 : soundEvent.Position.Y;
									dz = soundEvent.Position.Z;
									t = SoundTexture;
								}
								else if (e is PointSoundEvent)
								{
									s = 0.2;
									dx = 0;
									dy = 0.2;
									dz = 0;
									t = PointSoundTexture;
								}
								else if (e is RailSoundsChangeEvent)
								{
									s = 0.2;
									dy = 0.8;
									t = RunSoundTexture;
								}
								else if (e is LightingChangeEvent)
								{
									s = 0.2;
									dy = 1.5;
									t = LightingEventTexture;
								}
								else
								{
									s = 0.2;
									dy = 1.0;
									t = null;
								}

								if (e is SwitchEvent sw && !renderedSwitches.Contains(sw.Index))
								{
									s = 0.2;
									dy = 0.8;
									t = SwitchEventTexture;
									renderedSwitches.Add(sw.Index); // as otherwise we'll render the cube once for each track and z-fight
								}

								if (t != null)
								{
									TrackFollower f = new TrackFollower(Program.CurrentHost)
									{
										TrackIndex = railKey,
										TriggerType = EventTriggerType.None,
										TrackPosition = p
									};
									f.UpdateAbsolute(p + e.TrackPositionDelta, true, false);
									f.WorldPosition.X += dx * f.WorldSide.X + dy * f.WorldUp.X + dz * f.WorldDirection.X;
									f.WorldPosition.Y += dx * f.WorldSide.Y + dy * f.WorldUp.Y + dz * f.WorldDirection.Y;
									f.WorldPosition.Z += dx * f.WorldSide.Z + dy * f.WorldUp.Z + dz * f.WorldDirection.Z;

									renderer.Cube.Draw(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera, t);
								}
							}
						}
					}
					
				}
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

						renderer.Cube.Draw(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera, StopTexture);
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

					renderer.Cube.Draw(f.WorldPosition, f.WorldDirection, f.WorldUp, f.WorldSide, s, Camera, BufferTexture);
				}
			}

			renderer.OptionLighting = true;
		}
	}
}
