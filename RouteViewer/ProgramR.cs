// ╔═════════════════════════════════════════════════════════════╗
// ║ Program.cs for the Route Viewer                             ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using System.Globalization;
using System.Windows.Forms;

using Tao.OpenGl;
using Tao.Sdl;

namespace OpenBve {
	internal static class Program {

		// system
		internal enum Platform { Windows, Linux, Mac }
		internal static Platform CurrentPlatform = Platform.Windows;
		internal static bool CurrentlyRunOnMono = false;
		internal static FileSystem FileSystem = null;
		internal enum ProgramType { OpenBve, ObjectViewer, RouteViewer, Other }
		internal const ProgramType CurrentProgramType = ProgramType.RouteViewer;

		// members
		private static bool Quit = false;
		private static int LastTicks = int.MaxValue;
		internal static bool CpuReducedMode = false;
		internal static bool CpuAutomaticMode = true;
		private static int ReducedModeEnteringTime = 0;
		private static string CurrentRoute = null;
		internal static bool CurrentlyLoading = false;
		internal static int CurrentStation = -1;
		internal static bool JumpToPositionEnabled = false;
		internal static string JumpToPositionValue = "";
		
		// keys
		private static bool ShiftPressed = false;
		private static bool ControlPressed = false;
		private static bool AltPressed = false;

		// main
		[STAThread]
		internal static void Main(string[] args) {
			Interface.CurrentOptions.UseSound = true;
			Interface.CurrentOptions.ObjectOptimizationBasicThreshold = 1000;
			Interface.CurrentOptions.ObjectOptimizationFullThreshold = 250;
			// platform and mono
			int p = (int)Environment.OSVersion.Platform;
			if (p == 4 | p == 128) {
				/// general Unix
				CurrentPlatform = Platform.Linux;
			} else if (p == 6) {
				/// Mac
				CurrentPlatform = Platform.Mac;
			} else {
				/// non-Unix
				CurrentPlatform = Platform.Windows;
			}
			CurrentlyRunOnMono = Type.GetType("Mono.Runtime") != null;
			// file system
			FileSystem = FileSystem.FromCommandLineArgs(args);
			FileSystem.CreateFileSystem();
			SetPackageLookupDirectories();
			// command line arguments
			bool[] SkipArgs = new bool[args.Length];
			if (args.Length != 0) {
				string File = System.IO.Path.Combine(Application.StartupPath, "ObjectViewer.exe");
				if (System.IO.File.Exists(File)) {
					int Skips = 0;
					System.Text.StringBuilder NewArgs = new System.Text.StringBuilder();
					for (int i = 0; i < args.Length; i++) {
						if (System.IO.File.Exists(args[i])) {
							if (System.IO.Path.GetExtension(args[i]).Equals(".csv", StringComparison.OrdinalIgnoreCase)) {
								string Text = System.IO.File.ReadAllText(args[i], System.Text.Encoding.UTF8);
								if (Text.Length == 0 || Text.IndexOf("CreateMeshBuilder", StringComparison.OrdinalIgnoreCase) >= 0) {
									if (NewArgs.Length != 0) NewArgs.Append(" ");
									NewArgs.Append("\"" + args[i] + "\"");
									SkipArgs[i] = true;
									Skips++;
								}
							}
						} else {
							SkipArgs[i] = true;
							Skips++;
						}
					}
					if (NewArgs.Length != 0) {
						System.Diagnostics.Process.Start(File, NewArgs.ToString());
					}
					if (Skips == args.Length) return;
				}
			}
			// application
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if (Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO) != 0) {
				MessageBox.Show("SDL failed to initialize the video subsystem.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			// initialize sdl window
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DOUBLEBUFFER, 1);
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DEPTH_SIZE, 16);
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_RED_SIZE, 8);
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_GREEN_SIZE, 8);
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_BLUE_SIZE, 8);
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_ALPHA_SIZE, 8);
			Sdl.SDL_ShowCursor(Sdl.SDL_ENABLE);
			// icon
			if (Program.CurrentPlatform == Platform.Windows) {
				string File = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder(), "icon.bmp");
				if (System.IO.File.Exists(File)) {
					IntPtr Bitmap = Sdl.SDL_LoadBMP(File);
					if (Bitmap != null) {
						Sdl.SDL_Surface Surface = (Sdl.SDL_Surface)System.Runtime.InteropServices.Marshal.PtrToStructure(Bitmap, typeof(Sdl.SDL_Surface));
						int ColorKey = Sdl.SDL_MapRGB(Surface.format, 0, 0, 255);
						Sdl.SDL_SetColorKey(Bitmap, Sdl.SDL_SRCCOLORKEY, ColorKey);
						Sdl.SDL_WM_SetIcon(Bitmap, null);
					}
				}
			}
			// initialize camera
			ResetCamera();
			World.BackgroundImageDistance = 600.0;
			World.ForwardViewingDistance = 600.0;
			World.BackwardViewingDistance = 0.0;
			World.ExtraViewingDistance = 50.0;
			// create window
			Renderer.ScreenWidth = 960;
			Renderer.ScreenHeight = 600;
			int Bits = 32;
			IntPtr video = Sdl.SDL_SetVideoMode(Renderer.ScreenWidth, Renderer.ScreenHeight, Bits, Sdl.SDL_OPENGL | Sdl.SDL_DOUBLEBUF);
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
					Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.AnisotropicFiltering;
				} else {
					Interface.CurrentOptions.AnisotropicFilteringLevel = Interface.CurrentOptions.AnisotropicFilteringMaximum;
					Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.TrilinearMipmapped;
				}
				Interface.CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Sharp;
				// module initialization
				Renderer.Initialize();
				Renderer.InitializeLighting();
				SoundManager.Initialize();
				Gl.glClearColor(0.75f, 0.75f, 0.75f, 1.0f);
				Sdl.SDL_GL_SwapBuffers();
				Fonts.Initialize();
				UpdateViewport();
				// loop
				bool processCommandLineArgs = true;
				while (!Quit) {
					ProcessEvents();
					int a = Sdl.SDL_GetTicks();
					double TimeElapsed = 0.001 * (double)(a - LastTicks);
					if (CpuReducedMode) {
						System.Threading.Thread.Sleep(250);
					} else {
						System.Threading.Thread.Sleep(1);
						if (ReducedModeEnteringTime == 0) {
							ReducedModeEnteringTime = a + 2500;
						}
						if (World.CameraAlignmentDirection.Position.X != 0.0 | World.CameraAlignmentDirection.Position.Y != 0.0 | World.CameraAlignmentDirection.Position.Z != 0.0 | World.CameraAlignmentDirection.Pitch != 0.0 | World.CameraAlignmentDirection.Yaw != 0.0 | World.CameraAlignmentDirection.Roll != 0.0 | World.CameraAlignmentDirection.TrackPosition != 0.0 | World.CameraAlignmentDirection.Zoom != 0.0) {
							ReducedModeEnteringTime = a + 2500;
						} else if (a > ReducedModeEnteringTime & CpuAutomaticMode) {
							ReducedModeEnteringTime = 0;
							CpuReducedMode = true;
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
					Sdl.SDL_GL_SwapBuffers();
					LastTicks = a;
					// command line arguments
					if (processCommandLineArgs) {
						processCommandLineArgs = false;
						for (int i = 0; i < args.Length; i++) {
							if (!SkipArgs[i] && System.IO.File.Exists(args[i])) {
								CurrentlyLoading = true;
								Renderer.RenderScene(0.0);
								Sdl.SDL_GL_SwapBuffers();
								CurrentRoute = args[i];
								LoadRoute();
								CurrentlyLoading = false;
								UpdateCaption();
								break;
							}
						}
					}
				}
				// quit
				TextureManager.UnuseAllTextures();
				SoundManager.Deinitialize();
				Sdl.SDL_Quit();
			} else {
				MessageBox.Show("SDL failed to create the window.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		// reset camera
		private static void ResetCamera() {
			World.AbsoluteCameraPosition = new World.Vector3D(0.0, 2.5, -5.0);
			World.AbsoluteCameraDirection = new World.Vector3D(-World.AbsoluteCameraPosition.X, -World.AbsoluteCameraPosition.Y, -World.AbsoluteCameraPosition.Z);
			World.AbsoluteCameraSide = new World.Vector3D(-World.AbsoluteCameraPosition.Z, 0.0, World.AbsoluteCameraPosition.X);
			World.Normalize(ref World.AbsoluteCameraDirection.X, ref World.AbsoluteCameraDirection.Y, ref World.AbsoluteCameraDirection.Z);
			World.Normalize(ref World.AbsoluteCameraSide.X, ref World.AbsoluteCameraSide.Y, ref World.AbsoluteCameraSide.Z);
			World.AbsoluteCameraUp = World.Cross(World.AbsoluteCameraDirection, World.AbsoluteCameraSide);
			World.VerticalViewingAngle = 45.0 * 0.0174532925199433;
			World.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * World.VerticalViewingAngle) * World.AspectRatio);
			World.OriginalVerticalViewingAngle = World.VerticalViewingAngle;
		}

		// update viewport
		internal static void UpdateViewport() {
			Gl.glViewport(0, 0, Renderer.ScreenWidth, Renderer.ScreenHeight);
			World.AspectRatio = (double)Renderer.ScreenWidth / (double)Renderer.ScreenHeight;
			World.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * World.VerticalViewingAngle) * World.AspectRatio);
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glLoadIdentity();
			const double invdeg = 57.295779513082320877;
			Glu.gluPerspective(World.VerticalViewingAngle * invdeg, -World.AspectRatio, 0.2, 1000.0);
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glLoadIdentity();
		}

		// load route
		private static bool LoadRoute() {
			CurrentStation = -1;
			Game.Reset();
			Renderer.Initialize();
			Fonts.Initialize();
			UpdateViewport();
			bool result;
			try {
				Loading.Load(CurrentRoute, System.Text.Encoding.UTF8);
				result = true;
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				Game.Reset();
				CurrentRoute = null;
				result = false;
			}
			Renderer.InitializeLighting();
			ObjectManager.InitializeVisibility();
			return result;
		}

		// jump to station
		private static void JumpToStation(int Direction) {
			if (Direction < 0) {
				for (int i = Game.Stations.Length - 1; i >= 0; i--) {
					if (Game.Stations[i].Stops.Length != 0) {
						double p = Game.Stations[i].Stops[Game.Stations[i].Stops.Length - 1].TrackPosition;
						if (p < World.CameraTrackFollower.TrackPosition - 0.1) {
							TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, p, true, false);
							World.CameraCurrentAlignment.TrackPosition = p;
							CurrentStation = i;
							break;
						}
					}
				}
			} else if (Direction > 0) {
				for (int i = 0; i < Game.Stations.Length; i++) {
					if (Game.Stations[i].Stops.Length != 0) {
						double p = Game.Stations[i].Stops[Game.Stations[i].Stops.Length - 1].TrackPosition;
						if (p > World.CameraTrackFollower.TrackPosition + 0.1) {
							TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, p, true, false);
							World.CameraCurrentAlignment.TrackPosition = p;
							CurrentStation = i;
							break;
						}
					}
				}
			}
		}

		// process events
		private static void ProcessEvents() {
			Sdl.SDL_Event Event;
			double speedModified = (ShiftPressed ? 2.0 : 1.0) * (ControlPressed ? 4.0 : 1.0) * (AltPressed ? 8.0 : 1.0);
			while (Sdl.SDL_PollEvent(out Event) != 0) {
				switch (Event.type) {
						// quit
					case Sdl.SDL_QUIT:
						Quit = true;
						return;
						// resize
					case Sdl.SDL_VIDEORESIZE:
						Renderer.ScreenWidth = Event.resize.w;
						Renderer.ScreenHeight = Event.resize.h;
						UpdateViewport();
						break;
						// mouse
					case Sdl.SDL_MOUSEBUTTONDOWN:
						break;
					case Sdl.SDL_MOUSEBUTTONUP:
						break;
					case Sdl.SDL_MOUSEMOTION:
						break;
						// key down
					case Sdl.SDL_KEYDOWN:
						switch (Event.key.keysym.sym) {
							case Sdl.SDLK_LSHIFT:
							case Sdl.SDLK_RSHIFT:
								ShiftPressed = true;
								break;
							case Sdl.SDLK_LCTRL:
							case Sdl.SDLK_RCTRL:
								ControlPressed = true;
								break;
							case Sdl.SDLK_LALT:
							case Sdl.SDLK_RALT:
								AltPressed = true;
								break;
							case Sdl.SDLK_F5:
								if (CurrentRoute != null) {
									CurrentlyLoading = true;
									Renderer.RenderScene(0.0);
									Sdl.SDL_GL_SwapBuffers();
									World.CameraAlignment a = World.CameraCurrentAlignment;
									if (LoadRoute()) {
										World.CameraCurrentAlignment = a;
										TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, -1.0, true, false);
										TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, a.TrackPosition, true, false);
										World.CameraAlignmentDirection = new World.CameraAlignment();
										World.CameraAlignmentSpeed = new World.CameraAlignment();
										ObjectManager.UpdateVisibility(a.TrackPosition, true);
										ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
									}
									CurrentlyLoading = false;
								}
								break;
							case Sdl.SDLK_F7:
								{
									OpenFileDialog Dialog = new OpenFileDialog();
									Dialog.CheckFileExists = true;
									Dialog.Filter = "CSV/RW files|*.csv;*.rw|All files|*";
									if (Dialog.ShowDialog() == DialogResult.OK) {
										CurrentlyLoading = true;
										Renderer.RenderScene(0.0);
										Sdl.SDL_GL_SwapBuffers();
										CurrentRoute = Dialog.FileName;
										LoadRoute();
										ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
										CurrentlyLoading = false;
										UpdateCaption();
									}
								}
								break;
							case Sdl.SDLK_F9:
								if (Interface.MessageCount != 0) {
									formMessages.ShowMessages();
								}
								break;
							case Sdl.SDLK_a:
							case Sdl.SDLK_KP4:
								World.CameraAlignmentDirection.Position.X = -World.CameraExteriorTopSpeed * speedModified;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_d:
							case Sdl.SDLK_KP6:
								World.CameraAlignmentDirection.Position.X = World.CameraExteriorTopSpeed * speedModified;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_KP2:
								World.CameraAlignmentDirection.Position.Y = -World.CameraExteriorTopSpeed * speedModified;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_KP8:
								World.CameraAlignmentDirection.Position.Y = World.CameraExteriorTopSpeed * speedModified;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_w:
							case Sdl.SDLK_KP9:
								World.CameraAlignmentDirection.TrackPosition = World.CameraExteriorTopSpeed * speedModified;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_s:
							case Sdl.SDLK_KP3:
								World.CameraAlignmentDirection.TrackPosition = -World.CameraExteriorTopSpeed * speedModified;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_LEFT:
								World.CameraAlignmentDirection.Yaw = -World.CameraExteriorTopAngularSpeed * speedModified;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_RIGHT:
								World.CameraAlignmentDirection.Yaw = World.CameraExteriorTopAngularSpeed * speedModified;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_UP:
								World.CameraAlignmentDirection.Pitch = World.CameraExteriorTopAngularSpeed * speedModified;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_DOWN:
								World.CameraAlignmentDirection.Pitch = -World.CameraExteriorTopAngularSpeed * speedModified;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_KP_DIVIDE:
								World.CameraAlignmentDirection.Roll = -World.CameraExteriorTopAngularSpeed * speedModified;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_KP_MULTIPLY:
								World.CameraAlignmentDirection.Roll = World.CameraExteriorTopAngularSpeed * speedModified;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_KP0:
								World.CameraAlignmentDirection.Zoom = World.CameraZoomTopSpeed * speedModified;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_KP_PERIOD:
								World.CameraAlignmentDirection.Zoom = -World.CameraZoomTopSpeed * speedModified;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_KP1:
								Game.ApplyPointOfInterest(-1, true);
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_KP7:
								Game.ApplyPointOfInterest(1, true);
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_PAGEUP:
								JumpToStation(1);
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_PAGEDOWN:
								JumpToStation(-1);
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_KP5:
								World.CameraCurrentAlignment.Yaw = 0.0;
								World.CameraCurrentAlignment.Pitch = 0.0;
								World.CameraCurrentAlignment.Roll = 0.0;
								World.CameraCurrentAlignment.Position = new World.Vector3D(0.0, 2.5, 0.0);
								World.CameraCurrentAlignment.Zoom = 0.0;
								World.CameraAlignmentDirection = new World.CameraAlignment();
								World.CameraAlignmentSpeed = new World.CameraAlignment();
								World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
								UpdateViewport();
								World.UpdateAbsoluteCamera(0.0);
								World.UpdateViewingDistances();
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_f:
								Renderer.OptionWireframe = !Renderer.OptionWireframe;
								CpuReducedMode = false;
								if (Renderer.OptionWireframe) {
									Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE);
								} else {
									Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
								} break;
							case Sdl.SDLK_n:
								Renderer.OptionNormals = !Renderer.OptionNormals;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_e:
								Renderer.OptionEvents = !Renderer.OptionEvents;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_c:
								CpuAutomaticMode = !CpuAutomaticMode;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_i:
								Renderer.OptionInterface = !Renderer.OptionInterface;
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_m:
								SoundManager.Mute = !SoundManager.Mute;
								break;
							case Sdl.SDLK_PLUS:
							case Sdl.SDLK_KP_PLUS:
								if (!JumpToPositionEnabled) {
									JumpToPositionEnabled = true;
									JumpToPositionValue = "+";
									CpuReducedMode = false;
								}
								break;
							case Sdl.SDLK_MINUS:
							case Sdl.SDLK_KP_MINUS:
								if (!JumpToPositionEnabled) {
									JumpToPositionEnabled = true;
									JumpToPositionValue = "-";
									CpuReducedMode = false;
								}
								break;
							case Sdl.SDLK_0:
							case Sdl.SDLK_1:
							case Sdl.SDLK_2:
							case Sdl.SDLK_3:
							case Sdl.SDLK_4:
							case Sdl.SDLK_5:
							case Sdl.SDLK_6:
							case Sdl.SDLK_7:
							case Sdl.SDLK_8:
							case Sdl.SDLK_9:
								if (!JumpToPositionEnabled) {
									JumpToPositionEnabled = true;
									JumpToPositionValue = string.Empty;
								}
								JumpToPositionValue += char.ConvertFromUtf32(48 + Event.key.keysym.sym - Sdl.SDLK_0);
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_PERIOD:
								if (!JumpToPositionEnabled) {
									JumpToPositionEnabled = true;
									JumpToPositionValue = "0.";
								} else if (JumpToPositionValue.IndexOf('.') == -1) {
									JumpToPositionValue += ".";
								}
								CpuReducedMode = false;
								break;
							case Sdl.SDLK_BACKSPACE:
								if (JumpToPositionEnabled && JumpToPositionValue.Length != 0) {
									JumpToPositionValue = JumpToPositionValue.Substring(0, JumpToPositionValue.Length - 1);
									CpuReducedMode = false;
								}
								break;
							case Sdl.SDLK_RETURN:
								if (JumpToPositionEnabled) {
									if (JumpToPositionValue.Length != 0) {
										int direction;
										if (JumpToPositionValue[0] == '-') {
											JumpToPositionValue = JumpToPositionValue.Substring(1);
											direction = -1;
										} else if (JumpToPositionValue[0] == '+') {
											JumpToPositionValue = JumpToPositionValue.Substring(1);
											direction = 1;
										} else {
											direction = 0;
										}
										double value;
										if (double.TryParse(JumpToPositionValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) {
											if (direction != 0) {
												value = World.CameraTrackFollower.TrackPosition + (double)direction * value;
											}
											TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, value, true, false);
											World.CameraCurrentAlignment.TrackPosition = value;
											World.UpdateAbsoluteCamera(0.0);
											World.UpdateViewingDistances();
										}
									}
									JumpToPositionEnabled = false;
									CpuReducedMode = false;
								}
								break;
							case Sdl.SDLK_ESCAPE:
								JumpToPositionEnabled = false;
								CpuReducedMode = false;
								break;
						} break;
						// key up
					case Sdl.SDL_KEYUP:
						switch (Event.key.keysym.sym) {
							case Sdl.SDLK_LSHIFT:
							case Sdl.SDLK_RSHIFT:
								ShiftPressed = false;
								break;
							case Sdl.SDLK_LCTRL:
							case Sdl.SDLK_RCTRL:
								ControlPressed = false;
								break;
							case Sdl.SDLK_LALT:
							case Sdl.SDLK_RALT:
								AltPressed = false;
								break;
							case Sdl.SDLK_a:
							case Sdl.SDLK_KP4:
							case Sdl.SDLK_d:
							case Sdl.SDLK_KP6:
								World.CameraAlignmentDirection.Position.X = 0.0;
								break;
							case Sdl.SDLK_KP2:
							case Sdl.SDLK_KP8:
								World.CameraAlignmentDirection.Position.Y = 0.0;
								break;
							case Sdl.SDLK_w:
							case Sdl.SDLK_KP9:
							case Sdl.SDLK_s:
							case Sdl.SDLK_KP3:
								World.CameraAlignmentDirection.TrackPosition = 0.0;
								break;
							case Sdl.SDLK_LEFT:
							case Sdl.SDLK_RIGHT:
								World.CameraAlignmentDirection.Yaw = 0.0;
								break;
							case Sdl.SDLK_UP:
							case Sdl.SDLK_DOWN:
								World.CameraAlignmentDirection.Pitch = 0.0;
								break;
							case Sdl.SDLK_KP_DIVIDE:
							case Sdl.SDLK_KP_MULTIPLY:
								World.CameraAlignmentDirection.Roll = 0.0;
								break;
							case Sdl.SDLK_KP0:
							case Sdl.SDLK_KP_PERIOD:
								World.CameraAlignmentDirection.Zoom = 0.0;
								break;
						} break;
				}
			}
		}

		// update caption
		private static void UpdateCaption() {
			if (CurrentRoute != null) {
				Sdl.SDL_WM_SetCaption(System.IO.Path.GetFileName(CurrentRoute) + " - " + Application.ProductName, null);
			} else {
				Sdl.SDL_WM_SetCaption(Application.ProductName, null);
			}
		}
		
		/// <summary>The object that serves as an authentication for the SetPackageLookupDirectories call.</summary>
		private static object SetPackageLookupDirectoriesAuthentication = null;

		/// <summary>Provides the API with lookup directories for all installed packages.</summary>
		internal static void SetPackageLookupDirectories() {
			int size = 16;
			string[] names = new string[size];
			string[] directories = new string[size];
			int count = 0;
			foreach (string lookupDirectory in FileSystem.ManagedContentFolders) {
				string[] packageDirectories = System.IO.Directory.GetDirectories(lookupDirectory);
				foreach (string packageDirectory in packageDirectories) {
					string package = System.IO.Path.GetFileName(packageDirectory);
					if (count == size) {
						size <<= 1;
						Array.Resize<string>(ref names, size);
						Array.Resize<string>(ref directories, size);
					}
					names[count] = package;
					directories[count] = packageDirectory;
					count++;
				}
			}
			Array.Resize<string>(ref names, count);
			Array.Resize<string>(ref directories, count);
			SetPackageLookupDirectoriesAuthentication = OpenBveApi.Path.SetPackageLookupDirectories(names, directories, SetPackageLookupDirectoriesAuthentication);
		}

	}
}