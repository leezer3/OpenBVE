using System;
using OpenTK;
using OpenTK.Graphics;
using Vector3 = OpenBveApi.Math.Vector3;

namespace OpenBve
{
    class ObjectViewer : OpenTK.GameWindow
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
            }
        }
        
        private static double ReducedModeEnteringTime = 0;
        
        private static double RotateXSpeed = 0.0;
        private static double RotateYSpeed = 0.0;
        
        private static double MoveXSpeed = 0.0;
        private static double MoveYSpeed = 0.0;
        private static double MoveZSpeed = 0.0;
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            Program.MouseMovement();
            double timeElapsed = CPreciseTimer.GetElapsedTime();
            DateTime time = DateTime.Now;
            Game.SecondsSinceMidnight = (double)(3600 * time.Hour + 60 * time.Minute + time.Second) + 0.001 * (double)time.Millisecond;
            lock (Program.LockObj)
            {
                ObjectManager.UpdateAnimatedWorldObjects(timeElapsed, false);
            }
            if (Program.ReducedMode)
            {
                System.Threading.Thread.Sleep(125);
            }
            else
            {
                System.Threading.Thread.Sleep(1);
            }
            bool updatelight = false;
            bool keep = false;
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
                    RotateXSpeed -= (double)Math.Sign(RotateXSpeed) * d;
                }
            }
            else
            {
                double d = (1.0 + 1.0 - 1.0 / (1.0 + RotateXSpeed * RotateXSpeed)) * timeElapsed;
                double m = 1.0;
                RotateXSpeed += (double)Program.RotateX * d;
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
                double cosa = Math.Cos(RotateXSpeed * timeElapsed);
                double sina = Math.Sin(RotateXSpeed * timeElapsed);
				World.AbsoluteCameraDirection.Rotate(Vector3.Down, cosa, sina);
	            World.AbsoluteCameraUp.Rotate(Vector3.Down, cosa, sina);
	            World.AbsoluteCameraSide.Rotate(Vector3.Down, cosa, sina);
                keep = true;
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
                    RotateYSpeed -= (double)Math.Sign(RotateYSpeed) * d;
                }
            }
            else
            {
                double d = (1.0 + 1.0 - 1.0 / (1.0 + RotateYSpeed * RotateYSpeed)) * timeElapsed;
                double m = 1.0;
                RotateYSpeed += (double)Program.RotateY * d;
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
                double cosa = Math.Cos(RotateYSpeed * timeElapsed);
                double sina = Math.Sin(RotateYSpeed * timeElapsed);
				World.AbsoluteCameraDirection.Rotate(World.AbsoluteCameraSide, cosa, sina);
	            World.AbsoluteCameraUp.Rotate(World.AbsoluteCameraSide, cosa, sina);
                keep = true;
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
                    MoveXSpeed -= (double)Math.Sign(MoveXSpeed) * d;
                }
            }
            else
            {
                double d = (5.0 + 10.0 - 10.0 / (1.0 + MoveXSpeed * MoveXSpeed)) * timeElapsed;
                double m = 25.0;
                MoveXSpeed += (double)Program.MoveX * d;
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
                World.AbsoluteCameraPosition.X += MoveXSpeed * timeElapsed * World.AbsoluteCameraSide.X;
                World.AbsoluteCameraPosition.Y += MoveXSpeed * timeElapsed * World.AbsoluteCameraSide.Y;
                World.AbsoluteCameraPosition.Z += MoveXSpeed * timeElapsed * World.AbsoluteCameraSide.Z;
                keep = true;
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
                    MoveYSpeed -= (double)Math.Sign(MoveYSpeed) * d;
                }
            }
            else
            {
                double d = (5.0 + 10.0 - 10.0 / (1.0 + MoveYSpeed * MoveYSpeed)) * timeElapsed;
                double m = 25.0;
                MoveYSpeed += (double)Program.MoveY * d;
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
                World.AbsoluteCameraPosition.X += MoveYSpeed * timeElapsed * World.AbsoluteCameraUp.X;
                World.AbsoluteCameraPosition.Y += MoveYSpeed * timeElapsed * World.AbsoluteCameraUp.Y;
                World.AbsoluteCameraPosition.Z += MoveYSpeed * timeElapsed * World.AbsoluteCameraUp.Z;
                keep = true;
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
                    MoveZSpeed -= (double)Math.Sign(MoveZSpeed) * d;
                }
            }
            else
            {
                double d = (5.0 + 10.0 - 10.0 / (1.0 + MoveZSpeed * MoveZSpeed)) * timeElapsed;
                double m = 25.0;
                MoveZSpeed += (double)Program.MoveZ * d;
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
                World.AbsoluteCameraPosition.X += MoveZSpeed * timeElapsed * World.AbsoluteCameraDirection.X;
                World.AbsoluteCameraPosition.Y += MoveZSpeed * timeElapsed * World.AbsoluteCameraDirection.Y;
                World.AbsoluteCameraPosition.Z += MoveZSpeed * timeElapsed * World.AbsoluteCameraDirection.Z;
                keep = true;
            }
            // lighting
            if (Program.LightingRelative == -1)
            {
                Program.LightingRelative = (double)Program.LightingTarget;
                updatelight = true;
            }
            if (Program.LightingTarget == 0)
            {
                if (Program.LightingRelative != 0.0)
                {
                    Program.LightingRelative -= 0.5 * timeElapsed;
                    if (Program.LightingRelative < 0.0) Program.LightingRelative = 0.0;
                    updatelight = true;
                    keep = true;
                }
            }
            else
            {
                if (Program.LightingRelative != 1.0)
                {
                    Program.LightingRelative += 0.5 * timeElapsed;
                    if (Program.LightingRelative > 1.0) Program.LightingRelative = 1.0;
                    updatelight = true;
                    keep = true;
                }
            }
            // continue
            if (Program.ReducedMode)
            {
                ReducedModeEnteringTime = 3.0;
            }
            else
            {
                if (keep)
                {
                    ReducedModeEnteringTime =3.0;
                }
                else if (ReducedModeEnteringTime <= 0)
                {
                    Program.ReducedMode = true;
                    World.AbsoluteCameraSide.Y = 0.0;
                    World.AbsoluteCameraSide.Normalize();
                    World.AbsoluteCameraDirection.Normalize();
                    World.AbsoluteCameraUp = Vector3.Cross(World.AbsoluteCameraDirection, World.AbsoluteCameraSide);
                }
                else
                {
                    ReducedModeEnteringTime -= timeElapsed;
                }
            }
            if (updatelight)
            {
               Renderer.OptionAmbientColor.R = (byte)Math.Round(32.0 + 128.0 * Program.LightingRelative * (2.0 - Program.LightingRelative));
               Renderer.OptionAmbientColor.G = (byte)Math.Round(32.0 + 128.0 * 0.5 * (Program.LightingRelative + Program.LightingRelative * (2.0 - Program.LightingRelative)));
                Renderer.OptionAmbientColor.B = (byte)Math.Round(32.0 + 128.0 * Program.LightingRelative);
                Renderer.OptionDiffuseColor.R = (byte)Math.Round(32.0 + 128.0 * Program.LightingRelative);
                Renderer.OptionDiffuseColor.G = (byte)Math.Round(32.0 + 128.0 * Program.LightingRelative);
                Renderer.OptionDiffuseColor.B = (byte)Math.Round(32.0 + 128.0 * Math.Sqrt(Program.LightingRelative));
                Renderer.InitializeLighting();
            }
            Renderer.RenderScene();
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
            KeyDown += Program.KeyDown;
            KeyUp += Program.KeyUp;
            MouseDown += Program.MouseEvent;
            MouseUp += Program.MouseEvent;
			MouseWheel += Program.MouseWheelEvent;
	        FileDrop += Program.DragFile;
            Program.ResetCamera();
            Renderer.Initialize();
            Renderer.InitializeLighting();
            //SwapBuffers();
            //Fonts.Initialize();
            Program.UpdateViewport();
            // command line arguments
            // if (commandLineArgs != null)
            // {
            //     for (int i = 0; i < commandLineArgs.Length; i++)
            //     {
            //         if (!Program.SkipArgs[i] && System.IO.File.Exists(commandLineArgs[i]))
            //         {
            //             try
            //             {
            //                 UnifiedObject o = ObjectManager.LoadObject(commandLineArgs[i],
            //                     System.Text.Encoding.UTF8, false, false, false,0,0,0);
            //                 ObjectManager.CreateObject(o, new Vector3(0.0, 0.0, 0.0),
            //                     new Transformation(0.0, 0.0, 0.0), new Transformation(0.0, 0.0, 0.0), true,
            //                     0.0, 0.0, 25.0, 0.0);
            //             }
            //             catch (Exception ex)
            //             {
            //                 Interface.AddMessage(MessageType.Critical, false, "Unhandled error (" + ex.Message + ") encountered while processing the file " + commandLineArgs[i] + ".");
            //             }
            //             Array.Resize<string>(ref Program.Files, Program.Files.Length + 1);
            //             Program.Files[Program.Files.Length - 1] = commandLineArgs[i];
            //         }
            //     }
            // }
            ObjectManager.InitializeVisibility();
            ObjectManager.FinishCreatingObjects();
            ObjectManager.UpdateVisibility(0.0, true);
            ObjectManager.UpdateAnimatedWorldObjects(0.01, true);
        }
    }
}
