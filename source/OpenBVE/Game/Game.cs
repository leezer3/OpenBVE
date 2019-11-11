using System;
using OpenBveApi.Colors;
using OpenBveApi.Textures;
using OpenBveApi.Trains;
using OpenBveApi.Routes;
using RouteManager2;
using RouteManager2.Climate;
using RouteManager2.SignalManager;
using RouteManager2.SignalManager.PreTrain;
using RouteManager2.Stations;

namespace OpenBve {
	internal static partial class Game {

		// date and time
        
        /// <summary>The time at which the current game started, expressed as the number of seconds since midnight on the first day</summary>
		internal static double StartupTime = 0.0;
		/// <summary>Whether the game is in minimal simulation mode: 
		/// This is used when the game is fast-forwarding on start or station jump.
		/// Train and time movements are processed, but no graphical processing is done
		/// </summary>
		internal static bool MinimalisticSimulation = false;

		
		
		
		

		// other trains
		internal static double[] PrecedingTrainTimeDeltas = new double[] { };
		internal static double PrecedingTrainSpeedLimit = double.PositiveInfinity;
		

		
        /// <summary>The default mode for the train's safety system to start in</summary>
		internal static TrainStartMode TrainStart = TrainStartMode.EmergencyBrakesAts;
        /// <summary>The name of the current train</summary>
		internal static string TrainName = "";
		/// <summary>The initial destination for any train within the game</summary>
		internal static int InitialDestination = -1;

		internal static int InitialViewpoint = 0;

		// ================================

		/// <summary>Call this function to reset the game</summary>
		/// <param name="ResetLogs">Whether the logs should be reset</param>
		/// <param name="ResetRenderer">Whether the renderer should be reset</param>
		internal static void Reset(bool ResetLogs, bool ResetRenderer) {
			// track manager
			for (int i = 0; i < Program.CurrentRoute.Tracks.Length; i++)
			{
				Program.CurrentRoute.Tracks[i] = new Track();
			}
			// train manager
			TrainManager.Trains = new TrainManager.Train[] { };
			// game
			Interface.ClearMessages();
			CurrentInterface = InterfaceType.Normal;
			Program.CurrentRoute.Comment = "";
			Program.CurrentRoute.Image = "";
			Program.CurrentRoute.Atmosphere.AccelerationDueToGravity = 9.80665;
			Program.CurrentRoute.Atmosphere.InitialAirPressure = 101325.0;
			Program.CurrentRoute.Atmosphere.InitialAirTemperature = 293.15;
			Program.CurrentRoute.Atmosphere.InitialElevation = 0.0;
			Program.CurrentRoute.Atmosphere.SeaLevelAirPressure = 101325.0;
			Program.CurrentRoute.Atmosphere.SeaLevelAirTemperature = 293.15;
			Program.CurrentRoute.BufferTrackPositions = new double[] { };
			//Messages = new Message[] { };
			Program.Renderer.Marker.MarkerTextures = new Texture[] { };
			Program.CurrentRoute.PointsOfInterest = new PointOfInterest[] { };
			PrecedingTrainTimeDeltas = new double[] { };
			PrecedingTrainSpeedLimit = double.PositiveInfinity;
			Program.CurrentRoute.BogusPreTrainInstructions = new BogusPreTrainInstruction[] { };
			TrainName = "";
			TrainStart = TrainStartMode.EmergencyBrakesNoAts;
			Program.CurrentRoute.NoFogStart = (float)Math.Max(1.33333333333333 * Interface.CurrentOptions.ViewingDistance, 800.0);
			Program.CurrentRoute.NoFogEnd = (float)Math.Max(2.66666666666667 * Interface.CurrentOptions.ViewingDistance, 1600.0);
			Program.CurrentRoute.PreviousFog = new Fog(Program.CurrentRoute.NoFogStart, Program.CurrentRoute.NoFogEnd, Color24.Grey, 0.0);
			Program.CurrentRoute.CurrentFog = new Fog(Program.CurrentRoute.NoFogStart, Program.CurrentRoute.NoFogEnd, Color24.Grey, 0.5);
			Program.CurrentRoute.NextFog = new Fog(Program.CurrentRoute.NoFogStart, Program.CurrentRoute.NoFogEnd, Color24.Grey, 1.0);
			if (ResetLogs) {
				LogRouteName = "";
				LogTrainName = "";
				LogDateTime = DateTime.Now;
				CurrentScore = new Score();
				ScoreMessages = new ScoreMessage[] { };
				ScoreLogs = new ScoreLog[64];
				ScoreLogCount = 0;
				BlackBoxEntries = new BlackBoxEntry[256];
				BlackBoxEntryCount = 0;
				BlackBoxNextUpdate = 0.0;
			}
			// renderer
			if (ResetRenderer)
			{
				Program.Renderer.InfoTotalTriangles = 0;
				Program.Renderer.InfoTotalTriangleStrip = 0;
				Program.Renderer.InfoTotalQuads = 0;
				Program.Renderer.InfoTotalQuadStrip = 0;
				Program.Renderer.InfoTotalPolygon = 0;
				Program.Renderer.Reset();
			}
			
		}

		// ================================

		// black box
		internal enum BlackBoxEventToken : short {
			None = 0
		}
		internal enum BlackBoxPower : short {
			PowerNull = 0
		}
		internal enum BlackBoxBrake : short {
			BrakeNull = 0,
			Emergency = -1,
			HoldBrake = -2,
			Release = -3,
			Lap = -4,
			Service = -5
		}
		internal struct BlackBoxEntry {
			internal double Time;
			internal double Position;
			internal float Speed;
			internal float Acceleration;
			internal short ReverserDriver;
			internal short ReverserSafety;
			internal BlackBoxPower PowerDriver;
			internal BlackBoxPower PowerSafety;
			internal BlackBoxBrake BrakeDriver;
			internal BlackBoxBrake BrakeSafety;
			internal BlackBoxEventToken EventToken;
		}
		internal static BlackBoxEntry[] BlackBoxEntries = new BlackBoxEntry[256];
		internal static int BlackBoxEntryCount = 0;
		private static double BlackBoxNextUpdate = 0.0;

		internal static void UpdateBlackBox() {
			if (Program.CurrentRoute.SecondsSinceMidnight >= BlackBoxNextUpdate) {
				AddBlackBoxEntry(BlackBoxEventToken.None);
				BlackBoxNextUpdate = Program.CurrentRoute.SecondsSinceMidnight + 1.0;
			}
		}
		internal static void AddBlackBoxEntry(BlackBoxEventToken EventToken) {
			if (Interface.CurrentOptions.BlackBox) {
				if (BlackBoxEntryCount >= BlackBoxEntries.Length) {
					Array.Resize<BlackBoxEntry>(ref BlackBoxEntries, BlackBoxEntries.Length << 1);
				}
				BlackBoxEntries[BlackBoxEntryCount].Time = Program.CurrentRoute.SecondsSinceMidnight;
				BlackBoxEntries[BlackBoxEntryCount].Position = TrainManager.PlayerTrain.Cars[0].TrackPosition;
				BlackBoxEntries[BlackBoxEntryCount].Speed = (float)TrainManager.PlayerTrain.CurrentSpeed;
				BlackBoxEntries[BlackBoxEntryCount].Acceleration = (float)TrainManager.PlayerTrain.Specs.CurrentAverageAcceleration;
				BlackBoxEntries[BlackBoxEntryCount].ReverserDriver = (short)TrainManager.PlayerTrain.Handles.Reverser.Driver;
				BlackBoxEntries[BlackBoxEntryCount].ReverserSafety = (short)TrainManager.PlayerTrain.Handles.Reverser.Actual;
				BlackBoxEntries[BlackBoxEntryCount].PowerDriver = (BlackBoxPower)TrainManager.PlayerTrain.Handles.Power.Driver;
				BlackBoxEntries[BlackBoxEntryCount].PowerSafety = (BlackBoxPower)TrainManager.PlayerTrain.Handles.Power.Safety;
				if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver) {
					BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.Emergency;
				} else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver) {
					BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.HoldBrake;
				} else if (TrainManager.PlayerTrain.Handles.Brake is TrainManager.AirBrakeHandle) {
					switch ((TrainManager.AirBrakeHandleState)TrainManager.PlayerTrain.Handles.Brake.Driver) {
							case TrainManager.AirBrakeHandleState.Release: BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.Release; break;
							case TrainManager.AirBrakeHandleState.Lap: BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.Lap; break;
							case TrainManager.AirBrakeHandleState.Service: BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.Service; break;
							default: BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.Emergency; break;
					}
				} else {
					BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = (BlackBoxBrake)TrainManager.PlayerTrain.Handles.Brake.Driver;
				}
				if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Safety) {
					BlackBoxEntries[BlackBoxEntryCount].BrakeSafety = BlackBoxBrake.Emergency;
				} else if (TrainManager.PlayerTrain.Handles.HoldBrake.Actual) {
					BlackBoxEntries[BlackBoxEntryCount].BrakeSafety = BlackBoxBrake.HoldBrake;
				} else if (TrainManager.PlayerTrain.Handles.Brake is TrainManager.AirBrakeHandle) {
					switch ((TrainManager.AirBrakeHandleState)TrainManager.PlayerTrain.Handles.Brake.Safety) {
							case TrainManager.AirBrakeHandleState.Release: BlackBoxEntries[BlackBoxEntryCount].BrakeSafety = BlackBoxBrake.Release; break;
							case TrainManager.AirBrakeHandleState.Lap: BlackBoxEntries[BlackBoxEntryCount].BrakeSafety = BlackBoxBrake.Lap; break;
							case TrainManager.AirBrakeHandleState.Service: BlackBoxEntries[BlackBoxEntryCount].BrakeSafety = BlackBoxBrake.Service; break;
							default: BlackBoxEntries[BlackBoxEntryCount].BrakeSafety = BlackBoxBrake.Emergency; break;
					}
				} else {
					BlackBoxEntries[BlackBoxEntryCount].BrakeSafety = (BlackBoxBrake)TrainManager.PlayerTrain.Handles.Brake.Safety;
				}
				BlackBoxEntries[BlackBoxEntryCount].EventToken = EventToken;
				BlackBoxEntryCount++;
			}
		}


		// buffers
		
	}
}
