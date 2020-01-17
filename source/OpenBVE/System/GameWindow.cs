using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using LibRender2.Cameras;
using LibRender2.Viewports;
using OpenBve.Graphics;
using OpenBveApi.Colors;
using OpenBveApi.Runtime;
using OpenBveApi.Interface;
using OpenBveApi.Trains;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenTK.Graphics.OpenGL;
using RouteManager2;
using RouteManager2.MessageManager;
using Path = System.IO.Path;
using Vector2 = OpenTK.Vector2;

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
				System.Drawing.Icon ico = new System.Drawing.Icon(OpenBveApi.Path.CombineFile(OpenBveApi.Path.CombineDirectory(assemblyFolder, "Data"), "icon.ico"));
				this.Icon = ico;
			}
			catch
			{
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
			if(Game.CurrentInterface != Game.InterfaceType.Normal)
			{
				MainLoop.UpdateControlRepeats(0.0);
				MainLoop.ProcessKeyboard();
				MainLoop.ProcessControls(0.0);
				if (Game.CurrentInterface == Game.InterfaceType.Pause)
				{
					System.Threading.Thread.Sleep(10);
				}
				//Renderer.UpdateLighting();
				Program.Renderer.RenderScene(TimeElapsed);
				Program.currentGameWindow.SwapBuffers();
				if (MainLoop.Quit != MainLoop.QuitMode.ContinueGame)
				{
					Close();
					if (Program.CurrentlyRunningOnMono && MainLoop.Quit == MainLoop.QuitMode.QuitProgram)
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
			
			
			if (Game.PreviousInterface != Game.InterfaceType.Normal)
			{
				ObjectManager.UpdateAnimatedWorldObjects(0.0, false);
				Game.PreviousInterface = Game.InterfaceType.Normal;
			}
			else
			{
				ObjectManager.UpdateAnimatedWorldObjects(TimeElapsed, false);
			}

			//We need to update the camera position in the render sequence
			//Not doing this means that the camera doesn't move
			// update in one piece
			if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead)
			{
				//Update the in-car camera based upon the current driver car (Cabview or passenger view)
				TrainManager.PlayerTrain.Cars[World.CameraCar].UpdateCamera();
			}
			else if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Exterior)
			{
				//Update the camera position based upon the relative car position
				TrainManager.PlayerTrain.Cars[World.CameraCar].UpdateCamera();
			}
			if (Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable)
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
			TrainManager.UpdateTrainObjects(TimeElapsed, false);
			if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead | Program.Renderer.Camera.CurrentMode == CameraViewMode.Exterior)
			{
				Program.Renderer.UpdateVisibility(World.CameraTrackFollower.TrackPosition + Program.Renderer.Camera.Alignment.Position.Z);
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
				if (Program.CurrentlyRunningOnMono && MainLoop.Quit == MainLoop.QuitMode.QuitProgram)
				{
					Environment.Exit(0);
				}				
			}
			Program.Renderer.Lighting.UpdateLighting(Program.CurrentRoute.SecondsSinceMidnight);
			Program.Renderer.RenderScene(TimeElapsed);
			Program.Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
			Program.currentGameWindow.SwapBuffers();
			Game.UpdateBlackBox();
			// pause/menu
			
			// limit framerate
			if (MainLoop.LimitFramerate)
			{
				System.Threading.Thread.Sleep(10);
			}
			MainLoop.UpdateControlRepeats(RealTimeElapsed);
			MainLoop.ProcessKeyboard();
			MainLoop.UpdateMouse(RealTimeElapsed);
			MainLoop.ProcessControls(TimeElapsed);
			for (int i = 0; i < JoystickManager.AttachedJoysticks.Length; i++)
			{
				var railDriver = JoystickManager.AttachedJoysticks[i] as JoystickManager.Raildriver;
				if (railDriver != null)
				{
					if (Interface.CurrentOptions.RailDriverMPH)
					{
						railDriver.SetDisplay((int)(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Specs
							                             .CurrentPerceivedSpeed * 2.23694));
					}
					else
					{
						railDriver.SetDisplay((int)(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Specs
							                             .CurrentPerceivedSpeed * 3.6));
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
			if (Game.CurrentInterface == Game.InterfaceType.Normal)
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
						TrainManager.UpdateTrains(TimeElapsed);
					}
					else
					{
						const int maxChunks = 2;
						int chunks = Math.Min((int) Math.Round(TimeElapsed/chunkTime), maxChunks);
						double time = TimeElapsed/(double) chunks;
						for (int i = 0; i < chunks; i++)
						{
							Program.CurrentRoute.SecondsSinceMidnight += time;
							TrainManager.UpdateTrains(time);
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

		protected override void OnLoad(EventArgs e)
		{
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
					Interface.Control[] AddControls = new Interface.Control[AddControlsLength];
					for (int j = 0; j < AddControlsLength; j++)
					{
						AddControls[j].Command = InputDevicePlugin.AvailablePlugins[i].Controls[j].Command;
						AddControls[j].Method = Interface.ControlMethod.InputDevicePlugin;
						AddControls[j].Option = InputDevicePlugin.AvailablePlugins[i].Controls[j].Option;
					}
					Interface.CurrentControls = Interface.CurrentControls.Concat(AddControls).ToArray();
					foreach (var Train in TrainManager.Trains)
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
			if (Loading.Cancel == true)
			{
				return;
			}
			if (!Loading.Complete)
			{
				e.Cancel = true;
				Loading.Cancel = true;
			}
			for (int i = 0; i < TrainManager.Trains.Length; i++)
			{
				if (TrainManager.Trains[i].State != TrainState.Bogus)
				{
					TrainManager.Trains[i].UnloadPlugin();
				}
			}
			Program.Renderer.TextureManager.UnloadAllTextures();
			for (int i = 0; i < InputDevicePlugin.AvailablePluginInfos.Count; i++)
			{
				InputDevicePlugin.CallPluginUnload(i);
			}
			if (MainLoop.Quit == MainLoop.QuitMode.ContinueGame && Program.CurrentlyRunningOnMono)
			{
				//More forcefully close under Mono, stuff *still* hanging around....
				Environment.Exit(0);
			}
			base.OnClosing(e);
		}

		protected override void OnMouseMove(MouseMoveEventArgs e)
		{
			base.OnMouseMove(e);

			if (Game.CurrentInterface == Game.InterfaceType.Normal)
			{
				OpenBve.Cursor.Status Status;
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

			lock (Illustrations.Locker)
			{
				Timetable.CreateTimetable();
			}
			//Check if any critical errors have occured during the route or train loading
			for (int i = 0; i < Interface.MessageCount; i++)
			{
				if (Interface.LogMessages[i].Type == MessageType.Critical)
				{
					MessageBox.Show("A critical error has occured:\n\n" + Interface.LogMessages[i].Text + "\n\nPlease inspect the error log file for further information.", "Load", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					Close();
				}
			}
			Program.Renderer.Lighting.Initialize();
			Game.LogRouteName = System.IO.Path.GetFileName(MainLoop.currentResult.RouteFile);
			Game.LogTrainName = System.IO.Path.GetFileName(MainLoop.currentResult.TrainFolder);
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
			World.CameraTrackFollower.UpdateAbsolute(0.0, true, false);
			World.CameraTrackFollower.UpdateAbsolute(-0.1, true, false);
			World.CameraTrackFollower.UpdateAbsolute(0.1, true, false);
			World.CameraTrackFollower.TriggerType = EventTriggerType.Camera;
			// starting time and track position
			Program.CurrentRoute.SecondsSinceMidnight = 0.0;
			Game.StartupTime = 0.0;
			int PlayerFirstStationIndex = -1;
			double PlayerFirstStationPosition;
			int os = -1;
			bool f = false;
			for (int i = 0; i < Program.CurrentRoute.Stations.Length; i++)
			{
				if (!String.IsNullOrEmpty(Game.InitialStationName))
				{
					if (Game.InitialStationName.ToLowerInvariant() == Program.CurrentRoute.Stations[i].Name.ToLowerInvariant())
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
				if (Game.InitialStationTime != -1)
				{
					Program.CurrentRoute.SecondsSinceMidnight = Game.InitialStationTime;
					Game.StartupTime = Game.InitialStationTime;
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
			if (Game.PrecedingTrainTimeDeltas.Length != 0)
			{
				OtherFirstStationTime -= Game.PrecedingTrainTimeDeltas[Game.PrecedingTrainTimeDeltas.Length - 1];
				if (OtherFirstStationTime < Program.CurrentRoute.SecondsSinceMidnight)
				{
					Program.CurrentRoute.SecondsSinceMidnight = OtherFirstStationTime;
				}
			}
			// initialize trains
			for (int i = 0; i < TrainManager.Trains.Length; i++)
			{
				TrainManager.Trains[i].Initialize();
				int s = TrainManager.Trains[i].IsPlayerTrain ? PlayerFirstStationIndex : OtherFirstStationIndex;
				if (s >= 0)
				{
					if (Program.CurrentRoute.Stations[s].OpenLeftDoors)
					{
						for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++)
						{
							TrainManager.Trains[i].Cars[j].Doors[0].AnticipatedOpen = true;
						}
					}
					if (Program.CurrentRoute.Stations[s].OpenRightDoors)
					{
						for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++)
						{
							TrainManager.Trains[i].Cars[j].Doors[1].AnticipatedOpen = true;
						}
					}
				}
				if (Program.CurrentRoute.Sections.Length != 0)
				{
					Program.CurrentRoute.Sections[0].Enter(TrainManager.Trains[i]);
				}
				for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++)
				{
					double length = TrainManager.Trains[i].Cars[0].Length;
					TrainManager.Trains[i].Cars[j].Move(-length);
					TrainManager.Trains[i].Cars[j].Move(length);
				}
			}

			foreach (var Train in TrainManager.TFOs)
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
			for (int i = 0; i < TrainManager.Trains.Length; i++)
			{
				double p;
				if (TrainManager.Trains[i].IsPlayerTrain)
				{
					p = PlayerFirstStationPosition;
				}
				else if (TrainManager.Trains[i].State == TrainState.Bogus)
				{
					p = Program.CurrentRoute.BogusPreTrainInstructions[0].TrackPosition;
					TrainManager.Trains[i].AI = new Game.BogusPretrainAI(TrainManager.Trains[i]);
				}
				else
				{
					p = OtherFirstStationPosition;
				}
				for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++)
				{
					TrainManager.Trains[i].Cars[j].Move(p);
				}
			}
			// timetable
			if (Timetable.DefaultTimetableDescription.Length == 0)
			{
				Timetable.DefaultTimetableDescription = Game.LogTrainName;
			}

			// initialize camera
			if (Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable)
			{
				Program.Renderer.Camera.CurrentMode = CameraViewMode.InteriorLookAhead;
			}
			//Place the initial camera in the driver car
			TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].UpdateCamera();
			World.CameraTrackFollower.UpdateAbsolute(-1.0, true, false);
			Program.Renderer.UpdateVisibility(World.CameraTrackFollower.TrackPosition + Program.Renderer.Camera.Alignment.Position.Z);
			World.CameraSavedExterior = new CameraAlignment(new OpenBveApi.Math.Vector3(-2.5, 1.5, -15.0), 0.3, -0.2, 0.0, PlayerFirstStationPosition, 1.0);
			World.CameraSavedTrack = new CameraAlignment(new OpenBveApi.Math.Vector3(-3.0, 2.5, 0.0), 0.3, 0.0, 0.0, TrainManager.PlayerTrain.Cars[0].TrackPosition - 10.0, 1.0);
			// signalling sections
			for (int i = 0; i < TrainManager.Trains.Length; i++)
			{
				int s = TrainManager.Trains[i].CurrentSectionIndex;
				Program.CurrentRoute.Sections[s].Enter(TrainManager.Trains[i]);
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
						TrainManager.UpdateTrains(v);
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
			TrainManager.UpdateTrainObjects(0.0, true);
			//HACK: This function calls a single update on all objects attached to the player's train
			//      but ignores any specified damping so that all needles etc. are in the correct place
			//      for the first frame, rather than spinning wildly to get to the starting point.
			TrainManager.PlayerTrain.UpdateCabObjects();
			// timetable
			if (TrainManager.PlayerTrain.Station >= 0)
			{
				Timetable.UpdateCustomTimetable(Program.CurrentRoute.Stations[TrainManager.PlayerTrain.Station].TimetableDaytimeTexture, Program.CurrentRoute.Stations[TrainManager.PlayerTrain.Station].TimetableNighttimeTexture);
				if (Timetable.CustomObjectsUsed != 0 & Timetable.CustomTimetableAvailable && Interface.CurrentOptions.TimeTableStyle != Interface.TimeTableMode.AutoGenerated && Interface.CurrentOptions.TimeTableStyle != Interface.TimeTableMode.None)
				{
					Timetable.CurrentTimetable = Timetable.TimetableState.Custom;
				}
			}
			//Create AI driver for the player train if specified via the commmand line
			if (Game.InitialAIDriver == true)
			{
				TrainManager.PlayerTrain.AI = new Game.SimpleHumanDriverAI(TrainManager.PlayerTrain);
				if (TrainManager.PlayerTrain.Plugin != null && !TrainManager.PlayerTrain.Plugin.SupportsAI)
				{
					Game.AddMessage(Translations.GetInterfaceString("notification_aiunable"),MessageDependency.None, GameMode.Expert,
						OpenBveApi.Colors.MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
				}
			}
			
			// warnings / errors
			if (Interface.MessageCount != 0)
			{
				int filesNotFound = 0;
				int errors = 0;
				int warnings = 0;
				for (int i = 0; i < Interface.MessageCount; i++)
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
					Game.AddMessage(NotFound, MessageDependency.None, GameMode.Expert, MessageColor.Magenta, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
					
				}
				if (errors != 0 & warnings != 0)
				{
					Messages = errors.ToString() + " error(s), " + warnings.ToString() + " warning(s)";
					Game.AddMessage(Messages, MessageDependency.None, GameMode.Expert, MessageColor.Magenta, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
				}
				else if (errors != 0)
				{
					Messages = errors.ToString() + " error(s)";
					Game.AddMessage(Messages, MessageDependency.None, GameMode.Expert, MessageColor.Magenta, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
				}
				else
				{
					Messages = warnings.ToString() + " warning(s)";
					Game.AddMessage(Messages, MessageDependency.None, GameMode.Expert, MessageColor.Magenta, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
				}
				Game.RouteInformation.FilesNotFound = NotFound;
				Game.RouteInformation.ErrorsAndWarnings = Messages;
				//Print the plugin error encountered (If any) for 10s
				//This must be done after the simulation has init, as otherwise the timeout doesn't work
				if (Loading.PluginError != null)
				{
					Game.AddMessage(Loading.PluginError, MessageDependency.None, GameMode.Expert, OpenBveApi.Colors.MessageColor.Red, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
					Game.AddMessage(Translations.GetInterfaceString("errors_plugin_failure2"), MessageDependency.None, GameMode.Expert, OpenBveApi.Colors.MessageColor.Red, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
				}
			}
			loadComplete = true;
			RenderRealTimeElapsed = 0.0;
			RenderTimeElapsed = 0.0;
			World.InitializeCameraRestriction();
			Loading.SimulationSetup = true;
			switch (Game.InitialViewpoint)
			{
				case 1:
					//Switch camera to exterior
					MainLoop.SaveCameraSettings();
					Program.Renderer.Camera.CurrentMode = CameraViewMode.Exterior;
					MainLoop.RestoreCameraSettings();
					for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					{
						TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.Exterior);
						//Make bogies visible
						TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
					}
					Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					World.UpdateAbsoluteCamera(0.0);
					World.UpdateViewingDistances();
					break;
				case 2:
					//Switch camera to track
					MainLoop.SaveCameraSettings();
					Program.Renderer.Camera.CurrentMode = CameraViewMode.Track;
					MainLoop.RestoreCameraSettings();
					for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					{
						TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.Exterior);
						TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
					}

					Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					World.UpdateAbsoluteCamera(0.0);
					World.UpdateViewingDistances();
					break;
				case 3:
					//Switch camera to flyby
					MainLoop.SaveCameraSettings();
					Program.Renderer.Camera.CurrentMode = CameraViewMode.FlyBy;
					MainLoop.RestoreCameraSettings();
					for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					{
						TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.Exterior);
						TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
					}

					Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					World.UpdateAbsoluteCamera(0.0);
					World.UpdateViewingDistances();
					break;
				case 4:
					//Switch camera to flyby
					MainLoop.SaveCameraSettings();
					Program.Renderer.Camera.CurrentMode = CameraViewMode.FlyByZooming;
					MainLoop.RestoreCameraSettings();
					for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					{
						TrainManager.PlayerTrain.Cars[j].ChangeCarSection(TrainManager.CarSectionType.Exterior);
						TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
						TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
					}

					Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					World.UpdateAbsoluteCamera(0.0);
					World.UpdateViewingDistances();
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
				Program.Renderer.Loading.DrawLoadingScreen(Fonts.SmallFont, Loading.RouteProgress, Loading.TrainProgress);
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
			Program.Renderer.Finalization();

			base.Dispose();
		}
	}
}
