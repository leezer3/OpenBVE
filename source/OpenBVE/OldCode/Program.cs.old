--- DO NOT COMPILE ---

	using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Tao.OpenGl;
using Tao.Sdl;

namespace OpenBve {
	public static partial class Program {

		// system
		internal static string RestartArguments = null;
		internal enum Platform { Windows, Linux, Mac }
		internal static Platform CurrentPlatform = Platform.Windows;
		internal static bool CurrentlyRunOnMono = false;
		internal static FileSystem FileSystem = null;
		internal enum ProgramType { OpenBve, RouteViewer, ObjectViewer, Other };
		internal const ProgramType CurrentProgramType = ProgramType.OpenBve;
		private static bool SdlWindowCreated = false;
		internal static Host CurrentHost = new Host();

		// main
		[STAThread]
		private static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			// platform and mono
			int p = (int)Environment.OSVersion.Platform;
			if (p == 4 | p == 128) {
				// general Unix
				CurrentPlatform = Platform.Linux;
			} else if (p == 6) {
				// Mac
				CurrentPlatform = Platform.Mac;
			} else {
				// non-Unix
				CurrentPlatform = Platform.Windows;
			}
			CurrentlyRunOnMono = Type.GetType("Mono.Runtime") != null;
			// file system
			try {
				FileSystem = FileSystem.FromCommandLineArgs(args);
			} catch (Exception ex) {
				MessageBox.Show("The file system configuration could not be accessed or is invalid due to the following reason:\n\n" + ex.Message, "openBVE", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			FileSystem.CreateFileSystem();
			// start
			#if DEBUG
			try {
				#endif
				Start(args);
				#if DEBUG
			} catch (Exception ex) {
				bool shown = false;
				for (int i = 0; i < TrainManager.Trains.Length; i++) {
					if (TrainManager.Trains[i] != null && TrainManager.Trains[i].Plugin != null) {
						if (TrainManager.Trains[i].Plugin.LastException != null) {
							string text = GetExceptionText(TrainManager.Trains[i].Plugin.LastException, 5);
							MessageBox.Show("The train plugin " + TrainManager.Trains[i].Plugin.PluginTitle + " caused the following exception:\n\n" + text, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
							shown = false;
						}
					}
				}
				if (!shown) {
					string text = GetExceptionText(ex, 5);
					MessageBox.Show(text, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			#endif
			// deinitialize
			if(SdlWindowCreated & Interface.CurrentOptions.FullscreenMode) {
				Sdl.SDL_SetVideoMode(Interface.CurrentOptions.WindowWidth, Interface.CurrentOptions.WindowHeight, 32, Sdl.SDL_OPENGL | Sdl.SDL_DOUBLEBUF);
			}
			Renderer.Deinitialize();
			Textures.Deinitialize();
			Plugins.UnloadPlugins();
			for (int i = 0; i < TrainManager.Trains.Length; i++) {
				if (TrainManager.Trains[i] != null && TrainManager.Trains[i].Plugin != null) {
					PluginManager.UnloadPlugin(TrainManager.Trains[i]);
				}
			}
			SoundManager.Deinitialize();
			// close sdl
			for (int i = 0; i < Interface.CurrentJoysticks.Length; i++) {
				Sdl.SDL_JoystickClose(Interface.CurrentJoysticks[i].SdlHandle);
				Interface.CurrentJoysticks[i].SdlHandle = IntPtr.Zero;
			}
			Sdl.SDL_Quit();
			// restart
			if (RestartArguments != null) {
				string arguments;
				if (FileSystem.RestartArguments.Length != 0 & RestartArguments.Length != 0) {
					arguments = FileSystem.RestartArguments + " " + RestartArguments;
				} else {
					arguments = FileSystem.RestartArguments + RestartArguments;
				}
				try {
					System.Diagnostics.Process.Start(FileSystem.RestartProcess, arguments);
				} catch (Exception ex) {
					MessageBox.Show(ex.Message + "\n\nProcess = " + FileSystem.RestartProcess + "\nArguments = " + arguments, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		// get exception text
		/// <summary>Returns a textual representation of an exception and its inner exceptions.</summary>
		/// <param name="ex">The exception to serialize.</param>
		/// <param name="Levels">The amount of inner exceptions to include.</param>
		private static string GetExceptionText(Exception ex, int Levels) {
			if (Levels > 0 & ex.InnerException != null) {
				return ex.Message + "\n\n" + GetExceptionText(ex.InnerException, Levels - 1);
			} else {
				return ex.Message;
			}
		}

		// start
		private static void Start(string[] Args) {
			// initialize sdl video
			if (Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO) != 0) {
				MessageBox.Show("SDL failed to initialize the video subsystem.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			if (Sdl.SDL_Init(Sdl.SDL_INIT_JOYSTICK) != 0) {
				MessageBox.Show("SDL failed to initialize the joystick subsystem.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			// initialize sdl joysticks
			{
				int n = Sdl.SDL_NumJoysticks();
				Interface.CurrentJoysticks = new Interface.Joystick[n];
				for (int i = 0; i < n; i++) {
					Interface.CurrentJoysticks[i].SdlHandle = Sdl.SDL_JoystickOpen(i);
					if (CurrentPlatform == Platform.Windows) {
						string s = Sdl.SDL_JoystickName(i);
						/* string returned is ascii packed in utf-16 (2 chars per codepoint) */
						System.Text.StringBuilder t = new System.Text.StringBuilder(s.Length << 1);
						for (int k = 0; k < s.Length; k++) {
							int a = (int)s[k];
							t.Append(char.ConvertFromUtf32(a & 0xFF) + char.ConvertFromUtf32(a >> 8));
						}
						Interface.CurrentJoysticks[i].Name = t.ToString();
					} else {
						Interface.CurrentJoysticks[i].Name = Sdl.SDL_JoystickName(i);
					}
				}
			}
			// load options and controls
			Interface.LoadOptions();
			Interface.LoadControls(null, out Interface.CurrentControls);
			{
				string f = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Controls"), "Default keyboard assignment.controls");
				Interface.Control[] c;
				Interface.LoadControls(f, out c);
				Interface.AddControls(ref Interface.CurrentControls, c);
			}
			// command line arguments
			formMain.MainDialogResult Result = new formMain.MainDialogResult();
			for (int i = 0; i < Args.Length; i++) {
				if (Args[i].StartsWith("/route=", StringComparison.OrdinalIgnoreCase)) {
					Result.RouteFile = Args[i].Substring(7);
					Result.RouteEncoding = System.Text.Encoding.UTF8;
					for (int j = 0; j < Interface.CurrentOptions.RouteEncodings.Length; j++) {
						if (string.Compare(Interface.CurrentOptions.RouteEncodings[j].Value, Result.RouteFile, StringComparison.InvariantCultureIgnoreCase) == 0) {
							Result.RouteEncoding = System.Text.Encoding.GetEncoding(Interface.CurrentOptions.RouteEncodings[j].Codepage);
							break;
						}
					}
				} else if (Args[i].StartsWith("/train=", StringComparison.OrdinalIgnoreCase)) {
					Result.TrainFolder = Args[i].Substring(7);
					Result.TrainEncoding = System.Text.Encoding.UTF8;
					for (int j = 0; j < Interface.CurrentOptions.TrainEncodings.Length; j++) {
						if (string.Compare(Interface.CurrentOptions.TrainEncodings[j].Value, Result.TrainFolder, StringComparison.InvariantCultureIgnoreCase) == 0) {
							Result.TrainEncoding = System.Text.Encoding.GetEncoding(Interface.CurrentOptions.TrainEncodings[j].Codepage);
							break;
						}
					}
				}
			}
			// train provided
			if (Result.TrainFolder != null) {
				if (System.IO.Directory.Exists(Result.TrainFolder)) {
					string File = OpenBveApi.Path.CombineFile(Result.TrainFolder, "train.dat");
					if (System.IO.File.Exists(File)) {
						Result.TrainEncoding = System.Text.Encoding.UTF8;
						for (int j = 0; j < Interface.CurrentOptions.TrainEncodings.Length; j++) {
							if (string.Compare(Interface.CurrentOptions.TrainEncodings[j].Value, Result.TrainFolder, StringComparison.InvariantCultureIgnoreCase) == 0) {
								Result.TrainEncoding = System.Text.Encoding.GetEncoding(Interface.CurrentOptions.TrainEncodings[j].Codepage);
							}
						}
					} else {
						Result.TrainFolder = null;
					}
				} else {
					Result.TrainFolder = null;
				}
			}
			// route provided
			if (Result.RouteFile != null) {
				if (!System.IO.File.Exists(Result.RouteFile)) {
					Result.RouteFile = null;
				}
			}
			// route provided but no train
			if (Result.RouteFile != null & Result.TrainFolder == null) {
				bool IsRW = string.Equals(System.IO.Path.GetExtension(Result.RouteFile), ".rw", StringComparison.OrdinalIgnoreCase);
				CsvRwRouteParser.ParseRoute(Result.RouteFile, IsRW, Result.RouteEncoding, null, null, null, true);
				if (Game.TrainName != null && Game.TrainName.Length != 0) {
					string Folder = System.IO.Path.GetDirectoryName(Result.RouteFile);
					while (true) {
						string TrainFolder = OpenBveApi.Path.CombineDirectory(Folder, "Train");
						if (System.IO.Directory.Exists(TrainFolder)) {
							Folder = OpenBveApi.Path.CombineDirectory(TrainFolder, Game.TrainName);
							if (System.IO.Directory.Exists(Folder)) {
								string File = OpenBveApi.Path.CombineFile(Folder, "train.dat");
								if (System.IO.File.Exists(File)) {
									// associated train found
									Result.TrainFolder = Folder;
									Result.TrainEncoding = System.Text.Encoding.UTF8;
									for (int j = 0; j < Interface.CurrentOptions.TrainEncodings.Length; j++) {
										if (string.Compare(Interface.CurrentOptions.TrainEncodings[j].Value, Result.TrainFolder, StringComparison.InvariantCultureIgnoreCase) == 0) {
											Result.TrainEncoding = System.Text.Encoding.GetEncoding(Interface.CurrentOptions.TrainEncodings[j].Codepage);
											break;
										}
									}
								}
							} break;
						} else {
							System.IO.DirectoryInfo Info = System.IO.Directory.GetParent(Folder);
							if (Info != null) {
								Folder = Info.FullName;
							} else {
								break;
							}
						}
					}
				}
				Game.Reset(false);
			}
			// show main menu if applicable
			if (Result.RouteFile == null | Result.TrainFolder == null) {
				Result = formMain.ShowMainDialog();
				if (!Result.Start) {
					return;
				}
			}
			// screen
			int Width = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenWidth : Interface.CurrentOptions.WindowWidth;
			int Height = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenHeight : Interface.CurrentOptions.WindowHeight;
			if (Width < 16) Width = 16;
			if (Height < 16) Height = 16;
			Screen.Width = Width;
			Screen.Height = Height;
			World.AspectRatio = (double)Screen.Width / (double)Screen.Height;
			const double degree = 0.0174532925199433;
			World.VerticalViewingAngle = 45.0 * degree;
			World.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * World.VerticalViewingAngle) * World.AspectRatio);
			World.OriginalVerticalViewingAngle = World.VerticalViewingAngle;
			World.ExtraViewingDistance = 50.0;
			World.ForwardViewingDistance = (double)Interface.CurrentOptions.ViewingDistance;
			World.BackwardViewingDistance = 0.0;
			World.BackgroundImageDistance = (double)Interface.CurrentOptions.ViewingDistance;
			// load route and train
			SoundManager.Initialize();
			Plugins.LoadPlugins();
			if (!Loading.LoadX(Result.RouteFile, Result.RouteEncoding, Result.TrainFolder, Result.TrainEncoding)) {
				return;
			}
			Game.LogRouteName = System.IO.Path.GetFileName(Result.RouteFile);
			Game.LogTrainName = System.IO.Path.GetFileName(Result.TrainFolder);
			Game.LogDateTime = DateTime.Now;
			// initialize sdl window
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DOUBLEBUFFER, 1);
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DEPTH_SIZE, 24);
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_RED_SIZE, 8);
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_GREEN_SIZE, 8);
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_BLUE_SIZE, 8);
			//Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_ALPHA_SIZE, 8);
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_SWAP_CONTROL, Interface.CurrentOptions.VerticalSynchronization ? 1 : 0);
			Sdl.SDL_ShowCursor(Sdl.SDL_DISABLE);
			SdlWindowCreated = true;
			int Bits = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenBits : 32;
			// icon
			{
				string File = OpenBveApi.Path.CombineFile(Program.FileSystem.DataFolder, "icon.bmp");
				if (System.IO.File.Exists(File)) {
					try {
						IntPtr Bitmap = Sdl.SDL_LoadBMP(File);
						if (Bitmap != null) {
							if (CurrentPlatform == Platform.Windows) {
								Sdl.SDL_Surface Surface = (Sdl.SDL_Surface)System.Runtime.InteropServices.Marshal.PtrToStructure(Bitmap, typeof(Sdl.SDL_Surface));
								int ColorKey = Sdl.SDL_MapRGB(Surface.format, 0, 0, 255);
								Sdl.SDL_SetColorKey(Bitmap, Sdl.SDL_SRCCOLORKEY, ColorKey);
								Sdl.SDL_WM_SetIcon(Bitmap, null);
							} else {
								Sdl.SDL_WM_SetIcon(Bitmap, null);
							}
						}
					} catch { }
				}
			}
			// create window
			int fullscreen = Interface.CurrentOptions.FullscreenMode ? Sdl.SDL_FULLSCREEN : 0;
			IntPtr video = Sdl.SDL_SetVideoMode(Width, Height, Bits, Sdl.SDL_OPENGL | Sdl.SDL_DOUBLEBUF | fullscreen);
			if (video != IntPtr.Zero) {
				// create window
				Sdl.SDL_WM_SetCaption(Application.ProductName, null);
				// anisotropic filtering
				string[] Extensions = Gl.glGetString(Gl.GL_EXTENSIONS).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
				Interface.CurrentOptions.AnisotropicFilteringMaximum = 0;
				for (int i = 0; i < Extensions.Length; i++) {
					if (string.Compare(Extensions[i], "GL_EXT_texture_filter_anisotropic", StringComparison.OrdinalIgnoreCase) == 0) {
						float n; Gl.glGetFloatv(Gl.GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, out n);
						Interface.CurrentOptions.AnisotropicFilteringMaximum = (int)Math.Round((double)n);
						break;
					}
				}
				if (Interface.CurrentOptions.AnisotropicFilteringMaximum <= 0) {
					Interface.CurrentOptions.AnisotropicFilteringMaximum = 0;
					Interface.CurrentOptions.AnisotropicFilteringLevel = 0;
				} else if (Interface.CurrentOptions.AnisotropicFilteringLevel == 0 & Interface.CurrentOptions.AnisotropicFilteringMaximum > 0) {
					Interface.CurrentOptions.AnisotropicFilteringLevel = Interface.CurrentOptions.AnisotropicFilteringMaximum;
				} else if (Interface.CurrentOptions.AnisotropicFilteringLevel > Interface.CurrentOptions.AnisotropicFilteringMaximum) {
					Interface.CurrentOptions.AnisotropicFilteringLevel = Interface.CurrentOptions.AnisotropicFilteringMaximum;
				}
				// module initialization
				Renderer.Initialize();
				Renderer.InitializeLighting();
				Sdl.SDL_GL_SwapBuffers();
				Timetable.CreateTimetable();
				// camera
				MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
				MainLoop.InitializeMotionBlur();
				// start loop
				MainLoop.StartLoop();
			} else {
				// failed
				MessageBox.Show("SDL failed to create the window.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

	}
}