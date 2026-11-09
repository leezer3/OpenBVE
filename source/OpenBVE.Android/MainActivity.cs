// DroidBVE - main activity.
//
// Boots the portable OpenBVE core on Android and exposes a minimal driving UI
// (throttle / brake + speed readout). The simulation bootstrap below mirrors
// the verified flow in source/Mobile/OpenBve.Headless, which is the reference
// for anything added here.

using System;
using System.IO;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using OpenBveApi.FileSystem;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using RouteManager2;
using TrainManager;
using TrainManager.Handles;
using TrainManager.Trains;

namespace DroidBVE
{
	[Activity(Label = "DroidBVE", MainLauncher = true, ScreenOrientation = ScreenOrientation.SensorLandscape, ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.KeyboardHidden)]
	public class MainActivity : Activity
	{
		private AndroidHost host;
		private AndroidOptions options;
		private FileSystem fileSystem;
		private TrainBase playerTrain;

		private TextView statusText;
		private TextView speedText;
		private TextView positionText;
		private TextView messageText;
		private SeekBar powerBar;
		private SeekBar brakeBar;

		private Timer simTimer;
		private readonly object simLock = new object();

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// ---- UI (built in code to keep the skeleton self-contained) ----
			LinearLayout root = new LinearLayout(this) { Orientation = Orientation.Vertical };
			statusText = new TextView(this) { TextSize = 14 };
			speedText = new TextView(this) { TextSize = 48 };
			positionText = new TextView(this) { TextSize = 14 };
			messageText = new TextView(this) { TextSize = 12 };
			powerBar = new SeekBar(this);
			brakeBar = new SeekBar(this);
			root.AddView(statusText);
			root.AddView(speedText);
			root.AddView(positionText);
			root.AddView(new TextView(this) { Text = "Throttle" });
			root.AddView(powerBar);
			root.AddView(new TextView(this) { Text = "Brake" });
			root.AddView(brakeBar);
			root.AddView(messageText);
			SetContentView(root);

			statusText.Text = "Loading simulation...";

			// ---- boot the simulation on a background thread ----
			new Thread(InitializeSimulation) { IsBackground = true }.Start();
		}

		protected override void OnDestroy()
		{
			simTimer?.Dispose();
			base.OnDestroy();
		}

		private void InitializeSimulation()
		{
			try
			{
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

				// Android asset packs are not real files: unpack what the
				// parsers need (data folder + test content) to app storage.
				string filesDir = FilesDir.AbsolutePath;
				string dataFolder = Path.Combine(filesDir, "Data");
				string testData = Path.Combine(filesDir, "TestData");
				CopyAssetFolder(Assets, "Compatibility", Path.Combine(dataFolder, "Compatibility"));
				CopyAssetFolder(Assets, "Languages", Path.Combine(dataFolder, "Languages"));
				CopyAssetFolder(Assets, "TestData", testData);

				host = new AndroidHost();
				options = new AndroidOptions();
				options.TrainStart = TrainStartMode.ServiceBrakesAts;
				fileSystem = FileSystem.FromCommandLineArgs(new string[0], host);
				fileSystem.DataFolder = dataFolder;
				fileSystem.SettingsFolder = Path.Combine(filesDir, "Settings");
				fileSystem.InitialRouteFolder = Path.Combine(filesDir, "Railway");
				fileSystem.InitialTrainFolder = Path.Combine(filesDir, "Train");
				fileSystem.CreateFileSystem();

				Translations.LoadLanguageFiles(fileSystem.GetDataFolder("Languages"));
				host.MessageReceived += message => RunOnUiThread(() => messageText.Text = message);

				AndroidRenderer renderer = new AndroidRenderer(host, options, fileSystem);
				AndroidTrainManager trainManager = new AndroidTrainManager(host, renderer, options, fileSystem);
				CurrentRoute currentRoute = new CurrentRoute(host, renderer);
				host.CurrentRoute = currentRoute;

				string routeFile = Path.Combine(testData, "TestRoute.csv");
				string trainFolder = Path.Combine(testData, "TestTrain");

				CsvRwRouteParser.Plugin routePlugin = new CsvRwRouteParser.Plugin();
				routePlugin.Load(host, fileSystem, options, trainManager);
				object route = currentRoute;
				if (!routePlugin.LoadRoute(routeFile, Encoding.UTF8, trainFolder, testData, testData, false, ref route))
				{
					RunOnUiThread(() => statusText.Text = "Route failed to load, see logcat.");
					return;
				}

				Train.OpenBve.Plugin trainPlugin = new Train.OpenBve.Plugin();
				trainPlugin.Load(host, fileSystem, options, renderer);
				playerTrain = new TrainBase(TrainState.Pending, TrainType.LocalPlayerTrain);
				TrainManagerBase.PlayerTrain = playerTrain;
				AbstractTrain abstractTrain = playerTrain;
				Control[] controls = new Control[0];
				if (!trainPlugin.LoadTrain(Encoding.UTF8, trainFolder, ref abstractTrain, ref controls))
				{
					RunOnUiThread(() => statusText.Text = "Train failed to load, see logcat.");
					return;
				}

				TrainManagerBase.CurrentRoute = (CurrentRoute)route;
				playerTrain.PlaceCars(50.0);
				// PlaceCars only positions the followers; prime the world
				// coordinates so the physics adhesion model works immediately.
				for (int i = 0; i < playerTrain.Cars.Length; i++)
				{
					playerTrain.Cars[i].FrontAxle.Follower.UpdateAbsolute(playerTrain.Cars[i].FrontAxle.Follower.TrackPosition, true, false);
					playerTrain.Cars[i].RearAxle.Follower.UpdateAbsolute(playerTrain.Cars[i].RearAxle.Follower.TrackPosition, true, false);
				}
				playerTrain.Handles.Reverser.Actual = ReverserPosition.Forwards;
				playerTrain.Handles.Reverser.Driver = ReverserPosition.Forwards;

				// ---- driving controls ----
				RunOnUiThread(() =>
				{
					powerBar.Max = Math.Max(1, playerTrain.Handles.Power.MaximumNotch);
					brakeBar.Max = Math.Max(1, playerTrain.Handles.Brake.MaximumNotch);
					powerBar.ProgressChanged += (s, e) =>
					{
						lock (simLock)
						{
							playerTrain.Handles.Power.Driver = e.Progress;
							playerTrain.Handles.Power.Actual = e.Progress;
						}
					};
					brakeBar.ProgressChanged += (s, e) =>
					{
						lock (simLock)
						{
							playerTrain.Handles.Brake.Driver = e.Progress;
							playerTrain.Handles.Brake.Actual = e.Progress;
						}
					};
					statusText.Text = "Ready - drag the throttle to drive.";
				});

				// ---- 10 Hz simulation tick (same step as the headless harness) ----
				simTimer = new Timer(SimTick, null, 100, 100);
			}
			catch (Exception ex)
			{
				RunOnUiThread(() => statusText.Text = "Startup failed: " + ex.Message);
			}
		}

		private void SimTick(object state)
		{
			TrainBase train = playerTrain;
			if (train == null)
			{
				return;
			}
			lock (simLock)
			{
				train.Update(0.1);
			}
			double kmh = train.CurrentSpeed * 3.6;
			double position = train.Cars[0].FrontAxle.Follower.TrackPosition;
			RunOnUiThread(() =>
			{
				speedText.Text = kmh.ToString("0.0") + " km/h";
				positionText.Text = "Position: " + position.ToString("0") + " m";
			});
		}

		/// <summary>Recursively unpacks an Android asset folder into real files.</summary>
		private static void CopyAssetFolder(Android.Content.Res.AssetManager assets, string assetPath, string targetFolder)
		{
			string[] children;
			try
			{
				children = assets.List(assetPath);
			}
			catch
			{
				return;
			}
			if (children == null || children.Length == 0)
			{
				// Leaf = file
				string fileName = Path.GetFileName(assetPath);
				string target = Path.Combine(targetFolder, fileName);
				Directory.CreateDirectory(Path.GetDirectoryName(target));
				using (Stream input = assets.Open(assetPath))
				using (FileStream output = File.Create(target))
				{
					input.CopyTo(output);
				}
				return;
			}
			Directory.CreateDirectory(targetFolder);
			foreach (string child in children)
			{
				CopyAssetFolder(assets, assetPath.Length == 0 ? child : assetPath + "/" + child, Path.Combine(targetFolder, child));
			}
		}
	}
}