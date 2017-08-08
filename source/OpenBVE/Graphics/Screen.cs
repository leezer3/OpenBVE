using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace OpenBve {
	internal static class Screen {
		
		/// <summary>Stores the current width of the screen.</summary>
		internal static int Width = 0;
		
		/// <summary>Stores the current height of the screen.</summary>
		internal static int Height = 0;
		
		/// <summary>Whether the screen is set to fullscreen mode.</summary>
		internal static bool Fullscreen = false;
		
		
		// --- functions ---
		
		/// <summary>Initializes the default values of the screen.</summary>
		internal static void Initialize()
		{
            // --- video mode ---
            Width = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenWidth : Interface.CurrentOptions.WindowWidth;
            Height = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenHeight : Interface.CurrentOptions.WindowHeight;
            Fullscreen = Interface.CurrentOptions.FullscreenMode;
		}
		
        /// <summary>Resizes the OpenGL viewport if the window is resized</summary>
	    internal static void WindowResize(int newWidth, int newHeight)
        {
            Width = newWidth;
            Height = newHeight;
            if (Loading.Complete)
            {
                MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
                World.InitializeCameraRestriction();
                if (Renderer.OptionBackfaceCulling)
                {
                    GL.Enable(EnableCap.CullFace);
                }
                else
                {
                    GL.Disable(EnableCap.CullFace);
                }
                Renderer.ReAddObjects();
            } else {
                GL.Viewport(0, 0, Width, Height);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0.0, (double)Width, (double)Height, 0.0, -1.0, 1.0);
            }
	    }

		/// <summary>Changes to or from fullscreen mode.</summary>
		internal static void ToggleFullscreen() {
            
			Fullscreen = !Fullscreen;
			// begin HACK //
			Renderer.ClearDisplayLists();
			
			GL.Disable(EnableCap.Fog);
			GL.Disable(EnableCap.Lighting);
			Renderer.LightingEnabled = false;
			if (Fullscreen)
			{
                
                IList<DisplayResolution> resolutions = OpenTK.DisplayDevice.Default.AvailableResolutions;
                
			    for (int i = 0; i < resolutions.Count; i++)
			    {
			        //Test each resolution
			        if (resolutions[i].Width == Interface.CurrentOptions.FullscreenWidth &&
			            resolutions[i].Height == Interface.CurrentOptions.FullscreenHeight &&
			            resolutions[i].BitsPerPixel == Interface.CurrentOptions.FullscreenBits)
			        {
			            OpenTK.DisplayDevice.Default.ChangeResolution(resolutions[i]);
			            Program.currentGameWindow.Width = resolutions[i].Width;
			            Program.currentGameWindow.Height = resolutions[i].Height;
                        Screen.Width = Interface.CurrentOptions.FullscreenWidth;
                        Screen.Height = Interface.CurrentOptions.FullscreenHeight;
                        Program.currentGameWindow.WindowState = WindowState.Fullscreen;
				        break;
			        }
			    }
			    System.Threading.Thread.Sleep(20);
			    if (Program.currentGameWindow.WindowState != WindowState.Fullscreen)
			    {
                    MessageBox.Show(Interface.GetInterfaceString("errors_fullscreen_switch1") + System.Environment.NewLine +
                        Interface.GetInterfaceString("errors_fullscreen_switch2"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			        Fullscreen = false;
			    }
			} else {
                OpenTK.DisplayDevice.Default.RestoreResolution();
                Program.currentGameWindow.WindowState = WindowState.Normal;
                Program.currentGameWindow.Width = Interface.CurrentOptions.WindowWidth;
                Program.currentGameWindow.Height = Interface.CurrentOptions.WindowHeight;
			    
                Screen.Width = Interface.CurrentOptions.WindowWidth;
                Screen.Height = Interface.CurrentOptions.WindowHeight;
			}
			Renderer.InitializeLighting();
			MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
			MainLoop.InitializeMotionBlur();
			Timetable.CreateTimetable();
			Timetable.UpdateCustomTimetable(null, null);
			
			World.InitializeCameraRestriction();
			if (Renderer.OptionBackfaceCulling)
			{
			    GL.Enable(EnableCap.CullFace);
			} else {
				GL.Disable(EnableCap.CullFace);
			}
			Renderer.ReAddObjects();
			// end HACK //

            //Reset the camera when switching between fullscreen and windowed mode
            //Otherwise, if the aspect ratio changes distortion will occur until the view is changed or the camera reset
            if (World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead)
            {
                World.CameraCurrentAlignment.Position = new OpenBveApi.Math.Vector3(0.0, 0.0,
                    0.0);
            }
            World.CameraCurrentAlignment.Yaw = 0.0;
            World.CameraCurrentAlignment.Pitch = 0.0;
            World.CameraCurrentAlignment.Roll = 0.0;
            if (World.CameraMode == World.CameraViewMode.Track)
            {
                TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower,TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition, true,false);
            }  
		}
        
		
	}
}