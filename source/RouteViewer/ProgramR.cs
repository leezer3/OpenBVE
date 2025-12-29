// ╔═════════════════════════════════════════════════════════════╗
// ║ Program.cs for the Route Viewer                             ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using LibRender2.Cameras;
using LibRender2.Menu;
using LibRender2.Overlays;
using LibRender2.Screens;
using LibRender2.Viewports;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using RouteManager2;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Windows.Forms;
using Buffer = System.Buffer;
using ButtonState = OpenTK.Input.ButtonState;

namespace RouteViewer
{
	internal static class Program {

		// system
		internal static FileSystem FileSystem = null;
		internal static string CurrentRouteFile = null;
		internal static bool CurrentlyLoading = false;
		internal static bool JumpToPositionEnabled = false;
		internal static string JumpToPositionValue = "";
		internal static double MinimumJumpToPositionValue =  0;
		internal static bool processCommandLineArgs;

		// keys
		private static bool ShiftPressed = false;
		private static bool ControlPressed = false;
		private static bool AltPressed = false;

		// mouse
		private static int MouseButton;

		/// <summary>The host API used by this program.</summary>
		internal static Host CurrentHost = null;

		internal static NewRenderer Renderer;

		internal static CurrentRoute CurrentRoute;

		internal static Sounds Sounds;

		internal static TrainManager TrainManager;

		internal static formRailPaths pathForm;

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		private static extern bool SetProcessDPIAware();

		// main
		[STAThread]
		internal static void Main(string[] args)
		{
			CurrentHost = new Host();
			// file system
			FileSystem = FileSystem.FromCommandLineArgs(args, CurrentHost);
			FileSystem.CreateFileSystem();
			Sounds = new Sounds(CurrentHost);
			Options.LoadOptions();
			// n.b. Init the toolkit before the renderer
			var options = new ToolkitOptions
			{
				Backend = PlatformBackend.PreferX11,
			};

			if (CurrentHost.Platform == HostPlatform.MicrosoftWindows)
			{
				// We're managing our own DPI
				options.EnableHighResolution = false;
				SetProcessDPIAware();
			}

			Toolkit.Init(options);

			Renderer = new NewRenderer(CurrentHost, Interface.CurrentOptions, FileSystem);
			CurrentRoute = new CurrentRoute(CurrentHost, Renderer);
			TrainManager = new TrainManager(CurrentHost, Renderer, Interface.CurrentOptions, FileSystem);
			if (!CurrentHost.LoadPlugins(FileSystem, Interface.CurrentOptions, out string error, TrainManager, Renderer))
			{
				MessageBox.Show(error, @"OpenBVE", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			GameMenu.Instance = new GameMenu();
			// command line arguments
			string objectsToLoad = string.Empty;
			if (args.Length != 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i] != null)
					{
						if (System.IO.File.Exists(args[i]))
						{
							for (int j = 0; j < CurrentHost.Plugins.Length; j++)
							{
								if (CurrentHost.Plugins[j].Object != null && CurrentHost.Plugins[j].Object.CanLoadObject(args[i]))
								{
									objectsToLoad += args[i] + " ";
									continue;
								}

								if (CurrentHost.Plugins[j].Route != null && CurrentHost.Plugins[j].Route.CanLoadRoute(args[i]))
								{
									if (string.IsNullOrEmpty(CurrentRouteFile))
									{
										CurrentRouteFile = args[i];
										processCommandLineArgs = true;
									}
								}
							}
						}
						else if (args[i].ToLowerInvariant() == "/enablehacks")
						{
							//Deliberately undocumented option for debugging use
							Interface.CurrentOptions.EnableBveTsHacks = true;
							for (int j = 0; j < CurrentHost.Plugins.Length; j++)
							{
								if (CurrentHost.Plugins[j].Object != null)
								{
									CompatabilityHacks enabledHacks = new CompatabilityHacks
									{
										BveTsHacks = true, 
										CylinderHack = false,
										BlackTransparency =  true
									};
									CurrentHost.Plugins[j].Object.SetCompatibilityHacks(enabledHacks);
								}
							}
						}
					}
				}
			}

			if (objectsToLoad.Length != 0)
			{
				string File = System.IO.Path.Combine(Application.StartupPath, "ObjectViewer.exe");
				if (System.IO.File.Exists(File))
				{
					System.Diagnostics.Process.Start(File, objectsToLoad);
					if (string.IsNullOrEmpty(CurrentRouteFile))
					{
						//We only supplied objects, so launch Object Viewer instead
						Environment.Exit(0);
					}
				}

			}
			if (CurrentHost.Platform == HostPlatform.MicrosoftWindows)
			{
				// Tell Windows that the main game is managing it's own DPI
				SetProcessDPIAware();
			}

			string folder = Program.FileSystem.GetDataFolder("Languages");
			Translations.LoadLanguageFiles(folder);
			Interface.CurrentOptions.ObjectOptimizationBasicThreshold = 1000;
			Interface.CurrentOptions.ObjectOptimizationFullThreshold = 250;
			// application
			Renderer.GraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, Interface.CurrentOptions.AntiAliasingLevel);
			if (Renderer.Screen.Width == 0 || Renderer.Screen.Height == 0)
			{
				//Duff values saved, so reset to something sensible else we crash
				Renderer.Screen.Width = 1024;
				Renderer.Screen.Height = 768;
			}
			Renderer.CameraTrackFollower = new TrackFollower(Program.CurrentHost);
			Renderer.GameWindow = new RouteViewer(Renderer.Screen.Width, Renderer.Screen.Height, Renderer.GraphicsMode, "Route Viewer", GameWindowFlags.Default);
			Renderer.GameWindow.Visible = true;
			Renderer.GameWindow.TargetUpdateFrequency = 0;
			Renderer.GameWindow.TargetRenderFrequency = 0;
			Renderer.GameWindow.Title = "Route Viewer";
			processCommandLineArgs = true;
			Renderer.GameWindow.Run();
			//Unload
			Sounds.DeInitialize();
		}
		
		// load route
		internal static bool LoadRoute(byte[] textureBytes = null) {
			if (string.IsNullOrEmpty(CurrentRouteFile))
			{
				return false;
			}
			Renderer.UpdateViewport(ViewportChangeMode.NoChange);
			bool result;
			try
			{
				Encoding encoding = TextEncoding.GetSystemEncodingFromFile(CurrentRouteFile);
				Loading.Load(CurrentRouteFile, encoding, textureBytes);
				result = true;
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				Game.Reset();
				result = false;
				CurrentRouteFile = null;
			}

			if (Loading.Cancel)
			{
				result = false;
				CurrentRouteFile = null;
			}

			Renderer.Camera.QuadTreeLeaf = null;
			Renderer.Lighting.Initialize();
			Renderer.InitializeVisibility();
			for (int i = 0; i < CurrentRoute.Tracks.Count; i++)
			{
				int key = Program.CurrentRoute.Tracks.ElementAt(i).Key;
					
				if (!Renderer.trackColors.ContainsKey(key))
				{
					if (key == 0)
					{
						Renderer.trackColors.Add(key, new RailPath(CurrentHost, Renderer, key, Program.CurrentRoute.BlockLength, Color24.Red));
						Renderer.usedTrackColors.Add(5);
					}
					else
					{
						var randomGenerator = new Random();
						int colorIdx = 5; // known value already in list to make our while loop easy
						while (Renderer.usedTrackColors.Contains(colorIdx))
						{
							colorIdx = randomGenerator.Next(0, 255);
						}
						Renderer.usedTrackColors.Add(colorIdx);
						Renderer.trackColors.Add(key, new RailPath(Program.CurrentHost, Renderer, key, Program.CurrentRoute.BlockLength, ColorPalettes.Windows256ColorPalette[colorIdx])); //use the 256 color Windows pallette for a decent set of contrasting colors	
					}
				}
				Renderer.trackColors[key].Render();
			}

			Renderer.CurrentInterface = InterfaceType.Normal;
			return result;
		}

		// jump to station
		private static void JumpToStation(int Direction) {
			if (Program.CurrentRoute.Tracks[0].Direction == TrackDirection.Reverse)
			{
				Direction = -Direction;
			}
			if (Direction < 0) {
				for (int i = CurrentRoute.Stations.Length - 1; i >= 0; i--) {
					if (CurrentRoute.Stations[i].Stops.Length != 0) {
						double p = CurrentRoute.Stations[i].Stops[CurrentRoute.Stations[i].Stops.Length - 1].TrackPosition;
						if (p < Program.Renderer.CameraTrackFollower.TrackPosition - 0.1) {
							Program.Renderer.CameraTrackFollower.UpdateAbsolute(p, true, false);
							Renderer.Camera.Alignment.TrackPosition = p;
							Renderer.Camera.Reset(Program.CurrentRoute.Tracks[0].Direction == TrackDirection.Reverse);
							break;
						}
					}
				}
			} else if (Direction > 0) {
				for (int i = 0; i < CurrentRoute.Stations.Length; i++) {
					if (CurrentRoute.Stations[i].Stops.Length != 0) {
						double p = CurrentRoute.Stations[i].Stops[CurrentRoute.Stations[i].Stops.Length - 1].TrackPosition;
						if (p > Program.Renderer.CameraTrackFollower.TrackPosition + 0.1) {
							Program.Renderer.CameraTrackFollower.UpdateAbsolute(p, true, false);
							Renderer.Camera.Alignment.TrackPosition = p;
							Renderer.Camera.Reset(Program.CurrentRoute.Tracks[0].Direction == TrackDirection.Reverse);
							break;
						}
					}
				}
			}
		}

		internal static void UpdateGraphicsSettings()
		{
			Renderer.Camera.ForwardViewingDistance = Interface.CurrentOptions.ViewingDistance;
			Program.CurrentRoute.CurrentBackground.BackgroundImageDistance = Interface.CurrentOptions.ViewingDistance;
			if (CurrentRouteFile != null)
			{
				Program.CurrentlyLoading = true;
				Renderer.RenderScene(0.0);
				Program.Renderer.GameWindow.SwapBuffers();
				CameraAlignment a = Renderer.Camera.Alignment;
				if (Program.LoadRoute())
				{
					Renderer.Camera.Alignment = a;
					Program.Renderer.CameraTrackFollower.UpdateAbsolute(-1.0, true, false);
					Program.Renderer.CameraTrackFollower.UpdateAbsolute(a.TrackPosition, true, false);
					Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
				}
				Renderer.UpdateViewingDistances(Interface.CurrentOptions.ViewingDistance);
				Program.CurrentlyLoading = false;
			}
		}

		internal static void MouseWheelEvent(object sender, MouseWheelEventArgs e)
		{
			switch (Program.Renderer.CurrentInterface)
			{
				case InterfaceType.Menu:
				case InterfaceType.GLMainMenu:
					Game.Menu.ProcessMouseScroll(e.Delta);
					break;
			}
		}

		internal static void MouseMoveEvent(object sender, MouseMoveEventArgs e)
		{
			switch (Program.Renderer.CurrentInterface)
			{
				case InterfaceType.Menu:
				case InterfaceType.GLMainMenu:
					Game.Menu.ProcessMouseMove(e.X, e.Y);
					break;
			}
		}

		internal static void MouseEvent(object sender, MouseButtonEventArgs e)
		{
			switch (Renderer.CurrentInterface)
			{
				case InterfaceType.Menu:
				case InterfaceType.GLMainMenu:
					if (e.IsPressed)
					{
						// viewer hooks up and down to same event
						Game.Menu.ProcessMouseDown(e.X, e.Y);
					}
					break;
				default:
					if (e.Button == OpenTK.Input.MouseButton.Left)
					{
						MouseButton = e.Mouse.LeftButton == ButtonState.Pressed ? 1 : 0;
					}
					if (e.Button == OpenTK.Input.MouseButton.Right)
					{
						MouseButton = e.Mouse.RightButton == ButtonState.Pressed ? 2 : 0;
					}
					if (e.Button == OpenTK.Input.MouseButton.Middle)
					{
						MouseButton = e.Mouse.RightButton == ButtonState.Pressed ? 3 : 0;
					}
					previousMouseState = Mouse.GetState();
					if (MouseButton == 0)
					{
						Renderer.Camera.AlignmentDirection.Yaw = 0.0;
						Renderer.Camera.AlignmentDirection.Pitch = 0.0;
						Renderer.Camera.AlignmentDirection.Position.X = 0.0;
						Renderer.Camera.AlignmentDirection.Position.Y = 0.0;
					}
					break;
			}
			
		}

		internal static MouseState currentMouseState;
		internal static MouseState previousMouseState;

		internal static void MouseMovement()
		{
			if (MouseButton == 0 || Program.Renderer.CurrentInterface != InterfaceType.Normal) return;

			currentMouseState = Mouse.GetState();
			if (currentMouseState != previousMouseState)
			{
				Renderer.Camera.AlignmentDirection.Yaw = 0.0;
				Renderer.Camera.AlignmentDirection.Pitch = 0.0;
				Renderer.Camera.AlignmentDirection.Position.X = 0.0;
				Renderer.Camera.AlignmentDirection.Position.Y = 0.0;
				if (MouseButton == 1)
				{
					Renderer.Camera.AlignmentDirection.Yaw = 0.025 * (previousMouseState.X - currentMouseState.X);
					Renderer.Camera.AlignmentDirection.Pitch = 0.025 * (previousMouseState.Y - currentMouseState.Y);
				}
				else if (MouseButton == 2)
				{
					Renderer.Camera.AlignmentDirection.Position.X = 0.1 * (previousMouseState.X - currentMouseState.X);
					Renderer.Camera.AlignmentDirection.Position.Y = 0.1 * (previousMouseState.Y - currentMouseState.Y);
				}
				
			}
		}

		internal static void FileDrop(object sender, FileDropEventArgs e)
		{
			//Seem to need to flush the WM queue after dropping a file, else it crashes nastily after load on Mono
			Application.DoEvents();
			CurrentlyLoading = true;
			CurrentRouteFile = e.FileName;
			lock (Renderer.VisibilityUpdateLock)
			{
				UpdateCaption();
				LoadRoute();
				ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
			}

			CurrentlyLoading = false;
			Renderer.Camera.Reset(Program.CurrentRoute.Tracks[0].Direction == TrackDirection.Reverse);
			UpdateCaption();

		}

		internal static void KeyDownEvent(object sender, KeyboardKeyEventArgs e)
		{
			double speedModified = (ShiftPressed ? 2.0 : 1.0) * (ControlPressed ? 4.0 : 1.0) * (AltPressed ? 8.0 : 1.0);
			switch (e.Key)
			{
				case Key.ShiftLeft:
				case Key.ShiftRight:
					ShiftPressed = true;
					break;
				case Key.ControlLeft:
				case Key.ControlRight:
					ControlPressed = true;
					break;
				case Key.LAlt:
				case Key.RAlt:
					AltPressed = true;
					break;
				case Key.F5:
					if (CurrentlyLoading)
					{
						return;
					}

					if (JumpToPositionEnabled)
					{
						JumpToPositionEnabled = false;
						JumpToPositionValue = string.Empty;
					}
					byte[] textureBytes = {};
					if (CurrentRouteFile != null && CurrentlyLoading == false)
					{
						CurrentlyLoading = true;
						lock (Renderer.VisibilityUpdateLock)
						{
							Renderer.OptionInterface = false;
							UpdateCaption();
							if (!Interface.CurrentOptions.LoadingBackground)
							{
								Renderer.RenderScene(0.0);
								Renderer.GameWindow.SwapBuffers();
								textureBytes = new byte[Renderer.Screen.Width * Renderer.Screen.Height * 4];
								GL.ReadPixels(0, 0, Renderer.Screen.Width, Renderer.Screen.Height, PixelFormat.Rgba, PixelType.UnsignedByte, textureBytes);
								// GL.ReadPixels is reversed for what it wants as a texture, so we've got to flip it
								byte[] tmp = new byte[Renderer.Screen.Width * 4]; // temp row
								int currentLine = 0;
								while (currentLine < Renderer.Screen.Height / 2)
								{
									int start = currentLine * Renderer.Screen.Width * 4;
									int flipStart = (Renderer.Screen.Height - currentLine - 1) * Renderer.Screen.Width * 4;
									Buffer.BlockCopy(textureBytes, start, tmp, 0, Renderer.Screen.Width * 4);
									Buffer.BlockCopy(textureBytes, flipStart, textureBytes, start, Renderer.Screen.Width * 4);
									Buffer.BlockCopy(tmp, 0, textureBytes, flipStart, Renderer.Screen.Width * 4);
									currentLine++;
								}
							}

							Renderer.Reset();
							CameraAlignment a = Renderer.Camera.Alignment;

							if (LoadRoute(textureBytes))
							{
								Renderer.Camera.Alignment = a;
								Program.Renderer.CameraTrackFollower.UpdateAbsolute(-1.0, true, false);
								Program.Renderer.CameraTrackFollower.UpdateAbsolute(a.TrackPosition, true, false);
								ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
							}
							else
							{
								Renderer.Camera.Reset(Program.CurrentRoute.Tracks[0].Direction == TrackDirection.Reverse);
								Renderer.UpdateViewport(ViewportChangeMode.NoChange);
								World.UpdateAbsoluteCamera(0.0);
								Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
							}
						}

						CurrentlyLoading = false;
						Renderer.OptionInterface = true;
						GCSettings.LargeObjectHeapCompactionMode =  GCLargeObjectHeapCompactionMode.CompactOnce; 
						GC.Collect();
						UpdateCaption();
						
					}
					break;
				case Key.F7:
					if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
					{
						return;
					}
					
					if (CurrentlyLoading)
					{
						break;
					}

					if (JumpToPositionEnabled)
					{
						JumpToPositionEnabled = false;
						JumpToPositionValue = string.Empty;
					}

					string previousRoute = CurrentRouteFile;
					OpenFileDialog Dialog = new OpenFileDialog();
					Dialog.CheckFileExists = true;
					Dialog.Filter = @"All Supported Routes|*.csv;*.rw;*.dat;*.txt|CSV/RW files|*.csv;*.rw|Mechanik Routes|*.dat|BVE5 Routes|*.txt|All files|*";
					if (Dialog.ShowDialog() == DialogResult.OK)
					{
						Application.DoEvents();
						CurrentlyLoading = true;
						CurrentRouteFile = Dialog.FileName;
						UpdateCaption();
						bool canLoad = false;
						for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
						{
							if (Program.CurrentHost.Plugins[i].Route != null && Program.CurrentHost.Plugins[i].Route.CanLoadRoute(CurrentRouteFile))
							{
								canLoad = true;
								break;
							}
						}

						lock (Renderer.VisibilityUpdateLock)
						{
							if (canLoad && LoadRoute())
							{
								ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
								Renderer.Camera.Reset(Program.CurrentRoute.Tracks[0].Direction == TrackDirection.Reverse);
								Renderer.UpdateViewport(ViewportChangeMode.NoChange);
								World.UpdateAbsoluteCamera(0.0);
								Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
							}
							else
							{
								bool isObject = false;
								for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
								{
									if (Program.CurrentHost.Plugins[i].Object != null && Program.CurrentHost.Plugins[i].Object.CanLoadObject(CurrentRouteFile))
									{
										isObject = true;
										break;
									}
								}

								if (isObject)
								{
									// oops, that's actually an object- Let's show Object Viewer
									string File = System.IO.Path.Combine(Application.StartupPath, "ObjectViewer.exe");
									if (System.IO.File.Exists(File))
									{
										System.Diagnostics.Process.Start(File, CurrentRouteFile);
									}
								}
								else
								{
									MessageBox.Show("No plugins found capable of loading routefile: " + Environment.NewLine + CurrentRouteFile);
								}

								CurrentRouteFile = previousRoute;
							}

							Renderer.Camera.Reset(Program.CurrentRoute.Tracks[0].Direction == TrackDirection.Reverse);
							Renderer.UpdateViewport(ViewportChangeMode.NoChange);
							World.UpdateAbsoluteCamera(0.0);
							Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
						}
						CurrentlyLoading = false;
						UpdateCaption();
					}
					else
					{
						if (Program.CurrentHost.MonoRuntime)
						{
							//HACK: Dialog doesn't close properly when pressing the ESC key under Mono
							//Avoid calling Application.DoEvents() unless absolutely necessary though!
							Application.DoEvents();
						}
					}
					Dialog.Dispose();
					break;
				case Key.F8:
					if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
					{
						return;
					}

					if (CurrentlyLoading)
					{
						//Don't allow the user to update the settings during loading, bad idea..
						break;
					}
					if (FormOptions.ShowOptions() == DialogResult.OK)
					{
						UpdateGraphicsSettings();
					}
					Application.DoEvents();
					Renderer.Camera.AlignmentDirection.TrackPosition = 0;
					Renderer.Camera.AlignmentDirection.Position.X = 0;
					Renderer.Camera.AlignmentDirection.Position.Y = 0;
					break;
				case Key.F9:
					if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
					{
						Program.Renderer.CurrentInterface = InterfaceType.Menu;
						Game.Menu.PushMenu(MenuType.ErrorList);
						return;
					}

					if (Interface.LogMessages.Count != 0)
					{
						formMessages.ShowMessages();
						Application.DoEvents();
						Renderer.Camera.AlignmentDirection.TrackPosition = 0;
						Renderer.Camera.AlignmentDirection.Position.X = 0;
						Renderer.Camera.AlignmentDirection.Position.Y = 0;
					}
					break;
				case Key.F10:
					Renderer.RenderStatsOverlay = !Renderer.RenderStatsOverlay;
					break;
				case Key.A:
				case Key.Keypad4:
					Renderer.Camera.AlignmentDirection.Position.X = -CameraProperties.ExteriorTopSpeed * speedModified;
					break;
				case Key.D:
				case Key.Keypad6:
					Renderer.Camera.AlignmentDirection.Position.X = CameraProperties.ExteriorTopSpeed * speedModified;
					break;
				case Key.Keypad2:
					Renderer.Camera.AlignmentDirection.Position.Y = -CameraProperties.ExteriorTopSpeed * speedModified;
					break;
				case Key.Keypad8:
					Renderer.Camera.AlignmentDirection.Position.Y = CameraProperties.ExteriorTopSpeed * speedModified;
					break;
				case Key.W:
				case Key.Keypad9:
					Renderer.Camera.AlignmentDirection.TrackPosition = CameraProperties.ExteriorTopSpeed * (Program.CurrentRoute.Tracks[0].Direction == TrackDirection.Reverse ? -speedModified : speedModified);
					break;
				case Key.S:
				case Key.Keypad3:
					Renderer.Camera.AlignmentDirection.TrackPosition = -CameraProperties.ExteriorTopSpeed * (Program.CurrentRoute.Tracks[0].Direction == TrackDirection.Reverse ? -speedModified : speedModified);
					break;
				case Key.Left:
					Renderer.Camera.AlignmentDirection.Yaw = -CameraProperties.ExteriorTopAngularSpeed * speedModified;
					break;
				case Key.Right:
					Renderer.Camera.AlignmentDirection.Yaw = CameraProperties.ExteriorTopAngularSpeed * speedModified;
					break;
				case Key.Up:
					if (Renderer.CurrentInterface != InterfaceType.Normal)
					{
						Game.Menu.ProcessCommand(Translations.Command.MenuUp, 0);
					}
					else
					{
						Renderer.Camera.AlignmentDirection.Pitch = CameraProperties.ExteriorTopAngularSpeed * speedModified;
					}
					break;
				case Key.Down:
					if (Renderer.CurrentInterface != InterfaceType.Normal)
					{
						Game.Menu.ProcessCommand(Translations.Command.MenuDown, 0);
					}
					else
					{
						Renderer.Camera.AlignmentDirection.Pitch = -CameraProperties.ExteriorTopAngularSpeed * speedModified;
					}
					break;
				case Key.KeypadDivide:
					Renderer.Camera.AlignmentDirection.Roll = -CameraProperties.ExteriorTopAngularSpeed * speedModified;
					break;
				case Key.KeypadMultiply:
					Renderer.Camera.AlignmentDirection.Roll = CameraProperties.ExteriorTopAngularSpeed * speedModified;
					break;
				case Key.Keypad0:
					Renderer.Camera.AlignmentDirection.Zoom = CameraProperties.ZoomTopSpeed * speedModified;
					break;
				case Key.KeypadPeriod:
					Renderer.Camera.AlignmentDirection.Zoom = -CameraProperties.ZoomTopSpeed * speedModified;
					break;
				case Key.Keypad1:
					Program.CurrentRoute.ApplyPointOfInterest(TrackDirection.Reverse);
					break;
				case Key.Keypad7:
					Program.CurrentRoute.ApplyPointOfInterest(TrackDirection.Forwards);
					break;
				case Key.PageUp:
					JumpToStation(1);
					break;
				case Key.PageDown:
					JumpToStation(-1);
					break;
				case Key.Keypad5:
					Renderer.Camera.Reset(Program.CurrentRoute.Tracks[0].Direction == TrackDirection.Reverse);
					Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					World.UpdateAbsoluteCamera(0.0);
					Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
					break;
				case Key.F:
					Renderer.OptionWireFrame = !Renderer.OptionWireFrame;
					break;
				case Key.N:
					Renderer.OptionNormals = !Renderer.OptionNormals;
					break;
				case Key.E:
					Renderer.OptionEvents = !Renderer.OptionEvents;
					break;
				case Key.I:
					Renderer.OptionInterface = !Renderer.OptionInterface;
					break;
				case Key.M:
					//SoundManager.Mute = !SoundManager.Mute;
					break;
				case Key.Plus:
				case Key.KeypadPlus:
					if (!JumpToPositionEnabled)
					{
						JumpToPositionEnabled = true;
						JumpToPositionValue = "+";
					}
					break;
				case Key.Minus:
				case Key.KeypadMinus:
					if (!JumpToPositionEnabled)
					{
						JumpToPositionEnabled = true;
						JumpToPositionValue = "-";
					}
					break;
				case Key.Number0:
				case Key.Number1:
				case Key.Number2:
				case Key.Number3:
				case Key.Number4:
				case Key.Number5:
				case Key.Number6:
				case Key.Number7:
				case Key.Number8:
				case Key.Number9:
					if (!JumpToPositionEnabled)
					{
						JumpToPositionEnabled = true;
						JumpToPositionValue = string.Empty;
					}
					JumpToPositionValue += char.ConvertFromUtf32(48 + e.Key - Key.Number0);
					break;
				case Key.Period:
					if (!JumpToPositionEnabled)
					{
						JumpToPositionEnabled = true;
						JumpToPositionValue = "0.";
					}
					else if (JumpToPositionValue.IndexOf('.') == -1)
					{
						JumpToPositionValue += ".";
					}
					break;
				case Key.BackSpace:
					if (JumpToPositionEnabled && JumpToPositionValue.Length != 0)
					{
						JumpToPositionValue = JumpToPositionValue.Substring(0, JumpToPositionValue.Length - 1);
					}
					break;
				case Key.Enter:
					if (Renderer.CurrentInterface != InterfaceType.Normal)
					{
						Game.Menu.ProcessCommand(Translations.Command.MenuEnter, 0);
						return;
					}
					if (JumpToPositionEnabled)
					{
						if (JumpToPositionValue.Length != 0)
						{
							int direction;
							if (JumpToPositionValue[0] == '-')
							{
								JumpToPositionValue = JumpToPositionValue.Substring(1);
								direction = -1;
							}
							else if (JumpToPositionValue[0] == '+')
							{
								JumpToPositionValue = JumpToPositionValue.Substring(1);
								direction = 1;
							}
							else
							{
								direction = 0;
							}
							if (double.TryParse(JumpToPositionValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
							{
								if (value < CurrentRoute.Tracks[0].Elements[CurrentRoute.Tracks[0].Elements.Length - 1].StartingTrackPosition + 100 && value > MinimumJumpToPositionValue - 100)
								{
									if (direction != 0)
									{
										value = Program.Renderer.CameraTrackFollower.TrackPosition + direction * value;
									}

									Program.Renderer.CameraTrackFollower.UpdateAbsolute(value, true, false);
									Renderer.Camera.Alignment.TrackPosition = value;
									Renderer.Camera.Reset(Program.CurrentRoute.Tracks[0].Direction == TrackDirection.Reverse);
									World.UpdateAbsoluteCamera(0.0);
									Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
								}
							}
						}
					}
					JumpToPositionEnabled = false;
					break;
				case Key.Escape:
					if (JumpToPositionEnabled)
					{
						JumpToPositionEnabled = false;
					}
					else
					{
						if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
						{
							Program.Renderer.GameWindow.TargetRenderFrequency = 0;
							Program.Renderer.CurrentInterface = InterfaceType.Menu;
							Game.Menu.PushMenu(MenuType.GameStart);
						}
					}
					break;
				case Key.R:
					Renderer.SwitchOpenGLVersion();
					break;
				case Key.P:
					if (CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
					{
						// no WinForms support
						return;
					}
					if (CurrentRouteFile != null && CurrentlyLoading == false)
					{
						if (pathForm == null || pathForm.IsDisposed)
						{
							pathForm = new formRailPaths();
						}
						// Must be shown as a dialog box on Linux for paint events to work....
						if (CurrentHost.Platform == HostPlatform.MicrosoftWindows)
						{
							if (!pathForm.Visible)
							{
								pathForm.Show();
							}
							
						}
						else
						{
							pathForm.ShowDialog();
							Application.DoEvents();
						}

					}
					
					//pathForm.ShowDialog();
					break;
			}
		}

		internal static void KeyUpEvent(object sender, KeyboardKeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.LShift:
				case Key.RShift:
					ShiftPressed = false;
					break;
				case Key.LControl:
				case Key.RControl:
					ControlPressed = false;
					break;
				case Key.LAlt:
				case Key.RAlt:
					AltPressed = false;
					break;
				case Key.A:
				case Key.Keypad4:
				case Key.D:
				case Key.Keypad6:
					Renderer.Camera.AlignmentDirection.Position.X = 0.0;
					break;
				case Key.Keypad2:
				case Key.Keypad8:
					Renderer.Camera.AlignmentDirection.Position.Y = 0.0;
					break;
				case Key.W:
				case Key.Keypad9:
				case Key.S:
				case Key.Keypad3:
					Renderer.Camera.AlignmentDirection.TrackPosition = 0.0;
					break;
				case Key.Left:
				case Key.Right:
					Renderer.Camera.AlignmentDirection.Yaw = 0.0;
					break;
				case Key.Up:
				case Key.Down:
					Renderer.Camera.AlignmentDirection.Pitch = 0.0;
					break;
				case Key.KeypadDivide:
				case Key.KeypadMultiply:
					Renderer.Camera.AlignmentDirection.Roll = 0.0;
					break;
				case Key.Keypad0:
				case Key.KeypadPeriod:
					Renderer.Camera.AlignmentDirection.Zoom = 0.0;
					break;
			}
		}

		// update caption
		internal static void UpdateCaption() {
			if (CurrentRouteFile != null)
			{
				Renderer.GameWindow.Title = Program.CurrentlyLoading ? @"Loading: " + System.IO.Path.GetFileName(CurrentRouteFile) + " - " + Application.ProductName : System.IO.Path.GetFileName(CurrentRouteFile) + " - " + Application.ProductName;
			} else
			{
				Renderer.GameWindow.Title = Application.ProductName;
			}
		}
	}
}
