// ╔═════════════════════════════════════════════════════════════╗
// ║ Loading.cs for the Route Viewer                             ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using LibRender;
using OpenBve.SignalManager;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Runtime;

namespace OpenBve {
	internal static class Loading {

		// members
		internal static double RouteProgress;
		internal static double TrainProgress;
		internal static bool Cancel;
		internal static bool Complete;
		private static Thread Loader = null;
		private static string CurrentRouteFile;
		private static Encoding CurrentRouteEncoding;
		internal static double TrainProgressCurrentSum;
		internal static double TrainProgressCurrentWeight;

		internal static bool JobAvailable;

		// load
		internal static void Load(string RouteFile, Encoding RouteEncoding) {
			// members
			Renderer.InitLoading();
			RouteProgress = 0.0;
			TrainProgress = 0.0;
			TrainProgressCurrentSum = 0.0;
			TrainProgressCurrentWeight = 1.0;
			Cancel = false;
			Complete = false;
			CurrentRouteFile = RouteFile;
			CurrentRouteEncoding = RouteEncoding;
			// thread
			Loading.LoadAsynchronously(CurrentRouteFile, Encoding.UTF8);
			RouteViewer.LoadingScreenLoop();
		}

		// get railway folder
		private static string GetRailwayFolder(string RouteFile) {
			try {
				string Folder = System.IO.Path.GetDirectoryName(RouteFile);
				while (true) {
					string Subfolder = OpenBveApi.Path.CombineDirectory(Folder, "Railway");
					if (System.IO.Directory.Exists(Subfolder)) {
						return Subfolder;
					}
					System.IO.DirectoryInfo Info = System.IO.Directory.GetParent(Folder);
					if (Info == null) break;
					Folder = Info.FullName;
				}
			} catch { }
			//If the Route, Object and Sound folders exist, but are not in a railway folder.....
			try
			{
				string Folder = System.IO.Path.GetDirectoryName(RouteFile);
				while (true)
				{
					string RouteFolder = OpenBveApi.Path.CombineDirectory(Folder, "Route");
					string ObjectFolder = OpenBveApi.Path.CombineDirectory(Folder, "Object"); 
					string SoundFolder = OpenBveApi.Path.CombineDirectory(Folder, "Sound");
					if (System.IO.Directory.Exists(RouteFolder) && System.IO.Directory.Exists(ObjectFolder) && System.IO.Directory.Exists(SoundFolder))
					{
						return Folder;
					}
					System.IO.DirectoryInfo Info = System.IO.Directory.GetParent(Folder);
					if (Info == null) break;
					Folder = Info.FullName;
				}
			}
			catch { }
			return Application.StartupPath;
		}

		// load threaded
		private static void LoadThreaded() {
			#if DEBUG
			LoadEverythingThreaded();
			#else
			try {
				LoadEverythingThreaded();
			} catch (Exception ex) {
				Interface.AddMessage(MessageType.Critical, false, "The route and train loader encountered the following critical error: " + ex.Message);
			}
			#endif
			Complete = true;
		}

		internal static void LoadAsynchronously(string RouteFile, Encoding RouteEncoding)
		{
			// members
			RouteProgress = 0.0;
			TrainProgress = 0.0;
			TrainProgressCurrentSum = 0.0;
			TrainProgressCurrentWeight = 1.0;
			Cancel = false;
			Complete = false;
			CurrentRouteFile = RouteFile;
			CurrentRouteEncoding = RouteEncoding;
			
			//Set the route and train folders in the info class
			Loader = new Thread(LoadThreaded) { IsBackground = true };
			Loader.Start();
		}

		private static void LoadEverythingThreaded() {
			string RailwayFolder = GetRailwayFolder(CurrentRouteFile);
			string ObjectFolder = OpenBveApi.Path.CombineDirectory(RailwayFolder, "Object");
			string SoundFolder = OpenBveApi.Path.CombineDirectory(RailwayFolder, "Sound");
			// string CompatibilityFolder = OpenBveApi.Path.CombineDirectory(Application.StartupPath, "Compatibility");
			// reset
			Game.Reset();
			Game.MinimalisticSimulation = true;
			// screen
			World.CameraTrackFollower = new TrackManager.TrackFollower();
			World.CameraTrackFollower.Train = null;
			World.CameraTrackFollower.CarIndex = -1;
			World.CameraMode = CameraViewMode.Interior;
			// load route
			bool IsRW = string.Equals(System.IO.Path.GetExtension(CurrentRouteFile), ".rw", StringComparison.OrdinalIgnoreCase);
			CsvRwRouteParser.ParseRoute(CurrentRouteFile, IsRW, CurrentRouteEncoding, Application.StartupPath, ObjectFolder, SoundFolder, false);
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			Game.CalculateSeaLevelConstants();
			RouteProgress = 1.0;
			// camera
			ObjectManager.InitializeVisibility();
			TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, 0.0, true, false);
			TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, 0.1, true, false);
			TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, -0.1, true, false);
			World.CameraTrackFollower.TriggerType = TrackManager.EventTriggerType.Camera;
			// default starting time
			Game.SecondsSinceMidnight = 0.0;
			Game.StartupTime = 0.0;
			// finished created objects
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			ObjectManager.FinishCreatingObjects();
			// signals
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			if (CurrentRoute.Sections.Length > 0) {
				Game.UpdateSection(CurrentRoute.Sections.Length - 1);
			}
			// starting track position
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			// int FirstStationIndex = -1;
			double FirstStationPosition = 0.0;
			for (int i = 0; i < Game.Stations.Length; i++) {
				if (Game.Stations[i].Stops.Length != 0) {
					// FirstStationIndex = i;
					FirstStationPosition = Game.Stations[i].Stops[Game.Stations[i].Stops.Length - 1].TrackPosition;
					if (Game.Stations[i].ArrivalTime < 0.0) {
						if (Game.Stations[i].DepartureTime < 0.0) {
							Game.SecondsSinceMidnight = 0.0;
						} else {
							Game.SecondsSinceMidnight = Game.Stations[i].DepartureTime - Game.Stations[i].StopTime;
						}
					} else {
						Game.SecondsSinceMidnight = Game.Stations[i].ArrivalTime;
					}
					Game.StartupTime = Game.SecondsSinceMidnight;
					break;
				}
			}
			// initialize camera
			TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, -1.0, true, false);
			TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, FirstStationPosition, true, false);
			World.CameraCurrentAlignment = new CameraAlignment(new Vector3(0.0, 2.5, 0.0), 0.0, 0.0, 0.0, FirstStationPosition, 1.0);
			World.UpdateAbsoluteCamera(0.0);
			ObjectManager.UpdateVisibility(World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z);
		}

	}
}
