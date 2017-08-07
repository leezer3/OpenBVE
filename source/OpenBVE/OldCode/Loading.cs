﻿using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using OpenBveApi.Math;

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
			try {
				LoadEverythingThreaded();
			} catch (Exception ex) {
				for (int i = 0; i < TrainManager.Trains.Length; i++) {
					if (TrainManager.Trains[i] != null && TrainManager.Trains[i].Plugin != null) {
						if (TrainManager.Trains[i].Plugin.LastException != null) {
							Interface.AddMessage(Interface.MessageType.Critical, false, "The train plugin " + TrainManager.Trains[i].Plugin.PluginTitle + " caused a critical error in the route and train loader: " + TrainManager.Trains[i].Plugin.LastException.Message);
							CrashHandler.LoadingCrash(TrainManager.Trains[i].Plugin.LastException + Environment.StackTrace, true);
							 Program.RestartArguments = " ";
							 Cancel = true;    
							return;
						}
					}
				}
				Interface.AddMessage(Interface.MessageType.Critical, false, "The route and train loader encountered the following critical error: " + ex.Message);
				CrashHandler.LoadingCrash(ex + Environment.StackTrace, false);
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
			System.Threading.Thread.Sleep(1); if (Cancel) return;
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
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			TrainManager.Trains = new TrainManager.Train[Game.PrecedingTrainTimeDeltas.Length + 1 + (Game.BogusPretrainInstructions.Length != 0 ? 1 : 0)];
			for (int k = 0; k < TrainManager.Trains.Length; k++) {
				TrainManager.Trains[k] = new TrainManager.Train {TrainIndex = k};
				if (k == TrainManager.Trains.Length - 1 & Game.BogusPretrainInstructions.Length != 0) {
					TrainManager.Trains[k].State = TrainManager.TrainState.Bogus;
				} else {
					TrainManager.Trains[k].State = TrainManager.TrainState.Pending;
				}
			}
			TrainManager.PlayerTrain = TrainManager.Trains[Game.PrecedingTrainTimeDeltas.Length];
			// load trains
			double TrainProgressMaximum = 0.7 + 0.3 * (double)TrainManager.Trains.Length;
			for (int k = 0; k < TrainManager.Trains.Length; k++) {
				//Sleep for 10ms to allow route loading locks to release
				Thread.Sleep(20);
				if (TrainManager.Trains[k].State == TrainManager.TrainState.Bogus) {
					// bogus train
					string TrainData = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Compatibility", "PreTrain"), "train.dat");
					TrainDatParser.ParseTrainData(TrainData, System.Text.Encoding.UTF8, TrainManager.Trains[k]);
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					TrainManager.Trains[k].InitializeCarSounds();
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					TrainProgressCurrentWeight = 0.3 / TrainProgressMaximum;
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
				} else {
					// real train
					Program.AppendToLogFile("Loading player train: " + CurrentTrainFolder);
					TrainProgressCurrentWeight = 0.1 / TrainProgressMaximum;
					string TrainData = OpenBveApi.Path.CombineFile(CurrentTrainFolder, "train.dat");
					TrainDatParser.ParseTrainData(TrainData, CurrentTrainEncoding, TrainManager.Trains[k]);
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					TrainProgressCurrentWeight = 0.2 / TrainProgressMaximum;
					SoundCfgParser.ParseSoundConfig(CurrentTrainFolder, CurrentTrainEncoding, TrainManager.Trains[k]);
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					// door open/close speed
					for (int i = 0; i < TrainManager.Trains[k].Cars.Length; i++) {
						if (TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency <= 0.0) {
							if (TrainManager.Trains[k].Cars[i].Sounds.DoorOpenL.Buffer != null & TrainManager.Trains[k].Cars[i].Sounds.DoorOpenR.Buffer != null) {
								Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Sounds.DoorOpenL.Buffer);
								Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Sounds.DoorOpenR.Buffer);
								double a = TrainManager.Trains[k].Cars[i].Sounds.DoorOpenL.Buffer.Duration;
								double b = TrainManager.Trains[k].Cars[i].Sounds.DoorOpenR.Buffer.Duration;
								TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency = a + b > 0.0 ? 2.0 / (a + b) : 0.8;
							} else if (TrainManager.Trains[k].Cars[i].Sounds.DoorOpenL.Buffer != null) {
								Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Sounds.DoorOpenL.Buffer);
								double a = TrainManager.Trains[k].Cars[i].Sounds.DoorOpenL.Buffer.Duration;
								TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency = a > 0.0 ? 1.0 / a : 0.8;
							} else if (TrainManager.Trains[k].Cars[i].Sounds.DoorOpenR.Buffer != null) {
								Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Sounds.DoorOpenL.Buffer);
								double b = TrainManager.Trains[k].Cars[i].Sounds.DoorOpenR.Buffer.Duration;
								TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency = b > 0.0 ? 1.0 / b : 0.8;
							} else {
								TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency = 0.8;
							}
						}
						if (TrainManager.Trains[k].Cars[i].Specs.DoorCloseFrequency <= 0.0) {
							if (TrainManager.Trains[k].Cars[i].Sounds.DoorCloseL.Buffer != null & TrainManager.Trains[k].Cars[i].Sounds.DoorCloseR.Buffer != null) {
								Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Sounds.DoorCloseL.Buffer);
								Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Sounds.DoorCloseR.Buffer);
								double a = TrainManager.Trains[k].Cars[i].Sounds.DoorCloseL.Buffer.Duration;
								double b = TrainManager.Trains[k].Cars[i].Sounds.DoorCloseR.Buffer.Duration;
								TrainManager.Trains[k].Cars[i].Specs.DoorCloseFrequency = a + b > 0.0 ? 2.0 / (a + b) : 0.8;
							} else if (TrainManager.Trains[k].Cars[i].Sounds.DoorCloseL.Buffer != null) {
								Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Sounds.DoorCloseL.Buffer);
								double a = TrainManager.Trains[k].Cars[i].Sounds.DoorCloseL.Buffer.Duration;
								TrainManager.Trains[k].Cars[i].Specs.DoorCloseFrequency = a > 0.0 ? 1.0 / a : 0.8;
							} else if (TrainManager.Trains[k].Cars[i].Sounds.DoorCloseR.Buffer != null) {
								Sounds.LoadBuffer(TrainManager.Trains[k].Cars[i].Sounds.DoorCloseL.Buffer);
								double b = TrainManager.Trains[k].Cars[i].Sounds.DoorCloseR.Buffer.Duration;
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
				if (k == TrainManager.PlayerTrain.TrainIndex) {
					TrainManager.Trains[k].Cars[TrainManager.Trains[k].DriverCar].CarSections = new TrainManager.CarSection[1];
				    TrainManager.Trains[k].Cars[TrainManager.Trains[k].DriverCar].CarSections[0] = new TrainManager.CarSection
				        {
				            Elements = new ObjectManager.AnimatedObject[] { },
				            Overlay = true
				        };
				    TrainProgressCurrentWeight = 0.7 / TrainProgressMaximum;
				    TrainManager.Trains[k].ParsePanelConfig(CurrentTrainFolder, CurrentTrainEncoding);
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					Program.AppendToLogFile("Train panel loaded sucessfully.");
				}
				// Load exterior
				if (TrainManager.Trains[k].State != TrainManager.TrainState.Bogus) {
					TrainManager.Trains[k].LoadExterior(CurrentTrainFolder, CurrentTrainEncoding);
					System.Threading.Thread.Sleep(1); if (Cancel) return;
				}
				// Place cars
				TrainManager.Trains[k].PlaceCars(0.0);

				// configure ai / timetable
				if (TrainManager.Trains[k] == TrainManager.PlayerTrain) {
					TrainManager.Trains[k].TimetableDelta = 0.0;
				} else if (TrainManager.Trains[k].State != TrainManager.TrainState.Bogus) {
					TrainManager.Trains[k].AI = new Game.SimpleHumanDriverAI(TrainManager.Trains[k]);
					TrainManager.Trains[k].TimetableDelta = Game.PrecedingTrainTimeDeltas[k];
					TrainManager.Trains[k].Specs.DoorOpenMode = TrainManager.DoorMode.Manual;
					TrainManager.Trains[k].Specs.DoorCloseMode = TrainManager.DoorMode.Manual;
				}
			}
			TrainProgress = 1.0;
			// finished created objects
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			ObjectManager.FinishCreatingObjects();
			// update sections
			if (Game.Sections.Length > 0) {
				Game.UpdateSection(Game.Sections.Length - 1);
			}
			// load plugin
			for (int i = 0; i < TrainManager.Trains.Length; i++) {
				if (TrainManager.Trains[i].State != TrainManager.TrainState.Bogus) {
					if (TrainManager.Trains[i] == TrainManager.PlayerTrain) {
						if (!PluginManager.LoadCustomPlugin(TrainManager.Trains[i], CurrentTrainFolder, CurrentTrainEncoding)) {
							PluginManager.LoadDefaultPlugin(TrainManager.Trains[i], CurrentTrainFolder);
						}
					} else {
						PluginManager.LoadDefaultPlugin(TrainManager.Trains[i], CurrentTrainFolder);
					}
				}
			}
		}

	}
}