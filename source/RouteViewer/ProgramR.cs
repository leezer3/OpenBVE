// ╔═════════════════════════════════════════════════════════════╗
// ║ Program.cs for the Route Viewer                             ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using LibRender2.Cameras;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using RouteManager2;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using OpenBveApi.Objects;
using ButtonState = OpenTK.Input.ButtonState;
using Vector3 = OpenBveApi.Math.Vector3;

namespace OpenBve
{
	internal static class Program {

		// system
		internal static FileSystem FileSystem = null;

		internal static bool CpuReducedMode = false;
		internal static bool CpuAutomaticMode = true;
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

		internal static GameWindow currentGameWindow;
		internal static GraphicsMode currentGraphicsMode;

		// mouse
		private static int MouseButton;

		/// <summary>The host API used by this program.</summary>
		internal static Host CurrentHost = null;

		internal static NewRenderer Renderer;

		internal static CurrentRoute CurrentRoute;

		internal static Sounds Sounds;

		internal static TrainManager TrainManager;

			// main
		[STAThread]
		internal static void Main(string[] args)
		{
			CurrentHost = new Host();
			// file system
			FileSystem = FileSystem.FromCommandLineArgs(args, CurrentHost);
			FileSystem.CreateFileSystem();
			Renderer = new NewRenderer();
			CurrentRoute = new CurrentRoute(CurrentHost, Renderer);
			Sounds = new Sounds();
			Options.LoadOptions();
			TrainManager = new TrainManager(CurrentHost, Renderer, Interface.CurrentOptions, FileSystem);
			string error;
			if (!CurrentHost.LoadPlugins(FileSystem, Interface.CurrentOptions, out error, TrainManager, Renderer))
			{
				MessageBox.Show(error, @"OpenBVE", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			
			// command line arguments
			StringBuilder objectsToLoad = new StringBuilder();
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
									objectsToLoad.Append(args[i] + " ");
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
					System.Diagnostics.Process.Start(File, objectsToLoad.ToString());
					if (string.IsNullOrEmpty(CurrentRouteFile))
					{
						//We only supplied objects, so launch Object Viewer instead
						Environment.Exit(0);
					}
				}

			}

			var options = new ToolkitOptions();
			options.Backend = PlatformBackend.PreferX11;
			Toolkit.Init(options);
			string folder = Program.FileSystem.GetDataFolder("Languages");
			Translations.LoadLanguageFiles(folder);
			Interface.CurrentOptions.ObjectOptimizationBasicThreshold = 1000;
			Interface.CurrentOptions.ObjectOptimizationFullThreshold = 250;
			// application
			currentGraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, Interface.CurrentOptions.AntiAliasingLevel);
			if (Renderer.Screen.Width == 0 || Renderer.Screen.Height == 0)
			{
				//Duff values saved, so reset to something sensible else we crash
				Renderer.Screen.Width = 1024;
				Renderer.Screen.Height = 768;
			}
			Renderer.CameraTrackFollower = new TrackFollower(Program.CurrentHost);
			currentGameWindow = new RouteViewer(Renderer.Screen.Width, Renderer.Screen.Height, currentGraphicsMode, "Route Viewer", GameWindowFlags.Default);
			currentGameWindow.Visible = true;
			currentGameWindow.TargetUpdateFrequency = 0;
			currentGameWindow.TargetRenderFrequency = 0;
			currentGameWindow.Title = "Route Viewer";
			processCommandLineArgs = true;
			currentGameWindow.Run();
			//Unload
			Sounds.Deinitialize();
		}
		
		// load route
		internal static bool LoadRoute(Bitmap bitmap = null) {
			if (string.IsNullOrEmpty(CurrentRouteFile))
			{
				return false;
			}
			Renderer.UpdateViewport();
			bool result;
			try
			{
				Encoding encoding = TextEncoding.GetSystemEncodingFromFile(CurrentRouteFile);
				Loading.Load(CurrentRouteFile, encoding, bitmap);
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
			Renderer.Lighting.Initialize();
			Renderer.InitializeVisibility();
			return result;
		}

		// jump to station
		private static void JumpToStation(int Direction) {
			if (Direction < 0) {
				for (int i = CurrentRoute.Stations.Length - 1; i >= 0; i--) {
					if (CurrentRoute.Stations[i].Stops.Length != 0) {
						double p = CurrentRoute.Stations[i].Stops[CurrentRoute.Stations[i].Stops.Length - 1].TrackPosition;
						if (p < Program.Renderer.CameraTrackFollower.TrackPosition - 0.1) {
							Program.Renderer.CameraTrackFollower.UpdateAbsolute(p, true, false);
							Renderer.Camera.Alignment.TrackPosition = p;
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
							break;
						}
					}
				}
			}
		}

		internal static void UpdateGraphicsSettings()
		{
			if (CurrentRouteFile != null)
			{
				Program.CurrentlyLoading = true;
				Renderer.RenderScene(0.0);
				Program.currentGameWindow.SwapBuffers();
				CameraAlignment a = Renderer.Camera.Alignment;
				if (Program.LoadRoute())
				{
					Renderer.Camera.Alignment = a;
					Program.Renderer.CameraTrackFollower.UpdateAbsolute(-1.0, true, false);
					Program.Renderer.CameraTrackFollower.UpdateAbsolute(a.TrackPosition, true, false);
					Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					Renderer.UpdateVisibility(a.TrackPosition, true);
					ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
				}
				Program.CurrentlyLoading = false;
			}
		}

		internal static void MouseEvent(object sender, MouseButtonEventArgs e)
		{
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
		}

		internal static MouseState currentMouseState;
		internal static MouseState previousMouseState;

		internal static void MouseMovement()
		{
			if (MouseButton == 0)
			{
				return;
			}
			currentMouseState = Mouse.GetState();
			if (currentMouseState != previousMouseState)
			{
				Renderer.Camera.AlignmentDirection.Yaw = 0.0;
				Renderer.Camera.AlignmentDirection.Pitch = 0.0;
				Renderer.Camera.AlignmentDirection.Position.X = 0.0;
				Renderer.Camera.AlignmentDirection.Position.Y = 0.0;
				if (MouseButton == 1)
				{
					Renderer.Camera.AlignmentDirection.Yaw = 0.025 * (double) (previousMouseState.X - currentMouseState.X);
					Renderer.Camera.AlignmentDirection.Pitch = 0.025 * (double)(previousMouseState.Y - currentMouseState.Y);
				}
				else if (MouseButton == 2)
				{
					Renderer.Camera.AlignmentDirection.Position.X = 0.1 * (double)(previousMouseState.X - currentMouseState.X);
					Renderer.Camera.AlignmentDirection.Position.Y = 0.1 * (double)(previousMouseState.Y - currentMouseState.Y);
				}
				
			}
		}

		internal static void FileDrop(object sender, FileDropEventArgs e)
		{
			//Seem to need to flush the WM queue after dropping a file, else it crashes nastily after load on Mono
			Application.DoEvents();
			CurrentlyLoading = true;
			CurrentRouteFile = e.FileName;
			LoadRoute();
			ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
			CurrentlyLoading = false;
			UpdateCaption();
		}

		internal static void keyDownEvent(object sender, KeyboardKeyEventArgs e)
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
					if (CurrentRouteFile != null && CurrentlyLoading == false)
					{
						Bitmap bitmap = null;
						CurrentlyLoading = true;
						Renderer.OptionInterface = false;
						if (!Interface.CurrentOptions.LoadingBackground)
						{
							Renderer.RenderScene(0.0);
							currentGameWindow.SwapBuffers();
							bitmap = new Bitmap(Renderer.Screen.Width, Renderer.Screen.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
							BitmapData bData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
							GL.ReadPixels(0, 0, Renderer.Screen.Width, Renderer.Screen.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bData.Scan0);
							bitmap.UnlockBits(bData);
							bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
						}
						CameraAlignment a = Renderer.Camera.Alignment;
						if (LoadRoute(bitmap))
						{
							Renderer.Camera.Alignment = a;
							Program.Renderer.CameraTrackFollower.UpdateAbsolute(-1.0, true, false);
							Program.Renderer.CameraTrackFollower.UpdateAbsolute(a.TrackPosition, true, false);
							Renderer.Camera.AlignmentDirection = new CameraAlignment();
							Renderer.Camera.AlignmentSpeed = new CameraAlignment();
							Renderer.UpdateVisibility(a.TrackPosition, true);
							ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
						}
						else
						{
							Renderer.Camera.Alignment.Yaw = 0.0;
							Renderer.Camera.Alignment.Pitch = 0.0;
							Renderer.Camera.Alignment.Roll = 0.0;
							Renderer.Camera.Alignment.Position = new Vector3(0.0, 2.5, 0.0);
							Renderer.Camera.Alignment.Zoom = 0.0;
							Renderer.Camera.AlignmentDirection = new CameraAlignment();
							Renderer.Camera.AlignmentSpeed = new CameraAlignment();
							Renderer.Camera.VerticalViewingAngle = Renderer.Camera.OriginalVerticalViewingAngle;
							Renderer.UpdateViewport();
							World.UpdateAbsoluteCamera(0.0);
							Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
						}
						CurrentlyLoading = false;
						Renderer.OptionInterface = true;
						if (bitmap != null)
						{
							bitmap.Dispose();
						}
						
					}
					break;
				case Key.F7:
					if (CurrentlyLoading == true)
					{
						break;
					}
					OpenFileDialog Dialog = new OpenFileDialog();
					Dialog.CheckFileExists = true;
					Dialog.Filter = @"All Supported Routes|*.csv;*.rw;*.dat|CSV/RW files|*.csv;*.rw|Mechanik Routes|*.dat|All files|*";
					if (Dialog.ShowDialog() == DialogResult.OK)
					{
						
						Application.DoEvents();
						CurrentlyLoading = true;
						CurrentRouteFile = Dialog.FileName;
						bool canLoad = false;
						for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
						{
							if (Program.CurrentHost.Plugins[i].Route != null && Program.CurrentHost.Plugins[i].Route.CanLoadRoute(CurrentRouteFile))
							{
								canLoad = true;
								break;
							}
						}
						if (canLoad && LoadRoute())
						{
							ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
						}
						else
						{
							MessageBox.Show("No plugins found capable of loading routefile: " +Environment.NewLine + CurrentRouteFile);
							Renderer.Camera.Alignment.Yaw = 0.0;
							Renderer.Camera.Alignment.Pitch = 0.0;
							Renderer.Camera.Alignment.Roll = 0.0;
							Renderer.Camera.Alignment.Position = new Vector3(0.0, 2.5, 0.0);
							Renderer.Camera.Alignment.Zoom = 0.0;
							Renderer.Camera.AlignmentDirection = new CameraAlignment();
							Renderer.Camera.AlignmentSpeed = new CameraAlignment();
							Renderer.Camera.VerticalViewingAngle = Renderer.Camera.OriginalVerticalViewingAngle;
							Renderer.UpdateViewport();
							World.UpdateAbsoluteCamera(0.0);
							Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
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
					if (Program.CurrentlyLoading == true)
					{
						//Don't allow the user to update the settings during loading, bad idea..
						break;
					}
					if (formOptions.ShowOptions() == DialogResult.OK)
					{
						UpdateGraphicsSettings();
					}
					Application.DoEvents();
					Renderer.Camera.AlignmentDirection.TrackPosition = 0;
					Renderer.Camera.AlignmentDirection.Position.X = 0;
					Renderer.Camera.AlignmentDirection.Position.Y = 0;
					break;
				case Key.F9:
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
					CpuReducedMode = false;
					break;
				case Key.D:
				case Key.Keypad6:
					Renderer.Camera.AlignmentDirection.Position.X = CameraProperties.ExteriorTopSpeed * speedModified;
					CpuReducedMode = false;
					break;
				case Key.Keypad2:
					Renderer.Camera.AlignmentDirection.Position.Y = -CameraProperties.ExteriorTopSpeed * speedModified;
					CpuReducedMode = false;
					break;
				case Key.Keypad8:
					Renderer.Camera.AlignmentDirection.Position.Y = CameraProperties.ExteriorTopSpeed * speedModified;
					CpuReducedMode = false;
					break;
				case Key.W:
				case Key.Keypad9:
					Renderer.Camera.AlignmentDirection.TrackPosition = CameraProperties.ExteriorTopSpeed * speedModified;
					CpuReducedMode = false;
					break;
				case Key.S:
				case Key.Keypad3:
					Renderer.Camera.AlignmentDirection.TrackPosition = -CameraProperties.ExteriorTopSpeed * speedModified;
					CpuReducedMode = false;
					break;
				case Key.Left:
					Renderer.Camera.AlignmentDirection.Yaw = -CameraProperties.ExteriorTopAngularSpeed * speedModified;
					CpuReducedMode = false;
					break;
				case Key.Right:
					Renderer.Camera.AlignmentDirection.Yaw = CameraProperties.ExteriorTopAngularSpeed * speedModified;
					CpuReducedMode = false;
					break;
				case Key.Up:
					Renderer.Camera.AlignmentDirection.Pitch = CameraProperties.ExteriorTopAngularSpeed * speedModified;
					CpuReducedMode = false;
					break;
				case Key.Down:
					Renderer.Camera.AlignmentDirection.Pitch = -CameraProperties.ExteriorTopAngularSpeed * speedModified;
					CpuReducedMode = false;
					break;
				case Key.KeypadDivide:
					Renderer.Camera.AlignmentDirection.Roll = -CameraProperties.ExteriorTopAngularSpeed * speedModified;
					CpuReducedMode = false;
					break;
				case Key.KeypadMultiply:
					Renderer.Camera.AlignmentDirection.Roll = CameraProperties.ExteriorTopAngularSpeed * speedModified;
					CpuReducedMode = false;
					break;
				case Key.Keypad0:
					Renderer.Camera.AlignmentDirection.Zoom = CameraProperties.ZoomTopSpeed * speedModified;
					CpuReducedMode = false;
					break;
				case Key.KeypadPeriod:
					Renderer.Camera.AlignmentDirection.Zoom = -CameraProperties.ZoomTopSpeed * speedModified;
					CpuReducedMode = false;
					break;
				case Key.Keypad1:
					Game.ApplyPointOfInterest(-1, true);
					CpuReducedMode = false;
					break;
				case Key.Keypad7:
					Game.ApplyPointOfInterest(1, true);
					CpuReducedMode = false;
					break;
				case Key.PageUp:
					JumpToStation(1);
					CpuReducedMode = false;
					break;
				case Key.PageDown:
					JumpToStation(-1);
					CpuReducedMode = false;
					break;
				case Key.Keypad5:
					Renderer.Camera.Alignment.Yaw = 0.0;
					Renderer.Camera.Alignment.Pitch = 0.0;
					Renderer.Camera.Alignment.Roll = 0.0;
					Renderer.Camera.Alignment.Position = new Vector3(0.0, 2.5, 0.0);
					Renderer.Camera.Alignment.Zoom = 0.0;
					Renderer.Camera.AlignmentDirection = new CameraAlignment();
					Renderer.Camera.AlignmentSpeed = new CameraAlignment();
					Renderer.Camera.VerticalViewingAngle = Renderer.Camera.OriginalVerticalViewingAngle;
					Renderer.UpdateViewport();
					World.UpdateAbsoluteCamera(0.0);
					Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
					CpuReducedMode = false;
					break;
				case Key.F:
					Renderer.OptionWireFrame = !Renderer.OptionWireFrame;
					CpuReducedMode = false;
					break;
				case Key.N:
					Renderer.OptionNormals = !Renderer.OptionNormals;
					CpuReducedMode = false;
					break;
				case Key.E:
					Renderer.OptionEvents = !Renderer.OptionEvents;
					CpuReducedMode = false;
					break;
				case Key.C:
					CpuAutomaticMode = !CpuAutomaticMode;
					CpuReducedMode = false;
					break;
				case Key.I:
					Renderer.OptionInterface = !Renderer.OptionInterface;
					CpuReducedMode = false;
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
						CpuReducedMode = false;
					}
					break;
				case Key.Minus:
				case Key.KeypadMinus:
					if (!JumpToPositionEnabled)
					{
						JumpToPositionEnabled = true;
						JumpToPositionValue = "-";
						CpuReducedMode = false;
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
					CpuReducedMode = false;
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
					CpuReducedMode = false;
					break;
				case Key.BackSpace:
					if (JumpToPositionEnabled && JumpToPositionValue.Length != 0)
					{
						JumpToPositionValue = JumpToPositionValue.Substring(0, JumpToPositionValue.Length - 1);
						CpuReducedMode = false;
					}
					break;
				case Key.Enter:
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
							double value;
							if (double.TryParse(JumpToPositionValue, NumberStyles.Float, CultureInfo.InvariantCulture,out value))
							{
								if (value < CurrentRoute.Tracks[0].Elements[CurrentRoute.Tracks[0].Elements.Length - 1].StartingTrackPosition + 100 && value > MinimumJumpToPositionValue - 100)
								{
									if (direction != 0)
									{
										value = Program.Renderer.CameraTrackFollower.TrackPosition + (double)direction * value;
									}

									Program.Renderer.CameraTrackFollower.UpdateAbsolute(value, true, false);
									Renderer.Camera.Alignment.TrackPosition = value;
									World.UpdateAbsoluteCamera(0.0);
									Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
								}
							}
						}
					}
					JumpToPositionEnabled = false;
					CpuReducedMode = false;
					break;
				case Key.Escape:
					JumpToPositionEnabled = false;
					CpuReducedMode = false;
					break;
				case Key.R:
					Interface.CurrentOptions.IsUseNewRenderer = !Interface.CurrentOptions.IsUseNewRenderer;
					Renderer.Lighting.Initialize();
					break;
			}
		}

		internal static void keyUpEvent(object sender, KeyboardKeyEventArgs e)
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
			if (CurrentRouteFile != null) {
				currentGameWindow.Title = System.IO.Path.GetFileName(CurrentRouteFile) + " - " + Application.ProductName;
			} else
			{
				currentGameWindow.Title = Application.ProductName;
			}
		}
	}
}
