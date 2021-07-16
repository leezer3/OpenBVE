using System;
using System.Collections.Generic;
using System.Windows.Forms;
using LibRender2;
using LibRender2.Viewports;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Runtime;

namespace OpenBve
{
	internal static class Screen
	{
		/// <summary>Initializes the default values of the screen.</summary>
		internal static void Initialize()
		{
            //Initialize the values used by the renderer
            Program.Renderer.Screen.Width = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenWidth : Interface.CurrentOptions.WindowWidth;
            Program.Renderer.Screen.Height = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenHeight : Interface.CurrentOptions.WindowHeight;
            Program.Renderer.Screen.Fullscreen = Interface.CurrentOptions.FullscreenMode;
			//Set a new graphics mode, using 8 bits for R,G,B,A & a 8 bit stencil buffer (Currently unused)
			GraphicsMode currentGraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, Interface.CurrentOptions.AntiAliasingLevel);
			if (Interface.CurrentOptions.FullscreenMode)
			{
				IList<DisplayResolution> resolutions = DisplayDevice.Default.AvailableResolutions;
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
							DisplayDevice.Default.ChangeResolution(currentResolution);
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
	        Program.Renderer.Screen.Width = newWidth;
	        Program.Renderer.Screen.Height = newHeight;
            if (Loading.Complete)
            {
	            Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
                World.InitializeCameraRestriction();
                if (Program.Renderer.OptionBackFaceCulling)
                {
                    GL.Enable(EnableCap.CullFace);
                }
                else
                {
                    GL.Disable(EnableCap.CullFace);
                }
            }
			else
            {
	            GL.Viewport(0, 0, Program.Renderer.Screen.Width, Program.Renderer.Screen.Height);
	            Program.Renderer.PopMatrix(MatrixMode.Modelview);
	            Matrix4D.CreateOrthographicOffCenter(0.0f, Program.Renderer.Screen.Width, Program.Renderer.Screen.Height, 0.0f, -1.0f, 1.0f, out Program.Renderer.CurrentProjectionMatrix);
	            Program.Renderer.PushMatrix(MatrixMode.Modelview);
	            Program.Renderer.CurrentViewMatrix = Matrix4D.Identity;
				Program.Renderer.ResetOpenGlState();
            }
	    }

		/// <summary>Changes to or from fullscreen mode.</summary>
		internal static void ToggleFullscreen()
		{
			Program.Renderer.Screen.Fullscreen = !Program.Renderer.Screen.Fullscreen;

			// begin HACK //
			Program.Renderer.OptionFog = false;
			Program.Renderer.OptionLighting = false;

			if (Program.Renderer.Screen.Fullscreen)
			{
				IList<DisplayResolution> resolutions = DisplayDevice.Default.AvailableResolutions;
                
			    for (int i = 0; i < resolutions.Count; i++)
			    {
			        //Test each resolution
			        if (resolutions[i].Width == Interface.CurrentOptions.FullscreenWidth &&
			            resolutions[i].Height == Interface.CurrentOptions.FullscreenHeight &&
			            resolutions[i].BitsPerPixel == Interface.CurrentOptions.FullscreenBits)
			        {
			            DisplayDevice.Default.ChangeResolution(resolutions[i]);
			            Program.currentGameWindow.Width = resolutions[i].Width;
			            Program.currentGameWindow.Height = resolutions[i].Height;
			            Program.Renderer.Screen.Width = Interface.CurrentOptions.FullscreenWidth;
			            Program.Renderer.Screen.Height = Interface.CurrentOptions.FullscreenHeight;
                        Program.currentGameWindow.WindowState = WindowState.Fullscreen;
				        break;
			        }
			    }
			    System.Threading.Thread.Sleep(20);
			    if (Program.currentGameWindow.WindowState != WindowState.Fullscreen)
			    {
                    MessageBox.Show(Translations.GetInterfaceString("errors_fullscreen_switch1") + System.Environment.NewLine +
                        Translations.GetInterfaceString("errors_fullscreen_switch2"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    Program.Renderer.Screen.Fullscreen = false;
			    }
			}
			else
			{
                DisplayDevice.Default.RestoreResolution();
                Program.currentGameWindow.WindowState = WindowState.Normal;
                Program.currentGameWindow.Width = Interface.CurrentOptions.WindowWidth;
                Program.currentGameWindow.Height = Interface.CurrentOptions.WindowHeight;

                Program.Renderer.Screen.Width = Interface.CurrentOptions.WindowWidth;
                Program.Renderer.Screen.Height = Interface.CurrentOptions.WindowHeight;
			}
			Program.Renderer.Lighting.Initialize();
			Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
			Program.Renderer.MotionBlur.Initialize(Interface.CurrentOptions.MotionBlur);
			lock (BaseRenderer.GdiPlusLock)
			{
				Timetable.CreateTimetable();
			}
			Timetable.UpdateCustomTimetable(null, null);
			
			World.InitializeCameraRestriction();
			if (Program.Renderer.OptionBackFaceCulling)
			{
			    GL.Enable(EnableCap.CullFace);
			}
			else
			{
				GL.Disable(EnableCap.CullFace);
			}
			// end HACK //

            //Reset the camera when switching between fullscreen and windowed mode
            //Otherwise, if the aspect ratio changes distortion will occur until the view is changed or the camera reset
            if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead)
            {
	            Program.Renderer.Camera.Alignment.Position = OpenBveApi.Math.Vector3.Zero;
            }
            Program.Renderer.Camera.Alignment.Yaw = 0.0;
            Program.Renderer.Camera.Alignment.Pitch = 0.0;
            Program.Renderer.Camera.Alignment.Roll = 0.0;
		}
        
		
	}
}
