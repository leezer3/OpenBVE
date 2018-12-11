﻿using System;
using OpenBveApi.Colors;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenBveApi.Interface;
using OpenBveShared;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
	internal static partial class MainLoop
	{
		/// <summary>The ProcessControls function should be called once a frame, and updates the simulation accordingly</summary>
		/// <param name="TimeElapsed">The time elapsed in ms since the last call to this function</param>
		internal static void ProcessControls(double TimeElapsed)
		{
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
					World.CameraCar = TrainManager.PlayerTrain.DriverCar;
					MainLoop.SaveCameraSettings();
					bool lookahead = false;
					if (Camera.CameraView != CameraViewMode.InteriorLookAhead & Camera.CameraRestriction == CameraRestrictionMode.NotAvailable)
					{
						Game.AddMessage(Translations.GetInterfaceString("notification_interior_lookahead"),
							MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
							MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
						lookahead = true;
					}
					else
					{
						Game.AddMessage(Translations.GetInterfaceString("notification_interior"),
							MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
							MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
					}

					Camera.CameraView = CameraViewMode.Interior;
					MainLoop.RestoreCameraSettings();
					bool returnToCab = false;
					for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					{
						if (j == World.CameraCar)
						{
							if (TrainManager.PlayerTrain.Cars[j].HasInteriorView)
							{
								TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.Interior);
								Camera.CameraRestriction = TrainManager.PlayerTrain.Cars[j].CameraRestrictionMode;
							}
							else
							{
								TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.NotVisible);
								returnToCab = true;
							}
						}
						else
						{
							TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.NotVisible);
						}
					}

					if (returnToCab)
					{
						//If our selected car does not have an interior view, we must store this fact, and return to the driver car after the loop has finished
						World.CameraCar = TrainManager.PlayerTrain.DriverCar;
						TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].ChangeCarSection(TrainManager.CarSectionType.Interior);
						Camera.CameraRestriction = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestrictionMode;
					}

					//Hide bogies
					for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					{
						TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(-1);
						TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(-1);
					}

					Camera.CameraAlignmentDirection = new CameraAlignment();
					Camera.CameraAlignmentSpeed = new CameraAlignment();
					OpenBveShared.Renderer.UpdateViewport(OpenBveShared.Renderer.ViewPortChangeMode.NoChange);
					World.UpdateAbsoluteCamera(TimeElapsed);
					World.UpdateViewingDistances();
					if (Camera.CameraRestriction != CameraRestrictionMode.NotAvailable)
					{
						if (!World.PerformCameraRestrictionTest())
						{
							World.InitializeCameraRestriction();
						}
					}

					if (lookahead)
					{
						Camera.CameraView = CameraViewMode.InteriorLookAhead;
					}
					TrainManager.PlayerTrain.AI = new Game.SimpleHumanDriverAI(TrainManager.PlayerTrain);
					if (TrainManager.PlayerTrain.Plugin != null && !TrainManager.PlayerTrain.Plugin.SupportsAI)
					{
						Game.AddMessage(Translations.GetInterfaceString("notification_aiunable"), MessageManager.MessageDependency.None, Interface.GameMode.Expert, MessageColor.White, Game.SecondsSinceMidnight + 10.0, null);
					}

				}
			}
			//If we are currently blocking key repeat events from firing, return
			if (BlockKeyRepeat) return;
			switch (Game.CurrentInterface)
			{
				case Game.InterfaceType.Pause:
					// pause
					kioskModeTimer = 0;
					for (int i = 0; i < Interface.CurrentControls.Length; i++)
					{
						if (Interface.CurrentControls[i].InheritedType == Translations.CommandType.Digital)
						{
							if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Pressed)
							{
								Interface.CurrentControls[i].DigitalState =
									Interface.DigitalControlState.PressedAcknowledged;
								switch (Interface.CurrentControls[i].Command)
								{
									case Translations.Command.MiscPause:
										Game.CurrentInterface = Game.InterfaceType.Normal;
										break;
									case Translations.Command.MenuActivate:
										Game.Menu.PushMenu(Menu.MenuType.Top);
										break;
									case Translations.Command.MiscQuit:
										Game.Menu.PushMenu(Menu.MenuType.Quit);
										break;
									case Translations.Command.MiscFullscreen:
										Screen.ToggleFullscreen();
										break;
									case Translations.Command.MiscMute:
										Sounds.GlobalMute = !Sounds.GlobalMute;
										Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
										break;
								}
							}
						}
					}
					break;
/*
				case Game.InterfaceType.CustomiseControl:
					break;
*/
				case Game.InterfaceType.Menu:			// MENU
					kioskModeTimer = 0;
					for (int i = 0; i < Interface.CurrentControls.Length; i++)
					{
						if (Interface.CurrentControls[i].InheritedType == Translations.CommandType.Digital
								&& Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Pressed)
						{
							Interface.CurrentControls[i].DigitalState =
									Interface.DigitalControlState.PressedAcknowledged;
							Game.Menu.ProcessCommand(Interface.CurrentControls[i].Command, TimeElapsed);
						}
					}
					break;

				case Game.InterfaceType.Normal:
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
										// power half/full-axis
										if (!TrainManager.PlayerTrain.Handles.SingleHandle)
										{
											double a = Interface.CurrentControls[i].AnalogState;
											if (Interface.CurrentControls[i].Command == Translations.Command.BrakeFullAxis)
											{
												a = 0.5*(a + 1.0);
											}
											a *= (double) TrainManager.PlayerTrain.Handles.Power.MaximumNotch;
											int p = (int) Math.Round(a);
											TrainManager.PlayerTrain.ApplyNotch(p, false, 0, true);
										}
										break;
									case Translations.Command.BrakeHalfAxis:
									case Translations.Command.BrakeFullAxis:
										// brake half/full-axis
										if (!TrainManager.PlayerTrain.Handles.SingleHandle)
										{
											if (TrainManager.PlayerTrain.Handles.Brake is TrainManager.AirBrakeHandle)
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
														TrainManager.PlayerTrain.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
														break;
													case 1:
														TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver =
															false;
														TrainManager.PlayerTrain.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Lap);
														break;
													case 2:
														TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver =
															false;
														TrainManager.PlayerTrain.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Service);
														break;
													case 3:
														if (Interface.CurrentOptions.AllowAxisEB)
														{
															TrainManager.PlayerTrain.ApplyEmergencyBrake();
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
														TrainManager.PlayerTrain.UnapplyEmergencyBrake();
														TrainManager.PlayerTrain.ApplyNotch(0, true, b,
															false);
													}
													else
													{
														TrainManager.PlayerTrain.ApplyEmergencyBrake();
													}
													TrainManager.PlayerTrain.ApplyHoldBrake(q);
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
														TrainManager.PlayerTrain.UnapplyEmergencyBrake();
														TrainManager.PlayerTrain.ApplyNotch(0, true, b,
															false);
													}
													else
													{
														if (Interface.CurrentOptions.AllowAxisEB)
														{
															TrainManager.PlayerTrain.ApplyEmergencyBrake();
														}
													}
												}
											}
										}
										break;
									case Translations.Command.SingleFullAxis:
										// single full axis
										if (TrainManager.PlayerTrain.Handles.SingleHandle)
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
													TrainManager.PlayerTrain.UnapplyEmergencyBrake();
													TrainManager.PlayerTrain.ApplyNotch(p, false, b, false);
												}
												else
												{
													if (Interface.CurrentOptions.AllowAxisEB)
													{
														TrainManager.PlayerTrain.ApplyEmergencyBrake();
													}
												}
												TrainManager.PlayerTrain.ApplyHoldBrake(q);
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
													TrainManager.PlayerTrain.UnapplyEmergencyBrake();
													TrainManager.PlayerTrain.ApplyNotch(p, false, b, false);
												}
												else
												{
													if (Interface.CurrentOptions.AllowAxisEB)
													{
														TrainManager.PlayerTrain.ApplyEmergencyBrake();
													}
												}
											}
										}
										break;
									case Translations.Command.ReverserFullAxis:
										// reverser full axis
									{
										double a = Interface.CurrentControls[i].AnalogState;
										int r = (int) Math.Round(a);
										TrainManager.PlayerTrain.ApplyReverser(r, false);
									}
										break;
									case Translations.Command.CameraMoveForward:
										// camera move forward
										if (Camera.CameraView == CameraViewMode.Interior |
											Camera.CameraView == CameraViewMode.InteriorLookAhead |
											Camera.CameraView == CameraViewMode.Exterior)
										{
											double s = Camera.CameraView == CameraViewMode.Interior |
													   Camera.CameraView == CameraViewMode.InteriorLookAhead
												? Camera.CameraInteriorTopSpeed
												: Camera.CameraExteriorTopSpeed;
											Camera.CameraAlignmentDirection.Position.Z = s*
																						Interface.CurrentControls[i]
																							.AnalogState;
										}
										else
										{
											if (World.CameraAtWorldEnd)
											{
												//Don't let the camera run off the end of the worldspace
												break;
											}
											Camera.CameraAlignmentDirection.TrackPosition = Camera.CameraExteriorTopSpeed*
																						   Interface.CurrentControls[i]
																							   .AnalogState;
										}
										break;
									case Translations.Command.CameraMoveBackward:
										// camera move backward
										if (Camera.CameraView == CameraViewMode.Interior |
											Camera.CameraView == CameraViewMode.InteriorLookAhead |
											Camera.CameraView == CameraViewMode.Exterior)
										{
											double s = Camera.CameraView == CameraViewMode.Interior |
													   Camera.CameraView == CameraViewMode.InteriorLookAhead
												? Camera.CameraInteriorTopSpeed
												: Camera.CameraExteriorTopSpeed;
											Camera.CameraAlignmentDirection.Position.Z = -s*
																						Interface.CurrentControls[i]
																							.AnalogState;
										}
										else
										{
											Camera.CameraAlignmentDirection.TrackPosition =
												-Camera.CameraExteriorTopSpeed*Interface.CurrentControls[i].AnalogState;
										}
										break;
									case Translations.Command.CameraMoveLeft:
										// camera move left
									{
										double s = Camera.CameraView == CameraViewMode.Interior |
												   Camera.CameraView == CameraViewMode.InteriorLookAhead
											? Camera.CameraInteriorTopSpeed
											: Camera.CameraExteriorTopSpeed;
										Camera.CameraAlignmentDirection.Position.X = -s*
																					Interface.CurrentControls[i]
																						.AnalogState;
									}
										break;
									case Translations.Command.CameraMoveRight:
										// camera move right
									{
										double s = Camera.CameraView == CameraViewMode.Interior |
												   Camera.CameraView == CameraViewMode.InteriorLookAhead
											? Camera.CameraInteriorTopSpeed
											: Camera.CameraExteriorTopSpeed;
										Camera.CameraAlignmentDirection.Position.X = s*
																					Interface.CurrentControls[i]
																						.AnalogState;
									}
										break;
									case Translations.Command.CameraMoveUp:
										// camera move up
									{
										double s = Camera.CameraView == CameraViewMode.Interior |
												   Camera.CameraView == CameraViewMode.InteriorLookAhead
											? Camera.CameraInteriorTopSpeed
											: Camera.CameraExteriorTopSpeed;
										Camera.CameraAlignmentDirection.Position.Y = s*
																					Interface.CurrentControls[i]
																						.AnalogState;
									}
										break;
									case Translations.Command.CameraMoveDown:
										// camera move down
									{
										double s = Camera.CameraView == CameraViewMode.Interior |
												   Camera.CameraView == CameraViewMode.InteriorLookAhead
											? Camera.CameraInteriorTopSpeed
											: Camera.CameraExteriorTopSpeed;
										Camera.CameraAlignmentDirection.Position.Y = -s*
																					Interface.CurrentControls[i]
																						.AnalogState;
									}
										break;
									case Translations.Command.CameraRotateLeft:
										// camera rotate left
									{
										double s = Camera.CameraView == CameraViewMode.Interior |
												   Camera.CameraView == CameraViewMode.InteriorLookAhead
											? Camera.CameraInteriorTopAngularSpeed
											: Camera.CameraExteriorTopAngularSpeed;
										Camera.CameraAlignmentDirection.Yaw = -s*Interface.CurrentControls[i].AnalogState;
									}
										break;
									case Translations.Command.CameraRotateRight:
										// camera rotate right
									{
										double s = Camera.CameraView == CameraViewMode.Interior |
												   Camera.CameraView == CameraViewMode.InteriorLookAhead
											? Camera.CameraInteriorTopAngularSpeed
											: Camera.CameraExteriorTopAngularSpeed;
										Camera.CameraAlignmentDirection.Yaw = s*Interface.CurrentControls[i].AnalogState;
									}
										break;
									case Translations.Command.CameraRotateUp:
										// camera rotate up
									{
										double s = Camera.CameraView == CameraViewMode.Interior |
												   Camera.CameraView == CameraViewMode.InteriorLookAhead
											? Camera.CameraInteriorTopAngularSpeed
											: Camera.CameraExteriorTopAngularSpeed;
										Camera.CameraAlignmentDirection.Pitch = s*
																			   Interface.CurrentControls[i].AnalogState;
									}
										break;
									case Translations.Command.CameraRotateDown:
										// camera rotate down
									{
										double s = Camera.CameraView == CameraViewMode.Interior |
												   Camera.CameraView == CameraViewMode.InteriorLookAhead
											? Camera.CameraInteriorTopAngularSpeed
											: Camera.CameraExteriorTopAngularSpeed;
										Camera.CameraAlignmentDirection.Pitch = -s*
																			   Interface.CurrentControls[i].AnalogState;
									}
										break;
									case Translations.Command.CameraRotateCCW:
										// camera rotate ccw
										if ((Camera.CameraView != CameraViewMode.Interior &
											 Camera.CameraView != CameraViewMode.InteriorLookAhead) |
											Camera.CameraRestriction != CameraRestrictionMode.On)
										{
											double s = Camera.CameraView == CameraViewMode.Interior |
													   Camera.CameraView == CameraViewMode.InteriorLookAhead
												? Camera.CameraInteriorTopAngularSpeed
												: Camera.CameraExteriorTopAngularSpeed;
											Camera.CameraAlignmentDirection.Roll = -s*
																				  Interface.CurrentControls[i]
																					  .AnalogState;
										}
										break;
									case Translations.Command.CameraRotateCW:
										// camera rotate cw
										if ((Camera.CameraView != CameraViewMode.Interior &
											 Camera.CameraView != CameraViewMode.InteriorLookAhead) |
											Camera.CameraRestriction != CameraRestrictionMode.On)
										{
											double s = Camera.CameraView == CameraViewMode.Interior |
													   Camera.CameraView == CameraViewMode.InteriorLookAhead
												? Camera.CameraInteriorTopAngularSpeed
												: Camera.CameraExteriorTopAngularSpeed;
											Camera.CameraAlignmentDirection.Roll = s*
																				  Interface.CurrentControls[i]
																					  .AnalogState;
										}
										break;
									case Translations.Command.CameraZoomIn:
										// camera zoom in
										if (TimeElapsed > 0.0)
										{
											Camera.CameraAlignmentDirection.Zoom = -Camera.CameraZoomTopSpeed*
																				  Interface.CurrentControls[i]
																					  .AnalogState;
										}
										break;
									case Translations.Command.CameraZoomOut:
										// camera zoom out
										if (TimeElapsed > 0.0)
										{
											Camera.CameraAlignmentDirection.Zoom = Camera.CameraZoomTopSpeed*
																				  Interface.CurrentControls[i]
																					  .AnalogState;
										}
										break;
									case Translations.Command.TimetableUp:
										// timetable up
										if (TimeElapsed > 0.0)
										{
											const double scrollSpeed = 250.0;
											if (Timetable.CurrentTimetable == Timetable.TimetableState.Default)
											{
												Timetable.DefaultTimetablePosition += scrollSpeed*
																					  Interface.CurrentControls[i]
																						  .AnalogState*TimeElapsed;
												if (Timetable.DefaultTimetablePosition > 0.0)
													Timetable.DefaultTimetablePosition = 0.0;
											}
											else if (Timetable.CurrentTimetable == Timetable.TimetableState.Custom)
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
											if (Timetable.CurrentTimetable == Timetable.TimetableState.Default)
											{
												Timetable.DefaultTimetablePosition -= scrollSpeed*
																					  Interface.CurrentControls[i]
																						  .AnalogState*TimeElapsed;
												double max;
												if (Timetable.DefaultTimetableTexture != null)
												{
													Textures.LoadTexture(Timetable.DefaultTimetableTexture,
														OpenGlTextureWrapMode.ClampClamp);
													max =
														Math.Min(
															OpenBveShared.Renderer.Height - Timetable.DefaultTimetableTexture.Height,
															0.0);
												}
												else
												{
													max = 0.0;
												}
												if (Timetable.DefaultTimetablePosition < max)
													Timetable.DefaultTimetablePosition = max;
											}
											else if (Timetable.CurrentTimetable == Timetable.TimetableState.Custom)
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
													Textures.LoadTexture(texture,
														OpenGlTextureWrapMode.ClampClamp);
													max = Math.Min(OpenBveShared.Renderer.Height - texture.Height, 0.0);
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
							if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Pressed)
							{
								// pressed
								Interface.CurrentControls[i].DigitalState =
									Interface.DigitalControlState.PressedAcknowledged;
								switch (Interface.CurrentControls[i].Command)
								{
									case Translations.Command.MiscQuit:
										// quit
										Game.Menu.PushMenu(Menu.MenuType.Quit);
										break;
									case Translations.Command.CameraInterior:
										// camera: interior
										MainLoop.SaveCameraSettings();
										bool lookahead = false;
										if (Camera.CameraView != CameraViewMode.InteriorLookAhead & Camera.CameraRestriction == CameraRestrictionMode.NotAvailable)
										{
											Game.AddMessage(Translations.GetInterfaceString("notification_interior_lookahead"),
												MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											lookahead = true;
										}
										else
										{
											Game.AddMessage(Translations.GetInterfaceString("notification_interior"),
												MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
										}
										Camera.CameraView = CameraViewMode.Interior;
										MainLoop.RestoreCameraSettings();
										bool returnToCab = false;
										for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
										{
											if (j == World.CameraCar)
											{
												if (TrainManager.PlayerTrain.Cars[j].HasInteriorView)
												{
													TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.Interior);
													Camera.CameraRestriction = TrainManager.PlayerTrain.Cars[j].CameraRestrictionMode;
												}
												else
												{
													TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.NotVisible);
													returnToCab = true;
												}
											}
											else
											{
												TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.NotVisible);
											}
										}
										if (returnToCab)
										{
											//If our selected car does not have an interior view, we must store this fact, and return to the driver car after the loop has finished
											World.CameraCar = TrainManager.PlayerTrain.DriverCar;
											TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].ChangeCarSection(TrainManager.CarSectionType.Interior);
											Camera.CameraRestriction = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestrictionMode;
										}
										//Hide bogies
										for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
										{
											TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(-1);
											TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(-1);
										}
										Camera.CameraAlignmentDirection = new CameraAlignment();
										Camera.CameraAlignmentSpeed = new CameraAlignment();
										OpenBveShared.Renderer.UpdateViewport(OpenBveShared.Renderer.ViewPortChangeMode.NoChange);
										World.UpdateAbsoluteCamera(TimeElapsed);
										World.UpdateViewingDistances();
										if (Camera.CameraRestriction != CameraRestrictionMode.NotAvailable)
										{
											if (!World.PerformCameraRestrictionTest())
											{
												World.InitializeCameraRestriction();
											}
										}
										if (lookahead)
										{
											Camera.CameraView = CameraViewMode.InteriorLookAhead;
										}
										break;
									case Translations.Command.CameraExterior:
										// camera: exterior
										Game.AddMessage(Translations.GetInterfaceString("notification_exterior") + " " + (World.CameraCar + 1), MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
										SaveCameraSettings();
										Camera.CameraView = CameraViewMode.Exterior;
										RestoreCameraSettings();
										for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
										{
											TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.Exterior);
										}
										//Make bogies visible
										for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
										{
											TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
											TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
										}
										Camera.CameraAlignmentDirection = new CameraAlignment();
										Camera.CameraAlignmentSpeed = new CameraAlignment();
										OpenBveShared.Renderer.UpdateViewport(OpenBveShared.Renderer.ViewPortChangeMode.NoChange);
										World.UpdateAbsoluteCamera(TimeElapsed);
										World.UpdateViewingDistances();
										break;
									case Translations.Command.CameraTrack:
									case Translations.Command.CameraFlyBy:
										// camera: track / fly-by
									{
										SaveCameraSettings();
										if (Interface.CurrentControls[i].Command == Translations.Command.CameraTrack)
										{
											Camera.CameraView = CameraViewMode.Track;
											Game.AddMessage(Translations.GetInterfaceString("notification_track"),
												MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
										}
										else
										{
											if (Camera.CameraView == CameraViewMode.FlyBy)
											{
												Camera.CameraView = CameraViewMode.FlyByZooming;
												Game.AddMessage(
													Translations.GetInterfaceString("notification_flybyzooming"),
													MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
													MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											}
											else
											{
												Camera.CameraView = CameraViewMode.FlyBy;
												Game.AddMessage(
													Translations.GetInterfaceString("notification_flybynormal"),
													MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
													MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											}
										}
										RestoreCameraSettings();
										for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
										{
											TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.Exterior);
										}

										for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
										{
											TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
											TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
										}
										Camera.CameraAlignmentDirection = new CameraAlignment();
										Camera.CameraAlignmentSpeed = new CameraAlignment();
										OpenBveShared.Renderer.UpdateViewport(OpenBveShared.Renderer.ViewPortChangeMode.NoChange);
										World.UpdateAbsoluteCamera(TimeElapsed);
										World.UpdateViewingDistances();
									}
										break;
									case Translations.Command.CameraPreviousPOI:
										//If we are in the exterior train view, shift down one car until we hit the last car
										if (Camera.CameraView == CameraViewMode.Exterior)
										{
											if (World.CameraCar < TrainManager.PlayerTrain.Cars.Length - 1)
											{
												World.CameraCar++;
												Game.AddMessage(Translations.GetInterfaceString("notification_exterior") + " " + (World.CameraCar + 1), MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											}
											return;
										}
										//Otherwise, check if we can move down to the previous POI
										if (Game.ApplyPointOfInterest(-1, true))
										{
											if (Camera.CameraView != CameraViewMode.Track &
												Camera.CameraView != CameraViewMode.FlyBy &
												Camera.CameraView != CameraViewMode.FlyByZooming)
											{
												Camera.CameraView = CameraViewMode.Track;
												Game.AddMessage(Translations.GetInterfaceString("notification_track"),
													MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
													MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											}
											double z = Camera.CameraCurrentAlignment.Position.Z;
											Camera.CameraCurrentAlignment.Position =
												new OpenBveApi.Math.Vector3(Camera.CameraCurrentAlignment.Position.X,
													Camera.CameraCurrentAlignment.Position.Y, 0.0);
											Camera.CameraCurrentAlignment.Zoom = 0.0;
											Camera.CameraAlignmentDirection = new CameraAlignment();
											Camera.CameraAlignmentSpeed = new CameraAlignment();
											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
											{
												TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.Exterior);
											}

											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
											{
												TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
												TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
											}
											World.CameraTrackFollower.Update(TrackManager.CurrentTrack, World.CameraTrackFollower.TrackPosition + z, true, false);
											Camera.CameraCurrentAlignment.TrackPosition = World.CameraTrackFollower.TrackPosition;
											OpenBveShared.World.VerticalViewingAngle = OpenBveShared.World.OriginalVerticalViewingAngle;
											OpenBveShared.Renderer.UpdateViewport(OpenBveShared.Renderer.ViewPortChangeMode.NoChange);
											World.UpdateAbsoluteCamera(TimeElapsed);
											World.UpdateViewingDistances();
										}
										break;
									case Translations.Command.CameraNextPOI:
										//If we are in the exterior train view, shift up one car until we hit index 0
										if (Camera.CameraView == CameraViewMode.Exterior)
										{
											if (World.CameraCar > 0)
											{
												World.CameraCar--;
												Game.AddMessage(Translations.GetInterfaceString("notification_exterior") + " " + (World.CameraCar + 1), MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											}
											return;
										}
										//Otherwise, check if we can move up to the next POI
										if (Game.ApplyPointOfInterest(1, true))
										{
											if (Camera.CameraView != CameraViewMode.Track &
												Camera.CameraView != CameraViewMode.FlyBy &
												Camera.CameraView != CameraViewMode.FlyByZooming)
											{
												Camera.CameraView = CameraViewMode.Track;
												Game.AddMessage(Translations.GetInterfaceString("notification_track"),
													MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
													MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											}
											double z = Camera.CameraCurrentAlignment.Position.Z;
											Camera.CameraCurrentAlignment.Position =
												new OpenBveApi.Math.Vector3(Camera.CameraCurrentAlignment.Position.X,
													Camera.CameraCurrentAlignment.Position.Y, 0.0);
											Camera.CameraCurrentAlignment.Zoom = 0.0;
											Camera.CameraAlignmentDirection = new CameraAlignment();
											Camera.CameraAlignmentSpeed = new CameraAlignment();
											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
											{
												TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.Exterior);
											}
											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
											{
												TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
												TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
											}
											World.CameraTrackFollower.Update(TrackManager.CurrentTrack, World.CameraTrackFollower.TrackPosition + z, true, false);
											Camera.CameraCurrentAlignment.TrackPosition =
												World.CameraTrackFollower.TrackPosition;
											OpenBveShared.World.VerticalViewingAngle = OpenBveShared.World.OriginalVerticalViewingAngle;
											OpenBveShared.Renderer.UpdateViewport(OpenBveShared.Renderer.ViewPortChangeMode.NoChange);
											World.UpdateAbsoluteCamera(TimeElapsed);
											World.UpdateViewingDistances();
										}
										break;
									case Translations.Command.CameraReset:
										// camera: reset
										if (Camera.CameraView == CameraViewMode.Interior |
											Camera.CameraView == CameraViewMode.InteriorLookAhead)
										{
											Camera.CameraCurrentAlignment.Position = new OpenBveApi.Math.Vector3(0.0, 0.0,
												0.0);
										}
										Camera.CameraCurrentAlignment.Yaw = 0.0;
										Camera.CameraCurrentAlignment.Pitch = 0.0;
										Camera.CameraCurrentAlignment.Roll = 0.0;
										if (Camera.CameraView == CameraViewMode.Track)
										{
											World.CameraTrackFollower.Update(
												TrackManager.CurrentTrack, TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition, true,
												false);
										}
										else if (Camera.CameraView == CameraViewMode.FlyBy |
												 Camera.CameraView == CameraViewMode.FlyByZooming)
										{
											if (TrainManager.PlayerTrain.Specs.CurrentAverageSpeed >= 0.0)
											{
												double d = 30.0 +
														   4.0*TrainManager.PlayerTrain.Specs.CurrentAverageSpeed;
												World.CameraTrackFollower.Update(
													TrackManager.CurrentTrack, TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower
														.TrackPosition + d, true, false);
											}
											else
											{
												double d = 30.0 -
														   4.0*TrainManager.PlayerTrain.Specs.CurrentAverageSpeed;
												World.CameraTrackFollower.Update(
													TrackManager.CurrentTrack, TrainManager.PlayerTrain.Cars[
														TrainManager.PlayerTrain.Cars.Length - 1].RearAxle.Follower
														.TrackPosition - d, true, false);
											}
										}
										Camera.CameraCurrentAlignment.TrackPosition =
											World.CameraTrackFollower.TrackPosition;
										Camera.CameraCurrentAlignment.Zoom = 0.0;
										OpenBveShared.World.VerticalViewingAngle = OpenBveShared.World.OriginalVerticalViewingAngle;
										Camera.CameraAlignmentDirection = new CameraAlignment();
										Camera.CameraAlignmentSpeed = new CameraAlignment();
										OpenBveShared.Renderer.UpdateViewport(OpenBveShared.Renderer.ViewPortChangeMode.NoChange);
										World.UpdateAbsoluteCamera(TimeElapsed);
										World.UpdateViewingDistances();
										if ((Camera.CameraView == CameraViewMode.Interior |
											 Camera.CameraView == CameraViewMode.InteriorLookAhead) &
											Camera.CameraRestriction == CameraRestrictionMode.On)
										{
											if (!World.PerformCameraRestrictionTest())
											{
												World.InitializeCameraRestriction();
											}
										}
										break;
									case Translations.Command.CameraRestriction:
										// camera: restriction
										if (Camera.CameraRestriction != CameraRestrictionMode.NotAvailable)
										{
											if (Camera.CameraRestriction == CameraRestrictionMode.Off)
											{
												Camera.CameraRestriction = CameraRestrictionMode.On;
											}
											else
											{
												Camera.CameraRestriction = CameraRestrictionMode.Off;
											}
											World.InitializeCameraRestriction();
											if (Camera.CameraRestriction == CameraRestrictionMode.Off)
											{
												Game.AddMessage(
													Translations.GetInterfaceString("notification_camerarestriction_off"),
													MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
													MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											}
											else
											{
												Game.AddMessage(
													Translations.GetInterfaceString("notification_camerarestriction_on"),
													MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
													MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											}
										}
										break;
									case Translations.Command.SinglePower:
										// single power
										if (TrainManager.PlayerTrain.Handles.SingleHandle)
										{
											int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
											if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
											{
												TrainManager.PlayerTrain.UnapplyEmergencyBrake();
											}
											else if (b == 1 & TrainManager.PlayerTrain.Handles.HasHoldBrake)
											{
												TrainManager.PlayerTrain.ApplyNotch(0, true, 0, false);
												TrainManager.PlayerTrain.ApplyHoldBrake(true);
											}
											else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
											{
												TrainManager.PlayerTrain.ApplyHoldBrake(false);
											}
											else if (b > 0)
											{
												TrainManager.PlayerTrain.ApplyNotch(0, true, -1, true);
											}
											else
											{
												int p = TrainManager.PlayerTrain.Handles.Power.Driver;
												if (p < TrainManager.PlayerTrain.Handles.Power.MaximumNotch)
												{
													TrainManager.PlayerTrain.ApplyNotch(1, true, 0, true);
												}
											}
										}
										break;
									case Translations.Command.SingleNeutral:
										// single neutral
										if (TrainManager.PlayerTrain.Handles.SingleHandle)
										{
											int p = TrainManager.PlayerTrain.Handles.Power.Driver;
											if (p > 0)
											{
												TrainManager.PlayerTrain.ApplyNotch(-1, true, 0, true);
											}
											else
											{
												int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
												if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
												{
													TrainManager.PlayerTrain.UnapplyEmergencyBrake();
												}
												else if (b == 1 & TrainManager.PlayerTrain.Handles.HasHoldBrake)
												{
													TrainManager.PlayerTrain.ApplyNotch(0, true, 0, false);
													TrainManager.PlayerTrain.ApplyHoldBrake(true);
												}
												else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.ApplyHoldBrake(false);
												}
												else if (b > 0)
												{
													TrainManager.PlayerTrain.ApplyNotch(0, true, -1, true);
												}
											}
										}
										break;
									case Translations.Command.SingleBrake:
										// single brake
										if (TrainManager.PlayerTrain.Handles.SingleHandle)
										{
											int p = TrainManager.PlayerTrain.Handles.Power.Driver;
											if (p > 0)
											{
												TrainManager.PlayerTrain.ApplyNotch(-1, true, 0, true);
											}
											else
											{
												int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
												if (TrainManager.PlayerTrain.Handles.HasHoldBrake & b == 0 &
													!TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.ApplyHoldBrake(true);
												}
												else if (b < TrainManager.PlayerTrain.Handles.Brake.MaximumNotch)
												{
													TrainManager.PlayerTrain.ApplyNotch(0, true, 1, true);
													TrainManager.PlayerTrain.ApplyHoldBrake(false);
												}
											}
										}
										break;
									case Translations.Command.SingleEmergency:
										// single emergency
										if (TrainManager.PlayerTrain.Handles.SingleHandle)
										{
											TrainManager.PlayerTrain.ApplyEmergencyBrake();
										}
										break;
									case Translations.Command.PowerIncrease:
										// power increase
										if (!TrainManager.PlayerTrain.Handles.SingleHandle)
										{
											int p = TrainManager.PlayerTrain.Handles.Power.Driver;
											if (p < TrainManager.PlayerTrain.Handles.Power.MaximumNotch)
											{
												TrainManager.PlayerTrain.ApplyNotch(1, true, 0, true);
											}
										}
										break;
									case Translations.Command.PowerDecrease:
										// power decrease
										if (!TrainManager.PlayerTrain.Handles.SingleHandle)
										{
											int p = TrainManager.PlayerTrain.Handles.Power.Driver;
											if (p > 0)
											{
												TrainManager.PlayerTrain.ApplyNotch(-1, true, 0, true);
											}
										}
										break;
									case Translations.Command.BrakeIncrease:
										// brake increase
										if (!TrainManager.PlayerTrain.Handles.SingleHandle)
										{
											if (TrainManager.PlayerTrain.Handles.Brake is TrainManager.AirBrakeHandle)
											{
												if (TrainManager.PlayerTrain.Handles.HasHoldBrake &
													TrainManager.PlayerTrain.Handles.Brake.Driver ==
													(int)TrainManager.AirBrakeHandleState.Release &
													!TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.ApplyHoldBrake(true);
												}
												else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Lap);
													TrainManager.PlayerTrain.ApplyHoldBrake(false);
												}
												else if (TrainManager.PlayerTrain.Handles.Brake.Driver ==
												         (int)TrainManager.AirBrakeHandleState.Lap)
												{
													TrainManager.PlayerTrain.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Service);
												}
												else if (TrainManager.PlayerTrain.Handles.Brake.Driver ==
												         (int)TrainManager.AirBrakeHandleState.Release)
												{
													TrainManager.PlayerTrain.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Lap);
												}
											}
											else
											{
												int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
												if (TrainManager.PlayerTrain.Handles.HasHoldBrake & b == 0 &
													!TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.ApplyHoldBrake(true);
												}
												else if (b < TrainManager.PlayerTrain.Handles.Brake.MaximumNotch)
												{
													TrainManager.PlayerTrain.ApplyNotch(0, true, 1, true);
													TrainManager.PlayerTrain.ApplyHoldBrake(false);
												}
											}
										}
										break;
									case Translations.Command.BrakeDecrease:
										// brake decrease
										if (!TrainManager.PlayerTrain.Handles.SingleHandle)
										{
											if (TrainManager.PlayerTrain.Handles.Brake is TrainManager.AirBrakeHandle)
											{
												if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
												{
													TrainManager.PlayerTrain.UnapplyEmergencyBrake();
												}
												else if (TrainManager.PlayerTrain.Handles.HasHoldBrake &
														 TrainManager.PlayerTrain.Handles.Brake.Driver ==
														 (int)TrainManager.AirBrakeHandleState.Lap &
														 !TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.ApplyHoldBrake(true);
												}
												else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
													TrainManager.PlayerTrain.ApplyHoldBrake(false);
												}
												else if (TrainManager.PlayerTrain.Handles.Brake.Driver ==
												         (int)TrainManager.AirBrakeHandleState.Lap)
												{
													TrainManager.PlayerTrain.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
												}
												else if (TrainManager.PlayerTrain.Handles.Brake.Driver ==
												         (int)TrainManager.AirBrakeHandleState.Service)
												{
													TrainManager.PlayerTrain.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Lap);
												}
											}
											else
											{
												int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
												if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
												{
													TrainManager.PlayerTrain.UnapplyEmergencyBrake();
												}
												else if (b == 1 & TrainManager.PlayerTrain.Handles.HasHoldBrake)
												{
													TrainManager.PlayerTrain.ApplyNotch(0, true, 0, false);
													TrainManager.PlayerTrain.ApplyHoldBrake(true);
												}
												else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.PlayerTrain.ApplyHoldBrake(false);
												}
												else if (b > 0)
												{
													TrainManager.PlayerTrain.ApplyNotch(0, true, -1, true);
												}
											}
										}
										break;
									case Translations.Command.LocoBrakeIncrease:
										if (TrainManager.PlayerTrain.Handles.LocoBrake is TrainManager.LocoAirBrakeHandle)
										{
											if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)TrainManager.AirBrakeHandleState.Lap)
											{
												TrainManager.PlayerTrain.ApplyLocoAirBrakeHandle(TrainManager.AirBrakeHandleState.Service);
											}
											else if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)TrainManager.AirBrakeHandleState.Release)
											{
												TrainManager.PlayerTrain.ApplyLocoAirBrakeHandle(TrainManager.AirBrakeHandleState.Lap);
											}
										}
										else
										{
											TrainManager.PlayerTrain.ApplyLocoBrakeNotch(1, true);
										}
										
										break;
									case Translations.Command.LocoBrakeDecrease:
										if (TrainManager.PlayerTrain.Handles.LocoBrake is TrainManager.LocoAirBrakeHandle)
										{
											if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)TrainManager.AirBrakeHandleState.Lap)
											{
												TrainManager.PlayerTrain.ApplyLocoAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
											}
											else if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)TrainManager.AirBrakeHandleState.Service)
											{
												TrainManager.PlayerTrain.ApplyLocoAirBrakeHandle(TrainManager.AirBrakeHandleState.Lap);
											}
										}
										else
										{
											TrainManager.PlayerTrain.ApplyLocoBrakeNotch(-1, true);
										}
										break;
									case Translations.Command.BrakeEmergency:
										// brake emergency
										TrainManager.PlayerTrain.ApplyEmergencyBrake();
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
										if (TrainManager.PlayerTrain.Handles.SingleHandle && TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
										{
											TrainManager.PlayerTrain.UnapplyEmergencyBrake();
										}
										TrainManager.PlayerTrain.ApplyNotch(Interface.CurrentControls[i].Option, false, 0, !TrainManager.PlayerTrain.Handles.SingleHandle);
										break;
									case Translations.Command.BrakeAnyNotch:
										if (TrainManager.PlayerTrain.Handles.Brake is TrainManager.AirBrakeHandle)
										{
											if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
											{
												TrainManager.PlayerTrain.UnapplyEmergencyBrake();
											}
											TrainManager.PlayerTrain.ApplyHoldBrake(false);
											if (Interface.CurrentControls[i].Option <= (int)TrainManager.AirBrakeHandleState.Release)
											{
												TrainManager.PlayerTrain.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
											}
											else if (Interface.CurrentControls[i].Option == (int)TrainManager.AirBrakeHandleState.Lap)
											{
												TrainManager.PlayerTrain.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Lap);
											}
											else
											{
												TrainManager.PlayerTrain.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Service);
											}
										}
										else
										{
											if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
											{
												TrainManager.PlayerTrain.UnapplyEmergencyBrake();
											}
											TrainManager.PlayerTrain.ApplyHoldBrake(false);
											TrainManager.PlayerTrain.ApplyNotch(0, !TrainManager.PlayerTrain.Handles.SingleHandle, Interface.CurrentControls[i].Option, false);
										}
										break;
									case Translations.Command.ReverserAnyPostion:
										TrainManager.PlayerTrain.ApplyReverser(Interface.CurrentControls[i].Option, false);
										break;
									case Translations.Command.HoldBrake:
										if (TrainManager.PlayerTrain.Handles.HasHoldBrake && (TrainManager.PlayerTrain.Handles.Brake.Driver == 0 || TrainManager.PlayerTrain.Handles.Brake.Driver == 1) && !TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
										{
											TrainManager.PlayerTrain.ApplyHoldBrake(true);
										}
										break;
									case Translations.Command.ReverserForward:
										// reverser forward
										if (TrainManager.PlayerTrain.Handles.Reverser.Driver < TrainManager.ReverserPosition.Forwards)
										{
											TrainManager.PlayerTrain.ApplyReverser(1, true);
										}
										break;
									case Translations.Command.ReverserBackward:
										// reverser backward
										if (TrainManager.PlayerTrain.Handles.Reverser.Driver > TrainManager.ReverserPosition.Reverse)
										{
											TrainManager.PlayerTrain.ApplyReverser(-1, true);
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
														? OpenBveApi.Runtime.HornTypes.Primary
														: j == 1
															? OpenBveApi.Runtime.HornTypes.Secondary
															: OpenBveApi.Runtime.HornTypes.Music);
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
										if ((TrainManager.GetDoorsState(TrainManager.PlayerTrain, true, false) &
											 TrainManager.TrainDoorState.Opened) == 0)
										{
											if (TrainManager.PlayerTrain.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic
												& (TrainManager.PlayerTrain.Specs.DoorInterlockState == TrainManager.DoorInterlockStates.Unlocked
												   | TrainManager.PlayerTrain.Specs.DoorInterlockState == TrainManager.DoorInterlockStates.Left))
											{
												TrainManager.OpenTrainDoors(TrainManager.PlayerTrain, true, false);
											}
										}
										else
										{
											if (TrainManager.PlayerTrain.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic
												& (TrainManager.PlayerTrain.Specs.DoorInterlockState == TrainManager.DoorInterlockStates.Unlocked
												   | TrainManager.PlayerTrain.Specs.DoorInterlockState == TrainManager.DoorInterlockStates.Left))
											{
												TrainManager.CloseTrainDoors(TrainManager.PlayerTrain, true, false);
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
										if ((TrainManager.GetDoorsState(TrainManager.PlayerTrain, false, true) &
											 TrainManager.TrainDoorState.Opened) == 0)
										{
											if (TrainManager.PlayerTrain.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic
												& (TrainManager.PlayerTrain.Specs.DoorInterlockState == TrainManager.DoorInterlockStates.Unlocked
												   | TrainManager.PlayerTrain.Specs.DoorInterlockState == TrainManager.DoorInterlockStates.Right))
											{
												TrainManager.OpenTrainDoors(TrainManager.PlayerTrain, false, true);
											}
										}
										else
										{
											if (TrainManager.PlayerTrain.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic
												& (TrainManager.PlayerTrain.Specs.DoorInterlockState == TrainManager.DoorInterlockStates.Unlocked
												   | TrainManager.PlayerTrain.Specs.DoorInterlockState == TrainManager.DoorInterlockStates.Right))
											{
												TrainManager.CloseTrainDoors(TrainManager.PlayerTrain, false, true);
											}
										}
										if (TrainManager.PlayerTrain.Plugin != null)
										{
											TrainManager.PlayerTrain.Plugin.KeyDown(VirtualKeys.RightDoors);
										}
										TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Doors[1].ButtonPressed = true;
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
											TrainManager.PlayerTrain.Plugin.KeyDown(
												Translations.SecurityToVirtualKey(Interface.CurrentControls[i].Command));
										}
										break;



									case Translations.Command.TimetableToggle:
										// option: timetable
										if (Interface.CurrentOptions.TimeTableStyle == Interface.TimeTableMode.None)
										{
											//We have selected not to display a timetable at all
											break;
										}
										if (Interface.CurrentOptions.TimeTableStyle == Interface.TimeTableMode.AutoGenerated || !Timetable.CustomTimetableAvailable)
										{
											//Either the auto-generated timetable has been selected as the preference, or no custom timetable has been supplied
											switch (Timetable.CurrentTimetable)
											{
												case Timetable.TimetableState.Default:
													Timetable.CurrentTimetable = Timetable.TimetableState.None;
													break;
												default:
													Timetable.CurrentTimetable = Timetable.TimetableState.Default;
													break;
											}
											break;
										}
										if (Interface.CurrentOptions.TimeTableStyle == Interface.TimeTableMode.PreferCustom)
										{
											//We have already determined that a custom timetable is available in the if above
											if (Timetable.CurrentTimetable == Timetable.TimetableState.None)
											{
												Timetable.CurrentTimetable = Timetable.TimetableState.Custom;
											}
											else
											{
												Timetable.CurrentTimetable = Timetable.TimetableState.None;
											}
											break;
										}
										//Fallback legacy behaviour- Cycles between custom, default and none
										switch (Timetable.CurrentTimetable)
										{
											case Timetable.TimetableState.Custom:
												Timetable.CurrentTimetable = Timetable.TimetableState.Default;
												break;
											case Timetable.TimetableState.Default:
												Timetable.CurrentTimetable = Timetable.TimetableState.None;
												break;
											default:
												Timetable.CurrentTimetable = Timetable.TimetableState.Custom;
												break;
										}
										break;
									case Translations.Command.DebugWireframe:
										// option: wireframe
										OpenBveShared.Renderer.OptionWireframe = !OpenBveShared.Renderer.OptionWireframe;
										if (OpenBveShared.Renderer.OptionWireframe)
										{
											GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
										}
										else
										{
											GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
										}
										OpenBveShared.Renderer.StaticOpaqueForceUpdate = true;
										break;
									case Translations.Command.DebugNormals:
										// option: normals
										OpenBveShared.Renderer.OptionNormals = !OpenBveShared.Renderer.OptionNormals;
										OpenBveShared.Renderer.StaticOpaqueForceUpdate = true;
										break;
									case Translations.Command.ShowEvents:
										Interface.CurrentOptions.ShowEvents = !Interface.CurrentOptions.ShowEvents;
										break;
									case Translations.Command.MiscAI:
										// option: AI
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert)
										{
											Game.AddMessage(
												Translations.GetInterfaceString("notification_notavailableexpert"),
												MessageManager.MessageDependency.None, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 5.0, null);
										}
										else
										{
											if (TrainManager.PlayerTrain.AI == null)
											{
												TrainManager.PlayerTrain.AI =
													new Game.SimpleHumanDriverAI(TrainManager.PlayerTrain);
												if (TrainManager.PlayerTrain.Plugin != null &&
													!TrainManager.PlayerTrain.Plugin.SupportsAI)
												{
													Game.AddMessage(
														Translations.GetInterfaceString("notification_aiunable"),
														MessageManager.MessageDependency.None, Interface.GameMode.Expert,
														MessageColor.White, Game.SecondsSinceMidnight + 10.0, null);
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
										switch (Renderer.CurrentOutputMode)
										{
											case Renderer.OutputMode.Default:
												Renderer.CurrentOutputMode = Interface.CurrentOptions.GameMode ==
																			 Interface.GameMode.Expert
													? Renderer.OutputMode.None
													: Renderer.OutputMode.Debug;
												break;
											case Renderer.OutputMode.Debug:
												Renderer.CurrentOutputMode = Renderer.OutputMode.None;
												break;
											case Renderer.OutputMode.DebugATS:
												Renderer.CurrentOutputMode = Renderer.PreviousOutputMode;
												break;
											default:
												Renderer.CurrentOutputMode = Renderer.OutputMode.Default;
												break;
										}
										Renderer.PreviousOutputMode = Renderer.CurrentOutputMode;
										break;
									case Translations.Command.DebugATS:
										if (Renderer.CurrentOutputMode == Renderer.OutputMode.DebugATS)
										{
											Renderer.CurrentOutputMode = Renderer.PreviousOutputMode;
										}
										else
										{
											Renderer.PreviousOutputMode = Renderer.CurrentOutputMode;
											Renderer.CurrentOutputMode = Renderer.OutputMode.DebugATS;
										}
										break;
									case Translations.Command.MiscBackfaceCulling:
										// option: backface culling
										OpenBveShared.Renderer.OptionBackfaceCulling = !OpenBveShared.Renderer.OptionBackfaceCulling;
										OpenBveShared.Renderer.StaticOpaqueForceUpdate = true;
										Game.AddMessage(
											Translations.GetInterfaceString(OpenBveShared.Renderer.OptionBackfaceCulling
												? "notification_backfaceculling_on"
												: "notification_backfaceculling_off"), MessageManager.MessageDependency.None,
											Interface.GameMode.Expert, MessageColor.White,
											Game.SecondsSinceMidnight + 2.0, null);
										break;
									case Translations.Command.MiscCPUMode:
										// option: limit frame rate
										LimitFramerate = !LimitFramerate;
										Game.AddMessage(
											Translations.GetInterfaceString(LimitFramerate
												? "notification_cpu_low"
												: "notification_cpu_normal"), MessageManager.MessageDependency.None,
											Interface.GameMode.Expert, MessageColor.White,
											Game.SecondsSinceMidnight + 2.0, null);
										break;
									case Translations.Command.DebugBrakeSystems:
										// option: brake systems
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert)
										{
											Game.AddMessage(
												Translations.GetInterfaceString("notification_notavailableexpert"),
												MessageManager.MessageDependency.None, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 5.0, null);
										}
										else
										{
											Renderer.OptionBrakeSystems = !Renderer.OptionBrakeSystems;
										}
										break;
									case Translations.Command.MenuActivate:
										// menu
										Game.Menu.PushMenu(Menu.MenuType.Top);
										break;
									case Translations.Command.MiscPause:
										// pause
										Game.CurrentInterface = Game.InterfaceType.Pause;
										break;
									case Translations.Command.MiscClock:
										// clock
										Renderer.OptionClock = !Renderer.OptionClock;
										break;
									case Translations.Command.MiscTimeFactor:
										// time factor
										if (!PluginManager.Plugin.DisableTimeAcceleration)
										{
											if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert)
											{
												Game.AddMessage(
													Translations.GetInterfaceString("notification_notavailableexpert"),
													MessageManager.MessageDependency.None, Interface.GameMode.Expert,
													MessageColor.White,
													Game.SecondsSinceMidnight + 5.0, null);
											}
											else
											{
												TimeFactor = TimeFactor == 1
													? Interface.CurrentOptions.TimeAccelerationFactor
													: 1;
												Game.AddMessage(
													TimeFactor.ToString(
														System.Globalization.CultureInfo.InvariantCulture) + "x",
													MessageManager.MessageDependency.None, Interface.GameMode.Expert,
													MessageColor.White,
													Game.SecondsSinceMidnight + 5.0*(double) TimeFactor, null);
											}
										}
										break;
									case Translations.Command.MiscSpeed:
										// speed
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert)
										{
											Game.AddMessage(
												Translations.GetInterfaceString("notification_notavailableexpert"),
												MessageManager.MessageDependency.None, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 5.0, null);
										}
										else
										{
											Renderer.OptionSpeed++;
											if ((int) Renderer.OptionSpeed >= 3) Renderer.OptionSpeed = 0;
										}
										break;
									case Translations.Command.MiscGradient:
										// gradient
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert)
										{
											Game.AddMessage(
												Translations.GetInterfaceString("notification_notavailableexpert"),
												MessageManager.MessageDependency.None, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 5.0, null);
										}
										else
										{
											Renderer.OptionGradient++;
											if ((int)Renderer.OptionGradient >= 4) Renderer.OptionGradient = 0;
										}
										break;
									case Translations.Command.MiscDistanceToNextStation:
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert)
										{
											Game.AddMessage(
												Translations.GetInterfaceString("notification_notavailableexpert"),
												MessageManager.MessageDependency.None, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 5.0, null);
										}
										else
										{
											Renderer.OptionDistanceToNextStation++;
											if ((int)Renderer.OptionDistanceToNextStation >= 3) Renderer.OptionDistanceToNextStation = 0;
										}
										break;
									case Translations.Command.MiscFps:
										// fps
										Renderer.OptionFrameRates = !Renderer.OptionFrameRates;
										break;
									case Translations.Command.MiscFullscreen:
										// toggle fullscreen
										Screen.ToggleFullscreen();
										break;
									case Translations.Command.MiscMute:
										// mute
										Sounds.GlobalMute = !Sounds.GlobalMute;
										Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
										break;
								case Translations.Command.RouteInformation:
// Replaced by RouteInfoOverlay, but not deleted for future reference
//									if (RouteInfoThread == null)
//									{
//										RouteInfoThread = new Thread(ThreadProc)
//										{
//											IsBackground = true
//										};
//										RouteInfoThread.Start();
//										while (MainLoop.RouteInformationForm == null || !MainLoop.RouteInformationForm.IsHandleCreated)
//										{
//											//The form may take a few milliseconds to load
//											//Takes longer on Mono
//											Thread.Sleep(10);
//										}
//										MainLoop.RouteInformationForm.Invoke((MethodInvoker) delegate
//										{
//											byte[] RouteMap = OpenBve.Game.RouteInformation.RouteMap.ToByteArray(ImageFormat.Bmp);
//											byte[] GradientProfile = OpenBve.Game.RouteInformation.GradientProfile.ToByteArray(ImageFormat.Bmp);
//											RouteInformationForm.UpdateImage(RouteMap, GradientProfile,Game.RouteInformation.RouteBriefing);
//										});
//									}
//									else
//									{
//										if (MainLoop.RouteInformationForm.Visible == true)
//										{
//											MainLoop.RouteInformationForm.Invoke((MethodInvoker) delegate
//											{
//												MainLoop.RouteInformationForm.Hide();
//											});
//										}
//										else
//										{
//											MainLoop.RouteInformationForm.Invoke((MethodInvoker)delegate
//											{
//												MainLoop.RouteInformationForm.Show();
//											});
//										}
//									}
									Game.routeInfoOverlay.ProcessCommand(Translations.Command.RouteInformation);
									break;
								}
							}
							else if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Released)
							{
								// released
								Interface.CurrentControls[i].DigitalState =
									Interface.DigitalControlState.ReleasedAcknowledged;
								switch (Interface.CurrentControls[i].Command)
								{
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
								}
							}
						}
					}
					break;
			}
		}

	}
}
