using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using GL = OpenTK.Graphics.OpenGL.GL;
using MatrixMode = OpenTK.Graphics.OpenGL.MatrixMode;

namespace OpenBve
{
    class OpenBVEGame: GameWindow
    {
        int TimeFactor = 1;
        double TotalTimeElapsedForInfo = 0.0;
        double TotalTimeElapsedForSectionUpdate = 0.0;
        private bool loadComplete;
        private bool firstFrame;
        private double RenderTimeElapsed;
        private double RenderRealTimeElapsed;
        //We need to explicitly specify the default constructor
        public OpenBVEGame(int width, int height, GraphicsMode currentGraphicsMode, string openbve, GameWindowFlags @default): base (width,height,currentGraphicsMode,openbve,@default)
        {
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
            //Use the OpenTK framerate as this is much more accurate
            //Also avoids running a calculation
            if (TotalTimeElapsedForInfo >= 0.2)
            {
                Game.InfoFrameRate = RenderFrequency;
                TotalTimeElapsedForInfo = 0.0;
            }

            //We need to update the camera position in the render sequence
            //Not doing this means that the camera doesn't move
            // update in one piece
            ObjectManager.UpdateAnimatedWorldObjects(TimeElapsed, false);
            if (World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead | World.CameraMode == World.CameraViewMode.Exterior)
            {
                TrainManager.UpdateCamera(TrainManager.PlayerTrain);
            }
            if (World.CameraRestriction == World.CameraRestrictionMode.NotAvailable)
            {
                World.UpdateDriverBody(TimeElapsed);
            }
            World.UpdateAbsoluteCamera(TimeElapsed);
            TrainManager.UpdateTrainObjects(TimeElapsed, false);
            if (World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead | World.CameraMode == World.CameraViewMode.Exterior)
            {
                ObjectManager.UpdateVisibility(World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z);
                int d = TrainManager.PlayerTrain.DriverCar;
                World.CameraSpeed = TrainManager.PlayerTrain.Cars[d].Specs.CurrentSpeed;
            }
            else
            {
                World.CameraSpeed = 0.0;
            }

            World.CameraAlignmentDirection = new World.CameraAlignment();
            if (MainLoop.Quit)
            {
                Program.currentGameWindow.Exit();
            }
            
                Renderer.RenderScene(TimeElapsed);
                
                Program.currentGameWindow.SwapBuffers();

                Game.UpdateBlackBox();

                // pause/menu
                while (Game.CurrentInterface != Game.InterfaceType.Normal)
                {
                    MainLoop.UpdateControlRepeats(RealTimeElapsed);
                    MainLoop.ProcessKeyboard();
                    MainLoop.ProcessControls(0.0);
                    if (MainLoop.Quit) break;
                    if (Game.CurrentInterface == Game.InterfaceType.Pause)
                    {
                        System.Threading.Thread.Sleep(10);
                    }
                    Renderer.RenderScene(TimeElapsed);
                    Program.currentGameWindow.SwapBuffers();
                    TimeElapsed = CPreciseTimer.GetElapsedTime();
                }
                // limit framerate
                if (MainLoop.LimitFramerate)
                {
                    System.Threading.Thread.Sleep(10);
                }
                MainLoop.UpdateControlRepeats(RealTimeElapsed);
                MainLoop.ProcessKeyboard();
                World.UpdateMouseGrab(TimeElapsed);
                MainLoop.ProcessControls(TimeElapsed);
                RenderRealTimeElapsed = 0.0;
                RenderTimeElapsed = 0.0;
                
                

#if DEBUG
                //MainLoop.CheckForOpenGlError("MainLoop");
             
#endif
                       
            // finish
            try
            {
                Interface.SaveLogs();
            }
            catch { }
            try
            {
                Interface.SaveOptions();
            }
            catch { }

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            TimeFactor = MainLoop.TimeFactor;
            // timer
            double RealTimeElapsed;
            double TimeElapsed;
            if (Game.SecondsSinceMidnight >= Game.StartupTime)
            {
                RealTimeElapsed = CPreciseTimer.GetElapsedTime();
                TimeElapsed = RealTimeElapsed * (double)TimeFactor;
            }
            else
            {
                RealTimeElapsed = 0.0;
                TimeElapsed = Game.StartupTime - Game.SecondsSinceMidnight;
            }
#if DEBUG
            //If we're in debug mode and a frame takes greater than a second to render, we can safely assume that VS has hit a breakpoint
            //Check this and the sim no longer barfs because the update time was too great
            if (RealTimeElapsed > 1)
            {
                RealTimeElapsed = 0.0;
                TimeElapsed = 0.0;
            }
#endif
            TotalTimeElapsedForInfo += TimeElapsed;
            TotalTimeElapsedForSectionUpdate += TimeElapsed;

            
            if (TotalTimeElapsedForSectionUpdate >= 1.0)
            {
                if (Game.Sections.Length != 0)
                {
                    Game.UpdateSection(Game.Sections.Length - 1);
                }
                TotalTimeElapsedForSectionUpdate = 0.0;
            }
           
            // events
            
            // update simulation in chunks
            {
                const double chunkTime = 1.0 / 2.0;
                if (TimeElapsed <= chunkTime)
                {
                    Game.SecondsSinceMidnight += TimeElapsed;
                    TrainManager.UpdateTrains(TimeElapsed);
                }
                else
                {
                    const int maxChunks = 2;
                    int chunks = Math.Min((int)Math.Round(TimeElapsed / chunkTime), maxChunks);
                    double time = TimeElapsed / (double)chunks;
                    for (int i = 0; i < chunks; i++)
                    {
                        Game.SecondsSinceMidnight += time;
                        TrainManager.UpdateTrains(time);
                    }
                }
            }
            Game.UpdateScore(TimeElapsed);
            Game.UpdateMessages();
            Game.UpdateScoreMessages(TimeElapsed);
            Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
            RenderTimeElapsed += TimeElapsed;
            RenderRealTimeElapsed += RealTimeElapsed;
            if (loadComplete && !firstFrame)
            {
                firstFrame = true;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            jobs = new Queue<ThreadStart>(10);
            locks = new Queue<object>(10);
            Renderer.Initialize();
            Renderer.InitializeLighting();
            MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
            MainLoop.InitializeMotionBlur();
            Loading.LoadAsynchronously(MainLoop.currentResult.RouteFile, MainLoop.currentResult.RouteEncoding, MainLoop.currentResult.TrainFolder, MainLoop.currentResult.TrainEncoding);
            LoadingScreenLoop();
        }

        private void SetupSimulation()
        {
            Renderer.InitializeLighting();
            Timetable.CreateTimetable();
            for (int i = 0; i < Interface.MessageCount; i++)
            {
                if (Interface.Messages[i].Type == Interface.MessageType.Critical)
                {
                    MessageBox.Show("A critical error has occured:\n\n" + Interface.Messages[i].Text + "\n\nPlease inspect the error log file for further information.", "Load", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
            }
            Renderer.InitializeLighting();
            Game.LogRouteName = System.IO.Path.GetFileName(MainLoop.currentResult.RouteFile);
            Game.LogTrainName = System.IO.Path.GetFileName(MainLoop.currentResult.TrainFolder);
            Game.LogDateTime = DateTime.Now;


            Textures.UnloadAllTextures();
            if (Interface.CurrentOptions.LoadInAdvance)
            {
                Textures.LoadAllTextures();
            }
            // camera
            ObjectManager.InitializeVisibility();
            TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, 0.0, true, false);
            TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, -0.1, true, false);
            TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, 0.1, true, false);
            World.CameraTrackFollower.TriggerType = TrackManager.EventTriggerType.Camera;
            // starting time and track position
            Game.SecondsSinceMidnight = 0.0;
            Game.StartupTime = 0.0;
            int PlayerFirstStationIndex = -1;
            double PlayerFirstStationPosition = 0.0;
            for (int i = 0; i < Game.Stations.Length; i++)
            {
                if (Game.Stations[i].StopMode == Game.StationStopMode.AllStop | Game.Stations[i].StopMode == Game.StationStopMode.PlayerStop & Game.Stations[i].Stops.Length != 0)
                {
                    PlayerFirstStationIndex = i;
                    int s = Game.GetStopIndex(i, TrainManager.PlayerTrain.Cars.Length);
                    if (s >= 0)
                    {
                        PlayerFirstStationPosition = Game.Stations[i].Stops[s].TrackPosition;
                    }
                    else
                    {
                        PlayerFirstStationPosition = Game.Stations[i].DefaultTrackPosition;
                    }
                    if (Game.Stations[i].ArrivalTime < 0.0)
                    {
                        if (Game.Stations[i].DepartureTime < 0.0)
                        {
                            Game.SecondsSinceMidnight = 0.0;
                            Game.StartupTime = 0.0;
                        }
                        else
                        {
                            Game.SecondsSinceMidnight = Game.Stations[i].DepartureTime - Game.Stations[i].StopTime;
                            Game.StartupTime = Game.Stations[i].DepartureTime - Game.Stations[i].StopTime;
                        }
                    }
                    else
                    {
                        Game.SecondsSinceMidnight = Game.Stations[i].ArrivalTime;
                        Game.StartupTime = Game.Stations[i].ArrivalTime;
                    }
                    break;
                }
            }
            int OtherFirstStationIndex = -1;
            double OtherFirstStationPosition = 0.0;
            double OtherFirstStationTime = 0.0;
            for (int i = 0; i < Game.Stations.Length; i++)
            {
                if (Game.Stations[i].StopMode == Game.StationStopMode.AllStop | Game.Stations[i].StopMode == Game.StationStopMode.PlayerPass & Game.Stations[i].Stops.Length != 0)
                {
                    OtherFirstStationIndex = i;
                    int s = Game.GetStopIndex(i, TrainManager.PlayerTrain.Cars.Length);
                    if (s >= 0)
                    {
                        OtherFirstStationPosition = Game.Stations[i].Stops[s].TrackPosition;
                    }
                    else
                    {
                        OtherFirstStationPosition = Game.Stations[i].DefaultTrackPosition;
                    }
                    if (Game.Stations[i].ArrivalTime < 0.0)
                    {
                        if (Game.Stations[i].DepartureTime < 0.0)
                        {
                            OtherFirstStationTime = 0.0;
                        }
                        else
                        {
                            OtherFirstStationTime = Game.Stations[i].DepartureTime - Game.Stations[i].StopTime;
                        }
                    }
                    else
                    {
                        OtherFirstStationTime = Game.Stations[i].ArrivalTime;
                    }
                    break;
                }
            }
            if (Game.PrecedingTrainTimeDeltas.Length != 0)
            {
                OtherFirstStationTime -= Game.PrecedingTrainTimeDeltas[Game.PrecedingTrainTimeDeltas.Length - 1];
                if (OtherFirstStationTime < Game.SecondsSinceMidnight)
                {
                    Game.SecondsSinceMidnight = OtherFirstStationTime;
                }
            }
            // initialize trains
            for (int i = 0; i < TrainManager.Trains.Length; i++)
            {
                TrainManager.InitializeTrain(TrainManager.Trains[i]);
                int s = i == TrainManager.PlayerTrain.TrainIndex ? PlayerFirstStationIndex : OtherFirstStationIndex;
                if (s >= 0)
                {
                    if (Game.Stations[s].OpenLeftDoors)
                    {
                        for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++)
                        {
                            TrainManager.Trains[i].Cars[j].Specs.AnticipatedLeftDoorsOpened = true;
                        }
                    }
                    if (Game.Stations[s].OpenRightDoors)
                    {
                        for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++)
                        {
                            TrainManager.Trains[i].Cars[j].Specs.AnticipatedRightDoorsOpened = true;
                        }
                    }
                }
                if (Game.Sections.Length != 0)
                {
                    Game.Sections[0].Enter(TrainManager.Trains[i]);
                }
                for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++)
                {
                    double length = TrainManager.Trains[i].Cars[0].Length;
                    TrainManager.MoveCar(TrainManager.Trains[i], j, -length, 0.01);
                    TrainManager.MoveCar(TrainManager.Trains[i], j, length, 0.01);
                }
            }
            // score
            Game.CurrentScore.ArrivalStation = PlayerFirstStationIndex + 1;
            Game.CurrentScore.DepartureStation = PlayerFirstStationIndex;
            Game.CurrentScore.Maximum = 0;
            for (int i = 0; i < Game.Stations.Length; i++)
            {
                if (i != PlayerFirstStationIndex & Game.PlayerStopsAtStation(i))
                {
                    if (i == 0 || Game.Stations[i - 1].StationType != Game.StationType.ChangeEnds)
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
            if (Game.Sections.Length > 0)
            {
                Game.UpdateSection(Game.Sections.Length - 1);
            }
            // move train in position
            for (int i = 0; i < TrainManager.Trains.Length; i++)
            {
                double p;
                if (i == TrainManager.PlayerTrain.TrainIndex)
                {
                    p = PlayerFirstStationPosition;
                }
                else if (TrainManager.Trains[i].State == TrainManager.TrainState.Bogus)
                {
                    p = Game.BogusPretrainInstructions[0].TrackPosition;
                    TrainManager.Trains[i].AI = new Game.BogusPretrainAI(TrainManager.Trains[i]);
                }
                else
                {
                    p = OtherFirstStationPosition;
                }
                for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++)
                {
                    TrainManager.MoveCar(TrainManager.Trains[i], j, p, 0.01);
                }
            }
            // timetable
            if (Timetable.DefaultTimetableDescription.Length == 0)
            {
                Timetable.DefaultTimetableDescription = Game.LogTrainName;
            }

            // initialize camera
            if (World.CameraRestriction == World.CameraRestrictionMode.NotAvailable)
            {
                World.CameraMode = World.CameraViewMode.InteriorLookAhead;
            }
            TrainManager.UpdateCamera(TrainManager.PlayerTrain);
            TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, -1.0, true, false);
            ObjectManager.UpdateVisibility(World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z);
            World.CameraSavedInterior = new World.CameraAlignment();
            World.CameraSavedExterior = new World.CameraAlignment(new OpenBveApi.Math.Vector3(-2.5, 1.5, -15.0), 0.3, -0.2, 0.0, PlayerFirstStationPosition, 1.0);
            World.CameraSavedTrack = new World.CameraAlignment(new OpenBveApi.Math.Vector3(-3.0, 2.5, 0.0), 0.3, 0.0, 0.0, TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition - 10.0, 1.0);
            // signalling sections
            for (int i = 0; i < TrainManager.Trains.Length; i++)
            {
                int s = TrainManager.Trains[i].CurrentSectionIndex;
                Game.Sections[s].Enter(TrainManager.Trains[i]);
            }
            if (Game.Sections.Length > 0)
            {
                Game.UpdateSection(Game.Sections.Length - 1);
            }
            // fast-forward until start time
            {
                Game.MinimalisticSimulation = true;
                const double w = 0.25;
                double u = Game.StartupTime - Game.SecondsSinceMidnight;
                if (u > 0)
                {
                    while (true)
                    {
                        double v = u < w ? u : w; u -= v;
                        Game.SecondsSinceMidnight += v;
                        TrainManager.UpdateTrains(v);
                        if (u <= 0.0) break;
                        TotalTimeElapsedForSectionUpdate += v;
                        if (TotalTimeElapsedForSectionUpdate >= 1.0)
                        {
                            if (Game.Sections.Length > 0)
                            {
                                Game.UpdateSection(Game.Sections.Length - 1);
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
            // timetable
            if (TrainManager.PlayerTrain.Station >= 0)
            {
                Timetable.UpdateCustomTimetable(Game.Stations[TrainManager.PlayerTrain.Station].TimetableDaytimeTexture, Game.Stations[TrainManager.PlayerTrain.Station].TimetableNighttimeTexture);
                if (Timetable.CustomObjectsUsed != 0 & Timetable.CustomTimetableAvailable)
                {
                    Timetable.CurrentTimetable = Timetable.TimetableState.Custom;
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
                    if (Interface.Messages[i].FileNotFound)
                    {
                        filesNotFound++;
                    }
                    else if (Interface.Messages[i].Type == Interface.MessageType.Error)
                    {
                        errors++;
                    }
                    else if (Interface.Messages[i].Type == Interface.MessageType.Warning)
                    {
                        warnings++;
                    }
                }
                if (filesNotFound != 0)
                {
                    Game.AddDebugMessage(filesNotFound.ToString() + " file(s) not found", 10.0);
                }
                if (errors != 0 & warnings != 0)
                {
                    Game.AddDebugMessage(errors.ToString() + " error(s), " + warnings.ToString() + " warning(s)", 10.0);
                }
                else if (errors != 0)
                {
                    Game.AddDebugMessage(errors.ToString() + " error(s)", 10.0);
                }
                else
                {
                    Game.AddDebugMessage(warnings.ToString() + " warning(s)", 10.0);
                }
            }
            loadComplete = true;
            RenderRealTimeElapsed = 0.0;
            RenderTimeElapsed = 0.0;
            World.InitializeCameraRestriction();
        }

        private void LoadingScreenLoop()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0.0, (double)Screen.Width, (double)Screen.Height, 0.0, -1.0, 1.0);
            
            while (!Loading.Complete)
            {
                CPreciseTimer.GetElapsedTime();
                GL.Viewport(0, 0, Screen.Width, Screen.Height);
                Renderer.DrawLoadingScreen();
                Program.currentGameWindow.SwapBuffers();
                
                if (JobAvailable)
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
                    JobAvailable = false;
                }
                double time = CPreciseTimer.GetElapsedTime();
                double wait = 1000.0 / 60.0 - time * 1000 - 50;
                if (wait > 0)
                    Thread.Sleep((int)(wait));
            }
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            SetupSimulation();
        }
        private static bool JobAvailable = false;
        private static readonly object jobLock = new object();
        private static Queue<ThreadStart> jobs;
        private static Queue<object> locks;
        internal static void RunInRenderThread(ThreadStart job)
        {
            object locker = new object();
            lock (jobLock)
            {
                JobAvailable = true;
                jobs.Enqueue(job);
                locks.Enqueue(locker);
            }
            lock (locker)
            {
                Monitor.Wait(locker);
            }
        }
    }
}
