using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;


namespace OpenBve {
	internal static partial class Loading {

		// members
		/// <summary>The current route loading progress</summary>
		internal static double RouteProgress;
		/// <summary>The current train loading progress</summary>
		internal static double TrainProgress;

		private static double TrainProgressMaximum;
		/// <summary>Set this member to true to cancel loading</summary>
		internal static bool Cancel;
		/// <summary>Set this member to true to pause loading</summary>
		internal static bool Pause;
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
		/// <summary>Stores the plugin error message string, or a null reference if no error encountered</summary>
		internal static string PluginError;

		private static ObjectManager.UnifiedObject[] CarObjects = null;
		private static ObjectManager.UnifiedObject[] BogieObjects = null;

		// load
		/// <summary>Initializes loading the route and train asynchronously. Set the Loading.Cancel member to cancel loading. Check the Loading.Complete member to see when loading has finished.</summary>
		internal static void LoadAsynchronously(string RouteFile, Encoding RouteEncoding, string TrainFolder, Encoding TrainEncoding) {
			// members
			RouteProgress = 0.0;
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
			Game.RouteInformation.RouteFile = RouteFile;
			Game.RouteInformation.TrainFolder = TrainFolder;
			Game.RouteInformation.FilesNotFound = null;
			Game.RouteInformation.ErrorsAndWarnings = null;
			Loader = new Thread(LoadThreaded) {IsBackground = true};
			Loader.Start();
		}

		/// <summary>Gets the absolute Railway folder for a given route file</summary>
		/// <returns>The absolute on-disk path of the railway folder</returns>
		private static string GetRailwayFolder(string RouteFile) {
			try {
				string Folder = System.IO.Path.GetDirectoryName(RouteFile);
				
				while (true) {
					string Subfolder = OpenBveApi.Path.CombineDirectory(Folder, "Railway");
					if (System.IO.Directory.Exists(Subfolder)) {
						if (System.IO.Directory.EnumerateDirectories(Subfolder).Any() || System.IO.Directory.EnumerateFiles(Subfolder).Any())
						{
							//HACK: Ignore completely empty directories
							//Doesn't handle wrong directories, or those with stuff missing, TODO.....
							Program.AppendToLogFile(Subfolder + " : Railway folder found.");
							return Subfolder;
						}
					Program.AppendToLogFile(Subfolder + " : Railway folder candidate rejected- Directory empty.");
						
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
						Program.AppendToLogFile(Folder + " : Railway folder found.");
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
							Program.AppendToLogFile(Folder + " : The best candidate for the Railway folder has been selected- Sound folder not detected.");
							return candidate;
						}
						break;
					}
					Folder = Info.FullName;
				}
			}
			catch { }
			Program.AppendToLogFile("No Railway folder found- Returning the openBVE startup path.");
			return Application.StartupPath;
		}

		// load threaded
		private static void LoadThreaded() {
			string[] Lines;
			try {
				LoadEverythingThreaded();
			} catch (Exception ex) {
				for (int i = 0; i < TrainManager.Trains.Length; i++) {
					if (TrainManager.Trains[i] != null && TrainManager.Trains[i].Plugin != null) {
						if (TrainManager.Trains[i].Plugin.LastException != null) {
							Lines = new string[]
							{
								"The train plugin:",
								TrainManager.Trains[i].Plugin.PluginTitle, 
								"caused a critical error in the route and train loader.",
								"Please inspect the error log for further information."
							};
							Game.Menu.ShowMessageBox("Error", Lines);
							Interface.AddMessage(Interface.MessageType.Critical, false, "The train plugin " + TrainManager.Trains[i].Plugin.PluginTitle + " caused a critical error in the route and train loader: " + TrainManager.Trains[i].Plugin.LastException.Message);
							CrashHandler.LoadingCrash(TrainManager.Trains[i].Plugin.LastException + Environment.StackTrace, true);
							 Program.RestartArguments = " ";
							return;
						}
					}
				}
				
				if (ex is System.DllNotFoundException)
				{
					Pause = true;
					Interface.AddMessage(Interface.MessageType.Critical, false, "The required system library " + ex.Message + " was not found on the system.");
					
					switch (ex.Message)
					{
						case "libopenal.so.1":
							Lines = new string[]
							{
								"openAL was not found on this system.",
								"Please install libopenal1 via your distribtion's package management system."
							};
							Game.Menu.ShowMessageBox("Error", Lines);
							break;
						default:
							Lines = new string[]
							{
								"The required system library:",
								ex.Message,
								"was not found on this system."
							};
							Game.Menu.ShowMessageBox("Error", Lines);
							break;
					}
				}
				else
				{
					Lines = new string[]
					{
						"he route and train loader encountered the following critical error: ",
						ex.Message,
						"Please inspect the error log for further information."
					};
					Game.Menu.ShowMessageBox("Critical Error", Lines);
					CrashHandler.LoadingCrash(ex + Environment.StackTrace, false);
				}
				Program.RestartArguments = " ";           
			}
			if (JobAvailable || Pause)
			{
				Thread.Sleep(10);
			}
			Complete = true;
		}
		private static void LoadEverythingThreaded() {
			Program.AppendToLogFile("Loading route file: " + CurrentRouteFile);
			string RailwayFolder = GetRailwayFolder(CurrentRouteFile);
			string ObjectFolder = OpenBveApi.Path.CombineDirectory(RailwayFolder, "Object");
			string SoundFolder = OpenBveApi.Path.CombineDirectory(RailwayFolder, "Sound");
			// reset
			Game.Reset(true);
			Game.MinimalisticSimulation = true;
			// screen
			World.CameraTrackFollower = new TrackManager.TrackFollower{ Train = null, CarIndex = -1 };
			World.CameraMode = World.CameraViewMode.Interior;
			//First, check the format of the route file
			//RW routes were written for BVE1 / 2, and have a different command syntax
			bool IsRW = CsvRwRouteParser.isRWFile(CurrentRouteFile);
			Program.AppendToLogFile("Route file format is: " + (IsRW ? "RW" : "CSV"));
			CsvRwRouteParser.ParseRoute(CurrentRouteFile, IsRW, CurrentRouteEncoding, CurrentTrainFolder, ObjectFolder, SoundFolder, false);
			Thread createIllustrations = new Thread(Game.RouteInformation.LoadInformation) {IsBackground = true};
			createIllustrations.Start();
			Thread.Sleep(1); if (Cancel) return; while(Pause) Thread.Sleep(1); while(Pause) Thread.Sleep(1);

			Game.CalculateSeaLevelConstants();
			if (Game.BogusPretrainInstructions.Length != 0) {
				double t = Game.BogusPretrainInstructions[0].Time;
				double p = Game.BogusPretrainInstructions[0].TrackPosition;
				for (int i = 1; i < Game.BogusPretrainInstructions.Length; i++) {
					if (Game.BogusPretrainInstructions[i].Time > t) {
						t = Game.BogusPretrainInstructions[i].Time;
					} else {
						t += 1.0;
						Game.BogusPretrainInstructions[i].Time = t;
					}
					if (Game.BogusPretrainInstructions[i].TrackPosition > p) {
						p = Game.BogusPretrainInstructions[i].TrackPosition;
					} else {
						p += 1.0;
						Game.BogusPretrainInstructions[i].TrackPosition = p;
					}
				}
			}

			if (Game.Stations.Length == 1)
			{
				//Log the fact that only a single station is present, as this is probably not right
				Program.AppendToLogFile("The processed route file only contains a single station.");
			}
			Program.AppendToLogFile("Route file loaded successfully.");
			RouteProgress = 1.0;
			// initialize trains
			Thread.Sleep(1); if (Cancel) return; while(Pause) Thread.Sleep(1);
			TrainManager.Trains = new TrainManager.Train[Game.PrecedingTrainTimeDeltas.Length + 1 + (Game.BogusPretrainInstructions.Length != 0 ? 1 : 0)];
			for (int k = 0; k < TrainManager.Trains.Length; k++)
			{
				if (k == TrainManager.Trains.Length - 1 & Game.BogusPretrainInstructions.Length != 0)
				{
					TrainManager.Trains[k] = new TrainManager.Train(k, TrainManager.TrainState.Bogus);
				}
				else
				{
					TrainManager.Trains[k] = new TrainManager.Train(k, TrainManager.TrainState.Pending);
				}
				Thread.Sleep(1); if (Cancel) return; while(Pause) Thread.Sleep(1);
			}
			TrainManager.PlayerTrain = TrainManager.Trains[Game.PrecedingTrainTimeDeltas.Length];

			

			// load trains
			TrainProgressMaximum = 0.7 + 0.3 * (double)TrainManager.Trains.Length;
			for (int k = 0; k < TrainManager.Trains.Length; k++)
			{
				//Sleep for 20ms to allow route loading locks to release
				Thread.Sleep(20);
				if (TrainManager.Trains[k].State == TrainManager.TrainState.Bogus)
				{
					LoadBogusTrain(ref TrainManager.Trains[k]);
				}
				else
				{
					LoadBve4Train(ref TrainManager.Trains[k]);
				}
				Thread.Sleep(1); if (Cancel) return; while(Pause) Thread.Sleep(1);
				TrainManager.Trains[k].PlaceCars(0.0);

				// configure ai / timetable
				if (TrainManager.Trains[k] == TrainManager.PlayerTrain)
				{
					TrainManager.Trains[k].TimetableDelta = 0.0;
				}
				else if (TrainManager.Trains[k].State != TrainManager.TrainState.Bogus)
				{
					TrainManager.Trains[k].AI = new Game.SimpleHumanDriverAI(TrainManager.Trains[k]);
					TrainManager.Trains[k].TimetableDelta = Game.PrecedingTrainTimeDeltas[k];
					TrainManager.Trains[k].Specs.DoorOpenMode = TrainManager.DoorMode.Manual;
					TrainManager.Trains[k].Specs.DoorCloseMode = TrainManager.DoorMode.Manual;
				}
			}

			TrainProgress = 1.0;
			// finished created objects
			Thread.Sleep(1); if (Cancel) return; while(Pause) Thread.Sleep(1);
			Array.Resize(ref ObjectManager.Objects, ObjectManager.ObjectsUsed);
			Array.Resize(ref ObjectManager.AnimatedWorldObjects, ObjectManager.AnimatedWorldObjectsUsed);
			// update sections
			if (Game.Sections.Length > 0) {
				Game.UpdateSection(Game.Sections.Length - 1);
			}
			// load plugin
			for (int i = 0; i < TrainManager.Trains.Length; i++) {
				if (TrainManager.Trains[i].State != TrainManager.TrainState.Bogus) {
					if (TrainManager.Trains[i] == TrainManager.PlayerTrain) {
						if (!PluginManager.LoadCustomPlugin(TrainManager.Trains[i], TrainManager.Trains[i].TrainFolder, CurrentTrainEncoding)) {
							PluginManager.LoadDefaultPlugin(TrainManager.Trains[i], TrainManager.Trains[i].TrainFolder);
						}
					} else {
						PluginManager.LoadDefaultPlugin(TrainManager.Trains[i], TrainManager.Trains[i].TrainFolder);
					}
				}
			}
		}

	}
}
