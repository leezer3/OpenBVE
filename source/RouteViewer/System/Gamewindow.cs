﻿using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;
using OpenBveApi;
using OpenBveApi.Hosts;
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

            if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
            {
	            // attempted workaround for massive CPU usage when idle
	            TargetRenderFrequency = 5.0;
			}
			
        }

        //Default Properties
        private static bool currentlyLoading;

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

            if (Program.CurrentRouteFile != null)
            {
	            DateTime d = DateTime.Now;
	            Game.SecondsSinceMidnight = 3600 * d.Hour + 60 * d.Minute + d.Second + 0.001 * d.Millisecond;
	            ObjectManager.UpdateAnimatedWorldObjects(TimeElapsed, false);
	            World.UpdateAbsoluteCamera(TimeElapsed);
	            Program.Renderer.UpdateVisibility(true);
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
            KeyDown += Program.KeyDownEvent;
            KeyUp += Program.KeyUpEvent;
			MouseDown += Program.MouseEvent;
			MouseUp += Program.MouseEvent;
	        FileDrop += Program.FileDrop;
	        MouseMove += Program.MouseMoveEvent;
	        MouseWheel += Program.MouseWheelEvent;
			Program.Renderer.Camera.Reset(new Vector3(0.0, 2.5, -5.0));
            Program.CurrentRoute.CurrentBackground.BackgroundImageDistance = 600.0;
            Program.Renderer.Camera.ForwardViewingDistance = 600.0;
            Program.Renderer.Camera.BackwardViewingDistance = 0.0;
            Program.Renderer.Camera.ExtraViewingDistance = 50.0;

            Program.Renderer.Initialize();
            Program.Renderer.Lighting.Initialize();
			Program.Sounds.Initialize(SoundRange.Low);
			Program.Renderer.UpdateViewport();
            if (Program.ProcessCommandLineArgs)
            {
                Program.ProcessCommandLineArgs = false;
                Program.UpdateCaption();
				Program.LoadRoute();
                Program.UpdateCaption();
            }
        }

	    protected override void OnClosing(CancelEventArgs e)
	    {
			Interface.CurrentOptions.Save(Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_rv.cfg"));
			// Minor hack:
			// If we are currently loading, catch the first close event, and terminate the loader threads
			// before actually closing the game-window.
			if (Loading.Cancel)
			{
				return;
			}
			Program.Renderer.visibilityThread = false;
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
				Program.CurrentGameWindow.ProcessEvents();
				if (Program.CurrentGameWindow.IsExiting)
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
				Program.CurrentGameWindow.SwapBuffers();

				if (Loading.JobAvailable)
				{
					while (jobs.Count > 0)
					{
						jobs.TryDequeue(out ThreadStart currentJob);
						currentJob();
						lock (currentJob)
						{
							Monitor.Pulse(currentJob);
						}
					}
					Loading.JobAvailable = false;
				}
				double time = CPreciseTimer.GetElapsedTime();
				double wait = 1000.0 / 60.0 - time * 1000 - 50;
				if (wait > 0)
					Thread.Sleep((int)(wait));
			}
			Program.Renderer.Loading.CompleteLoading();
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

#pragma warning disable 0649
		private static ConcurrentQueue<ThreadStart> jobs;
#pragma warning restore 0649

		/// <summary>This method is used during loading to run commands requiring an OpenGL context in the main render loop</summary>
		/// <param name="job">The OpenGL command</param>
		/// <param name="timeout">The timeout</param>
		internal static void RunInRenderThread(ThreadStart job, int timeout)
		{
			object locker = new object();
			jobs.Enqueue(job);
			//Don't set the job to available until after it's been loaded into the queue
			Loading.JobAvailable = true;
			//Failsafe: If our job has taken more than the timeout, stop waiting for it
			//A missing texture is probably better than an infinite loadscreen
			lock (job)
			{
				Monitor.Wait(job, timeout);
			}
		}
	}
}
