using System;
using LibRender2.Cameras;
using LibRender2.Overlays;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using TrainManager.Handles;

namespace OpenBve
{
	internal static partial class MainLoop
	{
		private static void ProcessAnalogControl(double timeElapsed, ref Control control)
		{
			// analog control
			if (control.AnalogState != 0.0)
			{
				switch (control.Command)
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
							double a = control.AnalogState;
							if (control.Command == Translations.Command.PowerFullAxis)
							{
								a = 0.5 * (a + 1.0);
							}

							a *= TrainManager.PlayerTrain.Handles.Power.MaximumNotch;
							int p = (int)Math.Round(a);
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
								double a = control.AnalogState;
								if (control.Command ==
								    Translations.Command.BrakeFullAxis)
								{
									a = 0.5 * (a + 1.0);
								}

								int b = (int)Math.Round(3.0 * a);
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
									double a = control.AnalogState;
									if (control.Command ==
									    Translations.Command.BrakeFullAxis)
									{
										a = 0.5 * (a + 1.0);
									}

									a *= (double)TrainManager.PlayerTrain.Handles.Brake.MaximumNotch + 2;
									int b = (int)Math.Round(a);
									bool q = b == 1;
									if (b > 0) b--;
									if (b <= TrainManager.PlayerTrain.Handles.Brake.MaximumNotch)
									{
										TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
										TrainManager.PlayerTrain.Handles.Brake.ApplyState(b, false);
									}
									else
									{
										TrainManager.PlayerTrain.Handles.EmergencyBrake.Apply();
									}

									TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(q);
								}
								else
								{
									double a = control.AnalogState;
									if (control.Command ==
									    Translations.Command.BrakeFullAxis)
									{
										a = 0.5 * (a + 1.0);
									}

									a *= (double)TrainManager.PlayerTrain.Handles.Brake.MaximumNotch + 1;
									int b = (int)Math.Round(a);
									if (b <= TrainManager.PlayerTrain.Handles.Brake.MaximumNotch)
									{
										TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
										TrainManager.PlayerTrain.Handles.Brake.ApplyState(b, false);
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
								int p = (int)Math.Round(control.AnalogState * TrainManager.PlayerTrain.Handles.Power.MaximumNotch);
								int b = (int)Math.Round(-control.AnalogState * (TrainManager.PlayerTrain.Handles.Brake.MaximumNotch + 2.0));
								if (p < 0) p = 0;
								if (b < 0) b = 0;
								bool q = b == 1;
								if (b > 0) b--;
								if (b <= TrainManager.PlayerTrain.Handles.Brake.MaximumNotch)
								{
									TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
									TrainManager.PlayerTrain.Handles.Brake.ApplyState(b, false);
									TrainManager.PlayerTrain.Handles.Power.ApplyState(p, false);
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
								int p = (int)Math.Round(control.AnalogState * TrainManager.PlayerTrain.Handles.Power.MaximumNotch);
								int b = (int)Math.Round(-control.AnalogState * (TrainManager.PlayerTrain.Handles.Brake.MaximumNotch + 1.0));
								if (p < 0) p = 0;
								if (b < 0) b = 0;
								if (b <= TrainManager.PlayerTrain.Handles.Brake.MaximumNotch)
								{
									TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
									TrainManager.PlayerTrain.Handles.Brake.ApplyState(b, false);
									TrainManager.PlayerTrain.Handles.Power.ApplyState(p, false);
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
						TrainManager.PlayerTrain.Handles.Reverser.ApplyState((ReverserPosition)(int)Math.Round(control.AnalogState));
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
							Program.Renderer.Camera.AlignmentDirection.Position.Z = s * control.AnalogState;
						}
						else
						{
							if (Program.Renderer.Camera.AtWorldEnd)
							{
								//Don't let the camera run off the end of the worldspace
								break;
							}

							Program.Renderer.Camera.AlignmentDirection.TrackPosition = CameraProperties.ExteriorTopSpeed * control.AnalogState;
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
							Program.Renderer.Camera.AlignmentDirection.Position.Z = -s * control.AnalogState;
						}
						else
						{
							Program.Renderer.Camera.AlignmentDirection.TrackPosition =
								-CameraProperties.ExteriorTopSpeed * control.AnalogState;
						}

						break;
					case Translations.Command.CameraMoveLeft:
						// camera move left
					{
						double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
						           Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
							? CameraProperties.InteriorTopSpeed
							: CameraProperties.ExteriorTopSpeed;
						Program.Renderer.Camera.AlignmentDirection.Position.X = -s * control.AnalogState;
					}
						break;
					case Translations.Command.CameraMoveRight:
						// camera move right
					{
						double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
						           Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
							? CameraProperties.InteriorTopSpeed
							: CameraProperties.ExteriorTopSpeed;
						Program.Renderer.Camera.AlignmentDirection.Position.X = s * control.AnalogState;
					}
						break;
					case Translations.Command.CameraMoveUp:
						// camera move up
					{
						double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
						           Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
							? CameraProperties.InteriorTopSpeed
							: CameraProperties.ExteriorTopSpeed;
						Program.Renderer.Camera.AlignmentDirection.Position.Y = s * control.AnalogState;
					}
						break;
					case Translations.Command.CameraMoveDown:
						// camera move down
					{
						double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
						           Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
							? CameraProperties.InteriorTopSpeed
							: CameraProperties.ExteriorTopSpeed;
						Program.Renderer.Camera.AlignmentDirection.Position.Y = -s * control.AnalogState;
					}
						break;
					case Translations.Command.CameraRotateLeft:
						// camera rotate left
					{
						double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
						           Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
							? CameraProperties.InteriorTopAngularSpeed
							: CameraProperties.ExteriorTopAngularSpeed;
						Program.Renderer.Camera.AlignmentDirection.Yaw = -s * control.AnalogState;
					}
						break;
					case Translations.Command.CameraRotateRight:
						// camera rotate right
					{
						double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
						           Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
							? CameraProperties.InteriorTopAngularSpeed
							: CameraProperties.ExteriorTopAngularSpeed;
						Program.Renderer.Camera.AlignmentDirection.Yaw = s * control.AnalogState;
					}
						break;
					case Translations.Command.CameraRotateUp:
						// camera rotate up
					{
						double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
						           Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
							? CameraProperties.InteriorTopAngularSpeed
							: CameraProperties.ExteriorTopAngularSpeed;
						Program.Renderer.Camera.AlignmentDirection.Pitch = s * control.AnalogState;
					}
						break;
					case Translations.Command.CameraRotateDown:
						// camera rotate down
					{
						double s = Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
						           Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead
							? CameraProperties.InteriorTopAngularSpeed
							: CameraProperties.ExteriorTopAngularSpeed;
						Program.Renderer.Camera.AlignmentDirection.Pitch = -s * control.AnalogState;
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
							Program.Renderer.Camera.AlignmentDirection.Roll = -s * control.AnalogState;
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
							Program.Renderer.Camera.AlignmentDirection.Roll = s * control.AnalogState;
						}

						break;
					case Translations.Command.CameraZoomIn:
						// camera zoom in
						if (timeElapsed > 0.0)
						{
							Program.Renderer.Camera.AlignmentDirection.Zoom = -CameraProperties.ZoomTopSpeed * control.AnalogState;
						}

						break;
					case Translations.Command.CameraZoomOut:
						// camera zoom out
						if (timeElapsed > 0.0)
						{
							Program.Renderer.Camera.AlignmentDirection.Zoom = CameraProperties.ZoomTopSpeed * control.AnalogState;
						}

						break;
					case Translations.Command.TimetableUp:
						// timetable up
						if (timeElapsed > 0.0)
						{
							const double scrollSpeed = 250.0;
							if (Program.Renderer.CurrentTimetable == DisplayedTimetable.Default)
							{
								Timetable.DefaultTimetablePosition += scrollSpeed * control.AnalogState * timeElapsed;
								if (Timetable.DefaultTimetablePosition > 0.0)
									Timetable.DefaultTimetablePosition = 0.0;
							}
							else if (Program.Renderer.CurrentTimetable == DisplayedTimetable.Custom)
							{
								Timetable.CustomTimetablePosition += scrollSpeed * control.AnalogState * timeElapsed;
								if (Timetable.CustomTimetablePosition > 0.0)
									Timetable.CustomTimetablePosition = 0.0;
							}
						}

						break;
					case Translations.Command.TimetableDown:
						// timetable down
						if (timeElapsed > 0.0)
						{
							const double scrollSpeed = 250.0;
							if (Program.Renderer.CurrentTimetable == DisplayedTimetable.Default)
							{
								Timetable.DefaultTimetablePosition -= scrollSpeed * control.AnalogState * timeElapsed;
								double max;
								if (Timetable.DefaultTimetableTexture != null)
								{
									Program.CurrentHost.LoadTexture(ref Timetable.DefaultTimetableTexture, OpenGlTextureWrapMode.ClampClamp);
									max = Math.Min(Program.Renderer.Screen.Height - Timetable.DefaultTimetableTexture.Height, 0.0);
								}
								else
								{
									max = 0.0;
								}

								if (Timetable.DefaultTimetablePosition < max)
								{
									Timetable.DefaultTimetablePosition = max;
								}
							}
							else if (Program.Renderer.CurrentTimetable == DisplayedTimetable.Custom)
							{
								Timetable.CustomTimetablePosition -= scrollSpeed * control.AnalogState * timeElapsed;
								Texture texture = Timetable.CurrentCustomTimetableDaytimeTexture ?? Timetable.CurrentCustomTimetableNighttimeTexture;
								double max;
								if (texture != null)
								{
									Program.CurrentHost.LoadTexture(ref texture, OpenGlTextureWrapMode.ClampClamp);
									max = Math.Min(Program.Renderer.Screen.Height - texture.Height, 0.0);
								}
								else
								{
									max = 0.0;
								}

								if (Timetable.CustomTimetablePosition < max)
								{
									Timetable.CustomTimetablePosition = max;
								}
							}
						}
						break;
				}
			}
		}
	}
}
