using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibRender2.Menu;
using LibRender2.Screens;
using LibRender2.Trains;
using ObjectViewer.Graphics;
using ObjectViewer.Trains;
using OpenBveApi;
using OpenBveApi.Colors;
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
	    private static readonly HashSet<string> Files = new HashSet<string>();
		/// <summary>Guards all access to the loaded file set (UI, watcher and loader threads).</summary>
		private static readonly object FilesLock = new object();
		/// <summary>True while an async object load is in progress. Input that would mutate the file set is ignored until it clears.</summary>
		/// <remarks>Single-writer convention: only the UI thread sets/clears this; watcher threads only set PendingReload.</remarks>
		internal static volatile bool IsLoading = false;
		/// <summary>Set when a reload was requested while <see cref="IsLoading"/>; consumed as a single follow-up reload once loading finishes.</summary>
		internal static volatile bool PendingReload = false;
        /// <summary>Last time the objects were reloaded (UTC).</summary>
        internal static DateTime LastReloadTime = DateTime.UtcNow;
		/// <summary>The number of failed auto-reloads</summary>
        internal static int AutoReloadFailureCounter = 0;
        private static double reloadCheckTimer;

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
	        ToolkitOptions options = new ToolkitOptions
	        {
		        Backend = PlatformBackend.PreferX11
	        };

	        if (CurrentHost.Platform == HostPlatform.MicrosoftWindows)
	        {
		        // We're managing our own DPI
		        options.EnableHighResolution = false;
		        SetProcessDPIAware();
	        }

	        Toolkit.Init(options);

			Renderer = new NewRenderer(CurrentHost, Interface.CurrentOptions, FileSystem);
			// Apply persistent sun direction
			double azimuthRad = Interface.CurrentOptions.LightAzimuth * Math.PI / 180.0;
			double elevationRad = Interface.CurrentOptions.LightElevation * Math.PI / 180.0;
			float lx = (float)(-Math.Sin(azimuthRad) * Math.Cos(elevationRad));
			float ly = (float)Math.Sin(elevationRad);
			float lz = (float)(-Math.Cos(azimuthRad) * Math.Cos(elevationRad));
			Renderer.Lighting.OptionLightPosition = new Vector3(lx, ly, lz);
	        
	        
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
							        string routeViewer = System.IO.Path.Combine(Application.StartupPath, "RouteViewer.exe");
							        if (System.IO.File.Exists(routeViewer))
							        {
								        System.Diagnostics.Process.Start(routeViewer, args[i]);
							        }
							        continue;
						        }

						        if (CurrentHost.Plugins[j].Object != null && CurrentHost.Plugins[j].Object.CanLoadObject(args[i]))
						        {
								        filesToLoad.Add(System.IO.Path.GetFullPath(args[i]));
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
				SetFiles(filesToLoad);
			}
	        }

	        
	        // --- load language ---
	        string folder = FileSystem.GetDataFolder("Languages");
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
	        Renderer.GameWindow.VSync = Interface.CurrentOptions.VerticalSynchronization ? VSyncMode.On : VSyncMode.Off;
	        if (Interface.CurrentOptions.FPSLimit > 0)
	        {
		        Renderer.GameWindow.TargetRenderFrequency = Interface.CurrentOptions.FPSLimit;
	        }
	        Renderer.GameWindow.Run();
			// quit
			Renderer.TextureManager.UnloadAllTextures(false);

			formTrain.WaitTaskFinish();
	    }

	    // reset camera
	    

		internal static void MouseWheelEvent(object sender, MouseWheelEventArgs e)
		{
			switch (Renderer.CurrentInterface)
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
			switch (Renderer.CurrentInterface)
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
					PreviousMouseState = Mouse.GetState();
					break;
		    }
            
	    }

		internal static void DragFile(object sender, FileDropEventArgs e)
		{
			if (IsLoading)
			{
				Interface.AddMessage(MessageType.Information, false, "Still loading objects, ignoring dropped file until the current load finishes.");
				return;
			}
			string fullPath = System.IO.Path.GetFullPath(e.FileName);
			if (ContainsFile(fullPath))
			{
				return;
			}
			AddFile(fullPath);
			RefreshObjectsAsync();
		}

		internal static MouseState CurrentMouseState;
	    internal static MouseState PreviousMouseState;

	    internal static void MouseMovement()
	    {
	        if (MouseButton == 0 || Renderer.CurrentInterface != InterfaceType.Normal) return;
	        CurrentMouseState = Mouse.GetState();
	        if (CurrentMouseState != PreviousMouseState)
	        {
	            if (MouseButton == 1)
	            {
		            Renderer.Camera.AbsoluteDirection = MouseCameraDirection;
		            Renderer.Camera.AbsoluteUp = MouseCameraUp;
		            Renderer.Camera.AbsoluteSide = MouseCameraSide;
					double dx = 0.0025 * (PreviousMouseState.X - CurrentMouseState.X);
					Renderer.Camera.AbsoluteDirection.Rotate(Vector3.Down, dx);
					Renderer.Camera.AbsoluteUp.Rotate(Vector3.Down, dx);
					Renderer.Camera.AbsoluteSide.Rotate(Vector3.Down, dx);
					double dy = 0.0025 * (PreviousMouseState.Y - CurrentMouseState.Y);
					Renderer.Camera.AbsoluteDirection.Rotate(Renderer.Camera.AbsoluteSide, dy);
					Renderer.Camera.AbsoluteUp.Rotate(Renderer.Camera.AbsoluteSide, dy);
				}
	            else if(MouseButton == 2)
	            {
		            Renderer.Camera.AbsolutePosition = MouseCameraPosition;
                    double dx = -0.025 * (CurrentMouseState.X - PreviousMouseState.X);
                    Renderer.Camera.AbsolutePosition += dx * Renderer.Camera.AbsoluteSide;
                    double dy = 0.025 * (CurrentMouseState.Y - PreviousMouseState.Y);
                    Renderer.Camera.AbsolutePosition += dy * Renderer.Camera.AbsoluteUp;
	            }
	            else
	            {
		            Renderer.Camera.AbsolutePosition = MouseCameraPosition;
                    double dx = -0.025 * (CurrentMouseState.X - PreviousMouseState.X);
                    Renderer.Camera.AbsolutePosition += dx * Renderer.Camera.AbsoluteSide;
                    double dz = -0.025 * (CurrentMouseState.Y - PreviousMouseState.Y);
                    Renderer.Camera.AbsolutePosition += dz * Renderer.Camera.AbsoluteDirection;
	            }
	        }
	    }

		/// <summary>Takes a thread-safe snapshot of the loaded file list.</summary>
		internal static List<string> SnapshotFiles()
		{
			lock (FilesLock)
			{
				return new List<string>(Files);
			}
		}

		/// <summary>Thread-safe loaded file count.</summary>
		internal static int FileCount
		{
			get
			{
				lock (FilesLock)
				{
					return Files.Count;
				}
			}
		}

		/// <summary>Thread-safe check whether a file is in the loaded set.</summary>
		internal static bool ContainsFile(string file)
		{
			lock (FilesLock)
			{
				return Files.Contains(file);
			}
		}

		/// <summary>Thread-safe add to the loaded file set.</summary>
		internal static void AddFile(string file)
		{
			lock (FilesLock)
			{
				Files.Add(file);
			}
		}

		/// <summary>Thread-safe replace of the whole loaded file set.</summary>
		internal static void SetFiles(IEnumerable<string> files)
		{
			// Copy before locking so a lazy enumerable never runs under FilesLock.
			List<string> copy = new List<string>(files);
			lock (FilesLock)
			{
				Files.Clear();
				Files.UnionWith(copy);
			}
		}

		/// <summary>Thread-safe clear of the loaded file set.</summary>
		internal static void ClearFiles()
		{
			lock (FilesLock)
			{
				Files.Clear();
			}
		}

    internal static void RefreshObjects(bool autoReload = false, bool quietTiming = false)
    {
    if (!quietTiming)
	    {
		    loadTimer = System.Diagnostics.Stopwatch.StartNew();
		    ResetLoadMetrics();
	    }
	    LightingRelative = -1.0;
			// Prune cache to allow actual reloading of modified files
			CurrentHost.PruneStaleStaticObjects();
			CurrentHost.ClearAnimatedObjectCache();
			// Let TextureManager check for texture changes
			Renderer.TextureManager.UnloadAllTextures(true);

			Renderer.Reset();
		    Game.Reset();
			formTrain.Instance?.DisableUI();
			List<string> filesToRefresh = SnapshotFiles();
			foreach (string currentFile in filesToRefresh)
			{
			    try
			    {
				    if(currentFile.EndsWith(".dat", StringComparison.InvariantCultureIgnoreCase) || currentFile.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase) || currentFile.EndsWith(".cfg", StringComparison.InvariantCultureIgnoreCase) || currentFile.EndsWith(".con", StringComparison.InvariantCultureIgnoreCase))
				    {
					    string currentTrain = currentFile;
						if (currentTrain.EndsWith("extensions.cfg", StringComparison.InvariantCultureIgnoreCase) || currentTrain.EndsWith("train.dat"))
					    {
						    currentTrain = System.IO.Path.GetDirectoryName(currentTrain);
					    }
					    bool trainLoaded = false;
					    for (int j = 0; j < Program.CurrentHost.Plugins.Length; j++)
					    {
						    if (Program.CurrentHost.Plugins[j].Train != null && Program.CurrentHost.Plugins[j].Train.CanLoadTrain(currentTrain))
						    {
							    Control[] dummyControls = Array.Empty<Control>();
								TrainManager.Trains = new List<TrainBase> { new TrainBase(TrainState.Available, TrainType.LocalPlayerTrain) };
								AbstractTrain playerTrain = TrainManager.Trains[0];
								Program.CurrentHost.Plugins[j].Train.LoadTrain(Encoding.UTF8, currentTrain, ref playerTrain, ref dummyControls);
								TrainManager.PlayerTrain = TrainManager.Trains[0];
								trainLoaded = true;

								TrainManager.PlayerTrain.Initialize();
								foreach (var Car in TrainManager.PlayerTrain.Cars)
								{
									double length = Math.Max(TrainManager.PlayerTrain.Cars[0].Length, 1);
									Car.Move(-length);
									Car.Move(length);
								}
								TrainManager.PlayerTrain.PlaceCars(0);
								
								for (int k = 0; k < TrainManager.PlayerTrain.Cars.Length; k++)
								{
									TrainManager.PlayerTrain.Cars[k].UpdateTrackFollowers(0, true, false);
									TrainManager.PlayerTrain.Cars[k].UpdateTopplingCantAndSpring(0.0);
									TrainManager.PlayerTrain.Cars[k].ChangeCarSection(CarSectionType.Exterior);
									TrainManager.PlayerTrain.Cars[k].FrontBogie.UpdateTopplingCantAndSpring();
									TrainManager.PlayerTrain.Cars[k].RearBogie.UpdateTopplingCantAndSpring();
								}
								break;
						    }
					    }

					    if (!trainLoaded)
					    {
							//As we now attempt to load the train as a whole, the most likely outcome is that the train.dat file is MIA
						    Interface.AddMessage(MessageType.Critical, false, "No plugin found capable of loading file " + currentFile + ".");
					    }
				    }
				    else
				    {
					    if (CurrentHost.LoadObject(currentFile, Encoding.UTF8, out UnifiedObject o))
					    {
						    o.CreateObject(Vector3.Zero, new ObjectCreationParameters());
					    }
					    
				    }

			    }
			    catch (Exception ex)
			    {
				    if (!autoReload || AutoReloadFailureCounter > 5)
				    {
					    if (autoReload)
					    {
							// failure counter must be above 5
							Interface.CurrentOptions.AutoReloadObjects = false;
							Interface.AddMessage(MessageType.Critical, false, "Stopped automatically re-loading objects due to the Unhandled error (" + ex.Message + ") encountered while processing the file " + currentFile + ".");
						}
					    else
					    {
						    Interface.AddMessage(MessageType.Critical, false, "Unhandled error (" + ex.Message + ") encountered while processing the file " + currentFile + ".");
						}
							
				    }
				    else
				    {
					    AutoReloadFailureCounter++;
					    // If auto-reloading, a failure likely means the file is locked / being written to
					    // So we don't update the last reload time, and try again in 0.5s
					    return;
				    }
			    }
		    }

		    AutoReloadFailureCounter = 0;

			NearestTrain.UpdateSpecs();
			NearestTrain.Apply();
			formTrain.Instance?.EnableUI();

		    Renderer.InitializeVisibility();
		    Renderer.UpdateViewingDistances(600);
		    Renderer.UpdateVisibility(true);
		    ObjectManager.UpdateAnimatedWorldObjects(0.01, true);
		    Program.TrainManager.UpdateTrainObjects(0.0, true);
		    Renderer.ApplyBackgroundColor();

		    if (filesToRefresh.Count == 1)
		    {
			    Renderer.GameWindow.Title = "Object Viewer - " + Path.GetFileName(filesToRefresh[0]);
		    }
		    else
		    {
			    Renderer.GameWindow.Title = "Object Viewer";
		    }
	    LastReloadTime = DateTime.UtcNow;
		UpdateWatchers();
		if (!quietTiming)
		{
			loadTimer.Stop();
			Interface.AddMessage(MessageType.Information, false, FormatLoadMessage(loadTimer.ElapsedMilliseconds, -1, -1, true));
		}
    }

		/// <summary>Resets the per-load timing metrics. UI thread only (no loader running).</summary>
		private static void ResetLoadMetrics()
		{
			CurrentHost.TextureDecodeCalls = 0;
			CurrentHost.TextureDecodeMs = 0;
			CurrentHost.PluginObjectLoadTime = 0;
			LibRender2.Textures.TextureManager.UploadCount = 0;
			LibRender2.Textures.TextureManager.UploadMs = 0;
		}

		/// <summary>Formats the RouteViewer-style load-timing message. Pass -1 for decode/commit on the synchronous path.</summary>
		private static string FormatLoadMessage(long totalMs, long decodeMs, long commitMs, bool synchronous)
		{
			// Interlocked reads: 64-bit fields are written from worker threads.
			long textureMs = System.Threading.Interlocked.Read(ref CurrentHost.TextureDecodeMs);
			long textureCalls = System.Threading.Interlocked.Read(ref CurrentHost.TextureDecodeCalls);
			long uploadMs = System.Threading.Interlocked.Read(ref LibRender2.Textures.TextureManager.UploadMs);
			long uploadCalls = System.Threading.Interlocked.Read(ref LibRender2.Textures.TextureManager.UploadCount);
			string message = "Objects loaded in " + totalMs + " ms" + (synchronous ? " (synchronous)" : "") +
				" | textures: " + textureMs + " ms decode (" + textureCalls + " calls)" +
				" / " + uploadMs + " ms upload (" + uploadCalls + " calls)";
			if (!synchronous)
			{
				message += " | decode: " + decodeMs + " ms | commit: " + commitMs + " ms";
			}
			return message + ".";
		}

		/// <summary>Maximum file count that still takes the synchronous load path.</summary>
		internal const int MaxSyncFiles = 3;
		/// <summary>Maximum total MeshBuilder count (across files) that still takes the synchronous load path.</summary>
		internal const int MaxSyncBuilders = 10;

		// Async load state. Written on the UI thread before the worker starts (loadFiles,
		// loadResults, loadTotal), written by workers (loadResults slots, loadDoneCount via
		// Interlocked), published by LoadDone (volatile). The commit runs on the UI thread.
		private static volatile bool LoadDone = false;
		private static List<string> loadFiles = null;
		private static UnifiedObject[] loadResults = null;
		private static int loadTotal = 0;
		private static long loadDoneCount = 0;
		private static bool loadWasAutoReload = false;
		/// <summary>True when the background work is train cache-warming (commit = sync RefreshObjects).</summary>
		private static bool loadIsTrain = false;
		private static string loadFatalError = null;
		private static readonly object loadErrorLock = new object();
		private static readonly List<KeyValuePair<string, string>> loadErrors = new List<KeyValuePair<string, string>>();
		/// <summary>Wall-clock timer for the current load (kickoff to finished commit).</summary>
		private static System.Diagnostics.Stopwatch loadTimer = null;

	    /// <summary>Reloads objects, using the synchronous path for small loads and a
	    /// parallel-decode / serial-commit path for larger ones.</summary>
	    internal static void RefreshObjectsAsync(bool autoReload = false)
	    {
	    if (IsLoading)
	    {
		    Interface.AddMessage(MessageType.Information, false, "Still loading objects, ignoring reload until the current load finishes.");
		    return;
	    }
    loadTimer = System.Diagnostics.Stopwatch.StartNew();
    ResetLoadMetrics();
    List<string> snapshot = SnapshotFiles();
    if (snapshot.Count == 0)
    {
	    RefreshObjects(autoReload);
	    return;
    }
    bool hasTrainDesc = snapshot.Any(IsTrainDescriptor);
    bool hasOutOfScope = snapshot.Any(path => !IsTrainDescriptor(path) && !IsAsyncCapable(path));
    if (hasOutOfScope)
    {
	    // train.xml, standalone .animated/.x/... : synchronous for now (train.xml later).
	    // Only b3d/csv are proven GL-free on workers (other parsers may issue GL
	    // uploads mid-parse, e.g. transparency textures).
	    RefreshObjects(autoReload);
	    return;
    }
    if (hasTrainDesc)
    {
	    // Train consist: warm the object cache in parallel, then run the unchanged
	    // synchronous train load (cache-hot). extensions.cfg may point at .animated,
	    // .b3d or .csv meshes; .animated decode is parse-only (no GL registration).
	    List<string> warmPaths = CollectTrainObjectPaths(snapshot);
	    if (warmPaths.Count < MaxSyncFiles)
	    {
		    RefreshObjects(autoReload);
		    return;
	    }
	    BeginBackgroundLoad(warmPaths, true, autoReload);
	    return;
    }
	    if (snapshot.Count <= MaxSyncFiles && CountMeshBuilders(snapshot) < MaxSyncBuilders)
	    {
		    RefreshObjects(autoReload);
		    return;
	    }

		BeginBackgroundLoad(snapshot, false, autoReload);
    }

		/// <summary>Shared async kickoff on the UI thread: backdrop, teardown deferral,
		/// state init, worker start. Commit happens via <see cref="PumpLoader"/>.</summary>
		private static void BeginBackgroundLoad(List<string> workItems, bool trainLoad, bool autoReload)
		{
	    LightingRelative = -1.0;

			// Prune cache to allow actual reloading of modified files
			CurrentHost.PruneStaleStaticObjects();
			CurrentHost.ClearAnimatedObjectCache();

			// Freeze the last live frame as the loading backdrop (render thread, before teardown).
			CaptureLoadingBackground();
			// Let TextureManager check for texture changes
			Renderer.TextureManager.UnloadAllTextures(true);

			// NOTE: Renderer.Reset() / Game.Reset() are deferred to the commit so the old
			// scene graph survives until the atomic swap (no blank screen; backdrop covers it).
			// (Train commit runs RefreshObjects, which performs its own teardown.)
			formTrain.Instance?.DisableUI();

			loadFiles = workItems;
			loadResults = new UnifiedObject[workItems.Count];
			loadTotal = workItems.Count;
			loadDoneCount = 0;
			loadWasAutoReload = autoReload;
			loadIsTrain = trainLoad;
			loadFatalError = null;
			lock (loadErrorLock)
			{
				loadErrors.Clear();
			}
		LoadDone = false;
		IsLoading = true;
		Renderer.GameWindow.Title = "Object Viewer - " + (trainLoad ? "Loading train objects... 0/" : "Loading objects... 0/") + loadTotal;
		Task.Run(() => DecodeLoop());
    }

		/// <summary>Checks whether a path is a BVE train descriptor (folder-based train load).</summary>
		private static bool IsTrainDescriptor(string path)
		{
			string name = System.IO.Path.GetFileName(path);
			return name.Equals("extensions.cfg", StringComparison.InvariantCultureIgnoreCase)
				|| name.Equals("train.dat", StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>Mesh extensions worth pre-decoding (decode is CPU-only for these parsers).</summary>
		private static readonly string[] WarmObjectExtensions = { ".b3d", ".csv", ".animated" };

		/// <summary>Collects warmable mesh paths for a train load: standalone b3d/csv files
		/// plus object paths referenced by extensions.cfg files. Mirrors the parser's path
		/// resolution (folder-relative, ';' comments stripped, File.Exists gate); anything
		/// missed simply loads at sync speed inside the parser. Thread: UI (fast text scan).</summary>
		private static List<string> CollectTrainObjectPaths(List<string> snapshot)
		{
			HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			List<string> paths = new List<string>();
			foreach (string file in snapshot)
			{
				if (IsAsyncCapable(file) && seen.Add(file))
				{
					paths.Add(file);
				}
			}
			foreach (string file in snapshot)
			{
				if (!IsTrainDescriptor(file))
				{
					continue;
				}
				string folder;
				try
				{
					folder = System.IO.Path.GetDirectoryName(file);
				}
				catch
				{
					continue;
				}
				if (string.IsNullOrEmpty(folder))
				{
					continue;
				}
				string cfg = System.IO.Path.Combine(folder, "extensions.cfg");
				if (!System.IO.File.Exists(cfg))
				{
					continue;
				}
				string[] lines;
				try
				{
					lines = System.IO.File.ReadAllLines(cfg);
				}
				catch
				{
					continue;
				}
				foreach (string rawLine in lines)
				{
					string line = rawLine;
					int comment = line.IndexOf(';');
					if (comment >= 0)
					{
						line = line.Substring(0, comment);
					}
					line = line.Trim();
					if (line.Length == 0 || (line.StartsWith("[") && line.EndsWith("]")))
					{
						continue;
					}
					int equals = line.IndexOf('=');
					if (equals < 0)
					{
						continue;
					}
					string value = line.Substring(equals + 1).Trim().Trim('"');
					if (value.Length == 0)
					{
						continue;
					}
					string ext = System.IO.Path.GetExtension(value);
					bool warm = false;
					foreach (string warmExt in WarmObjectExtensions)
					{
						if (ext.Equals(warmExt, StringComparison.InvariantCultureIgnoreCase))
						{
							warm = true;
							break;
						}
					}
					if (!warm)
					{
						continue;
					}
					string full;
					try
					{
						full = System.IO.Path.IsPathRooted(value) ? value : System.IO.Path.Combine(folder, value);
					}
					catch
					{
						continue;
					}
					if (System.IO.File.Exists(full) && seen.Add(full))
					{
						paths.Add(full);
					}
				}
			}
			return paths;
		}

		/// <summary>Checks whether a path may take the async path (b3d/csv via the CsvB3d
		/// parser, proven to issue no GL calls during decode).</summary>
		private static bool IsAsyncCapable(string path)
		{
			return path.EndsWith(".b3d", StringComparison.InvariantCultureIgnoreCase)
				|| path.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>Counts MeshBuilder sections across b3d/csv files via a cheap raw-byte
		/// substring scan. Unknown-cost files force the async path.</summary>
		private static int CountMeshBuilders(List<string> files)
		{
			int total = 0;
			foreach (string file in files)
			{
				string ext = System.IO.Path.GetExtension(file);
				if (!ext.Equals(".b3d", StringComparison.InvariantCultureIgnoreCase)
					&& !ext.Equals(".csv", StringComparison.InvariantCultureIgnoreCase))
				{
					return MaxSyncBuilders;
				}
				try
				{
					total += CountToken(System.IO.File.ReadAllBytes(file));
					if (total >= MaxSyncBuilders)
					{
						return total;
					}
				}
				catch
				{
					// Unreadable here just means the async path (which reports per-file errors).
					return MaxSyncBuilders;
				}
			}
			return total;
		}

		private static readonly byte[] MeshBuilderToken = Encoding.ASCII.GetBytes("meshbuilder");

		/// <summary>Case-insensitive raw-byte occurrence count (ASCII letters only).</summary>
		private static int CountToken(byte[] data)
		{
			int count = 0;
			for (int i = 0; i + MeshBuilderToken.Length <= data.Length; i++)
			{
				bool match = true;
				for (int j = 0; j < MeshBuilderToken.Length; j++)
				{
					byte b = data[i + j];
					if (b >= (byte)'A' && b <= (byte)'Z')
					{
						b += 32;
					}
					if (b != MeshBuilderToken[j])
					{
						match = false;
						break;
					}
				}
				if (match)
				{
					count++;
				}
			}
			return count;
		}

		/// <summary>Renders the current scene and freezes it as the loading-screen backdrop.</summary>
		/// <remarks>Must run on the render thread with the old scene still intact.</remarks>
		private static void CaptureLoadingBackground()
		{
			Renderer.Loading.InitLoading(FileSystem.GetDataFolder("In-game"), typeof(NewRenderer).Assembly.GetName().Version.ToString(), true, Interface.CurrentOptions.LoadingProgressBar);
			if (Renderer.Screen.Width <= 0 || Renderer.Screen.Height <= 0)
			{
				// Minimized window: skip capture, the default logo backdrop applies.
				return;
			}
			byte[] textureBytes = null;
			try
			{
				Renderer.RenderScene(0.0);
				Renderer.GameWindow.SwapBuffers();
				textureBytes = new byte[Renderer.Screen.Width * Renderer.Screen.Height * 4];
				OpenTK.Graphics.OpenGL.GL.ReadPixels(0, 0, Renderer.Screen.Width, Renderer.Screen.Height, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, OpenTK.Graphics.OpenGL.PixelType.UnsignedByte, textureBytes);
				// GL.ReadPixels is bottom-up; flip rows for texture use.
				byte[] tmp = new byte[Renderer.Screen.Width * 4];
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
			catch
			{
				textureBytes = null;
			}
			if (textureBytes != null && textureBytes.Length > 0)
			{
				OpenBveApi.Textures.Texture t = new OpenBveApi.Textures.Texture(Renderer.Screen.Width, Renderer.Screen.Height, OpenBveApi.Textures.PixelFormat.RGBAlpha, textureBytes, (Color24[])null);
				Renderer.Loading.SetLoadingBkg(t);
			}
		}

		/// <summary>Decode progress in [0,1] for the loading screen. Thread-safe.</summary>
		internal static double LoadProgress
		{
			get
			{
				int total = loadTotal;
				if (total <= 0)
				{
					return 0.0;
				}
				long done = Interlocked.Read(ref loadDoneCount);
				return Math.Max(0.0, Math.Min(1.0, (double)done / total));
			}
		}

		/// <summary>Draws the loading screen over the frozen backdrop. Render thread only.</summary>
		internal static void DrawLoaderScreen()
		{
			Renderer.Loading.DrawLoadingScreen(Renderer.Fonts.SmallFont, LoadProgress);
			Renderer.GameWindow.SwapBuffers();
		}

		/// <summary>Worker count for parallel decode (mirrors the route plugin formula).</summary>
		private static int ComputeObjectLoadDop()
		{
			int cpu = Environment.ProcessorCount;
			if (cpu <= 4)
			{
				return Math.Max(1, cpu - 1);
			}
			return Math.Min(8, cpu);
		}

		/// <summary>Background decode: parse only, no GL. Runs on worker threads.</summary>
		private static void DecodeLoop()
		{
			System.Diagnostics.Stopwatch decodeTimer = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				int n = loadFiles.Count;
				int dop = ComputeObjectLoadDop();
				if (dop <= 1 || n < 2)
				{
					for (int i = 0; i < n; i++)
					{
						DecodeOne(i);
					}
				}
				else
				{
					// Workers only call Host.LoadObject, whose shared state (object caches,
					// failure sets, texture registration, log) is lock-guarded. Warmed parsers
					// (CsvB3d, Animated) are stateless per call: per-file locals, read-only
					// config, no GL (CsvB3d never sets TransparencyTexture; .animated decode
					// is parse-only, its GL registration runs at commit on this thread).
					ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = dop };
					Parallel.For(0, n, options, DecodeOne);
				}
			}
			catch (Exception ex)
			{
				loadFatalError = ex.Message;
			}
			finally
			{
				decodeTimer.Stop();
				CurrentHost.PluginObjectLoadTime = decodeTimer.ElapsedMilliseconds;
				LoadDone = true;
			}
		}

		/// <summary>Decodes a single file. Runs on a worker thread; never touches GL.</summary>
		private static void DecodeOne(int index)
		{
			try
			{
				string file = loadFiles[index];
				if (CurrentHost.LoadObject(file, Encoding.UTF8, out UnifiedObject o))
				{
					loadResults[index] = o;
				}
				// A false return was already reported via the (locked) log by the host/plugins.
			}
			catch (Exception ex)
			{
				lock (loadErrorLock)
				{
					loadErrors.Add(new KeyValuePair<string, string>(loadFiles[index], ex.Message));
				}
			}
			finally
			{
				Interlocked.Increment(ref loadDoneCount);
			}
		}

		/// <summary>Pumps the async loader. Called every update frame on the UI thread.</summary>
		internal static void PumpLoader()
		{
			if (!IsLoading)
			{
				return;
			}
		if (!LoadDone)
		{
			Renderer.GameWindow.Title = "Object Viewer - " + (loadIsTrain ? "Loading train objects... " : "Loading objects... ") + Interlocked.Read(ref loadDoneCount) + "/" + loadTotal;
			return;
		}
		if (loadIsTrain)
		{
			CommitTrain();
		}
		else
		{
			CommitAll();
		}
	}

		/// <summary>Serial commit on the UI/render thread: GL upload, then finalization.
		/// Mirrors the tail of <see cref="RefreshObjects"/>.</summary>
		private static void CommitAll()
		{
			try
			{
				// DrawLoaderScreen flips this to LoadScreen every frame; restore first so a
				// teardown failure below cannot leave the interface stuck.
				Renderer.CurrentInterface = InterfaceType.Normal;
				// Atomic swap: dismiss the loading screen, tear down the old scene, then commit.
				// (Reset() unloads textures internally; the kickoff unload covers decode freshness.)
				Renderer.Loading.CompleteLoading();
				Renderer.Reset();
				Game.Reset();
				// Commit time covers object creation + finalization only (teardown belongs to neither phase).
				System.Diagnostics.Stopwatch commitTimer = System.Diagnostics.Stopwatch.StartNew();

				if (loadFatalError != null)
				{
					commitTimer.Stop();
					loadTimer.Stop();
					Interface.AddMessage(MessageType.Critical, false, "Object load failed after " + loadTimer.ElapsedMilliseconds + " ms (" + loadFatalError + ").");
					formTrain.Instance?.EnableUI();
					Renderer.GameWindow.Title = "Object Viewer";
				}
				else
				{
					List<KeyValuePair<string, string>> errors;
					lock (loadErrorLock)
					{
						errors = new List<KeyValuePair<string, string>>(loadErrors);
					}
					if (errors.Count != 0 && loadWasAutoReload && AutoReloadFailureCounter <= 5)
					{
						// Transient failure (e.g. file locked / being written to):
						// skip LastReloadTime so the next check retries in 0.5s.
						// (Matches the sync path, which also leaves the UI disabled here.)
						AutoReloadFailureCounter++;
						Renderer.GameWindow.Title = "Object Viewer";
					}
					else
					{
						foreach (KeyValuePair<string, string> error in errors)
						{
							if (loadWasAutoReload)
							{
								Interface.CurrentOptions.AutoReloadObjects = false;
								Interface.AddMessage(MessageType.Critical, false, "Stopped automatically re-loading objects due to the Unhandled error (" + error.Value + ") encountered while processing the file " + error.Key + ".");
							}
							else
							{
								Interface.AddMessage(MessageType.Critical, false, "Unhandled error (" + error.Value + ") encountered while processing the file " + error.Key + ".");
							}
						}
						AutoReloadFailureCounter = 0;

						for (int i = 0; i < loadFiles.Count; i++)
						{
							try
							{
								if (loadResults[i] != null)
								{
									loadResults[i].CreateObject(OpenBveApi.Math.Vector3.Zero, new ObjectCreationParameters());
								}
							}
							catch (Exception ex)
							{
								if (!loadWasAutoReload || AutoReloadFailureCounter > 5)
								{
									if (loadWasAutoReload)
									{
										Interface.CurrentOptions.AutoReloadObjects = false;
										Interface.AddMessage(MessageType.Critical, false, "Stopped automatically re-loading objects due to the Unhandled error (" + ex.Message + ") encountered while processing the file " + loadFiles[i] + ".");
									}
									else
									{
										Interface.AddMessage(MessageType.Critical, false, "Unhandled error (" + ex.Message + ") encountered while processing the file " + loadFiles[i] + ".");
									}
								}
								else
								{
									AutoReloadFailureCounter++;
									Renderer.GameWindow.Title = "Object Viewer";
									return;
								}
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

						if (loadFiles.Count == 1)
						{
							Renderer.GameWindow.Title = "Object Viewer - " + System.IO.Path.GetFileName(loadFiles[0]);
						}
						else
						{
							Renderer.GameWindow.Title = "Object Viewer";
						}
				LastReloadTime = DateTime.UtcNow;
				UpdateWatchers();
				commitTimer.Stop();
					loadTimer.Stop();
					Interface.AddMessage(MessageType.Information, false,
						FormatLoadMessage(loadTimer.ElapsedMilliseconds, CurrentHost.PluginObjectLoadTime, commitTimer.ElapsedMilliseconds, false));
				}
			}
		}
			finally
			{
				loadFiles = null;
				loadResults = null;
				LoadDone = false;
				IsLoading = false;
				if (PendingReload)
				{
					PendingReload = false;
					RefreshObjectsAsync(false);
				}
			}
		}

		/// <summary>Train commit on the UI/render thread: the object cache is now warm,
		/// so the unchanged synchronous train load below runs cache-hot. Owns no train
		/// semantics itself; failure-counter/title/watcher behavior is RefreshObjects'.</summary>
		private static void CommitTrain()
		{
			System.Diagnostics.Stopwatch commitTimer = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				Renderer.CurrentInterface = InterfaceType.Normal;
				Renderer.Loading.CompleteLoading();
				// No teardown here: RefreshObjects performs its own (Unload+Reset+Game.Reset).

				if (loadFatalError != null)
				{
					commitTimer.Stop();
					loadTimer.Stop();
					Interface.AddMessage(MessageType.Critical, false, "Object load failed after " + loadTimer.ElapsedMilliseconds + " ms (" + loadFatalError + ").");
					formTrain.Instance?.EnableUI();
					Renderer.GameWindow.Title = "Object Viewer";
				}
				else
				{
					// Pre-decode failures were already logged (locked) and are deduped from
					// the parser's own reports via the host failure sets; the parser simply
					// re-reports what is still failing. loadResults are intentionally unused:
					// the host object cache is the handoff.
					// Snapshot the pre-decode figure: the delegated RefreshObjects must run
					// quiet so it neither restarts the timer nor wipes/emits metrics.
					long preDecodeMs = CurrentHost.PluginObjectLoadTime;
					DateTime reloadStamp = LastReloadTime;
					RefreshObjects(loadWasAutoReload, true);
					commitTimer.Stop();
					loadTimer.Stop();
					if (LastReloadTime != reloadStamp)
					{
						Interface.AddMessage(MessageType.Information, false,
							FormatLoadMessage(loadTimer.ElapsedMilliseconds, preDecodeMs, commitTimer.ElapsedMilliseconds, false));
					}
					// Else: inner transient-retry early return (no finalize); the retry logs on completion.
				}
			}
			finally
			{
				loadFiles = null;
				loadResults = null;
				LoadDone = false;
				IsLoading = false;
				if (PendingReload)
				{
					PendingReload = false;
					RefreshObjectsAsync(false);
				}
			}
		}

        /// <summary>Checks if any of the loaded files have been updated externally.</summary>
        internal static void CheckFileChanges(double timeElapsed)
        {
            if (IsLoading)
            {
	            // Coalesce into a single follow-up reload once the current load finishes,
	            // but only when auto-reload is actually armed and something is loaded.
	            if (Interface.CurrentOptions.AutoReloadObjects && FileCount != 0)
	            {
		            PendingReload = true;
	            }
	            return;
            }
            if (Interface.CurrentOptions.AutoReloadObjects == false || FileCount == 0 || (reloadCheckTimer += timeElapsed) < 0.5)
            {
                return;
            }
            reloadCheckTimer = 0.0;

            List<string> files = SnapshotFiles();
            foreach (string currentFile in files)
            {
				try
	            {
		            DateTime time = System.IO.File.GetLastWriteTimeUtc(currentFile);
		            if ((DateTime.Now - time).TotalSeconds < 5 || time.Year == 1601)
		            {
			            // file is held open for constant write (within last 5s)
						// file no longer exists (returns 01/01/1601)
			            continue;
		            }
		            if (System.IO.File.GetLastWriteTimeUtc(currentFile) > LastReloadTime)
		            {
			            RefreshObjectsAsync();
			            return;
		            }
				}
	            catch(Exception e)
	            {
		            Interface.AddMessage(MessageType.Error, false, "Stopping automatically reloading objects due to the following exception: " + e);
	            }
	            
            }
        }


		private static readonly List<System.IO.FileSystemWatcher> FileWatchers = new List<System.IO.FileSystemWatcher>();
		/// <summary>Guards <see cref="FileWatchers"/> against watcher-callback races.</summary>
		private static readonly object FileWatchersLock = new object();
		internal static volatile bool ReloadRequested = false;
		private static DateTime LastFileChangedTime = DateTime.MinValue;

		internal static void UpdateWatchers()
		{
			lock (FileWatchersLock)
			{
				foreach (var watcher in FileWatchers)
				{
					watcher.EnableRaisingEvents = false;
					watcher.Dispose();
				}
				FileWatchers.Clear();
				foreach (string file in SnapshotFiles())
				{
					try
					{
						string dir = System.IO.Path.GetDirectoryName(file);
						string name = System.IO.Path.GetFileName(file);
						if (System.IO.Directory.Exists(dir))
						{
							var watcher = new System.IO.FileSystemWatcher(dir, name);
							watcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;
							watcher.Changed += OnFileChanged;
							watcher.EnableRaisingEvents = true;
							FileWatchers.Add(watcher);
						}
					}
					catch
					{
						// ignored
					}
				}
			}
		}

		private static void OnFileChanged(object source, System.IO.FileSystemEventArgs e)
		{
			lock (FileWatchersLock)
			{
				if ((DateTime.Now - LastFileChangedTime).TotalMilliseconds <= 500)
				{
					return;
				}
				LastFileChangedTime = DateTime.Now;
				if (IsLoading)
				{
					PendingReload = true;
				}
				else
				{
					ReloadRequested = true;
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
	                if (IsLoading)
	                {
		                Interface.AddMessage(MessageType.Information, false, "Still loading objects, ignoring manual reload until the current load finishes.");
		                break;
	                }
	                RefreshObjectsAsync();
	                break;
	            case Key.F7:
					{
						if (IsLoading)
						{
							Interface.AddMessage(MessageType.Information, false, "Still loading objects, ignoring file dialog until the current load finishes.");
							break;
						}
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
						            AddFile(System.IO.Path.GetFullPath(f[i]));
					            }
					            if (!string.IsNullOrEmpty(currentTrain) && Program.CurrentHost.Plugins[j].Train != null && Program.CurrentHost.Plugins[j].Train.CanLoadTrain(currentTrain))
					            {
						            AddFile(System.IO.Path.GetFullPath(f[i]));
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
						RefreshObjectsAsync();
					} 
					break;
	            case Key.F9:
		            if (Program.CurrentHost.Platform == HostPlatform.AppleOSX && IntPtr.Size != 4)
		            {
						Program.Renderer.CurrentInterface = InterfaceType.Menu;
						Game.Menu.PushMenu(MenuType.ErrorList);
						return;
		            }
					if (Interface.GetLogSnapshot().Count != 0)
	                {
	                    formMessages.ShowMessages();
                        Application.DoEvents();
	                }
	                break;
			case Key.BackSpace:
            case Key.Delete:
	            if (IsLoading)
	            {
		            Interface.AddMessage(MessageType.Information, false, "Still loading objects, ignoring unload until the current load finishes.");
		            break;
	            }
	            LightingRelative = -1.0;
                Game.Reset();
				// Pressing delete means unload current loaded objects, so release everything instead of preserving.
				Renderer.TextureManager.UnloadAllTextures(false);
				CurrentHost.ClearObjectCaches();
	            ClearFiles();
				// One full collect so the freed LOH buffers leave RAM immediately.
				GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
				GC.WaitForPendingFinalizers();
                LastReloadTime = DateTime.UtcNow;
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

	                if (formOptions.ShowOptions() == DialogResult.OK)
	                {
		                // Sun direction is already updated in real-time via slider events

		                // Appy shadow map settings immediately
		                Renderer.ReloadShadowSettings();
					}
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
				                Interface.CurrentOptions.BackgroundColor = new Color24(dialog.Color.R, dialog.Color.G, dialog.Color.B);
				                Renderer.ApplyBackgroundColor(dialog.Color.R, dialog.Color.G, dialog.Color.B);
			                }
		                }
	                }
	                else
	                {
		                if (Interface.CurrentOptions.BackgroundColor == Color24.LightGrey)
		                {
			                Interface.CurrentOptions.BackgroundColor = Color24.White;
			                Interface.CurrentOptions.TextColor = Color32.Black;
		                }
	                    else if (Interface.CurrentOptions.BackgroundColor == Color24.White)
	                    {
		                    Interface.CurrentOptions.BackgroundColor = Color24.Black;
		                    Interface.CurrentOptions.TextColor = Color32.White;
	                    }
						else if (Interface.CurrentOptions.BackgroundColor == Color24.Black)
						{
							Interface.CurrentOptions.BackgroundColor = Color24.DarkGrey;
							Interface.CurrentOptions.TextColor = Color32.White;
						}
						else
						{
							Interface.CurrentOptions.BackgroundColor = Color24.LightGrey;
							Interface.CurrentOptions.TextColor = Color32.White;
						}
		                Renderer.ApplyBackgroundColor();
	                }
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
							Renderer.CurrentInterface = InterfaceType.Menu;
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
