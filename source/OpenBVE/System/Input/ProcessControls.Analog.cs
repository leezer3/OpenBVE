using System;
using LibRender2.Cameras;
using LibRender2.Overlays;
using OpenBveApi.Interface;
using OpenBveApi.Textures;
using TrainManager.Handles;

namespace OpenBve
{
	internal static partial class MainLoop
	{
		private static void ProcessAnalogControl(double TimeElapsed, ref Control Control)
		{
			// analog control
			if (Control.AnalogState == 0.0)
			{
				return;
			}
			// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault : Many controls can't be assigned to an axis
			switch (Control.Command)
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
						double a = Control.AnalogState;
						if (Control.Command == Translations.Command.PowerFullAxis)
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
							double a = Control.AnalogState;
							if (Control.Command ==
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
								double a = Control.AnalogState;
								if (Control.Command ==
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
								double a = Control.AnalogState;
								if (Control.Command ==
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
							int p = (int)Math.Round(Control.AnalogState * TrainManager.PlayerTrain.Handles.Power.MaximumNotch);
							int b = (int)Math.Round(-Control.AnalogState * (TrainManager.PlayerTrain.Handles.Brake.MaximumNotch + 2.0));
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
							int p = (int)Math.Round(Control.AnalogState * TrainManager.PlayerTrain.Handles.Power.MaximumNotch);
							int b = (int)Math.Round(-Control.AnalogState * (TrainManager.PlayerTrain.Handles.Brake.MaximumNotch + 1.0));
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
					TrainManager.PlayerTrain.Handles.Reverser.ApplyState((ReverserPosition)(int)Math.Round(Control.AnalogState));
					break;
				case Translations.Command.CameraMoveForward:
				case Translations.Command.CameraMoveBackward:
				case Translations.Command.CameraMoveLeft:
				case Translations.Command.CameraMoveRight:
				case Translations.Command.CameraMoveUp:
				case Translations.Command.CameraMoveDown:
					Program.Renderer.Camera.Move(Control.Command, Control.AnalogState);
					break;
				case Translations.Command.CameraRotateLeft:
				case Translations.Command.CameraRotateRight:
				case Translations.Command.CameraRotateUp:
				case Translations.Command.CameraRotateDown:
				case Translations.Command.CameraRotateCCW:
				case Translations.Command.CameraRotateCW:
					Program.Renderer.Camera.Rotate(Control.Command, Control.AnalogState);
					break;
				case Translations.Command.CameraZoomIn:
					// camera zoom in
					if (TimeElapsed > 0.0)
					{
						Program.Renderer.Camera.AlignmentDirection.Zoom = -CameraProperties.ZoomTopSpeed * Control.AnalogState;
					}

					break;
				case Translations.Command.CameraZoomOut:
					// camera zoom out
					if (TimeElapsed > 0.0)
					{
						Program.Renderer.Camera.AlignmentDirection.Zoom = CameraProperties.ZoomTopSpeed * Control.AnalogState;
					}

					break;
				case Translations.Command.TimetableUp:
					// timetable up
					if (TimeElapsed > 0.0)
					{
						const double scrollSpeed = 250.0;
						switch (Program.Renderer.CurrentTimetable)
						{
							case DisplayedTimetable.Default:
								Timetable.DefaultTimetablePosition += scrollSpeed * Control.AnalogState * TimeElapsed;
								if (Timetable.DefaultTimetablePosition > 0.0)
									Timetable.DefaultTimetablePosition = 0.0;
								break;
							case DisplayedTimetable.Custom:
								Timetable.CustomTimetablePosition += scrollSpeed * Control.AnalogState * TimeElapsed;
								if (Timetable.CustomTimetablePosition > 0.0)
									Timetable.CustomTimetablePosition = 0.0;
								break;
						}
					}

					break;
				case Translations.Command.TimetableDown:
					// timetable down
					if (TimeElapsed > 0.0)
					{
						const double scrollSpeed = 250.0;
						switch (Program.Renderer.CurrentTimetable)
						{
							case DisplayedTimetable.Default:
								Timetable.DefaultTimetablePosition -= scrollSpeed * Control.AnalogState * TimeElapsed;
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

								break;
							case DisplayedTimetable.Custom:
								Timetable.CustomTimetablePosition -= scrollSpeed * Control.AnalogState * TimeElapsed;
								Texture texture = Timetable.CurrentCustomTimetableDaytimeTexture ?? Timetable.CurrentCustomTimetableNighttimeTexture;
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
								break;
						}
					}
					break;
			}
		}
	}
}
