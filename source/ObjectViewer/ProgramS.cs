// ╔═════════════════════════════════════════════════════════════╗
// ║ Program.cs for the Structure Viewer                         ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using LibRender2.Trains;
using ObjectViewer.Graphics;
using ObjectViewer.Trains;
using OpenBveApi.FileSystem;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using RouteManager2;
using TrainManager.Trains;
using ButtonState = OpenTK.Input.ButtonState;
using Control = OpenBveApi.Interface.Control;
using Vector3 = OpenBveApi.Math.Vector3;

namespace OpenBve {
	internal static class Program {
		internal static FileSystem FileSystem = null;

		// members
	    internal static string[] Files = { };

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


        internal static GameWindow currentGameWindow;
        internal static GraphicsMode currentGraphicsMode;

		internal static OpenBveApi.Hosts.HostInterface CurrentHost;

		internal static NewRenderer Renderer;

		internal static CurrentRoute CurrentRoute;

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
	        Renderer.CameraTrackFollower = new TrackFollower(CurrentHost);
	        Options.LoadOptions();
	        TrainManager = new TrainManager(CurrentHost, Renderer, Interface.CurrentOptions, FileSystem);
	        if (Renderer.Screen.Width == 0 || Renderer.Screen.Height == 0)
	        {
		        Renderer.Screen.Width = 960;
		        Renderer.Screen.Height = 600;
	        }
	        string error;
	        if (!CurrentHost.LoadPlugins(FileSystem, Interface.CurrentOptions, out error, TrainManager, Renderer))
	        {
		        MessageBox.Show(error, @"OpenBVE", MessageBoxButtons.OK, MessageBoxIcon.Error);
		        return;
	        }
	        // command line arguments
	        List<string> filesToLoad = new List<string>();
	        
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
						        if (CurrentHost.Plugins[j].Route != null && CurrentHost.Plugins[j].Route.CanLoadRoute(args[i]))
						        {
							        string File = System.IO.Path.Combine(Application.StartupPath, "RouteViewer.exe");
							        if (System.IO.File.Exists(File))
							        {
								        System.Diagnostics.Process.Start(File, args[i]);
							        }
							        continue;
						        }

						        if (CurrentHost.Plugins[j].Object != null && CurrentHost.Plugins[j].Object.CanLoadObject(args[i]))
						        {
							        filesToLoad.Add(args[i]);
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

				if (filesToLoad.Count != 0)
				{
					Files = filesToLoad.ToArray();
				}
	        }

	        var options = new ToolkitOptions
	        {
		        Backend = PlatformBackend.PreferX11
	        };
	        Toolkit.Init(options);
	        // initialize camera

	        currentGraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8,Interface.CurrentOptions.AntiAliasingLevel);
	        currentGameWindow = new ObjectViewer(Renderer.Screen.Width, Renderer.Screen.Height, currentGraphicsMode, "Object Viewer", GameWindowFlags.Default)
	        {
		        Visible = true,
		        TargetUpdateFrequency = 0,
		        TargetRenderFrequency = 0,
		        Title = "Object Viewer"
	        };
	        currentGameWindow.Run();
			// quit
			Renderer.TextureManager.UnloadAllTextures();

			formTrain.WaitTaskFinish();
	    }

	    // reset camera
	    

		internal static void MouseWheelEvent(object sender, MouseWheelEventArgs e)
		{	
			if(e.Delta != 0)
			{
				double dx = -0.025 * e.Delta;
				Renderer.Camera.AbsolutePosition += dx * Renderer.Camera.AbsoluteDirection;
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
			Array.Resize(ref Files, n + 1);
			Files[n] = e.FileName;
			// reset
			LightingRelative = -1.0;
			Game.Reset();
			RefreshObjects();
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
                        Renderer.Camera.AbsoluteDirection.Rotate(Vector3.Down, dx);
                        Renderer.Camera.AbsoluteUp.Rotate(Vector3.Down, dx);
                        Renderer.Camera.AbsoluteSide.Rotate(Vector3.Down, dx);
                    }
                    {
                        double dy = 0.0025 * (double)(previousMouseState.Y - currentMouseState.Y);
                        Renderer.Camera.AbsoluteDirection.Rotate(Renderer.Camera.AbsoluteSide, dy);
                        Renderer.Camera.AbsoluteUp.Rotate(Renderer.Camera.AbsoluteSide, dy);
                    }
	            }
	            else if(MouseButton == 2)
	            {
		            Renderer.Camera.AbsolutePosition = MouseCameraPosition;
                    double dx = -0.025 * (double)(currentMouseState.X - previousMouseState.X);
                    Renderer.Camera.AbsolutePosition += dx * Renderer.Camera.AbsoluteSide;
                    double dy = 0.025 * (double)(currentMouseState.Y - previousMouseState.Y);
                    Renderer.Camera.AbsolutePosition += dy * Renderer.Camera.AbsoluteUp;
	            }
	            else
	            {
		            Renderer.Camera.AbsolutePosition = MouseCameraPosition;
                    double dx = -0.025 * (double)(currentMouseState.X - previousMouseState.X);
                    Renderer.Camera.AbsolutePosition += dx * Renderer.Camera.AbsoluteSide;
                    double dz = -0.025 * (double)(currentMouseState.Y - previousMouseState.Y);
                    Renderer.Camera.AbsolutePosition += dz * Renderer.Camera.AbsoluteDirection;
	            }
	        }
	    }

	    internal static void RefreshObjects()
	    {
		    LightingRelative = -1.0;
		    Game.Reset();
			formTrain.Instance?.DisableUI();
		    for (int i = 0; i < Files.Length; i++)
		    {
			    try
			    {
				    if (String.Compare(System.IO.Path.GetFileName(Files[i]), "extensions.cfg", StringComparison.OrdinalIgnoreCase) == 0)
				    {
					    string currentTrainFolder = System.IO.Path.GetDirectoryName(Files[i]);

					    for (int j = 0; j < Program.CurrentHost.Plugins.Length; j++)
					    {
						    if (Program.CurrentHost.Plugins[j].Train != null && Program.CurrentHost.Plugins[j].Train.CanLoadTrain(currentTrainFolder))
						    {
							    Control[] dummyControls = new Control[0];
								TrainManager.Trains = new[] { new TrainBase(TrainState.Available) };
								AbstractTrain playerTrain = TrainManager.Trains[0];
								Program.CurrentHost.Plugins[j].Train.LoadTrain(Encoding.UTF8, currentTrainFolder, ref playerTrain, ref dummyControls);
								TrainManager.PlayerTrain = TrainManager.Trains[0];
								break;
						    }
					    }
						TrainManager.PlayerTrain.Initialize();
					    foreach (var Car in TrainManager.PlayerTrain.Cars)
					    {
						    double length = TrainManager.PlayerTrain.Cars[0].Length;
						    Car.Move(-length);
						    Car.Move(length);
					    }
					    TrainManager.PlayerTrain.PlaceCars(0);
					    for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					    {
							TrainManager.PlayerTrain.Cars[j].UpdateTrackFollowers(0, true, false);
							TrainManager.PlayerTrain.Cars[j].UpdateTopplingCantAndSpring(0.0);
							TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Exterior);
							TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
							TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
					    }
				    }
				    else
				    {
					    UnifiedObject o;
					    if (CurrentHost.LoadObject(Files[i], System.Text.Encoding.UTF8, out o))
					    {
						    o.CreateObject(Vector3.Zero, 0.0, 0.0, 0.0);
					    }
					    
				    }

			    }
			    catch (Exception ex)
			    {
				    Interface.AddMessage(MessageType.Critical, false, "Unhandled error (" + ex.Message + ") encountered while processing the file " + Files[i] + ".");
			    }
		    }

			NearestTrain.UpdateSpecs();
			NearestTrain.Apply();
			formTrain.Instance?.EnableUI();

		    Renderer.InitializeVisibility();
		    Renderer.UpdateViewingDistances(600);
		    Renderer.UpdateVisibility(0.0, true);
		    ObjectManager.UpdateAnimatedWorldObjects(0.01, true);
		    Program.TrainManager.UpdateTrainObjects(0.0, true);
		    Renderer.ApplyBackgroundColor();
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
	                RefreshObjects();
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
						    Array.Resize(ref Files, n + f.Length);
							for (int i = 0; i < f.Length; i++)
				            {
					            Files[n + i] = f[i];
				            }
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
						RefreshObjects();
					} 
					break;
	            case Key.F9:
	                if (Interface.LogMessages.Count != 0)
	                {
	                    formMessages.ShowMessages();
                        Application.DoEvents();
	                }
	                break;
	            case Key.Delete:
		            LightingRelative = -1.0;
	                Game.Reset();
		            Files = new string[] {};
					NearestTrain.UpdateSpecs();
					Renderer.ApplyBackgroundColor();
	                break;
	            case Key.Left:
	                RotateX = -1;
	                break;
	            case Key.Right:
	                RotateX = 1;
	                break;
	            case Key.Up:
	                RotateY = -1;
	                break;
	            case Key.Down:
	                RotateY = 1;
	                break;
	            case Key.A:
	            case Key.Keypad4:
	                MoveX = -1;
	                break;
	            case Key.D:
	            case Key.Keypad6:
	                MoveX = 1;
	                break;
	            case Key.Keypad8:
	                MoveY = 1;
	                break;
	            case Key.Keypad2:
	                MoveY = -1;
	                break;
	            case Key.W:
	            case Key.Keypad9:
	                MoveZ = 1;
	                break;
	            case Key.S:
	            case Key.Keypad3:
	                MoveZ = -1;
	                break;
	            case Key.Keypad5:
	                Renderer.Camera.Reset(new Vector3(-5.0, 2.5, -25.0));
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
	                break;
	            case Key.I:
	            case Key.F4:
	                Renderer.OptionInterface = !Renderer.OptionInterface;
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
	                break;
	            case Key.B:
	                if (ShiftPressed)
	                {
		                using (ColorDialog dialog = new ColorDialog {FullOpen = true})
		                {
			                if (dialog.ShowDialog() == DialogResult.OK)
			                {
				                Renderer.BackgroundColor = -1;
				                Renderer.ApplyBackgroundColor(dialog.Color.R, dialog.Color.G, dialog.Color.B);
			                }
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
	                break;
				case Key.R:
					Interface.CurrentOptions.IsUseNewRenderer = !Interface.CurrentOptions.IsUseNewRenderer;
					break;
				case Key.F11:
					Renderer.RenderStatsOverlay = !Renderer.RenderStatsOverlay;
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
