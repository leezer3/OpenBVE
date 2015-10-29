
namespace OpenBve {
	internal static class Screen {
		
		// --- members ---
		
		/// <summary>Whether the screen is initialized.</summary>
		private static bool Initialized = false;
		
		/// <summary>The fixed width of the screen.</summary>
		internal static int Width = 0;
		
		/// <summary>The fixed height of the screen.</summary>
		internal static int Height = 0;
		
		/// <summary>Whether the screen is set to fullscreen mode.</summary>
		internal static bool Fullscreen = false;
		
		
		// --- functions ---
		
		/// <summary>Initializes the screen. A call to SDL_Init must have been made before calling this function. A call to Deinitialize must be made when terminating the program.</summary>
		/// <returns>Whether initializing the screen was successful.</returns>
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
            /*
			Fullscreen = !Fullscreen;
			// begin HACK //
			Renderer.ClearDisplayLists();
			if (World.MouseGrabEnabled) {
				Sdl.SDL_WM_GrabInput(Sdl.SDL_GRAB_OFF);
			}
			GL.Disable(EnableCap.Fog);
			GL.Disable(EnableCap.Lighting);
			Renderer.LightingEnabled = false;
			Textures.UnloadAllTextures();
			if (Fullscreen) {
				Sdl.SDL_SetVideoMode(Interface.CurrentOptions.FullscreenWidth, Interface.CurrentOptions.FullscreenHeight, Interface.CurrentOptions.FullscreenBits, Sdl.SDL_OPENGL | Sdl.SDL_DOUBLEBUF | Sdl.SDL_FULLSCREEN);
				Width = Interface.CurrentOptions.FullscreenWidth;
				Height = Interface.CurrentOptions.FullscreenHeight;
			} else {
				Sdl.SDL_SetVideoMode(Interface.CurrentOptions.WindowWidth, Interface.CurrentOptions.WindowHeight, 32, Sdl.SDL_OPENGL | Sdl.SDL_DOUBLEBUF);
				Width = Interface.CurrentOptions.WindowWidth;
				Height = Interface.CurrentOptions.WindowHeight;
			}
			Renderer.InitializeLighting();
			MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
			MainLoop.InitializeMotionBlur();
			Timetable.CreateTimetable();
			Timetable.UpdateCustomTimetable(null, null);
			if (World.MouseGrabEnabled) {
				Sdl.SDL_WM_GrabInput(Sdl.SDL_GRAB_ON);
			}
			World.MouseGrabTarget = new World.Vector2D(0.0, 0.0);
			World.MouseGrabIgnoreOnce = true;
			World.InitializeCameraRestriction();
			if (Renderer.OptionBackfaceCulling)
			{
			    GL.Enable(EnableCap.CullFace);
			} else {
				GL.Disable(EnableCap.CullFace);
			}
			Renderer.ReAddObjects();
			// end HACK //
             */
		}
             
		
	}
}