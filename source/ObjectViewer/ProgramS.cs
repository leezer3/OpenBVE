// ╔═════════════════════════════════════════════════════════════╗
// ║ Program.cs for the Structure Viewer                         ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using System.Windows.Forms;
using OpenBveApi.World;
using OpenBveApi.FileSystem;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using RouteManager2;
using ButtonState = OpenTK.Input.ButtonState;
using Vector3 = OpenBveApi.Math.Vector3;

namespace OpenBve {
	internal static class Program {


		internal static bool CurrentlyRunOnMono = false;
		internal static FileSystem FileSystem = null;

		// members
	    internal static string[] Files = new string[] { };
	    internal static bool[] SkipArgs;

		// mouse
		internal static Vector3 MouseCameraPosition = Vector3.Zero;
		internal static Vector3 MouseCameraDirection = Vector3.Forward;
		internal static Vector3 MouseCameraUp = Vector3.Down;
		internal static Vector3 MouseCameraSide = Vector3.Right;
	    internal static int MouseButton;

	    internal static int MoveX = 0;
	    internal static int MoveY = 0;
	    internal static int MoveZ = 0;
	    internal static int RotateX = 0;
	    internal static int RotateY = 0;
        internal static int LightingTarget = 1;
        internal static double LightingRelative = 1.0;
        private static bool ShiftPressed = false;
        internal static bool ReducedMode = true;


        internal static GameWindow currentGameWindow;
        internal static GraphicsMode currentGraphicsMode;

		internal static OpenBveApi.Hosts.HostInterface CurrentHost;

		internal static NewRenderer Renderer;

		internal static CurrentRoute CurrentRoute;

		internal static readonly Object LockObj = new Object();

		// main
	    [STAThread]
	    internal static void Main(string[] args)
	    {
			CurrentlyRunOnMono = Type.GetType("Mono.Runtime") != null;
			CurrentHost = new Host();
			// file system
	        FileSystem = FileSystem.FromCommandLineArgs(args);
	        FileSystem.CreateFileSystem();
	        Renderer = new NewRenderer();
	        CurrentRoute = new CurrentRoute(Renderer);
	        Options.LoadOptions();
		    Plugins.LoadPlugins();
	        // command line arguments
	        SkipArgs = new bool[args.Length];
	        if (args.Length != 0)
	        {
	            string File = System.IO.Path.Combine(Application.StartupPath, "RouteViewer.exe");
	            if (System.IO.File.Exists(File))
	            {
	                int Skips = 0;
	                System.Text.StringBuilder NewArgs = new System.Text.StringBuilder();
	                for (int i = 0; i < args.Length; i++)
	                {
	                    if (args[i] != null && System.IO.File.Exists(args[i]))
	                    {
	                        if (System.IO.Path.GetExtension(args[i]).Equals(".csv", StringComparison.OrdinalIgnoreCase))
	                        {
	                            string Text = System.IO.File.ReadAllText(args[i], System.Text.Encoding.UTF8);
	                            if (Text.Length != -1 &&
	                                Text.IndexOf("CreateMeshBuilder", StringComparison.OrdinalIgnoreCase) == -1)
	                            {
	                                if (NewArgs.Length != 0) NewArgs.Append(" ");
	                                NewArgs.Append("\"" + args[i] + "\"");
	                                SkipArgs[i] = true;
	                                Skips++;
	                            }
	                        }
	                    }
	                    else
	                    {
	                        SkipArgs[i] = true;
	                        Skips++;
	                    }
	                }
	                if (NewArgs.Length != 0)
	                {
	                    System.Diagnostics.Process.Start(File, NewArgs.ToString());
	                }
	                if (Skips == args.Length) return;
	            }
	        }
			
	        var options = new ToolkitOptions();
	        options.Backend = PlatformBackend.PreferX11;
	        Toolkit.Init(options);
            Interface.CurrentOptions.ObjectOptimizationBasicThreshold = 1000;
	        Interface.CurrentOptions.ObjectOptimizationFullThreshold = 250;
	        Interface.CurrentOptions.AntiAliasingLevel = 16;
	        Interface.CurrentOptions.AnisotropicFilteringLevel = 16;
	        // initialize camera

	        currentGraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8,Interface.CurrentOptions.AntiAliasingLevel);
	        currentGameWindow = new ObjectViewer(Renderer.Screen.Width, Renderer.Screen.Height, currentGraphicsMode,"Object Viewer", GameWindowFlags.Default);
	        currentGameWindow.Visible = true;
	        currentGameWindow.TargetUpdateFrequency = 0;
	        currentGameWindow.TargetRenderFrequency = 0;
	        currentGameWindow.Title = "Object Viewer";
	        currentGameWindow.Run();
			// quit
			Renderer.TextureManager.UnloadAllTextures();

	    }

	    // reset camera
	    internal static void ResetCamera() {
		    Renderer.Camera.AbsolutePosition = new Vector3(-5.0, 2.5, -25.0);
		    Renderer.Camera.AbsoluteDirection = new Vector3(-Renderer.Camera.AbsolutePosition.X, -Renderer.Camera.AbsolutePosition.Y, -Renderer.Camera.AbsolutePosition.Z);
		    Renderer.Camera.AbsoluteSide = new Vector3(-Renderer.Camera.AbsolutePosition.Z, 0.0, Renderer.Camera.AbsolutePosition.X);
		    Renderer.Camera.AbsoluteDirection.Normalize();
		    Renderer.Camera.AbsoluteSide.Normalize();
		    Renderer.Camera.AbsoluteUp = Vector3.Cross(Renderer.Camera.AbsoluteDirection, Renderer.Camera.AbsoluteSide);
		    Renderer.Camera.VerticalViewingAngle = 45.0.ToRadians();
		    Renderer.Camera.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * Renderer.Camera.VerticalViewingAngle) * Renderer.Screen.AspectRatio);
		    Renderer.Camera.OriginalVerticalViewingAngle = Renderer.Camera.VerticalViewingAngle;
		}

		internal static void MouseWheelEvent(object sender, MouseWheelEventArgs e)
		{	
			if(e.Delta != 0)
			{
				double dx = -0.025 * e.Delta;
				Renderer.Camera.AbsolutePosition += dx * Renderer.Camera.AbsoluteDirection;
				ReducedMode = false;
			}
		}

	    internal static void MouseEvent(object sender, MouseButtonEventArgs e)
	    {
            MouseCameraPosition = Renderer.Camera.AbsolutePosition;
            MouseCameraDirection = Renderer.Camera.AbsoluteDirection;
            MouseCameraUp = Renderer.Camera.AbsoluteUp;
            MouseCameraSide = Renderer.Camera.AbsoluteSide;
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
	    }

		internal static void DragFile(object sender, FileDropEventArgs e)
		{
			int n = Files.Length;
			Array.Resize<string>(ref Files, n + 1);
			Files[n] = e.FileName;
			// reset
			ReducedMode = false;
			LightingRelative = -1.0;
			Game.Reset();
			Renderer.TextureManager.UnloadAllTextures();
			//Fonts.Initialize();
			Interface.ClearMessages();
			for (int i = 0; i < Files.Length; i++)
			{
#if !DEBUG
				            try
				            {
#endif
				if (String.Compare(System.IO.Path.GetFileName(Files[i]), "extensions.cfg", StringComparison.OrdinalIgnoreCase) == 0)
				{
					UnifiedObject[] carObjects;
					UnifiedObject[] bogieObjects;
					TrainManager.Train train;
					double[] axleLocations;
					ExtensionsCfgParser.ParseExtensionsConfig(Files[i], System.Text.Encoding.UTF8, out carObjects, out bogieObjects, out axleLocations, out train, true);
					if (axleLocations.Length == 0)
					{
						axleLocations = new double[train.Cars.Length * 2];
						for (int j = 0; j < train.Cars.Length; j++)
						{
							double ap = train.Cars.Length * 0.4;
							axleLocations[j] = ap;
							j++;
							axleLocations[j] = -ap;
						}
					}
					double z = 0.0;
					for (int j = 0; j < carObjects.Length; j++)
					{
						Renderer.CreateObject(carObjects[j], new Vector3(0.0, 0.0, z),
						                           new Transformation(), new Transformation(), true, 0.0, 0.0, 25.0,
						                           0.0);
						if (j < train.Cars.Length - 1)
						{
							z -= (train.Cars[j].Length + train.Cars[j + 1].Length) / 2;
						}
					}
					z = 0.0;
					int trainCar = 0;
					for (int j = 0; j < bogieObjects.Length; j++)
					{
						Renderer.CreateObject(bogieObjects[j], new Vector3(0.0, 0.0, z + axleLocations[j]),
							new Transformation(), new Transformation(), true, 0.0, 0.0, 25.0,
							0.0);
						j++;
						Renderer.CreateObject(bogieObjects[j], new Vector3(0.0, 0.0, z + axleLocations[j]),
							new Transformation(), new Transformation(), true, 0.0, 0.0, 25.0,
							0.0);
						if (trainCar < train.Cars.Length - 1)
						{
							z -= (train.Cars[trainCar].Length + train.Cars[trainCar + 1].Length) / 2;
						}
						trainCar++;
					}
				}
				else
				{
					UnifiedObject o = ObjectManager.LoadObject(Files[i], System.Text.Encoding.UTF8, false);
					Renderer.CreateObject(o, Vector3.Zero,
					                           new Transformation(), new Transformation(), true, 0.0, 0.0, 25.0,
					                           0.0);
				}
#if !DEBUG
				            }
				            catch (Exception ex)
				            {
					            Interface.AddMessage(MessageType.Critical, false,
						            "Unhandled error (" + ex.Message + ") encountered while processing the file " +
						            Files[i] + ".");
				            }
#endif
			}
			Renderer.InitializeVisibility();
			Renderer.UpdateVisibility(0.0, true);
			ObjectManager.UpdateAnimatedWorldObjects(0.01, true);
			Renderer.ApplyBackgroundColor();
		}

		internal static MouseState currentMouseState;
	    internal static MouseState previousMouseState;

	    internal static void MouseMovement()
	    {
	        if (MouseButton == 0) return;
	        currentMouseState = Mouse.GetState();
	        if (currentMouseState != previousMouseState)
	        {
	            if (MouseButton == 1)
	            {
		            Renderer.Camera.AbsoluteDirection = MouseCameraDirection;
		            Renderer.Camera.AbsoluteUp = MouseCameraUp;
		            Renderer.Camera.AbsoluteSide = MouseCameraSide;
                    {
                        double dx = 0.0025 * (double)(previousMouseState.X - currentMouseState.X);
                        double cosa = Math.Cos(dx);
                        double sina = Math.Sin(dx);
                        Renderer.Camera.AbsoluteDirection.Rotate(Vector3.Down, cosa, sina);
                        Renderer.Camera.AbsoluteUp.Rotate(Vector3.Down, cosa, sina);
                        Renderer.Camera.AbsoluteSide.Rotate(Vector3.Down, cosa, sina);
                    }
                    {
                        double dy = 0.0025 * (double)(previousMouseState.Y - currentMouseState.Y);
                        double cosa = Math.Cos(dy);
                        double sina = Math.Sin(dy);
                        Renderer.Camera.AbsoluteDirection.Rotate(Renderer.Camera.AbsoluteSide, cosa, sina);
                        Renderer.Camera.AbsoluteUp.Rotate(Renderer.Camera.AbsoluteSide, cosa, sina);
                    }
                    ReducedMode = false;
	            }
	            else if(MouseButton == 2)
	            {
		            Renderer.Camera.AbsolutePosition = MouseCameraPosition;
                    double dx = -0.025 * (double)(currentMouseState.X - previousMouseState.X);
                    Renderer.Camera.AbsolutePosition += dx * Renderer.Camera.AbsoluteSide;
                    double dy = 0.025 * (double)(currentMouseState.Y - previousMouseState.Y);
                    Renderer.Camera.AbsolutePosition += dy * Renderer.Camera.AbsoluteUp;
                    ReducedMode = false;
	            }
	            else
	            {
		            Renderer.Camera.AbsolutePosition = MouseCameraPosition;
                    double dx = -0.025 * (double)(currentMouseState.X - previousMouseState.X);
                    Renderer.Camera.AbsolutePosition += dx * Renderer.Camera.AbsoluteSide;
                    double dz = -0.025 * (double)(currentMouseState.Y - previousMouseState.Y);
                    Renderer.Camera.AbsolutePosition += dz * Renderer.Camera.AbsoluteDirection;
                    ReducedMode = false;
	            }
	        }
	    }

		// process events
	    internal static void KeyDown(object sender, KeyboardKeyEventArgs e)
	    {
	        switch (e.Key)
	        {

	            case Key.LShift:
	            case Key.RShift:
	                ShiftPressed = true;
	                break;
	            case Key.F5:
	                // reset
	                ReducedMode = false;
	                LightingRelative = -1.0;
	                Game.Reset();
	                Renderer.TextureManager.UnloadAllTextures();
	                Interface.ClearMessages();
	                for (int i = 0; i < Files.Length; i++)
	                {
#if !DEBUG
									try {
										#endif
		                if (String.Compare(System.IO.Path.GetFileName(Files[i]), "extensions.cfg", StringComparison.OrdinalIgnoreCase) == 0)
		                {
		                	UnifiedObject[] carObjects;
		                	UnifiedObject[] bogieObjects;
		                    double[] axleLocations;
		                	TrainManager.Train train;
		                	ExtensionsCfgParser.ParseExtensionsConfig(Files[i], System.Text.Encoding.UTF8, out carObjects, out bogieObjects, out axleLocations, out train, true);
		                    if (axleLocations.Length == 0)
		                    {
			                    axleLocations = new double[train.Cars.Length * 2];
			                    for (int j = 0; j < train.Cars.Length; j++)
			                    {
				                    double ap = train.Cars.Length * 0.4;
				                    axleLocations[j] = ap;
				                    j++;
				                    axleLocations[j] = -ap;
			                    }
		                    }
		                	double z = 0.0;
		                    for (int j = 0; j < carObjects.Length; j++)
		                	{
			                    Renderer.CreateObject(carObjects[j], new Vector3(0.0, 0.0, z),
		                		                           new Transformation(), new Transformation(), true, 0.0, 0.0, 25.0,
		                		                           0.0);
		                		if (j < train.Cars.Length - 1)
		                		{
		                			z -= (train.Cars[j].Length + train.Cars[j + 1].Length) / 2;
		                		}
		                	}
		                    z = 0.0;
		                    int trainCar = 0;
		                    for (int j = 0; j < bogieObjects.Length; j++)
		                    {
			                    Renderer.CreateObject(bogieObjects[j], new Vector3(0.0, 0.0, z + axleLocations[j]),
				                    new Transformation(), new Transformation(), true, 0.0, 0.0, 25.0,
				                    0.0);
			                    j++;
			                    Renderer.CreateObject(bogieObjects[j], new Vector3(0.0, 0.0, z + axleLocations[j]),
				                    new Transformation(), new Transformation(), true, 0.0, 0.0, 25.0,
				                    0.0);
			                    if (trainCar < train.Cars.Length - 1)
			                    {
				                    z -= (train.Cars[trainCar].Length + train.Cars[trainCar + 1].Length) / 2;
			                    }
								trainCar++;
		                    }
		                }
		                else
		                {
		                	UnifiedObject o = ObjectManager.LoadObject(Files[i], System.Text.Encoding.UTF8, false);
		                    Renderer.CreateObject(o, Vector3.Zero,
		                	                           new Transformation(), new Transformation(), true, 0.0, 0.0, 25.0,
		                	                           0.0);
		                }
#if !DEBUG
									} catch (Exception ex) {
										Interface.AddMessage(MessageType.Critical, false, "Unhandled error (" + ex.Message + ") encountered while processing the file " + Files[i] + ".");
									}
									#endif
	                }
	                Renderer.InitializeVisibility();
	                Renderer.UpdateVisibility(0.0, true);
	                ObjectManager.UpdateAnimatedWorldObjects(0.01, true);
					Renderer.ApplyBackgroundColor();
	                break;
	            case Key.F7:
	            {
		            OpenFileDialog Dialog = new OpenFileDialog
		            {
			            CheckFileExists = true,
			            Multiselect = true,
			            Filter = @"All supported object files|*.csv;*.b3d;*.x;*.animated;extensions.cfg;*.l3dobj;*.l3dgrp;*.obj;*.s|openBVE Objects|*.csv;*.b3d;*.x;*.animated;extensions.cfg|LokSim 3D Objects|*.l3dobj;*.l3dgrp|Wavefront Objects|*.obj|Microsoft Train Simulator Objects|*.s|All files|*"
		            };
		            if (Dialog.ShowDialog() == DialogResult.OK)
		            {
			            Application.DoEvents();
			            string[] f = Dialog.FileNames;
			            int n = Files.Length;
			            Array.Resize<string>(ref Files, n + f.Length);
			            for (int i = 0; i < f.Length; i++)
			            {
				            Files[n + i] = f[i];
			            }
			            // reset
			            ReducedMode = false;
			            LightingRelative = -1.0;
			            Game.Reset();
			            Renderer.TextureManager.UnloadAllTextures();
			            Interface.ClearMessages();
			            for (int i = 0; i < Files.Length; i++)
			            {
#if !DEBUG
				            try
				            {
#endif
				            if (String.Compare(System.IO.Path.GetFileName(Files[i]), "extensions.cfg", StringComparison.OrdinalIgnoreCase) == 0)
				            {
					            UnifiedObject[] carObjects;
					            UnifiedObject[] bogieObjects;
					            TrainManager.Train train;
					            double[] axleLocations;
					            ExtensionsCfgParser.ParseExtensionsConfig(Files[i], System.Text.Encoding.UTF8, out carObjects, out bogieObjects, out axleLocations, out train, true);
					            if (axleLocations.Length == 0)
					            {
						            axleLocations = new double[train.Cars.Length * 2];
						            for (int j = 0; j < train.Cars.Length; j++)
						            {
							            double ap = train.Cars.Length * 0.4;
							            axleLocations[j] = ap;
							            j++;
							            axleLocations[j] = -ap;
						            }
					            }
					            double z = 0.0;
					            for (int j = 0; j < carObjects.Length; j++)
					            {
						            Renderer.CreateObject(carObjects[j], new Vector3(0.0, 0.0, z),
							            new Transformation(), new Transformation(), true, 0.0, 0.0, 25.0,
							            0.0);
						            if (j < train.Cars.Length - 1)
						            {
							            z -= (train.Cars[j].Length + train.Cars[j + 1].Length) / 2;
						            }
					            }
					            z = 0.0;
					            int trainCar = 0;
					            for (int j = 0; j < bogieObjects.Length; j++)
					            {
						            Renderer.CreateObject(bogieObjects[j], new Vector3(0.0, 0.0, z + axleLocations[j]),
							            new Transformation(), new Transformation(), true, 0.0, 0.0, 25.0,
							            0.0);
						            j++;
						            Renderer.CreateObject(bogieObjects[j], new Vector3(0.0, 0.0, z + axleLocations[j]),
							            new Transformation(), new Transformation(), true, 0.0, 0.0, 25.0,
							            0.0);
						            if (trainCar < train.Cars.Length - 1)
						            {
							            z -= (train.Cars[trainCar].Length + train.Cars[trainCar + 1].Length) / 2;
						            }
						            trainCar++;
					            }
				            }
				            else
				            {
				            	UnifiedObject o = ObjectManager.LoadObject(Files[i], System.Text.Encoding.UTF8, false);
				                Renderer.CreateObject(o, Vector3.Zero,
				            	                           new Transformation(), new Transformation(), true, 0.0, 0.0, 25.0,
				            	                           0.0);
				            }
#if !DEBUG
				            }
				            catch (Exception ex)
				            {
					            Interface.AddMessage(MessageType.Critical, false,
						            "Unhandled error (" + ex.Message + ") encountered while processing the file " +
						            Files[i] + ".");
				            }
#endif
			            }
			            Renderer.InitializeVisibility();
			            Renderer.UpdateVisibility(0.0, true);
			            ObjectManager.UpdateAnimatedWorldObjects(0.01, true);
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
	            }
					Renderer.ApplyBackgroundColor();
	                break;
	            case Key.F9:
	                if (Interface.MessageCount != 0)
	                {
	                    formMessages.ShowMessages();
                        Application.DoEvents();
	                }
	                break;
	            case Key.Delete:
	                ReducedMode = false;
	                LightingRelative = -1.0;
	                Game.Reset();
	                Renderer.TextureManager.UnloadAllTextures();
	                Interface.ClearMessages();
	                Files = new string[] {};
					Renderer.ApplyBackgroundColor();
	                break;
	            case Key.Left:
	                RotateX = -1;
	                ReducedMode = false;
	                break;
	            case Key.Right:
	                RotateX = 1;
	                ReducedMode = false;
	                break;
	            case Key.Up:
	                RotateY = -1;
	                ReducedMode = false;
	                break;
	            case Key.Down:
	                RotateY = 1;
	                ReducedMode = false;
	                break;
	            case Key.A:
	            case Key.Keypad4:
	                MoveX = -1;
	                ReducedMode = false;
	                break;
	            case Key.D:
	            case Key.Keypad6:
	                MoveX = 1;
	                ReducedMode = false;
	                break;
	            case Key.Keypad8:
	                MoveY = 1;
	                ReducedMode = false;
	                break;
	            case Key.Keypad2:
	                MoveY = -1;
	                ReducedMode = false;
	                break;
	            case Key.W:
	            case Key.Keypad9:
	                MoveZ = 1;
	                ReducedMode = false;
	                break;
	            case Key.S:
	            case Key.Keypad3:
	                MoveZ = -1;
	                ReducedMode = false;
	                break;
	            case Key.Keypad5:
	                ResetCamera();
	                break;
	            case Key.F:
	            case Key.F1:
		            Renderer.OptionWireFrame = !Renderer.OptionWireFrame;
	                break;
	            case Key.N:
	            case Key.F2:
		            Renderer.OptionNormals = !Renderer.OptionNormals;
	                break;
	            case Key.L:
	            case Key.F3:
	                LightingTarget = 1 - LightingTarget;
	                ReducedMode = false;
	                break;
	            case Key.I:
	            case Key.F4:
	                Renderer.OptionInterface = !Renderer.OptionInterface;
	                ReducedMode = false;
	                break;
                case Key.F8:
                    formOptions.ShowOptions();
                    Application.DoEvents();
                    break;
                case Key.F10:
                    formTrain.ShowTrainSettings();
                    break;
	            case Key.G:
	            case Key.C:
	                Renderer.OptionCoordinateSystem = !Renderer.OptionCoordinateSystem;
	                ReducedMode = false;
	                break;
	            case Key.B:
	                if (ShiftPressed)
	                {
	                    ColorDialog dialog = new ColorDialog();
	                    dialog.FullOpen = true;
	                    if (dialog.ShowDialog() == DialogResult.OK)
	                    {
	                        Renderer.BackgroundColor = -1;
	                        Renderer.ApplyBackgroundColor(dialog.Color.R, dialog.Color.G, dialog.Color.B);
	                    }
	                }
	                else
	                {
	                    Renderer.BackgroundColor++;
	                    if (Renderer.BackgroundColor >= NewRenderer.MaxBackgroundColor)
	                    {
	                        Renderer.BackgroundColor = 0;
	                    }
	                    Renderer.ApplyBackgroundColor();
	                }
	                ReducedMode = false;
	                break;
	        }
	    }

	    internal static void KeyUp(object sender, KeyboardKeyEventArgs e)
	    {
	        switch (e.Key)
	        {
	            case Key.LShift:
	            case Key.RShift:
	                ShiftPressed = false;
	                break;
	            case Key.Left:
	            case Key.Right:
	                RotateX = 0;
	                break;
	            case Key.Up:
	            case Key.Down:
	                RotateY = 0;
	                break;
	            case Key.A:
	            case Key.D:
	            case Key.Keypad4:
	            case Key.Keypad6:
	                MoveX = 0;
	                break;
	            case Key.Keypad8:
	            case Key.Keypad2:
	                MoveY = 0;
	                break;
	            case Key.W:
	            case Key.S:
	            case Key.Keypad9:
	            case Key.Keypad3:
	                MoveZ = 0;
	                break;
	        }
	    }
	}
}
