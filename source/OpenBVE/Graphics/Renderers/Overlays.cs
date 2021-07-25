﻿using System;
using System.Drawing;
using DavyKager;
using LibRender2.Overlays;
using LibRender2.Screens;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenBveApi.Trains;
using OpenTK.Graphics.OpenGL;
using Vector2 = OpenBveApi.Math.Vector2;

namespace OpenBve.Graphics.Renderers
{
	internal partial class Overlays
	{
		/* --------------------------------------------------------------
		 * This file contains the base drawing routines for screen overlays
		 * -------------------------------------------------------------- */

		private readonly NewRenderer renderer;

		private bool PauseAnnounced;

		// fade to black
		private double FadeToBlackDueToChangeEnds;

		internal Overlays(NewRenderer renderer)
		{
			this.renderer = renderer;
		}

		/// <summary>Is called once by the main renderer loop, in order to render all overlays shown on the screen</summary>
		/// <param name="TimeElapsed">The time elapsed since the last call to this function</param>
		internal void Render(double TimeElapsed)
		{
			//Initialize openGL
			renderer.SetBlendFunc();
			GL.Enable(EnableCap.Blend);
			renderer.PushMatrix(MatrixMode.Projection);
			Matrix4D.CreateOrthographicOffCenter(0.0f, renderer.Screen.Width, renderer.Screen.Height, 0.0f, -1.0f, 1.0f, out renderer.CurrentProjectionMatrix);
			renderer.PushMatrix(MatrixMode.Modelview);
			renderer.CurrentViewMatrix = Matrix4D.Identity;

			//Check which overlays to show
			switch (renderer.CurrentOutputMode)
			{
				case OutputMode.Default:

					//Route info overlay (if selected)
					Game.routeInfoOverlay.Show();

					//HUD
					foreach (HUD.Element element in HUD.CurrentHudElements)
					{
						switch (element.Subject.ToLowerInvariant())
						{
							case "messages":
								RenderGameMessages(element, TimeElapsed);
								break;
							case "scoremessages":
								RenderScoreMessages(element, TimeElapsed);
								break;
							case "ats":
								RenderATSLamps(element, TimeElapsed);
								break;
							default:
								RenderHUDElement(element, TimeElapsed);
								break;
						}
					}

					//Marker textures
					if (Interface.CurrentOptions.GameMode != GameMode.Expert)
					{
						double y = 8.0;

						foreach (Texture t in renderer.Marker.MarkerTextures)
						{
							if (Program.CurrentHost.LoadTexture(t, OpenGlTextureWrapMode.ClampClamp))
							{
								double w = t.Width;
								double h = t.Height;
								renderer.Rectangle.Draw(t, new OpenBveApi.Math.Vector2(renderer.Screen.Width - w - 8.0,y), new Vector2(w, h), Color128.White);
								y += h + 8.0;
							}
						}
					}

					//Timetable overlay
					//NOTE: Only affects auto-generated timetable, possibly change this inconsistant behaviour
					if (Program.Renderer.CurrentTimetable == DisplayedTimetable.Default)
					{
						// default
						if (Program.CurrentHost.LoadTexture(Timetable.DefaultTimetableTexture, OpenGlTextureWrapMode.ClampClamp))
						{
							int w = Timetable.DefaultTimetableTexture.Width;
							int h = Timetable.DefaultTimetableTexture.Height;
							renderer.Rectangle.Draw(Timetable.DefaultTimetableTexture, new OpenBveApi.Math.Vector2(renderer.Screen.Width - w, Timetable.DefaultTimetablePosition), new Vector2(w, h), Color128.White);
						}
					}
					else if (Program.Renderer.CurrentTimetable == DisplayedTimetable.Custom & Timetable.CustomObjectsUsed == 0)
					{
						// custom
						if (Program.CurrentHost.LoadTexture(Timetable.CurrentCustomTimetableDaytimeTexture, OpenGlTextureWrapMode.ClampClamp))
						{
							int w = Timetable.CurrentCustomTimetableDaytimeTexture.Width;
							int h = Timetable.CurrentCustomTimetableDaytimeTexture.Height;
							renderer.Rectangle.Draw(Timetable.CurrentCustomTimetableDaytimeTexture, new OpenBveApi.Math.Vector2(renderer.Screen.Width - w, Timetable.CustomTimetablePosition), new Vector2(w, h), Color128.White);
						}

						if (Program.CurrentHost.LoadTexture(Timetable.CurrentCustomTimetableDaytimeTexture, OpenGlTextureWrapMode.ClampClamp))
						{
							int w = Timetable.CurrentCustomTimetableDaytimeTexture.Width;
							int h = Timetable.CurrentCustomTimetableDaytimeTexture.Height;
							float alpha;

							if (Timetable.CurrentCustomTimetableDaytimeTexture != null)
							{
								double t = (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].TrackPosition - TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Brightness.PreviousTrackPosition) / (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Brightness.NextTrackPosition - TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Brightness.PreviousTrackPosition);
								alpha = (float)((1.0 - t) * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Brightness.PreviousBrightness + t * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Brightness.NextBrightness);
							}
							else
							{
								alpha = 1.0f;
							}

							renderer.Rectangle.Draw(Timetable.CurrentCustomTimetableDaytimeTexture, new OpenBveApi.Math.Vector2(renderer.Screen.Width - w, Timetable.CustomTimetablePosition), new Vector2(w, h), new Color128(1.0f, 1.0f, 1.0f, alpha));
						}
					}
					break;
				case OutputMode.Debug:
					RenderDebugOverlays();
					break;
				case OutputMode.DebugATS:
					RenderATSDebugOverlay();
					break;
			}

			// air brake debug output
			if (Interface.CurrentOptions.GameMode != GameMode.Expert & renderer.OptionBrakeSystems)
			{
				RenderBrakeSystemDebug();
			}

			switch (Program.Renderer.CurrentInterface)
			{
				case InterfaceType.Pause:
				{
					//If paused, fade out the screen & write PAUSE
					renderer.Rectangle.Draw(null, Vector2.Null, new Vector2(renderer.Screen.Width, renderer.Screen.Height), new Color128(0.0f, 0.0f, 0.0f, 0.5f));
					renderer.OpenGlString.Draw(renderer.Fonts.VeryLargeFont, Translations.GetInterfaceString("menu_pause_title"), new Vector2(renderer.Screen.Width / 2.0, renderer.Screen.Height / 2.0), TextAlignment.CenterMiddle, Color128.White, true);
					if (!PauseAnnounced)
					{
						if (!Tolk.Output(Translations.GetInterfaceString("menu_pause_title")))
						{
							Interface.CurrentOptions.ScreenReaderAvailable = false;
						}
						PauseAnnounced = true;
					}
					break;
				}
				case InterfaceType.Menu:
					Game.Menu.Draw(TimeElapsed);
					PauseAnnounced = false;
					break;
				default:
					PauseAnnounced = false;
					break;
					 
			}

			//Fade to black on change ends
			if (TrainManager.PlayerTrain != null)
			{
				if (TrainManager.PlayerTrain.Station >= 0 && Program.CurrentRoute.Stations[TrainManager.PlayerTrain.Station].Type == StationType.ChangeEnds && TrainManager.PlayerTrain.StationState == TrainStopState.Boarding)
				{
					double time = TrainManager.PlayerTrain.StationDepartureTime - Program.CurrentRoute.SecondsSinceMidnight;
					if (time < 1.0)
					{
						FadeToBlackDueToChangeEnds = Math.Max(0.0, 1.0 - time);
					}
					else if (FadeToBlackDueToChangeEnds > 0.0)
					{
						FadeToBlackDueToChangeEnds -= TimeElapsed;
						if (FadeToBlackDueToChangeEnds < 0.0)
						{
							FadeToBlackDueToChangeEnds = 0.0;
						}
					}
				}
				else if (FadeToBlackDueToChangeEnds > 0.0)
				{
					FadeToBlackDueToChangeEnds -= TimeElapsed;
					if (FadeToBlackDueToChangeEnds < 0.0)
					{
						FadeToBlackDueToChangeEnds = 0.0;
					}
				}
				if (FadeToBlackDueToChangeEnds > 0.0 & (renderer.Camera.CurrentMode == CameraViewMode.Interior | renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead))
				{
					renderer.Rectangle.Draw(null, Vector2.Null, new Vector2(renderer.Screen.Width, renderer.Screen.Height), new Color128(0.0f, 0.0f, 0.0f, (float)FadeToBlackDueToChangeEnds));
				}
			}

			// finalize
			renderer.PopMatrix(MatrixMode.Projection);
			renderer.PopMatrix(MatrixMode.Modelview);
		}
	}
}
