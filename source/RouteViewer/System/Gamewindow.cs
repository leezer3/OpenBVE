﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using OpenBveApi;
using OpenBveApi.Math;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SoundManager;
using Vector3 = OpenBveApi.Math.Vector3;

namespace RouteViewer
{
    class RouteViewer : GameWindow
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
				// Ignored- Just an icon
            }
        }

        //Default Properties
        private static bool currentlyLoading;
        private double ReducedModeEnteringTime;

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
	        base.OnUpdateFrame(e);

	        if (Loading.Complete && currentlyLoading)
	        {
		        currentlyLoading = false;
	        }
        }

        //This renders the frame
        protected override void OnRenderFrame(FrameEventArgs e)
        {
			Program.MouseMovement();
			Program.Renderer.FrameRate = RenderFrequency;

            //Do not do anything whilst loading
            if (currentlyLoading)
            {
                return;
            }
            ProcessEvents();
            double TimeElapsed = CPreciseTimer.GetElapsedTime();
            if (Program.CpuReducedMode)
            {
                Thread.Sleep(250);
            }
            else
            {
                Thread.Sleep(1);
                if (ReducedModeEnteringTime == 0)
                {
                    ReducedModeEnteringTime = 2500;
                }
                if (Program.Renderer.Camera.AlignmentDirection.Position.X != 0.0 | Program.Renderer.Camera.AlignmentDirection.Position.Y != 0.0 | Program.Renderer.Camera.AlignmentDirection.Position.Z != 0.0 | Program.Renderer.Camera.AlignmentDirection.Pitch != 0.0 | Program.Renderer.Camera.AlignmentDirection.Yaw != 0.0 | Program.Renderer.Camera.AlignmentDirection.Roll != 0.0 | Program.Renderer.Camera.AlignmentDirection.TrackPosition != 0.0 | Program.Renderer.Camera.AlignmentDirection.Zoom != 0.0)
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

            if (Program.CurrentRouteFile != null)
            {
	            DateTime d = DateTime.Now;
	            Game.SecondsSinceMidnight = (double)(3600 * d.Hour + 60 * d.Minute + d.Second) + 0.001 * (double)d.Millisecond;
	            ObjectManager.UpdateAnimatedWorldObjects(TimeElapsed, false);
	            World.UpdateAbsoluteCamera(TimeElapsed);
	            Program.Renderer.UpdateVisibility(Program.Renderer.CameraTrackFollower.TrackPosition + Program.Renderer.Camera.Alignment.Position.Z);
	            Program.Sounds.Update(TimeElapsed, SoundModels.Linear);
            }
            Program.Renderer.Lighting.UpdateLighting(Program.CurrentRoute.SecondsSinceMidnight, Program.CurrentRoute.LightDefinitions);
            Program.Renderer.RenderScene(TimeElapsed);
            MessageManager.UpdateMessages();
            SwapBuffers();
            
        }

        protected override void OnResize(EventArgs e)
        {
	        Program.Renderer.Screen.Width = Width;
	        Program.Renderer.Screen.Height = Height;
	        Program.Renderer.UpdateViewport();
        }

        protected override void OnLoad(EventArgs e)
        {
            KeyDown += Program.keyDownEvent;
            KeyUp += Program.keyUpEvent;
			MouseDown += Program.MouseEvent;
			MouseUp += Program.MouseEvent;
	        FileDrop += Program.FileDrop;
            Program.Renderer.Camera.Reset(new Vector3(0.0, 2.5, -5.0));
            Program.CurrentRoute.CurrentBackground.BackgroundImageDistance = 600.0;
            Program.Renderer.Camera.ForwardViewingDistance = 600.0;
            Program.Renderer.Camera.BackwardViewingDistance = 0.0;
            Program.Renderer.Camera.ExtraViewingDistance = 50.0;

            Program.Renderer.Initialize();
            Program.Renderer.Lighting.Initialize();
			Program.Sounds.Initialize(Program.CurrentHost, SoundRange.Low);
			Program.Renderer.UpdateViewport();
            if (Program.processCommandLineArgs)
            {
                Program.processCommandLineArgs = false;
                Program.LoadRoute();
                Program.UpdateCaption();
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
			if (!Loading.Complete && Program.CurrentRouteFile != null)
			{
				e.Cancel = true;
				Loading.Cancel = true;
			}
	    }

		public static void LoadingScreenLoop()
		{
			currentlyLoading = true;

			Program.Renderer.PushMatrix(MatrixMode.Projection);
			Matrix4D.CreateOrthographicOffCenter(0.0f, Program.Renderer.Screen.Width, Program.Renderer.Screen.Height, 0.0f, -1.0f, 1.0f, out Program.Renderer.CurrentProjectionMatrix);
			Program.Renderer.PushMatrix(MatrixMode.Modelview);
			Program.Renderer.CurrentViewMatrix = Matrix4D.Identity;

			while (!Loading.Complete && !Loading.Cancel)
			{
				CPreciseTimer.GetElapsedTime();
				Program.currentGameWindow.ProcessEvents();
				if (Program.currentGameWindow.IsExiting)
					Loading.Cancel = true;
				double routeProgress = 1.0;
				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
				{
					if (Program.CurrentHost.Plugins[i].Route != null && Program.CurrentHost.Plugins[i].Route.IsLoading)
					{
						routeProgress = Program.CurrentHost.Plugins[i].Route.CurrentProgress;
					}
				}
				Program.Renderer.Loading.DrawLoadingScreen(Program.Renderer.Fonts.SmallFont, routeProgress);
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
				Program.Renderer.PopMatrix(MatrixMode.Modelview);
				Program.Renderer.PopMatrix(MatrixMode.Projection);
			}
			else
			{
				Game.Reset();
				currentlyLoading = false;
				Program.CurrentRouteFile = null;
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

		public override void Dispose()
		{
			base.Dispose();
		}
    }
}
