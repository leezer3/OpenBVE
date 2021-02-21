using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using OpenBve.Parsers.Train;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using OpenBveApi.Routes;
using RouteManager2;
using TrainManager.Car;
using TrainManager.Trains;

namespace OpenBve {
	internal static class Loading {
		/// <summary>The current train loading progress</summary>
		internal static double TrainProgress;
		/// <summary>Set this member to true to cancel loading</summary>
		internal static bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				if (value)
				{
					//Send cancellation call to plugins
					for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
					{
						if (Program.CurrentHost.Plugins[i].Route != null && Program.CurrentHost.Plugins[i].Route.IsLoading)
						{
							Program.CurrentHost.Plugins[i].Route.Cancel = true;
						}
					}
					_cancel = true;
				}
				else
				{
					_cancel = false;
				}
			}
		}

		private static bool _cancel;
		/// <summary>Whether loading is complete</summary>
		internal static bool Complete;
		/// <summary>True when the simulation has been completely setup</summary>
		internal static bool SimulationSetup;
		/// <summary>Whether there is currently a job waiting to complete in the main game loop</summary>
		internal static bool JobAvailable = false;
		private static Thread Loader;
		/// <summary>The current route file</summary>
		private static string CurrentRouteFile;
		/// <summary>The character encoding of this route file</summary>
		private static Encoding CurrentRouteEncoding;
		/// <summary>The current train folder</summary>
		private static string CurrentTrainFolder;
		/// <summary>The character encoding of this train</summary>
		private static Encoding CurrentTrainEncoding;
		internal static double TrainProgressCurrentSum;
		internal static double TrainProgressCurrentWeight;
		
		// load
		/// <summary>Initializes loading the route and train asynchronously. Set the Loading.Cancel member to cancel loading. Check the Loading.Complete member to see when loading has finished.</summary>
		internal static void LoadAsynchronously(string RouteFile, Encoding RouteEncoding, string TrainFolder, Encoding TrainEncoding) {
			//Deliberately purge all plugins and reload in case a preview thread is running
			Plugins.UnloadPlugins();
			Plugins.LoadPlugins();
			// members
			TrainProgress = 0.0;
			TrainProgressCurrentSum = 0.0;
			TrainProgressCurrentWeight = 1.0;
			Cancel = false;
			Complete = false;
			CurrentRouteFile = RouteFile;
			CurrentRouteEncoding = RouteEncoding;
			CurrentTrainFolder = TrainFolder;
			CurrentTrainEncoding = TrainEncoding;
			//Set the route and train folders in the info class
			Program.CurrentRoute.Information.RouteFile = RouteFile;
			Program.CurrentRoute.Information.TrainFolder = TrainFolder;
			Program.CurrentRoute.Information.FilesNotFound = null;
			Program.CurrentRoute.Information.ErrorsAndWarnings = null;
			Loader = new Thread(LoadThreaded) {IsBackground = true};
			Loader.Start();
		}

		/// <summary>Gets the absolute Railway folder for a given route file</summary>
		/// <returns>The absolute on-disk path of the railway folder</returns>
		internal static string GetRailwayFolder(string RouteFile) {
			try {
				string Folder = System.IO.Path.GetDirectoryName(RouteFile);
				
				while (true) {
					string Subfolder = OpenBveApi.Path.CombineDirectory(Folder, "Railway");
					if (System.IO.Directory.Exists(Subfolder)) {
						if (System.IO.Directory.EnumerateDirectories(Subfolder).Any() || System.IO.Directory.EnumerateFiles(Subfolder).Any())
						{
							//HACK: Ignore completely empty directories
							//Doesn't handle wrong directories, or those with stuff missing, TODO.....
							Program.FileSystem.AppendToLogFile(Subfolder + " : Railway folder found.");
							return Subfolder;
						}
						Program.FileSystem.AppendToLogFile(Subfolder + " : Railway folder candidate rejected- Directory empty.");
					}
					if (Folder == null) continue;
					System.IO.DirectoryInfo Info = System.IO.Directory.GetParent(Folder);
					if (Info == null) break;
					Folder = Info.FullName;
				}
			} catch { }
			
			//If the Route, Object and Sound folders exist, but are not in a railway folder.....
			try
			{
				string Folder = System.IO.Path.GetDirectoryName(RouteFile);
				string candidate = null;
				while (true)
				{
					string RouteFolder = OpenBveApi.Path.CombineDirectory(Folder, "Route");
					string ObjectFolder = OpenBveApi.Path.CombineDirectory(Folder, "Object");
					string SoundFolder = OpenBveApi.Path.CombineDirectory(Folder, "Sound");
					if (System.IO.Directory.Exists(RouteFolder) && System.IO.Directory.Exists(ObjectFolder) && System.IO.Directory.Exists(SoundFolder))
					{
						Program.FileSystem.AppendToLogFile(Folder + " : Railway folder found.");
						return Folder;
					}
					if (System.IO.Directory.Exists(RouteFolder) && System.IO.Directory.Exists(ObjectFolder))
					{
						candidate = Folder;
					}
					System.IO.DirectoryInfo Info = System.IO.Directory.GetParent(Folder);
					if (Info == null)
					{
						if (candidate != null)
						{
							Program.FileSystem.AppendToLogFile(Folder + " : The best candidate for the Railway folder has been selected- Sound folder not detected.");
							return candidate;
						}
						break;
					}
					Folder = Info.FullName;
				}
			}
			catch { }
			Program.FileSystem.AppendToLogFile("No Railway folder found- Returning the openBVE startup path.");
			return Application.StartupPath;
		}

		// load threaded
		private static void LoadThreaded() {
			try {
				LoadEverythingThreaded();
			} catch (Exception ex) {
				for (int i = 0; i < Program.TrainManager.Trains.Length; i++) {
					if (Program.TrainManager.Trains[i] != null && Program.TrainManager.Trains[i].Plugin != null) {
						if (Program.TrainManager.Trains[i].Plugin.LastException != null) {
							Interface.AddMessage(MessageType.Critical, false, "The train plugin " + Program.TrainManager.Trains[i].Plugin.PluginTitle + " caused a critical error in the route and train loader: " + Program.TrainManager.Trains[i].Plugin.LastException.Message);
							CrashHandler.LoadingCrash(Program.TrainManager.Trains[i].Plugin.LastException + Environment.StackTrace, true);
							 Program.RestartArguments = " ";
							 Cancel = true;    
							return;
						}
					}
				}
				if (ex is System.DllNotFoundException)
				{
					Interface.AddMessage(MessageType.Critical, false, "The required system library " + ex.Message + " was not found on the system.");
					switch (ex.Message)
					{
						case "libopenal.so.1":
							MessageBox.Show("openAL was not found on this system. \n Please install libopenal1 via your distribtion's package management system.", Translations.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
							break;
						default:
							MessageBox.Show("The required system library " + ex.Message + " was not found on this system.", Translations.GetInterfaceString("program_title"), MessageBoxButtons.OK, MessageBoxIcon.Hand);
							break;
					}
				}
				else
				{
					Interface.AddMessage(MessageType.Critical, false, "The route and train loader encountered the following critical error: " + ex.Message);
					CrashHandler.LoadingCrash(ex + Environment.StackTrace, false);
				}
				
				Program.RestartArguments = " ";
				Cancel = true;                
			}
			if (JobAvailable)
			{
				Thread.Sleep(10);
			}
			Complete = true;
		}
		private static void LoadEverythingThreaded() {
			
			string RailwayFolder = GetRailwayFolder(CurrentRouteFile);
			string ObjectFolder = OpenBveApi.Path.CombineDirectory(RailwayFolder, "Object");
			string SoundFolder = OpenBveApi.Path.CombineDirectory(RailwayFolder, "Sound");
			Game.Reset(true);
			Game.MinimalisticSimulation = true;
			// screen
			Program.Renderer.Camera.CurrentMode = CameraViewMode.Interior;

			bool loaded = false;
			Program.FileSystem.AppendToLogFile("INFO: " + Program.CurrentHost.AvailableRoutePluginCount + " Route loading plugins available.");
			Program.FileSystem.AppendToLogFile("INFO: " + Program.CurrentHost.AvailableObjectPluginCount + " Object loading plugins available.");
			Program.FileSystem.AppendToLogFile("INFO: " + Program.CurrentHost.AvailableRoutePluginCount + " Sound loading plugins available.");
			for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
			{
				if (Program.CurrentHost.Plugins[i].Route != null && Program.CurrentHost.Plugins[i].Route.CanLoadRoute(CurrentRouteFile))
				{
					object Route = (object)Program.CurrentRoute; //must cast to allow us to use the ref keyword.
					if (Program.CurrentHost.Plugins[i].Route.LoadRoute(CurrentRouteFile, CurrentRouteEncoding, CurrentTrainFolder, ObjectFolder, SoundFolder, false, ref Route))
					{
						Program.CurrentRoute = (CurrentRoute) Route;
						Program.Renderer.Lighting.OptionAmbientColor = Program.CurrentRoute.Atmosphere.AmbientLightColor;
						Program.Renderer.Lighting.OptionDiffuseColor = Program.CurrentRoute.Atmosphere.DiffuseLightColor;
						Program.Renderer.Lighting.OptionLightPosition = Program.CurrentRoute.Atmosphere.LightPosition;
						loaded = true;
						break;
					}
					var currentError = Translations.GetInterfaceString("errors_critical_file");
					currentError = currentError.Replace("[file]", System.IO.Path.GetFileName(CurrentRouteFile));
					MessageBox.Show(currentError, @"OpenBVE", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					Interface.AddMessage(MessageType.Critical, false, "The route and train loader encountered the following critical error: " + Program.CurrentHost.Plugins[i].Route.LastException.Message);
					CrashHandler.LoadingCrash(Program.CurrentHost.Plugins[i].Route.LastException.Message, false);
					Program.RestartArguments = " ";
					Cancel = true;
					return;

				}
			}

			TrainManager.Derailments = Interface.CurrentOptions.Derailments;
			TrainManager.Toppling = Interface.CurrentOptions.Toppling;
			TrainManager.CurrentRoute = Program.CurrentRoute;
			if (!loaded)
			{
				throw new Exception("No plugins capable of loading routefile " + CurrentRouteFile + " were found.");
			}
			Thread createIllustrations = new Thread(Program.CurrentRoute.Information.LoadInformation) {IsBackground = true};
			createIllustrations.Start();
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			Program.CurrentRoute.Atmosphere.CalculateSeaLevelConstants();
			if (Program.CurrentRoute.BogusPreTrainInstructions.Length != 0) {
				double t = Program.CurrentRoute.BogusPreTrainInstructions[0].Time;
				double p = Program.CurrentRoute.BogusPreTrainInstructions[0].TrackPosition;
				for (int i = 1; i < Program.CurrentRoute.BogusPreTrainInstructions.Length; i++) {
					if (Program.CurrentRoute.BogusPreTrainInstructions[i].Time > t) {
						t = Program.CurrentRoute.BogusPreTrainInstructions[i].Time;
					} else {
						t += 1.0;
						Program.CurrentRoute.BogusPreTrainInstructions[i].Time = t;
					}
					if (Program.CurrentRoute.BogusPreTrainInstructions[i].TrackPosition > p) {
						p = Program.CurrentRoute.BogusPreTrainInstructions[i].TrackPosition;
					} else {
						p += 1.0;
						Program.CurrentRoute.BogusPreTrainInstructions[i].TrackPosition = p;
					}
				}
			}
			Program.Renderer.CameraTrackFollower = new TrackFollower(Program.CurrentHost) { Train = null, Car = null };
			if (Program.CurrentRoute.Stations.Length == 1)
			{
				//Log the fact that only a single station is present, as this is probably not right
				Program.FileSystem.AppendToLogFile("The processed route file only contains a single station.");
			}
			Program.FileSystem.AppendToLogFile("Route file loaded successfully.");
			// initialize trains
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			Program.TrainManager.Trains = new TrainBase[Program.CurrentRoute.PrecedingTrainTimeDeltas.Length + 1 + (Program.CurrentRoute.BogusPreTrainInstructions.Length != 0 ? 1 : 0)];
			for (int k = 0; k < Program.TrainManager.Trains.Length; k++)
			{
				if (k == Program.TrainManager.Trains.Length - 1 & Program.CurrentRoute.BogusPreTrainInstructions.Length != 0)
				{
					Program.TrainManager.Trains[k] = new TrainManager.Train(TrainState.Bogus);
				}
				else
				{
					Program.TrainManager.Trains[k] = new TrainManager.Train(TrainState.Pending);
				}
				
			}
			TrainManager.PlayerTrain = Program.TrainManager.Trains[Program.CurrentRoute.PrecedingTrainTimeDeltas.Length];

			UnifiedObject[] CarObjects = null;
			UnifiedObject[] BogieObjects = null;
			UnifiedObject[] CouplerObjects = null;

			// load trains
			double TrainProgressMaximum = 0.7 + 0.3 * (double)Program.TrainManager.Trains.Length;
			for (int k = 0; k < Program.TrainManager.Trains.Length; k++) {
				TrainManager.Train currentTrain = Program.TrainManager.Trains[k] as TrainManager.Train;
				//Sleep for 20ms to allow route loading locks to release
				Thread.Sleep(20);
				if (currentTrain.State == TrainState.Bogus) {
					// bogus train
					string TrainData = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Compatibility", "PreTrain"), "train.dat");
					TrainDatParser.ParseTrainData(TrainData, System.Text.Encoding.UTF8, currentTrain);
					Thread.Sleep(1); if (Cancel) return;
					TrainProgressCurrentWeight = 0.3 / TrainProgressMaximum;
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
				} else {
					
					currentTrain.TrainFolder = CurrentTrainFolder;
					// real train
					if (currentTrain.IsPlayerTrain)
					{
						Program.FileSystem.AppendToLogFile("Loading player train: " + currentTrain.TrainFolder);
					}
					else
					{
						Program.FileSystem.AppendToLogFile("Loading AI train: " + currentTrain.TrainFolder);
					}
					TrainProgressCurrentWeight = 0.1 / TrainProgressMaximum;
					string TrainData = OpenBveApi.Path.CombineFile(currentTrain.TrainFolder, "train.dat");
					TrainDatParser.ParseTrainData(TrainData, CurrentTrainEncoding, currentTrain);
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					TrainProgressCurrentWeight = 0.2 / TrainProgressMaximum;
					SoundCfgParser.ParseSoundConfig(currentTrain.TrainFolder, currentTrain);
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					// door open/close speed
					for (int i = 0; i < currentTrain.Cars.Length; i++) {
						currentTrain.Cars[i].DetermineDoorClosingSpeed();
					}
				}
				// add panel section
				if (currentTrain.IsPlayerTrain) {	
					TrainProgressCurrentWeight = 0.7 / TrainProgressMaximum;
					currentTrain.ParsePanelConfig(currentTrain.TrainFolder, CurrentTrainEncoding);
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					Program.FileSystem.AppendToLogFile("Train panel loaded sucessfully.");
				}
				// add exterior section
				if (currentTrain.State != TrainState.Bogus)
				{
					bool LoadObjects = false;
					bool[] VisibleFromInterior = new bool[currentTrain.Cars.Length];
					if (CarObjects == null)
					{
						CarObjects = new UnifiedObject[currentTrain.Cars.Length];
						BogieObjects = new UnifiedObject[currentTrain.Cars.Length * 2];
						CouplerObjects = new UnifiedObject[currentTrain.Cars.Length];
						LoadObjects = true;
					}
					string tXml = OpenBveApi.Path.CombineFile(currentTrain.TrainFolder, "train.xml");
					if (System.IO.File.Exists(tXml))
					{
						TrainXmlParser.Parse(tXml, currentTrain, ref CarObjects, ref BogieObjects, ref CouplerObjects, ref VisibleFromInterior);
					}
					else
					{
						ExtensionsCfgParser.ParseExtensionsConfig(currentTrain.TrainFolder, CurrentTrainEncoding, ref CarObjects, ref BogieObjects, ref CouplerObjects, ref VisibleFromInterior, currentTrain, LoadObjects);
					}
					TrainManager.PlayerTrain.CameraCar = currentTrain.DriverCar;
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					//Stores the current array index of the bogie object to add
					//Required as there are two bogies per car, and we're using a simple linear array....
					int currentBogieObject = 0;
					for (int i = 0; i < currentTrain.Cars.Length; i++)
					{
						if (CarObjects[i] == null) {
							// load default exterior object
							string file = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Compatibility"), "exterior.csv");
							StaticObject so;
							Program.CurrentHost.LoadStaticObject(file, System.Text.Encoding.UTF8, false, out so);
							if (so == null) {
								CarObjects[i] = null;
							} else {
								StaticObject c = (StaticObject)so.Clone(); //Clone as otherwise the cached object doesn't scale right
								c.ApplyScale(currentTrain.Cars[i].Width, currentTrain.Cars[i].Height, currentTrain.Cars[i].Length);
								CarObjects[i] = c;
							}
						}
						if (CarObjects[i] != null) {
							// add object
							currentTrain.Cars[i].LoadCarSections(CarObjects[i], VisibleFromInterior[i]);
						}

						if (CouplerObjects[i] != null)
						{
							currentTrain.Cars[i].Coupler.LoadCarSections(CouplerObjects[i], VisibleFromInterior[i]);
						}
						//Load bogie objects
						if (BogieObjects[currentBogieObject] != null)
						{
							currentTrain.Cars[i].FrontBogie.LoadCarSections(BogieObjects[currentBogieObject], VisibleFromInterior[i]);
						}
						currentBogieObject++;
						if (BogieObjects[currentBogieObject] != null)
						{
							currentTrain.Cars[i].RearBogie.LoadCarSections(BogieObjects[currentBogieObject], VisibleFromInterior[i]);
						}
						currentBogieObject++;
					}
				}
				// place cars
				currentTrain.PlaceCars(0.0);

				// configure other properties
				if (currentTrain.IsPlayerTrain) {
					currentTrain.TimetableDelta = 0.0;
					if (Game.InitialReversedConsist)
					{
						currentTrain.Reverse();
						TrainManager.PlayerTrain.CameraCar = currentTrain.DriverCar;
						Program.Renderer.Camera.CurrentRestriction = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestrictionMode;
					}
				} else if (currentTrain.State != TrainState.Bogus) {
					currentTrain.AI = new Game.SimpleHumanDriverAI(currentTrain, Interface.CurrentOptions.PrecedingTrainSpeedLimit);
					currentTrain.TimetableDelta = Program.CurrentRoute.PrecedingTrainTimeDeltas[k];
					currentTrain.Specs.DoorOpenMode = DoorMode.Manual;
					currentTrain.Specs.DoorCloseMode = DoorMode.Manual;
				}
			}
			TrainProgress = 1.0;
			// finished created objects
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			Array.Resize(ref ObjectManager.AnimatedWorldObjects, ObjectManager.AnimatedWorldObjectsUsed);
			// update sections
			if (Program.CurrentRoute.Sections.Length > 0) {
				Program.CurrentRoute.UpdateAllSections();
			}
			// load plugin
			for (int i = 0; i < Program.TrainManager.Trains.Length; i++) {
				if ( Program.TrainManager.Trains[i].State != TrainState.Bogus) {
					if ( Program.TrainManager.Trains[i].IsPlayerTrain) {
						if (! Program.TrainManager.Trains[i].LoadCustomPlugin( Program.TrainManager.Trains[i].TrainFolder, CurrentTrainEncoding)) {
							Program.TrainManager.Trains[i].LoadDefaultPlugin(  Program.TrainManager.Trains[i].TrainFolder);
						}
					} else {
						Program.TrainManager.Trains[i].LoadDefaultPlugin( Program.TrainManager.Trains[i].TrainFolder);
					}
					for (int j = 0; j < InputDevicePlugin.AvailablePluginInfos.Count; j++) {
						if (InputDevicePlugin.AvailablePluginInfos[j].Status == InputDevicePlugin.PluginInfo.PluginStatus.Enable && InputDevicePlugin.AvailablePlugins[j] is ITrainInputDevice)
						{
							ITrainInputDevice trainInputDevice = (ITrainInputDevice)InputDevicePlugin.AvailablePlugins[j];
							trainInputDevice.SetVehicleSpecs( Program.TrainManager.Trains[i].vehicleSpecs());
						}
					}
				}
			}
		}

	}
}
