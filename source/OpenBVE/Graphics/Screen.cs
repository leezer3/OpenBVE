using System;
using System.Collections.Generic;
using System.Windows.Forms;
using LibRender2;
using LibRender2.Viewports;
using OpenBveApi.Hosts;
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
			Program.Renderer.GraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, Interface.CurrentOptions.AntiAliasingLevel);
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
							if (Interface.CurrentOptions.IsUseNewRenderer && (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4 || Interface.CurrentOptions.ForceForwardsCompatibleContext))
							{
								/*
								 * OS-X is a fickle beast
								 * In order to get a functioning GL3 context, we appear to need to be running as 64-bit & explicitly specify the forwards compatible flag
								 */
								Program.Renderer.GameWindow = new OpenBVEGame(currentResolution.Width, currentResolution.Height, Program.Renderer.GraphicsMode,
									GameWindowFlags.Default, GraphicsContextFlags.ForwardCompatible)
								{
									Visible = true,
									WindowState = WindowState.Fullscreen,
								};
							}
							else
							{
								Program.Renderer.GameWindow = new OpenBVEGame(currentResolution.Width, currentResolution.Height, Program.Renderer.GraphicsMode,
									GameWindowFlags.Default)
								{
									Visible = true,
									WindowState = WindowState.Fullscreen,
								};	
							}
							
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
					Program.ShowMessageBox(
						"The graphics card driver reported that the selected resolution was not supported:" + Environment.NewLine +
						Interface.CurrentOptions.FullscreenWidth + @" x " + Interface.CurrentOptions.FullscreenHeight + " " +
						Interface.CurrentOptions.FullscreenBits + "bit color" + Environment.NewLine +
						"Please check your resolution settings.", Application.ProductName);
					Program.RestartArguments = " ";
					return;
				}
			}
			else
			{
				try
				{
					if (Interface.CurrentOptions.IsUseNewRenderer && (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4 || Interface.CurrentOptions.ForceForwardsCompatibleContext))
					{
						/*
						 * OS-X is a fickle beast
						 * In order to get a functioning GL3 context, we appear to need to be running as 64-bit & explicitly specify the forwards compatible flag
						 */
						Program.Renderer.GameWindow = new OpenBVEGame(Interface.CurrentOptions.WindowWidth,
							Interface.CurrentOptions.WindowHeight, Program.Renderer.GraphicsMode, GameWindowFlags.Default, GraphicsContextFlags.ForwardCompatible)
						{
							Visible = true
						};
					}
					else
					{
						Program.Renderer.GameWindow = new OpenBVEGame(Interface.CurrentOptions.WindowWidth,
							Interface.CurrentOptions.WindowHeight, Program.Renderer.GraphicsMode, GameWindowFlags.Default)
						{
							Visible = true
						};
					}
					
				}
				catch
				{
					//Windowed mode failed to launch
					Program.ShowMessageBox("An error occured whilst trying to launch in windowed mode at resolution:" + Environment.NewLine +
									Interface.CurrentOptions.WindowWidth + @" x " + Interface.CurrentOptions.WindowHeight + @" " +
									Environment.NewLine +
									"Please check your resolution settings.", Application.ProductName);
					Program.RestartArguments = " ";
					return;
				}
			}
			if (Program.Renderer.GameWindow == null)
			{
				//We should never really get an unspecified error here, but it's good manners to handle all cases
				Program.ShowMessageBox("An unspecified error occured whilst attempting to launch the graphics subsystem.",
					Application.ProductName);
				Program.RestartArguments = " ";
				return;
			}

			// BUG: Currently disabled- https://github.com/leezer3/OpenBVE/issues/957
			//Program.currentGameWindow.TargetUpdateFrequency = Interface.CurrentOptions.FPSLimit;
			//Program.currentGameWindow.TargetRenderFrequency = Interface.CurrentOptions.FPSLimit;
			Program.Renderer.GameWindow.VSync = Interface.CurrentOptions.VerticalSynchronization ? VSyncMode.On : VSyncMode.Off;
			
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
			Program.Renderer.Fog.Enabled = false;
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
			            Program.Renderer.GameWindow.Width = resolutions[i].Width;
			            Program.Renderer.GameWindow.Height = resolutions[i].Height;
			            Program.Renderer.Screen.Width = Interface.CurrentOptions.FullscreenWidth;
			            Program.Renderer.Screen.Height = Interface.CurrentOptions.FullscreenHeight;
                        Program.Renderer.GameWindow.WindowState = WindowState.Fullscreen;
				        break;
			        }
			    }
			    System.Threading.Thread.Sleep(20);
			    if (Program.Renderer.GameWindow.WindowState != WindowState.Fullscreen)
			    {
                    Program.ShowMessageBox(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","fullscreen_switch1"}) + Environment.NewLine +
                        Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","fullscreen_switch2"}), Application.ProductName);
					Program.Renderer.SetWindowState(WindowState.Fullscreen);
				}
			}
			else
			{
                DisplayDevice.Default.RestoreResolution();
				Program.Renderer.SetWindowState(WindowState.Normal);
				Program.Renderer.GameWindow.Width = Interface.CurrentOptions.WindowWidth;
                Program.Renderer.GameWindow.Height = Interface.CurrentOptions.WindowHeight;

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
