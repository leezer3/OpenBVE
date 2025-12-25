using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DavyKager;
using LibRender2.Cameras;
using LibRender2.Screens;
using OpenBve.Input;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using TrainManager;
using ButtonState = OpenTK.Input.ButtonState;

namespace OpenBve
{
	internal static partial class MainLoop
	{
		internal static QuitMode Quit = QuitMode.ContinueGame;
		/// <summary>BlockKeyRepeat should be set to 'true' whilst processing a KeyUp or KeyDown event.</summary>
		internal static bool BlockKeyRepeat;
		/// <summary>The current simulation time-factor</summary>
		internal static int TimeFactor = 1;
		
		internal static double timeSinceLastMouseEvent;

		internal static LaunchParameters currentResult;

		private static double kioskModeTimer;

		internal static void StartLoopEx(LaunchParameters result)
		{
			Program.Sounds.Initialize(Interface.CurrentOptions.SoundRange);

			Program.FileSystem.AppendToLogFile(@"Attached Joysticks:", false);
			Program.FileSystem.AppendToLogFile(@"--------------------", false);
			for (int i = 0; i < Program.Joysticks.AttachedJoysticks.Count; i++)
			{
				Guid key = Program.Joysticks.AttachedJoysticks.ElementAt(i).Key;
				Program.FileSystem.AppendToLogFile(Program.Joysticks.AttachedJoysticks[key].ToString(), false);
			}
			Program.FileSystem.AppendToLogFile(@"--------------------", false);

			Program.FileSystem.AppendToLogFile("Detected Platform: " + Program.CurrentHost.Platform);
			Program.FileSystem.AppendToLogFile("Backend: " + (Interface.CurrentOptions.PreferNativeBackend ? "Native" : "SDL2"));
			Program.FileSystem.AppendToLogFile("User Interface Size: " + Interface.CurrentOptions.UserInterfaceFolder);
			Program.FileSystem.AppendToLogFile("User Interface Scale Factor: " + Interface.CurrentOptions.UserInterfaceScaleFactor);
			if (Program.CurrentHost.Platform == HostPlatform.MicrosoftWindows)
			{
				Tolk.Load();
				string name = Tolk.DetectScreenReader();
				if (!string.IsNullOrEmpty(name))
				{
					Interface.CurrentOptions.ScreenReaderAvailable = true;
					Program.FileSystem.AppendToLogFile("Supported screen reader driver " + name + " initialised.");
				}
				else
				{
					Program.FileSystem.AppendToLogFile("No supported screen reader found.");
				}
			}
			
			//Process extra command line arguments supplied
			if (result.InitialStation != null)
			{
				//We have supplied a station name or index to the loader
				Program.CurrentRoute.InitialStationName = result.InitialStation;
			}
			if (result.StartTime != 0)
			{
				Program.CurrentRoute.InitialStationTime = result.StartTime;
			}

			Game.InitialAIDriver = result.AIDriver;
			Game.InitialReversedConsist = result.ReverseConsist;
			if (result.FullScreen)
			{
				Interface.CurrentOptions.FullscreenMode = true;
			}
			if (result.Width != 0 && result.Height != 0)
			{
				if (Interface.CurrentOptions.FullscreenMode)
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

			Program.FileSystem.AppendToLogFile(Interface.CurrentOptions.IsUseNewRenderer ? "Using openGL 4 (new) renderer" : "Using openGL 1.2 (old) renderer");
			if (Interface.CurrentOptions.FullscreenMode)
			{
				Program.FileSystem.AppendToLogFile("Initialising full-screen game window of size " + Interface.CurrentOptions.FullscreenWidth + " x " + Interface.CurrentOptions.FullscreenHeight);
			}
			else
			{
				Program.FileSystem.AppendToLogFile("Initialising game window of size " + Interface.CurrentOptions.WindowWidth + " x " + Interface.CurrentOptions.WindowHeight);
			}
			Screen.Initialize();
			currentResult = result;
			Program.Renderer.GameWindow.Closing += OpenTKQuit;
			Program.Renderer.GameWindow.Run();
		}

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

		internal static bool MouseGrabEnabled = false;
		internal static bool MouseGrabIgnoreOnce = false;
		internal static OpenBveApi.Math.Vector2 MouseGrabTarget = new OpenBveApi.Math.Vector2(0.0, 0.0);

		/// <summary>Called when a mouse button is pressed</summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">The button arguments</param>
		internal static void mouseDownEvent(object sender, MouseButtonEventArgs e)
		{
			if (Program.Renderer.CurrentInterface == InterfaceType.LoadScreen)
			{
				// as otherwise right-click can unexpectedly operate camera spin
				return;
			}
			timeSinceLastMouseEvent = 0;
			if (e.Button == MouseButton.Right)
			{
				MouseGrabEnabled = !MouseGrabEnabled;
				MouseGrabIgnoreOnce = true;
			}
			if (e.Button == MouseButton.Left)
			{
				switch (Program.Renderer.CurrentInterface)
				{
					case InterfaceType.Normal:
						Program.Renderer.Touch.TouchCheck(new Vector2(e.X, e.Y));
						break;
					case InterfaceType.Menu:
					case InterfaceType.GLMainMenu:
						// if currently in a menu, forward the click to the menu system
						Game.Menu.ProcessMouseDown(e.X, e.Y);
						break;
					case InterfaceType.SwitchChangeMap:
						Game.SwitchChangeDialog.ProcessMouseDown(e.X, e.Y);
						break;
				}
			}
		}

		internal static void mouseUpEvent(object sender, MouseButtonEventArgs e)
		{
			if (Program.Renderer.CurrentInterface == InterfaceType.LoadScreen)
			{
				// as otherwise right-click can unexpectedly operate camera spin
				return;
			}
			timeSinceLastMouseEvent = 0;
			if (e.Button == MouseButton.Left)
			{
				if (Program.Renderer.CurrentInterface == InterfaceType.Normal)
				{
					Program.Renderer.Touch.LeaveCheck(new Vector2(e.X, e.Y));
				}
			}
		}

		/// <summary>Called when the mouse is moved</summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">The move arguments</param>
		internal static void mouseMoveEvent(object sender, MouseMoveEventArgs e)
		{
			timeSinceLastMouseEvent = 0;
			// Forward movement appropriately
			switch (Program.Renderer.CurrentInterface)
			{
				case InterfaceType.Menu:
				case InterfaceType.GLMainMenu:
					Game.Menu.ProcessMouseMove(e.X, e.Y);
					break;
				case InterfaceType.SwitchChangeMap:
					Game.SwitchChangeDialog.ProcessMouseMove(e.X, e.Y);
					break;
			}
		}

		/// <summary>Called when the state of the mouse wheel changes</summary>
		/// <param name="sender">The sender</param>
		/// <param name="e">The button arguments</param>
		internal static void mouseWheelEvent(object sender, MouseWheelEventArgs e)
		{
			timeSinceLastMouseEvent = 0;
			if (Program.Renderer.CurrentInterface >= InterfaceType.Menu)
			{
				Game.Menu.ProcessMouseScroll(e.Delta);
			}
		}

		internal static void UpdateMouse(double TimeElapsed)
		{
			if (Program.Renderer.CurrentInterface < InterfaceType.Menu)
			{
				timeSinceLastMouseEvent += TimeElapsed;
			}
			else
			{
				timeSinceLastMouseEvent = 0; //Always show the mouse in the menu
			}

			if (Interface.CurrentOptions.CursorHideDelay > 0 && timeSinceLastMouseEvent > Interface.CurrentOptions.CursorHideDelay)
			{
				Program.Renderer.GameWindow.CursorVisible = false;
			}
			else
			{
				Program.Renderer.GameWindow.CursorVisible = true;
			}

			if (MainLoop.MouseGrabEnabled)
			{
				double factor;
				if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead)
				{
					factor = 1.0;
				}
				else
				{
					factor = 3.0;
				}

				Program.Renderer.Camera.AlignmentDirection.Yaw += factor * MouseGrabTarget.X;
				Program.Renderer.Camera.AlignmentDirection.Pitch -= factor * MouseGrabTarget.Y;
				MouseGrabTarget = OpenBveApi.Math.Vector2.Null;
			}
		}

		//
		// KEYBOARD EVENTS
		//
		private static KeyboardModifier CurrentKeyboardModifier = KeyboardModifier.None;

		internal static void ProcessKeyboard()
		{
			if (Interface.CurrentOptions.UseJoysticks)
			{
				for (int k = 0; k < Program.Joysticks.AttachedJoysticks.Count; k++)
				{
					Guid guid = Program.Joysticks.AttachedJoysticks.ElementAt(k).Key;
					Program.Joysticks.AttachedJoysticks[guid].Poll();
				}
			}
			if (Program.Renderer.CurrentInterface >= InterfaceType.Menu && Game.Menu.IsCustomizingControl())
			{
				if (Interface.CurrentOptions.UseJoysticks)
				{
					for (int k = 0; k < Program.Joysticks.AttachedJoysticks.Count; k++)
					{
						Guid guid = Program.Joysticks.AttachedJoysticks.ElementAt(k).Key;
						int axes = Program.Joysticks.AttachedJoysticks[guid].AxisCount();
						for (int i = 0; i < axes; i++)
						{
							double aa = Program.Joysticks.AttachedJoysticks[guid].GetAxis(i);
							if (aa < -0.75)
							{
								Game.Menu.SetControlJoyCustomData(guid, JoystickComponent.Axis, i, -1);
								return;
							}
							if (aa > 0.75)
							{
								Game.Menu.SetControlJoyCustomData(guid, JoystickComponent.Axis, i, 1);
								return;
							}
						}
						int buttons = Program.Joysticks.AttachedJoysticks[guid].ButtonCount();
						for (int i = 0; i < buttons; i++)
						{
							if (Program.Joysticks.AttachedJoysticks[guid].GetButton(i) == ButtonState.Pressed)
							{
								Game.Menu.SetControlJoyCustomData(guid, JoystickComponent.Button, i, 1);
								return;
							}
						}
						int hats = Program.Joysticks.AttachedJoysticks[guid].HatCount();
						for (int i = 0; i < hats; i++)
						{
							JoystickHatState hat = Program.Joysticks.AttachedJoysticks[guid].GetHat(i);
							if (hat.Position != HatPosition.Centered)
							{
								Game.Menu.SetControlJoyCustomData(guid, JoystickComponent.Hat, i, (int)hat.Position);
								return;
							}
						}
					}
				}
				return;
			}
			if (MouseGrabEnabled)
			{
				previousMouseState = currentMouseState;
				currentMouseState = Mouse.GetState();
				if (previousMouseState != currentMouseState)
				{
					if (MouseGrabIgnoreOnce)
					{
						MouseGrabIgnoreOnce = false;
					}
					else if (MouseGrabEnabled)
					{
						MouseGrabTarget = new OpenBveApi.Math.Vector2(currentMouseState.X - previousMouseState.X, currentMouseState.Y - previousMouseState.Y);
					}
				}
			}

			//Traverse the controls array
			for (int i = 0; i < Interface.CurrentControls.Length; i++)
			{
				Guid currentDevice = Interface.CurrentControls[i].Device;
				//Check to see if our device is currently available
				switch (Interface.CurrentControls[i].Method)
				{
					case ControlMethod.Joystick:
						if (Program.Joysticks.AttachedJoysticks.Count == 0 || !Program.Joysticks.AttachedJoysticks.ContainsKey(Interface.CurrentControls[i].Device) || !Program.Joysticks.AttachedJoysticks[Interface.CurrentControls[i].Device].IsConnected())
						{
							//Not currently connected
							continue;
						}
						break;
					case ControlMethod.RailDriver:
						if (!Program.Joysticks.AttachedJoysticks.ContainsKey(AbstractRailDriver.Guid))
						{
							//Not currently connected
							continue;
						}
						currentDevice = AbstractRailDriver.Guid;
						break;
					default:
						//Not a joystick / RD
						continue;
				}

				
				switch (Interface.CurrentControls[i].Component)
				{
					case JoystickComponent.Axis:
						// Assume that a joystick axis fulfills the criteria for the handle to be 'held' in place
						if (TrainManager.PlayerTrain != null)
						{
							TrainManager.PlayerTrain.Handles.Power.ResetSpring();
							TrainManager.PlayerTrain.Handles.Brake.ResetSpring();
						}
						var axisState = Program.Joysticks.GetAxis(currentDevice, Interface.CurrentControls[i].Element);
						if (axisState.ToString(CultureInfo.InvariantCulture) != Interface.CurrentControls[i].LastState)
						{
							Interface.CurrentControls[i].LastState = axisState.ToString(CultureInfo.InvariantCulture);
							switch (Interface.CurrentControls[i].InheritedType)
							{
								case Translations.CommandType.AnalogHalf:
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
									break;
								case Translations.CommandType.AnalogFull:
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
										Interface.CurrentControls[i].AnalogState = Math.Sign(axisState);
									}
									break;
								default:
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
										if (Interface.CurrentControls[i].DigitalState == DigitalControlState.Released | Interface.CurrentControls[i].DigitalState == DigitalControlState.ReleasedAcknowledged)
										{
											if (axisState > 0.67) Interface.CurrentControls[i].DigitalState = DigitalControlState.Pressed;
										}
										else
										{
											if (axisState < 0.33) Interface.CurrentControls[i].DigitalState = DigitalControlState.Released;
										}
									}
									break;
							}
						}
						break;
					case JoystickComponent.Button:
						//Load the current state
						var buttonState = Program.Joysticks.GetButton(currentDevice, Interface.CurrentControls[i].Element);
						//Test whether the state is the same as the last frame
						if (buttonState.ToString() != Interface.CurrentControls[i].LastState)
						{
							// Attempt to reset handle spring
							if (TrainManager.PlayerTrain != null)
							{
								TrainManager.PlayerTrain.Handles.Power.ResetSpring();
								TrainManager.PlayerTrain.Handles.Brake.ResetSpring();
							}
							if (buttonState == ButtonState.Pressed)
							{
								Interface.CurrentControls[i].AnalogState = 1.0;
								Interface.CurrentControls[i].DigitalState = DigitalControlState.Pressed;
								AddControlRepeat(i);
							}
							else
							{
								Interface.CurrentControls[i].AnalogState = 0.0;
								Interface.CurrentControls[i].DigitalState = DigitalControlState.Released;
								RemoveControlRepeat(i);
							}
							//Store the state
							Interface.CurrentControls[i].LastState = buttonState.ToString();
						}
						break;
					case JoystickComponent.Hat:
						//Load the current state
						var hatState = Program.Joysticks.GetHat(currentDevice, Interface.CurrentControls[i].Element).Position;
						//Test if the state is the same as last frame
						if (hatState.ToString() != Interface.CurrentControls[i].LastState)
						{
							// Attempt to reset handle spring
							if (TrainManager.PlayerTrain != null)
							{
								TrainManager.PlayerTrain.Handles.Power.ResetSpring();
								TrainManager.PlayerTrain.Handles.Brake.ResetSpring();
							}
							if ((int)hatState == Interface.CurrentControls[i].Direction)
							{
								Interface.CurrentControls[i].AnalogState = 1.0;
								Interface.CurrentControls[i].DigitalState = DigitalControlState.Pressed;
								AddControlRepeat(i);
							}
							else
							{
								Interface.CurrentControls[i].AnalogState = 0.0;
								Interface.CurrentControls[i].DigitalState = DigitalControlState.Released;
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
			switch (Program.Renderer.Camera.CurrentMode)
			{
				case CameraViewMode.Interior:
				case CameraViewMode.InteriorLookAhead:
					TrainManagerBase.PlayerTrain.Cars[TrainManagerBase.PlayerTrain.CameraCar].InteriorCamera = Program.Renderer.Camera.Alignment;
					break;
				case CameraViewMode.Exterior:
					Program.Renderer.Camera.SavedExterior = Program.Renderer.Camera.Alignment;
					Program.Renderer.Camera.SavedExterior.CameraCar = TrainManagerBase.PlayerTrain.CameraCar;
					break;
				case CameraViewMode.Track:
				case CameraViewMode.FlyBy:
				case CameraViewMode.FlyByZooming:
					Program.Renderer.Camera.SavedTrack = Program.Renderer.Camera.Alignment;
					break;
			}
		}

		// restore camera setting
		internal static void RestoreCameraSettings()
		{
			switch (Program.Renderer.Camera.CurrentMode)
			{
				case CameraViewMode.Interior:
				case CameraViewMode.InteriorLookAhead:
					Program.Renderer.Camera.Alignment = TrainManagerBase.PlayerTrain.Cars[TrainManagerBase.PlayerTrain.CameraCar].InteriorCamera ?? TrainManagerBase.PlayerTrain.Cars[TrainManagerBase.PlayerTrain.DriverCar].InteriorCamera ?? new CameraAlignment();
					break;
				case CameraViewMode.Exterior:
					Program.Renderer.Camera.Alignment = Program.Renderer.Camera.SavedExterior;
					TrainManagerBase.PlayerTrain.CameraCar = Program.Renderer.Camera.SavedExterior.CameraCar;
					break;
				case CameraViewMode.Track:
				case CameraViewMode.FlyBy:
				case CameraViewMode.FlyByZooming:
					Program.Renderer.Camera.Alignment = Program.Renderer.Camera.SavedTrack;
					Program.Renderer.CameraTrackFollower.UpdateAbsolute(Program.Renderer.Camera.SavedTrack.TrackPosition, true, false);
					Program.Renderer.Camera.Alignment.TrackPosition = Program.Renderer.CameraTrackFollower.TrackPosition;
					break;
			}
			Program.Renderer.Camera.Alignment.Zoom = 0.0;
			Program.Renderer.Camera.VerticalViewingAngle = Program.Renderer.Camera.OriginalVerticalViewingAngle;
		}

		
#if DEBUG

		/// <summary>Checks whether an OpenGL error has occured this frame</summary>
		/// <param name="Location">The location of the caller (The main loop or the loading screen loop)</param>
		internal static void CheckForOpenGlError(string Location) {
			if (Program.Renderer.ReShadeInUse)
			{
				return;
			}
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
						if (IntPtr.Size == 4)
						{
							/*
							 * Exceeded the 32-bit memory limit (~1.2gb due to CLR overhead)
							 * Can't try anything fancy with strings etc. here as things
							 * are most likely falling down. This also means the error logger etc.
							 * are unlikely to work (or be useful)
							 */
							message = @"Total memory usage exceeded the 32-bit memory limit. \r\nPlease use the 64-bit version of OpenBVE to run this content.";
							Environment.Exit(0);
						}
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
