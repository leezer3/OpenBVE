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
using LibRender2.Menu;
using LibRender2.Screens;
using LibRender2.Trains;
using ObjectViewer.Graphics;
using ObjectViewer.Trains;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Trains;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using RouteManager2;
using TrainManager.Trains;
using ButtonState = OpenTK.Input.ButtonState;
using Control = OpenBveApi.Interface.Control;
using Vector3 = OpenBveApi.Math.Vector3;

namespace ObjectViewer {
	internal static class Program {
		internal static FileSystem FileSystem = null;

		// members
	    internal static List<string> Files = new List<string>();

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

		internal static HostInterface CurrentHost;

		internal static NewRenderer Renderer;

		internal static CurrentRoute CurrentRoute;

		internal static TrainManager TrainManager;

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
	        
	        CurrentRoute = new CurrentRoute(CurrentHost, Renderer);
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
	        
	        
	        TrainManager = new TrainManager(CurrentHost, Renderer, Interface.CurrentOptions, FileSystem);
	        if (Renderer.Screen.Width == 0 || Renderer.Screen.Height == 0)
	        {
		        Renderer.Screen.Width = 960;
		        Renderer.Screen.Height = 600;
	        }
	        if (!CurrentHost.LoadPlugins(FileSystem, Interface.CurrentOptions, out string error, TrainManager, Renderer))
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
					Files = filesToLoad;
				}
	        }

	        
	        // --- load language ---
	        string folder = Program.FileSystem.GetDataFolder("Languages");
	        Translations.LoadLanguageFiles(folder);
			GameMenu.Instance = new GameMenu();
			// initialize camera
			Renderer.GraphicsMode = new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8,Interface.CurrentOptions.AntiAliasingLevel);
	        Renderer.GameWindow = new ObjectViewer(Renderer.Screen.Width, Renderer.Screen.Height, Renderer.GraphicsMode, "Object Viewer", GameWindowFlags.Default)
	        {
		        Visible = true,
		        TargetUpdateFrequency = 0,
		        TargetRenderFrequency = 0,
		        Title = "Object Viewer"
	        };
	        Renderer.GameWindow.Run();
			// quit
			Renderer.TextureManager.UnloadAllTextures(false);

			formTrain.WaitTaskFinish();
	    }

	    // reset camera
	    

		internal static void MouseWheelEvent(object sender, MouseWheelEventArgs e)
		{
			switch (Program.Renderer.CurrentInterface)
			{
				case InterfaceType.Menu:
				case InterfaceType.GLMainMenu:
					Game.Menu.ProcessMouseScroll(e.Delta);
					break;
				default:
					if (e.Delta != 0)
					{
						double dx = -0.025 * e.Delta;
						Renderer.Camera.AbsolutePosition += dx * Renderer.Camera.AbsoluteDirection;
					}
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
		    switch (Program.Renderer.CurrentInterface)
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
					break;
		    }
            
	    }

		internal static void DragFile(object sender, FileDropEventArgs e)
		{
			Files.Add(e.FileName);
			// reset
			LightingRelative = -1.0;
			Game.Reset();
			RefreshObjects();
			Renderer.InitializeVisibility();
			Renderer.UpdateVisibility(true);
			ObjectManager.UpdateAnimatedWorldObjects(0.01, true);
			Renderer.ApplyBackgroundColor();
		}

		internal static MouseState currentMouseState;
	    internal static MouseState previousMouseState;

	    internal static void MouseMovement()
	    {
	        if (MouseButton == 0 || Program.Renderer.CurrentInterface != InterfaceType.Normal) return;
	        currentMouseState = Mouse.GetState();
	        if (currentMouseState != previousMouseState)
	        {
	            if (MouseButton == 1)
	            {
		            Renderer.Camera.AbsoluteDirection = MouseCameraDirection;
		            Renderer.Camera.AbsoluteUp = MouseCameraUp;
		            Renderer.Camera.AbsoluteSide = MouseCameraSide;
                    {
                        double dx = 0.0025 * (previousMouseState.X - currentMouseState.X);
                        Renderer.Camera.AbsoluteDirection.Rotate(Vector3.Down, dx);
                        Renderer.Camera.AbsoluteUp.Rotate(Vector3.Down, dx);
                        Renderer.Camera.AbsoluteSide.Rotate(Vector3.Down, dx);
                    }
                    {
                        double dy = 0.0025 * (previousMouseState.Y - currentMouseState.Y);
                        Renderer.Camera.AbsoluteDirection.Rotate(Renderer.Camera.AbsoluteSide, dy);
                        Renderer.Camera.AbsoluteUp.Rotate(Renderer.Camera.AbsoluteSide, dy);
                    }
	            }
	            else if(MouseButton == 2)
	            {
		            Renderer.Camera.AbsolutePosition = MouseCameraPosition;
                    double dx = -0.025 * (currentMouseState.X - previousMouseState.X);
                    Renderer.Camera.AbsolutePosition += dx * Renderer.Camera.AbsoluteSide;
                    double dy = 0.025 * (currentMouseState.Y - previousMouseState.Y);
                    Renderer.Camera.AbsolutePosition += dy * Renderer.Camera.AbsoluteUp;
	            }
	            else
	            {
		            Renderer.Camera.AbsolutePosition = MouseCameraPosition;
                    double dx = -0.025 * (currentMouseState.X - previousMouseState.X);
                    Renderer.Camera.AbsolutePosition += dx * Renderer.Camera.AbsoluteSide;
                    double dz = -0.025 * (currentMouseState.Y - previousMouseState.Y);
                    Renderer.Camera.AbsolutePosition += dz * Renderer.Camera.AbsoluteDirection;
	            }
	        }
	    }

	    internal static void RefreshObjects()
	    {
		    LightingRelative = -1.0;
			Renderer.Reset();
		    Game.Reset();
			formTrain.Instance?.DisableUI();
		    for (int i = 0; i < Files.Count; i++)
		    {
			    try
			    {
				    if(Files[i].EndsWith(".dat", StringComparison.InvariantCultureIgnoreCase) || Files[i].EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) || Files[i].EndsWith(".cfg", StringComparison.InvariantCultureIgnoreCase) || Files[i].EndsWith(".con", StringComparison.InvariantCultureIgnoreCase))
				    {
					    string currentTrain = Files[i];
						if (currentTrain.EndsWith("extensions.cfg", StringComparison.InvariantCultureIgnoreCase))
					    {
						    currentTrain = System.IO.Path.GetDirectoryName(currentTrain);
					    }
					    bool canLoad = false;
					    for (int j = 0; j < Program.CurrentHost.Plugins.Length; j++)
					    {
						    if (Program.CurrentHost.Plugins[j].Train != null && Program.CurrentHost.Plugins[j].Train.CanLoadTrain(currentTrain))
						    {
							    Control[] dummyControls = new Control[0];
								TrainManager.Trains = new List<TrainBase> { new TrainBase(TrainState.Available, TrainType.LocalPlayerTrain) };
								AbstractTrain playerTrain = TrainManager.Trains[0];
								Program.CurrentHost.Plugins[j].Train.LoadTrain(Encoding.UTF8, currentTrain, ref playerTrain, ref dummyControls);
								TrainManager.PlayerTrain = TrainManager.Trains[0];
								canLoad = true;
								break;
						    }
					    }

					    if (canLoad)
					    {
						    TrainManager.PlayerTrain.Initialize();
						    foreach (var Car in TrainManager.PlayerTrain.Cars)
						    {
							    double length = Math.Max(TrainManager.PlayerTrain.Cars[0].Length, 1);
							    Car.Move(-length);
							    Car.Move(length);
						    }
						    TrainManager.PlayerTrain.PlaceCars(0);
						    for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
						    {
							    TrainManager.PlayerTrain.Cars[j].UpdateTrackFollowers(0, true, false);
							    TrainManager.PlayerTrain.Cars[j].UpdateTopplingCantAndSpring(0.0);
							    TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Exterior);
							    TrainManager.PlayerTrain.Cars[j].FrontBogie.UpdateTopplingCantAndSpring();
							    TrainManager.PlayerTrain.Cars[j].RearBogie.UpdateTopplingCantAndSpring();
						    }
					    }
					    else
					    {
							//As we now attempt to load the train as a whole, the most likely outcome is that the train.dat file is MIA
						    Interface.AddMessage(MessageType.Critical, false, "No plugin found capable of loading file " + Files[i] + ".");
					    }
				    }
				    else
				    {
					    if (CurrentHost.LoadObject(Files[i], Encoding.UTF8, out UnifiedObject o))
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
		    Renderer.UpdateVisibility(true);
		    ObjectManager.UpdateAnimatedWorldObjects(0.01, true);
		    Program.TrainManager.UpdateTrainObjects(0.0, true);
		    Renderer.ApplyBackgroundColor();

		    if (Files.Count == 1)
		    {
			    Renderer.GameWindow.Title = "Object Viewer - " + Path.GetFileName(Files[0]);
		    }
		    else
		    {
			    Renderer.GameWindow.Title = "Object Viewer";
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
	                RefreshObjects();
	                break;
	            case Key.F7:
					{
						if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
						{
							return;
						}
						OpenFileDialog Dialog = new OpenFileDialog
					    {
				            CheckFileExists = true,
				            Multiselect = true,
				            Filter = @"All supported object files|*.csv;*.b3d;*.x;*.animated;extensions.cfg;*.l3dobj;*.l3dgrp;*.obj;*.s;train.xml;*.con|openBVE Objects|*.csv;*.b3d;*.x;*.animated;extensions.cfg;train.xml|LokSim 3D Objects|*.l3dobj;*.l3dgrp|Wavefront Objects|*.obj|Microsoft Train Simulator Files|*.s;*.con|All files|*"
			            };
						if (Dialog.ShowDialog() == DialogResult.OK)
						{
							Application.DoEvents();
							string[] f = Dialog.FileNames;
							for (int i = 0; i < f.Length; i++)
				            {
								string currentTrain = string.Empty;
								if(f[i].EndsWith(".dat", StringComparison.InvariantCultureIgnoreCase) || f[i].EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) || f[i].EndsWith(".cfg", StringComparison.InvariantCultureIgnoreCase) || f[i].EndsWith(".con", StringComparison.InvariantCultureIgnoreCase))
								{
									// only check to see if it's a train if this is a specified filetype, else we'll start loading the full train from an object in it's folder
									currentTrain = f[i].EndsWith(".con", StringComparison.InvariantCultureIgnoreCase) ? f[i] : Path.GetDirectoryName(f[i]);
								}
								for (int j = 0; j < Program.CurrentHost.Plugins.Length; j++)
					            {
									if (Program.CurrentHost.Plugins[j].Route != null && Program.CurrentHost.Plugins[j].Route.CanLoadRoute(f[i]))
						            {
							            // oops, that's actually a routefile- Let's show Route Viewer
							            string File = System.IO.Path.Combine(Application.StartupPath, "RouteViewer.exe");
							            if (System.IO.File.Exists(File))
							            {
								            System.Diagnostics.Process.Start(File, "\"" + f[i] + "\"");
							            }
						            }

						            if (Program.CurrentHost.Plugins[j].Object != null && Program.CurrentHost.Plugins[j].Object.CanLoadObject(f[i]))
						            {
							            Files.Add(f[i]);
						            }
						            if (!string.IsNullOrEmpty(currentTrain) && Program.CurrentHost.Plugins[j].Train != null && Program.CurrentHost.Plugins[j].Train.CanLoadTrain(currentTrain))
						            {
							            Files.Add(f[i]);
						            }
					            }
					            
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
	                }
	                break;
				case Key.BackSpace:
	            case Key.Delete:
		            LightingRelative = -1.0;
	                Game.Reset();
		            Files = new List<string>();
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
		            if (Renderer.CurrentInterface == InterfaceType.Normal)
		            {
			            RotateY = -1;
					}
		            else
		            {
			            Game.Menu.ProcessCommand(Translations.Command.MenuUp, 0);
		            }
	                break;
	            case Key.Down:
		            if (Renderer.CurrentInterface == InterfaceType.Normal)
		            {
			            RotateY = 1;
					}
		            else
		            {
						Game.Menu.ProcessCommand(Translations.Command.MenuDown, 0);
					}
	                break;
	            case var value when value == (Key)Interface.CurrentOptions.CameraMoveLeft:
	            case Key.Keypad4:
	                MoveX = -1;
	                break;
				case var value when value == (Key)Interface.CurrentOptions.CameraMoveRight:
				case Key.Keypad6:
	                MoveX = 1;
	                break;
				case var value when value == (Key)Interface.CurrentOptions.CameraMoveUp:
				case Key.Keypad8:
	                MoveY = 1;
	                break;
				case var value when value == (Key)Interface.CurrentOptions.CameraMoveDown:
				case Key.Keypad2:
	                MoveY = -1;
	                break;
				case var value when value == (Key)Interface.CurrentOptions.CameraMoveForward:
				case Key.Keypad9:
	                MoveZ = 1;
	                break;
				case var value when value == (Key)Interface.CurrentOptions.CameraMoveBackward:
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
	                if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
	                {
		                return;
	                }
					formOptions.ShowOptions();
                    Application.DoEvents();
                    break;
                case Key.F10:
	                if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
	                {
		                return;
	                }
					formTrain.ShowTrainSettings();
                    break;
	            case Key.G:
	            case Key.C:
	                Renderer.OptionCoordinateSystem = !Renderer.OptionCoordinateSystem;
	                break;
	            case Key.B:
	                if (ShiftPressed)
	                {
		                using (ColorDialog dialog = new ColorDialog())
		                {
			                dialog.FullOpen = true;
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
					Renderer.SwitchOpenGLVersion();
					break;
				case Key.F11:
					Renderer.RenderStatsOverlay = !Renderer.RenderStatsOverlay;
					break;
				case Key.Enter:
					if (Renderer.CurrentInterface != InterfaceType.Normal)
					{
						Game.Menu.ProcessCommand(Translations.Command.MenuEnter, 0);
					}
					break;
				case Key.Escape:
					if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
					{
						if (Renderer.CurrentInterface != InterfaceType.Normal)
						{
							Game.Menu.ProcessCommand(Translations.Command.MenuBack, 0);
						}
						else
						{
							Program.Renderer.CurrentInterface = InterfaceType.Menu;
							Game.Menu.PushMenu(MenuType.GameStart);
						}
					}
					
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
				case var value when value == (Key)Interface.CurrentOptions.CameraMoveLeft || value == (Key)Interface.CurrentOptions.CameraMoveRight:
				case Key.Keypad4:
	            case Key.Keypad6:
	                MoveX = 0;
	                break;
	            case var value when value == (Key)Interface.CurrentOptions.CameraMoveUp || value == (Key)Interface.CurrentOptions.CameraMoveDown:
				case Key.Keypad8:
	            case Key.Keypad2:
	                MoveY = 0;
	                break;
				case var value when value == (Key)Interface.CurrentOptions.CameraMoveForward || value == (Key)Interface.CurrentOptions.CameraMoveBackward:
				case Key.Keypad9:
	            case Key.Keypad3:
	                MoveZ = 0;
	                break;
	        }
	    }
	}
}
