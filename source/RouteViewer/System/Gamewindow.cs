using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
    class RouteViewer : OpenTK.GameWindow
    {
        //Deliberately specify the default constructor with various overrides
        public RouteViewer(int width, int height, GraphicsMode currentGraphicsMode, string openbve, GameWindowFlags @default): base (width,height,currentGraphicsMode,openbve,@default)
        {
            try
            {
                System.Drawing.Icon ico = new System.Drawing.Icon("data\\icon.ico");
                this.Icon = ico;
            }
            catch
            {
            }
        }

        //Default Properties
        private bool currentlyLoading;
        private double ReducedModeEnteringTime;

        //This renders the frame
        protected override void OnRenderFrame(FrameEventArgs e)
        {
			Program.MouseMovement();
			Game.InfoFrameRate = RenderFrequency;
            GL.ClearColor(0.75f, 0.75f, 0.75f, 1.0f);
            //Do not do anything whilst loading
            if (currentlyLoading)
            {
                return;
            }
            ProcessEvents();
            double TimeElapsed = CPreciseTimer.GetElapsedTime();
            if (Program.CpuReducedMode)
            {
                System.Threading.Thread.Sleep(250);
            }
            else
            {
                System.Threading.Thread.Sleep(1);
                if (ReducedModeEnteringTime == 0)
                {
                    ReducedModeEnteringTime = 2500;
                }
                if (World.CameraAlignmentDirection.Position.X != 0.0 | World.CameraAlignmentDirection.Position.Y != 0.0 | World.CameraAlignmentDirection.Position.Z != 0.0 | World.CameraAlignmentDirection.Pitch != 0.0 | World.CameraAlignmentDirection.Yaw != 0.0 | World.CameraAlignmentDirection.Roll != 0.0 | World.CameraAlignmentDirection.TrackPosition != 0.0 | World.CameraAlignmentDirection.Zoom != 0.0)
                {
                    ReducedModeEnteringTime = 2500;
                }
                //Automatically enter reduced CPU mode if appropriate
                if (Program.CpuAutomaticMode && Program.CpuReducedMode == false)
                {
                    ReducedModeEnteringTime -= TimeElapsed;
                    if (ReducedModeEnteringTime <= 0)
                    {
                        Program.CpuReducedMode = true;
                        ReducedModeEnteringTime = 0;
                    }
                }
            }
            DateTime d = DateTime.Now;
            Game.SecondsSinceMidnight = (double)(3600 * d.Hour + 60 * d.Minute + d.Second) + 0.001 * (double)d.Millisecond;
            ObjectManager.UpdateAnimatedWorldObjects(TimeElapsed, false);
            World.UpdateAbsoluteCamera(TimeElapsed);
            ObjectManager.UpdateVisibility(World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z);
	        TextureManager.Update(TimeElapsed);
			Sounds.Update(TimeElapsed, Sounds.SoundModels.Linear);
            Renderer.RenderScene(TimeElapsed);
            SwapBuffers();
            
        }

        protected override void OnResize(EventArgs e)
        {
            Renderer.ScreenWidth = Width;
            Renderer.ScreenHeight = Height;
            Program.UpdateViewport();
        }

        protected override void OnLoad(EventArgs e)
        {
            KeyDown += Program.keyDownEvent;
            KeyUp += Program.keyUpEvent;
			MouseDown += Program.MouseEvent;
			MouseUp += Program.MouseEvent;
	        FileDrop += Program.FileDrop;
            Program.ResetCamera();
            World.BackgroundImageDistance = 600.0;
            World.ForwardViewingDistance = 600.0;
            World.BackwardViewingDistance = 0.0;
            World.ExtraViewingDistance = 50.0;

            Renderer.Initialize();
            Renderer.InitializeLighting();
            Sounds.Initialize();
            Program.UpdateViewport();
            if (Program.processCommandLineArgs)
            {
                Program.processCommandLineArgs = false;
                for (int i = 0; i < Program.commandLineArguments.Length; i++)
                {
                    if (!Program.SkipArgs[i] && System.IO.File.Exists(Program.commandLineArguments[i]))
                    {
                        currentlyLoading = true;
                        Program.CurrentRoute = Program.commandLineArguments[i];
                        Program.LoadRoute();
                        Program.UpdateCaption();
                        break;
                    }
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
	    }

		public static void LoadingScreenLoop()
		{
			GL.Disable(EnableCap.DepthTest);
			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.LoadIdentity();
			GL.Ortho(0.0, (double)Renderer.ScreenWidth, (double)Renderer.ScreenHeight, 0.0, -1.0, 1.0);
			GL.Viewport(0, 0, Renderer.ScreenWidth, Renderer.ScreenHeight);

			while (!Loading.Complete && !Loading.Cancel)
			{
				CPreciseTimer.GetElapsedTime();
				Program.currentGameWindow.ProcessEvents();
				if (Program.currentGameWindow.IsExiting)
					Loading.Cancel = true;
				Renderer.DrawLoadingScreen();
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
			if (!Loading.Cancel)
			{

				GL.PopMatrix();
				GL.MatrixMode(MatrixMode.Projection);
			}
			else
			{
				Program.currentGameWindow.Exit();
			}
		}

		internal static readonly object LoadingLock = new object();

		private static readonly object jobLock = new object();
#pragma warning disable 0649
		private static Queue<ThreadStart> jobs;
		private static Queue<object> locks;
#pragma warning restore 0649

		/// <summary>This method is used during loading to run commands requiring an OpenGL context in the main render loop</summary>
		/// <param name="job">The OpenGL command</param>
		internal static void RunInRenderThread(ThreadStart job)
		{
			object locker = new object();
			lock (jobLock)
			{
				Loading.JobAvailable = true;
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
