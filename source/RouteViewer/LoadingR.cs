// ╔═════════════════════════════════════════════════════════════╗
// ║ Loading.cs for the Route Viewer                             ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibRender2.Cameras;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using RouteManager2;

namespace RouteViewer {
	internal static class Loading {

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
		internal static bool Complete;
		private static string CurrentRouteFile;
		private static Encoding CurrentRouteEncoding;

		internal static bool JobAvailable;

		// load
		internal static void Load(string RouteFile, Encoding RouteEncoding, Bitmap bitmap = null)
		{
			// reset
			Game.Reset();
			Program.Renderer.Loading.InitLoading(Program.FileSystem.GetDataFolder("In-game"), typeof(NewRenderer).Assembly.GetName().Version.ToString(), Interface.CurrentOptions.LoadingLogo, Interface.CurrentOptions.LoadingProgressBar);
			if (bitmap != null)
			{
				Program.Renderer.Loading.SetLoadingBkg(Program.Renderer.TextureManager.RegisterTexture(bitmap, new TextureParameters(null, null)));
			}
			// members
			Cancel = false;
			Complete = false;
			CurrentRouteFile = RouteFile;
			CurrentRouteEncoding = RouteEncoding;
			// thread
			Loading.LoadAsynchronously(CurrentRouteFile, CurrentRouteEncoding);
			RouteViewer.LoadingScreenLoop();
		}

		/// <summary>Gets the absolute Railway folder for a given route file</summary>
		/// <returns>The absolute on-disk path of the railway folder</returns>
		internal static string GetRailwayFolder(string RouteFile) {
			try
			{
				string Folder = System.IO.Path.GetDirectoryName(RouteFile);

				while (true)
				{
					string Subfolder = OpenBveApi.Path.CombineDirectory(Folder, "Railway");
					if (System.IO.Directory.Exists(Subfolder))
					{
						if (System.IO.Directory.EnumerateDirectories(Subfolder).Any() || System.IO.Directory.EnumerateFiles(Subfolder).Any())
						{
							//HACK: Ignore completely empty directories
							//Doesn't handle wrong directories, or those with stuff missing, TODO.....
							return Subfolder;
						}
					}

					if (Folder == null) continue;
					System.IO.DirectoryInfo Info = System.IO.Directory.GetParent(Folder);
					if (Info == null) break;
					Folder = Info.FullName;
				}
			}
			catch
			{
				//ignored
			}
			
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
						return Folder;
					}

					if (System.IO.Directory.Exists(RouteFolder) && System.IO.Directory.Exists(ObjectFolder))
					{
						candidate = Folder;
					}

					// ReSharper disable once AssignNullToNotNullAttribute
					System.IO.DirectoryInfo Info = System.IO.Directory.GetParent(Folder);
					if (Info == null)
					{
						if (candidate != null)
						{
							return candidate;
						}
						break;
					}
					Folder = Info.FullName;
				}
			}
			catch
			{
				//ignored
			}
			return Application.StartupPath;
		}

		// load threaded
		private static async Task LoadThreaded()
		{
			try
			{
				await Task.Run(() => LoadEverythingThreaded());
			}
			catch (Exception ex)
			{
				MessageBox.Show("The route loader encountered the following critical error: " + ex.Message, @"OpenBVE", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				Cancel = true;
			}
		}

		internal static void LoadAsynchronously(string RouteFile, Encoding RouteEncoding)
		{
			// members
			Cancel = false;
			Complete = false;
			CurrentRouteFile = RouteFile;
			CurrentRouteEncoding = RouteEncoding;

			//Set the route and train folders in the info class
			// ReSharper disable once UnusedVariable
			Task loadThreaded = LoadThreaded();
		}

		private static void LoadEverythingThreaded() {
			string RailwayFolder = GetRailwayFolder(CurrentRouteFile);
			string ObjectFolder = OpenBveApi.Path.CombineDirectory(RailwayFolder, "Object");
			string SoundFolder = OpenBveApi.Path.CombineDirectory(RailwayFolder, "Sound");
			Program.Renderer.Camera.CurrentMode = CameraViewMode.Track;
			// load route
			bool loaded = false;
			for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
			{
				if (Program.CurrentHost.Plugins[i].Route != null && Program.CurrentHost.Plugins[i].Route.CanLoadRoute(CurrentRouteFile))
				{
					object Route = (object)Program.CurrentRoute; //must cast to allow us to use the ref keyword.
					if (Program.CurrentHost.Plugins[i].Route.LoadRoute(CurrentRouteFile, CurrentRouteEncoding, null, ObjectFolder, SoundFolder, false, ref Route))
					{
						Program.CurrentRoute = (CurrentRoute) Route;
						Program.CurrentRoute.UpdateLighting();
						loaded = true;
						break;
					}
					throw Program.CurrentHost.Plugins[i].Route.LastException;
				}
			}

			if (!loaded)
			{
				throw new Exception("No plugins capable of loading routefile " + CurrentRouteFile + " were found.");
			}
			Program.Renderer.CameraTrackFollower = new TrackFollower(Program.CurrentHost);
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			Program.CurrentRoute.Atmosphere.CalculateSeaLevelConstants();
			// camera
			Program.Renderer.CameraTrackFollower.UpdateAbsolute( 0.0, true, false);
			Program.Renderer.CameraTrackFollower.UpdateAbsolute(0.1, true, false);
			Program.Renderer.CameraTrackFollower.UpdateAbsolute(-0.1, true, false);
			Program.Renderer.CameraTrackFollower.TriggerType = EventTriggerType.Camera;
			// default starting time
			Game.SecondsSinceMidnight = 0.0;
			// finished created objects
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			// signals
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			Program.CurrentRoute.UpdateAllSections();
			// starting track position
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			// int FirstStationIndex = -1;
			double FirstStationPosition = 0.0;
			for (int i = 0; i < Program.CurrentRoute.Stations.Length; i++) {
				if (Program.CurrentRoute.Stations[i].Stops.Length != 0) {
					// FirstStationIndex = i;
					FirstStationPosition = Program.CurrentRoute.Stations[i].Stops[Program.CurrentRoute.Stations[i].Stops.Length - 1].TrackPosition;
					if (Program.CurrentRoute.Stations[i].ArrivalTime < 0.0) {
						if (Program.CurrentRoute.Stations[i].DepartureTime < 0.0) {
							Game.SecondsSinceMidnight = 0.0;
						} else {
							Game.SecondsSinceMidnight = Program.CurrentRoute.Stations[i].DepartureTime - Program.CurrentRoute.Stations[i].StopTime;
						}
					} else {
						Game.SecondsSinceMidnight = Program.CurrentRoute.Stations[i].ArrivalTime;
					}
					break;
				}
			}
			// initialize camera
			Program.Renderer.CameraTrackFollower.UpdateAbsolute(-1.0, true, false);
			Program.Renderer.CameraTrackFollower.UpdateAbsolute(FirstStationPosition, true, false);
			Program.Renderer.Camera.Alignment = new CameraAlignment(new Vector3(0.0, 2.5, 0.0), 0.0, 0.0, 0.0, FirstStationPosition, 1.0);
			World.UpdateAbsoluteCamera(0.0);
			Complete = true;
		}

	}
}
