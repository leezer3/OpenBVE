using System;
using System.Linq;
using LibRender2.Cameras;
using LibRender2.Overlays;
using LibRender2.Screens;
using LibRender2.Trains;
using LibRender2.Viewports;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenBveApi.Interface;
using RouteManager2.MessageManager;
using RouteManager2.SignalManager;
using RouteManager2.Stations;
using TrainManager;
using TrainManager.Handles;
using TrainManager.Car;
using TrainManager.Car.Systems;
using OpenBveApi.Routes;

namespace OpenBve
{
	internal static partial class MainLoop
	{
		/// <summary>The ProcessControls function should be called once a frame, and updates the simulation accordingly</summary>
		/// <param name="TimeElapsed">The time elapsed in ms since the last call to this function</param>
		internal static void ProcessControls(double TimeElapsed)
		{
			for (int i = 0; i < Program.Joysticks.AttachedJoysticks.Count; i++)
			{
				/*
				 * Prequisite checks:
				 * Is our joystick connected?
				 * Have we already detected the disconnection?
				 */
				Guid guid = Program.Joysticks.AttachedJoysticks.ElementAt(i).Key;
				if (!Program.Joysticks.AttachedJoysticks[guid].IsConnected() && Program.Joysticks.AttachedJoysticks[guid].Disconnected == false)
				{
					Program.Joysticks.AttachedJoysticks[guid].Disconnected = true;
					for (int j = 0; j < Interface.CurrentControls.Length; j++)
					{
						if (Interface.CurrentControls[j].Method == ControlMethod.Joystick && Interface.CurrentControls[i].Device == guid)
						{
							//This control is bound to our disconnected joystick, so let's kick into pause mode
							Program.Renderer.CurrentInterface = InterfaceType.Pause;
						}
					}
				}
				else if (Program.Joysticks.AttachedJoysticks[guid].IsConnected() && Program.Joysticks.AttachedJoysticks[guid].Disconnected)
				{
					//Reconnected, so kick out of pause mode
					Program.Renderer.CurrentInterface = InterfaceType.Normal;
					Program.Joysticks.AttachedJoysticks[guid].Disconnected = false;
				}

			}
			if (Interface.CurrentOptions.KioskMode)
			{
				kioskModeTimer += TimeElapsed;
				if (kioskModeTimer > Interface.CurrentOptions.KioskModeTimer && TrainManager.PlayerTrain.AI == null)
				{
					/*
					 * We are in kiosk mode, and the timer has expired (NOTE: The AI null check saves us time)
					 *
					 * Therefore:
					 * ==> Switch back to the interior view of the driver's car
					 * ==> Enable AI
					 */
					TrainManager.PlayerTrain.CameraCar = TrainManager.PlayerTrain.DriverCar;
					MainLoop.SaveCameraSettings();
					bool lookahead = false;
					if (Program.Renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead & (Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable || Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.Restricted3D))
					{
						MessageManager.AddMessage(Translations.GetInterfaceString("notification_interior_lookahead"),
							MessageDependency.CameraView, GameMode.Expert,
							MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
						lookahead = true;
					}
					else
					{
						MessageManager.AddMessage(Translations.GetInterfaceString("notification_interior"),
							MessageDependency.CameraView, GameMode.Expert,
							MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
					}

					Program.Renderer.Camera.CurrentMode = CameraViewMode.Interior;
					MainLoop.RestoreCameraSettings();
					bool returnToCab = false;
					for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					{
						if (j == TrainManager.PlayerTrain.CameraCar)
						{
							if (TrainManager.PlayerTrain.Cars[j].HasInteriorView)
							{
								TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Interior);
								Program.Renderer.Camera.CurrentRestriction = TrainManager.PlayerTrain.Cars[j].CameraRestrictionMode;
							}
							else
							{
								TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.NotVisible, true);
								returnToCab = true;
							}
						}
						else
						{
							TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.NotVisible, true);
						}
					}

					if (returnToCab)
					{
						//If our selected car does not have an interior view, we must store this fact, and return to the driver car after the loop has finished
						TrainManager.PlayerTrain.CameraCar = TrainManager.PlayerTrain.DriverCar;
						TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].ChangeCarSection(CarSectionType.Interior);
						Program.Renderer.Camera.CurrentRestriction = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestrictionMode;
					}

					//Hide bogies
					for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					{
						TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(-1);
						TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(-1);
						TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(-1);
					}

					Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					World.UpdateAbsoluteCamera(TimeElapsed);
					Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
					if (Program.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable)
					{
						if (!Program.Renderer.Camera.PerformRestrictionTest(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction))
						{
							World.InitializeCameraRestriction();
						}
					}

					if (lookahead)
					{
						Program.Renderer.Camera.CurrentMode = CameraViewMode.InteriorLookAhead;
					}
					TrainManager.PlayerTrain.AI = new Game.SimpleHumanDriverAI(TrainManager.PlayerTrain, Double.PositiveInfinity);
					if (TrainManager.PlayerTrain.Plugin != null && TrainManager.PlayerTrain.Plugin.SupportsAI == AISupport.None)
					{
						MessageManager.AddMessage(Translations.GetInterfaceString("notification_aiunable"), MessageDependency.None, GameMode.Expert, MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
					}

				}
			}
			//If we are currently blocking key repeat events from firing, return
			if (BlockKeyRepeat) return;
			if (TrainManagerBase.PlayerTrain.Plugin != null && TrainManagerBase.PlayerTrain.Plugin.BlockingInput) return;
			switch (Program.Renderer.CurrentInterface)
			{
				case InterfaceType.Pause:
					// pause
					kioskModeTimer = 0;
					for (int i = 0; i < Interface.CurrentControls.Length; i++)
					{
						if (Interface.CurrentControls[i].InheritedType == Translations.CommandType.Digital)
						{
							if (Interface.CurrentControls[i].DigitalState == DigitalControlState.Pressed)
							{
								Interface.CurrentControls[i].DigitalState =
									DigitalControlState.PressedAcknowledged;
								switch (Interface.CurrentControls[i].Command)
								{
									case Translations.Command.MiscPause:
										Program.Renderer.CurrentInterface = InterfaceType.Normal;
										break;
									case Translations.Command.MenuActivate:
										Game.Menu.PushMenu(MenuType.Top);
										break;
									case Translations.Command.MiscQuit:
										Game.Menu.PushMenu(MenuType.Quit);
										break;
									case Translations.Command.MiscFullscreen:
										Screen.ToggleFullscreen();
										Interface.CurrentControls[i].AnalogState = 0.0;
										Interface.CurrentControls[i].DigitalState = DigitalControlState.Released;
										RemoveControlRepeat(i);
										break;
									case Translations.Command.MiscMute:
										Program.Sounds.GlobalMute = !Program.Sounds.GlobalMute;
										Program.Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
										break;
								}
							}
						}
					}
					break;
/*
				case InterfaceType.CustomiseControl:
					break;
*/
				case InterfaceType.Menu:			// MENU
					kioskModeTimer = 0;
					for (int i = 0; i < Interface.CurrentControls.Length; i++)
					{
						if (Interface.CurrentControls[i].InheritedType == Translations.CommandType.Digital
								&& Interface.CurrentControls[i].DigitalState == DigitalControlState.Pressed)
						{
							Interface.CurrentControls[i].DigitalState =
									DigitalControlState.PressedAcknowledged;
							Game.Menu.ProcessCommand(Interface.CurrentControls[i].Command, TimeElapsed);
						}
					}
					break;

				case InterfaceType.Normal:
					// normal
					for (int i = 0; i < Interface.CurrentControls.Length; i++)
					{
						if (Interface.CurrentControls[i].InheritedType == Translations.CommandType.AnalogHalf |
							Interface.CurrentControls[i].InheritedType == Translations.CommandType.AnalogFull)
						{
							// analog control
							if (Interface.CurrentControls[i].AnalogState != 0.0)
							{
								switch (Interface.CurrentControls[i].Command)
								{
									case Translations.Command.PowerHalfAxis:
									case Translations.Command.PowerFullAxis:
										if (TrainManager.PlayerTrain.AI != null)
										{
											//If AI is enabled, it fights with the axis....
											return;
										}
										// power half/full-axis
										if (TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle)
										{
											double a = Interface.CurrentControls[i].AnalogState;
											if (Interface.CurrentControls[i].Command == Translations.Command.PowerFullAxis)
											{
												a = 0.5*(a + 1.0);
											}
											a *= (double) TrainManager.PlayerTrain.Handles.Power.MaximumNotch;
											int p = (int) Math.Round(a);
											TrainManager.PlayerTrain.Handles.Power.ApplyState(p, false);
										}
										break;
									case Translations.Command.BrakeHalfAxis:
									case Translations.Command.BrakeFullAxis:
										if (TrainManager.PlayerTrain.AI != null)
										{
											//If AI is enabled, it fights with the axis....
											return;
										}
										// brake half/full-axis
										if (TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle)
										{
											if (TrainManager.PlayerTrain.Handles.Brake is AirBrakeHandle)
											{
												double a = Interface.CurrentControls[i].AnalogState;
												if (Interface.CurrentControls[i].Command ==
													Translations.Command.BrakeFullAxis)
												{
													a = 0.5*(a + 1.0);
												}
												int b = (int) Math.Round(3.0*a);
												switch (b)
												{
													case 0:
														TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver =
															false;
														TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
														break;
													case 1:
														TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver =
															false;
														TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Lap);
														break;
													case 2:
														TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver =
															false;
														TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
														break;
													case 3:
														if (Interface.CurrentOptions.AllowAxisEB)
														{
															TrainManager.PlayerTrain.Handles.EmergencyBrake.Apply();
														}
														break;
												}
											}
											else
											{
												if (TrainManager.PlayerTrain.Handles.HasHoldBrake)
												{
													double a = Interface.CurrentControls[i].AnalogState;
													if (Interface.CurrentControls[i].Command ==
														Translations.Command.BrakeFullAxis)
													{
														a = 0.5*(a + 1.0);
													}
													a *= (double) TrainManager.PlayerTrain.Handles.Brake.MaximumNotch + 2;
													int b = (int) Math.Round(a);
													bool q = b == 1;
													if (b > 0) b--;
													if (b <= TrainManager.PlayerTrain.Handles.Brake.MaximumNotch)
													{
														TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
														TrainManager.PlayerTrain.Handles.Brake.ApplyState(b,false);
													}
													else
													{
														TrainManager.PlayerTrain.Handles.EmergencyBrake.Apply();
													}
													TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(q);
												}
												else
												{
													double a = Interface.CurrentControls[i].AnalogState;
													if (Interface.CurrentControls[i].Command ==
														Translations.Command.BrakeFullAxis)
													{
														a = 0.5*(a + 1.0);
													}
													a *= (double) TrainManager.PlayerTrain.Handles.Brake.MaximumNotch + 1;
													int b = (int) Math.Round(a);
													if (b <= TrainManager.PlayerTrain.Handles.Brake.MaximumNotch)
													{
														TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
														TrainManager.PlayerTrain.Handles.Brake.ApplyState(b,false);
													}
													else
													{
														if (Interface.CurrentOptions.AllowAxisEB)
														{
															TrainManager.PlayerTrain.Handles.EmergencyBrake.Apply();
														}
													}
												}
											}
										}
										break;
									case Translations.Command.SingleFullAxis:
										if (TrainManager.PlayerTrain.AI != null)
										{
											//If AI is enabled, it fights with the axis....
											return;
										}
										// single full axis
										if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
										{
											if (TrainManager.PlayerTrain.Handles.HasHoldBrake)
											{
												double a = Interface.CurrentControls[i].AnalogState;
												int p =
													(int)
														Math.Round(a*
																   (double)
																	   TrainManager.PlayerTrain.Handles.Power.MaximumNotch);
												int b =
													(int)
														Math.Round(-a*
																   ((double)
																	   TrainManager.PlayerTrain.Handles.Brake.MaximumNotch +
																   2));
												if (p < 0) p = 0;
												if (b < 0) b = 0;
												bool q = b == 1;
												if (b > 0) b--;
												if (b <= TrainManager.PlayerTrain.Handles.Brake.MaximumNotch)
												{
													TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
													TrainManager.PlayerTrain.Handles.Brake.ApplyState(b,false);
													TrainManager.PlayerTrain.Handles.Power.ApplyState(p,false);
												}
												else
												{
													if (Interface.CurrentOptions.AllowAxisEB)
													{
														TrainManager.PlayerTrain.Handles.EmergencyBrake.Apply();
													}
												}
												TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(q);
											}
											else
											{
												double a = Interface.CurrentControls[i].AnalogState;
												int p =
													(int)
														Math.Round(a*
																   (double)
																	   TrainManager.PlayerTrain.Handles.Power.MaximumNotch);
												int b =
													(int)
														Math.Round(-a*
																   ((double)
																	   TrainManager.PlayerTrain.Handles.Brake.MaximumNotch +
																	1));
												if (p < 0) p = 0;
												if (b < 0) b = 0;
												if (b <= TrainManager.PlayerTrain.Handles.Brake.MaximumNotch)
												{
													TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
													TrainManager.PlayerTrain.Handles.Brake.ApplyState(b,false);
													TrainManager.PlayerTrain.Handles.Power.ApplyState(p,false);
												}
												else
												{
													if (Interface.CurrentOptions.AllowAxisEB)
													{
														TrainManager.PlayerTrain.Handles.EmergencyBrake.Apply();
													}
												}
											}
										}
										break;
									case Translations.Command.ReverserFullAxis:
										if (TrainManager.PlayerTrain.AI != null)
										{
											//If AI is enabled, it fights with the axis....
											return;
										}
										// reverser full axis
										double als = Interface.CurrentControls[i].AnalogState;
										TrainManager.PlayerTrain.Handles.Reverser.ApplyState((ReverserPosition) (int) Math.Round(als));
										break;
									case Translations.Command.CameraMoveForward:
										// camera move forward
										if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
											Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead |
											Program.Renderer.Camera.CurrentMode == CameraViewMode.Exterior)
										{
											double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
													   Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
												? CameraProperties.InteriorTopSpeed
												: CameraProperties.ExteriorTopSpeed;
											Program.Renderer.Camera.AlignmentDirection.Position.Z = s*
																						Interface.CurrentControls[i]
																							.AnalogState;
										}
										else
										{
											if (Program.Renderer.Camera.AtWorldEnd)
											{
												//Don't let the camera run off the end of the worldspace
												break;
											}
											Program.Renderer.Camera.AlignmentDirection.TrackPosition = CameraProperties.ExteriorTopSpeed *
																						   Interface.CurrentControls[i]
																							   .AnalogState;
										}
										break;
									case Translations.Command.CameraMoveBackward:
										// camera move backward
										if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
											Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead |
											Program.Renderer.Camera.CurrentMode == CameraViewMode.Exterior)
										{
											double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
													   Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
												? CameraProperties.InteriorTopSpeed
												: CameraProperties.ExteriorTopSpeed;
											Program.Renderer.Camera.AlignmentDirection.Position.Z = -s*
																						Interface.CurrentControls[i]
																							.AnalogState;
										}
										else
										{
											Program.Renderer.Camera.AlignmentDirection.TrackPosition =
												-CameraProperties.ExteriorTopSpeed *Interface.CurrentControls[i].AnalogState;
										}
										break;
									case Translations.Command.CameraMoveLeft:
										// camera move left
									{
										double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
												   Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
											? CameraProperties.InteriorTopSpeed
											: CameraProperties.ExteriorTopSpeed;
										Program.Renderer.Camera.AlignmentDirection.Position.X = -s*
																					Interface.CurrentControls[i]
																						.AnalogState;
									}
										break;
									case Translations.Command.CameraMoveRight:
										// camera move right
									{
										double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
												   Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
											? CameraProperties.InteriorTopSpeed
											: CameraProperties.ExteriorTopSpeed;
										Program.Renderer.Camera.AlignmentDirection.Position.X = s*
																					Interface.CurrentControls[i]
																						.AnalogState;
									}
										break;
									case Translations.Command.CameraMoveUp:
										// camera move up
									{
										double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
												   Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
											? CameraProperties.InteriorTopSpeed
											: CameraProperties.ExteriorTopSpeed;
										Program.Renderer.Camera.AlignmentDirection.Position.Y = s*
																					Interface.CurrentControls[i]
																						.AnalogState;
									}
										break;
									case Translations.Command.CameraMoveDown:
										// camera move down
									{
										double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
												   Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
											? CameraProperties.InteriorTopSpeed
											: CameraProperties.ExteriorTopSpeed;
										Program.Renderer.Camera.AlignmentDirection.Position.Y = -s*
																					Interface.CurrentControls[i]
																						.AnalogState;
									}
										break;
									case Translations.Command.CameraRotateLeft:
										// camera rotate left
									{
										double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
												   Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
											? CameraProperties.InteriorTopAngularSpeed
											: CameraProperties.ExteriorTopAngularSpeed;
										Program.Renderer.Camera.AlignmentDirection.Yaw = -s*Interface.CurrentControls[i].AnalogState;
									}
										break;
									case Translations.Command.CameraRotateRight:
										// camera rotate right
									{
										double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
												   Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
											? CameraProperties.InteriorTopAngularSpeed
											: CameraProperties.ExteriorTopAngularSpeed;
										Program.Renderer.Camera.AlignmentDirection.Yaw = s*Interface.CurrentControls[i].AnalogState;
									}
										break;
									case Translations.Command.CameraRotateUp:
										// camera rotate up
									{
										double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
												   Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
											? CameraProperties.InteriorTopAngularSpeed
											: CameraProperties.ExteriorTopAngularSpeed;
										Program.Renderer.Camera.AlignmentDirection.Pitch = s*
																			   Interface.CurrentControls[i].AnalogState;
									}
										break;
									case Translations.Command.CameraRotateDown:
										// camera rotate down
									{
										double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
												   Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
											? CameraProperties.InteriorTopAngularSpeed
											: CameraProperties.ExteriorTopAngularSpeed;
										Program.Renderer.Camera.AlignmentDirection.Pitch = -s*
																			   Interface.CurrentControls[i].AnalogState;
									}
										break;
									case Translations.Command.CameraRotateCCW:
										// camera rotate ccw
										if ((Program.Renderer.Camera.CurrentMode != CameraViewMode.Interior &
											 Program.Renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead) |
											Program.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.On)
										{
											double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
													   Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
												? CameraProperties.InteriorTopAngularSpeed
												: CameraProperties.ExteriorTopAngularSpeed;
											Program.Renderer.Camera.AlignmentDirection.Roll = -s*
																				  Interface.CurrentControls[i]
																					  .AnalogState;
										}
										break;
									case Translations.Command.CameraRotateCW:
										// camera rotate cw
										if ((Program.Renderer.Camera.CurrentMode != CameraViewMode.Interior &
											 Program.Renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead) |
											Program.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.On)
										{
											double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
													   Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
												? CameraProperties.InteriorTopAngularSpeed
												: CameraProperties.ExteriorTopAngularSpeed;
											Program.Renderer.Camera.AlignmentDirection.Roll = s*
																				  Interface.CurrentControls[i]
																					  .AnalogState;
										}
										break;
									case Translations.Command.CameraZoomIn:
										// camera zoom in
										if (TimeElapsed > 0.0)
										{
											Program.Renderer.Camera.AlignmentDirection.Zoom = -CameraProperties.ZoomTopSpeed *
																				  Interface.CurrentControls[i]
																					  .AnalogState;
										}
										break;
									case Translations.Command.CameraZoomOut:
										// camera zoom out
										if (TimeElapsed > 0.0)
										{
											Program.Renderer.Camera.AlignmentDirection.Zoom = CameraProperties.ZoomTopSpeed *
																				  Interface.CurrentControls[i]
																					  .AnalogState;
										}
										break;
									case Translations.Command.TimetableUp:
										// timetable up
										if (TimeElapsed > 0.0)
										{
											const double scrollSpeed = 250.0;
											if (Program.Renderer.CurrentTimetable == DisplayedTimetable.Default)
											{
												Timetable.DefaultTimetablePosition += scrollSpeed*
																					  Interface.CurrentControls[i]
																						  .AnalogState*TimeElapsed;
												if (Timetable.DefaultTimetablePosition > 0.0)
													Timetable.DefaultTimetablePosition = 0.0;
											}
											else if (Program.Renderer.CurrentTimetable == DisplayedTimetable.Custom)
											{
												Timetable.CustomTimetablePosition += scrollSpeed*
																					 Interface.CurrentControls[i]
																						 .AnalogState*TimeElapsed;
												if (Timetable.CustomTimetablePosition > 0.0)
													Timetable.CustomTimetablePosition = 0.0;
											}
										}
										break;
									case Translations.Command.TimetableDown:
										// timetable down
										if (TimeElapsed > 0.0)
										{
											const double scrollSpeed = 250.0;
											if (Program.Renderer.CurrentTimetable == DisplayedTimetable.Default)
											{
												Timetable.DefaultTimetablePosition -= scrollSpeed*
																					  Interface.CurrentControls[i]
																						  .AnalogState*TimeElapsed;
												double max;
												if (Timetable.DefaultTimetableTexture != null)
												{
													Program.CurrentHost.LoadTexture(ref Timetable.DefaultTimetableTexture,
														OpenGlTextureWrapMode.ClampClamp);
													max =
														Math.Min(
															Program.Renderer.Screen.Height - Timetable.DefaultTimetableTexture.Height,
															0.0);
												}
												else
												{
													max = 0.0;
												}
												if (Timetable.DefaultTimetablePosition < max)
													Timetable.DefaultTimetablePosition = max;
											}
											else if (Program.Renderer.CurrentTimetable == DisplayedTimetable.Custom)
											{
												Timetable.CustomTimetablePosition -= scrollSpeed*
																					 Interface.CurrentControls[i]
																						 .AnalogState*TimeElapsed;
												Texture texture =
													Timetable.CurrentCustomTimetableDaytimeTexture;
												if (texture == null)
												{
													texture = Timetable.CurrentCustomTimetableNighttimeTexture;
												}
												double max;
												if (texture != null)
												{
													Program.CurrentHost.LoadTexture(ref texture,
														OpenGlTextureWrapMode.ClampClamp);
													max = Math.Min(Program.Renderer.Screen.Height - texture.Height, 0.0);
												}
												else
												{
													max = 0.0;
												}
												if (Timetable.CustomTimetablePosition < max)
													Timetable.CustomTimetablePosition = max;
											}
										}
										break;
								}
							}
						}
						else if (Interface.CurrentControls[i].InheritedType == Translations.CommandType.Digital)
						{
							// digital control
							if (Interface.CurrentControls[i].DigitalState == DigitalControlState.Pressed)
							{
								// pressed
								Interface.CurrentControls[i].DigitalState =
									DigitalControlState.PressedAcknowledged;
								switch (Interface.CurrentControls[i].Command)
								{
									case Translations.Command.MiscQuit:
										// quit
										Game.Menu.PushMenu(MenuType.Quit);
										break;
									case Translations.Command.CameraInterior:
										// camera: interior
										{
											MainLoop.SaveCameraSettings();
											bool lookahead = false;
											if (Program.Renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead & Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable)
											{
												MessageManager.AddMessage(Translations.GetInterfaceString("notification_interior_lookahead"),
												                MessageDependency.CameraView, GameMode.Expert,
												                MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
												lookahead = true;
											}
											else
											{
												MessageManager.AddMessage(Translations.GetInterfaceString("notification_interior"),
												                MessageDependency.CameraView, GameMode.Expert,
												                MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
											}
											Program.Renderer.Camera.CurrentMode = CameraViewMode.Interior;
											MainLoop.RestoreCameraSettings();
											bool returnToCab = false;
											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
											{
												if (j == TrainManager.PlayerTrain.CameraCar)
												{
													if (TrainManager.PlayerTrain.Cars[j].HasInteriorView)
													{
														TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Interior);
														Program.Renderer.Camera.CurrentRestriction = TrainManager.PlayerTrain.Cars[j].CameraRestrictionMode;
													}
													else
													{
														TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.NotVisible, true);
														returnToCab = true;
													}
												}
												else
												{
													TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.NotVisible, true);
												}
											}
											if (returnToCab)
											{
												//If our selected car does not have an interior view, we must store this fact, and return to the driver car after the loop has finished
												TrainManager.PlayerTrain.CameraCar = TrainManager.PlayerTrain.DriverCar;
												TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].ChangeCarSection(CarSectionType.Interior);
												Program.Renderer.Camera.CurrentRestriction = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestrictionMode;
											}
											//Hide bogies
											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
											{
												TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(-1);
												TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(-1);
												TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(-1);
											}
											Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
											Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
											Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
											World.UpdateAbsoluteCamera(TimeElapsed);
											Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
											if (Program.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable)
											{
												if (!Program.Renderer.Camera.PerformRestrictionTest(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction))
												{
													World.InitializeCameraRestriction();
												}
											}
											if (lookahead)
											{
												Program.Renderer.Camera.CurrentMode = CameraViewMode.InteriorLookAhead;
											}
										}
										break;
									case Translations.Command.CameraInteriorNoPanel:
										// camera: interior
										{
											MainLoop.SaveCameraSettings();
											bool lookahead = false;
											if (Program.Renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead & Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable)
											{
												MessageManager.AddMessage(Translations.GetInterfaceString("notification_interior_lookahead"),
												                MessageDependency.CameraView, GameMode.Expert,
												                MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
												lookahead = true;
											}
											else
											{
												MessageManager.AddMessage(Translations.GetInterfaceString("notification_interior"),
												                MessageDependency.CameraView, GameMode.Expert,
												                MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
											}
											Program.Renderer.Camera.CurrentMode = CameraViewMode.Interior;
											MainLoop.RestoreCameraSettings();
											bool returnToCab = false;
											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
											{
												if (j == TrainManager.PlayerTrain.CameraCar)
												{
													if (TrainManager.PlayerTrain.Cars[j].HasInteriorView)
													{
														TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Interior);
														Program.Renderer.Camera.CurrentRestriction = TrainManager.PlayerTrain.Cars[j].CameraRestrictionMode;
													}
													else
													{
														TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.NotVisible, true);
														returnToCab = true;
													}
												}
												else
												{
													TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.NotVisible, true);
												}
											}
											if (returnToCab)
											{
												//If our selected car does not have an interior view, we must store this fact, and return to the driver car after the loop has finished
												TrainManager.PlayerTrain.CameraCar = TrainManager.PlayerTrain.DriverCar;
												TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].ChangeCarSection(CarSectionType.Interior);
												Program.Renderer.Camera.CurrentRestriction = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestrictionMode;
											}
											//Hide interior and bogies
											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
											{
												TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.NotVisible, true);
												TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(-1);
												TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(-1);
												TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(-1);
											}
											Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
											Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
											Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
											World.UpdateAbsoluteCamera(TimeElapsed);
											Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
											if (Program.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable)
											{
												if (!Program.Renderer.Camera.PerformRestrictionTest(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction))
												{
													World.InitializeCameraRestriction();
												}
											}
											if (lookahead)
											{
												Program.Renderer.Camera.CurrentMode = CameraViewMode.InteriorLookAhead;
											}
										}
										break;
									case Translations.Command.CameraExterior:
										// camera: exterior
										if (TrainManager.PlayerTrain.CurrentDirection == TrackDirection.Reverse)
										{
											MessageManager.AddMessage(Translations.GetInterfaceString("notification_exterior") + " " + (TrainManager.PlayerTrain.Cars.Length - TrainManager.PlayerTrain.CameraCar), MessageDependency.CameraView, GameMode.Expert,
												MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
										}
										else
										{
											MessageManager.AddMessage(Translations.GetInterfaceString("notification_exterior") + " " + (TrainManager.PlayerTrain.CameraCar + 1), MessageDependency.CameraView, GameMode.Expert,
												MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
										}
										
										SaveCameraSettings();
										Program.Renderer.Camera.CurrentMode = CameraViewMode.Exterior;
										RestoreCameraSettings();
										for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
										{
											TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Exterior);
											TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
											TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
											TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
										}
										Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
										Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
										Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
										World.UpdateAbsoluteCamera(TimeElapsed);
										Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
										break;
									case Translations.Command.CameraTrack:
									case Translations.Command.CameraFlyBy:
										// camera: track / fly-by
									{
										SaveCameraSettings();
										if (Interface.CurrentControls[i].Command == Translations.Command.CameraTrack)
										{
											Program.Renderer.Camera.CurrentMode = CameraViewMode.Track;
											MessageManager.AddMessage(Translations.GetInterfaceString("notification_track"),
												MessageDependency.CameraView, GameMode.Expert,
												MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
										}
										else
										{
											if (Program.Renderer.Camera.CurrentMode == CameraViewMode.FlyBy)
											{
												Program.Renderer.Camera.CurrentMode = CameraViewMode.FlyByZooming;
												MessageManager.AddMessage(
													Translations.GetInterfaceString("notification_flybyzooming"),
													MessageDependency.CameraView, GameMode.Expert,
													MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
											}
											else
											{
												Program.Renderer.Camera.CurrentMode = CameraViewMode.FlyBy;
												MessageManager.AddMessage(
													Translations.GetInterfaceString("notification_flybynormal"),
													MessageDependency.CameraView, GameMode.Expert,
													MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
											}
										}
										RestoreCameraSettings();
										for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
										{
											TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Exterior);
											TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
											TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
											TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
										}
										Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
										Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
										Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
										World.UpdateAbsoluteCamera(TimeElapsed);
										Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
									}
										break;
									case Translations.Command.CameraPreviousPOI:
										//If we are in the exterior train view, shift down one car until we hit the last car
										if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Exterior)
										{
											TrainManager.PlayerTrain.ChangeCameraCar(false);
											return;
										}
										//Otherwise, check if we can move down to the previous POI
										if (Game.ApplyPointOfInterest(-1, true))
										{
											if (Program.Renderer.Camera.CurrentMode < CameraViewMode.Exterior)
											{
												SaveCameraSettings();
												Program.Renderer.Camera.CurrentMode = CameraViewMode.Track;
												MessageManager.AddMessage(Translations.GetInterfaceString("notification_track"),
													MessageDependency.CameraView, GameMode.Expert,
													MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
											}
											double z = Program.Renderer.Camera.Alignment.Position.Z;
											Program.Renderer.Camera.Alignment.Position =
												new OpenBveApi.Math.Vector3(Program.Renderer.Camera.Alignment.Position.X,
													Program.Renderer.Camera.Alignment.Position.Y, 0.0);
											Program.Renderer.Camera.Alignment.Zoom = 0.0;
											Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
											Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
											{
												TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Exterior);
												TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
												TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
												TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
											}
											Program.Renderer.CameraTrackFollower.UpdateRelative(z, true, false);
											Program.Renderer.Camera.Alignment.TrackPosition = Program.Renderer.CameraTrackFollower.TrackPosition;
											Program.Renderer.Camera.VerticalViewingAngle = Program.Renderer.Camera.OriginalVerticalViewingAngle;
											Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
											World.UpdateAbsoluteCamera(TimeElapsed);
											Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
										}
										break;
									case Translations.Command.CameraNextPOI:
										//If we are in the exterior train view, shift up one car until we hit index 0
										if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Exterior)
										{
											TrainManagerBase.PlayerTrain.ChangeCameraCar(true);
											return;
										}
										//Otherwise, check if we can move up to the next POI
										if (Game.ApplyPointOfInterest(1, true))
										{
											if (Program.Renderer.Camera.CurrentMode < CameraViewMode.Exterior)
											{
												SaveCameraSettings();
												Program.Renderer.Camera.CurrentMode = CameraViewMode.Track;
												MessageManager.AddMessage(Translations.GetInterfaceString("notification_track"),
													MessageDependency.CameraView, GameMode.Expert,
													MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
											}
											double z = Program.Renderer.Camera.Alignment.Position.Z;
											Program.Renderer.Camera.Alignment.Position =
												new OpenBveApi.Math.Vector3(Program.Renderer.Camera.Alignment.Position.X,
													Program.Renderer.Camera.Alignment.Position.Y, 0.0);
											Program.Renderer.Camera.Alignment.Zoom = 0.0;
											Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
											Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
											{
												TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Exterior);
												TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
												TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
												TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
											}
											Program.Renderer.CameraTrackFollower.UpdateRelative(z, true, false);
											Program.Renderer.Camera.Alignment.TrackPosition =
												Program.Renderer.CameraTrackFollower.TrackPosition;
											Program.Renderer.Camera.VerticalViewingAngle = Program.Renderer.Camera.OriginalVerticalViewingAngle;
											Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
											World.UpdateAbsoluteCamera(TimeElapsed);
											Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
										}
										break;
									case Translations.Command.CameraReset:
										// camera: reset
										if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
											Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead)
										{
											Program.Renderer.Camera.Alignment.Position = new OpenBveApi.Math.Vector3(0.0, 0.0,
												0.0);
										}
										Program.Renderer.Camera.Alignment.Yaw = TrainManager.PlayerTrain.CurrentDirection == TrackDirection.Reverse ? 180 / 57.2957795130824 : 0;
										Program.Renderer.Camera.Alignment.Pitch = 0.0;
										Program.Renderer.Camera.Alignment.Roll = 0.0;
										if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Track)
										{
											Program.Renderer.CameraTrackFollower.UpdateAbsolute(
												TrainManager.PlayerTrain.Cars[0].TrackPosition, true,
												false);
										}
										else if (Program.Renderer.Camera.CurrentMode == CameraViewMode.FlyBy |
												 Program.Renderer.Camera.CurrentMode == CameraViewMode.FlyByZooming)
										{
											if (TrainManager.PlayerTrain.CurrentSpeed >= 0.0)
											{
												double d = 30.0 +
														   4.0*TrainManager.PlayerTrain.CurrentSpeed;
												Program.Renderer.CameraTrackFollower.UpdateAbsolute(
													TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower
														.TrackPosition + d, true, false);
											}
											else
											{
												double d = 30.0 -
														   4.0*TrainManager.PlayerTrain.CurrentSpeed;
												Program.Renderer.CameraTrackFollower.UpdateAbsolute(
													TrainManager.PlayerTrain.Cars[
														TrainManager.PlayerTrain.Cars.Length - 1].RearAxle.Follower
														.TrackPosition - d, true, false);
											}
										}
										Program.Renderer.Camera.Alignment.TrackPosition =
											Program.Renderer.CameraTrackFollower.TrackPosition;
										Program.Renderer.Camera.Alignment.Zoom = 0.0;
										Program.Renderer.Camera.VerticalViewingAngle = Program.Renderer.Camera.OriginalVerticalViewingAngle;
										Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
										Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
										Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
										World.UpdateAbsoluteCamera(TimeElapsed);
										Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
										if ((Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
											 Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead) &
											(Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.On || Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.Restricted3D))
										{
											if (!Program.Renderer.Camera.PerformRestrictionTest(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction))
											{
												World.InitializeCameraRestriction();
											}
										}
										break;
									case Translations.Command.CameraRestriction:
										// camera: restriction
										switch (Program.Renderer.Camera.CurrentRestriction)
										{
											case CameraRestrictionMode.Restricted3D:
												Program.Renderer.Camera.CurrentRestriction = CameraRestrictionMode.NotAvailable;
												MessageManager.AddMessage(Translations.GetInterfaceString("notification_camerarestriction_off"),
													MessageDependency.CameraView, GameMode.Expert,
													MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
												break;
											case CameraRestrictionMode.NotAvailable:
												Program.Renderer.Camera.CurrentRestriction = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestrictionMode;
												if (Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.Restricted3D)
												{
													MessageManager.AddMessage(Translations.GetInterfaceString("notification_camerarestriction_on"),
														MessageDependency.CameraView, GameMode.Expert,
														MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
												}
												break;
											default:
												Program.Renderer.Camera.CurrentRestriction = Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.Off ? TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestrictionMode : CameraRestrictionMode.Off;
												World.InitializeCameraRestriction();
												if (Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.Off)
												{
													MessageManager.AddMessage(Translations.GetInterfaceString("notification_camerarestriction_off"),
														MessageDependency.CameraView, GameMode.Expert,
														MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
												}
												else
												{
													MessageManager.AddMessage(Translations.GetInterfaceString("notification_camerarestriction_on"),
														MessageDependency.CameraView, GameMode.Expert,
														MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
												}
												break;
										}
										break;
									case Translations.Command.SinglePower:
										// single power
										if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
										{
											int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
											if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
											{
												TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
											}
											else if (b == 1 & TrainManager.PlayerTrain.Handles.HasHoldBrake)
											{
												TrainManager.PlayerTrain.Handles.Brake.ApplyState(0,false);
												TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
											}
											else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
											{
												TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
											}
											else if (b > 0)
											{
												TrainManager.PlayerTrain.Handles.Brake.ApplyState(-1,true);
											}
											else
											{
												int p = TrainManager.PlayerTrain.Handles.Power.Driver;
												if (p < TrainManager.PlayerTrain.Handles.Power.MaximumNotch)
												{
													TrainManager.PlayerTrain.Handles.Power.ApplyState(1, true);
												}
											}
										}
										TrainManager.PlayerTrain.Handles.Power.ContinuousMovement = true;
										break;
									case Translations.Command.SingleNeutral:
										// single neutral
										if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
										{
											int p = TrainManager.PlayerTrain.Handles.Power.Driver;
											if (p > 0)
											{
												TrainManager.PlayerTrain.Handles.Power.ApplyState(-1, true);
												TrainManager.PlayerTrain.Handles.Power.ContinuousMovement = true;
											}
											else
											{
												int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
												if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
												{
													TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
												}
												else if (b == 1 & TrainManager.PlayerTrain.Handles.HasHoldBrake)
												{
													TrainManager.PlayerTrain.Handles.Brake.ApplyState(0,false);
													TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
												}
												else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
												}
												else if (b > 0)
												{
													TrainManager.PlayerTrain.Handles.Brake.ApplyState(-1,true);
												}
												TrainManager.PlayerTrain.Handles.Brake.ContinuousMovement = true;
											}
										}
										break;
									case Translations.Command.SingleBrake:
										// single brake
										if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
										{
											int p = TrainManager.PlayerTrain.Handles.Power.Driver;
											if (p > 0)
											{
												TrainManager.PlayerTrain.Handles.Power.ApplyState(-1,true);
											}
											else
											{
												int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
												if (TrainManager.PlayerTrain.Handles.HasHoldBrake & b == 0 &
													!TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
												}
												else if (b < TrainManager.PlayerTrain.Handles.Brake.MaximumNotch)
												{
													TrainManager.PlayerTrain.Handles.Brake.ApplyState(1,true);
													TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
												}
											}
										}
										//Set the brake handle fast movement bool at the end of the call in order to not catch it on the first movement
										TrainManager.PlayerTrain.Handles.Brake.ContinuousMovement = true;
										break;
									case Translations.Command.SingleEmergency:
										// single emergency
										if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
										{
											TrainManager.PlayerTrain.Handles.EmergencyBrake.Apply();
										}
										break;
									case Translations.Command.PowerIncrease:
										// power increase
										if (TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle)
										{
											int p = TrainManager.PlayerTrain.Handles.Power.Driver;
											if (p < TrainManager.PlayerTrain.Handles.Power.MaximumNotch)
											{
												TrainManager.PlayerTrain.Handles.Power.ApplyState(1,true);
											}
										}
										TrainManager.PlayerTrain.Handles.Power.ContinuousMovement = true;
										break;
									case Translations.Command.PowerDecrease:
										// power decrease
										if (TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle)
										{
											int p = TrainManager.PlayerTrain.Handles.Power.Driver;
											if (p > 0)
											{
												TrainManager.PlayerTrain.Handles.Power.ApplyState(-1,true);
											}
										}
										TrainManager.PlayerTrain.Handles.Power.ContinuousMovement = true;
										break;
									case Translations.Command.BrakeIncrease:
										// brake increase
										if (TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle)
										{
											if (TrainManager.PlayerTrain.Handles.Brake is AirBrakeHandle)
											{
												if (TrainManager.PlayerTrain.Handles.HasHoldBrake &
													TrainManager.PlayerTrain.Handles.Brake.Driver ==
													(int)AirBrakeHandleState.Release &
													!TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
												}
												else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Lap);
													TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
												}
												else if (TrainManager.PlayerTrain.Handles.Brake.Driver ==
												         (int)AirBrakeHandleState.Lap)
												{
													TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
												}
												else if (TrainManager.PlayerTrain.Handles.Brake.Driver ==
												         (int)AirBrakeHandleState.Release)
												{
													TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Lap);
												}
											}
											else
											{
												int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
												if (TrainManager.PlayerTrain.Handles.HasHoldBrake & b == 0 &
													!TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
												}
												else if (b < TrainManager.PlayerTrain.Handles.Brake.MaximumNotch)
												{
													TrainManager.PlayerTrain.Handles.Brake.ApplyState(1,true);
													TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
												}
											}
										}
										TrainManager.PlayerTrain.Handles.Brake.ContinuousMovement = true;
										break;
									case Translations.Command.BrakeDecrease:
										// brake decrease
										if (TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle)
										{
											if (TrainManager.PlayerTrain.Handles.Brake is AirBrakeHandle)
											{
												if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
												{
													TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
												}
												else if (TrainManager.PlayerTrain.Handles.HasHoldBrake &
														 TrainManager.PlayerTrain.Handles.Brake.Driver ==
														 (int)AirBrakeHandleState.Lap &
														 !TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
												}
												else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
													TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
												}
												else if (TrainManager.PlayerTrain.Handles.Brake.Driver ==
												         (int)AirBrakeHandleState.Lap)
												{
													TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
												}
												else if (TrainManager.PlayerTrain.Handles.Brake.Driver ==
												         (int)AirBrakeHandleState.Service)
												{
													TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Lap);
												}
											}
											else
											{
												int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
												if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
												{
													TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
												}
												else if (b == 1 & TrainManager.PlayerTrain.Handles.HasHoldBrake)
												{
													TrainManager.PlayerTrain.Handles.Brake.ApplyState(0,false);
													TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
												}
												else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
												}
												else if (b > 0)
												{
													TrainManager.PlayerTrain.Handles.Brake.ApplyState(-1,true);
												}
											}
										}
										TrainManager.PlayerTrain.Handles.Brake.ContinuousMovement = true;
										break;
									case Translations.Command.LocoBrakeIncrease:
										if (TrainManager.PlayerTrain.Handles.LocoBrake is LocoAirBrakeHandle)
										{
											if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)AirBrakeHandleState.Lap)
											{
												TrainManager.PlayerTrain.Handles.LocoBrake.ApplyState(AirBrakeHandleState.Service);
											}
											else if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)AirBrakeHandleState.Release)
											{
												TrainManager.PlayerTrain.Handles.LocoBrake.ApplyState(AirBrakeHandleState.Lap);
											}
										}
										else
										{
											TrainManager.PlayerTrain.Handles.LocoBrake.ApplyState(1, true);
										}
										
										break;
									case Translations.Command.LocoBrakeDecrease:
										if (TrainManager.PlayerTrain.Handles.LocoBrake is LocoAirBrakeHandle)
										{
											if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)AirBrakeHandleState.Lap)
											{
												TrainManager.PlayerTrain.Handles.LocoBrake.ApplyState(AirBrakeHandleState.Release);
											}
											else if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)AirBrakeHandleState.Service)
											{
												TrainManager.PlayerTrain.Handles.LocoBrake.ApplyState(AirBrakeHandleState.Lap);
											}
										}
										else
										{
											TrainManager.PlayerTrain.Handles.LocoBrake.ApplyState(-1, true);
										}
										break;
									case Translations.Command.BrakeEmergency:
										// brake emergency
										TrainManager.PlayerTrain.Handles.EmergencyBrake.Apply();
										break;
									case Translations.Command.DeviceConstSpeed:
										// const speed
										if (TrainManager.PlayerTrain.Specs.HasConstSpeed)
										{
											TrainManager.PlayerTrain.Specs.CurrentConstSpeed =
												!TrainManager.PlayerTrain.Specs.CurrentConstSpeed;
										}
										break;
									case Translations.Command.PowerAnyNotch:
										if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle && TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
										{
											TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
										}
										TrainManager.PlayerTrain.Handles.Brake.ApplyState(0,TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle);
										TrainManager.PlayerTrain.Handles.Power.ApplyState(Interface.CurrentControls[i].Option,false);
										break;
									case Translations.Command.BrakeAnyNotch:
										if (TrainManager.PlayerTrain.Handles.Brake is AirBrakeHandle)
										{
											if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
											{
												TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
											}
											TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
											if (Interface.CurrentControls[i].Option <= (int)AirBrakeHandleState.Release)
											{
												TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
											}
											else if (Interface.CurrentControls[i].Option == (int)AirBrakeHandleState.Lap)
											{
												TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Lap);
											}
											else
											{
												TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
											}
										}
										else
										{
											if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
											{
												TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
											}
											TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
											TrainManager.PlayerTrain.Handles.Brake.ApplyState(Interface.CurrentControls[i].Option, false);
											TrainManager.PlayerTrain.Handles.Power.ApplyState(0, TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle);
										}
										break;
									case Translations.Command.ReverserAnyPostion:
										TrainManager.PlayerTrain.Handles.Reverser.ApplyState((ReverserPosition)Interface.CurrentControls[i].Option);
										break;
									case Translations.Command.HoldBrake:
										if (TrainManager.PlayerTrain.Handles.HasHoldBrake && (TrainManager.PlayerTrain.Handles.Brake.Driver == 0 || TrainManager.PlayerTrain.Handles.Brake.Driver == 1) && !TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
										{
											TrainManager.PlayerTrain.Handles.Brake.ApplyState(0, false);
											TrainManager.PlayerTrain.Handles.Power.ApplyState(0, TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle);
											TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
										}
										break;
									case Translations.Command.ReverserForward:
										// reverser forward
										if (TrainManager.PlayerTrain.Handles.Reverser.Driver < ReverserPosition.Forwards)
										{
											TrainManager.PlayerTrain.Handles.Reverser.ApplyState(1, true);
										}
										break;
									case Translations.Command.ReverserBackward:
										// reverser backward
										if (TrainManager.PlayerTrain.Handles.Reverser.Driver > ReverserPosition.Reverse)
										{
											TrainManager.PlayerTrain.Handles.Reverser.ApplyState(-1, true);
										}
										break;
									case Translations.Command.HornPrimary:
									case Translations.Command.HornSecondary:
									case Translations.Command.HornMusic:
										// horn
										{
										int j = Interface.CurrentControls[i].Command == Translations.Command.HornPrimary
											? 0 : Interface.CurrentControls[i].Command == Translations.Command.HornSecondary ? 1 : 2;
										int d = TrainManager.PlayerTrain.DriverCar;
											if (TrainManager.PlayerTrain.Cars[d].Horns.Length > j)
											{
												TrainManager.PlayerTrain.Cars[d].Horns[j].Play();
												if (TrainManager.PlayerTrain.Plugin != null)
												{
													TrainManager.PlayerTrain.Plugin.HornBlow(j == 0
														? HornTypes.Primary
														: j == 1
															? HornTypes.Secondary
															: HornTypes.Music);
												}
											}
										}
										break;
									case Translations.Command.DoorsLeft:
										// doors: left
										if (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Doors[0].ButtonPressed)
										{
											return;
										}
										if ((TrainManager.PlayerTrain.GetDoorsState(true, false) &
											 TrainDoorState.Opened) == 0)
										{
											if (TrainManager.PlayerTrain.Specs.DoorOpenMode != DoorMode.Automatic)
											{
												TrainManager.PlayerTrain.OpenDoors(true, false);
											}
										}
										else
										{
											if (TrainManager.PlayerTrain.Specs.DoorCloseMode != DoorMode.Automatic)
											{
												TrainManager.PlayerTrain.CloseDoors(true, false);
											}
										}
										if (TrainManager.PlayerTrain.Plugin != null)
										{
											TrainManager.PlayerTrain.Plugin.KeyDown(VirtualKeys.LeftDoors);
										}
										//Set door button to pressed in the driver's car
										TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Doors[0].ButtonPressed = true;
										break;
									case Translations.Command.DoorsRight:
										// doors: right
										if (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Doors[1].ButtonPressed)
										{
											return;
										}
										if ((TrainManager.PlayerTrain.GetDoorsState(false, true) &
											 TrainDoorState.Opened) == 0)
										{
											if (TrainManager.PlayerTrain.Specs.DoorOpenMode != DoorMode.Automatic)
											{
												TrainManager.PlayerTrain.OpenDoors(false, true);
											}
										}
										else
										{
											if (TrainManager.PlayerTrain.Specs.DoorCloseMode != DoorMode.Automatic)
											{
												TrainManager.PlayerTrain.CloseDoors(false, true);
											}
										}
										if (TrainManager.PlayerTrain.Plugin != null)
										{
											TrainManager.PlayerTrain.Plugin.KeyDown(VirtualKeys.RightDoors);
										}
										TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Doors[1].ButtonPressed = true;
										break;
									case Translations.Command.PlayMicSounds:
										Program.Sounds.IsPlayingMicSounds = !Program.Sounds.IsPlayingMicSounds;
										break;
//We only want to mark these as obsolete for new users of the API
#pragma warning disable 618
									case Translations.Command.SecurityS:
									case Translations.Command.SecurityA1:
									case Translations.Command.SecurityA2:
									case Translations.Command.SecurityB1:
									case Translations.Command.SecurityB2:
									case Translations.Command.SecurityC1:
									case Translations.Command.SecurityC2:
									case Translations.Command.SecurityD:
									case Translations.Command.SecurityE:
									case Translations.Command.SecurityF:
									case Translations.Command.SecurityG:
									case Translations.Command.SecurityH:
									case Translations.Command.SecurityI:
									case Translations.Command.SecurityJ:
									case Translations.Command.SecurityK:
									case Translations.Command.SecurityL:
									case Translations.Command.SecurityM:
									case Translations.Command.SecurityN:
									case Translations.Command.SecurityO:
									case Translations.Command.SecurityP:
#pragma warning restore 618
									case Translations.Command.FillFuel:
									case Translations.Command.LiveSteamInjector:
									case Translations.Command.ExhaustSteamInjector:
									case Translations.Command.IncreaseCutoff:
									case Translations.Command.DecreaseCutoff:
									case Translations.Command.Blowers:
									case Translations.Command.EngineStart:
									case Translations.Command.EngineStop:
									case Translations.Command.GearUp:
									case Translations.Command.GearDown:
									case Translations.Command.RaisePantograph:
									case Translations.Command.LowerPantograph:
										if (TrainManager.PlayerTrain.Plugin != null)
										{
											TrainManager.PlayerTrain.Plugin.KeyDown(
												Translations.SecurityToVirtualKey(Interface.CurrentControls[i].Command));
										}

										break;
									case Translations.Command.Headlights:
										if (TrainManager.PlayerTrain.Plugin != null)
										{
											TrainManager.PlayerTrain.Plugin.KeyDown(
												Translations.SecurityToVirtualKey(Interface.CurrentControls[i].Command));
										}
										TrainManager.PlayerTrain.SafetySystems.Headlights.ChangeState();
										break;
									case Translations.Command.MainBreaker:
										if (TrainManager.PlayerTrain.Plugin != null)
										{
											TrainManager.PlayerTrain.Plugin.KeyDown(
												Translations.SecurityToVirtualKey(Interface.CurrentControls[i].Command));
										}
										break;
									case Translations.Command.WiperSpeedUp:
									case Translations.Command.WiperSpeedDown:
										if (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Windscreen != null)
										{
											TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Windscreen.Wipers.ChangeSpeed(Interface.CurrentControls[i].Command);
										}
										if (TrainManager.PlayerTrain.Plugin != null)
										{
											//Also inform the plugin that these keys have been pressed
											TrainManager.PlayerTrain.Plugin.KeyDown(
												Translations.SecurityToVirtualKey(Interface.CurrentControls[i].Command));
										}
										break;
									case Translations.Command.Sanders:
										if (TrainManager.PlayerTrain.Plugin != null)
										{
											TrainManager.PlayerTrain.Plugin.KeyDown(
												Translations.SecurityToVirtualKey(Interface.CurrentControls[i].Command));
										}

										for (int c = 0; c < TrainManager.PlayerTrain.Cars.Length; c++)
										{
											if (TrainManager.PlayerTrain.Cars[c].ReAdhesionDevice is Sanders sanders)
											{
												sanders.Toggle();
											}
										}
										break;
									case Translations.Command.TimetableToggle:
										// option: timetable
										if (Interface.CurrentOptions.TimeTableStyle == TimeTableMode.None)
										{
											//We have selected not to display a timetable at all
											break;
										}
										if (Interface.CurrentOptions.TimeTableStyle == TimeTableMode.AutoGenerated || !Timetable.CustomTimetableAvailable)
										{
											//Either the auto-generated timetable has been selected as the preference, or no custom timetable has been supplied
											switch (Program.Renderer.CurrentTimetable)
											{
												case DisplayedTimetable.Default:
													Program.Renderer.CurrentTimetable = DisplayedTimetable.None;
													break;
												default:
													Program.Renderer.CurrentTimetable = DisplayedTimetable.Default;
													break;
											}
											break;
										}
										if (Interface.CurrentOptions.TimeTableStyle == TimeTableMode.PreferCustom)
										{
											//We have already determined that a custom timetable is available in the if above
											Program.Renderer.CurrentTimetable = Program.Renderer.CurrentTimetable != DisplayedTimetable.Custom ? DisplayedTimetable.Custom : DisplayedTimetable.None;
											break;
										}
										//Fallback legacy behaviour- Cycles between custom, default and none
										switch (Program.Renderer.CurrentTimetable)
										{
											case DisplayedTimetable.Custom:
												Program.Renderer.CurrentTimetable = DisplayedTimetable.Default;
												break;
											case DisplayedTimetable.Default:
												Program.Renderer.CurrentTimetable = DisplayedTimetable.None;
												break;
											default:
												Program.Renderer.CurrentTimetable = DisplayedTimetable.Custom;
												break;
										}
										break;
									case Translations.Command.DebugWireframe:
										Program.Renderer.OptionWireFrame = !Program.Renderer.OptionWireFrame;
										break;
									case Translations.Command.DebugNormals:
										// option: normals
										Program.Renderer.OptionNormals = !Program.Renderer.OptionNormals;
										break;
									case Translations.Command.DebugTouchMode:
										Program.Renderer.DebugTouchMode = !Program.Renderer.DebugTouchMode;
										break;
									case Translations.Command.ShowEvents:
										Interface.CurrentOptions.ShowEvents = !Interface.CurrentOptions.ShowEvents;
										break;
									case Translations.Command.DebugRendererMode:
										Interface.CurrentOptions.IsUseNewRenderer = !Interface.CurrentOptions.IsUseNewRenderer;
										MessageManager.AddMessage($"Renderer mode: {(Program.Renderer.AvailableNewRenderer ? "New renderer" : "Original renderer")}", MessageDependency.None, GameMode.Expert, MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
										break;
									case Translations.Command.MiscAI:
										// option: AI
										if (Interface.CurrentOptions.GameMode == GameMode.Expert)
										{
											MessageManager.AddMessage(
												Translations.GetInterfaceString("notification_notavailableexpert"),
												MessageDependency.None, GameMode.Expert,
												MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
										}
										else
										{
											if (TrainManager.PlayerTrain.AI == null)
											{
												TrainManager.PlayerTrain.AI =
													new Game.SimpleHumanDriverAI(TrainManager.PlayerTrain, Double.PositiveInfinity);
												if (TrainManager.PlayerTrain.Plugin != null && TrainManager.PlayerTrain.Plugin.SupportsAI == AISupport.None)
												{
													MessageManager.AddMessage(
														Translations.GetInterfaceString("notification_aiunable"),
														MessageDependency.None, GameMode.Expert,
														MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
												}
											}
											else
											{
												TrainManager.PlayerTrain.AI = null;
											}
										}
										break;
									case Translations.Command.MiscInterfaceMode:
										// option: debug
										switch (Program.Renderer.CurrentOutputMode)
										{
											case OutputMode.Default:
												Program.Renderer.CurrentOutputMode = Interface.CurrentOptions.GameMode ==
																			 GameMode.Expert
													? OutputMode.None
													: OutputMode.Debug;
												break;
											case OutputMode.Debug:
												Program.Renderer.CurrentOutputMode = OutputMode.None;
												break;
											case OutputMode.DebugATS:
												Program.Renderer.CurrentOutputMode = Program.Renderer.PreviousOutputMode;
												break;
											default:
												Program.Renderer.CurrentOutputMode = OutputMode.Default;
												break;
										}
										Program.Renderer.PreviousOutputMode = Program.Renderer.CurrentOutputMode;
										break;
									case Translations.Command.DebugATS:
										if (Program.Renderer.CurrentOutputMode == OutputMode.DebugATS)
										{
											Program.Renderer.CurrentOutputMode = Program.Renderer.PreviousOutputMode;
										}
										else
										{
											Program.Renderer.PreviousOutputMode = Program.Renderer.CurrentOutputMode;
											Program.Renderer.CurrentOutputMode = OutputMode.DebugATS;
										}
										break;
									case Translations.Command.MiscBackfaceCulling:
										// option: backface culling
										Program.Renderer.OptionBackFaceCulling = !Program.Renderer.OptionBackFaceCulling;
										MessageManager.AddMessage(
											Translations.GetInterfaceString(Program.Renderer.OptionBackFaceCulling
												? "notification_backfaceculling_on"
												: "notification_backfaceculling_off"), MessageDependency.None,
											GameMode.Expert, MessageColor.White,
											Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
										break;
									case Translations.Command.MiscCPUMode:
										// option: limit frame rate
										LimitFramerate = !LimitFramerate;
										MessageManager.AddMessage(
											Translations.GetInterfaceString(LimitFramerate
												? "notification_cpu_low"
												: "notification_cpu_normal"), MessageDependency.None,
											GameMode.Expert, MessageColor.White,
											Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
										break;
									case Translations.Command.DebugBrakeSystems:
										// option: brake systems
										if (Interface.CurrentOptions.GameMode == GameMode.Expert)
										{
											MessageManager.AddMessage(
												Translations.GetInterfaceString("notification_notavailableexpert"),
												MessageDependency.None, GameMode.Expert,
												MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
										}
										else
										{
											Program.Renderer.OptionBrakeSystems = !Program.Renderer.OptionBrakeSystems;
										}
										break;
									case Translations.Command.MenuActivate:
										// menu
										Game.Menu.PushMenu(MenuType.Top);
										break;
									case Translations.Command.MiscPause:
										// pause
										Program.Renderer.CurrentInterface = InterfaceType.Pause;
										break;
									case Translations.Command.MiscClock:
										// clock
										Program.Renderer.OptionClock = !Program.Renderer.OptionClock;
										break;
									case Translations.Command.MiscTimeFactor:
										// time factor
										if (TrainManager.PlayerTrain.Plugin != null && !TrainManager.PlayerTrain.Plugin.DisableTimeAcceleration)
										{
											if (Interface.CurrentOptions.GameMode == GameMode.Expert)
											{
												MessageManager.AddMessage(
													Translations.GetInterfaceString("notification_notavailableexpert"),
													MessageDependency.None, GameMode.Expert,
													MessageColor.White,
													Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
											}
											else
											{
												TimeFactor = TimeFactor == 1
													? Interface.CurrentOptions.TimeAccelerationFactor
													: 1;
												MessageManager.AddMessage(
													TimeFactor.ToString(
														System.Globalization.CultureInfo.InvariantCulture) + "x",
													MessageDependency.None, GameMode.Expert,
													MessageColor.White,
													Program.CurrentRoute.SecondsSinceMidnight + 5.0*(double) TimeFactor, null);
											}
										}
										break;
									case Translations.Command.MiscSpeed:
										// speed
										if (Interface.CurrentOptions.GameMode == GameMode.Expert)
										{
											MessageManager.AddMessage(
												Translations.GetInterfaceString("notification_notavailableexpert"),
												MessageDependency.None, GameMode.Expert,
												MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
										}
										else
										{
											Program.Renderer.OptionSpeed++;
											if ((int)Program.Renderer.OptionSpeed >= 3) Program.Renderer.OptionSpeed = 0;
										}
										break;
									case Translations.Command.MiscGradient:
										// gradient
										if (Interface.CurrentOptions.GameMode == GameMode.Expert)
										{
											MessageManager.AddMessage(
												Translations.GetInterfaceString("notification_notavailableexpert"),
												MessageDependency.None, GameMode.Expert,
												MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
										}
										else
										{
											Program.Renderer.OptionGradient++;
											if ((int)Program.Renderer.OptionGradient >= 4) Program.Renderer.OptionGradient = 0;
										}
										break;
									case Translations.Command.MiscDistanceToNextStation:
										if (Interface.CurrentOptions.GameMode == GameMode.Expert)
										{
											MessageManager.AddMessage(
												Translations.GetInterfaceString("notification_notavailableexpert"),
												MessageDependency.None, GameMode.Expert,
												MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
										}
										else
										{
											Program.Renderer.OptionDistanceToNextStation++;
											if ((int)Program.Renderer.OptionDistanceToNextStation >= 3) Program.Renderer.OptionDistanceToNextStation = 0;
										}
										break;
									case Translations.Command.MiscFps:
										// fps
										Program.Renderer.OptionFrameRates = !Program.Renderer.OptionFrameRates;
										break;
									case Translations.Command.MiscFullscreen:
										// toggle fullscreen
										Screen.ToggleFullscreen();
										Interface.CurrentControls[i].AnalogState = 0.0;
										Interface.CurrentControls[i].DigitalState = DigitalControlState.Released;
										RemoveControlRepeat(i);
										break;
									case Translations.Command.MiscMute:
										// mute
										Program.Sounds.GlobalMute = !Program.Sounds.GlobalMute;
										Program.Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
										break;
								case Translations.Command.RouteInformation:
									Game.routeInfoOverlay.ProcessCommand(Translations.Command.RouteInformation);
									break;
								case Translations.Command.AccessibilityCurrentSpeed:
									string s = Translations.GetInterfaceString("message_train_currentspeed").Replace("[speed]", $"{TrainManagerBase.PlayerTrain.CurrentSpeed * 3.6:0.0}") + "km/h";
									Program.CurrentHost.AddMessage(s, MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, Program.CurrentHost.InGameTime + 10.0, null);
									break;
								case Translations.Command.AccessibilityNextSignal:
									Section nextSection = TrainManagerBase.CurrentRoute.NextSection(TrainManagerBase.PlayerTrain.FrontCarTrackPosition);
									if (nextSection != null)
									{
										double tPos = nextSection.TrackPosition - TrainManagerBase.PlayerTrain.FrontCarTrackPosition;
										string st = Translations.GetInterfaceString("message_route_nextsection_aspect").Replace("[distance]", $"{tPos:0.0}") + "m".Replace("[aspect]", nextSection.CurrentAspect.ToString());
										Program.CurrentHost.AddMessage(st, MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, Program.CurrentHost.InGameTime + 10.0, null);
									}
									break; 
								case Translations.Command.AccessibilityNextStation:
									RouteStation nextStation = TrainManagerBase.CurrentRoute.NextStation(TrainManagerBase.PlayerTrain.FrontCarTrackPosition);
									if (nextStation != null)
									{
										//If we find an appropriate signal, and the distance to it is less than 500m, announce if screen reader is present
										//Aspect announce to be triggered via a separate keybind
										double tPos = nextStation.DefaultTrackPosition - TrainManagerBase.PlayerTrain.FrontCarTrackPosition;
										string stt = Translations.GetInterfaceString("message_route_nextstation").Replace("[distance]", $"{tPos:0.0}") + "m".Replace("[name]", nextStation.Name);
										Program.CurrentHost.AddMessage(stt, MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, Program.CurrentHost.InGameTime + 10.0, null);
										nextStation.AccessibilityAnnounced = true;
									}
									break;
								}
							}
							else if (Interface.CurrentControls[i].DigitalState == DigitalControlState.Released)
							{
								// released
								Interface.CurrentControls[i].DigitalState =
									DigitalControlState.ReleasedAcknowledged;
								switch (Interface.CurrentControls[i].Command)
								{
									case Translations.Command.SingleBrake:
									case Translations.Command.BrakeIncrease:
									case Translations.Command.BrakeDecrease:
										TrainManager.PlayerTrain.Handles.Brake.ContinuousMovement = false;
										break;
									case Translations.Command.SinglePower:
									case Translations.Command.PowerIncrease:
									case Translations.Command.PowerDecrease:
										TrainManager.PlayerTrain.Handles.Power.ContinuousMovement = false;
										break;
									case Translations.Command.SingleNeutral:
										TrainManager.PlayerTrain.Handles.Brake.ContinuousMovement = false;
										TrainManager.PlayerTrain.Handles.Power.ContinuousMovement = false;
										break;

									/*
									 * Keys after this point are used by the plugin API
									 *
									 */
//We only want to mark these as obsolete for new users of the API
#pragma warning disable 618
									case Translations.Command.SecurityS:
									case Translations.Command.SecurityA1:
									case Translations.Command.SecurityA2:
									case Translations.Command.SecurityB1:
									case Translations.Command.SecurityB2:
									case Translations.Command.SecurityC1:
									case Translations.Command.SecurityC2:
									case Translations.Command.SecurityD:
									case Translations.Command.SecurityE:
									case Translations.Command.SecurityF:
									case Translations.Command.SecurityG:
									case Translations.Command.SecurityH:
									case Translations.Command.SecurityI:
									case Translations.Command.SecurityJ:
									case Translations.Command.SecurityK:
									case Translations.Command.SecurityL:
									case Translations.Command.SecurityM:
									case Translations.Command.SecurityN:
									case Translations.Command.SecurityO:
									case Translations.Command.SecurityP:
#pragma warning restore 618
									case Translations.Command.WiperSpeedUp:
									case Translations.Command.WiperSpeedDown:
									case Translations.Command.FillFuel:
									case Translations.Command.LiveSteamInjector:
									case Translations.Command.ExhaustSteamInjector:
									case Translations.Command.IncreaseCutoff:
									case Translations.Command.DecreaseCutoff:
									case Translations.Command.Blowers:
									case Translations.Command.EngineStart:
									case Translations.Command.EngineStop:
									case Translations.Command.GearUp:
									case Translations.Command.GearDown:
									case Translations.Command.RaisePantograph:
									case Translations.Command.LowerPantograph:
									case Translations.Command.MainBreaker:
										if (TrainManager.PlayerTrain.Plugin != null)
										{
											TrainManager.PlayerTrain.Plugin.KeyUp(
												Translations.SecurityToVirtualKey(Interface.CurrentControls[i].Command));
										}
										break;

									case Translations.Command.HornPrimary:
									case Translations.Command.HornSecondary:
									case Translations.Command.HornMusic:
										// horn
										int j = Interface.CurrentControls[i].Command == Translations.Command.HornPrimary
											? 0
											: Interface.CurrentControls[i].Command == Translations.Command.HornSecondary ? 1 : 2;
										int d = TrainManager.PlayerTrain.DriverCar;
										if (TrainManager.PlayerTrain.Cars[d].Horns.Length > j)
										{
											//Required for detecting the end of the loop and triggering the stop sound
											TrainManager.PlayerTrain.Cars[d].Horns[j].Stop();
										}
										break;
									case Translations.Command.DoorsLeft:
										TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Doors[0].ButtonPressed = false;
										if (TrainManager.PlayerTrain.Plugin != null)
										{
											TrainManager.PlayerTrain.Plugin.KeyUp(VirtualKeys.LeftDoors);
										}
										break;
									case Translations.Command.DoorsRight:
										TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Doors[1].ButtonPressed = false;
										if (TrainManager.PlayerTrain.Plugin != null)
										{
											TrainManager.PlayerTrain.Plugin.KeyUp(VirtualKeys.RightDoors);
										}
										break;
									case Translations.Command.RailDriverSpeedUnits:
										Interface.CurrentOptions.RailDriverMPH = !Interface.CurrentOptions.RailDriverMPH;
										break;
									case Translations.Command.Sanders:
										for (int c = 0; c < TrainManager.PlayerTrain.Cars.Length; c++)
										{
											if (TrainManager.PlayerTrain.Cars[c].ReAdhesionDevice is Sanders sanders)
											{
												if (sanders.Type == SandersType.PressAndHold)
												{
													sanders.Toggle();
												}
											}
										}
										if (TrainManager.PlayerTrain.Plugin != null)
										{
											TrainManager.PlayerTrain.Plugin.KeyUp(VirtualKeys.Sanders);
										}
										break;
								}
							}
						}
					}
					break;
			}
		}

	}
}
