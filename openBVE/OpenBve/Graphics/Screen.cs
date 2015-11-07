using System.Collections.Generic;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace OpenBve {
	internal static class Screen {
		
		// --- members ---
		
		/// <summary>Whether the screen is initialized.</summary>
		private static bool Initialized = false;
		
		/// <summary>Stores the current width of the screen.</summary>
		internal static int Width = 0;
		
		/// <summary>Stores the current height of the screen.</summary>
		internal static int Height = 0;
		
		/// <summary>Whether the screen is set to fullscreen mode.</summary>
		internal static bool Fullscreen = false;
		
		
		// --- functions ---
		
		/// <summary>Initializes the default values of the screen.</summary>
		/// <returns>True (To be removed).</returns>
		internal static bool Initialize()
		{
                // --- video mode ---
            Width = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenWidth : Interface.CurrentOptions.WindowWidth;
            Height = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenHeight : Interface.CurrentOptions.WindowHeight;
            Fullscreen = Interface.CurrentOptions.FullscreenMode;
		    return true;

		}
		
		/// <summary>Deinitializes the screen.</summary>
		internal static void Deinitialize() {
			if (Initialized) {
				Initialized = false;
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
			Textures.UnloadAllTextures();
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
			        }
			    }
			    if (Program.currentGameWindow.WindowState != WindowState.Fullscreen)
			    {
                    MessageBox.Show("An error occured whilst attempting to switch to fullscreen mode." + System.Environment.NewLine +
                        "Please check your fullscreen resolution settings.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			        Fullscreen = false;
			    }
			} else {
                OpenTK.DisplayDevice.Default.RestoreResolution();
                Program.currentGameWindow.Width = Interface.CurrentOptions.WindowWidth;
                Program.currentGameWindow.Height = Interface.CurrentOptions.WindowHeight;
			    Program.currentGameWindow.WindowState = WindowState.Normal;
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
             
		}
             
		
	}
}