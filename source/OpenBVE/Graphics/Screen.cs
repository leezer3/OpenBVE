using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;


namespace OpenBve
{
	internal static class Screen
	{
		
		/// <summary>Stores the current width of the screen.</summary>
		internal static int Width = 0;
		
		/// <summary>Stores the current height of the screen.</summary>
		internal static int Height = 0;
		
		/// <summary>Whether the screen is set to fullscreen mode.</summary>
		internal static bool Fullscreen = false;

		/// <summary>Whether the window is currently minimized</summary>
		internal static bool Minimized = false;
		
		// --- functions ---
		
		/// <summary>Initializes the default values of the screen.</summary>
		internal static void Initialize()
		{
            //Initialize the values used by the renderer
            Width = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenWidth : Interface.CurrentOptions.WindowWidth;
            Height = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenHeight : Interface.CurrentOptions.WindowHeight;
            Fullscreen = Interface.CurrentOptions.FullscreenMode;
			//Set a new graphics mode, using 8 bits for R,G,B,A & a 8 bit stencil buffer (Currently unused)
			GraphicsMode currentGraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, Interface.CurrentOptions.AntiAliasingLevel);
			if (Interface.CurrentOptions.FullscreenMode)
			{
				IList<DisplayResolution> resolutions = OpenTK.DisplayDevice.Default.AvailableResolutions;
				bool resolutionFound = false;
				foreach (DisplayResolution currentResolution in resolutions)
				{
					//Test resolution
					if (currentResolution.Width == Interface.CurrentOptions.FullscreenWidth &&
					    currentResolution.Height == Interface.CurrentOptions.FullscreenHeight &&
					    currentResolution.BitsPerPixel == Interface.CurrentOptions.FullscreenBits)
					{
						try
						{
							OpenTK.DisplayDevice.Default.ChangeResolution(currentResolution);
							Program.currentGameWindow = new OpenBVEGame(currentResolution.Width, currentResolution.Height, currentGraphicsMode,
								GameWindowFlags.Default)
							{
								Visible = true,
								WindowState = WindowState.Fullscreen,
							};
							resolutionFound = true;
							break;
						}
						catch
						{
							//A candidate resolution was found, but we failed to switch to it
							//Carry on enumerating through the rest of the resolutions, in case one with
							//a different refresh rate but identical properties works
							resolutionFound = false;
						}
					}
				}
				if (resolutionFound == false)
				{
					//Our resolution was not found at all
					MessageBox.Show(
						"The graphics card driver reported that the selected resolution was not supported:" + Environment.NewLine +
						Interface.CurrentOptions.FullscreenWidth + " x " + Interface.CurrentOptions.FullscreenHeight + " " +
						Interface.CurrentOptions.FullscreenBits + "bit color" + Environment.NewLine +
						"Please check your resolution settings.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					Program.RestartArguments = " ";
					return;
				}
			}
			else
			{

				try
				{
					Program.currentGameWindow = new OpenBVEGame(Interface.CurrentOptions.WindowWidth,
						Interface.CurrentOptions.WindowHeight, currentGraphicsMode, GameWindowFlags.Default)
					{
						Visible = true
					};
				}
				catch
				{
					//Windowed mode failed to launch
					MessageBox.Show("An error occured whilst tring to launch in windowed mode at resolution:" + Environment.NewLine +
									Interface.CurrentOptions.WindowWidth + " x " + Interface.CurrentOptions.WindowHeight + " " +
									Environment.NewLine +
									"Please check your resolution settings.", Application.ProductName, MessageBoxButtons.OK,
						MessageBoxIcon.Hand);
					Program.RestartArguments = " ";
					return;
				}
			}
			if (Program.currentGameWindow == null)
			{
				//We should never really get an unspecified error here, but it's good manners to handle all cases
				MessageBox.Show("An unspecified error occured whilst attempting to launch the graphics subsystem.",
					Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				Program.RestartArguments = " ";
				return;
			}

			Program.currentGameWindow.TargetUpdateFrequency = 0;
			Program.currentGameWindow.TargetRenderFrequency = 0;
			Program.currentGameWindow.VSync = Interface.CurrentOptions.VerticalSynchronization ? VSyncMode.On : VSyncMode.Off;
			
		}
		
        /// <summary>Resizes the OpenGL viewport if the window is resized</summary>
	    internal static void WindowResize(int newWidth, int newHeight)
        {
            Width = newWidth;
            Height = newHeight;
            if (Loading.Complete)
            {
                Renderer.UpdateViewport(Renderer.ViewPortChangeMode.NoChange);
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
            }
			else
			{
                GL.Viewport(0, 0, Width, Height);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(0.0, (double)Width, (double)Height, 0.0, -1.0, 1.0);
            }
	    }

		/// <summary>Changes to or from fullscreen mode.</summary>
		internal static void ToggleFullscreen()
		{
            
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
                    MessageBox.Show(Translations.GetInterfaceString("errors_fullscreen_switch1") + System.Environment.NewLine +
                        Translations.GetInterfaceString("errors_fullscreen_switch2"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			        Fullscreen = false;
			    }
			}
			else
			{
                OpenTK.DisplayDevice.Default.RestoreResolution();
                Program.currentGameWindow.WindowState = WindowState.Normal;
                Program.currentGameWindow.Width = Interface.CurrentOptions.WindowWidth;
                Program.currentGameWindow.Height = Interface.CurrentOptions.WindowHeight;
			    
                Screen.Width = Interface.CurrentOptions.WindowWidth;
                Screen.Height = Interface.CurrentOptions.WindowHeight;
			}
			Renderer.InitializeLighting();
			Renderer.UpdateViewport(Renderer.ViewPortChangeMode.NoChange);
			Renderer.InitializeMotionBlur();
			Timetable.CreateTimetable();
			Timetable.UpdateCustomTimetable(null, null);
			
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
			// end HACK //

            //Reset the camera when switching between fullscreen and windowed mode
            //Otherwise, if the aspect ratio changes distortion will occur until the view is changed or the camera reset
            if (World.CameraMode == CameraViewMode.Interior | World.CameraMode == CameraViewMode.InteriorLookAhead)
            {
                World.CameraCurrentAlignment.Position = new OpenBveApi.Math.Vector3(0.0, 0.0, 0.0);
            }
            World.CameraCurrentAlignment.Yaw = 0.0;
            World.CameraCurrentAlignment.Pitch = 0.0;
            World.CameraCurrentAlignment.Roll = 0.0;
		}
        
		
	}
}
