using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using LibRender2.Trains;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using OpenBveApi.Routes;
using RouteManager2;
using TrainManager;
using TrainManager.Car;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace OpenBve {
	internal static class Loading
	{
		/// <summary>The number of trains that have been loaded so far</summary>
		internal static int LoadedTrain;
		/// <summary>Set this member to true to cancel loading</summary>
		internal static bool Cancel
		{
			get => _cancel;
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
		private static Thread Loader;
		/// <summary>The current route file</summary>
		private static string CurrentRouteFile;
		/// <summary>The character encoding of this route file</summary>
		private static Encoding CurrentRouteEncoding;
		/// <summary>The current train folder</summary>
		internal static string CurrentTrainFolder;
		/// <summary>The character encoding of this train</summary>
		private static Encoding CurrentTrainEncoding;
		
		// load
		/// <summary>Initializes loading the route and train asynchronously. Set the Loading.Cancel member to cancel loading. Check the Loading.Complete member to see when loading has finished.</summary>
		internal static void LoadAsynchronously(string routeFile, Encoding routeEncoding, string trainFolder, Encoding trainEncoding) {
			// members
			Cancel = false;
			Complete = false;
			CurrentRouteFile = routeFile;
			CurrentRouteEncoding = routeEncoding;
			CurrentTrainFolder = trainFolder;
			CurrentTrainEncoding = trainEncoding;
			//Set the route and train folders in the info class
			Program.CurrentRoute.Information.RouteFile = routeFile;
			Program.CurrentRoute.Information.TrainFolder = trainFolder;
			Program.CurrentRoute.Information.FilesNotFound = null;
			Program.CurrentRoute.Information.ErrorsAndWarnings = null;
			Loader = new Thread(LoadThreaded) {IsBackground = true};
			Loader.Start();
		}

		/// <summary>Gets the absolute Railway folder for a given route file</summary>
		/// <returns>The absolute on-disk path of the railway folder</returns>
		internal static string GetRailwayFolder(string routeFile) {
			try
			{
				string currentFolder = Path.GetDirectoryName(routeFile);

				while (true)
				{
					string Subfolder = Path.CombineDirectory(currentFolder, "Railway");
					if (Directory.Exists(Subfolder))
					{
						if (Directory.EnumerateDirectories(Subfolder).Any() || Directory.EnumerateFiles(Subfolder).Any())
						{
							//HACK: Ignore completely empty directories
							//Doesn't handle wrong directories, or those with stuff missing, TODO.....
							Program.FileSystem.AppendToLogFile(Subfolder + " : Railway folder found.");
							return Subfolder;
						}

						Program.FileSystem.AppendToLogFile(Subfolder + " : Railway folder candidate rejected- Directory empty.");
					}

					if (currentFolder == null) continue;
					DirectoryInfo directoryInfo = Directory.GetParent(currentFolder);
					if (directoryInfo == null) break;
					currentFolder = directoryInfo.FullName;
				}
			}
			catch
			{
				// ignored
			}
			
			// If the Route, Object and Sound folders exist, but are not in a railway folder.....
			try
			{
				string currentFolder = Path.GetDirectoryName(routeFile);
				if (currentFolder == null)
				{
					// Unlikely to work, but attempt to make the best of it
					Program.FileSystem.AppendToLogFile("The route file appears to be stored on a root path- Returning the " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"program","title"}) + " startup path.");
					return Application.StartupPath;
				}
				string candidate = null;
				while (true)
				{
					string routeFolder = Path.CombineDirectory(currentFolder, "Route");
					string objectFolder = Path.CombineDirectory(currentFolder, "Object");
					string soundFolder = Path.CombineDirectory(currentFolder, "Sound");
					if (Directory.Exists(routeFolder) && Directory.Exists(objectFolder) && Directory.Exists(soundFolder))
					{
						Program.FileSystem.AppendToLogFile(currentFolder + " : Railway folder found.");
						return currentFolder;
					}

					if (Directory.Exists(routeFolder) && Directory.Exists(objectFolder))
					{
						candidate = currentFolder;
					}

					DirectoryInfo directoryInfo = Directory.GetParent(currentFolder);
					if (directoryInfo == null)
					{
						if (candidate != null)
						{
							Program.FileSystem.AppendToLogFile(currentFolder + " : The best candidate for the Railway folder has been selected- Sound folder not detected.");
							return candidate;
						}

						break;
					}

					currentFolder = directoryInfo.FullName;
				}
			}
			catch
			{
				// ignored
			}
			Program.FileSystem.AppendToLogFile("No Railway folder found- Returning the " + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"program","title"}) + " startup path.");
			return Application.StartupPath;
		}

		/// <summary>Gets the default train folder for a given route file</summary>
		/// <returns>The absolute on-disk path of the train folder</returns>
		internal static string GetDefaultTrainFolder(string routeFile)
		{
			if (string.IsNullOrEmpty(routeFile) || string.IsNullOrEmpty(Interface.CurrentOptions.TrainName)) {
				return string.Empty;
			}

			if (Directory.Exists(Program.FileSystem.MSTSDirectory) && Interface.CurrentOptions.TrainName.EndsWith(".con", StringComparison.InvariantCultureIgnoreCase))
			{
				// potential MSTS consist
				string consistDirectory = Path.CombineDirectory(Program.FileSystem.MSTSDirectory, "TRAINS\\Consists");
				string consistFile = Path.CombineFile(consistDirectory, Interface.CurrentOptions.TrainName);
				if (File.Exists(consistFile))
				{
					return consistFile;
				}
			}

			string currentFolder;
			try {
				currentFolder = Path.GetDirectoryName(routeFile);
				if (Interface.CurrentOptions.TrainName[0] == '$') {
					currentFolder = Path.CombineDirectory(currentFolder, Interface.CurrentOptions.TrainName);
					if (Directory.Exists(currentFolder)) {
						string File = Path.CombineFile(currentFolder, "train.dat");
						if (System.IO.File.Exists(File)) {
							
							return currentFolder;
						}
					}
				}
			} catch {
				currentFolder = null;
			}
			bool recursionTest = false;
			string lastFolder = null;
			try
			{
				while (true)
				{
					string trainFolder = Path.CombineDirectory(currentFolder, "Train");
					var oldFolder = currentFolder;
					if (Directory.Exists(trainFolder))
					{
						try
						{
							currentFolder = Path.CombineDirectory(trainFolder, Interface.CurrentOptions.TrainName);
						}
						catch (Exception ex)
						{
							if (ex is ArgumentException)
							{
								break; // Invalid character in path causes infinite recursion
							}

							currentFolder = null;
						}

						if (currentFolder != null)
						{
							char c = System.IO.Path.DirectorySeparatorChar;
							if (Directory.Exists(currentFolder))
							{

								string File = Path.CombineFile(currentFolder, "train.dat");
								if (System.IO.File.Exists(File))
								{
									// train found
									return currentFolder;
								}

								if (lastFolder == currentFolder || recursionTest)
								{
									break;
								}

								lastFolder = currentFolder;
							}
							else if (currentFolder.ToLowerInvariant().Contains(c + "railway" + c))
							{
								//If we have a misplaced Train folder in either our Railway\Route
								//or Railway folders, this can cause the train search to fail
								//Detect the presence of a railway folder and carry on traversing upwards if this is the case
								recursionTest = true;
								currentFolder = oldFolder;
							}
							else
							{
								break;
							}
						}
					}

					if (currentFolder == null) continue;
					DirectoryInfo directoryInfo = Directory.GetParent(currentFolder);
					if (directoryInfo != null)
					{
						currentFolder = directoryInfo.FullName;
					}
					else
					{
						break;
					}
				}
			}
			catch
			{
				//Something broke, but we don't care as it just shows an error below
			}
			return string.Empty;
		}

		// load threaded
		private static void LoadThreaded() {
			try {
				LoadEverythingThreaded();
			} catch (Exception ex) {
				for (int i = 0; i < Program.TrainManager.Trains.Count; i++) {
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
				if (ex is DllNotFoundException)
				{
					Interface.AddMessage(MessageType.Critical, false, "The required system library " + ex.Message + " was not found on the system.");
					switch (ex.Message)
					{
						case "libopenal.so.1":
							Program.ShowMessageBox("openAL was not found on this system. \n Please install libopenal1 via your distribtion's package management system.", Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"program","title"}));
							break;
						default:
							Program.ShowMessageBox("The required system library " + ex.Message + " was not found on this system.", Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "program", "title" }));
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
			if (Program.Renderer.RenderThreadJobWaiting)
			{
				Thread.Sleep(10);
			}
			Complete = true;
		}
		private static void LoadEverythingThreaded() {
			
			string railwayFolder = GetRailwayFolder(CurrentRouteFile);
			string objectFolder = Path.CombineDirectory(railwayFolder, "Object");
			string soundFolder = Path.CombineDirectory(railwayFolder, "Sound");
			Game.Reset(true);
			Game.MinimalisticSimulation = true;
			// screen

			bool loaded = false;
			Program.FileSystem.AppendToLogFile("INFO: " + Program.CurrentHost.AvailableRoutePluginCount + " Route loading plugins available.");
			Program.FileSystem.AppendToLogFile("INFO: " + Program.CurrentHost.AvailableObjectPluginCount + " Object loading plugins available.");
			Program.FileSystem.AppendToLogFile("INFO: " + Program.CurrentHost.AvailableRoutePluginCount + " Sound loading plugins available.");
			Program.FileSystem.AppendToLogFile("Load in Advance is " + (Interface.CurrentOptions.LoadInAdvance ? "enabled" : "disabled"));
			
			for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
			{
				if (Program.CurrentHost.Plugins[i].Route != null && Program.CurrentHost.Plugins[i].Route.CanLoadRoute(CurrentRouteFile))
				{
					object currentRoute = (object)Program.CurrentRoute; //must cast to allow us to use the ref keyword.
					if (Program.CurrentHost.Plugins[i].Route.LoadRoute(CurrentRouteFile, CurrentRouteEncoding, CurrentTrainFolder, objectFolder, soundFolder, false, ref currentRoute))
					{
						Program.CurrentRoute = (CurrentRoute) currentRoute;
						Program.CurrentRoute.UpdateLighting();
						
						loaded = true;
						break;
					}
					var currentError = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"errors","critical_file"});
					currentError = currentError.Replace("[file]", System.IO.Path.GetFileName(CurrentRouteFile));
					Program.ShowMessageBox(currentError, Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"program","title"}));
					Interface.AddMessage(MessageType.Critical, false, "The route and train loader encountered the following critical error: " + Program.CurrentHost.Plugins[i].Route.LastException.Message);
					CrashHandler.LoadingCrash(Program.CurrentHost.Plugins[i].Route.LastException.Message, false);
					Program.RestartArguments = " ";
					Cancel = true;
					return;

				}
			}

			TrainManagerBase.Derailments = Interface.CurrentOptions.Derailments;
			TrainManagerBase.Toppling = Interface.CurrentOptions.Toppling;
			TrainManagerBase.CurrentRoute = Program.CurrentRoute;
			if (!loaded)
			{
				throw new Exception("No plugins capable of loading routefile " + CurrentRouteFile + " were found.");
			}
			Thread createIllustrations = new Thread(Program.CurrentRoute.Information.LoadInformation) {IsBackground = true};
			createIllustrations.Start();
			Thread.Sleep(1); if (Cancel) return;
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
			Program.Renderer.CameraTrackFollower = new TrackFollower(Program.CurrentHost);
			if (Program.CurrentRoute.Stations.Length == 1)
			{
				//Log the fact that only a single station is present, as this is probably not right
				Program.FileSystem.AppendToLogFile("The processed route file only contains a single station.");
			}
			Program.FileSystem.AppendToLogFile("Route file loaded successfully.");
			// initialize trains
			Thread.Sleep(1); if (Cancel) return;
			Program.TrainManager.Trains = new List<TrainBase>
			{
				new TrainBase(TrainState.Pending, TrainType.LocalPlayerTrain)
			};
			TrainManagerBase.PlayerTrain = Program.TrainManager.Trains[0];
			
			for (int i = 0; i < Program.CurrentRoute.PrecedingTrainTimeDeltas.Length; i++)
			{
				Program.TrainManager.Trains.Add(new TrainBase(TrainState.Pending, TrainType.PreTrain));
			}

			for (int i = 0; i < Program.CurrentRoute.BogusPreTrainInstructions.Length; i++)
			{
				Program.TrainManager.Trains.Add(new TrainBase(TrainState.Bogus, TrainType.PreTrain));
			}

			LoadedTrain = 0;
			// load trains
			for (int k = 0; k < Program.TrainManager.Trains.Count; k++) {

				AbstractTrain currentTrain = Program.TrainManager.Trains[k];
				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
				{
					if (Program.CurrentHost.Plugins[i].Train != null && Program.CurrentHost.Plugins[i].Train.CanLoadTrain(CurrentTrainFolder))
					{
						
						Program.CurrentHost.Plugins[i].Train.LoadTrain(CurrentTrainEncoding, CurrentTrainFolder, ref currentTrain, ref Interface.CurrentControls);
						LoadedTrain++;
						break;
					}
				}
				Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
				// configure other properties
				if (currentTrain.IsPlayerTrain) {
					currentTrain.TimetableDelta = 0.0;
					if (Game.InitialReversedConsist)
					{
						currentTrain.Reverse();
						TrainManagerBase.PlayerTrain.CameraCar = currentTrain.DriverCar;
						Program.Renderer.Camera.CurrentRestriction = TrainManagerBase.PlayerTrain.Cars[TrainManagerBase.PlayerTrain.DriverCar].CameraRestrictionMode;
					}
				} else if (currentTrain.State != TrainState.Bogus) {
					TrainBase train = currentTrain as TrainBase;
					currentTrain.AI = new Game.SimpleHumanDriverAI(train, Interface.CurrentOptions.PrecedingTrainSpeedLimit);
					currentTrain.TimetableDelta = Program.CurrentRoute.PrecedingTrainTimeDeltas[k - 1];
					// ReSharper disable once PossibleNullReferenceException - Will always succeed 
					train.Specs.DoorOpenMode = DoorMode.Manual;
					train.Specs.DoorCloseMode = DoorMode.Manual;
				}
			}
			for (int i = Program.TrainManager.TFOs.Count - 1; i >= 0; i--)
			{
				// HACK: Copy PreTrain type TFOs back into the main array so they affect signalling
				if (Program.TrainManager.TFOs[i].Type == TrainType.PreTrain)
				{
					Program.TrainManager.Trains.Add(Program.TrainManager.TFOs[i] as TrainBase);
					Array.Resize(ref Program.CurrentRoute.PrecedingTrainTimeDeltas, Program.CurrentRoute.PrecedingTrainTimeDeltas.Length + 1);
					Program.CurrentRoute.PrecedingTrainTimeDeltas[Program.CurrentRoute.PrecedingTrainTimeDeltas.Length - 1] = Program.TrainManager.TFOs[i].TimetableDelta;
					Program.TrainManager.TFOs.RemoveAt(i);
				}
			}

			if(TrainManagerBase.PlayerTrain.Cars[TrainManagerBase.PlayerTrain.DriverCar].CarSections.ContainsKey(CarSectionType.Interior))
			{
				Program.Renderer.Camera.CurrentMode = CameraViewMode.Interior;
			}
			else
			{
				Program.Renderer.Camera.CurrentMode = CameraViewMode.Exterior;
				Program.Renderer.Camera.CurrentRestriction = CameraRestrictionMode.Off;
			}
			
			// finished created objects
			Thread.Sleep(1); if (Cancel) return;
			Array.Resize(ref ObjectManager.AnimatedWorldObjects, ObjectManager.AnimatedWorldObjectsUsed);
			// update sections
			if (Program.CurrentRoute.Sections.Length > 0) {
				Program.CurrentRoute.UpdateAllSections();
			}
			// load plugin
			for (int i = 0; i < Program.TrainManager.Trains.Count; i++) {
				if ( Program.TrainManager.Trains[i].State != TrainState.Bogus) {
					if (Program.TrainManager.Trains[i].IsPlayerTrain && !string.IsNullOrEmpty(Program.TrainManager.Trains[i].TrainFolder)) {
						if (Program.TrainManager.Trains[i].Plugin == null && !Program.TrainManager.Trains[i].LoadCustomPlugin(Program.TrainManager.Trains[i].TrainFolder, CurrentTrainEncoding)) {
							Program.TrainManager.Trains[i].LoadDefaultPlugin(Program.TrainManager.Trains[i].TrainFolder);
						}
					} else {
						Program.TrainManager.Trains[i].LoadDefaultPlugin( Program.TrainManager.Trains[i].TrainFolder);
					}
					if (Program.TrainManager.Trains[i].IsPlayerTrain)
					{
						for (int j = 0; j < InputDevicePlugin.AvailablePluginInfos.Count; j++)
						{
							if (InputDevicePlugin.AvailablePluginInfos[j].Status == InputDevicePlugin.PluginInfo.PluginStatus.Enable && InputDevicePlugin.AvailablePlugins[j] is ITrainInputDevice trainInputDevice)
							{
								trainInputDevice.SetVehicleSpecs(Program.TrainManager.Trains[i].GetVehicleSpecs());
							}
						}
					}
				}
			}

			
		}

	}
}
