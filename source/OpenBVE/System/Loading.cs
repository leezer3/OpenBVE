﻿using System;
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

namespace OpenBve {
	internal static class Loading {

		// members
		/// <summary>The current route loading progress</summary>
		internal static double RouteProgress;
		/// <summary>The current train loading progress</summary>
		internal static double TrainProgress;
		/// <summary>Set this member to true to cancel loading</summary>
		internal static bool Cancel;
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
				for (int i = 0; i < TrainManager.Trains.Length; i++) {
					if (TrainManager.Trains[i] != null && TrainManager.Trains[i].Plugin != null) {
						if (TrainManager.Trains[i].Plugin.LastException != null) {
							Interface.AddMessage(MessageType.Critical, false, "The train plugin " + TrainManager.Trains[i].Plugin.PluginTitle + " caused a critical error in the route and train loader: " + TrainManager.Trains[i].Plugin.LastException.Message);
							CrashHandler.LoadingCrash(TrainManager.Trains[i].Plugin.LastException + Environment.StackTrace, true);
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
			Program.FileSystem.AppendToLogFile("Loading route file: " + CurrentRouteFile);
			Program.FileSystem.AppendToLogFile("INFO: Route file hash " + CsvRwRouteParser.GetChecksum(CurrentRouteFile));
			string RailwayFolder = GetRailwayFolder(CurrentRouteFile);
			string ObjectFolder = OpenBveApi.Path.CombineDirectory(RailwayFolder, "Object");
			string SoundFolder = OpenBveApi.Path.CombineDirectory(RailwayFolder, "Sound");
			Game.Reset(true, false);
			Game.MinimalisticSimulation = true;
			// screen
			Program.Renderer.Camera.CurrentMode = CameraViewMode.Interior;
			//First, check the format of the route file
			//RW routes were written for BVE1 / 2, and have a different command syntax
			bool IsRW = CsvRwRouteParser.isRWFile(CurrentRouteFile);
			Program.FileSystem.AppendToLogFile("Route file format is: " + (IsRW ? "RW" : "CSV"));
			CsvRwRouteParser.ParseRoute(CurrentRouteFile, IsRW, CurrentRouteEncoding, CurrentTrainFolder, ObjectFolder, SoundFolder, false);
			Thread createIllustrations = new Thread(Game.RouteInformation.LoadInformation) {IsBackground = true};
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
			World.CameraTrackFollower = new TrackFollower(Program.CurrentHost) { Train = null, Car = null };
			if (Program.CurrentRoute.Stations.Length == 1)
			{
				//Log the fact that only a single station is present, as this is probably not right
				Program.FileSystem.AppendToLogFile("The processed route file only contains a single station.");
			}
			Program.FileSystem.AppendToLogFile("Route file loaded successfully.");
			RouteProgress = 1.0;
			// initialize trains
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			TrainManager.Trains = new TrainManager.Train[Game.PrecedingTrainTimeDeltas.Length + 1 + (Program.CurrentRoute.BogusPreTrainInstructions.Length != 0 ? 1 : 0)];
			for (int k = 0; k < TrainManager.Trains.Length; k++)
			{
				if (k == TrainManager.Trains.Length - 1 & Program.CurrentRoute.BogusPreTrainInstructions.Length != 0)
				{
					TrainManager.Trains[k] = new TrainManager.Train(TrainState.Bogus);
				}
				else
				{
					TrainManager.Trains[k] = new TrainManager.Train(TrainState.Pending);
				}
				
			}
			TrainManager.PlayerTrain = TrainManager.Trains[Game.PrecedingTrainTimeDeltas.Length];

			UnifiedObject[] CarObjects = null;
			UnifiedObject[] BogieObjects = null;
			UnifiedObject[] CouplerObjects = null;

			// load trains
			double TrainProgressMaximum = 0.7 + 0.3 * (double)TrainManager.Trains.Length;
			for (int k = 0; k < TrainManager.Trains.Length; k++) {
				//Sleep for 20ms to allow route loading locks to release
				Thread.Sleep(20);
				if (TrainManager.Trains[k].State == TrainState.Bogus) {
					// bogus train
					string TrainData = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Compatibility", "PreTrain"), "train.dat");
					TrainDatParser.ParseTrainData(TrainData, System.Text.Encoding.UTF8, TrainManager.Trains[k]);
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					TrainManager.Trains[k].InitializeCarSounds();
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					TrainProgressCurrentWeight = 0.3 / TrainProgressMaximum;
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
				} else {
					TrainManager.Trains[k].TrainFolder = CurrentTrainFolder;
					// real train
					if (TrainManager.Trains[k].IsPlayerTrain)
					{
						Program.FileSystem.AppendToLogFile("Loading player train: " + TrainManager.Trains[k].TrainFolder);
					}
					else
					{
						Program.FileSystem.AppendToLogFile("Loading AI train: " + TrainManager.Trains[k].TrainFolder);
					}
					TrainProgressCurrentWeight = 0.1 / TrainProgressMaximum;
					string TrainData = OpenBveApi.Path.CombineFile(TrainManager.Trains[k].TrainFolder, "train.dat");
					TrainDatParser.ParseTrainData(TrainData, CurrentTrainEncoding, TrainManager.Trains[k]);
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					TrainProgressCurrentWeight = 0.2 / TrainProgressMaximum;
					SoundCfgParser.ParseSoundConfig(TrainManager.Trains[k].TrainFolder, CurrentTrainEncoding, TrainManager.Trains[k]);
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					// door open/close speed
					for (int i = 0; i < TrainManager.Trains[k].Cars.Length; i++) {
						if (TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency <= 0.0) {
							if (TrainManager.Trains[k].Cars[i].Doors[0].OpenSound.Buffer != null & TrainManager.Trains[k].Cars[i].Doors[1].OpenSound.Buffer != null) {
								Program.Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Doors[0].OpenSound.Buffer);
								Program.Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Doors[1].OpenSound.Buffer);
								double a = TrainManager.Trains[k].Cars[i].Doors[0].OpenSound.Buffer.Duration;
								double b = TrainManager.Trains[k].Cars[i].Doors[1].OpenSound.Buffer.Duration;
								TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency = a + b > 0.0 ? 2.0 / (a + b) : 0.8;
							} else if (TrainManager.Trains[k].Cars[i].Doors[0].OpenSound.Buffer != null) {
								Program.Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Doors[0].OpenSound.Buffer);
								double a = TrainManager.Trains[k].Cars[i].Doors[0].OpenSound.Buffer.Duration;
								TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency = a > 0.0 ? 1.0 / a : 0.8;
							} else if (TrainManager.Trains[k].Cars[i].Doors[1].OpenSound.Buffer != null) {
								Program.Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Doors[0].OpenSound.Buffer);
								double b = TrainManager.Trains[k].Cars[i].Doors[1].OpenSound.Buffer.Duration;
								TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency = b > 0.0 ? 1.0 / b : 0.8;
							} else {
								TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency = 0.8;
							}
						}
						if (TrainManager.Trains[k].Cars[i].Specs.DoorCloseFrequency <= 0.0) {
							if (TrainManager.Trains[k].Cars[i].Doors[0].CloseSound.Buffer != null & TrainManager.Trains[k].Cars[i].Doors[1].CloseSound.Buffer != null) {
								Program.Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Doors[0].CloseSound.Buffer);
								Program.Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Doors[1].CloseSound.Buffer);
								double a = TrainManager.Trains[k].Cars[i].Doors[0].CloseSound.Buffer.Duration;
								double b = TrainManager.Trains[k].Cars[i].Doors[1].CloseSound.Buffer.Duration;
								TrainManager.Trains[k].Cars[i].Specs.DoorCloseFrequency = a + b > 0.0 ? 2.0 / (a + b) : 0.8;
							} else if (TrainManager.Trains[k].Cars[i].Doors[0].CloseSound.Buffer != null) {
								Program.Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Doors[0].CloseSound.Buffer);
								double a = TrainManager.Trains[k].Cars[i].Doors[0].CloseSound.Buffer.Duration;
								TrainManager.Trains[k].Cars[i].Specs.DoorCloseFrequency = a > 0.0 ? 1.0 / a : 0.8;
							} else if (TrainManager.Trains[k].Cars[i].Doors[1].CloseSound.Buffer != null) {
								Program.Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Doors[0].CloseSound.Buffer);
								double b = TrainManager.Trains[k].Cars[i].Doors[1].CloseSound.Buffer.Duration;
								TrainManager.Trains[k].Cars[i].Specs.DoorCloseFrequency = b > 0.0 ? 1.0 / b : 0.8;
							} else {
								TrainManager.Trains[k].Cars[i].Specs.DoorCloseFrequency = 0.8;
							}
						}
						const double f = 0.015;
						const double g = 2.75;
						TrainManager.Trains[k].Cars[i].Specs.DoorOpenPitch = Math.Exp(f * Math.Tan(g * (Program.RandomNumberGenerator.NextDouble() - 0.5)));
						TrainManager.Trains[k].Cars[i].Specs.DoorClosePitch = Math.Exp(f * Math.Tan(g * (Program.RandomNumberGenerator.NextDouble() - 0.5)));
						TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency /= TrainManager.Trains[k].Cars[i].Specs.DoorOpenPitch;
						TrainManager.Trains[k].Cars[i].Specs.DoorCloseFrequency /= TrainManager.Trains[k].Cars[i].Specs.DoorClosePitch;
						/* 
						 * Remove the following two lines, then the pitch at which doors play
						 * takes their randomized opening and closing times into account.
						 * */
						TrainManager.Trains[k].Cars[i].Specs.DoorOpenPitch = 1.0;
						TrainManager.Trains[k].Cars[i].Specs.DoorClosePitch = 1.0;
					}
				}
				// add panel section
				if (TrainManager.Trains[k].IsPlayerTrain) {	
					TrainProgressCurrentWeight = 0.7 / TrainProgressMaximum;
					TrainManager.ParsePanelConfig(TrainManager.Trains[k].TrainFolder, CurrentTrainEncoding, TrainManager.Trains[k]);
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					Program.FileSystem.AppendToLogFile("Train panel loaded sucessfully.");
				}
				// add exterior section
				if (TrainManager.Trains[k].State != TrainState.Bogus)
				{
					bool LoadObjects = false;
					if (CarObjects == null)
					{
						CarObjects = new UnifiedObject[TrainManager.Trains[k].Cars.Length];
						BogieObjects = new UnifiedObject[TrainManager.Trains[k].Cars.Length * 2];
						CouplerObjects = new UnifiedObject[TrainManager.Trains[k].Cars.Length];
						LoadObjects = true;
					}
					string tXml = OpenBveApi.Path.CombineFile(TrainManager.Trains[k].TrainFolder, "train.xml");
					if (System.IO.File.Exists(tXml))
					{
						TrainXmlParser.Parse(tXml, TrainManager.Trains[k], ref CarObjects, ref BogieObjects, ref CouplerObjects);
					}
					else
					{
						ExtensionsCfgParser.ParseExtensionsConfig(TrainManager.Trains[k].TrainFolder, CurrentTrainEncoding, ref CarObjects, ref BogieObjects, ref CouplerObjects, TrainManager.Trains[k], LoadObjects);
					}
					World.CameraCar = TrainManager.Trains[k].DriverCar;
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					//Stores the current array index of the bogie object to add
					//Required as there are two bogies per car, and we're using a simple linear array....
					int currentBogieObject = 0;
					for (int i = 0; i < TrainManager.Trains[k].Cars.Length; i++)
					{
						if (CarObjects[i] == null) {
							// load default exterior object
							string file = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Compatibility"), "exterior.csv");
							StaticObject so = ObjectManager.LoadStaticObject(file, System.Text.Encoding.UTF8, false);
							if (so == null) {
								CarObjects[i] = null;
							} else {
								double sx = TrainManager.Trains[k].Cars[i].Width;
								double sy = TrainManager.Trains[k].Cars[i].Height;
								double sz = TrainManager.Trains[k].Cars[i].Length;
								so.ApplyScale(sx, sy, sz);
								CarObjects[i] = so;
							}
						}
						if (CarObjects[i] != null) {
							// add object
							TrainManager.Trains[k].Cars[i].LoadCarSections(CarObjects[i]);
						}

						if (CouplerObjects[i] != null)
						{
							TrainManager.Trains[k].Cars[i].Coupler.LoadCarSections(CouplerObjects[i]);
						}
						//Load bogie objects
						if (BogieObjects[currentBogieObject] != null)
						{
							TrainManager.Trains[k].Cars[i].FrontBogie.LoadCarSections(BogieObjects[currentBogieObject]);
						}
						currentBogieObject++;
						if (BogieObjects[currentBogieObject] != null)
						{
							TrainManager.Trains[k].Cars[i].RearBogie.LoadCarSections(BogieObjects[currentBogieObject]);
						}
						currentBogieObject++;
					}
				}
				// place cars
				TrainManager.Trains[k].PlaceCars(0.0);
				
				// configure ai / timetable
				if (TrainManager.Trains[k].IsPlayerTrain) {
					TrainManager.Trains[k].TimetableDelta = 0.0;
				} else if (TrainManager.Trains[k].State != TrainState.Bogus) {
					TrainManager.Trains[k].AI = new Game.SimpleHumanDriverAI(TrainManager.Trains[k]);
					TrainManager.Trains[k].TimetableDelta = Game.PrecedingTrainTimeDeltas[k];
					TrainManager.Trains[k].Specs.DoorOpenMode = TrainManager.DoorMode.Manual;
					TrainManager.Trains[k].Specs.DoorCloseMode = TrainManager.DoorMode.Manual;
				}
			}
			/*
			 * HACK: Store the TrainManager.Trains reference in the RouteManager also
			 *		 Note that this may change when the TrainManager is separated from the lump
			 *       Remember not to modify via this ref
			 */
			// ReSharper disable once CoVariantArrayConversion
			Program.CurrentRoute.Trains = TrainManager.Trains;
			TrainProgress = 1.0;
			// finished created objects
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			Array.Resize(ref ObjectManager.AnimatedWorldObjects, ObjectManager.AnimatedWorldObjectsUsed);
			// update sections
			if (Program.CurrentRoute.Sections.Length > 0) {
				Game.UpdateAllSections();
			}
			// load plugin
			for (int i = 0; i < TrainManager.Trains.Length; i++) {
				if (TrainManager.Trains[i].State != TrainState.Bogus) {
					if (TrainManager.Trains[i].IsPlayerTrain) {
						if (!TrainManager.Trains[i].LoadCustomPlugin(TrainManager.Trains[i].TrainFolder, CurrentTrainEncoding)) {
							TrainManager.Trains[i].LoadDefaultPlugin( TrainManager.Trains[i].TrainFolder);
						}
					} else {
						TrainManager.Trains[i].LoadDefaultPlugin(TrainManager.Trains[i].TrainFolder);
					}
				}
			}
		}

	}
}
