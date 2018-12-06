using System;
using System.ComponentModel;
using System.Globalization;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;
using OpenTK.Input;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using ButtonState = OpenTK.Input.ButtonState;

namespace OpenBve
{
	internal static partial class MainLoop
	{
		internal enum QuitMode
		{
			ContinueGame = 0,
			QuitProgram = 1,
			ExitToMenu = 2
		}
		// declarations
		internal static bool LimitFramerate = false;
		internal static QuitMode Quit = QuitMode.ContinueGame;
		/// <summary>BlockKeyRepeat should be set to 'true' whilst processing a KeyUp or KeyDown event.</summary>
		internal static bool BlockKeyRepeat;
		/// <summary>The current simulation time-factor</summary>
		internal static int TimeFactor = 1;
		
		internal static formMain.MainDialogResult currentResult;
		//		internal static formRouteInformation RouteInformationForm;
		//		internal static Thread RouteInfoThread;
		//		internal static bool RouteInfoActive
		//		{
		//			get
		//			{
		//				return RouteInformationForm != null && RouteInformationForm.IsHandleCreated && RouteInformationForm.Visible;
		//			}
		//		}


		//		internal static AppDomain RouteInfoFormDomain;

		private static double kioskModeTimer;

		internal static void StartLoopEx(formMain.MainDialogResult result)
		{
			Sounds.Initialize();
			//Process extra command line arguments supplied
			if (result.InitialStation != null)
			{
				//We have supplied a station name or index to the loader
				Game.InitialStationName = result.InitialStation;
			}
			if (result.StartTime != default(double))
			{
				Game.InitialStationTime = result.StartTime;
			}
			if (result.AIDriver == true)
			{
				Game.InitialAIDriver = true;
			}
			if (result.FullScreen == true)
			{
				Interface.CurrentOptions.FullscreenMode = true;
			}
			if (result.Width != default(double) && result.Height != default(double))
			{
				if (Interface.CurrentOptions.FullscreenMode == true)
				{
					Interface.CurrentOptions.FullscreenWidth = result.Width;
					Interface.CurrentOptions.FullscreenHeight = result.Height;

				}
				else
				{
					Interface.CurrentOptions.WindowWidth = result.Width;
					Interface.CurrentOptions.WindowHeight = result.Height;
				}
			}
			Screen.Initialize();
			currentResult = result;
			Program.currentGameWindow.Closing += OpenTKQuit;
			Program.currentGameWindow.Run();
		}

		// --------------------------------

		// repeats


		//		private static void ThreadProc()
		//		{
		//			RouteInformationForm = new formRouteInformation();
		//			Application.Run(RouteInformationForm);
		//		}

		private static void OpenTKQuit(object sender, CancelEventArgs e)
		{
			Quit = QuitMode.QuitProgram;
		}

		/********************
			PROCESS EVENTS
		********************/
		//
		// MOUSE EVENTS
		//

		/// <summary>The current mouse state</summary>
		internal static MouseState currentMouseState, previousMouseState;

		/// <summary>Called when a mouse button is pressed</summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">The button arguments</param>
		internal static void mouseDownEvent(object sender, MouseButtonEventArgs e)
		{
			if (e.Button == MouseButton.Right)
			{
				World.MouseGrabEnabled = !World.MouseGrabEnabled;
				World.MouseGrabIgnoreOnce = true;
			}
			if (e.Button == MouseButton.Left)
			{
				// if currently in a menu, forward the click to the menu system
				if (Game.CurrentInterface == Game.InterfaceType.Menu)
				{
					Game.Menu.ProcessMouseDown(e.X, e.Y);
				}
			}
		}

		/// <summary>Called when a mouse button is released</summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">The button arguments</param>
		internal static void mouseMoveEvent(object sender, MouseMoveEventArgs e)
		{
			// if currently in a menu, forward the click to the menu system
			if (Game.CurrentInterface == Game.InterfaceType.Menu)
			{
				Game.Menu.ProcessMouseMove(e.X, e.Y);
			}
		}

		/// <summary>Called when the state of the mouse wheel changes</summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">The button arguments</param>
		internal static void mouseWheelEvent(object sender, MouseWheelEventArgs e)
		{
			if (Game.CurrentInterface == Game.InterfaceType.Menu)
			{
				Game.Menu.ProcessMouseScroll(e.Delta);
			}
		}

		//
		// KEYBOARD EVENTS
		//
		private static Interface.KeyboardModifier CurrentKeyboardModifier = Interface.KeyboardModifier.None;

		internal static void ProcessKeyboard()
		{
			if (Interface.CurrentOptions.UseJoysticks)
			{
				for (int k = 0; k < JoystickManager.AttachedJoysticks.Length; k++)
				{
					JoystickManager.AttachedJoysticks[k].Poll();
				}
			}
			if (Game.CurrentInterface == Game.InterfaceType.Menu && Game.Menu.IsCustomizingControl())
			{
				if (Interface.CurrentOptions.UseJoysticks)
				{
					for (int k = 0; k < JoystickManager.AttachedJoysticks.Length; k++)
					{
						int axes = JoystickManager.AttachedJoysticks[k].AxisCount();
						for (int i = 0; i < axes; i++)
						{
							double aa = JoystickManager.AttachedJoysticks[k].GetAxis(i);
							if (aa < -0.75)
							{
								Game.Menu.SetControlJoyCustomData(k, Interface.JoystickComponent.Axis, i, -1);
								return;
							}
							if (aa > 0.75)
							{
								Game.Menu.SetControlJoyCustomData(k, Interface.JoystickComponent.Axis, i, 1);
								return;
							}
						}
						int buttons = JoystickManager.AttachedJoysticks[k].ButtonCount();
						for (int i = 0; i < buttons; i++)
						{
							if (JoystickManager.AttachedJoysticks[k].GetButton(i) == ButtonState.Pressed)
							{
								Game.Menu.SetControlJoyCustomData(k, Interface.JoystickComponent.Button, i, 1);
								return;
							}
						}
						int hats = JoystickManager.AttachedJoysticks[k].HatCount();
						for (int i = 0; i < hats; i++)
						{
							JoystickHatState hat = JoystickManager.AttachedJoysticks[k].GetHat(i);
							if (hat.Position != HatPosition.Centered)
							{
								Game.Menu.SetControlJoyCustomData(k, Interface.JoystickComponent.Hat, i, (int)hat.Position);
								return;
							}
						}
					}
				}
				return;
			}
			if (World.MouseGrabEnabled)
			{
				previousMouseState = currentMouseState;
				currentMouseState = Mouse.GetState();
				if (previousMouseState != currentMouseState)
				{
					if (World.MouseGrabIgnoreOnce)
					{
						World.MouseGrabIgnoreOnce = false;
					}
					else if (World.MouseGrabEnabled)
					{
						World.MouseGrabTarget = new OpenBveApi.Math.Vector2(currentMouseState.X - previousMouseState.X, currentMouseState.Y - previousMouseState.Y);
					}
				}
			}

			//Traverse the controls array
			for (int i = 0; i < Interface.CurrentControls.Length; i++)
			{
				int currentDevice = Interface.CurrentControls[i].Device;
				//Check to see if our device is currently available
				switch (Interface.CurrentControls[i].Method)
				{
					case Interface.ControlMethod.Joystick:
						if (JoystickManager.AttachedJoysticks.Length == 0 || !Joystick.GetCapabilities(Interface.CurrentControls[i].Device).IsConnected)
						{
							//Not currently connected
							continue;
						}
						break;
					case Interface.ControlMethod.RailDriver:
						if (JoystickManager.RailDriverIndex == -1)
						{
							//Not currently connected
							continue;
						}
						currentDevice = JoystickManager.RailDriverIndex;
						break;
					default:
						//Not a joystick / RD
						continue;
				}

				
				switch (Interface.CurrentControls[i].Component)
				{
					case Interface.JoystickComponent.Axis:
						var axisState = JoystickManager.GetAxis(currentDevice, Interface.CurrentControls[i].Element);
						if (axisState.ToString(CultureInfo.InvariantCulture) != Interface.CurrentControls[i].LastState)
						{
							Interface.CurrentControls[i].LastState = axisState.ToString(CultureInfo.InvariantCulture);
							if (Interface.CurrentControls[i].InheritedType == Translations.CommandType.AnalogHalf)
							{
								if (Math.Sign(axisState) == Math.Sign(Interface.CurrentControls[i].Direction))
								{
									axisState = Math.Abs(axisState);
									if (axisState < Interface.CurrentOptions.JoystickAxisThreshold)
									{
										Interface.CurrentControls[i].AnalogState = 0.0;
									}
									else if (Interface.CurrentOptions.JoystickAxisThreshold != 1.0)
									{
										Interface.CurrentControls[i].AnalogState = (axisState - Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold);
									}
									else
									{
										Interface.CurrentControls[i].AnalogState = 1.0;
									}
								}
							}
							else if (Interface.CurrentControls[i].InheritedType == Translations.CommandType.AnalogFull)
							{
								axisState *= (float)Interface.CurrentControls[i].Direction;
								if (axisState > -Interface.CurrentOptions.JoystickAxisThreshold & axisState < Interface.CurrentOptions.JoystickAxisThreshold)
								{
									Interface.CurrentControls[i].AnalogState = 0.0;
								}
								else if (Interface.CurrentOptions.JoystickAxisThreshold != 1.0)
								{
									if (axisState < 0.0)
									{
										Interface.CurrentControls[i].AnalogState = (axisState + Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold);
									}
									else if (axisState > 0.0)
									{
										Interface.CurrentControls[i].AnalogState = (axisState - Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold);
									}
									else
									{
										Interface.CurrentControls[i].AnalogState = 0.0;
									}
								}
								else
								{
									Interface.CurrentControls[i].AnalogState = (double)Math.Sign(axisState);
								}
							}
							else
							{
								if (Math.Sign(axisState) == Math.Sign(Interface.CurrentControls[i].Direction))
								{
									axisState = Math.Abs(axisState);
									if (axisState < Interface.CurrentOptions.JoystickAxisThreshold)
									{
										axisState = 0.0f;
									}
									else if (Interface.CurrentOptions.JoystickAxisThreshold != 1.0)
									{
										axisState = (float)((axisState - Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold));
									}
									else
									{
										axisState = 1.0f;
									}
									if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Released | Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.ReleasedAcknowledged)
									{
										if (axisState > 0.67) Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
									}
									else
									{
										if (axisState < 0.33) Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
									}
								}
							}
						}
						break;
					case Interface.JoystickComponent.Button:
						//Load the current state
						var buttonState = JoystickManager.GetButton(currentDevice, Interface.CurrentControls[i].Element);
						//Test whether the state is the same as the last frame
						if (buttonState.ToString() != Interface.CurrentControls[i].LastState)
						{
							if (buttonState == ButtonState.Pressed)
							{
								Interface.CurrentControls[i].AnalogState = 1.0;
								Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
								AddControlRepeat(i);
							}
							else
							{
								Interface.CurrentControls[i].AnalogState = 0.0;
								Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
								RemoveControlRepeat(i);
							}
							//Store the state
							Interface.CurrentControls[i].LastState = buttonState.ToString();
						}
						break;
					case Interface.JoystickComponent.Hat:
						//Load the current state
						var hatState = JoystickManager.GetHat(currentDevice, Interface.CurrentControls[i].Element).Position;
						//Test if the state is the same as last frame
						if (hatState.ToString() != Interface.CurrentControls[i].LastState)
						{
							if ((int)hatState == Interface.CurrentControls[i].Direction)
							{
								Interface.CurrentControls[i].AnalogState = 1.0;
								Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
								AddControlRepeat(i);
							}
							else
							{
								Interface.CurrentControls[i].AnalogState = 0.0;
								Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
								RemoveControlRepeat(i);
							}
							//Store the state
							Interface.CurrentControls[i].LastState = hatState.ToString();
						}
						break;
				}
			}
		}

		// save camera setting
		internal static void SaveCameraSettings()
		{
			switch (World.CameraMode)
			{
				case CameraViewMode.Interior:
				case CameraViewMode.InteriorLookAhead:
					TrainManager.PlayerTrain.Cars[World.CameraCar].InteriorCamera = World.CameraCurrentAlignment;
					break;
				case CameraViewMode.Exterior:
					World.CameraSavedExterior = World.CameraCurrentAlignment;
					break;
				case CameraViewMode.Track:
				case CameraViewMode.FlyBy:
				case CameraViewMode.FlyByZooming:
					World.CameraSavedTrack = World.CameraCurrentAlignment;
					break;
			}
		}

		// restore camera setting
		internal static void RestoreCameraSettings()
		{
			switch (World.CameraMode)
			{
				case CameraViewMode.Interior:
				case CameraViewMode.InteriorLookAhead:
					World.CameraCurrentAlignment = TrainManager.PlayerTrain.Cars[World.CameraCar].InteriorCamera;
					break;
				case CameraViewMode.Exterior:
					World.CameraCurrentAlignment = World.CameraSavedExterior;
					break;
				case CameraViewMode.Track:
				case CameraViewMode.FlyBy:
				case CameraViewMode.FlyByZooming:
					World.CameraCurrentAlignment = World.CameraSavedTrack;
					World.CameraTrackFollower.Update(World.CameraSavedTrack.TrackPosition, true, false);
					World.CameraCurrentAlignment.TrackPosition = World.CameraTrackFollower.TrackPosition;
					break;
			}
			World.CameraCurrentAlignment.Zoom = 0.0;
			World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
		}

		
#if DEBUG

		/// <summary>Checks whether an OpenGL error has occured this frame</summary>
		/// <param name="Location">The location of the caller (The main loop or the loading screen loop)</param>
		internal static void CheckForOpenGlError(string Location) {
			var error = GL.GetError();
			if (error != ErrorCode.NoError) {
				string message = Location + ": ";
				switch (error) {
					case ErrorCode.InvalidEnum:
						message += "GL_INVALID_ENUM";
						break;
					case ErrorCode.InvalidValue:
						message += "GL_INVALID_VALUE";
						break;
					case ErrorCode.InvalidOperation:
						message += "GL_INVALID_OPERATION";
						break;
					case ErrorCode.StackOverflow:
						message += "GL_STACK_OVERFLOW";
						break;
					case ErrorCode.StackUnderflow:
						message += "GL_STACK_UNDERFLOW";
						break;
					case ErrorCode.OutOfMemory:
						message += "GL_OUT_OF_MEMORY";
						break;
					case ErrorCode.TableTooLargeExt:
						message += "GL_TABLE_TOO_LARGE";
						break;
					default:
						message += error.ToString();
						break;
				}
				throw new InvalidOperationException(message);
			}
		}
#endif
	}
}
