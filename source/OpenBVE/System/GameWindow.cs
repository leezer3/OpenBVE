using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using LibRender2;
using LibRender2.Cameras;
using LibRender2.Menu;
using LibRender2.Overlays;
using LibRender2.Screens;
using LibRender2.Trains;
using LibRender2.Viewports;
using OpenBve.Graphics;
using OpenBve.Input;
using OpenBveApi.Colors;
using OpenBveApi.Runtime;
using OpenBveApi.Interface;
using OpenBveApi.Trains;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenTK.Graphics.OpenGL;
using RouteManager2.MessageManager;
using SoundManager;
using TrainManager.Trains;
using Path = System.IO.Path;
using Vector2 = OpenTK.Vector2;
using Control = OpenBveApi.Interface.Control;
using MouseCursor = LibRender2.MouseCursor;

namespace OpenBve
{
	internal class OpenBVEGame: GameWindow
	{
		/// <summary>The current time acceleration factor</summary>
		private int TimeFactor = 1;
		private double TotalTimeElapsedForInfo;
		private double TotalTimeElapsedForSectionUpdate;
		private bool loadComplete;
		private bool firstFrame;
		private double RenderTimeElapsed;
		private double RenderRealTimeElapsed;
		//We need to explicitly specify the default constructor
		public OpenBVEGame(int width, int height, GraphicsMode currentGraphicsMode, GameWindowFlags @default): base(width, height, currentGraphicsMode, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"program","title"}), @default)
		{
			Program.FileSystem.AppendToLogFile("Creating game window with standard context.");
			if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
			{
				return;
			}
			try
			{
				var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				Icon ico = new Icon(OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(assemblyFolder, "Data"), "icon.ico"));
				Icon = ico;
			}
			catch
			{
				//it's only an icon
			}
		}

		public OpenBVEGame(int width, int height, GraphicsMode currentGraphicsMode, GameWindowFlags @default, GraphicsContextFlags flags): base(width, height, currentGraphicsMode, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"program","title"}), @default, DisplayDevice.Default, 3,3, flags)
		{
			Program.FileSystem.AppendToLogFile("Creating game window with forwards-compatible context.");
			if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
			{
				return;
			}
			try
			{
				var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				Icon ico = new Icon(OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(assemblyFolder, "Data"), "icon.ico"));
				Icon = ico;
			}
			catch
			{
				//it's only an icon
			}
		}


		//This renders the frame
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			
			if (!firstFrame)
			{
				//If the load is not complete, then we shouldn't be running the mainloop
				return;
			}

			if (Program.Renderer.RenderThreadJobWaiting)
			{
				while (!Program.Renderer.RenderThreadJobs.IsEmpty)
				{
					Program.Renderer.RenderThreadJobs.TryDequeue(out ThreadStart currentJob);
					currentJob();
					lock (currentJob)
					{
						Monitor.Pulse(currentJob);
					}
				}
			}
			
			Program.Renderer.RenderThreadJobWaiting = false;
			double TimeElapsed = RenderTimeElapsed;
			double RealTimeElapsed = RenderRealTimeElapsed;
			
			//Next, check if we're in paused/ in a menu
			if(Program.Renderer.CurrentInterface != InterfaceType.Normal)
			{
				MainLoop.UpdateControlRepeats(0.0);
				MainLoop.ProcessKeyboard();
				MainLoop.ProcessControls(0.0);
				if (Program.Renderer.CurrentInterface == InterfaceType.Pause)
				{
					Thread.Sleep(10);
				}
				Program.Renderer.RenderScene(TimeElapsed, RealTimeElapsed);
				SwapBuffers();
				if (MainLoop.Quit != QuitMode.ContinueGame)
				{
					Close();
					if (Program.CurrentHost.MonoRuntime && MainLoop.Quit == QuitMode.QuitProgram)
					{
						Environment.Exit(0);
					}
				}
				//If the menu state has not changed, don't update the rendered simulation
				return;
			}
			
			//Use the OpenTK framerate as this is much more accurate
			//Also avoids running a calculation
			if (TotalTimeElapsedForInfo >= 0.2)
			{
				Program.Renderer.FrameRate = RenderFrequency;
				TotalTimeElapsedForInfo = 0.0;
			}
			
			
			if (Program.Renderer.PreviousInterface != InterfaceType.Normal)
			{
				// Update animated objects with zero elapsed time (NOT time elapsed in menu)
				// and set again to avoid glitching
				ObjectManager.UpdateAnimatedWorldObjects(0.0, false);
				Program.Renderer.CurrentInterface = InterfaceType.Normal;
			}
			else
			{
				ObjectManager.UpdateAnimatedWorldObjects(TimeElapsed, false);
			}

			//We need to update the camera position in the render sequence
			//Not doing this means that the camera doesn't move
			// update in one piece
			if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead | Program.Renderer.Camera.CurrentMode == CameraViewMode.Exterior)
			{
				//Update the in-car camera based upon the current driver car (Cabview or passenger view)
				TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.CameraCar].UpdateCamera();
			}
			
			if (Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable || Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.Restricted3D)
			{
				TrainManager.PlayerTrain.DriverBody.Update(TimeElapsed);
			}
			//Check if we are running at an accelerated time factor-
			//Camera motion speed should be the same whatever the game speed is
			if (TimeFactor != 1)
			{
				World.UpdateAbsoluteCamera(TimeElapsed / TimeFactor);
			}
			else
			{
				World.UpdateAbsoluteCamera(TimeElapsed);
			}
			Program.TrainManager.UpdateTrainObjects(TimeElapsed, false);
			if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead | Program.Renderer.Camera.CurrentMode == CameraViewMode.Exterior)
			{
				int d = TrainManager.PlayerTrain.DriverCar;
				Program.Renderer.Camera.CurrentSpeed = TrainManager.PlayerTrain.Cars[d].CurrentSpeed;
			}
			else
			{
				Program.Renderer.Camera.CurrentSpeed = 0.0;
			}

			Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
			if (MainLoop.Quit != QuitMode.ContinueGame)
			{
				Exit();
				if (Program.CurrentHost.MonoRuntime && MainLoop.Quit == QuitMode.QuitProgram)
				{
					Environment.Exit(0);
				}				
			}

			if (Program.CurrentRoute.DynamicLighting)
			{
				Program.Renderer.Lighting.UpdateLighting(Program.CurrentRoute.SecondsSinceMidnight, Program.CurrentRoute.LightDefinitions);
			}
			Program.Renderer.RenderScene(TimeElapsed, RealTimeElapsed);
			Program.Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
			Program.Renderer.GameWindow.SwapBuffers();
			Game.UpdateBlackBox();
			// pause/menu
			
			MainLoop.UpdateControlRepeats(RealTimeElapsed);
			MainLoop.ProcessKeyboard();
			MainLoop.UpdateMouse(RealTimeElapsed);
			MainLoop.ProcessControls(TimeElapsed);
			if (Program.Joysticks.AttachedJoysticks.TryGetTypedValue(AbstractRailDriver.Guid, out AbstractRailDriver railDriver))
			{
				if (Interface.CurrentOptions.RailDriverMPH)
				{
					railDriver.SetDisplay((int)(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Specs.PerceivedSpeed * 2.23694));
				}
				else
				{
					railDriver.SetDisplay((int)(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Specs.PerceivedSpeed * 3.6));
				}
			}
			
			RenderRealTimeElapsed = 0.0;
			RenderTimeElapsed = 0.0;
				
				

#if DEBUG
			MainLoop.CheckForOpenGlError("MainLoop");
			 
#endif
			if (Interface.CurrentOptions.UnloadUnusedTextures)
			{
				Program.Renderer.TextureManager.UnloadUnusedTextures(TimeElapsed);
			}
			// finish
			try
			{
				Interface.SaveLogs();
			}
			catch
			{
				Interface.CurrentOptions.BlackBox = false;
			}
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			TimeFactor = MainLoop.TimeFactor;
			// timer
			double RealTimeElapsed;
			double TimeElapsed;
			if (Program.CurrentRoute.SecondsSinceMidnight >= Game.StartupTime)
			{
				
				RealTimeElapsed = CPreciseTimer.GetElapsedTime();
				TimeElapsed = RealTimeElapsed * TimeFactor;
				if (loadComplete && !firstFrame)
				{
					/*
					 * Our current in-game time is equal to or greater than the startup time, but the first frame has not yet been processed
					 * Therefore, reset the timer to zero as time consuming texture loads may cause us to be late at the first station
					 *
					 * Also update the viewing distances in case of jumps etc.
					 */
					Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
					RealTimeElapsed = 0.0;
					TimeElapsed = 0.0;
					firstFrame = true;
				}
			}
			else
			{
				RealTimeElapsed = 0.0;
				TimeElapsed = Game.StartupTime - Program.CurrentRoute.SecondsSinceMidnight;
			}

			//We only want to update the simulation if we aren't in a menu
			if (Program.Renderer.CurrentInterface == InterfaceType.Normal)
			{
#if DEBUG
				//If we're in debug mode and a frame takes greater than a second to render, we can safely assume that VS has hit a breakpoint
				//Check this and the sim no longer barfs because the update time was too great
				if (RealTimeElapsed > 1)
				{
					RealTimeElapsed = 0.0;
					TimeElapsed = 0.0;
				}
#endif
				TotalTimeElapsedForInfo += RealTimeElapsed;
				TotalTimeElapsedForSectionUpdate += TimeElapsed;


				if (TotalTimeElapsedForSectionUpdate >= 1.0)
				{
					if (Program.CurrentRoute.Sections.Length != 0)
					{
						Program.CurrentRoute.UpdateAllSections();
					}
					TotalTimeElapsedForSectionUpdate = 0.0;
				}

				// events

				// update simulation in chunks
				{
					const double chunkTime = 1.0/2.0;
					if (TimeElapsed <= chunkTime)
					{
						Program.CurrentRoute.SecondsSinceMidnight += TimeElapsed;
						Program.TrainManager.UpdateTrains(TimeElapsed);
					}
					else
					{
						const int maxChunks = 2;
						int chunks = Math.Min((int) Math.Round(TimeElapsed/chunkTime), maxChunks);
						double time = TimeElapsed/chunks;
						for (int i = 0; i < chunks; i++)
						{
							Program.CurrentRoute.SecondsSinceMidnight += time;
							Program.TrainManager.UpdateTrains(time);
						}
					}
				}
				Game.CurrentScore.Update(TimeElapsed);
				MessageManager.UpdateMessages(Program.Renderer.CurrentInterface != InterfaceType.Menu ? RealTimeElapsed : 0); // n.b. don't update message timeouts when in menu
				Game.UpdateScoreMessages(Program.Renderer.CurrentInterface != InterfaceType.Menu ? RealTimeElapsed : 0);

				for (int i = 0; i < InputDevicePlugin.AvailablePluginInfos.Count; i++)
				{
					if (InputDevicePlugin.AvailablePluginInfos[i].Status == InputDevicePlugin.PluginInfo.PluginStatus.Enable)
					{
						InputDevicePlugin.AvailablePlugins[i].OnUpdateFrame();
					}
				}
			}
			RenderTimeElapsed += TimeElapsed;
			RenderRealTimeElapsed += RealTimeElapsed;
		}

		protected override void OnResize(EventArgs e)
		{
			if(Width == 0 && Height == 0)
			{
				/*
				 * HACK: Don't resize if minimized
				 * 
				 * Unsure if this is an openTK bug, Windows or what
				 * but setting our width / height to zero breaks 
				 * stuff.....
				 */
				Program.Renderer.Screen.Minimized = true;
				return;
			}
			Program.Renderer.Screen.Minimized = false;
			Screen.WindowResize(Width,Height);
			if (Program.Renderer.CurrentInterface == InterfaceType.SwitchChangeMap)
			{
				// call the show method again to trigger resize
				Game.SwitchChangeDialog.Show();
			}
		}

		[DllImport("user32.dll")]
		private static extern int FindWindow(string className, string windowText);
		[DllImport("user32.dll")]
		public static extern int FindWindowEx(int parentHandle, int childAfter, string className, int windowTitle);

		[DllImport("user32.dll")]
		private static extern int ShowWindow(int hwnd, int command);
		[DllImport("user32.dll")]
		private static extern int GetDesktopWindow();
		protected override void OnLoad(EventArgs e)
		{
			if (Program.CurrentHost.Platform == HostPlatform.MicrosoftWindows && WindowState != WindowState.Fullscreen)
			{
				int w = DisplayDevice.Default.Width;
				int h = DisplayDevice.Default.Height;
				if (w == Interface.CurrentOptions.WindowWidth && h == Interface.CurrentOptions.WindowHeight)
				{
					// If we are not in full-screen, but height and width are equal to resolution, hide taskbar
					// e.g. Borderless windowed fulllscreen
					int hwnd = FindWindow("Shell_TrayWnd","");
					ShowWindow(hwnd,0);
					int hstart = FindWindowEx(GetDesktopWindow(), 0, "button", 0);
					ShowWindow(hstart,0);
					WindowBorder = WindowBorder.Hidden;
					Bounds = new Rectangle(0, 0, w, h);
				}
			}
			Program.FileSystem.AppendToLogFile("Game window initialised successfully.");
			//Initialise the loader thread queues
			Program.Renderer.Initialize();
			Program.Renderer.DetermineMaxAFLevel();
			Interface.CurrentOptions.Save(OpenBveApi.Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options.cfg"));
			HUD.LoadHUD();
			Program.Renderer.Loading.InitLoading(Program.FileSystem.GetDataFolder("In-game"), typeof(NewRenderer).Assembly.GetName().Version.ToString());
			Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
			Program.Renderer.MotionBlur.Initialize(Interface.CurrentOptions.MotionBlur);
			if (string.IsNullOrEmpty(MainLoop.currentResult.RouteFile))
			{
				Game.Menu.PushMenu(MenuType.GameStart);
				Loading.Complete = true;
				Program.Renderer.CameraTrackFollower = new TrackFollower(Program.CurrentHost);
				loadComplete = true;
			}
			else
			{
				Loading.LoadAsynchronously(MainLoop.currentResult.RouteFile, MainLoop.currentResult.RouteEncoding, MainLoop.currentResult.TrainFolder, MainLoop.currentResult.TrainEncoding);
				LoadingScreenLoop();
			}
			//Add event handler hooks for keyboard and mouse buttons
			//Do this after the renderer has init and the loop has started to prevent timing issues
			KeyDown	+= MainLoop.KeyDownEvent;
			KeyUp	+= MainLoop.KeyUpEvent;
			MouseDown	+= MainLoop.mouseDownEvent;
			MouseUp += MainLoop.mouseUpEvent;
			MouseMove	+= MainLoop.mouseMoveEvent;
			MouseWheel  += MainLoop.mouseWheelEvent;
			FileDrop += GameMenu.Instance.DragFile;

			for (int i = 0; i < InputDevicePlugin.AvailablePluginInfos.Count; i++)
			{
				if (InputDevicePlugin.AvailablePluginInfos[i].Status == InputDevicePlugin.PluginInfo.PluginStatus.Enable)
				{
					int AddControlsLength = InputDevicePlugin.AvailablePlugins[i].Controls.Length;
					Control[] AddControls = new Control[AddControlsLength];
					for (int j = 0; j < AddControlsLength; j++)
					{
						AddControls[j].Command = InputDevicePlugin.AvailablePlugins[i].Controls[j].Command;
						AddControls[j].Method = ControlMethod.InputDevicePlugin;
						AddControls[j].Option = InputDevicePlugin.AvailablePlugins[i].Controls[j].Option;
					}
					Interface.CurrentControls = Interface.CurrentControls.Concat(AddControls).ToArray();
					foreach (var Train in Program.TrainManager.Trains)
					{
						if (Train.State != TrainState.Bogus)
						{
							if (Train.IsPlayerTrain)
							{
								InputDevicePlugin.AvailablePlugins[i].SetMaxNotch(Train.Handles.Power.MaximumDriverNotch, Train.Handles.Brake.MaximumDriverNotch);
							}
						}
					}
					InputDevicePlugin.AvailablePlugins[i].KeyDown += MainLoop.InputDevicePluginKeyDown;
					InputDevicePlugin.AvailablePlugins[i].KeyUp += MainLoop.InputDevicePluginKeyUp;
				}
			}
		}
		protected override void OnClosing(CancelEventArgs e)
		{
			// Minor hack:
			// If we are currently loading, catch the first close event, and terminate the loader threads
			// before actually closing the game-window.
			if (Loading.Cancel)
			{
				return;
			}
			if (!Loading.Complete)
			{
				e.Cancel = true;
				Loading.Cancel = true;
			}

			try
			{
				//Force-save the black-box log, as otherwise we may be missing upto 30s of data
				Interface.SaveLogs(true);
			}
			catch
			{
				//Saving black-box failed, not really important
			}
			for (int i = 0; i < Program.TrainManager.Trains.Count; i++)
			{
				if (Program.TrainManager.Trains[i].State != TrainState.Bogus)
				{
					Program.TrainManager.Trains[i].UnloadPlugin();
				}
			}
			Program.Renderer.TextureManager.UnloadAllTextures(false);
			Program.Renderer.VisibilityThreadShouldRun = false;
			for (int i = 0; i < InputDevicePlugin.AvailablePluginInfos.Count; i++)
			{
				InputDevicePlugin.CallPluginUnload(i);
			}
			if (MainLoop.Quit == QuitMode.ContinueGame && Program.CurrentHost.MonoRuntime)
			{
				//More forcefully close under Mono, stuff *still* hanging around....
				Environment.Exit(0);
			}

			if (Program.CurrentHost.Platform == HostPlatform.MicrosoftWindows)
			{
				//Restore taskbar visibility if hidden
				int hwnd = FindWindow("Shell_TrayWnd","");
				ShowWindow(hwnd,1);
				int hstart = FindWindowEx(GetDesktopWindow(), 0, "button", 0);
				ShowWindow(hstart,1);
			}
			base.OnClosing(e);
		}

		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			base.OnMouseMove(e);

			if (Program.Renderer.CurrentInterface == InterfaceType.Normal)
			{
				if (Program.Renderer.Touch.MoveCheck(new Vector2(e.X, e.Y), out MouseCursor.Status Status, out MouseCursor newCursor))
				{
					if (newCursor != null)
					{
						switch (Status)
						{
							case MouseCursor.Status.Default: 
								Program.Renderer.SetCursor(newCursor.MyCursor);
								break;
							case MouseCursor.Status.Plus:
								Program.Renderer.SetCursor(newCursor.MyCursorPlus);
								break;
							case MouseCursor.Status.Minus:
								Program.Renderer.SetCursor(newCursor.MyCursorMinus);
								break;
						}
					}
					else if (AvailableCursors.CurrentCursor != null)
					{
						switch (Status)
						{
							case MouseCursor.Status.Default:
								Program.Renderer.SetCursor(AvailableCursors.CurrentCursor);
								break;
							case MouseCursor.Status.Plus:
								Program.Renderer.SetCursor(AvailableCursors.CurrentCursorPlus);
								break;
							case MouseCursor.Status.Minus:
								Program.Renderer.SetCursor(AvailableCursors.CurrentCursorMinus);
								break;
						}
					}
				}
				else
				{
					Program.Renderer.SetCursor(OpenTK.MouseCursor.Default);
				}
			}
		}

		/// <summary>This method is called once the route and train data have been preprocessed, in order to physically setup the simulation</summary>
		private void SetupSimulation()
		{
			if (Loading.Cancel)
			{
				Close();
			}

			lock (BaseRenderer.GdiPlusLock)
			{
				Timetable.CreateTimetable();
			}
			//Check if any critical errors have occured during the route or train loading
			for (int i = 0; i < Interface.LogMessages.Count; i++)
			{
				if (Interface.LogMessages[i].Type == MessageType.Critical)
				{
					string currentError = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "errors", "critical_loading" });
					currentError = currentError.Replace("[error]", Interface.LogMessages[i].Text);
					MessageBox.Show(currentError, Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "program", "title" }), MessageBoxButtons.OK, MessageBoxIcon.Hand);
					Close();
				}
			}
			Program.Renderer.Lighting.Initialize();
			Game.LogRouteName = Path.GetFileName(MainLoop.currentResult.RouteFile);
			Game.LogTrainName = Path.GetFileName(MainLoop.currentResult.TrainFolder);
			Game.LogDateTime = DateTime.Now;

			if (Interface.CurrentOptions.LoadInAdvance)
			{
				Program.Renderer.TextureManager.LoadAllTextures();
			}
			else
			{
				Program.Renderer.TextureManager.UnloadAllTextures(false);
			}
			// camera
			Program.Renderer.InitializeVisibility();
			Program.Renderer.CameraTrackFollower.UpdateAbsolute(0.0, true, false);
			Program.Renderer.CameraTrackFollower.UpdateAbsolute(-0.1, true, false);
			Program.Renderer.CameraTrackFollower.UpdateAbsolute(0.1, true, false);
			Program.Renderer.CameraTrackFollower.TriggerType = EventTriggerType.Camera;
			// starting time and track position
			Program.CurrentRoute.SecondsSinceMidnight = 0.0;
			Game.StartupTime = 0.0;
			int PlayerFirstStationIndex = Program.CurrentRoute.PlayerFirstStationIndex;
			double PlayerFirstStationPosition;
			
			{
				int s = Program.CurrentRoute.Stations[PlayerFirstStationIndex].GetStopIndex(TrainManager.PlayerTrain);
				if (s >= 0)
				{
					PlayerFirstStationPosition = Program.CurrentRoute.Stations[PlayerFirstStationIndex].Stops[s].TrackPosition;

					double TrainLength = 0.0;
					for (int c = 0; c < TrainManager.PlayerTrain.Cars.Length; c++)
					{
						TrainLength += TrainManager.PlayerTrain.Cars[c].Length;
					}

					for (int j = 0; j < Program.CurrentRoute.BufferTrackPositions.Count; j++)
					{
						if (Program.CurrentRoute.BufferTrackPositions[j].TrackIndex != 0)
						{
							// Player currently always starts on Rail0
							continue;
						}
						if (PlayerFirstStationPosition > Program.CurrentRoute.BufferTrackPositions[j].TrackPosition && PlayerFirstStationPosition - TrainLength < Program.CurrentRoute.BufferTrackPositions[j].TrackPosition)
						{
							/*
							 * HACK: The initial start position for the player train is stuck on a set of buffers
							 * This means we have to make some one the fly adjustments to the first station stop position
							 */

							//Set the start position to be the buffer position plus the train length plus 1m
							PlayerFirstStationPosition = Program.CurrentRoute.BufferTrackPositions[j].TrackPosition + TrainLength + 1;
							//Update the station stop location
							Program.CurrentRoute.Stations[PlayerFirstStationIndex].Stops[s].TrackPosition = PlayerFirstStationPosition;
							break;
						}
					}
				}
				else
				{
					PlayerFirstStationPosition = Program.CurrentRoute.Stations[PlayerFirstStationIndex].DefaultTrackPosition;
				}
				if (Program.CurrentRoute.InitialStationTime != -1)
				{
					Program.CurrentRoute.SecondsSinceMidnight = Program.CurrentRoute.InitialStationTime;
					Game.StartupTime = Program.CurrentRoute.InitialStationTime;
				}
				else
				{
					if (Program.CurrentRoute.Stations[PlayerFirstStationIndex].ArrivalTime < 0.0)
					{
						if (Program.CurrentRoute.Stations[PlayerFirstStationIndex].DepartureTime < 0.0)
						{
							Program.CurrentRoute.SecondsSinceMidnight = 0.0;
							Game.StartupTime = 0.0;
						}
						else
						{
							Program.CurrentRoute.SecondsSinceMidnight = Program.CurrentRoute.Stations[PlayerFirstStationIndex].DepartureTime -
							                            Program.CurrentRoute.Stations[PlayerFirstStationIndex].StopTime;
							Game.StartupTime = Program.CurrentRoute.Stations[PlayerFirstStationIndex].DepartureTime -
							                   Program.CurrentRoute.Stations[PlayerFirstStationIndex].StopTime;
						}
					}
					else
					{
						Program.CurrentRoute.SecondsSinceMidnight = Program.CurrentRoute.Stations[PlayerFirstStationIndex].ArrivalTime;
						Game.StartupTime = Program.CurrentRoute.Stations[PlayerFirstStationIndex].ArrivalTime;
					}
				}
			}
			int OtherFirstStationIndex = -1;
			double OtherFirstStationPosition = 0.0;
			double OtherFirstStationTime = 0.0;
			for (int i = 0; i < Program.CurrentRoute.Stations.Length; i++)
			{
				if (Program.CurrentRoute.Stations[i].StopMode == StationStopMode.AllStop | Program.CurrentRoute.Stations[i].StopMode == StationStopMode.PlayerPass & Program.CurrentRoute.Stations[i].Stops.Length != 0)
				{
					OtherFirstStationIndex = i;
					int s = Program.CurrentRoute.Stations[i].GetStopIndex(TrainManager.PlayerTrain);
					OtherFirstStationPosition = s >= 0 ? Program.CurrentRoute.Stations[i].Stops[s].TrackPosition : Program.CurrentRoute.Stations[i].DefaultTrackPosition;
					if (Program.CurrentRoute.Stations[i].ArrivalTime < 0.0)
					{
						if (Program.CurrentRoute.Stations[i].DepartureTime < 0.0)
						{
							OtherFirstStationTime = 0.0;
						}
						else
						{
							OtherFirstStationTime = Program.CurrentRoute.Stations[i].DepartureTime - Program.CurrentRoute.Stations[i].StopTime;
						}
					}
					else
					{
						OtherFirstStationTime = Program.CurrentRoute.Stations[i].ArrivalTime;
					}
					break;
				}
			}
			if (Program.CurrentRoute.PrecedingTrainTimeDeltas.Length != 0)
			{
				OtherFirstStationTime -= Program.CurrentRoute.PrecedingTrainTimeDeltas[Program.CurrentRoute.PrecedingTrainTimeDeltas.Length - 1];
				if (OtherFirstStationTime < Program.CurrentRoute.SecondsSinceMidnight)
				{
					Program.CurrentRoute.SecondsSinceMidnight = OtherFirstStationTime;
				}
			}
			// initialize trains
			for (int i = 0; i <  Program.TrainManager.Trains.Count; i++)
			{
				Program.TrainManager.Trains[i].Initialize();
				int s = Program.TrainManager.Trains[i].IsPlayerTrain ? PlayerFirstStationIndex : OtherFirstStationIndex;
				if (s >= 0)
				{
					if (Program.CurrentRoute.Stations[s].OpenLeftDoors)
					{
						for (int j = 0; j < Program.TrainManager.Trains[i].Cars.Length; j++)
						{
							Program.TrainManager.Trains[i].Cars[j].Doors[0].AnticipatedOpen = true;
						}
					}
					if (Program.CurrentRoute.Stations[s].OpenRightDoors)
					{
						for (int j = 0; j < Program.TrainManager.Trains[i].Cars.Length; j++)
						{
							Program.TrainManager.Trains[i].Cars[j].Doors[1].AnticipatedOpen = true;
						}
					}
				}
				if (Program.CurrentRoute.Sections.Length != 0)
				{
					Program.CurrentRoute.Sections[0].Enter(Program.TrainManager.Trains[i]);
				}
				for (int j = 0; j < Program.TrainManager.Trains[i].Cars.Length; j++)
				{
					double length = Program.TrainManager.Trains[i].Cars[0].Length;
					Program.TrainManager.Trains[i].Cars[j].Move(-length);
					Program.TrainManager.Trains[i].Cars[j].Move(length);
				}
			}

			// ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
			foreach (ScriptedTrain Train in Program.TrainManager.TFOs)  //Must not use var, as otherwise the wrong inferred type
			{
				Train.Initialize();

				foreach (var Car in Train.Cars)
				{
					double length = Train.Cars[0].Length;
					Car.Move(-length);
					Car.Move(length);
				}
			}

			// score
			Game.CurrentScore.ArrivalStation = PlayerFirstStationIndex + 1;
			Game.CurrentScore.DepartureStation = PlayerFirstStationIndex;
			Game.CurrentScore.Maximum = 0;
			for (int i = 0; i < Program.CurrentRoute.Stations.Length; i++)
			{
				if (i != PlayerFirstStationIndex & Program.CurrentRoute.Stations[i].PlayerStops())
				{
					if (i == 0 || Program.CurrentRoute.Stations[i - 1].Type != StationType.ChangeEnds && Program.CurrentRoute.Stations[i - 1].Type != StationType.Jump)
					{
						Game.CurrentScore.Maximum += Game.ScoreValueStationArrival;
					}
				}
			}
			if (Game.CurrentScore.Maximum <= 0)
			{
				Game.CurrentScore.Maximum = Game.ScoreValueStationArrival;
			}
			// signals
			if (Program.CurrentRoute.Sections.Length > 0)
			{
				Program.CurrentRoute.UpdateAllSections();
			}
			// move train in position
			for (int i = 0; i < Program.TrainManager.Trains.Count; i++)
			{
				double p;
				if (Program.TrainManager.Trains[i].IsPlayerTrain)
				{
					p = PlayerFirstStationPosition;
				}
				else if (Program.TrainManager.Trains[i].State == TrainState.Bogus)
				{
					p = Program.CurrentRoute.BogusPreTrainInstructions[0].TrackPosition;
					Program.TrainManager.Trains[i].AI = new Game.BogusPretrainAI(Program.TrainManager.Trains[i]);
				}
				else
				{
					p = OtherFirstStationPosition;
				}
				if (TrainManager.PlayerTrain.CurrentDirection == TrackDirection.Reverse)
				{
					/*
					 * Flip the train if running in reverse direction
					 * We also need to add the length of the train so that the driver car is actually positioned on the platform
					 *
					 * Position on routes not specifically designed for reverse running may well be wrong, but that's life
					 *
					 * We should also suppress any sound events triggered by moving the train into the 'new' position
					 */
					SoundsBase.SuppressSoundEvents = true;
					Program.TrainManager.Trains[i].Reverse(true, true);
					p += Program.TrainManager.Trains[i].Length;
					if (Program.TrainManager.Trains[i].IsPlayerTrain)
					{
						// HACK!
						Program.TrainManager.Trains[i].Station = PlayerFirstStationIndex;
					}
				}

				
				for (int j = 0; j < Program.TrainManager.Trains[i].Cars.Length; j++)
				{
					Program.TrainManager.Trains[i].Cars[j].Move(p);
				}
				SoundsBase.SuppressSoundEvents = false;
			}
			// timetable
			if (Program.CurrentRoute.Information.DefaultTimetableDescription.Length == 0)
			{
				Program.CurrentRoute.Information.DefaultTimetableDescription = Game.LogTrainName;
			}

			// initialize camera
			if (Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable || Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.Restricted3D)
			{
				Program.Renderer.Camera.CurrentMode = CameraViewMode.InteriorLookAhead;
			}
			//Place the initial camera in the driver car
			TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].UpdateCamera();
			Program.Renderer.CameraTrackFollower.UpdateAbsolute(-1.0, true, false);
			Program.Renderer.UpdateVisibility(true);
			if (TrainManager.PlayerTrain.CurrentDirection == TrackDirection.Reverse)
			{
				double reverse = 180 / 57.2957795130824;
				Program.Renderer.Camera.SavedExterior = new CameraAlignment(new OpenBveApi.Math.Vector3(2.5, 1.5, 15), 0.3 + reverse, -0.2, 0.0, PlayerFirstStationPosition, 1.0);
				Program.Renderer.Camera.SavedTrack = new CameraAlignment(new OpenBveApi.Math.Vector3(3.0, 2.5, 0.0), 0.3 + reverse, 0.0, 0.0, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.Cars.Length - 1].TrackPosition, 1.0);
			}
			else
			{
				Program.Renderer.Camera.SavedExterior = new CameraAlignment(new OpenBveApi.Math.Vector3(-2.5, 1.5, -15.0), 0.3, -0.2, 0.0, PlayerFirstStationPosition, 1.0);
				Program.Renderer.Camera.SavedTrack = new CameraAlignment(new OpenBveApi.Math.Vector3(-3.0, 2.5, 0.0), 0.3, 0.0, 0.0, TrainManager.PlayerTrain.Cars[0].TrackPosition - 10.0, 1.0);
			}
			Program.Renderer.Camera.SavedExterior.CameraCar = TrainManager.PlayerTrain.DriverCar; // otherwise non-default driver car jumps to Car0 when first switching to exterior

			// signalling sections
			for (int i = 0; i < Program.TrainManager.Trains.Count; i++)
			{
				int s = Program.TrainManager.Trains[i].CurrentSectionIndex;
				if (Program.CurrentRoute.Sections.Length > Program.TrainManager.Trains[i].CurrentSectionIndex)
				{
					Program.CurrentRoute.Sections[s].Enter(Program.TrainManager.Trains[i]);
				}
			}
			if (Program.CurrentRoute.Sections.Length > 0)
			{
				Program.CurrentRoute.UpdateAllSections();
			}
			// fast-forward until start time
			{
				Game.MinimalisticSimulation = true;
				const double w = 0.25;
				double u = Game.StartupTime - Program.CurrentRoute.SecondsSinceMidnight;
				if (u > 0)
				{
					while (true)
					{
						double v = u < w ? u : w; u -= v;
						Program.CurrentRoute.SecondsSinceMidnight += v;
						Program.TrainManager.UpdateTrains(v);
						if (u <= 0.0) break;
						TotalTimeElapsedForSectionUpdate += v;
						if (TotalTimeElapsedForSectionUpdate >= 1.0)
						{
							if (Program.CurrentRoute.Sections.Length > 0)
							{
								Program.CurrentRoute.UpdateAllSections();
							}
							TotalTimeElapsedForSectionUpdate = 0.0;
						}
					}
				}
				Game.MinimalisticSimulation = false;
			}
			// animated objects
			ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
			Program.TrainManager.UpdateTrainObjects(0.0, true);
			//HACK: This function calls a single update on all objects attached to the player's train
			//      but ignores any specified damping so that all needles etc. are in the correct place
			//      for the first frame, rather than spinning wildly to get to the starting point.
			TrainManager.PlayerTrain.UpdateCabObjects();
			// timetable
			if (TrainManager.PlayerTrain.Station >= 0)
			{
				Timetable.UpdateCustomTimetable(Program.CurrentRoute.Stations[TrainManager.PlayerTrain.Station].TimetableDaytimeTexture, Program.CurrentRoute.Stations[TrainManager.PlayerTrain.Station].TimetableNighttimeTexture);
				if (Timetable.CustomObjectsUsed != 0 & Timetable.CustomTimetableAvailable && Interface.CurrentOptions.TimeTableStyle != TimeTableMode.AutoGenerated && Interface.CurrentOptions.TimeTableStyle != TimeTableMode.None)
				{
					Program.Renderer.CurrentTimetable = DisplayedTimetable.Custom;
				}
			}
			//Create AI driver for the player train if specified via the commmand line
			if (Game.InitialAIDriver)
			{
				TrainManager.PlayerTrain.AI = new Game.SimpleHumanDriverAI(TrainManager.PlayerTrain, double.PositiveInfinity);
				if (TrainManager.PlayerTrain.Plugin != null && TrainManager.PlayerTrain.Plugin.SupportsAI == AISupport.None)
				{
					MessageManager.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","aiunable"}),MessageDependency.None, GameMode.Expert,
						MessageColor.White, 10, null);
				}
			}
			
			// warnings / errors
			if (Interface.LogMessages.Count != 0)
			{
				int filesNotFound = 0;
				int errors = 0;
				int warnings = 0;
				for (int i = 0; i < Interface.LogMessages.Count; i++)
				{
					if (Interface.LogMessages[i].FileNotFound)
					{
						filesNotFound++;
					}
					else if (Interface.LogMessages[i].Type == MessageType.Error)
					{
						errors++;
					}
					else if (Interface.LogMessages[i].Type == MessageType.Warning)
					{
						warnings++;
					}
				}
				string NotFound = null;
				string Messages;
				if (filesNotFound != 0)
				{
					NotFound = filesNotFound + " file(s) not found";
					MessageManager.AddMessage(NotFound, MessageDependency.None, GameMode.Expert, MessageColor.Magenta, 10, null);
					
				}
				if (errors != 0 & warnings != 0)
				{
					Messages = errors + " error(s), " + warnings + " warning(s)";
					MessageManager.AddMessage(Messages, MessageDependency.None, GameMode.Expert, MessageColor.Magenta, 10, null);
				}
				else if (errors != 0)
				{
					Messages = errors + " error(s)";
					MessageManager.AddMessage(Messages, MessageDependency.None, GameMode.Expert, MessageColor.Magenta, 10, null);
				}
				else
				{
					Messages = warnings + " warning(s)";
					MessageManager.AddMessage(Messages, MessageDependency.None, GameMode.Expert, MessageColor.Magenta, 10, null);
				}
				Program.CurrentRoute.Information.FilesNotFound = NotFound;
				Program.CurrentRoute.Information.ErrorsAndWarnings = Messages;
				//Print the plugin error encountered (If any) for 10s
				//This must be done after the simulation has init, as otherwise the timeout doesn't work
				if (TrainManager.PluginError != null)
				{
					MessageManager.AddMessage(TrainManager.PluginError, MessageDependency.None, GameMode.Expert, MessageColor.Red, 5, null);
					MessageManager.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","plugin_failure2"}), MessageDependency.None, GameMode.Expert, MessageColor.Red, 5, null);
				}
			}
			loadComplete = true;
			RenderRealTimeElapsed = 0.0;
			RenderTimeElapsed = 0.0;
			World.InitializeCameraRestriction();
			Loading.SimulationSetup = true;
			Program.Renderer.CurrentInterface = InterfaceType.Normal;
			TrainManager.PlayerTrain.PreloadTextures();
			if (TrainManager.PlayerTrain.CurrentDirection == TrackDirection.Reverse)
			{
				Program.Renderer.Camera.Alignment.Yaw = 180 / 57.2957795130824;
			}
			switch (Interface.CurrentOptions.InitialViewpoint)
			{
				case 0:
					if (Game.InitialReversedConsist)
					{
						/*
						 * HACK: The cab view has been created using the position of the initial driver car.
						 * Trigger a forced change to ensure that it is now correctly positioned in the consist
						 */
						TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].ChangeCarSection(CarSectionType.Interior);
					}
					break;
				case 1:
					//Switch camera to exterior
					MainLoop.SaveCameraSettings();
					Program.Renderer.Camera.CurrentMode = CameraViewMode.Exterior;
					MainLoop.RestoreCameraSettings();
					for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					{
						TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Exterior);
					}
					Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					World.UpdateAbsoluteCamera();
					Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
					break;
				case 2:
					//Switch camera to track
					MainLoop.SaveCameraSettings();
					Program.Renderer.Camera.CurrentMode = CameraViewMode.Track;
					MainLoop.RestoreCameraSettings();
					for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					{
						TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Exterior);
					}

					Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					World.UpdateAbsoluteCamera();
					Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
					break;
				case 3:
					//Switch camera to flyby
					MainLoop.SaveCameraSettings();
					Program.Renderer.Camera.CurrentMode = CameraViewMode.FlyBy;
					MainLoop.RestoreCameraSettings();
					for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					{
						TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Exterior);
					}

					Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					World.UpdateAbsoluteCamera();
					Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
					break;
				case 4:
					//Switch camera to flyby
					MainLoop.SaveCameraSettings();
					Program.Renderer.Camera.CurrentMode = CameraViewMode.FlyByZooming;
					MainLoop.RestoreCameraSettings();
					for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					{
						TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Exterior);
					}

					Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					World.UpdateAbsoluteCamera();
					Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
					break;
			}
			
			if (IntPtr.Size == 4)
			{
				using (Process proc = Process.GetCurrentProcess())
				{
					long memoryUsed = proc.PrivateMemorySize64;
					if ((memoryUsed > 900000000 && !Interface.CurrentOptions.LoadInAdvance) || memoryUsed > 1600000000)
					{
						// Either using ~900mb at the first station or 1.5gb + with all textures loaded is likely to cause critical OOM errors with the 32-bit process memory limit
						// Turn on UnloadUnusedTextures to try and mitigate
						Program.FileSystem.AppendToLogFile("Automatically enabling UnloadUnusedTextures due to memory pressure.");
						Interface.CurrentOptions.UnloadUnusedTextures = true;
					}
				}
			}

			simulationSetup = true;
			Program.FileSystem.AppendToLogFile(@"--------------------", false);
			Program.FileSystem.AppendToLogFile(@"Loading complete, starting simulation.");
			Program.FileSystem.AppendToLogFile(@"--------------------", false);
		}

		private bool simulationSetup = false;

		public void LoadingScreenLoop()
		{
			Program.Renderer.PushMatrix(MatrixMode.Projection);
			Matrix4D.CreateOrthographicOffCenter(0.0f, Program.Renderer.Screen.Width, Program.Renderer.Screen.Height, 0.0f, -1.0f, 1.0f, out Program.Renderer.CurrentProjectionMatrix);
			Program.Renderer.PushMatrix(MatrixMode.Modelview);
			Program.Renderer.CurrentViewMatrix = Matrix4D.Identity;

			while (!Loading.Complete && !Loading.Cancel && !simulationSetup)
			{
				CPreciseTimer.GetElapsedTime();
				ProcessEvents();
				if (IsExiting)
				{
					Loading.Cancel = true;
				}
				double routeProgress = 1.0, trainProgress = 0.0;
				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
				{
					if (Program.CurrentHost.Plugins[i].Route != null && Program.CurrentHost.Plugins[i].Route.IsLoading)
					{
						routeProgress = Program.CurrentHost.Plugins[i].Route.CurrentProgress;
						trainProgress = 0.0;
						break;
					}

					if (Program.CurrentHost.Plugins[i].Train != null && Program.CurrentHost.Plugins[i].Train.IsLoading)
					{
						trainProgress = Program.CurrentHost.Plugins[i].Train.CurrentProgress;
					}
				}
				double trainProgressWeight = 1.0 / Program.TrainManager.Trains.Count;
				double finalTrainProgress;
				if (Program.TrainManager.Trains.Count != 0)
				{
					//Trains are not loaded until after the route
					finalTrainProgress = (Loading.LoadedTrain * trainProgressWeight) + trainProgressWeight * trainProgress;
				}
				else
				{
					finalTrainProgress = 0.0;
				}

				Program.Renderer.Loading.SetLoadingBkg(Program.CurrentRoute.Information.LoadingScreenBackground);
				Program.Renderer.Loading.DrawLoadingScreen(Program.Renderer.Fonts.SmallFont, routeProgress, finalTrainProgress);
				SwapBuffers();
				
				if (Program.Renderer.RenderThreadJobWaiting)
				{
					while (!Program.Renderer.RenderThreadJobs.IsEmpty)
					{
						Program.Renderer.RenderThreadJobs.TryDequeue(out ThreadStart currentJob);
						currentJob();
						lock (currentJob)
						{
							Monitor.Pulse(currentJob);
						}
						
					}
					Program.Renderer.RenderThreadJobWaiting = false;
				}
				double time = CPreciseTimer.GetElapsedTime();
				double wait = 1000.0 / 60.0 - time * 1000 - 50;
				if (wait > 0)
					Thread.Sleep((int)(wait));
			}
			if(Loading.Cancel)
			{
				Exit();
				return;
			}
			Program.Renderer.PopMatrix(MatrixMode.Modelview);
			Program.Renderer.PopMatrix(MatrixMode.Projection);
			SetupSimulation();
		}
	}
}
