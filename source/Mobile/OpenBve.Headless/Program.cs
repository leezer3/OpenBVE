// Headless smoke test for the mobile (net8.0) build of the OpenBVE core.
//
// Proves that route + train loading works on .NET 8 without any desktop UI,
// GDI or OpenGL context - i.e. the same runtime family used by net8.0-android.

using System;
using System.IO;
using System.Text;
using OpenBveApi.FileSystem;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using RouteManager2;
using TrainManager;
using TrainManager.Handles;
using TrainManager.Trains;

namespace OpenBve.Headless
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			try
			{
				return RunSmokeTest();
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("FATAL: " + ex);
				return 1;
			}
		}

		private static int RunSmokeTest()
		{
			// Legacy BVE content is usually Shift_JIS encoded
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			HeadlessHost host = new HeadlessHost();
			HeadlessOptions options = new HeadlessOptions();
			options.TrainStart = TrainStartMode.ServiceBrakesAts;
			FileSystem fileSystem = FileSystem.FromCommandLineArgs(new string[0], host);
			fileSystem.DataFolder = LocateAssetsFolder();
			fileSystem.SettingsFolder = Path.Combine(Path.GetTempPath(), "OpenBveHeadless", "Settings");
			fileSystem.InitialRouteFolder = Path.Combine(Path.GetTempPath(), "OpenBveHeadless", "Railway");
			fileSystem.InitialTrainFolder = Path.Combine(Path.GetTempPath(), "OpenBveHeadless", "Train");
			fileSystem.CreateFileSystem();

			Translations.LoadLanguageFiles(fileSystem.GetDataFolder("Languages"));

			HeadlessRenderer renderer = new HeadlessRenderer(host, options, fileSystem);
			HeadlessTrainManager trainManager = new HeadlessTrainManager(host, renderer, options, fileSystem);
			CurrentRoute currentRoute = new CurrentRoute(host, renderer);
			host.CurrentRoute = currentRoute;

			string testData = Path.Combine(AppContext.BaseDirectory, "TestData");
			string routeFile = Path.Combine(testData, "TestRoute.csv");
			string trainFolder = Path.Combine(testData, "TestTrain");

			int failures = 0;

			// ---- route ----
			Console.WriteLine("== Loading route: " + routeFile);
			CsvRwRouteParser.Plugin routePlugin = new CsvRwRouteParser.Plugin();
			routePlugin.Load(host, fileSystem, options, trainManager);
			object route = currentRoute;
			bool routeLoaded = routePlugin.LoadRoute(routeFile, Encoding.UTF8, trainFolder, testData, testData, false, ref route);
			CurrentRoute loadedRoute = route as CurrentRoute;

			failures += Check("route loaded", routeLoaded && loadedRoute != null);
			if (loadedRoute != null)
			{
				Console.WriteLine("   Tracks:   " + loadedRoute.Tracks.Count);
				Console.WriteLine("   Stations: " + loadedRoute.Stations.Length);
				failures += Check("route has tracks", loadedRoute.Tracks.Count > 0);
				failures += Check("route has a station", loadedRoute.Stations.Length > 0);
				if (loadedRoute.Stations.Length > 0)
				{
					Console.WriteLine("   First station: " + loadedRoute.Stations[0].Name);
					failures += Check("station name parsed", loadedRoute.Stations[0].Name == "Test Station");
				}
			}

			// ---- train ----
			Console.WriteLine("== Loading train: " + trainFolder);
			Train.OpenBve.Plugin trainPlugin = new Train.OpenBve.Plugin();
			trainPlugin.Load(host, fileSystem, options, renderer);
			TrainBase playerTrain = new TrainBase(TrainState.Pending, TrainType.LocalPlayerTrain);
			TrainManagerBase.PlayerTrain = playerTrain;
			AbstractTrain abstractTrain = playerTrain;
			Control[] controls = new Control[0];
			bool trainLoaded = trainPlugin.LoadTrain(Encoding.UTF8, trainFolder, ref abstractTrain, ref controls);

			failures += Check("train loaded", trainLoaded);
			failures += Check("train has cars", playerTrain.Cars != null && playerTrain.Cars.Length > 0);
			if (playerTrain.Cars != null && playerTrain.Cars.Length > 0)
			{
				Console.WriteLine("   Cars:     " + playerTrain.Cars.Length);
				Console.WriteLine("   Car 0 mass: " + playerTrain.Cars[0].CurrentMass + " kg");
				failures += Check("train has 2 cars (1 motor + 1 trailer)", playerTrain.Cars.Length == 2);
			}

			Console.WriteLine();
			// ---- simulation ticks (proves the physics engine runs on .NET 8) ----
			Console.WriteLine("== Running the simulation for 20 s (200 x 0.1 s) under power");
			TrainManagerBase.CurrentRoute = loadedRoute;
			playerTrain.PlaceCars(50.0);
			// PlaceCars only positions the followers; the world coordinates (needed by the
			// physics adhesion model) are filled in on the first movement, so prime them here.
			for (int i = 0; i < playerTrain.Cars.Length; i++)
			{
				playerTrain.Cars[i].FrontAxle.Follower.UpdateAbsolute(playerTrain.Cars[i].FrontAxle.Follower.TrackPosition, true, false);
				playerTrain.Cars[i].RearAxle.Follower.UpdateAbsolute(playerTrain.Cars[i].RearAxle.Follower.TrackPosition, true, false);
			}
			playerTrain.Handles.Reverser.Actual = ReverserPosition.Forwards;
			playerTrain.Handles.Reverser.Driver = ReverserPosition.Forwards;
			playerTrain.Handles.Power.Driver = 1;
			playerTrain.Handles.Power.Actual = 1;
			playerTrain.Handles.Brake.Driver = 0;
			playerTrain.Handles.Brake.Actual = 0;
			for (int i = 0; i < 200; i++)
			{
				playerTrain.Update(0.1);
				if (i % 50 == 0)
				{
					Console.WriteLine("   t=" + (i * 0.1).ToString("0.0") + " s  speed=" + (playerTrain.CurrentSpeed * 3.6).ToString("0.0") + " km/h  position=" + playerTrain.Cars[0].FrontAxle.Follower.TrackPosition.ToString("0.0") + " m");
				}
			}
			double speed = playerTrain.CurrentSpeed;
			Console.WriteLine("   Speed after 20 s: " + (speed * 3.6).ToString("0.0") + " km/h");
			failures += Check("train accelerates under power", speed > 0.5);

			Console.WriteLine();
			Console.WriteLine("Host messages with severity Error/Critical: " + host.ErrorCount);
			if (failures == 0)
			{
				Console.WriteLine("HEADLESS SMOKE TEST PASSED");
				return 0;
			}
			Console.WriteLine("HEADLESS SMOKE TEST FAILED (" + failures + " check(s) failed)");
			return 1;
		}

		private static int Check(string description, bool condition)
		{
			Console.WriteLine((condition ? "PASS: " : "FAIL: ") + description);
			return condition ? 0 : 1;
		}

		/// <summary>Walks up from the binary folder until the repository's assets folder is found.</summary>
		private static string LocateAssetsFolder()
		{
			DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory);
			for (int i = 0; i < 8 && dir != null; i++)
			{
				string candidate = Path.Combine(dir.FullName, "assets");
				if (File.Exists(Path.Combine(candidate, "Compatibility", "CompatibilityObjects.xml")))
				{
					return candidate;
				}
				dir = dir.Parent;
			}
			throw new DirectoryNotFoundException("Could not locate the OpenBVE assets folder above " + AppContext.BaseDirectory);
		}
	}
}