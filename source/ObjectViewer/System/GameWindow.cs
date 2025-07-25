using LibRender2.Viewports;
using ObjectViewer.Trains;
using OpenBveApi;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.ComponentModel;
using System.Threading;
using Vector3 = OpenBveApi.Math.Vector3;

namespace ObjectViewer
{
    class ObjectViewer : GameWindow
    {
        //Deliberately specify the default constructor with various overrides
        public ObjectViewer(int width, int height, GraphicsMode currentGraphicsMode, string openbve,
            GameWindowFlags @default) : base(width, height, currentGraphicsMode, openbve, @default)
        {
            try
            {
                System.Drawing.Icon ico = new System.Drawing.Icon("data\\icon.ico");
                this.Icon = ico;
            }
            catch
            {
	            //Ignored
            }
        }

		private double RenderRealTimeElapsed;
		private double TotalTimeElapsedForInfo;

        private static double RotateXSpeed = 0.0;
        private static double RotateYSpeed = 0.0;
        
        private static double MoveXSpeed = 0.0;
        private static double MoveYSpeed = 0.0;
        private static double MoveZSpeed = 0.0;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
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
			double timeElapsed = RenderRealTimeElapsed;

			// Use the OpenTK frame rate as this is much more accurate
			// Also avoids running a calculation
			if (TotalTimeElapsedForInfo >= 0.2)
			{
				Program.Renderer.FrameRate = RenderFrequency;
				TotalTimeElapsedForInfo = 0.0;
			}

            Program.MouseMovement();

			ObjectManager.UpdateAnimatedWorldObjects(timeElapsed, false);

			if (Program.TrainManager.Trains.Count != 0)
			{
				Program.TrainManager.Trains[0].UpdateObjects(timeElapsed, false);
			}

            bool updatelight = false;
            // rotate x
            if (Program.RotateX == 0)
            {
                double d = (1.0 + Math.Abs(RotateXSpeed)) * timeElapsed;
                if (RotateXSpeed >= -d & RotateXSpeed <= d)
                {
                    RotateXSpeed = 0.0;
                }
                else
                {
                    RotateXSpeed -= Math.Sign(RotateXSpeed) * d;
                }
            }
            else
            {
                double d = (1.0 + 1.0 - 1.0 / (1.0 + RotateXSpeed * RotateXSpeed)) * timeElapsed;
                double m = 1.0;
                RotateXSpeed += Program.RotateX * d;
                if (RotateXSpeed < -m)
                {
                    RotateXSpeed = -m;
                }
                else if (RotateXSpeed > m)
                {
                    RotateXSpeed = m;
                }
            }
            if (RotateXSpeed != 0.0)
            {
	            Program.Renderer.Camera.AbsoluteDirection.Rotate(Vector3.Down, RotateXSpeed * timeElapsed);
                Program.Renderer.Camera.AbsoluteUp.Rotate(Vector3.Down, RotateXSpeed * timeElapsed);
                Program.Renderer.Camera.AbsoluteSide.Rotate(Vector3.Down, RotateXSpeed * timeElapsed);
            }
            // rotate y
            if (Program.RotateY == 0)
            {
                double d = (1.0 + Math.Abs(RotateYSpeed)) * timeElapsed;
                if (RotateYSpeed >= -d & RotateYSpeed <= d)
                {
                    RotateYSpeed = 0.0;
                }
                else
                {
                    RotateYSpeed -= Math.Sign(RotateYSpeed) * d;
                }
            }
            else
            {
                double d = (1.0 + 1.0 - 1.0 / (1.0 + RotateYSpeed * RotateYSpeed)) * timeElapsed;
                double m = 1.0;
                RotateYSpeed += Program.RotateY * d;
                if (RotateYSpeed < -m)
                {
                    RotateYSpeed = -m;
                }
                else if (RotateYSpeed > m)
                {
                    RotateYSpeed = m;
                }
            }
            if (RotateYSpeed != 0.0)
            {
	            Program.Renderer.Camera.AbsoluteDirection.Rotate(Program.Renderer.Camera.AbsoluteSide, RotateYSpeed * timeElapsed);
                Program.Renderer.Camera.AbsoluteUp.Rotate(Program.Renderer.Camera.AbsoluteSide, RotateYSpeed * timeElapsed);
            }
            // move x
            if (Program.MoveX == 0)
            {
                double d = (2.5 + Math.Abs(MoveXSpeed)) * timeElapsed;
                if (MoveXSpeed >= -d & MoveXSpeed <= d)
                {
                    MoveXSpeed = 0.0;
                }
                else
                {
                    MoveXSpeed -= Math.Sign(MoveXSpeed) * d;
                }
            }
            else
            {
                double d = (5.0 + 10.0 - 10.0 / (1.0 + MoveXSpeed * MoveXSpeed)) * timeElapsed;
                double m = 25.0;
                MoveXSpeed += Program.MoveX * d;
                if (MoveXSpeed < -m)
                {
                    MoveXSpeed = -m;
                }
                else if (MoveXSpeed > m)
                {
                    MoveXSpeed = m;
                }
            }
            if (MoveXSpeed != 0.0)
            {
	            Program.Renderer.Camera.AbsolutePosition += MoveXSpeed * timeElapsed * Program.Renderer.Camera.AbsoluteSide;
            }
            // move y
            if (Program.MoveY == 0)
            {
                double d = (2.5 + Math.Abs(MoveYSpeed)) * timeElapsed;
                if (MoveYSpeed >= -d & MoveYSpeed <= d)
                {
                    MoveYSpeed = 0.0;
                }
                else
                {
                    MoveYSpeed -= Math.Sign(MoveYSpeed) * d;
                }
            }
            else
            {
                double d = (5.0 + 10.0 - 10.0 / (1.0 + MoveYSpeed * MoveYSpeed)) * timeElapsed;
                double m = 25.0;
                MoveYSpeed += Program.MoveY * d;
                if (MoveYSpeed < -m)
                {
                    MoveYSpeed = -m;
                }
                else if (MoveYSpeed > m)
                {
                    MoveYSpeed = m;
                }
            }
            if (MoveYSpeed != 0.0)
            {
	            Program.Renderer.Camera.AbsolutePosition += MoveYSpeed * timeElapsed * Program.Renderer.Camera.AbsoluteUp;
            }
            // move z
            if (Program.MoveZ == 0)
            {
                double d = (2.5 + Math.Abs(MoveZSpeed)) * timeElapsed;
                if (MoveZSpeed >= -d & MoveZSpeed <= d)
                {
                    MoveZSpeed = 0.0;
                }
                else
                {
                    MoveZSpeed -= Math.Sign(MoveZSpeed) * d;
                }
            }
            else
            {
                double d = (5.0 + 10.0 - 10.0 / (1.0 + MoveZSpeed * MoveZSpeed)) * timeElapsed;
                double m = 25.0;
                MoveZSpeed += Program.MoveZ * d;
                if (MoveZSpeed < -m)
                {
                    MoveZSpeed = -m;
                }
                else if (MoveZSpeed > m)
                {
                    MoveZSpeed = m;
                }
            }
            if (MoveZSpeed != 0.0)
            {
	            Program.Renderer.Camera.AbsolutePosition += MoveZSpeed * timeElapsed * Program.Renderer.Camera.AbsoluteDirection;
            }
            // lighting
            if (Program.LightingRelative == -1)
            {
                Program.LightingRelative = Program.LightingTarget;
                updatelight = true;
            }
            if (Program.LightingTarget == 0)
            {
                if (Program.LightingRelative != 0.0)
                {
                    Program.LightingRelative -= 0.5 * timeElapsed;
                    if (Program.LightingRelative < 0.0) Program.LightingRelative = 0.0;
                    updatelight = true;
                }
            }
            else
            {
                if (Program.LightingRelative != 1.0)
                {
                    Program.LightingRelative += 0.5 * timeElapsed;
                    if (Program.LightingRelative > 1.0) Program.LightingRelative = 1.0;
                    updatelight = true;
                }
            }
            // continue
            if (updatelight)
            {
				Program.Renderer.Lighting.OptionAmbientColor.R = (byte)Math.Round(32.0 + 128.0 * Program.LightingRelative * (2.0 - Program.LightingRelative));
				Program.Renderer.Lighting.OptionAmbientColor.G = (byte)Math.Round(32.0 + 128.0 * 0.5 * (Program.LightingRelative + Program.LightingRelative * (2.0 - Program.LightingRelative)));
				Program.Renderer.Lighting.OptionAmbientColor.B = (byte)Math.Round(32.0 + 128.0 * Program.LightingRelative);
				Program.Renderer.Lighting.OptionDiffuseColor.R = (byte)Math.Round(32.0 + 128.0 * Program.LightingRelative);
				Program.Renderer.Lighting.OptionDiffuseColor.G = (byte)Math.Round(32.0 + 128.0 * Program.LightingRelative);
				Program.Renderer.Lighting.OptionDiffuseColor.B = (byte)Math.Round(32.0 + 128.0 * Math.Sqrt(Program.LightingRelative));
				
            }
            Program.Renderer.Lighting.Initialize();
            Program.Renderer.RenderScene(timeElapsed);
            SwapBuffers();

			RenderRealTimeElapsed = 0.0;
        }

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			double RealTimeElapsed = CPreciseTimer.GetElapsedTime();
			DateTime time = DateTime.Now;
			Game.SecondsSinceMidnight = 3600 * time.Hour + 60 * time.Minute + time.Second + 0.001 * time.Millisecond;

			NearestTrain.Apply();

			if (NearestTrain.IsExtensionsCfg)
			{
				Program.TrainManager.Trains[0].UpdateBrakeSystem(RealTimeElapsed, out _, out _); // dummy simulation
			}

			TotalTimeElapsedForInfo += RealTimeElapsed;
			RenderRealTimeElapsed += RealTimeElapsed;
		}

        protected override void OnResize(EventArgs e)
        {
	        Program.Renderer.Screen.Width = Width;
	        Program.Renderer.Screen.Height = Height;
            Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
        }

        protected override void OnLoad(EventArgs e)
        {
            KeyDown += Program.KeyDown;
            KeyUp += Program.KeyUp;
            MouseDown += Program.MouseEvent;
            MouseUp += Program.MouseEvent;
			MouseWheel += Program.MouseWheelEvent;
			MouseMove += Program.MouseMoveEvent;
	        FileDrop += Program.DragFile;
	        Program.Renderer.Camera.Reset(new Vector3(-5.0, 2.5, -25.0));
            Program.Renderer.Initialize();
            Program.Renderer.Lighting.Initialize();
            Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
			Program.Renderer.InitializeVisibility();
			Program.Renderer.UpdateVisibility(true);
            ObjectManager.UpdateAnimatedWorldObjects(0.01, true);
			Program.RefreshObjects();
			if (Width == DisplayDevice.Default.Width && Height == DisplayDevice.Default.Height)
			{
				WindowState = WindowState.Maximized;
			}
		}

        protected override void OnClosing(CancelEventArgs e)
        {
	        Interface.CurrentOptions.Save(Path.CombineFile(Program.FileSystem.SettingsFolder, "1.5.0/options_ov.cfg"));
			Program.Renderer.VisibilityThreadShouldRun = false;
			if (Program.CurrentHost.MonoRuntime)
			{
				Environment.Exit(0);
			}
        }

        protected override void OnUnload(EventArgs e)
		{
			formTrain.Instance?.CloseUI_Async();
		}
    }
}
