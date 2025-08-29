using LibRender2.Viewports;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SoundManager;
using System;
using System.ComponentModel;
using System.Threading;
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
            MessageManager.UpdateMessages(TimeElapsed);
            SwapBuffers();
            
        }

        protected override void OnResize(EventArgs e)
        {
	        Program.Renderer.Screen.Width = Width;
	        Program.Renderer.Screen.Height = Height;
	        Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
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
			Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
            if (Program.processCommandLineArgs)
            {
                Program.processCommandLineArgs = false;
                Program.UpdateCaption();
				Program.LoadRoute();
                Program.UpdateCaption();
            }

            if (Width == DisplayDevice.Default.Width && Height == DisplayDevice.Default.Height)
            {
	            WindowState = WindowState.Maximized;
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
			Program.Renderer.VisibilityThreadShouldRun = false;
			if (!Loading.Complete && Program.CurrentRouteFile != null)
			{
				e.Cancel = true;
				Loading.Cancel = true;
			}
			if (Program.CurrentHost.MonoRuntime)
			{
				// Mono often fails to close the main window properly
				// give it a brief pause (to the visibility thread terminate cleanly)
				// then issue forceful closeure
				Thread.Sleep(100);
				Environment.Exit(0);
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
				Program.Renderer.GameWindow.ProcessEvents();
				if (Program.Renderer.GameWindow.IsExiting)
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
				Program.Renderer.GameWindow.SwapBuffers();

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
	}
}
