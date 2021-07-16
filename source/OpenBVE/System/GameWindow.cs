using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using LibRender2;
using LibRender2.Cameras;
using LibRender2.Overlays;
using LibRender2.Screens;
using LibRender2.Trains;
using LibRender2.Viewports;
using OpenBve;
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
using TrainManager.Trains;
using Path = System.IO.Path;
using Vector2 = OpenTK.Vector2;
using Control = OpenBveApi.Interface.Control;

namespace OpenBve
{
	class OpenBVEGame: GameWindow
	{
		/// <summary>The current time acceleration factor</summary>
		int TimeFactor = 1;
		double TotalTimeElapsedForInfo;
		double TotalTimeElapsedForSectionUpdate;
		private bool loadComplete;
		private bool firstFrame;
		private double RenderTimeElapsed;
		private double RenderRealTimeElapsed;
		//We need to explicitly specify the default constructor
		public OpenBVEGame(int width, int height, GraphicsMode currentGraphicsMode, GameWindowFlags @default): base(width, height, currentGraphicsMode, Translations.GetInterfaceString("program_title"), @default)
		{
			try
			{
				var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				Icon ico = new Icon(OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(assemblyFolder, "Data"), "icon.ico"));
				this.Icon = ico;
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
				//Renderer.UpdateLighting();
				Program.Renderer.RenderScene(TimeElapsed, RealTimeElapsed);
				Program.currentGameWindow.SwapBuffers();
				if (MainLoop.Quit != MainLoop.QuitMode.ContinueGame)
				{
					Close();
					if (Program.CurrentHost.MonoRuntime && MainLoop.Quit == MainLoop.QuitMode.QuitProgram)
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
				Program.Renderer.UpdateVisibility(Program.Renderer.CameraTrackFollower.TrackPosition + Program.Renderer.Camera.Alignment.Position.Z);
				int d = TrainManager.PlayerTrain.DriverCar;
				Program.Renderer.Camera.CurrentSpeed = TrainManager.PlayerTrain.Cars[d].CurrentSpeed;
			}
			else
			{
				Program.Renderer.Camera.CurrentSpeed = 0.0;
			}

			Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
			if (MainLoop.Quit != MainLoop.QuitMode.ContinueGame)
			{
				Program.currentGameWindow.Exit();
				if (Program.CurrentHost.MonoRuntime && MainLoop.Quit == MainLoop.QuitMode.QuitProgram)
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
			Program.currentGameWindow.SwapBuffers();
			Game.UpdateBlackBox();
			// pause/menu
			
			// limit framerate
			if (MainLoop.LimitFramerate)
			{
				Thread.Sleep(10);
			}
			MainLoop.UpdateControlRepeats(RealTimeElapsed);
			MainLoop.ProcessKeyboard();
			MainLoop.UpdateMouse(RealTimeElapsed);
			MainLoop.ProcessControls(TimeElapsed);
			if (Program.Joysticks.AttachedJoysticks.ContainsKey(AbstractRailDriver.Guid))
			{
				var railDriver = Program.Joysticks.AttachedJoysticks[AbstractRailDriver.Guid] as AbstractRailDriver;
				if (railDriver != null)
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
			}
			
			RenderRealTimeElapsed = 0.0;
			RenderTimeElapsed = 0.0;
				
				

#if DEBUG
			MainLoop.CheckForOpenGlError("MainLoop");
			 
#endif
			if (Interface.CurrentOptions.UnloadUnusedTextures)
			{
				Textures.UnloadUnusedTextures(TimeElapsed);
			}
			// finish
			try
			{
				Interface.SaveLogs();
			}
			catch { }
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
				TimeElapsed = RealTimeElapsed * (double)TimeFactor;
				if (loadComplete && !firstFrame)
				{
					//Our current in-game time is equal to or greater than the startup time, but the first frame has not yet been processed
					//Therefore, reset the timer to zero as time consuming texture loads may cause us to be late at the first station
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
						double time = TimeElapsed/(double) chunks;
						for (int i = 0; i < chunks; i++)
						{
							Program.CurrentRoute.SecondsSinceMidnight += time;
							Program.TrainManager.UpdateTrains(time);
						}
					}
				}
				Game.CurrentScore.Update(TimeElapsed);
				MessageManager.UpdateMessages();
				Game.UpdateScoreMessages(TimeElapsed);

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
			jobs = new Queue<ThreadStart>(10);
			locks = new Queue<object>(10);
			Program.Renderer.Initialize(Program.CurrentHost, Interface.CurrentOptions);
			Program.Renderer.DetermineMaxAFLevel();
			HUD.LoadHUD();
			Program.Renderer.Loading.InitLoading(Program.FileSystem.GetDataFolder("In-game"), typeof(NewRenderer).Assembly.GetName().Version.ToString());
			Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
			Program.Renderer.MotionBlur.Initialize(Interface.CurrentOptions.MotionBlur);
			Loading.LoadAsynchronously(MainLoop.currentResult.RouteFile, MainLoop.currentResult.RouteEncoding, MainLoop.currentResult.TrainFolder, MainLoop.currentResult.TrainEncoding);
			LoadingScreenLoop();
			//Add event handler hooks for keyboard and mouse buttons
			//Do this after the renderer has init and the loop has started to prevent timing issues
			KeyDown	+= MainLoop.keyDownEvent;
			KeyUp	+= MainLoop.keyUpEvent;
			MouseDown	+= MainLoop.mouseDownEvent;
			MouseUp += MainLoop.mouseUpEvent;
			MouseMove	+= MainLoop.mouseMoveEvent;
			MouseWheel  += MainLoop.mouseWheelEvent;

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
			for (int i = 0; i < Program.TrainManager.Trains.Length; i++)
			{
				if (Program.TrainManager.Trains[i].State != TrainState.Bogus)
				{
					Program.TrainManager.Trains[i].UnloadPlugin();
				}
			}
			Program.Renderer.TextureManager.UnloadAllTextures();
			for (int i = 0; i < InputDevicePlugin.AvailablePluginInfos.Count; i++)
			{
				InputDevicePlugin.CallPluginUnload(i);
			}
			if (MainLoop.Quit == MainLoop.QuitMode.ContinueGame && Program.CurrentHost.MonoRuntime)
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
				Cursor.Status Status;
				if (Program.Renderer.Touch.MoveCheck(new Vector2(e.X, e.Y), out Status))
				{
					if (Cursors.CurrentCursor != null)
					{
						switch (Status)
						{
							case OpenBve.Cursor.Status.Default:
								Cursor = Cursors.CurrentCursor;
								break;
							case OpenBve.Cursor.Status.Plus:
								Cursor = Cursors.CurrentCursorPlus;
								break;
							case OpenBve.Cursor.Status.Minus:
								Cursor = Cursors.CurrentCursorMinus;
								break;
						}
					}
				}
				else
				{
					Cursor = MouseCursor.Default;
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
					MessageBox.Show("A critical error has occured:\n\n" + Interface.LogMessages[i].Text + "\n\nPlease inspect the error log file for further information.", "Load", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					Close();
				}
			}
			Program.Renderer.Lighting.Initialize();
			Game.LogRouteName = Path.GetFileName(MainLoop.currentResult.RouteFile);
			Game.LogTrainName = Path.GetFileName(MainLoop.currentResult.TrainFolder);
			Game.LogDateTime = DateTime.Now;

			if (Interface.CurrentOptions.LoadInAdvance)
			{
				Textures.LoadAllTextures();
			}
			else
			{
				Program.Renderer.TextureManager.UnloadAllTextures();
			}
			// camera
			Program.Renderer.InitializeVisibility();
			TrainManager.PlayerTrain.DriverBody = new DriverBody(TrainManager.PlayerTrain);
			Program.Renderer.CameraTrackFollower.UpdateAbsolute(0.0, true, false);
			Program.Renderer.CameraTrackFollower.UpdateAbsolute(-0.1, true, false);
			Program.Renderer.CameraTrackFollower.UpdateAbsolute(0.1, true, false);
			Program.Renderer.CameraTrackFollower.TriggerType = EventTriggerType.Camera;
			// starting time and track position
			Program.CurrentRoute.SecondsSinceMidnight = 0.0;
			Game.StartupTime = 0.0;
			int PlayerFirstStationIndex = -1;
			double PlayerFirstStationPosition;
			int os = -1;
			bool f = false;
			for (int i = 0; i < Program.CurrentRoute.Stations.Length; i++)
			{
				if (!String.IsNullOrEmpty(Program.CurrentRoute.InitialStationName))
				{
					if (Program.CurrentRoute.InitialStationName.ToLowerInvariant() == Program.CurrentRoute.Stations[i].Name.ToLowerInvariant())
					{
						PlayerFirstStationIndex = i;
					}
				}
				if (Program.CurrentRoute.Stations[i].StopMode == StationStopMode.AllStop | Program.CurrentRoute.Stations[i].StopMode == StationStopMode.PlayerStop & Program.CurrentRoute.Stations[i].Stops.Length != 0)
				{
					if (f == false)
					{
						os = i;
						f = true;
					}
				}
			}
			if (PlayerFirstStationIndex == -1)
			{
				if (os == -1)
				{
					PlayerFirstStationIndex = 0;
				}
				else
				{
					PlayerFirstStationIndex = os;	
				}
			}
			{
				int s = Program.CurrentRoute.Stations[PlayerFirstStationIndex].GetStopIndex(TrainManager.PlayerTrain.NumberOfCars);
				if (s >= 0)
				{
					PlayerFirstStationPosition = Program.CurrentRoute.Stations[PlayerFirstStationIndex].Stops[s].TrackPosition;

					double TrainLength = 0.0;
					for (int c = 0; c < TrainManager.PlayerTrain.Cars.Length; c++)
					{
						TrainLength += TrainManager.PlayerTrain.Cars[c].Length;
					}

					for (int j = 0; j < Program.CurrentRoute.BufferTrackPositions.Length; j++)
					{
						if (PlayerFirstStationPosition > Program.CurrentRoute.BufferTrackPositions[j] && PlayerFirstStationPosition - TrainLength < Program.CurrentRoute.BufferTrackPositions[j])
						{
							/*
							 * HACK: The initial start position for the player train is stuck on a set of buffers
							 * This means we have to make some one the fly adjustments to the first station stop position
							 */

							//Set the start position to be the buffer position plus the train length plus 1m
							PlayerFirstStationPosition = Program.CurrentRoute.BufferTrackPositions[j] + TrainLength + 1;
							//Update the station stop location
							if (s >= 0)
							{
								Program.CurrentRoute.Stations[PlayerFirstStationIndex].Stops[s].TrackPosition = PlayerFirstStationPosition;
							}
							else
							{
								Program.CurrentRoute.Stations[PlayerFirstStationIndex].DefaultTrackPosition = PlayerFirstStationPosition;
							}
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
					int s = Program.CurrentRoute.Stations[i].GetStopIndex(TrainManager.PlayerTrain.Cars.Length);
					if (s >= 0)
					{
						OtherFirstStationPosition = Program.CurrentRoute.Stations[i].Stops[s].TrackPosition;
					}
					else
					{
						OtherFirstStationPosition = Program.CurrentRoute.Stations[i].DefaultTrackPosition;
					}
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
			for (int i = 0; i <  Program.TrainManager.Trains.Length; i++)
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
			foreach (TrackFollowingObject Train in Program.TrainManager.TFOs)  //Must not use var, as otherwise the wrong inferred type
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
			for (int i = 0; i < Program.TrainManager.Trains.Length; i++)
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
				for (int j = 0; j < Program.TrainManager.Trains[i].Cars.Length; j++)
				{
					Program.TrainManager.Trains[i].Cars[j].Move(p);
				}
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
			Program.Renderer.UpdateVisibility(Program.Renderer.CameraTrackFollower.TrackPosition + Program.Renderer.Camera.Alignment.Position.Z);
			Program.Renderer.Camera.SavedExterior = new CameraAlignment(new OpenBveApi.Math.Vector3(-2.5, 1.5, -15.0), 0.3, -0.2, 0.0, PlayerFirstStationPosition, 1.0);
			Program.Renderer.Camera.SavedTrack = new CameraAlignment(new OpenBveApi.Math.Vector3(-3.0, 2.5, 0.0), 0.3, 0.0, 0.0, TrainManager.PlayerTrain.Cars[0].TrackPosition - 10.0, 1.0);
			// signalling sections
			for (int i = 0; i < Program.TrainManager.Trains.Length; i++)
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
				TrainManager.PlayerTrain.AI = new Game.SimpleHumanDriverAI(TrainManager.PlayerTrain, Double.PositiveInfinity);
				if (TrainManager.PlayerTrain.Plugin != null && TrainManager.PlayerTrain.Plugin.SupportsAI == AISupport.None)
				{
					MessageManager.AddMessage(Translations.GetInterfaceString("notification_aiunable"),MessageDependency.None, GameMode.Expert,
						MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
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
					NotFound = filesNotFound.ToString() + " file(s) not found";
					MessageManager.AddMessage(NotFound, MessageDependency.None, GameMode.Expert, MessageColor.Magenta, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
					
				}
				if (errors != 0 & warnings != 0)
				{
					Messages = errors.ToString() + " error(s), " + warnings.ToString() + " warning(s)";
					MessageManager.AddMessage(Messages, MessageDependency.None, GameMode.Expert, MessageColor.Magenta, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
				}
				else if (errors != 0)
				{
					Messages = errors.ToString() + " error(s)";
					MessageManager.AddMessage(Messages, MessageDependency.None, GameMode.Expert, MessageColor.Magenta, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
				}
				else
				{
					Messages = warnings.ToString() + " warning(s)";
					MessageManager.AddMessage(Messages, MessageDependency.None, GameMode.Expert, MessageColor.Magenta, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
				}
				Program.CurrentRoute.Information.FilesNotFound = NotFound;
				Program.CurrentRoute.Information.ErrorsAndWarnings = Messages;
				//Print the plugin error encountered (If any) for 10s
				//This must be done after the simulation has init, as otherwise the timeout doesn't work
				if (TrainManager.PluginError != null)
				{
					MessageManager.AddMessage(TrainManager.PluginError, MessageDependency.None, GameMode.Expert, MessageColor.Red, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
					MessageManager.AddMessage(Translations.GetInterfaceString("errors_plugin_failure2"), MessageDependency.None, GameMode.Expert, MessageColor.Red, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
				}
			}
			loadComplete = true;
			RenderRealTimeElapsed = 0.0;
			RenderTimeElapsed = 0.0;
			World.InitializeCameraRestriction();
			Loading.SimulationSetup = true;
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
						//Make bogies visible
						TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
					}
					Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					World.UpdateAbsoluteCamera(0.0);
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
						TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
					}

					Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					World.UpdateAbsoluteCamera(0.0);
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
						TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
					}

					Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					World.UpdateAbsoluteCamera(0.0);
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
						TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
					}

					Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					World.UpdateAbsoluteCamera(0.0);
					Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
					break;
			}
		}

		private void LoadingScreenLoop()
		{
			Program.Renderer.PushMatrix(MatrixMode.Projection);
			Matrix4D.CreateOrthographicOffCenter(0.0f, Program.Renderer.Screen.Width, Program.Renderer.Screen.Height, 0.0f, -1.0f, 1.0f, out Program.Renderer.CurrentProjectionMatrix);
			Program.Renderer.PushMatrix(MatrixMode.Modelview);
			Program.Renderer.CurrentViewMatrix = Matrix4D.Identity;

			while (!Loading.Complete && !Loading.Cancel)
			{
				CPreciseTimer.GetElapsedTime();
				this.ProcessEvents();
				if (this.IsExiting)
					Loading.Cancel = true;
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
				double trainProgressWeight = 1.0 / Program.TrainManager.Trains.Length;
				double finalTrainProgress;
				if (Program.TrainManager.Trains.Length != 0)
				{
					//Trains are not loaded until after the route
					finalTrainProgress = (Loading.CurrentTrain * trainProgressWeight) + trainProgressWeight * trainProgress;
				}
				else
				{
					finalTrainProgress = 0.0;
				}

				Program.Renderer.Loading.SetLoadingBkg(Program.CurrentRoute.Information.LoadingScreenBackground);
				Program.Renderer.Loading.DrawLoadingScreen(Program.Renderer.Fonts.SmallFont, routeProgress, finalTrainProgress);
				Program.currentGameWindow.SwapBuffers();
				
				if (Loading.JobAvailable)
				{
					while (jobs.Count > 0)
					{
						lock (jobLock)
						{
							var currentJob = jobs.Dequeue();
							var locker = locks.Dequeue();
							currentJob();
							lock (locker)
							{
								Monitor.Pulse(locker);
							}
						}
					}
					Loading.JobAvailable = false;
				}
				double time = CPreciseTimer.GetElapsedTime();
				double wait = 1000.0 / 60.0 - time * 1000 - 50;
				if (wait > 0)
					Thread.Sleep((int)(wait));
			}
			if(!Loading.Cancel)
			{
				Program.Renderer.PopMatrix(MatrixMode.Modelview);
				Program.Renderer.PopMatrix(MatrixMode.Projection);
				SetupSimulation();
			} else {
				this.Exit();
			}
		}

		private static readonly object jobLock = new object();
		private static Queue<ThreadStart> jobs;
		private static Queue<object> locks;
		
		/// <summary>This method is used during loading to run commands requiring an OpenGL context in the main render loop</summary>
		/// <param name="job">The OpenGL command</param>
		internal static void RunInRenderThread(ThreadStart job)
		{
			object locker = new object();
			lock (jobLock)
			{
				jobs.Enqueue(job);
				locks.Enqueue(locker);
				//Don't set the job to available until after it's been loaded into the queue
				Loading.JobAvailable = true;
			}
			lock (locker)
			{
				//Failsafe: If our job has taken more than a second, terminate it
				//A missing texture is probably better than an infinite loadscreen
				Monitor.Wait(locker, 1000);
			}
		}
		
		public override void Dispose()
		{
			base.Dispose();
		}
	}
}
