using System;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Textures;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Sounds;
using OpenBveApi.Trains;
using SoundManager;
using TrainManager.Car;
using TrainManager.Handles;

namespace OpenBve.Graphics.Renderers
{
	internal partial class Overlays
	{
		/// <summary>Renders all default HUD elements</summary>
		/// <param name="Element">The HUD element these are to be rendererd onto</param>
		/// <param name="TimeElapsed">The time elapsed</param>
		private void RenderHUDElement(HUD.Element Element, double TimeElapsed)
		{
			if (TrainManager.PlayerTrain == null)
			{
				return;
			}
			TrainDoorState LeftDoors = TrainManager.PlayerTrain.GetDoorsState(true, false);
			TrainDoorState RightDoors = TrainManager.PlayerTrain.GetDoorsState(false, true);
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string Command = Element.Subject.ToLowerInvariant();
			// default
			double w, h;
			if (Element.CenterMiddle.BackgroundTexture != null)
			{
				if (Program.CurrentHost.LoadTexture(ref Element.CenterMiddle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
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

			int stationIndex;
			if (TrainManager.PlayerTrain.Station >= 0 && TrainManager.PlayerTrain.StationState != TrainStopState.Completed)
			{
				stationIndex = TrainManager.PlayerTrain.LastStation;
			}
			else
			{
				stationIndex = TrainManager.PlayerTrain.LastStation + 1;
			}
			if (stationIndex > Program.CurrentRoute.Stations.Length - 1)
			{
				stationIndex = TrainManager.PlayerTrain.LastStation;
			}

			double x = Element.Alignment.X < 0 ? 0.0 : Element.Alignment.X == 0 ? 0.5 * (Program.Renderer.Screen.Width - w) : Program.Renderer.Screen.Width - w;
			double y = Element.Alignment.Y < 0 ? 0.0 : Element.Alignment.Y == 0 ? 0.5 * (Program.Renderer.Screen.Height - h) : Program.Renderer.Screen.Height - h;
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
						if (TrainManager.PlayerTrain.Handles.Reverser.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Reverser.NotchDescriptions.Length > 2)
						{
							t = TrainManager.PlayerTrain.Handles.Reverser.NotchDescriptions[2];
						}
						else
						{
							t = Translations.QuickReferences.HandleBackward;
						}
					}
					else if (TrainManager.PlayerTrain.Handles.Reverser.Driver > 0)
					{
						sc = MessageColor.Blue;
						if (TrainManager.PlayerTrain.Handles.Reverser.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Reverser.NotchDescriptions.Length > 0)
						{
							t = TrainManager.PlayerTrain.Handles.Reverser.NotchDescriptions[0];
						}
						else
						{
							t = Translations.QuickReferences.HandleForward;
						}
					}
					else
					{
						sc = MessageColor.Gray;
						if (TrainManager.PlayerTrain.Handles.Reverser.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Reverser.NotchDescriptions.Length > 1)
						{
							t = TrainManager.PlayerTrain.Handles.Reverser.NotchDescriptions[1];
						}
						else
						{
							t = Translations.QuickReferences.HandleNeutral;
						}
					}
					Element.TransitionState = 0.0;
					break;
				case "power":
					if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
					{
						return;
					}
					if (TrainManager.PlayerTrain.Handles.Power.Driver == 0)
					{
						sc = MessageColor.Gray;
						if (TrainManager.PlayerTrain.Handles.Power.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Power.NotchDescriptions.Length > 0)
						{
							t = TrainManager.PlayerTrain.Handles.Power.NotchDescriptions[0];
						}
						else
						{
							t = Translations.QuickReferences.HandlePowerNull;
						}
					}
					else
					{
						sc = MessageColor.Blue;
						if (TrainManager.PlayerTrain.Handles.Power.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Power.Driver < TrainManager.PlayerTrain.Handles.Power.NotchDescriptions.Length)
						{
							t = TrainManager.PlayerTrain.Handles.Power.NotchDescriptions[TrainManager.PlayerTrain.Handles.Power.Driver];
						}
						else
						{
							t = Translations.QuickReferences.HandlePower + TrainManager.PlayerTrain.Handles.Power.Driver.ToString(Culture);
						}
						
					}
					Element.TransitionState = 0.0;
					break;
				case "brake":
					if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
					{
						return;
					}
					if (TrainManager.PlayerTrain.Handles.Brake is AirBrakeHandle)
					{
						if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
						{
							sc = MessageColor.Red;
							if (TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions.Length > 0)
							{
								t = TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions[0];
							}
							else
							{
								t = Translations.QuickReferences.HandleEmergency;
							}
						}
						else if (TrainManager.PlayerTrain.Handles.Brake.Driver == (int)AirBrakeHandleState.Release)
						{
							sc = MessageColor.Gray;
							if (TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions.Length > 1)
							{
								t = TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions[1];
							}
							else
							{
								t = Translations.QuickReferences.HandleRelease;
							}
						}
						else if (TrainManager.PlayerTrain.Handles.Brake.Driver == (int)AirBrakeHandleState.Lap)
						{
							sc = MessageColor.Blue;
							if (TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions.Length > 2)
							{
								t = TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions[2];
							}
							else
							{
								t = Translations.QuickReferences.HandleLap;
							}
						}
						else
						{
							sc = MessageColor.Orange;
							if (TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions.Length > 3)
							{
								t = TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions[3];
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
							if (TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions.Length > 0)
							{
								t = TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions[0];
							}
							else
							{
								t = Translations.QuickReferences.HandleEmergency;
							}
						}
						else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
						{
							sc = MessageColor.Green;
							if (TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions.Length > 2)
							{
								t = TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions[2];
							}
							else
							{
								t = Translations.QuickReferences.HandleHoldBrake;
							}
						}
						else if (TrainManager.PlayerTrain.Handles.Brake.Driver == 0)
						{
							sc = MessageColor.Gray;
							if (TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions.Length > 1)
							{
								t = TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions[1];
							}
							else
							{
								t = Translations.QuickReferences.HandleBrakeNull;
							}
						}
						else
						{
							sc = MessageColor.Orange;
							if (TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions != null && ((TrainManager.PlayerTrain.Handles.HasHoldBrake && TrainManager.PlayerTrain.Handles.Brake.Driver + 2 < TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions.Length) || (!TrainManager.PlayerTrain.Handles.HasHoldBrake && TrainManager.PlayerTrain.Handles.Brake.Driver + 1 < TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions.Length)))
							{
								t = TrainManager.PlayerTrain.Handles.HasHoldBrake ? TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions[TrainManager.PlayerTrain.Handles.Brake.Driver + 2] : TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions[TrainManager.PlayerTrain.Handles.Brake.Driver + 1];
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

					if (TrainManager.PlayerTrain.Handles.LocoBrake is LocoAirBrakeHandle)
					{
						if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)AirBrakeHandleState.Release)
						{
							sc = MessageColor.Gray;
							if (TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions.Length > 1)
							{
								t = TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions[1];
							}
							else
							{
								t = Translations.QuickReferences.HandleRelease;
							}
						}
						else if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)AirBrakeHandleState.Lap)
						{
							sc = MessageColor.Blue;
							if (TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions.Length > 2)
							{
								t = TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions[2];
							}
							else
							{
								t = Translations.QuickReferences.HandleLap;
							}
						}
						else
						{
							sc = MessageColor.Orange;
							if (TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions.Length > 3)
							{
								t = TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions[3];
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
							if (TrainManager.PlayerTrain.Handles.LocoBrake.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.LocoBrake.NotchDescriptions.Length > 1)
							{
								t = TrainManager.PlayerTrain.Handles.LocoBrake.NotchDescriptions[1];
							}
							else
							{
								t = Translations.QuickReferences.HandleBrakeNull;
							}
						}
						else
						{
							sc = MessageColor.Orange;
							if (TrainManager.PlayerTrain.Handles.LocoBrake.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.LocoBrake.Driver < TrainManager.PlayerTrain.Handles.LocoBrake.NotchDescriptions.Length)
							{
								t = TrainManager.PlayerTrain.Handles.LocoBrake.NotchDescriptions[TrainManager.PlayerTrain.Handles.LocoBrake.Driver];
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
					if (TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle)
					{
						return;
					}
					if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
					{
						sc = MessageColor.Red;
						if (TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions.Length > 0)
						{
							t = TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions[0];
						}
						else
						{
							t = Translations.QuickReferences.HandleEmergency;
						}
					}
					else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
					{
						sc = MessageColor.Green;
						if (TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions.Length > 1)
						{
							t = TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions[1];
						}
						else
						{
							t = Translations.QuickReferences.HandleHoldBrake;
						}
					}
					else if (TrainManager.PlayerTrain.Handles.Brake.Driver > 0)
					{
						sc = MessageColor.Orange;
						if (TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Brake.Driver + 3 < TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions.Length)
						{
							t = TrainManager.PlayerTrain.Handles.Brake.NotchDescriptions[TrainManager.PlayerTrain.Handles.Brake.Driver + 3];
						}
						else
						{
							t = Translations.QuickReferences.HandleBrake + TrainManager.PlayerTrain.Handles.Brake.Driver.ToString(Culture);
						}
					}
					else if (TrainManager.PlayerTrain.Handles.Power.Driver > 0)
					{
						sc = MessageColor.Blue;
						if (TrainManager.PlayerTrain.Handles.Power.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Power.Driver < TrainManager.PlayerTrain.Handles.Power.NotchDescriptions.Length)
						{
							t = TrainManager.PlayerTrain.Handles.Power.NotchDescriptions[TrainManager.PlayerTrain.Handles.Power.Driver];
						}
						else
						{
							t = Translations.QuickReferences.HandlePower + TrainManager.PlayerTrain.Handles.Power.Driver.ToString(Culture);
						}
					}
					else
					{
						sc = MessageColor.Gray;
						if (TrainManager.PlayerTrain.Handles.Power.NotchDescriptions != null && TrainManager.PlayerTrain.Handles.Power.NotchDescriptions.Length > 0)
						{
							t = TrainManager.PlayerTrain.Handles.Power.NotchDescriptions[0];
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
					TrainDoorState Doors = Command == "doorsleft" ? LeftDoors : RightDoors;
					if ((Doors & TrainDoorState.Mixed) != 0)
					{
						sc = MessageColor.Orange;
					}
					else if ((Doors & TrainDoorState.AllClosed) != 0)
					{
						sc = MessageColor.Gray;
					}
					else if (TrainManager.PlayerTrain.Specs.DoorCloseMode == DoorMode.Manual)
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
					if (s >= 0 && Program.CurrentRoute.Stations[s].PlayerStops() && Interface.CurrentOptions.GameMode != GameMode.Expert)
					{
						bool cond;
						if (Command == "stopleft")
						{
							cond = Program.CurrentRoute.Stations[s].OpenLeftDoors;
						}
						else if (Command == "stopright")
						{
							cond = Program.CurrentRoute.Stations[s].OpenRightDoors;
						}
						else
						{
							cond = !Program.CurrentRoute.Stations[s].OpenLeftDoors & !Program.CurrentRoute.Stations[s].OpenRightDoors;
						}
						if (TrainManager.PlayerTrain.StationState == TrainStopState.Pending & cond)
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
					if (s >= 0 && Program.CurrentRoute.Stations[s].PlayerStops() && Interface.CurrentOptions.GameMode != GameMode.Expert && !Program.CurrentRoute.Stations[s].Dummy)
					{
						int c = Program.CurrentRoute.Stations[s].GetStopIndex(TrainManager.PlayerTrain.Cars.Length);
						if (c >= 0)
						{
							bool cond;
							switch (Command)
							{
								case "stoplefttick":
									cond = Program.CurrentRoute.Stations[s].OpenLeftDoors;
									break;
								case "stoprighttick":
									cond = Program.CurrentRoute.Stations[s].OpenRightDoors;
									break;
								default:
									cond = !Program.CurrentRoute.Stations[s].OpenLeftDoors & !Program.CurrentRoute.Stations[s].OpenRightDoors;
									break;
							}
							if (TrainManager.PlayerTrain.StationState == TrainStopState.Pending & cond)
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
								r = d / Program.CurrentRoute.Stations[s].Stops[c].BackwardTolerance;
							}
							else
							{
								r = d / Program.CurrentRoute.Stations[s].Stops[c].ForwardTolerance;
							}
							if (r < -1.0) r = -1.0;
							if (r > 1.0) r = 1.0;
							y -= r * (double)Element.Value1;
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


								if (TrainManager.PlayerTrain.StationState != TrainStopState.Pending || beepSpeed == 0)
								{
									// We're stopped or beep is not required
									if (HUD.stationAdjustBeepSource != null)
									{
										HUD.stationAdjustBeepSource.Stop();
									}
								}
								else
								{
									if (Element.TransitionState == 0.0)
									{
										if (HUD.stationAdjustBeepSource != null)
										{
											HUD.stationAdjustBeepSource.Stop();
										}
									}
									else
									{
										if (HUD.stationAdjustBeepSource != null && HUD.stationAdjustBeepSource.IsPlaying())
										{
											HUD.stationAdjustBeepSource.Volume = beepSpeed * 0.25;
											HUD.stationAdjustBeepSource.Position = Program.Renderer.Camera.AbsolutePosition;
										}
										else
										{
											HUD.stationAdjustBeepSource = (SoundSource)Program.CurrentHost.PlaySound((SoundHandle)HUD.stationAdjustBeep, 2.0, beepSpeed * 0.25, Program.Renderer.Camera.AbsolutePosition, null, true);
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
				case "clock":
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
				case "gradient":
					if (renderer.OptionGradient == NewRenderer.GradientDisplayMode.Percentage)
					{
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
					}
					else if (renderer.OptionGradient == NewRenderer.GradientDisplayMode.UnitOfChange)
					{
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
					}
					else if (renderer.OptionGradient == NewRenderer.GradientDisplayMode.Permil)
					{
						if (Program.Renderer.CameraTrackFollower.Pitch != 0)
						{
							double pm = Program.Renderer.CameraTrackFollower.Pitch;
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
					} break;
				case "speed":
					if (renderer.OptionSpeed == NewRenderer.SpeedDisplayMode.Kmph)
					{
						double kmph = Math.Abs(TrainManager.PlayerTrain.CurrentSpeed) * 3.6;
						t = kmph.ToString("0.00", Culture) + " km/h";
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else if (renderer.OptionSpeed == NewRenderer.SpeedDisplayMode.Mph)
					{
						double mph = Math.Abs(TrainManager.PlayerTrain.CurrentSpeed) * 2.2369362920544;
						t = mph.ToString("0.00", Culture) + " mph";
						Element.TransitionState -= speed * TimeElapsed;
						if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
					}
					else
					{
						double mph = Math.Abs(TrainManager.PlayerTrain.CurrentSpeed) * 2.2369362920544;
						t = mph.ToString("0.00", Culture) + " mph";
						Element.TransitionState += speed * TimeElapsed;
						if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
					} break;
				case "dist_next_station":
					if (!Program.CurrentRoute.Stations[stationIndex].PlayerStops())
					{
						int n = Program.CurrentRoute.Stations[stationIndex].GetStopIndex(TrainManager.PlayerTrain.NumberOfCars);
						double p0 = TrainManager.PlayerTrain.FrontCarTrackPosition();
						double p1 = Program.CurrentRoute.Stations[stationIndex].Stops.Length > 0 ? Program.CurrentRoute.Stations[stationIndex].Stops[n].TrackPosition : Program.CurrentRoute.Stations[stationIndex].DefaultTrackPosition;
						double m = p1 - p0;
						if (m < 0)
						{
							m = 0.0; //Don't display negative numbers when passing (stop zone goes beyond the absolute station limit)
						}
						if (renderer.OptionDistanceToNextStation == NewRenderer.DistanceToNextStationDisplayMode.Km)
						{
							m /= 1000.0;
							t = "Pass: " + m.ToString("0.000", Culture) + " km";
							Element.TransitionState -= speed * TimeElapsed;
							if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
						}
						else if (renderer.OptionDistanceToNextStation == NewRenderer.DistanceToNextStationDisplayMode.Mile)
						{
							m /= 1609.34;
							t = "Pass: " + m.ToString("0.0000", Culture) + " miles";
							Element.TransitionState -= speed * TimeElapsed;
							if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
						}
						else
						{
							m /= 1609.34;
							t = "Pass: " + m.ToString("0.0000", Culture) + " miles";
							Element.TransitionState += speed * TimeElapsed;
							if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
						}
					}
					else
					{
						t = String.Empty;
					}
					break;
				case "dist_next_station2":
					if (!Program.CurrentRoute.Stations[stationIndex].PlayerStops())
					{
						
						double p0 = TrainManager.PlayerTrain.FrontCarTrackPosition();
						double p1 = 0.0;
						for (int i = stationIndex; i < Program.CurrentRoute.Stations.Length; i++)
						{
							if (Program.CurrentRoute.Stations[i].PlayerStops())
							{
								int n = Program.CurrentRoute.Stations[i].GetStopIndex(TrainManager.PlayerTrain.NumberOfCars);
								p1 = Program.CurrentRoute.Stations[i].Stops.Length > 0 ? Program.CurrentRoute.Stations[i].Stops[n].TrackPosition : Program.CurrentRoute.Stations[i].DefaultTrackPosition;
								break;
							}
						}
						
						double m = p1 - p0;
						if (renderer.OptionDistanceToNextStation == NewRenderer.DistanceToNextStationDisplayMode.Km)
						{
							m /= 1000.0;
							t = "Next Stop: " + m.ToString("0.000", Culture) + " km";
							Element.TransitionState -= speed * TimeElapsed;
							if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
						}
						else if (renderer.OptionDistanceToNextStation == NewRenderer.DistanceToNextStationDisplayMode.Mile)
						{
							m /= 1609.34;
							t = "Next Stop: " + m.ToString("0.0000", Culture) + " miles";
							Element.TransitionState -= speed * TimeElapsed;
							if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
						}
						else
						{
							m /= 1609.34;
							t = "Next Stop: " + m.ToString("0.0000", Culture) + " miles";
							Element.TransitionState += speed * TimeElapsed;
							if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
						}
					}
					else
					{
						int n = Program.CurrentRoute.Stations[stationIndex].GetStopIndex(TrainManager.PlayerTrain.NumberOfCars);
						double p0 = TrainManager.PlayerTrain.FrontCarTrackPosition();
						double p1 = Program.CurrentRoute.Stations[stationIndex].Stops.Length > 0 ? Program.CurrentRoute.Stations[stationIndex].Stops[n].TrackPosition : Program.CurrentRoute.Stations[stationIndex].DefaultTrackPosition;
						double m = p1 - p0;
						if (renderer.OptionDistanceToNextStation == NewRenderer.DistanceToNextStationDisplayMode.Km)
						{
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
						}
						else if (renderer.OptionDistanceToNextStation == NewRenderer.DistanceToNextStationDisplayMode.Mile)
						{
							m /= 1609.34;
							t = "Stop: " + m.ToString("0.0000", Culture) + " miles";
							Element.TransitionState -= speed * TimeElapsed;
							if (Element.TransitionState < 0.0) Element.TransitionState = 0.0;
						}
						else
						{
							m /= 1609.34;
							t = "Stop: " + m.ToString("0.0000", Culture) + " miles";
							Element.TransitionState += speed * TimeElapsed;
							if (Element.TransitionState > 1.0) Element.TransitionState = 1.0;
						}
					}
					break;
				case "fps":
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
					if (Interface.CurrentOptions.GameMode == GameMode.Arcade)
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
					w = Math.Max(w, TrainManager.PlayerTrain.Handles.Reverser.MaxWidth);
					//X-Pos doesn't need to be changed
				}
				if (Element.Subject == "power")
				{
					w = Math.Max(w, TrainManager.PlayerTrain.Handles.Power.MaxWidth);
					if (TrainManager.PlayerTrain.Handles.Reverser.MaxWidth > 48)
					{
						x += (TrainManager.PlayerTrain.Handles.Reverser.MaxWidth - 48);
					}
				}
				if (Element.Subject == "brake")
				{
					w = Math.Max(w, TrainManager.PlayerTrain.Handles.Brake.MaxWidth);
					if (TrainManager.PlayerTrain.Handles.Reverser.MaxWidth > 48)
					{
						x += (TrainManager.PlayerTrain.Handles.Reverser.MaxWidth - 48);
					}
					if (TrainManager.PlayerTrain.Handles.Power.MaxWidth > 48)
					{
						x += (TrainManager.PlayerTrain.Handles.Power.MaxWidth - 48);
					}
				}
				if (Element.Subject == "single")
				{
					w = Math.Max(Math.Max(w, TrainManager.PlayerTrain.Handles.Power.MaxWidth), TrainManager.PlayerTrain.Handles.Brake.MaxWidth);
					if (TrainManager.PlayerTrain.Handles.Reverser.MaxWidth > 48)
					{
						x += (TrainManager.PlayerTrain.Handles.Reverser.MaxWidth - 48);
					}
				}
				if (Element.CenterMiddle.BackgroundTexture != null)
				{
					if (Program.CurrentHost.LoadTexture(ref Element.CenterMiddle.BackgroundTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						Color128 c = Element.BackgroundColor.CreateBackColor(sc, alpha);
						renderer.Rectangle.Draw(Element.CenterMiddle.BackgroundTexture, new Vector2(x, y), new Vector2(w, h), c);
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
				if (Element.CenterMiddle.OverlayTexture != null)
				{
					if (Program.CurrentHost.LoadTexture(ref Element.CenterMiddle.OverlayTexture, OpenGlTextureWrapMode.ClampClamp))
					{
						Color128 c = Element.OverlayColor.CreateBackColor(sc, alpha);
						renderer.Rectangle.Draw(Element.CenterMiddle.BackgroundTexture, new Vector2(x, y), new Vector2(w, h), c);
					}
				}
			}
		}
	}
}
