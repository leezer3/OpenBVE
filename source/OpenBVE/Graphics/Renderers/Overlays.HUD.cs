using System;
using LibRender2.Overlays;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Textures;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Trains;
using SoundManager;
using TrainManager.Car;
using TrainManager.Handles;
using static TrainManager.TrainManagerBase;

namespace OpenBve.Graphics.Renderers
{
	internal partial class Overlays
	{
		/// <summary>Renders all default HUD elements</summary>
		/// <param name="Element">The HUD element these are to be rendered onto</param>
		/// <param name="TimeElapsed">The time elapsed</param>
		private void RenderHUDElement(HUD.Element Element, double TimeElapsed)
		{
			if (PlayerTrain == null || (int)Element.Subject < 100 && Interface.CurrentOptions.GameMode == GameMode.Developer)
			{
				return;
			}
			TrainDoorState LeftDoors = PlayerTrain.GetDoorsState(true, false);
			TrainDoorState RightDoors = PlayerTrain.GetDoorsState(false, true);
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// default
			double w, h;
			if (Element.CenterMiddle.BackgroundTexture != null)
			{
				if (Program.CurrentHost.LoadTexture(ref Element.CenterMiddle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					w = Element.CenterMiddle.BackgroundTexture.Width;
					h = Element.CenterMiddle.BackgroundTexture.Height;
				}
				else
				{
					w = 0.0; h = 0.0;
				}
			}
			else
			{
				w = 0.0; h = 0.0;
			}

			int stationIndex;
			if (PlayerTrain.Station >= 0 && PlayerTrain.StationState != TrainStopState.Completed)
			{
				stationIndex = PlayerTrain.LastStation;
			}
			else
			{
				stationIndex = PlayerTrain.LastStation + 1;
			}
			if (stationIndex > Program.CurrentRoute.Stations.Length - 1)
			{
				stationIndex = PlayerTrain.LastStation;
			}

			double x = Element.Alignment.X < 0 ? 0.0 : Element.Alignment.X == 0 ? 0.5 * (Program.Renderer.Screen.Width - w) : Program.Renderer.Screen.Width - w;
			double y = Element.Alignment.Y < 0 ? 0.0 : Element.Alignment.Y == 0 ? 0.5 * (Program.Renderer.Screen.Height - h) : Program.Renderer.Screen.Height - h;
			x += Element.Position.X;
			y += Element.Position.Y;
			// command
			const double speed = 1.0;
			MessageColor sc = MessageColor.None;
			string t;
			switch (Element.Subject)
			{
				case HUDSubject.Reverser:
					t = PlayerTrain.Handles.Reverser.GetNotchDescription(out sc);
					Element.TransitionState = 0.0;
					break;
				case HUDSubject.Power:
					if (PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
					{
						return;
					}
					t = PlayerTrain.Handles.Power.GetNotchDescription(out sc);
					Element.TransitionState = 0.0;
					break;
				case HUDSubject.Single:
					if (PlayerTrain.Handles.HandleType != HandleType.SingleHandle)
					{
						return;
					}
					t = PlayerTrain.Handles.Power.GetNotchDescription(out sc);
					Element.TransitionState = 0.0;
					break;
				case HUDSubject.Brake:
					if (PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
					{
						return;
					}
					t = PlayerTrain.Handles.Brake.GetNotchDescription(out sc);
					Element.TransitionState = 0.0;
					break;
				case HUDSubject.LocoBrake:
					if (!PlayerTrain.Handles.HasLocoBrake)
					{
						return;
					}
					t = PlayerTrain.Handles.LocoBrake.GetNotchDescription(out sc);
					Element.TransitionState = 0.0;
					break;
				case HUDSubject.DoorsLeft:
				case HUDSubject.DoorsRight:
				{
					if ((LeftDoors & TrainDoorState.AllClosed) == 0 | (RightDoors & TrainDoorState.AllClosed) == 0)
					{
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					}
					TrainDoorState Doors = Element.Subject == HUDSubject.DoorsLeft ? LeftDoors : RightDoors;
					if ((Doors & TrainDoorState.Mixed) != 0)
					{
						sc = MessageColor.Orange;
					}
					else if ((Doors & TrainDoorState.AllClosed) != 0)
					{
						sc = MessageColor.Gray;
					}
					else if (PlayerTrain.Specs.DoorCloseMode == DoorMode.Manual)
					{
						sc = MessageColor.Green;
					}
					else
					{
						sc = MessageColor.Blue;
					}
					t = Element.Subject == HUDSubject.DoorsLeft ? Translations.QuickReferences.DoorsLeft : Translations.QuickReferences.DoorsRight;
				} break;
				case HUDSubject.StopLeft:
				case HUDSubject.StopRight:
				case HUDSubject.StopNone:
				{
					int s = PlayerTrain.Station;
					if (s >= 0 && Program.CurrentRoute.Stations[s].PlayerStops() && Interface.CurrentOptions.GameMode != GameMode.Expert)
					{
						bool cond;
						switch (Element.Subject)
						{
							case HUDSubject.StopLeft:
								cond = Program.CurrentRoute.Stations[s].OpenLeftDoors;
								break;
							case HUDSubject.StopRight:
								cond = Program.CurrentRoute.Stations[s].OpenRightDoors;
								break;
							default:
								cond = !Program.CurrentRoute.Stations[s].OpenLeftDoors & !Program.CurrentRoute.Stations[s].OpenRightDoors;
								break;
						}
						if (PlayerTrain.StationState == TrainStopState.Pending & cond)
						{
							Element.TransitionState -= speed * TimeElapsed;
							if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
						}
						else
						{
							Element.TransitionState += speed * TimeElapsed;
							if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
						}
					}
					else
					{
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					}
					t = Element.Text;
				} break;
				case HUDSubject.StopLeftTick:
				case HUDSubject.StopRightTick:
				case HUDSubject.StopNoneTick:
				{
					int s = PlayerTrain.Station;
					if (s >= 0 && Program.CurrentRoute.Stations[s].PlayerStops() && Interface.CurrentOptions.GameMode != GameMode.Expert && !Program.CurrentRoute.Stations[s].Dummy)
					{
						int c = Program.CurrentRoute.Stations[s].GetStopIndex(PlayerTrain);
						if (c >= 0)
						{
							bool cond;
							switch (Element.Subject)
							{
								case HUDSubject.StopLeftTick:
									cond = Program.CurrentRoute.Stations[s].OpenLeftDoors;
									break;
								case HUDSubject.StopRightTick:
									cond = Program.CurrentRoute.Stations[s].OpenRightDoors;
									break;
								default:
									cond = !Program.CurrentRoute.Stations[s].OpenLeftDoors & !Program.CurrentRoute.Stations[s].OpenRightDoors;
									break;
							}
							if (PlayerTrain.StationState == TrainStopState.Pending & cond)
							{
								Element.TransitionState -= speed * TimeElapsed;
								if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
							}
							else
							{
								Element.TransitionState += speed * TimeElapsed;
								if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
							}
							double d = PlayerTrain.StationDistanceToStopPoint;
							double r;
							if (d > 0.0)
							{
								r = d / Program.CurrentRoute.Stations[s].Stops[c].BackwardTolerance;
							}
							else
							{
								r = d / Program.CurrentRoute.Stations[s].Stops[c].ForwardTolerance;
							}
							if (r < -1.0) r = -1.0;
							if (r > 1.0) r = 1.0;
							y -= r * Element.Value1;
							if (Interface.CurrentOptions.Accessibility)
							{
								double beepSpeed = Math.Abs(r);
								if (beepSpeed < 0.01)
								{
									beepSpeed = 0;
								}
								else
								{
									beepSpeed = 1.0 / beepSpeed;
								}


								if (PlayerTrain.StationState != TrainStopState.Pending || beepSpeed == 0)
								{
									// We're stopped or beep is not required
									HUD.StationAdjustBeepSource?.Stop();
								}
								else
								{
									if (Element.TransitionState == 0.0)
									{
										HUD.StationAdjustBeepSource?.Stop();
									}
									else
									{
										if (HUD.StationAdjustBeepSource != null && HUD.StationAdjustBeepSource.IsPlaying())
										{
											HUD.StationAdjustBeepSource.Volume = beepSpeed * 0.25;
											HUD.StationAdjustBeepSource.Position = Program.Renderer.Camera.AbsolutePosition;
										}
										else
										{
											HUD.StationAdjustBeepSource = (SoundSource)Program.CurrentHost.PlaySound(HUD.StationAdjustBeep, 2.0, beepSpeed * 0.25, Program.Renderer.Camera.AbsolutePosition, null, true);
										}	
									}
									
								}
								
							}
						}
						else
						{
							Element.TransitionState += speed * TimeElapsed;
							if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
						}
					}
					else
					{
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					}
					t = Element.Text;
				} break;
				case HUDSubject.Clock:
				{
					int hours = (int)Math.Floor(Program.CurrentRoute.SecondsSinceMidnight);
					int seconds = hours % 60; hours /= 60;
					int minutes = hours % 60; hours /= 60;
					hours %= 24;
					t = hours.ToString(Culture).PadLeft(2, '0') + ":" + minutes.ToString(Culture).PadLeft(2, '0') + ":" + seconds.ToString(Culture).PadLeft(2, '0');
					if (renderer.OptionClock)
					{
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					}
				} break;
				case HUDSubject.Gradient:
					switch (renderer.OptionGradient)
					{
						case GradientDisplayMode.Percentage:
							if (Program.Renderer.CameraTrackFollower.Pitch != 0)
							{
								double pc = Program.Renderer.CameraTrackFollower.Pitch;
								t = Math.Abs(pc).ToString("0.00", Culture) + "%" + (Math.Abs(pc) == pc ? " ↗" : " ↘");
							}
							else
							{
								t = "Level";
							}
							Element.TransitionState -= speed * TimeElapsed;
							if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
							break;
						case GradientDisplayMode.UnitOfChange:
							if (Program.Renderer.CameraTrackFollower.Pitch != 0)
							{
								double gr = 1000 / Program.Renderer.CameraTrackFollower.Pitch;
								t = "1 in " + Math.Abs(gr).ToString("0", Culture) + (Math.Abs(gr) == gr ? " ↗" : " ↘");
							}
							else
							{
								t = "Level";
							}
							Element.TransitionState -= speed * TimeElapsed;
							if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
							break;
						case GradientDisplayMode.Permil:
							if (Program.Renderer.CameraTrackFollower.Pitch != 0)
							{
								double pm = Program.Renderer.CameraTrackFollower.Pitch * 10;
								t = Math.Abs(pm).ToString("0.00", Culture) + "‰" + (Math.Abs(pm) == pm ? " ↗" : " ↘");
							}
							else
							{
								t = "Level";
							}
							Element.TransitionState -= speed * TimeElapsed;
							if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
							break;
						default:
							if (Program.Renderer.CameraTrackFollower.Pitch != 0)
							{
								double gr = 1000 / Program.Renderer.CameraTrackFollower.Pitch;
								t = "1 in " + Math.Abs(gr).ToString("0", Culture) + (Math.Abs(gr) == gr ? " ↗" : " ↘");
							}
							else
							{
								t = "Level";
							}
							Element.TransitionState += speed * TimeElapsed;
							if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
							break;
					} break;
				case HUDSubject.Speed:
					switch (renderer.OptionSpeed)
					{
						case SpeedDisplayMode.Kmph:
							double kmph = Math.Abs(PlayerTrain.CurrentSpeed) * 3.6;
							t = kmph.ToString("0.00", Culture) + " km/h";
							Element.TransitionState -= speed * TimeElapsed;
							if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
							break;
						case SpeedDisplayMode.Mph:
							double mph = Math.Abs(PlayerTrain.CurrentSpeed) * 2.2369362920544;
							t = mph.ToString("0.00", Culture) + " mph";
							Element.TransitionState -= speed * TimeElapsed;
							if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
							break;
						default:
							mph = Math.Abs(PlayerTrain.CurrentSpeed) * 2.2369362920544;
							t = mph.ToString("0.00", Culture) + " mph";
							Element.TransitionState += speed * TimeElapsed;
							if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
							break;
					} break;
				case HUDSubject.DistNextStation:
					t = string.Empty;
					if (!Program.CurrentRoute.Stations[stationIndex].PlayerStops())
					{
						int n = Program.CurrentRoute.Stations[stationIndex].GetStopIndex(PlayerTrain);
						double p0 = PlayerTrain.FrontCarTrackPosition;
						double p1 = Program.CurrentRoute.Stations[stationIndex].Stops.Length > 0 ? Program.CurrentRoute.Stations[stationIndex].Stops[n].TrackPosition : Program.CurrentRoute.Stations[stationIndex].DefaultTrackPosition;
						double m = p1 - p0;
						if (m < 0)
						{
							m = 0.0; //Don't display negative numbers when passing (stop zone goes beyond the absolute station limit)
						}
						switch (renderer.OptionDistanceToNextStation)
						{
							case DistanceToNextStationDisplayMode.Km:
								m /= 1000.0;
								t = "Pass: " + m.ToString("0.000", Culture) + " km";
								Element.TransitionState -= speed * TimeElapsed;
								if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
								break;
							case DistanceToNextStationDisplayMode.Mile:
								m /= 1609.34;
								t = "Pass: " + m.ToString("0.0000", Culture) + " miles";
								Element.TransitionState -= speed * TimeElapsed;
								if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
								break;
							default:
								m /= 1609.34;
								t = "Pass: " + m.ToString("0.0000", Culture) + " miles";
								Element.TransitionState += speed * TimeElapsed;
								if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
								break;
						}
					}
					break;
				case HUDSubject.DistNextStation2:
					if (!Program.CurrentRoute.Stations[stationIndex].PlayerStops())
					{
						double p0 = PlayerTrain.FrontCarTrackPosition;
						double p1 = 0.0;
						for (int i = stationIndex; i < Program.CurrentRoute.Stations.Length; i++)
						{
							if (Program.CurrentRoute.Stations[i].PlayerStops())
							{
								int n = Program.CurrentRoute.Stations[i].GetStopIndex(PlayerTrain);
								p1 = Program.CurrentRoute.Stations[i].Stops.Length > 0 ? Program.CurrentRoute.Stations[i].Stops[n].TrackPosition : Program.CurrentRoute.Stations[i].DefaultTrackPosition;
								break;
							}
						}
						
						double m = p1 - p0;
						switch (renderer.OptionDistanceToNextStation)
						{
							case DistanceToNextStationDisplayMode.Km:
								m /= 1000.0;
								t = "Next Stop: " + m.ToString("0.000", Culture) + " km";
								Element.TransitionState -= speed * TimeElapsed;
								if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
								break;
							case DistanceToNextStationDisplayMode.Mile:
								m /= 1609.34;
								t = "Next Stop: " + m.ToString("0.0000", Culture) + " miles";
								Element.TransitionState -= speed * TimeElapsed;
								if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
								break;
							default:
								m /= 1609.34;
								t = "Next Stop: " + m.ToString("0.0000", Culture) + " miles";
								Element.TransitionState += speed * TimeElapsed;
								if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
								break;
						}
					}
					else
					{
						int n = Program.CurrentRoute.Stations[stationIndex].GetStopIndex(PlayerTrain);
						double p0 = PlayerTrain.FrontCarTrackPosition;
						double p1 = Program.CurrentRoute.Stations[stationIndex].Stops.Length > 0 ? Program.CurrentRoute.Stations[stationIndex].Stops[n].TrackPosition : Program.CurrentRoute.Stations[stationIndex].DefaultTrackPosition;
						double m = p1 - p0;
						switch (renderer.OptionDistanceToNextStation)
						{
							case DistanceToNextStationDisplayMode.Km:
								if (Math.Abs(m) <= 10.0)
								{
									t = "Stop: " + m.ToString("0.00", Culture) + " m";
								}
								else
								{
									m /= 1000.0;
									t = "Stop: " + m.ToString("0.000", Culture) + " km";
								}
								Element.TransitionState -= speed * TimeElapsed;
								if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
								break;
							case DistanceToNextStationDisplayMode.Mile:
								m /= 1609.34;
								t = "Stop: " + m.ToString("0.0000", Culture) + " miles";
								Element.TransitionState -= speed * TimeElapsed;
								if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
								break;
							default:
								m /= 1609.34;
								t = "Stop: " + m.ToString("0.0000", Culture) + " miles";
								Element.TransitionState += speed * TimeElapsed;
								if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
								break;
						}
					}
					break;
				case HUDSubject.FPS:
					int fps = (int)Math.Round(Program.Renderer.FrameRate);
					t = fps.ToString(Culture) + " fps";
					if (renderer.OptionFrameRates)
					{
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					} break;
				case HUDSubject.AI:
					t = "A.I.";
					if (PlayerTrain.AI != null)
					{
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					} break;
				case HUDSubject.Score:
					Element.TransitionState = 1.0;
					t = string.Empty;
					if (Interface.CurrentOptions.GameMode != GameMode.Arcade)
					{
						break;
					}
					t = Game.CurrentScore.CurrentValue.ToString(Culture) + " / " +
					    Game.CurrentScore.Maximum.ToString(Culture);
					if (Game.CurrentScore.CurrentValue < 0)
					{
						sc = MessageColor.Red;
					}
					else if (Game.CurrentScore.CurrentValue > 0)
					{
						sc = MessageColor.Green;
					}
					else
					{
						sc = MessageColor.Gray;
					}

					Element.TransitionState = 0.0;
					break;
				default:
					t = Element.Text;
					break;
			}
			// transitions
			float alpha = 1.0f;
			if ((Element.Transition & HUD.Transition.Move) != 0)
			{
				double s = Element.TransitionState;
				x += Element.TransitionVector.X * s * s;
				y += Element.TransitionVector.Y * s * s;
			}
			if ((Element.Transition & HUD.Transition.Fade) != 0 || Element.Transition == HUD.Transition.None)
			{
				alpha = (float)(1.0 - Element.TransitionState);
			}
			// render
			if (alpha != 0.0f)
			{
				/* First, we need to fix position when notch descriptions are used, as this may otherwise overflow the bar
				 *
				 * NOTES: The default width allowance for each string is 48px, e.g. the P , B indicator and 2 figures when measured
				 *        in the Normal font size
				 *        When a larger font is used, it fails the 48px width allowance and will resize appropriately
				 *
				 *        The default handle render order is Reverser ==> Power ==> Brake ==> LocoBrake (if present)
				 *        This code does not properly support re-ordering this default at present.
				 *
				 */
				switch (Element.Subject)
				{
					case HUDSubject.Reverser:
						w = Math.Max(w, PlayerTrain.Handles.Reverser.MaxWidth);
						// NOTE: X-Pos will never need to be changed as this comes first
						break;
					case HUDSubject.Power:
						w = Math.Max(w, PlayerTrain.Handles.Power.MaxWidth);
						if (PlayerTrain.Handles.Reverser.MaxWidth > 48)
						{
							x += (PlayerTrain.Handles.Reverser.MaxWidth - 48);
						}
						break;
					case HUDSubject.Brake:
						w = Math.Max(w, PlayerTrain.Handles.Brake.MaxWidth);
						if (PlayerTrain.Handles.Reverser.MaxWidth > 48)
						{
							x += (PlayerTrain.Handles.Reverser.MaxWidth - 48);
						}
						if (PlayerTrain.Handles.Power.MaxWidth > 48)
						{
							x += (PlayerTrain.Handles.Power.MaxWidth - 48);
						}
						break;
					case HUDSubject.Single:
						w = Math.Max(Math.Max(w, PlayerTrain.Handles.Power.MaxWidth), PlayerTrain.Handles.Brake.MaxWidth);
						if (PlayerTrain.Handles.Reverser.MaxWidth > 48)
						{
							x += PlayerTrain.Handles.Reverser.MaxWidth - 48;
						}
						break;
					case HUDSubject.LocoBrake:
						w = Math.Max(w, PlayerTrain.Handles.LocoBrake.MaxWidth);
						if (PlayerTrain.Handles.Reverser.MaxWidth > 48)
						{
							x += PlayerTrain.Handles.Reverser.MaxWidth - 48;
						}
						if (PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
						{
							double mw = Math.Max(Math.Max(w, PlayerTrain.Handles.Power.MaxWidth), PlayerTrain.Handles.Brake.MaxWidth);
							if (mw > 48)
							{
								x += mw - 48;
							}
						}
						else
						{
							if (PlayerTrain.Handles.Power.MaxWidth > 48)
							{
								x += PlayerTrain.Handles.Power.MaxWidth - 48;
							}
							if (PlayerTrain.Handles.Brake.MaxWidth > 48)
							{
								x += PlayerTrain.Handles.Brake.MaxWidth - 48;
							}
						}
						break;

				}


				Texture backgroundTexture = Element.CenterMiddle.BackgroundTexture;
				Texture overlayTexture = Element.CenterMiddle.OverlayTexture;
				if (w > 100 && Element.CenterMiddle.WideBackgroundTexture != null && Element.CenterMiddle.WideOverlayTexture != null)
				{
					// use non-slanted texture if we're greater than 100px wide, as this otherwise looks dreadful
					backgroundTexture = Element.CenterMiddle.WideBackgroundTexture;
					overlayTexture = Element.CenterMiddle.WideOverlayTexture;
					w -= 48 * Interface.CurrentOptions.UserInterfaceScaleFactor;
				}

				// background
				if (backgroundTexture != null)
				{
					if (Program.CurrentHost.LoadTexture(ref backgroundTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						Color128 c = Element.BackgroundColor.CreateBackColor(sc, alpha);
						renderer.Rectangle.Draw(backgroundTexture, new Vector2(x, y), new Vector2(w, h), c);
					}
				}
				{ // text
					Vector2 size = Element.Font.MeasureString(t);
					double p = Math.Round(Element.TextAlignment.X < 0 ? x : Element.TextAlignment.X == 0 ? x + 0.5 * (w - size.X) : x + w - size.X);
					double q = Math.Round(Element.TextAlignment.Y < 0 ? y : Element.TextAlignment.Y == 0 ? y + 0.5 * (h - size.Y) : y + h - size.Y);
					p += Element.TextPosition.X;
					q += Element.TextPosition.Y;
					Color128 c = Element.TextColor.CreateTextColor(sc, alpha);
					Program.Renderer.OpenGlString.Draw(Element.Font, t, new Vector2(p, q), TextAlignment.TopLeft, c, Element.TextShadow);
				}

				// overlay
				if (overlayTexture != null)
				{
					if (Program.CurrentHost.LoadTexture(ref overlayTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						Color128 c = Element.OverlayColor.CreateBackColor(sc, alpha);
						renderer.Rectangle.Draw(overlayTexture, new Vector2(x, y), new Vector2(w, h), c);
					}
				}
			}
		}
	}
}
