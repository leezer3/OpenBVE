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
using OpenBveApi.Objects;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
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

		internal static bool SoundError = false;

        internal static GameWindow currentGameWindow;
        internal static GraphicsMode currentGraphicsMode;

		internal static OpenBveApi.Hosts.HostInterface CurrentHost;

		internal static Object LockObj = new Object();

		// main
	    [STAThread]
	    internal static void Main(string[] args)
	    {
			CurrentlyRunOnMono = Type.GetType("Mono.Runtime") != null;
			CurrentHost = new Host();
		    
	        // file system
	        FileSystem = FileSystem.FromCommandLineArgs(args);
	        FileSystem.CreateFileSystem();
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
	        Options.LoadOptions();
	        var options = new ToolkitOptions();
	        options.Backend = PlatformBackend.PreferX11;
	        Toolkit.Init(options);
            Interface.CurrentOptions.ObjectOptimizationBasicThreshold = 1000;
	        Interface.CurrentOptions.ObjectOptimizationFullThreshold = 250;
	        Interface.CurrentOptions.AntialiasingLevel = 16;
	        Interface.CurrentOptions.AnisotropicFilteringLevel = 16;
	        // initialize camera

	        currentGraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8,Interface.CurrentOptions.AntialiasingLevel);
	        currentGameWindow = new ObjectViewer(Renderer.ScreenWidth, Renderer.ScreenHeight, currentGraphicsMode,"Object Viewer", GameWindowFlags.Default);
	        currentGameWindow.Visible = true;
	        currentGameWindow.TargetUpdateFrequency = 0;
	        currentGameWindow.TargetRenderFrequency = 0;
	        currentGameWindow.Title = "Object Viewer";
	        currentGameWindow.Run();
	        // quit
	        Textures.UnloadAllTextures();

	    }

	    // reset camera
	    internal static void ResetCamera() {
			World.AbsoluteCameraPosition = new Vector3(-5.0, 2.5, -25.0);
			World.AbsoluteCameraDirection = new Vector3(-World.AbsoluteCameraPosition.X, -World.AbsoluteCameraPosition.Y, -World.AbsoluteCameraPosition.Z);
			World.AbsoluteCameraSide = new Vector3(-World.AbsoluteCameraPosition.Z, 0.0, World.AbsoluteCameraPosition.X);
			World.AbsoluteCameraDirection.Normalize();
			World.AbsoluteCameraSide.Normalize();
			World.AbsoluteCameraUp = Vector3.Cross(World.AbsoluteCameraDirection, World.AbsoluteCameraSide);
			World.VerticalViewingAngle = 45.0 * 0.0174532925199433;
			World.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * World.VerticalViewingAngle) * World.AspectRatio);
			World.OriginalVerticalViewingAngle = World.VerticalViewingAngle;
		}

		// update viewport
		internal static void UpdateViewport() {
            GL.Viewport(0, 0, Renderer.ScreenWidth, Renderer.ScreenHeight);
            World.AspectRatio = (double)Renderer.ScreenWidth / (double)Renderer.ScreenHeight;
            World.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * World.VerticalViewingAngle) * World.AspectRatio);
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4d perspective = Matrix4d.CreatePerspectiveFieldOfView(World.VerticalViewingAngle, World.AspectRatio, 0.2, 1000.0);
            GL.LoadMatrix(ref perspective);
            GL.Scale(-1, 1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
		}

		internal static void MouseWheelEvent(object sender, MouseWheelEventArgs e)
		{	
			if(e.Delta != 0)
			{
				double dx = -0.025 * e.Delta;
				World.AbsoluteCameraPosition += dx * World.AbsoluteCameraDirection;
				ReducedMode = false;
			}
		}

	    internal static void MouseEvent(object sender, MouseButtonEventArgs e)
	    {
            MouseCameraPosition = World.AbsoluteCameraPosition;
            MouseCameraDirection = World.AbsoluteCameraDirection;
            MouseCameraUp = World.AbsoluteCameraUp;
            MouseCameraSide = World.AbsoluteCameraSide;
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
			Textures.UnloadAllTextures();
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
						ObjectManager.CreateObject(carObjects[j], new Vector3(0.0, 0.0, z),
						                           new Transformation(0.0, 0.0, 0.0), new Transformation(0.0, 0.0, 0.0), true, 0.0, 0.0, 25.0,
						                           0.0);
						if (j < train.Cars.Length - 1)
						{
							z -= (train.Cars[j].Length + train.Cars[j + 1].Length) / 2;
						}
					}
					z = 0.0;
					for (int j = 0; j < bogieObjects.Length; j++)
					{
						ObjectManager.CreateObject(bogieObjects[j], new Vector3(0.0, 0.0, z + axleLocations[j]),
							new Transformation(0.0, 0.0, 0.0), new Transformation(0.0, 0.0, 0.0), true, 0.0, 0.0, 25.0,
							0.0);
						j++;
						ObjectManager.CreateObject(bogieObjects[j], new Vector3(0.0, 0.0, z - axleLocations[j]),
							new Transformation(0.0, 0.0, 0.0), new Transformation(0.0, 0.0, 0.0), true, 0.0, 0.0, 25.0,
							0.0);
						if (j < train.Cars.Length - 1)
						{
							z -= (train.Cars[j].Length + train.Cars[j + 1].Length) / 2;
						}
					}
				}
				else
				{
					UnifiedObject o = ObjectManager.LoadObject(Files[i], System.Text.Encoding.UTF8, false, false, false);
					ObjectManager.CreateObject(o, Vector3.Zero,
					                           new Transformation(0.0, 0.0, 0.0), new Transformation(0.0, 0.0, 0.0), true, 0.0, 0.0, 25.0,
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
			ObjectManager.InitializeVisibility();
			ObjectManager.FinishCreatingObjects();
			ObjectManager.UpdateVisibility(0.0, true);
			ObjectManager.UpdateAnimatedWorldObjects(0.01, true);
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
                    World.AbsoluteCameraDirection = MouseCameraDirection;
                    World.AbsoluteCameraUp = MouseCameraUp;
                    World.AbsoluteCameraSide = MouseCameraSide;
                    {
                        double dx = 0.0025 * (double)(previousMouseState.X - currentMouseState.X);
                        double cosa = Math.Cos(dx);
                        double sina = Math.Sin(dx);
						World.AbsoluteCameraDirection.Rotate(Vector3.Down, cosa, sina);
	                    World.AbsoluteCameraUp.Rotate(Vector3.Down, cosa, sina);
	                    World.AbsoluteCameraSide.Rotate(Vector3.Down, cosa, sina);
                    }
                    {
                        double dy = 0.0025 * (double)(previousMouseState.Y - currentMouseState.Y);
                        double cosa = Math.Cos(dy);
                        double sina = Math.Sin(dy);
						World.AbsoluteCameraDirection.Rotate(World.AbsoluteCameraSide, cosa, sina);
	                    World.AbsoluteCameraUp.Rotate(World.AbsoluteCameraSide, cosa, sina);
                    }
                    ReducedMode = false;
	            }
	            else if(MouseButton == 2)
	            {
                    World.AbsoluteCameraPosition = MouseCameraPosition;
                    double dx = -0.025 * (double)(currentMouseState.X - previousMouseState.X);
                    World.AbsoluteCameraPosition += dx * World.AbsoluteCameraSide;
                    double dy = 0.025 * (double)(currentMouseState.Y - previousMouseState.Y);
                    World.AbsoluteCameraPosition += dy * World.AbsoluteCameraUp;
                    ReducedMode = false;
	            }
	            else
	            {
                    World.AbsoluteCameraPosition = MouseCameraPosition;
                    double dx = -0.025 * (double)(currentMouseState.X - previousMouseState.X);
                    World.AbsoluteCameraPosition += dx * World.AbsoluteCameraSide;
                    double dz = -0.025 * (double)(currentMouseState.Y - previousMouseState.Y);
                    World.AbsoluteCameraPosition += dz * World.AbsoluteCameraDirection;
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
	                Textures.UnloadAllTextures();
	                //Fonts.Initialize();
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
		                		ObjectManager.CreateObject(carObjects[j], new Vector3(0.0, 0.0, z),
		                		                           new Transformation(0.0, 0.0, 0.0), new Transformation(0.0, 0.0, 0.0), true, 0.0, 0.0, 25.0,
		                		                           0.0);
		                		if (j < train.Cars.Length - 1)
		                		{
		                			z -= (train.Cars[j].Length + train.Cars[j + 1].Length) / 2;
		                		}
		                	}
		                    z = 0.0;
		                    for (int j = 0; j < bogieObjects.Length; j++)
		                    {
			                    ObjectManager.CreateObject(bogieObjects[j], new Vector3(0.0, 0.0, z + axleLocations[j]),
				                    new Transformation(0.0, 0.0, 0.0), new Transformation(0.0, 0.0, 0.0), true, 0.0, 0.0, 25.0,
				                    0.0);
			                    j++;
			                    ObjectManager.CreateObject(bogieObjects[j], new Vector3(0.0, 0.0, z - axleLocations[j]),
				                    new Transformation(0.0, 0.0, 0.0), new Transformation(0.0, 0.0, 0.0), true, 0.0, 0.0, 25.0,
				                    0.0);
			                    if (j < train.Cars.Length - 1)
			                    {
				                    z -= (train.Cars[j].Length + train.Cars[j + 1].Length) / 2;
			                    }
		                    }
		                }
		                else
		                {
		                	UnifiedObject o = ObjectManager.LoadObject(Files[i], System.Text.Encoding.UTF8, false, false, false);
		                	ObjectManager.CreateObject(o, Vector3.Zero,
		                	                           new Transformation(0.0, 0.0, 0.0), new Transformation(0.0, 0.0, 0.0), true, 0.0, 0.0, 25.0,
		                	                           0.0);
		                }
#if !DEBUG
									} catch (Exception ex) {
										Interface.AddMessage(MessageType.Critical, false, "Unhandled error (" + ex.Message + ") encountered while processing the file " + Files[i] + ".");
									}
									#endif
	                }
	                ObjectManager.InitializeVisibility();
	                ObjectManager.UpdateVisibility(0.0, true);
	                ObjectManager.UpdateAnimatedWorldObjects(0.01, true);
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
			            Textures.UnloadAllTextures();
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
				            		ObjectManager.CreateObject(carObjects[j], new Vector3(0.0, 0.0, z),
				            		                           new Transformation(0.0, 0.0, 0.0), new Transformation(0.0, 0.0, 0.0), true, 0.0, 0.0, 25.0,
				            		                           0.0);
				            		if (j < train.Cars.Length - 1)
				            		{
				            			z -= (train.Cars[j].Length + train.Cars[j + 1].Length) / 2;
				            		}
				            	}
								z = 0.0;
				                for (int j = 0; j < bogieObjects.Length; j++)
				                {
					                ObjectManager.CreateObject(bogieObjects[j], new Vector3(0.0, 0.0, z + axleLocations[j]),
						                new Transformation(0.0, 0.0, 0.0), new Transformation(0.0, 0.0, 0.0), true, 0.0, 0.0, 25.0,
						                0.0);
					                j++;
					                ObjectManager.CreateObject(bogieObjects[j], new Vector3(0.0, 0.0, z - axleLocations[j]),
						                new Transformation(0.0, 0.0, 0.0), new Transformation(0.0, 0.0, 0.0), true, 0.0, 0.0, 25.0,
						                0.0);
					                if (j < train.Cars.Length - 1)
					                {
						                z -= (train.Cars[j].Length + train.Cars[j + 1].Length) / 2;
					                }
				                }
				            }
				            else
				            {
				            	UnifiedObject o = ObjectManager.LoadObject(Files[i], System.Text.Encoding.UTF8, false, false, false);
				            	ObjectManager.CreateObject(o, Vector3.Zero,
				            	                           new Transformation(0.0, 0.0, 0.0), new Transformation(0.0, 0.0, 0.0), true, 0.0, 0.0, 25.0,
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
			            ObjectManager.InitializeVisibility();
			            ObjectManager.FinishCreatingObjects();
			            ObjectManager.UpdateVisibility(0.0, true);
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
	                Textures.UnloadAllTextures();
	                //Fonts.Initialize();
	                Interface.ClearMessages();
	                Files = new string[] {};
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
	                Renderer.OptionWireframe = !Renderer.OptionWireframe;
	                if (Renderer.OptionWireframe)
	                {
	                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
	                }
	                else
	                {
	                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
	                }
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
	                    if (Renderer.BackgroundColor >= Renderer.MaxBackgroundColor)
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
