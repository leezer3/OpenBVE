using System;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Textures;
using OpenBveApi.Interface;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
	internal static partial class Renderer
	{
		/// <summary>Renders all default HUD elements</summary>
		/// <param name="Element">The HUD element these are to be rendererd onto</param>
		/// <param name="TimeElapsed">The time elapsed</param>
		private static void RenderHUDElement(HUD.Element Element, double TimeElapsed)
		{
			TrainManager.TrainDoorState LeftDoors = TrainManager.GetDoorsState(TrainManager.PlayerTrain, true, false);
			TrainManager.TrainDoorState RightDoors = TrainManager.GetDoorsState(TrainManager.PlayerTrain, false, true);
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string Command = Element.Subject.ToLowerInvariant();
			// default
			double w, h;
			if (Element.CenterMiddle.BackgroundTexture != null)
			{
				if (Textures.LoadTexture(Element.CenterMiddle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
				{
					w = (double)Element.CenterMiddle.BackgroundTexture.Width;
					h = (double)Element.CenterMiddle.BackgroundTexture.Height;
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
			double x = Element.Alignment.X < 0 ? 0.0 : Element.Alignment.X == 0 ? 0.5 * (Screen.Width - w) : Screen.Width - w;
			double y = Element.Alignment.Y < 0 ? 0.0 : Element.Alignment.Y == 0 ? 0.5 * (Screen.Height - h) : Screen.Height - h;
			x += Element.Position.X;
			y += Element.Position.Y;
			// command
			const double speed = 1.0;
			MessageColor sc = MessageColor.None;
			string t;
			switch (Command)
			{
				case "reverser":
					if (TrainManager.PlayerTrain.Handles.Reverser.Driver < 0)
					{
						sc = MessageColor.Orange;
						if (TrainManager.PlayerTrain.ReverserDescriptions != null && TrainManager.PlayerTrain.ReverserDescriptions.Length > 2)
						{
							t = TrainManager.PlayerTrain.ReverserDescriptions[2];
						}
						else
						{
							t = Translations.QuickReferences.HandleBackward;
						}
					}
					else if (TrainManager.PlayerTrain.Handles.Reverser.Driver > 0)
					{
						sc = MessageColor.Blue;
						if (TrainManager.PlayerTrain.ReverserDescriptions != null && TrainManager.PlayerTrain.ReverserDescriptions.Length > 0)
						{
							t = TrainManager.PlayerTrain.ReverserDescriptions[0];
						}
						else
						{
							t = Translations.QuickReferences.HandleForward;
						}
					}
					else
					{
						sc = MessageColor.Gray;
						if (TrainManager.PlayerTrain.ReverserDescriptions != null && TrainManager.PlayerTrain.ReverserDescriptions.Length > 1)
						{
							t = TrainManager.PlayerTrain.ReverserDescriptions[1];
						}
						else
						{
							t = Translations.QuickReferences.HandleNeutral;
						}
					}
					Element.TransitionState = 0.0;
					break;
				case "power":
					if (TrainManager.PlayerTrain.Handles.SingleHandle)
					{
						return;
					}
					if (TrainManager.PlayerTrain.Handles.Power.Driver == 0)
					{
						sc = MessageColor.Gray;
						if (TrainManager.PlayerTrain.PowerNotchDescriptions != null && TrainManager.PlayerTrain.PowerNotchDescriptions.Length > 0)
						{
							t = TrainManager.PlayerTrain.PowerNotchDescriptions[0];
						}
						else
						{
							t = Translations.QuickReferences.HandlePowerNull;
						}
					}
					else
					{
						sc = MessageColor.Blue;
						if (TrainManager.PlayerTrain.PowerNotchDescriptions != null && TrainManager.PlayerTrain.Handles.Power.Driver < TrainManager.PlayerTrain.PowerNotchDescriptions.Length)
						{
							t = TrainManager.PlayerTrain.PowerNotchDescriptions[TrainManager.PlayerTrain.Handles.Power.Driver];
						}
						else
						{
							t = Translations.QuickReferences.HandlePower + TrainManager.PlayerTrain.Handles.Power.Driver.ToString(Culture);
						}
						
					}
					Element.TransitionState = 0.0;
					break;
				case "brake":
					if (TrainManager.PlayerTrain.Handles.SingleHandle)
					{
						return;
					}
					if (TrainManager.PlayerTrain.Handles.Brake is TrainManager.AirBrakeHandle)
					{
						if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
						{
							sc = MessageColor.Red;
							if (TrainManager.PlayerTrain.BrakeNotchDescriptions != null && TrainManager.PlayerTrain.BrakeNotchDescriptions.Length > 0)
							{
								t = TrainManager.PlayerTrain.BrakeNotchDescriptions[0];
							}
							else
							{
								t = Translations.QuickReferences.HandleEmergency;
							}
						}
						else if (TrainManager.PlayerTrain.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Release)
						{
							sc = MessageColor.Gray;
							if (TrainManager.PlayerTrain.BrakeNotchDescriptions != null && TrainManager.PlayerTrain.BrakeNotchDescriptions.Length > 1)
							{
								t = TrainManager.PlayerTrain.BrakeNotchDescriptions[1];
							}
							else
							{
								t = Translations.QuickReferences.HandleRelease;
							}
						}
						else if (TrainManager.PlayerTrain.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Lap)
						{
							sc = MessageColor.Blue;
							if (TrainManager.PlayerTrain.BrakeNotchDescriptions != null && TrainManager.PlayerTrain.BrakeNotchDescriptions.Length > 2)
							{
								t = TrainManager.PlayerTrain.BrakeNotchDescriptions[2];
							}
							else
							{
								t = Translations.QuickReferences.HandleLap;
							}
						}
						else
						{
							sc = MessageColor.Orange;
							if (TrainManager.PlayerTrain.BrakeNotchDescriptions != null && TrainManager.PlayerTrain.BrakeNotchDescriptions.Length > 3)
							{
								t = TrainManager.PlayerTrain.BrakeNotchDescriptions[3];
							}
							else
							{
								t = Translations.QuickReferences.HandleService;
							}
							
						}
					}
					else
					{
						if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
						{
							sc = MessageColor.Red;
							if (TrainManager.PlayerTrain.BrakeNotchDescriptions != null && TrainManager.PlayerTrain.BrakeNotchDescriptions.Length > 0)
							{
								t = TrainManager.PlayerTrain.BrakeNotchDescriptions[0];
							}
							else
							{
								t = Translations.QuickReferences.HandleEmergency;
							}
						}
						else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
						{
							sc = MessageColor.Green;
							if (TrainManager.PlayerTrain.BrakeNotchDescriptions != null && TrainManager.PlayerTrain.BrakeNotchDescriptions.Length > 2)
							{
								t = TrainManager.PlayerTrain.BrakeNotchDescriptions[2];
							}
							else
							{
								t = Translations.QuickReferences.HandleHoldBrake;
							}
						}
						else if (TrainManager.PlayerTrain.Handles.Brake.Driver == 0)
						{
							sc = MessageColor.Gray;
							if (TrainManager.PlayerTrain.BrakeNotchDescriptions != null && TrainManager.PlayerTrain.BrakeNotchDescriptions.Length > 1)
							{
								t = TrainManager.PlayerTrain.BrakeNotchDescriptions[1];
							}
							else
							{
								t = Translations.QuickReferences.HandleBrakeNull;
							}
						}
						else
						{
							sc = MessageColor.Orange;
							if (TrainManager.PlayerTrain.BrakeNotchDescriptions != null && ((TrainManager.PlayerTrain.Handles.HasHoldBrake && TrainManager.PlayerTrain.Handles.Brake.Driver + 2 < TrainManager.PlayerTrain.BrakeNotchDescriptions.Length) || (!TrainManager.PlayerTrain.Handles.HasHoldBrake && TrainManager.PlayerTrain.Handles.Brake.Driver + 1 < TrainManager.PlayerTrain.BrakeNotchDescriptions.Length)))
							{
								t = TrainManager.PlayerTrain.Handles.HasHoldBrake ? TrainManager.PlayerTrain.BrakeNotchDescriptions[TrainManager.PlayerTrain.Handles.Brake.Driver + 2] : TrainManager.PlayerTrain.BrakeNotchDescriptions[TrainManager.PlayerTrain.Handles.Brake.Driver + 1];
							}
							else
							{
								t = Translations.QuickReferences.HandleBrake + TrainManager.PlayerTrain.Handles.Brake.Driver.ToString(Culture);
							}
							
						}
					}
					Element.TransitionState = 0.0;
					break;
				case "locobrake":
					if (!TrainManager.PlayerTrain.Handles.HasLocoBrake)
					{
						return;
					}

					if (TrainManager.PlayerTrain.Handles.LocoBrake is TrainManager.LocoAirBrakeHandle)
					{
						if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)TrainManager.AirBrakeHandleState.Release)
						{
							sc = MessageColor.Gray;
							if (TrainManager.PlayerTrain.BrakeNotchDescriptions != null && TrainManager.PlayerTrain.BrakeNotchDescriptions.Length > 1)
							{
								t = TrainManager.PlayerTrain.BrakeNotchDescriptions[1];
							}
							else
							{
								t = Translations.QuickReferences.HandleRelease;
							}
						}
						else if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)TrainManager.AirBrakeHandleState.Lap)
						{
							sc = MessageColor.Blue;
							if (TrainManager.PlayerTrain.BrakeNotchDescriptions != null && TrainManager.PlayerTrain.BrakeNotchDescriptions.Length > 2)
							{
								t = TrainManager.PlayerTrain.BrakeNotchDescriptions[2];
							}
							else
							{
								t = Translations.QuickReferences.HandleLap;
							}
						}
						else
						{
							sc = MessageColor.Orange;
							if (TrainManager.PlayerTrain.BrakeNotchDescriptions != null && TrainManager.PlayerTrain.BrakeNotchDescriptions.Length > 3)
							{
								t = TrainManager.PlayerTrain.BrakeNotchDescriptions[3];
							}
							else
							{
								t = Translations.QuickReferences.HandleService;
							}
							
						}
					}
					else
					{
						if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == 0)
						{
							sc = MessageColor.Gray;
							if (TrainManager.PlayerTrain.LocoBrakeNotchDescriptions != null && TrainManager.PlayerTrain.LocoBrakeNotchDescriptions.Length > 1)
							{
								t = TrainManager.PlayerTrain.LocoBrakeNotchDescriptions[1];
							}
							else
							{
								t = Translations.QuickReferences.HandleBrakeNull;
							}
						}
						else
						{
							sc = MessageColor.Orange;
							if (TrainManager.PlayerTrain.LocoBrakeNotchDescriptions != null && TrainManager.PlayerTrain.Handles.LocoBrake.Driver < TrainManager.PlayerTrain.LocoBrakeNotchDescriptions.Length)
							{
								t = TrainManager.PlayerTrain.LocoBrakeNotchDescriptions[TrainManager.PlayerTrain.Handles.LocoBrake.Driver];
							}
							else
							{
								t = Translations.QuickReferences.HandleLocoBrake + TrainManager.PlayerTrain.Handles.LocoBrake.Driver.ToString(Culture);
							}
							
						}
					}
					Element.TransitionState = 0.0;
					break;
				case "single":
					if (!TrainManager.PlayerTrain.Handles.SingleHandle)
					{
						return;
					}
					if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
					{
						sc = MessageColor.Red;
						if (TrainManager.PlayerTrain.BrakeNotchDescriptions != null && TrainManager.PlayerTrain.BrakeNotchDescriptions.Length > 0)
						{
							t = TrainManager.PlayerTrain.BrakeNotchDescriptions[0];
						}
						else
						{
							t = Translations.QuickReferences.HandleEmergency;
						}
					}
					else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
					{
						sc = MessageColor.Green;
						if (TrainManager.PlayerTrain.BrakeNotchDescriptions != null && TrainManager.PlayerTrain.BrakeNotchDescriptions.Length > 1)
						{
							t = TrainManager.PlayerTrain.BrakeNotchDescriptions[1];
						}
						else
						{
							t = Translations.QuickReferences.HandleHoldBrake;
						}
					}
					else if (TrainManager.PlayerTrain.Handles.Brake.Driver > 0)
					{
						sc = MessageColor.Orange;
						if (TrainManager.PlayerTrain.BrakeNotchDescriptions != null && TrainManager.PlayerTrain.Handles.Brake.Driver + 3 < TrainManager.PlayerTrain.BrakeNotchDescriptions.Length)
						{
							t = TrainManager.PlayerTrain.BrakeNotchDescriptions[TrainManager.PlayerTrain.Handles.Brake.Driver + 3];
						}
						else
						{
							t = Translations.QuickReferences.HandleBrake + TrainManager.PlayerTrain.Handles.Brake.Driver.ToString(Culture);
						}
					}
					else if (TrainManager.PlayerTrain.Handles.Power.Driver > 0)
					{
						sc = MessageColor.Blue;
						if (TrainManager.PlayerTrain.PowerNotchDescriptions != null && TrainManager.PlayerTrain.Handles.Power.Driver < TrainManager.PlayerTrain.PowerNotchDescriptions.Length)
						{
							t = TrainManager.PlayerTrain.PowerNotchDescriptions[TrainManager.PlayerTrain.Handles.Power.Driver];
						}
						else
						{
							t = Translations.QuickReferences.HandlePower + TrainManager.PlayerTrain.Handles.Power.Driver.ToString(Culture);
						}
					}
					else
					{
						sc = MessageColor.Gray;
						if (TrainManager.PlayerTrain.PowerNotchDescriptions != null && TrainManager.PlayerTrain.PowerNotchDescriptions.Length > 0)
						{
							t = TrainManager.PlayerTrain.PowerNotchDescriptions[0];
						}
						else
						{
							t = Translations.QuickReferences.HandlePowerNull;
						}
					}
					Element.TransitionState = 0.0;
					break;
				case "doorsleft":
				case "doorsright":
				{
					if ((LeftDoors & TrainManager.TrainDoorState.AllClosed) == 0 | (RightDoors & TrainManager.TrainDoorState.AllClosed) == 0)
					{
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					}
					TrainManager.TrainDoorState Doors = Command == "doorsleft" ? LeftDoors : RightDoors;
					if ((Doors & TrainManager.TrainDoorState.Mixed) != 0)
					{
						sc = MessageColor.Orange;
					}
					else if ((Doors & TrainManager.TrainDoorState.AllClosed) != 0)
					{
						sc = MessageColor.Gray;
					}
					else if (TrainManager.PlayerTrain.Specs.DoorCloseMode == TrainManager.DoorMode.Manual)
					{
						sc = MessageColor.Green;
					}
					else
					{
						sc = MessageColor.Blue;
					}
					t = Command == "doorsleft" ? Translations.QuickReferences.DoorsLeft : Translations.QuickReferences.DoorsRight;
				} break;
				case "stopleft":
				case "stopright":
				case "stopnone":
				{
					int s = TrainManager.PlayerTrain.Station;
					if (s >= 0 && Game.PlayerStopsAtStation(s) && Interface.CurrentOptions.GameMode != Interface.GameMode.Expert)
					{
						bool cond;
						if (Command == "stopleft")
						{
							cond = Game.Stations[s].OpenLeftDoors;
						}
						else if (Command == "stopright")
						{
							cond = Game.Stations[s].OpenRightDoors;
						}
						else
						{
							cond = !Game.Stations[s].OpenLeftDoors & !Game.Stations[s].OpenRightDoors;
						}
						if (TrainManager.PlayerTrain.StationState == TrainManager.TrainStopState.Pending & cond)
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
				case "stoplefttick":
				case "stoprighttick":
				case "stopnonetick":
				{
					int s = TrainManager.PlayerTrain.Station;
					if (s >= 0 && Game.PlayerStopsAtStation(s) && Interface.CurrentOptions.GameMode != Interface.GameMode.Expert)
					{
						int c = Game.Stations[s].GetStopIndex(TrainManager.PlayerTrain.Cars.Length);
						if (c >= 0)
						{
							bool cond;
							if (Command == "stoplefttick")
							{
								cond = Game.Stations[s].OpenLeftDoors;
							}
							else if (Command == "stoprighttick")
							{
								cond = Game.Stations[s].OpenRightDoors;
							}
							else
							{
								cond = !Game.Stations[s].OpenLeftDoors & !Game.Stations[s].OpenRightDoors;
							}
							if (TrainManager.PlayerTrain.StationState == TrainManager.TrainStopState.Pending & cond)
							{
								Element.TransitionState -= speed * TimeElapsed;
								if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
							}
							else
							{
								Element.TransitionState += speed * TimeElapsed;
								if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
							}
							double d = TrainManager.PlayerTrain.StationDistanceToStopPoint;
							double r;
							if (d > 0.0)
							{
								r = d / Game.Stations[s].Stops[c].BackwardTolerance;
							}
							else
							{
								r = d / Game.Stations[s].Stops[c].ForwardTolerance;
							}
							if (r < -1.0) r = -1.0;
							if (r > 1.0) r = 1.0;
							y -= r * (double)Element.Value1;
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
				case "clock":
				{
					int hours = (int)Math.Floor(Game.SecondsSinceMidnight);
					int seconds = hours % 60; hours /= 60;
					int minutes = hours % 60; hours /= 60;
					hours %= 24;
					t = hours.ToString(Culture).PadLeft(2, '0') + ":" + minutes.ToString(Culture).PadLeft(2, '0') + ":" + seconds.ToString(Culture).PadLeft(2, '0');
					if (OptionClock)
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
				case "gradient":
					if (OptionGradient == GradientDisplayMode.Percentage)
					{
						if (World.CameraTrackFollower.Pitch != 0)
						{
							double pc = World.CameraTrackFollower.Pitch;
							t = Math.Abs(pc).ToString("0.00", Culture) + "%" + (Math.Abs(pc) == pc ? " ↗" : " ↘");
						}
						else
						{
							t = "Level";
						}
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else if (OptionGradient == GradientDisplayMode.UnitOfChange)
					{
						if (World.CameraTrackFollower.Pitch != 0)
						{
							double gr = 1000 / World.CameraTrackFollower.Pitch;
							t = "1 in " + Math.Abs(gr).ToString("0", Culture) + (Math.Abs(gr) == gr ? " ↗" : " ↘");
						}
						else
						{
							t = "Level";
						}
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else if (OptionGradient == GradientDisplayMode.Permil)
					{
						if (World.CameraTrackFollower.Pitch != 0)
						{
							double pm = World.CameraTrackFollower.Pitch;
							t = Math.Abs(pm).ToString("0.00", Culture) + "‰" + (Math.Abs(pm) == pm ? " ↗" : " ↘");
						}
						else
						{
							t = "Level";
						}
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						if (World.CameraTrackFollower.Pitch != 0)
						{
							double gr = 1000 / World.CameraTrackFollower.Pitch;
							t = "1 in " + Math.Abs(gr).ToString("0", Culture) + (Math.Abs(gr) == gr ? " ↗" : " ↘");
						}
						else
						{
							t = "Level";
						}
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					} break;
				case "speed":
					if (OptionSpeed == SpeedDisplayMode.Kmph)
					{
						double kmph = Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) * 3.6;
						t = kmph.ToString("0.00", Culture) + " km/h";
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else if (OptionSpeed == SpeedDisplayMode.Mph)
					{
						double mph = Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) * 2.2369362920544;
						t = mph.ToString("0.00", Culture) + " mph";
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						double mph = Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) * 2.2369362920544;
						t = mph.ToString("0.00", Culture) + " mph";
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					} break;
				case "dist_next_station":
					int i;
					if (TrainManager.PlayerTrain.Station >= 0 && TrainManager.PlayerTrain.StationState != TrainManager.TrainStopState.Completed)
					{
					i = TrainManager.PlayerTrain.LastStation;
					}
					else
					{
						i = TrainManager.PlayerTrain.LastStation + 1;
					}
					if (i > Game.Stations.Length - 1)
					{
						i = TrainManager.PlayerTrain.LastStation;
					}
					int n = Game.Stations[i].GetStopIndex(TrainManager.PlayerTrain.Cars.Length);
					double p0 = TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition - TrainManager.PlayerTrain.Cars[0].FrontAxle.Position + 0.5 * TrainManager.PlayerTrain.Cars[0].Length;
					double p1;
					if (Game.Stations[i].Stops.Length > 0)
					{
						p1 = Game.Stations[i].Stops[n].TrackPosition;
					}
					else
					{
						p1 = Game.Stations[i].DefaultTrackPosition;
					}
					double m = p1 - p0;
					if (OptionDistanceToNextStation == DistanceToNextStationDisplayMode.Km)
					{
						if (Game.PlayerStopsAtStation(i))
						{
							t = "Stop: ";
							if (Math.Abs(m) <= 10.0)
							{
								t += m.ToString("0.00", Culture) + " m";
							}
							else
							{
								m /= 1000.0;
								t += m.ToString("0.000", Culture) + " km";
							}
						}
						else
						{
							m /= 1000.0;
							t = "Pass: " + m.ToString("0.000", Culture) + " km";
						}
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else if (OptionDistanceToNextStation == DistanceToNextStationDisplayMode.Mile)
					{
						m /= 1609.34;
						if (Game.PlayerStopsAtStation(i))
						{
							t = "Stop: ";
						}
						else
						{
							t = "Pass: ";
						}
						t += m.ToString("0.0000", Culture) + " miles";
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						m /= 1609.34;
						if (Game.PlayerStopsAtStation(i))
						{
							t = "Stop: ";
						}
						else
						{
							t = "Pass: ";
						}
						t += m.ToString("0.0000", Culture) + " miles";
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					} break;
				case "fps":
					int fps = (int)Math.Round(Game.InfoFrameRate);
					t = fps.ToString(Culture) + " fps";
					if (OptionFrameRates)
					{
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					} break;
				case "ai":
					t = "A.I.";
					if (TrainManager.PlayerTrain.AI != null)
					{
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					} break;
				case "score":
					if (Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade)
					{
						t = Game.CurrentScore.CurrentValue.ToString(Culture) + " / " + Game.CurrentScore.Maximum.ToString(Culture);
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
					}
					else
					{
						Element.TransitionState = 1.0;
						t = "";
					} break;
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
			if ((Element.Transition & HUD.Transition.Fade) != 0)
			{
				alpha = (float)(1.0 - Element.TransitionState);
			}
			else if (Element.Transition == HUD.Transition.None)
			{
				alpha = (float)(1.0 - Element.TransitionState);
			}
			// render
			if (alpha != 0.0f)
			{
				// background
				if (Element.Subject == "reverser")
				{
					w = Math.Max(w, TrainManager.PlayerTrain.MaxReverserWidth);
					//X-Pos doesn't need to be changed
				}
				if (Element.Subject == "power")
				{
					w = Math.Max(w, TrainManager.PlayerTrain.MaxPowerNotchWidth);
					if (TrainManager.PlayerTrain.MaxReverserWidth > 48)
					{
						x += (TrainManager.PlayerTrain.MaxReverserWidth - 48);
					}
				}
				if (Element.Subject == "brake")
				{
					w = Math.Max(w, TrainManager.PlayerTrain.MaxBrakeNotchWidth);
					if (TrainManager.PlayerTrain.MaxReverserWidth > 48)
					{
						x += (TrainManager.PlayerTrain.MaxReverserWidth - 48);
					}
					if (TrainManager.PlayerTrain.MaxPowerNotchWidth > 48)
					{
						x += (TrainManager.PlayerTrain.MaxPowerNotchWidth - 48);
					}
				}
				if (Element.Subject == "single")
				{
					w = Math.Max(Math.Max(w, TrainManager.PlayerTrain.MaxPowerNotchWidth), TrainManager.PlayerTrain.MaxBrakeNotchWidth);
					if (TrainManager.PlayerTrain.MaxReverserWidth > 48)
					{
						x += (TrainManager.PlayerTrain.MaxReverserWidth - 48);
					}
				}
				if (Element.CenterMiddle.BackgroundTexture != null)
				{
					if (Textures.LoadTexture(Element.CenterMiddle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						float r, g, b, a;
						CreateBackColor(Element.BackgroundColor, sc, out r, out g, out b, out a);
						GL.Color4(r, g, b, a * alpha);
						RenderOverlayTexture(Element.CenterMiddle.BackgroundTexture, x, y, x + w, y + h);
					}
				}
				{ // text
					System.Drawing.Size size = MeasureString(Element.Font, t);
					float u = size.Width;
					float v = size.Height;
					double p = Math.Round(Element.TextAlignment.X < 0 ? x : Element.TextAlignment.X == 0 ? x + 0.5 * (w - u) : x + w - u);
					double q = Math.Round(Element.TextAlignment.Y < 0 ? y : Element.TextAlignment.Y == 0 ? y + 0.5 * (h - v) : y + h - v);
					p += Element.TextPosition.X;
					q += Element.TextPosition.Y;
					float r, g, b, a;
					CreateTextColor(Element.TextColor, sc, out r, out g, out b, out a);
					DrawString(Element.Font, t, new System.Drawing.Point((int)p, (int)q), TextAlignment.TopLeft, new Color128(r, g, b, a * alpha), Element.TextShadow);
				}
				// overlay
				if (Element.CenterMiddle.OverlayTexture != null)
				{
					if (Textures.LoadTexture(Element.CenterMiddle.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						float r, g, b, a;
						CreateBackColor(Element.OverlayColor, sc, out r, out g, out b, out a);
						GL.Color4(r, g, b, a * alpha);
						RenderOverlayTexture(Element.CenterMiddle.OverlayTexture, x, y, x + w, y + h);
					}
				}
			}
		}
	}
}
