using System;
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
	        Game.InfoFrameRate = RenderFrequency;
            GL.ClearColor(0.75f, 0.75f, 0.75f, 1.0f);
            //Do not do anything whilst loading
            if (currentlyLoading)
            {
                System.Threading.Thread.Sleep(10);
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
            SoundManager.Update(TimeElapsed);
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
            Program.ResetCamera();
            World.BackgroundImageDistance = 600.0;
            World.ForwardViewingDistance = 600.0;
            World.BackwardViewingDistance = 0.0;
            World.ExtraViewingDistance = 50.0;

            Renderer.Initialize();
            Renderer.InitializeLighting();
            SoundManager.Initialize();
            Fonts.Initialize();
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
    }
}
