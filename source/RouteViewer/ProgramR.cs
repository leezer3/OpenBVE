// ╔═════════════════════════════════════════════════════════════╗
// ║ Program.cs for the Route Viewer                             ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;
using LibRender;
using OpenBveApi.FileSystem;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using ButtonState = OpenTK.Input.ButtonState;
using Screen = LibRender.Screen;
using Vector3 = OpenBveApi.Math.Vector3;

namespace OpenBve {
	internal static class Program {

		// system
		private static bool CurrentlyRunOnMono = false;
		internal static FileSystem FileSystem = null;

		internal static bool CpuReducedMode = false;
		internal static bool CpuAutomaticMode = true;
		internal static string CurrentRoute = null;
		internal static bool CurrentlyLoading = false;
		internal static int CurrentStation = -1;
		internal static bool JumpToPositionEnabled = false;
		internal static string JumpToPositionValue = "";
		internal static double MinimumJumpToPositionValue =  0;
		internal static bool processCommandLineArgs;
		internal static string[] commandLineArguments;
		internal static bool[] SkipArgs;

		internal static bool SoundError = false;
		
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

		// main
		[STAThread]
		internal static void Main(string[] args)
		{
			CurrentHost = new Host();
			LibRender.Renderer.currentHost = CurrentHost;
			commandLineArguments = args;
			// platform and mono
			CurrentlyRunOnMono = Type.GetType("Mono.Runtime") != null;
			// file system
			FileSystem = FileSystem.FromCommandLineArgs(args);
			FileSystem.CreateFileSystem();
			Plugins.LoadPlugins();
			// command line arguments
			SkipArgs = new bool[args.Length];
			if (args.Length != 0) {
				string File = System.IO.Path.Combine(Application.StartupPath, "ObjectViewer.exe");
				if (System.IO.File.Exists(File)) {
					int Skips = 0;
					System.Text.StringBuilder NewArgs = new System.Text.StringBuilder();
					for (int i = 0; i < args.Length; i++) {
						if (args[i] != null && System.IO.File.Exists(args[i])) {
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
			Options.LoadOptions();
			var options = new ToolkitOptions();
			options.Backend = PlatformBackend.PreferX11;
			Toolkit.Init(options);
			string folder = Program.FileSystem.GetDataFolder("Languages");
			Translations.LoadLanguageFiles(folder);
			Interface.CurrentOptions.ObjectOptimizationBasicThreshold = 1000;
			Interface.CurrentOptions.ObjectOptimizationFullThreshold = 250;
			// application
			currentGraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, Interface.CurrentOptions.AntialiasingLevel);
			currentGameWindow = new RouteViewer(LibRender.Screen.Width, LibRender.Screen.Height, currentGraphicsMode, "Route Viewer", GameWindowFlags.Default);
			currentGameWindow.Visible = true;
			currentGameWindow.TargetUpdateFrequency = 0;
			currentGameWindow.TargetRenderFrequency = 0;
			currentGameWindow.Title = "Route Viewer";
			processCommandLineArgs = true;
			currentGameWindow.Run();
			//Unload
			Sounds.Deinitialize();
		}

		// reset camera
		internal static void ResetCamera() {
			Camera.AbsolutePosition = new Vector3(0.0, 2.5, -5.0);
			Camera.AbsoluteDirection = new Vector3(-Camera.AbsolutePosition.X, -Camera.AbsolutePosition.Y, -Camera.AbsolutePosition.Z);
			Camera.AbsoluteSide = new Vector3(-Camera.AbsolutePosition.Z, 0.0, Camera.AbsolutePosition.X);
			Camera.AbsoluteDirection.Normalize();
			Camera.AbsoluteSide.Normalize();
			Camera.AbsoluteUp = Vector3.Cross(Camera.AbsoluteDirection, Camera.AbsoluteSide);
			Camera.VerticalViewingAngle = 45.0.ToRadians();
			Camera.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * Camera.VerticalViewingAngle) * Screen.AspectRatio);
			Camera.OriginalVerticalViewingAngle = Camera.VerticalViewingAngle;
		}

		// update viewport
		internal static void UpdateViewport() {
			GL.Viewport(0, 0, LibRender.Screen.Width, LibRender.Screen.Height);
			Screen.AspectRatio = (double)LibRender.Screen.Width / (double)LibRender.Screen.Height;
			Camera.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * Camera.VerticalViewingAngle) * Screen.AspectRatio);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			Matrix4d perspective =  Matrix4d.Perspective(Camera.VerticalViewingAngle, -Screen.AspectRatio, 0.2, 1000.0);
			GL.MultMatrix(ref perspective);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
		}

		// load route
		internal static bool LoadRoute() {
			
			CurrentStation = -1;
			Game.Reset();
			Renderer.Initialize();
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
			LibRender.Renderer.InitializeLighting();
			ObjectManager.InitializeVisibility();
			TextureManager.UnloadAllTextures();
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
							Camera.CurrentAlignment.TrackPosition = p;
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
							Camera.CurrentAlignment.TrackPosition = p;
							CurrentStation = i;
							break;
						}
					}
				}
			}
		}

		internal static void UpdateGraphicsSettings()
		{
			if (Program.CurrentRoute != null)
			{
				Program.CurrentlyLoading = true;
				Renderer.RenderScene(0.0);
				Program.currentGameWindow.SwapBuffers();
				CameraAlignment a = Camera.CurrentAlignment;
				TextureManager.UnloadAllTextures();
				if (Program.LoadRoute())
				{
					Camera.CurrentAlignment = a;
					TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, -1.0, true, false);
					TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, a.TrackPosition, true, false);
					Camera.AlignmentDirection = new CameraAlignment();
					Camera.AlignmentSpeed = new CameraAlignment();
					ObjectManager.UpdateVisibility(a.TrackPosition, true);
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
				Camera.AlignmentDirection.Yaw = 0.0;
				Camera.AlignmentDirection.Pitch = 0.0;
				Camera.AlignmentDirection.Position.X = 0.0;
				Camera.AlignmentDirection.Position.Y = 0.0;
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
				Camera.AlignmentDirection.Yaw = 0.0;
				Camera.AlignmentDirection.Pitch = 0.0;
				Camera.AlignmentDirection.Position.X = 0.0;
				Camera.AlignmentDirection.Position.Y = 0.0;
				if (MouseButton == 1)
				{
					Camera.AlignmentDirection.Yaw = 0.025 * (double) (previousMouseState.X - currentMouseState.X);
					Camera.AlignmentDirection.Pitch = 0.025 * (double)(previousMouseState.Y - currentMouseState.Y);
				}
				else if (MouseButton == 2)
				{
					Camera.AlignmentDirection.Position.X = 0.1 * (double)(previousMouseState.X - currentMouseState.X);
					Camera.AlignmentDirection.Position.Y = 0.1 * (double)(previousMouseState.Y - currentMouseState.Y);
				}
				
			}
		}

		internal static void FileDrop(object sender, FileDropEventArgs e)
		{
			CurrentlyLoading = true;
			CurrentRoute = e.FileName;
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
					if (CurrentRoute != null && CurrentlyLoading == false)
					{
						Bitmap bitmap = null;
						CurrentlyLoading = true;
						Renderer.OptionInterface = false;
						if (!Interface.CurrentOptions.LoadingBackground)
						{
							Renderer.RenderScene(0.0);
							currentGameWindow.SwapBuffers();
							bitmap = new Bitmap(LibRender.Screen.Width, LibRender.Screen.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
							BitmapData bData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
							GL.ReadPixels(0, 0, LibRender.Screen.Width, LibRender.Screen.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bData.Scan0);
							bitmap.UnlockBits(bData);
							bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
							LoadingScreen.SetLoadingBkg(TextureManager.RegisterTexture(bitmap, new TextureParameters(null, null)));
						}
						CameraAlignment a = Camera.CurrentAlignment;
						if (LoadRoute())
						{
							Camera.CurrentAlignment = a;
							TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, -1.0, true, false);
							TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, a.TrackPosition, true, false);
							Camera.AlignmentDirection = new CameraAlignment();
							Camera.AlignmentSpeed = new CameraAlignment();
							ObjectManager.UpdateVisibility(a.TrackPosition, true);
							ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
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
					Dialog.Filter = @"CSV/RW files|*.csv;*.rw|All files|*";
					if (Dialog.ShowDialog() == DialogResult.OK)
					{
						Application.DoEvents();
						CurrentlyLoading = true;
						CurrentRoute = Dialog.FileName;
						LoadRoute();
						ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
						CurrentlyLoading = false;
						UpdateCaption();
					}
					else
					{
						if (Program.CurrentlyRunOnMono)
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
						//Don't allow the user to update the settings during loading, bad idea....
						break;
					}
					if (formOptions.ShowOptions() == DialogResult.OK)
					{
						UpdateGraphicsSettings();
					}
					Application.DoEvents();
					break;
				case Key.F9:
					if (Interface.MessageCount != 0)
					{
						formMessages.ShowMessages();
						Application.DoEvents();
					}
					break;
				case Key.F10:
					Renderer.RenderStatsOverlay = !Renderer.RenderStatsOverlay;
					break;
				case Key.A:
				case Key.Keypad4:
					Camera.AlignmentDirection.Position.X = -Camera.ExteriorTopSpeed*speedModified;
					CpuReducedMode = false;
					break;
				case Key.D:
				case Key.Keypad6:
					Camera.AlignmentDirection.Position.X = Camera.ExteriorTopSpeed*speedModified;
					CpuReducedMode = false;
					break;
				case Key.Keypad2:
					Camera.AlignmentDirection.Position.Y = -Camera.ExteriorTopSpeed*speedModified;
					CpuReducedMode = false;
					break;
				case Key.Keypad8:
					Camera.AlignmentDirection.Position.Y = Camera.ExteriorTopSpeed*speedModified;
					CpuReducedMode = false;
					break;
				case Key.W:
				case Key.Keypad9:
					Camera.AlignmentDirection.TrackPosition = Camera.ExteriorTopSpeed*speedModified;
					CpuReducedMode = false;
					break;
				case Key.S:
				case Key.Keypad3:
					Camera.AlignmentDirection.TrackPosition = -Camera.ExteriorTopSpeed*speedModified;
					CpuReducedMode = false;
					break;
				case Key.Left:
					Camera.AlignmentDirection.Yaw = -Camera.ExteriorTopAngularSpeed*speedModified;
					CpuReducedMode = false;
					break;
				case Key.Right:
					Camera.AlignmentDirection.Yaw = Camera.ExteriorTopAngularSpeed*speedModified;
					CpuReducedMode = false;
					break;
				case Key.Up:
					Camera.AlignmentDirection.Pitch = Camera.ExteriorTopAngularSpeed*speedModified;
					CpuReducedMode = false;
					break;
				case Key.Down:
					Camera.AlignmentDirection.Pitch = -Camera.ExteriorTopAngularSpeed*speedModified;
					CpuReducedMode = false;
					break;
				case Key.KeypadDivide:
					Camera.AlignmentDirection.Roll = -Camera.ExteriorTopAngularSpeed*speedModified;
					CpuReducedMode = false;
					break;
				case Key.KeypadMultiply:
					Camera.AlignmentDirection.Roll = Camera.ExteriorTopAngularSpeed*speedModified;
					CpuReducedMode = false;
					break;
				case Key.Keypad0:
					Camera.AlignmentDirection.Zoom = Camera.ZoomTopSpeed*speedModified;
					CpuReducedMode = false;
					break;
				case Key.KeypadPeriod:
					Camera.AlignmentDirection.Zoom = -Camera.ZoomTopSpeed*speedModified;
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
					Camera.CurrentAlignment.Yaw = 0.0;
					Camera.CurrentAlignment.Pitch = 0.0;
					Camera.CurrentAlignment.Roll = 0.0;
					Camera.CurrentAlignment.Position = new Vector3(0.0, 2.5, 0.0);
					Camera.CurrentAlignment.Zoom = 0.0;
					Camera.AlignmentDirection = new CameraAlignment();
					Camera.AlignmentSpeed = new CameraAlignment();
					Camera.VerticalViewingAngle = Camera.OriginalVerticalViewingAngle;
					UpdateViewport();
					World.UpdateAbsoluteCamera(0.0);
					World.UpdateViewingDistances();
					CpuReducedMode = false;
					break;
				case Key.F:
					LibRender.Renderer.OptionWireframe = !LibRender.Renderer.OptionWireframe;
					CpuReducedMode = false;
					if (LibRender.Renderer.OptionWireframe)
					{
						GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
					}
					else
					{
						GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
					}
					break;
				case Key.N:
					LibRender.Renderer.OptionNormals = !LibRender.Renderer.OptionNormals;
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
							if (double.TryParse(JumpToPositionValue, NumberStyles.Float, CultureInfo.InvariantCulture,
								out value))
								if (value < TrackManager.CurrentTrack.Elements[TrackManager.CurrentTrack.Elements.Length - 1].StartingTrackPosition + 100 && value > MinimumJumpToPositionValue - 100)
							{
								if (direction != 0)
								{
									value = World.CameraTrackFollower.TrackPosition + (double) direction*value;
								}
								TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, value, true, false);
								Camera.CurrentAlignment.TrackPosition = value;
								World.UpdateAbsoluteCamera(0.0);
								World.UpdateViewingDistances();
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
					Camera.AlignmentDirection.Position.X = 0.0;
					break;
				case Key.Keypad2:
				case Key.Keypad8:
					Camera.AlignmentDirection.Position.Y = 0.0;
					break;
				case Key.W:
				case Key.Keypad9:
				case Key.S:
				case Key.Keypad3:
					Camera.AlignmentDirection.TrackPosition = 0.0;
					break;
				case Key.Left:
				case Key.Right:
					Camera.AlignmentDirection.Yaw = 0.0;
					break;
				case Key.Up:
				case Key.Down:
					Camera.AlignmentDirection.Pitch = 0.0;
					break;
				case Key.KeypadDivide:
				case Key.KeypadMultiply:
					Camera.AlignmentDirection.Roll = 0.0;
					break;
				case Key.Keypad0:
				case Key.KeypadPeriod:
					Camera.AlignmentDirection.Zoom = 0.0;
					break;
			}
		}

		// update caption
		internal static void UpdateCaption() {
			if (CurrentRoute != null) {
				currentGameWindow.Title = System.IO.Path.GetFileName(CurrentRoute) + " - " + Application.ProductName;
			} else
			{
				currentGameWindow.Title = Application.ProductName;
			}
		}
	}
}
