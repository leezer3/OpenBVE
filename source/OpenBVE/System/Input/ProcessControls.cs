using System;
using OpenBveApi.Colors;
using OpenBveApi.Runtime;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
	internal static partial class MainLoop
	{
		/// <summary>The ProcessControls function should be called once a frame, and updates the simulation accordingly</summary>
		/// <param name="TimeElapsed">The time elapsed in ms since the last call to this function</param>
		internal static void ProcessControls(double TimeElapsed)
		{
			//If we are currently blocking key repeat events from firing, return
			if (BlockKeyRepeat) return;
			switch (Game.CurrentInterface)
			{
				case Game.InterfaceType.Pause:
					// pause
					for (int i = 0; i < Interface.CurrentControls.Length; i++)
					{
						if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.Digital)
						{
							if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Pressed)
							{
								Interface.CurrentControls[i].DigitalState =
									Interface.DigitalControlState.PressedAcknowledged;
								switch (Interface.CurrentControls[i].Command)
								{
									case Interface.Command.MiscPause:
										Game.CurrentInterface = Game.InterfaceType.Normal;
										break;
									case Interface.Command.MenuActivate:
										Game.Menu.PushMenu(Menu.MenuType.Top);
										break;
									case Interface.Command.MiscQuit:
										Game.Menu.PushMenu(Menu.MenuType.Quit);
										break;
									case Interface.Command.MiscFullscreen:
										Screen.ToggleFullscreen();
										break;
									case Interface.Command.MiscMute:
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
					for (int i = 0; i < Interface.CurrentControls.Length; i++)
					{
						if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.Digital
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
						if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.AnalogHalf |
							Interface.CurrentControls[i].InheritedType == Interface.CommandType.AnalogFull)
						{
							// analog control
							if (Interface.CurrentControls[i].AnalogState != 0.0)
							{
								switch (Interface.CurrentControls[i].Command)
								{
									case Interface.Command.PowerHalfAxis:
									case Interface.Command.PowerFullAxis:
										// power half/full-axis
										if (!TrainManager.PlayerTrain.Specs.SingleHandle)
										{
											double a = Interface.CurrentControls[i].AnalogState;
											if (Interface.CurrentControls[i].Command == Interface.Command.BrakeFullAxis)
											{
												a = 0.5*(a + 1.0);
											}
											a *= (double) TrainManager.PlayerTrain.Specs.MaximumPowerNotch;
											int p = (int) Math.Round(a);
											TrainManager.ApplyNotch(TrainManager.PlayerTrain, p, false, 0, true);
										}
										break;
									case Interface.Command.BrakeHalfAxis:
									case Interface.Command.BrakeFullAxis:
										// brake half/full-axis
										if (!TrainManager.PlayerTrain.Specs.SingleHandle)
										{
											int d = TrainManager.PlayerTrain.DriverCar;
											if (TrainManager.PlayerTrain.Cars[d].Specs.BrakeType ==
												TrainManager.CarBrakeType.AutomaticAirBrake)
											{
												double a = Interface.CurrentControls[i].AnalogState;
												if (Interface.CurrentControls[i].Command ==
													Interface.Command.BrakeFullAxis)
												{
													a = 0.5*(a + 1.0);
												}
												int b = (int) Math.Round(3.0*a);
												switch (b)
												{
													case 0:
														TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver =
															false;
														TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
															TrainManager.AirBrakeHandleState.Release);
														break;
													case 1:
														TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver =
															false;
														TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
															TrainManager.AirBrakeHandleState.Lap);
														break;
													case 2:
														TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver =
															false;
														TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
															TrainManager.AirBrakeHandleState.Service);
														break;
													case 3:
														if (Interface.CurrentOptions.AllowAxisEB)
														{
															TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
														}
														break;
												}
											}
											else
											{
												if (TrainManager.PlayerTrain.Specs.HasHoldBrake)
												{
													double a = Interface.CurrentControls[i].AnalogState;
													if (Interface.CurrentControls[i].Command ==
														Interface.Command.BrakeFullAxis)
													{
														a = 0.5*(a + 1.0);
													}
													a *= (double) TrainManager.PlayerTrain.Specs.MaximumBrakeNotch + 2;
													int b = (int) Math.Round(a);
													bool q = b == 1;
													if (b > 0) b--;
													if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch)
													{
														TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
														TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, b,
															false);
													}
													else
													{
														TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
													}
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, q);
												}
												else
												{
													double a = Interface.CurrentControls[i].AnalogState;
													if (Interface.CurrentControls[i].Command ==
														Interface.Command.BrakeFullAxis)
													{
														a = 0.5*(a + 1.0);
													}
													a *= (double) TrainManager.PlayerTrain.Specs.MaximumBrakeNotch + 1;
													int b = (int) Math.Round(a);
													if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch)
													{
														TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
														TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, b,
															false);
													}
													else
													{
														if (Interface.CurrentOptions.AllowAxisEB)
														{
															TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
														}
													}
												}
											}
										}
										break;
									case Interface.Command.SingleFullAxis:
										// single full axis
										if (TrainManager.PlayerTrain.Specs.SingleHandle)
										{
											if (TrainManager.PlayerTrain.Specs.HasHoldBrake)
											{
												double a = Interface.CurrentControls[i].AnalogState;
												int p =
													(int)
														Math.Round(a*
																   (double)
																	   TrainManager.PlayerTrain.Specs.MaximumPowerNotch);
												int b =
													(int)
														Math.Round(-a*
																   (double)
																	   TrainManager.PlayerTrain.Specs.MaximumBrakeNotch +
																   2);
												if (p < 0) p = 0;
												if (b < 0) b = 0;
												bool q = b == 1;
												if (b > 0) b--;
												if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch)
												{
													TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, p, false, b, false);
												}
												else
												{
													if (Interface.CurrentOptions.AllowAxisEB)
													{
														TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
													}
												}
												TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, q);
											}
											else
											{
												double a = Interface.CurrentControls[i].AnalogState;
												int p =
													(int)
														Math.Round(a*
																   (double)
																	   TrainManager.PlayerTrain.Specs.MaximumPowerNotch);
												int b =
													(int)
														Math.Round(-a*
																   ((double)
																	   TrainManager.PlayerTrain.Specs.MaximumBrakeNotch +
																	1));
												if (p < 0) p = 0;
												if (b < 0) b = 0;
												if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch)
												{
													TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, p, false, b, false);
												}
												else
												{
													if (Interface.CurrentOptions.AllowAxisEB)
													{
														TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
													}
												}
											}
										}
										break;
									case Interface.Command.ReverserFullAxis:
										// reverser full axis
									{
										double a = Interface.CurrentControls[i].AnalogState;
										int r = (int) Math.Round(a);
										TrainManager.ApplyReverser(TrainManager.PlayerTrain, r, false);
									}
										break;
									case Interface.Command.CameraMoveForward:
										// camera move forward
										if (World.CameraMode == World.CameraViewMode.Interior |
											World.CameraMode == World.CameraViewMode.InteriorLookAhead |
											World.CameraMode == World.CameraViewMode.Exterior)
										{
											double s = World.CameraMode == World.CameraViewMode.Interior |
													   World.CameraMode == World.CameraViewMode.InteriorLookAhead
												? World.CameraInteriorTopSpeed
												: World.CameraExteriorTopSpeed;
											World.CameraAlignmentDirection.Position.Z = s*
																						Interface.CurrentControls[i]
																							.AnalogState;
										}
										else
										{
											World.CameraAlignmentDirection.TrackPosition = World.CameraExteriorTopSpeed*
																						   Interface.CurrentControls[i]
																							   .AnalogState;
										}
										break;
									case Interface.Command.CameraMoveBackward:
										// camera move backward
										if (World.CameraMode == World.CameraViewMode.Interior |
											World.CameraMode == World.CameraViewMode.InteriorLookAhead |
											World.CameraMode == World.CameraViewMode.Exterior)
										{
											double s = World.CameraMode == World.CameraViewMode.Interior |
													   World.CameraMode == World.CameraViewMode.InteriorLookAhead
												? World.CameraInteriorTopSpeed
												: World.CameraExteriorTopSpeed;
											World.CameraAlignmentDirection.Position.Z = -s*
																						Interface.CurrentControls[i]
																							.AnalogState;
										}
										else
										{
											World.CameraAlignmentDirection.TrackPosition =
												-World.CameraExteriorTopSpeed*Interface.CurrentControls[i].AnalogState;
										}
										break;
									case Interface.Command.CameraMoveLeft:
										// camera move left
									{
										double s = World.CameraMode == World.CameraViewMode.Interior |
												   World.CameraMode == World.CameraViewMode.InteriorLookAhead
											? World.CameraInteriorTopSpeed
											: World.CameraExteriorTopSpeed;
										World.CameraAlignmentDirection.Position.X = -s*
																					Interface.CurrentControls[i]
																						.AnalogState;
									}
										break;
									case Interface.Command.CameraMoveRight:
										// camera move right
									{
										double s = World.CameraMode == World.CameraViewMode.Interior |
												   World.CameraMode == World.CameraViewMode.InteriorLookAhead
											? World.CameraInteriorTopSpeed
											: World.CameraExteriorTopSpeed;
										World.CameraAlignmentDirection.Position.X = s*
																					Interface.CurrentControls[i]
																						.AnalogState;
									}
										break;
									case Interface.Command.CameraMoveUp:
										// camera move up
									{
										double s = World.CameraMode == World.CameraViewMode.Interior |
												   World.CameraMode == World.CameraViewMode.InteriorLookAhead
											? World.CameraInteriorTopSpeed
											: World.CameraExteriorTopSpeed;
										World.CameraAlignmentDirection.Position.Y = s*
																					Interface.CurrentControls[i]
																						.AnalogState;
									}
										break;
									case Interface.Command.CameraMoveDown:
										// camera move down
									{
										double s = World.CameraMode == World.CameraViewMode.Interior |
												   World.CameraMode == World.CameraViewMode.InteriorLookAhead
											? World.CameraInteriorTopSpeed
											: World.CameraExteriorTopSpeed;
										World.CameraAlignmentDirection.Position.Y = -s*
																					Interface.CurrentControls[i]
																						.AnalogState;
									}
										break;
									case Interface.Command.CameraRotateLeft:
										// camera rotate left
									{
										double s = World.CameraMode == World.CameraViewMode.Interior |
												   World.CameraMode == World.CameraViewMode.InteriorLookAhead
											? World.CameraInteriorTopAngularSpeed
											: World.CameraExteriorTopAngularSpeed;
										World.CameraAlignmentDirection.Yaw = -s*Interface.CurrentControls[i].AnalogState;
									}
										break;
									case Interface.Command.CameraRotateRight:
										// camera rotate right
									{
										double s = World.CameraMode == World.CameraViewMode.Interior |
												   World.CameraMode == World.CameraViewMode.InteriorLookAhead
											? World.CameraInteriorTopAngularSpeed
											: World.CameraExteriorTopAngularSpeed;
										World.CameraAlignmentDirection.Yaw = s*Interface.CurrentControls[i].AnalogState;
									}
										break;
									case Interface.Command.CameraRotateUp:
										// camera rotate up
									{
										double s = World.CameraMode == World.CameraViewMode.Interior |
												   World.CameraMode == World.CameraViewMode.InteriorLookAhead
											? World.CameraInteriorTopAngularSpeed
											: World.CameraExteriorTopAngularSpeed;
										World.CameraAlignmentDirection.Pitch = s*
																			   Interface.CurrentControls[i].AnalogState;
									}
										break;
									case Interface.Command.CameraRotateDown:
										// camera rotate down
									{
										double s = World.CameraMode == World.CameraViewMode.Interior |
												   World.CameraMode == World.CameraViewMode.InteriorLookAhead
											? World.CameraInteriorTopAngularSpeed
											: World.CameraExteriorTopAngularSpeed;
										World.CameraAlignmentDirection.Pitch = -s*
																			   Interface.CurrentControls[i].AnalogState;
									}
										break;
									case Interface.Command.CameraRotateCCW:
										// camera rotate ccw
										if ((World.CameraMode != World.CameraViewMode.Interior &
											 World.CameraMode != World.CameraViewMode.InteriorLookAhead) |
											World.CameraRestriction != World.CameraRestrictionMode.On)
										{
											double s = World.CameraMode == World.CameraViewMode.Interior |
													   World.CameraMode == World.CameraViewMode.InteriorLookAhead
												? World.CameraInteriorTopAngularSpeed
												: World.CameraExteriorTopAngularSpeed;
											World.CameraAlignmentDirection.Roll = -s*
																				  Interface.CurrentControls[i]
																					  .AnalogState;
										}
										break;
									case Interface.Command.CameraRotateCW:
										// camera rotate cw
										if ((World.CameraMode != World.CameraViewMode.Interior &
											 World.CameraMode != World.CameraViewMode.InteriorLookAhead) |
											World.CameraRestriction != World.CameraRestrictionMode.On)
										{
											double s = World.CameraMode == World.CameraViewMode.Interior |
													   World.CameraMode == World.CameraViewMode.InteriorLookAhead
												? World.CameraInteriorTopAngularSpeed
												: World.CameraExteriorTopAngularSpeed;
											World.CameraAlignmentDirection.Roll = s*
																				  Interface.CurrentControls[i]
																					  .AnalogState;
										}
										break;
									case Interface.Command.CameraZoomIn:
										// camera zoom in
										if (TimeElapsed > 0.0)
										{
											World.CameraAlignmentDirection.Zoom = -World.CameraZoomTopSpeed*
																				  Interface.CurrentControls[i]
																					  .AnalogState;
										}
										break;
									case Interface.Command.CameraZoomOut:
										// camera zoom out
										if (TimeElapsed > 0.0)
										{
											World.CameraAlignmentDirection.Zoom = World.CameraZoomTopSpeed*
																				  Interface.CurrentControls[i]
																					  .AnalogState;
										}
										break;
									case Interface.Command.TimetableUp:
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
									case Interface.Command.TimetableDown:
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
														Textures.OpenGlTextureWrapMode.ClampClamp);
													max =
														Math.Min(
															Screen.Height - Timetable.DefaultTimetableTexture.Height,
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
												Textures.Texture texture =
													Timetable.CurrentCustomTimetableDaytimeTexture;
												if (texture == null)
												{
													texture = Timetable.CurrentCustomTimetableNighttimeTexture;
												}
												double max;
												if (texture != null)
												{
													Textures.LoadTexture(texture,
														Textures.OpenGlTextureWrapMode.ClampClamp);
													max = Math.Min(Screen.Height - texture.Height, 0.0);
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
						else if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.Digital)
						{
							// digital control
							if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Pressed)
							{
								// pressed
								Interface.CurrentControls[i].DigitalState =
									Interface.DigitalControlState.PressedAcknowledged;
								switch (Interface.CurrentControls[i].Command)
								{
									case Interface.Command.MiscQuit:
										// quit
										Game.Menu.PushMenu(Menu.MenuType.Quit);
										break;
									case Interface.Command.CameraInterior:
										// camera: interior
										MainLoop.SaveCameraSettings();
										bool lookahead = false;
										if (World.CameraMode != World.CameraViewMode.InteriorLookAhead & World.CameraRestriction == World.CameraRestrictionMode.NotAvailable)
										{
											Game.AddMessage(Interface.GetInterfaceString("notification_interior_lookahead"),
												MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											lookahead = true;
										}
										else
										{
											Game.AddMessage(Interface.GetInterfaceString("notification_interior"),
												MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
										}
										World.CameraMode = World.CameraViewMode.Interior;
										MainLoop.RestoreCameraSettings();
										bool returnToCab = false;
										for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
										{
											if (j == World.CameraCar)
											{
												if (TrainManager.PlayerTrain.Cars[j].HasInteriorView)
												{
													TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.Interior);
													World.CameraRestriction = TrainManager.PlayerTrain.Cars[j].CameraRestrictionMode;
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
											World.CameraRestriction = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestrictionMode;
										}
										//Hide bogies
										for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
										{
											TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(-1);
											TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(-1);
										}
										World.CameraAlignmentDirection = new World.CameraAlignment();
										World.CameraAlignmentSpeed = new World.CameraAlignment();
										UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
										World.UpdateAbsoluteCamera(TimeElapsed);
										World.UpdateViewingDistances();
										if (World.CameraRestriction != World.CameraRestrictionMode.NotAvailable)
										{
											if (!World.PerformCameraRestrictionTest())
											{
												World.InitializeCameraRestriction();
											}
										}
										if (lookahead)
										{
											World.CameraMode = World.CameraViewMode.InteriorLookAhead;
										}
										break;
									case Interface.Command.CameraExterior:
										// camera: exterior
										Game.AddMessage(Interface.GetInterfaceString("notification_exterior") + " " + (World.CameraCar + 1), MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
										SaveCameraSettings();
										World.CameraMode = World.CameraViewMode.Exterior;
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
										World.CameraAlignmentDirection = new World.CameraAlignment();
										World.CameraAlignmentSpeed = new World.CameraAlignment();
										UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
										World.UpdateAbsoluteCamera(TimeElapsed);
										World.UpdateViewingDistances();
										break;
									case Interface.Command.CameraTrack:
									case Interface.Command.CameraFlyBy:
										// camera: track / fly-by
									{
										SaveCameraSettings();
										if (Interface.CurrentControls[i].Command == Interface.Command.CameraTrack)
										{
											World.CameraMode = World.CameraViewMode.Track;
											Game.AddMessage(Interface.GetInterfaceString("notification_track"),
												MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
										}
										else
										{
											if (World.CameraMode == World.CameraViewMode.FlyBy)
											{
												World.CameraMode = World.CameraViewMode.FlyByZooming;
												Game.AddMessage(
													Interface.GetInterfaceString("notification_flybyzooming"),
													MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
													MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											}
											else
											{
												World.CameraMode = World.CameraViewMode.FlyBy;
												Game.AddMessage(
													Interface.GetInterfaceString("notification_flybynormal"),
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
										World.CameraAlignmentDirection = new World.CameraAlignment();
										World.CameraAlignmentSpeed = new World.CameraAlignment();
										UpdateViewport(ViewPortChangeMode.NoChange);
										World.UpdateAbsoluteCamera(TimeElapsed);
										World.UpdateViewingDistances();
									}
										break;
									case Interface.Command.CameraPreviousPOI:
										//If we are in the exterior train view, shift down one car until we hit the last car
										if (World.CameraMode == World.CameraViewMode.Exterior)
										{
											if (World.CameraCar < TrainManager.PlayerTrain.Cars.Length - 1)
											{
												World.CameraCar++;
												Game.AddMessage(Interface.GetInterfaceString("notification_exterior") + " " + (World.CameraCar + 1), MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											}
											return;
										}
										//Otherwise, check if we can move down to the previous POI
										if (Game.ApplyPointOfInterest(-1, true))
										{
											if (World.CameraMode != World.CameraViewMode.Track &
												World.CameraMode != World.CameraViewMode.FlyBy &
												World.CameraMode != World.CameraViewMode.FlyByZooming)
											{
												World.CameraMode = World.CameraViewMode.Track;
												Game.AddMessage(Interface.GetInterfaceString("notification_track"),
													MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
													MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											}
											double z = World.CameraCurrentAlignment.Position.Z;
											World.CameraCurrentAlignment.Position =
												new OpenBveApi.Math.Vector3(World.CameraCurrentAlignment.Position.X,
													World.CameraCurrentAlignment.Position.Y, 0.0);
											World.CameraCurrentAlignment.Zoom = 0.0;
											World.CameraAlignmentDirection = new World.CameraAlignment();
											World.CameraAlignmentSpeed = new World.CameraAlignment();
											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
											{
												TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.Exterior);
											}

											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
											{
												TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
												TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
											}
											World.CameraTrackFollower.Update(World.CameraTrackFollower.TrackPosition + z, true, false);
											World.CameraCurrentAlignment.TrackPosition = World.CameraTrackFollower.TrackPosition;
											World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
											UpdateViewport(ViewPortChangeMode.NoChange);
											World.UpdateAbsoluteCamera(TimeElapsed);
											World.UpdateViewingDistances();
										}
										break;
									case Interface.Command.CameraNextPOI:
										//If we are in the exterior train view, shift up one car until we hit index 0
										if (World.CameraMode == World.CameraViewMode.Exterior)
										{
											if (World.CameraCar > 0)
											{
												World.CameraCar--;
												Game.AddMessage(Interface.GetInterfaceString("notification_exterior") + " " + (World.CameraCar + 1), MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											}
											return;
										}
										//Otherwise, check if we can move up to the next POI
										if (Game.ApplyPointOfInterest(1, true))
										{
											if (World.CameraMode != World.CameraViewMode.Track &
												World.CameraMode != World.CameraViewMode.FlyBy &
												World.CameraMode != World.CameraViewMode.FlyByZooming)
											{
												World.CameraMode = World.CameraViewMode.Track;
												Game.AddMessage(Interface.GetInterfaceString("notification_track"),
													MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
													MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											}
											double z = World.CameraCurrentAlignment.Position.Z;
											World.CameraCurrentAlignment.Position =
												new OpenBveApi.Math.Vector3(World.CameraCurrentAlignment.Position.X,
													World.CameraCurrentAlignment.Position.Y, 0.0);
											World.CameraCurrentAlignment.Zoom = 0.0;
											World.CameraAlignmentDirection = new World.CameraAlignment();
											World.CameraAlignmentSpeed = new World.CameraAlignment();
											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
											{
												TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.Exterior);
											}
											for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
											{
												TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
												TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
											}
											World.CameraTrackFollower.Update(World.CameraTrackFollower.TrackPosition + z, true, false);
											World.CameraCurrentAlignment.TrackPosition =
												World.CameraTrackFollower.TrackPosition;
											World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
											UpdateViewport(ViewPortChangeMode.NoChange);
											World.UpdateAbsoluteCamera(TimeElapsed);
											World.UpdateViewingDistances();
										}
										break;
									case Interface.Command.CameraReset:
										// camera: reset
										if (World.CameraMode == World.CameraViewMode.Interior |
											World.CameraMode == World.CameraViewMode.InteriorLookAhead)
										{
											World.CameraCurrentAlignment.Position = new OpenBveApi.Math.Vector3(0.0, 0.0,
												0.0);
										}
										World.CameraCurrentAlignment.Yaw = 0.0;
										World.CameraCurrentAlignment.Pitch = 0.0;
										World.CameraCurrentAlignment.Roll = 0.0;
										if (World.CameraMode == World.CameraViewMode.Track)
										{
											World.CameraTrackFollower.Update(
												TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition, true,
												false);
										}
										else if (World.CameraMode == World.CameraViewMode.FlyBy |
												 World.CameraMode == World.CameraViewMode.FlyByZooming)
										{
											if (TrainManager.PlayerTrain.Specs.CurrentAverageSpeed >= 0.0)
											{
												double d = 30.0 +
														   4.0*TrainManager.PlayerTrain.Specs.CurrentAverageSpeed;
												World.CameraTrackFollower.Update(
													TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower
														.TrackPosition + d, true, false);
											}
											else
											{
												double d = 30.0 -
														   4.0*TrainManager.PlayerTrain.Specs.CurrentAverageSpeed;
												World.CameraTrackFollower.Update(
													TrainManager.PlayerTrain.Cars[
														TrainManager.PlayerTrain.Cars.Length - 1].RearAxle.Follower
														.TrackPosition - d, true, false);
											}
										}
										World.CameraCurrentAlignment.TrackPosition =
											World.CameraTrackFollower.TrackPosition;
										World.CameraCurrentAlignment.Zoom = 0.0;
										World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
										World.CameraAlignmentDirection = new World.CameraAlignment();
										World.CameraAlignmentSpeed = new World.CameraAlignment();
										UpdateViewport(ViewPortChangeMode.NoChange);
										World.UpdateAbsoluteCamera(TimeElapsed);
										World.UpdateViewingDistances();
										if ((World.CameraMode == World.CameraViewMode.Interior |
											 World.CameraMode == World.CameraViewMode.InteriorLookAhead) &
											World.CameraRestriction == World.CameraRestrictionMode.On)
										{
											if (!World.PerformCameraRestrictionTest())
											{
												World.InitializeCameraRestriction();
											}
										}
										break;
									case Interface.Command.CameraRestriction:
										// camera: restriction
										if (World.CameraRestriction != World.CameraRestrictionMode.NotAvailable)
										{
											if (World.CameraRestriction == World.CameraRestrictionMode.Off)
											{
												World.CameraRestriction = World.CameraRestrictionMode.On;
											}
											else
											{
												World.CameraRestriction = World.CameraRestrictionMode.Off;
											}
											World.InitializeCameraRestriction();
											if (World.CameraRestriction == World.CameraRestrictionMode.Off)
											{
												Game.AddMessage(
													Interface.GetInterfaceString("notification_camerarestriction_off"),
													MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
													MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											}
											else
											{
												Game.AddMessage(
													Interface.GetInterfaceString("notification_camerarestriction_on"),
													MessageManager.MessageDependency.CameraView, Interface.GameMode.Expert,
													MessageColor.White, Game.SecondsSinceMidnight + 2.0, null);
											}
										}
										break;
									case Interface.Command.SinglePower:
										// single power
										if (TrainManager.PlayerTrain.Specs.SingleHandle)
										{
											int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
											if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
											{
												TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
											}
											else if (b == 1 & TrainManager.PlayerTrain.Specs.HasHoldBrake)
											{
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 0, false);
												TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
											}
											else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
											{
												TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
											}
											else if (b > 0)
											{
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, -1, true);
											}
											else
											{
												int p = TrainManager.PlayerTrain.Handles.Power.Driver;
												if (p < TrainManager.PlayerTrain.Specs.MaximumPowerNotch)
												{
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 1, true, 0, true);
												}
											}
										}
										break;
									case Interface.Command.SingleNeutral:
										// single neutral
										if (TrainManager.PlayerTrain.Specs.SingleHandle)
										{
											int p = TrainManager.PlayerTrain.Handles.Power.Driver;
											if (p > 0)
											{
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, -1, true, 0, true);
											}
											else
											{
												int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
												if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
												{
													TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
												}
												else if (b == 1 & TrainManager.PlayerTrain.Specs.HasHoldBrake)
												{
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 0, false);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												}
												else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												}
												else if (b > 0)
												{
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, -1, true);
												}
											}
										}
										break;
									case Interface.Command.SingleBrake:
										// single brake
										if (TrainManager.PlayerTrain.Specs.SingleHandle)
										{
											int p = TrainManager.PlayerTrain.Handles.Power.Driver;
											if (p > 0)
											{
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, -1, true, 0, true);
											}
											else
											{
												int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
												if (TrainManager.PlayerTrain.Specs.HasHoldBrake & b == 0 &
													!TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												}
												else if (b < TrainManager.PlayerTrain.Specs.MaximumBrakeNotch)
												{
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 1, true);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												}
											}
										}
										break;
									case Interface.Command.SingleEmergency:
										// single emergency
										if (TrainManager.PlayerTrain.Specs.SingleHandle)
										{
											TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
										}
										break;
									case Interface.Command.PowerIncrease:
										// power increase
										if (!TrainManager.PlayerTrain.Specs.SingleHandle)
										{
											int p = TrainManager.PlayerTrain.Handles.Power.Driver;
											if (p < TrainManager.PlayerTrain.Specs.MaximumPowerNotch)
											{
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, 1, true, 0, true);
											}
										}
										break;
									case Interface.Command.PowerDecrease:
										// power decrease
										if (!TrainManager.PlayerTrain.Specs.SingleHandle)
										{
											int p = TrainManager.PlayerTrain.Handles.Power.Driver;
											if (p > 0)
											{
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, -1, true, 0, true);
											}
										}
										break;
									case Interface.Command.BrakeIncrease:
										// brake increase
										if (!TrainManager.PlayerTrain.Specs.SingleHandle)
										{
											int d = TrainManager.PlayerTrain.DriverCar;
											if (TrainManager.PlayerTrain.Cars[d].Specs.BrakeType ==
												TrainManager.CarBrakeType.AutomaticAirBrake)
											{
												if (TrainManager.PlayerTrain.Specs.HasHoldBrake &
													TrainManager.PlayerTrain.Handles.AirBrake.Handle.Driver ==
													TrainManager.AirBrakeHandleState.Release &
													!TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												}
												else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
														TrainManager.AirBrakeHandleState.Lap);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												}
												else if (TrainManager.PlayerTrain.Handles.AirBrake.Handle.Driver ==
														 TrainManager.AirBrakeHandleState.Lap)
												{
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
														TrainManager.AirBrakeHandleState.Service);
												}
												else if (TrainManager.PlayerTrain.Handles.AirBrake.Handle.Driver ==
														 TrainManager.AirBrakeHandleState.Release)
												{
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
														TrainManager.AirBrakeHandleState.Lap);
												}
											}
											else
											{
												int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
												if (TrainManager.PlayerTrain.Specs.HasHoldBrake & b == 0 &
													!TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												}
												else if (b < TrainManager.PlayerTrain.Specs.MaximumBrakeNotch)
												{
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 1, true);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												}
											}
										}
										break;
									case Interface.Command.BrakeDecrease:
										// brake decrease
										if (!TrainManager.PlayerTrain.Specs.SingleHandle)
										{
											int d = TrainManager.PlayerTrain.DriverCar;
											if (TrainManager.PlayerTrain.Cars[d].Specs.BrakeType ==
												TrainManager.CarBrakeType.AutomaticAirBrake)
											{
												if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
												{
													TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
												}
												else if (TrainManager.PlayerTrain.Specs.HasHoldBrake &
														 TrainManager.PlayerTrain.Handles.AirBrake.Handle.Driver ==
														 TrainManager.AirBrakeHandleState.Lap &
														 !TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												}
												else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
														TrainManager.AirBrakeHandleState.Release);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												}
												else if (TrainManager.PlayerTrain.Handles.AirBrake.Handle.Driver ==
														 TrainManager.AirBrakeHandleState.Lap)
												{
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain,
														TrainManager.AirBrakeHandleState.Release);
												}
												else if (TrainManager.PlayerTrain.Handles.AirBrake.Handle.Driver ==
														 TrainManager.AirBrakeHandleState.Service)
												{
													TrainManager.ApplyAirBrakeHandle(
														TrainManager.PlayerTrain,
														TrainManager.AirBrakeHandleState.Lap);
												}
											}
											else
											{
												int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
												if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
												{
													TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
												}
												else if (b == 1 & TrainManager.PlayerTrain.Specs.HasHoldBrake)
												{
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 0, false);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												}
												else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
												{
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												}
												else if (b > 0)
												{
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, -1, true);
												}
											}
										}
										break;
									case Interface.Command.BrakeEmergency:
										// brake emergency
										if (!TrainManager.PlayerTrain.Specs.SingleHandle || Interface.CurrentOptions.AllowAxisEB == false)
										{
											TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
										}
										break;
									case Interface.Command.DeviceConstSpeed:
										// const speed
										if (TrainManager.PlayerTrain.Specs.HasConstSpeed)
										{
											TrainManager.PlayerTrain.Specs.CurrentConstSpeed =
												!TrainManager.PlayerTrain.Specs.CurrentConstSpeed;
										}
										break;
									case Interface.Command.ReverserForward:
										// reverser forward
										if (TrainManager.PlayerTrain.Handles.Reverser.Driver < 1)
										{
											TrainManager.ApplyReverser(TrainManager.PlayerTrain, 1, true);
										}
										break;
									case Interface.Command.ReverserBackward:
										// reverser backward
										if (TrainManager.PlayerTrain.Handles.Reverser.Driver > -1)
										{
											TrainManager.ApplyReverser(TrainManager.PlayerTrain, -1, true);
										}
										break;
									case Interface.Command.HornPrimary:
									case Interface.Command.HornSecondary:
									case Interface.Command.HornMusic:
										// horn
										{
										int j = Interface.CurrentControls[i].Command == Interface.Command.HornPrimary
											? 0 : Interface.CurrentControls[i].Command == Interface.Command.HornSecondary ? 1 : 2;
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
									case Interface.Command.DoorsLeft:
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
									case Interface.Command.DoorsRight:
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
									case Interface.Command.SecurityS:
									case Interface.Command.SecurityA1:
									case Interface.Command.SecurityA2:
									case Interface.Command.SecurityB1:
									case Interface.Command.SecurityB2:
									case Interface.Command.SecurityC1:
									case Interface.Command.SecurityC2:
									case Interface.Command.SecurityD:
									case Interface.Command.SecurityE:
									case Interface.Command.SecurityF:
									case Interface.Command.SecurityG:
									case Interface.Command.SecurityH:
									case Interface.Command.SecurityI:
									case Interface.Command.SecurityJ:
									case Interface.Command.SecurityK:
									case Interface.Command.SecurityL:
									case Interface.Command.SecurityM:
									case Interface.Command.SecurityN:
									case Interface.Command.SecurityO:
									case Interface.Command.SecurityP:
#pragma warning restore 618
									case Interface.Command.WiperSpeedUp:
									case Interface.Command.WiperSpeedDown:
									case Interface.Command.FillFuel:
									case Interface.Command.LiveSteamInjector:
									case Interface.Command.ExhaustSteamInjector:
									case Interface.Command.IncreaseCutoff:
									case Interface.Command.DecreaseCutoff:
									case Interface.Command.Blowers:
									case Interface.Command.EngineStart:
									case Interface.Command.EngineStop:
									case Interface.Command.GearUp:
									case Interface.Command.GearDown:
									case Interface.Command.RaisePantograph:
									case Interface.Command.LowerPantograph:
									case Interface.Command.MainBreaker:
										if (TrainManager.PlayerTrain.Plugin != null)
										{
											TrainManager.PlayerTrain.Plugin.KeyDown(
												Interface.SecurityToVirtualKey(Interface.CurrentControls[i].Command));
										}
										break;



									case Interface.Command.TimetableToggle:
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
									case Interface.Command.DebugWireframe:
										// option: wireframe
										Renderer.OptionWireframe = !Renderer.OptionWireframe;
										if (Renderer.OptionWireframe)
										{
											GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
										}
										else
										{
											GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
										}
										Renderer.StaticOpaqueForceUpdate = true;
										break;
									case Interface.Command.DebugNormals:
										// option: normals
										Renderer.OptionNormals = !Renderer.OptionNormals;
										Renderer.StaticOpaqueForceUpdate = true;
										break;
									case Interface.Command.ShowEvents:
										Interface.CurrentOptions.ShowEvents = !Interface.CurrentOptions.ShowEvents;
										break;
									case Interface.Command.MiscAI:
										// option: AI
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert)
										{
											Game.AddMessage(
												Interface.GetInterfaceString("notification_notavailableexpert"),
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
														Interface.GetInterfaceString("notification_aiunable"),
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
									case Interface.Command.MiscInterfaceMode:
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
											default:
												Renderer.CurrentOutputMode = Renderer.OutputMode.Default;
												break;
										}
										break;
									case Interface.Command.MiscBackfaceCulling:
										// option: backface culling
										Renderer.OptionBackfaceCulling = !Renderer.OptionBackfaceCulling;
										Renderer.StaticOpaqueForceUpdate = true;
										Game.AddMessage(
											Interface.GetInterfaceString(Renderer.OptionBackfaceCulling
												? "notification_backfaceculling_on"
												: "notification_backfaceculling_off"), MessageManager.MessageDependency.None,
											Interface.GameMode.Expert, MessageColor.White,
											Game.SecondsSinceMidnight + 2.0, null);
										break;
									case Interface.Command.MiscCPUMode:
										// option: limit frame rate
										LimitFramerate = !LimitFramerate;
										Game.AddMessage(
											Interface.GetInterfaceString(LimitFramerate
												? "notification_cpu_low"
												: "notification_cpu_normal"), MessageManager.MessageDependency.None,
											Interface.GameMode.Expert, MessageColor.White,
											Game.SecondsSinceMidnight + 2.0, null);
										break;
									case Interface.Command.DebugBrakeSystems:
										// option: brake systems
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert)
										{
											Game.AddMessage(
												Interface.GetInterfaceString("notification_notavailableexpert"),
												MessageManager.MessageDependency.None, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 5.0, null);
										}
										else
										{
											Renderer.OptionBrakeSystems = !Renderer.OptionBrakeSystems;
										}
										break;
									case Interface.Command.MenuActivate:
										// menu
										Game.Menu.PushMenu(Menu.MenuType.Top);
										break;
									case Interface.Command.MiscPause:
										// pause
										Game.CurrentInterface = Game.InterfaceType.Pause;
										break;
									case Interface.Command.MiscClock:
										// clock
										Renderer.OptionClock = !Renderer.OptionClock;
										break;
									case Interface.Command.MiscTimeFactor:
										// time factor
										if (!PluginManager.Plugin.DisableTimeAcceleration)
										{
											if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert)
											{
												Game.AddMessage(
													Interface.GetInterfaceString("notification_notavailableexpert"),
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
									case Interface.Command.MiscSpeed:
										// speed
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert)
										{
											Game.AddMessage(
												Interface.GetInterfaceString("notification_notavailableexpert"),
												MessageManager.MessageDependency.None, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 5.0, null);
										}
										else
										{
											Renderer.OptionSpeed++;
											if ((int) Renderer.OptionSpeed >= 3) Renderer.OptionSpeed = 0;
										}
										break;
									case Interface.Command.MiscGradient:
										// gradient
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert)
										{
											Game.AddMessage(
												Interface.GetInterfaceString("notification_notavailableexpert"),
												MessageManager.MessageDependency.None, Interface.GameMode.Expert,
												MessageColor.White, Game.SecondsSinceMidnight + 5.0, null);
										}
										else
										{
											Renderer.OptionGradient++;
											if ((int)Renderer.OptionGradient >= 3) Renderer.OptionGradient = 0;
										}
										break;
									case Interface.Command.MiscFps:
										// fps
										Renderer.OptionFrameRates = !Renderer.OptionFrameRates;
										break;
									case Interface.Command.MiscFullscreen:
										// toggle fullscreen
										Screen.ToggleFullscreen();
										break;
									case Interface.Command.MiscMute:
										// mute
										Sounds.GlobalMute = !Sounds.GlobalMute;
										Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
										break;
								case Interface.Command.RouteInformation:
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
									Game.routeInfoOverlay.ProcessCommand(Interface.Command.RouteInformation);
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
									case Interface.Command.SecurityS:
									case Interface.Command.SecurityA1:
									case Interface.Command.SecurityA2:
									case Interface.Command.SecurityB1:
									case Interface.Command.SecurityB2:
									case Interface.Command.SecurityC1:
									case Interface.Command.SecurityC2:
									case Interface.Command.SecurityD:
									case Interface.Command.SecurityE:
									case Interface.Command.SecurityF:
									case Interface.Command.SecurityG:
									case Interface.Command.SecurityH:
									case Interface.Command.SecurityI:
									case Interface.Command.SecurityJ:
									case Interface.Command.SecurityK:
									case Interface.Command.SecurityL:
									case Interface.Command.SecurityM:
									case Interface.Command.SecurityN:
									case Interface.Command.SecurityO:
									case Interface.Command.SecurityP:
#pragma warning restore 618
									case Interface.Command.WiperSpeedUp:
									case Interface.Command.WiperSpeedDown:
									case Interface.Command.FillFuel:
									case Interface.Command.LiveSteamInjector:
									case Interface.Command.ExhaustSteamInjector:
									case Interface.Command.IncreaseCutoff:
									case Interface.Command.DecreaseCutoff:
									case Interface.Command.Blowers:
									case Interface.Command.EngineStart:
									case Interface.Command.EngineStop:
									case Interface.Command.GearUp:
									case Interface.Command.GearDown:
									case Interface.Command.RaisePantograph:
									case Interface.Command.LowerPantograph:
									case Interface.Command.MainBreaker:
										if (TrainManager.PlayerTrain.Plugin != null)
										{
											TrainManager.PlayerTrain.Plugin.KeyUp(
												Interface.SecurityToVirtualKey(Interface.CurrentControls[i].Command));
										}
										break;

									case Interface.Command.HornPrimary:
									case Interface.Command.HornSecondary:
									case Interface.Command.HornMusic:
										// horn
										int j = Interface.CurrentControls[i].Command == Interface.Command.HornPrimary
											? 0
											: Interface.CurrentControls[i].Command == Interface.Command.HornSecondary ? 1 : 2;
										int d = TrainManager.PlayerTrain.DriverCar;
										if (TrainManager.PlayerTrain.Cars[d].Horns.Length > j)
										{
											//Required for detecting the end of the loop and triggering the stop sound
											TrainManager.PlayerTrain.Cars[d].Horns[j].Stop();
										}
										break;
									case Interface.Command.DoorsLeft:
										TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Doors[0].ButtonPressed = false;
										if (TrainManager.PlayerTrain.Plugin != null)
										{
											TrainManager.PlayerTrain.Plugin.KeyUp(VirtualKeys.LeftDoors);
										}
										break;
									case Interface.Command.DoorsRight:
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
