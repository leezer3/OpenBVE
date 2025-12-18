using System;
using System.Collections.Generic;
using System.Linq;
using LibRender2.Screens;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Trains;
using OpenBveApi.Routes;
using RouteManager2;
using RouteManager2.Climate;
using RouteManager2.SignalManager.PreTrain;
using RouteManager2.Tracks;
using TrainManager.Handles;
using TrainManager.Trains;

namespace OpenBve 
{
	internal static partial class Game 
	{
        /// <summary>The time at which the current game started, expressed as the number of seconds since midnight on the first day</summary>
		internal static double StartupTime = 0.0;
		/// <summary>Whether the game is in minimal simulation mode: 
		/// This is used when the game is fast-forwarding on start or station jump.
		/// Train and time movements are processed, but no graphical processing is done
		/// </summary>
		internal static bool MinimalisticSimulation = false;

		/// <summary>Holds all current score messages to be rendered</summary>
		internal static List<ScoreMessage> ScoreMessages = new List<ScoreMessage>();
		/// <summary>Holds the current on-screen size in px of the area occupied by score messages</summary>
		internal static Vector2 ScoreMessagesRendererSize = new Vector2(16.0, 16.0);

		/// <summary>Call this function to reset the game</summary>
		/// <param name="resetLogs">Whether the logs should be reset</param>
		internal static void Reset(bool resetLogs) {
			// track manager
			for (int i = 0; i < Program.CurrentRoute.Tracks.Count; i++)
			{
				int key = Program.CurrentRoute.Tracks.ElementAt(i).Key;
				Program.CurrentRoute.Tracks[key] = new Track();
			}
			// train manager
			Program.TrainManager.Trains = new List<TrainBase>();
			// game
			Interface.LogMessages.Clear();
			Program.CurrentHost.ClearErrors();
			if (Program.Renderer.CurrentInterface < InterfaceType.Menu)
			{
				Program.Renderer.CurrentInterface = InterfaceType.Normal;
			}
			
			Program.CurrentRoute.Comment = string.Empty;
			Program.CurrentRoute.Image = string.Empty;
			Program.CurrentRoute.Atmosphere = new Atmosphere();
			Program.CurrentRoute.LightDefinitions = new LightDefinition[] { };
			Program.CurrentRoute.BufferTrackPositions = new List<BufferStop>();
			Program.CurrentRoute.PointsOfInterest = new PointOfInterest[] { };
			Program.CurrentRoute.PrecedingTrainTimeDeltas = new double[] { };
			Interface.CurrentOptions.PrecedingTrainSpeedLimit = double.PositiveInfinity;
			Program.CurrentRoute.BogusPreTrainInstructions = new BogusPreTrainInstruction[] { };
			Interface.CurrentOptions.TrainName = string.Empty;
			Interface.CurrentOptions.TrainStart = TrainStartMode.EmergencyBrakesNoAts;
			Program.CurrentRoute.NoFogStart = (float)Math.Max(1.33333333333333 * Interface.CurrentOptions.ViewingDistance, 800.0);
			Program.CurrentRoute.NoFogEnd = (float)Math.Max(2.66666666666667 * Interface.CurrentOptions.ViewingDistance, 1600.0);
			Program.CurrentRoute.PreviousFog = new Fog(Program.CurrentRoute.NoFogStart, Program.CurrentRoute.NoFogEnd, Color24.Grey, 0.0);
			Program.CurrentRoute.CurrentFog = new Fog(Program.CurrentRoute.NoFogStart, Program.CurrentRoute.NoFogEnd, Color24.Grey, 0.5);
			Program.CurrentRoute.NextFog = new Fog(Program.CurrentRoute.NoFogStart, Program.CurrentRoute.NoFogEnd, Color24.Grey, 1.0);
			if (resetLogs) {
				LogRouteName = string.Empty;
				LogTrainName = string.Empty;
				LogDateTime = DateTime.Now;
				CurrentScore = new Score();
				ScoreMessages.Clear();
				ScoreLogs = new ScoreLog[64];
				ScoreLogCount = 0;
				BlackBoxEntries = new List<BlackBoxEntry>();
				BlackBoxNextUpdate = 0.0;
			}
		}

		// ================================

		// black box




		internal static List<BlackBoxEntry> BlackBoxEntries = new List<BlackBoxEntry>();
		private static double BlackBoxNextUpdate = 0.0;

		internal static void UpdateBlackBox() {
			if (Program.CurrentRoute.SecondsSinceMidnight >= BlackBoxNextUpdate) {
				AddBlackBoxEntry();
				BlackBoxNextUpdate = Program.CurrentRoute.SecondsSinceMidnight + 1.0;
			}
		}
		internal static void AddBlackBoxEntry() {
			if (Interface.CurrentOptions.BlackBox) {
				
				BlackBoxEntry newEntry = new BlackBoxEntry();
				newEntry.Time = Program.CurrentRoute.SecondsSinceMidnight;
				newEntry.Position = TrainManager.PlayerTrain.Cars[0].TrackPosition;
				newEntry.Speed = (float)TrainManager.PlayerTrain.CurrentSpeed;
				newEntry.Acceleration = (float)TrainManager.PlayerTrain.Specs.CurrentAverageAcceleration;
				newEntry.ReverserDriver = (short)TrainManager.PlayerTrain.Handles.Reverser.Driver;
				newEntry.ReverserSafety = (short)TrainManager.PlayerTrain.Handles.Reverser.Actual;
				newEntry.PowerDriver = (BlackBoxPower)TrainManager.PlayerTrain.Handles.Power.Driver;
				newEntry.PowerSafety = (BlackBoxPower)TrainManager.PlayerTrain.Handles.Power.Safety;
				if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver) {
					newEntry.BrakeDriver = BlackBoxBrake.Emergency;
				} else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver) {
					newEntry.BrakeDriver = BlackBoxBrake.HoldBrake;
				} else if (TrainManager.PlayerTrain.Handles.Brake is AirBrakeHandle) {
					switch ((AirBrakeHandleState)TrainManager.PlayerTrain.Handles.Brake.Driver) {
							case AirBrakeHandleState.Release: newEntry.BrakeDriver = BlackBoxBrake.Release; break;
							case AirBrakeHandleState.Lap: newEntry.BrakeDriver = BlackBoxBrake.Lap; break;
							case AirBrakeHandleState.Service: newEntry.BrakeDriver = BlackBoxBrake.Service; break;
							default: newEntry.BrakeDriver = BlackBoxBrake.Emergency; break;
					}
				} else {
					newEntry.BrakeDriver = (BlackBoxBrake)TrainManager.PlayerTrain.Handles.Brake.Driver;
				}
				if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Safety) {
					newEntry.BrakeSafety = BlackBoxBrake.Emergency;
				} else if (TrainManager.PlayerTrain.Handles.HoldBrake.Actual) {
					newEntry.BrakeSafety = BlackBoxBrake.HoldBrake;
				} else if (TrainManager.PlayerTrain.Handles.Brake is AirBrakeHandle) {
					switch ((AirBrakeHandleState)TrainManager.PlayerTrain.Handles.Brake.Safety) {
							case AirBrakeHandleState.Release: newEntry.BrakeSafety = BlackBoxBrake.Release; break;
							case AirBrakeHandleState.Lap: newEntry.BrakeSafety = BlackBoxBrake.Lap; break;
							case AirBrakeHandleState.Service: newEntry.BrakeSafety = BlackBoxBrake.Service; break;
							default: newEntry.BrakeSafety = BlackBoxBrake.Emergency; break;
					}
				} else {
					newEntry.BrakeSafety = (BlackBoxBrake)TrainManager.PlayerTrain.Handles.Brake.Safety;
				}
				BlackBoxEntries.Add(newEntry);
			}
		}
	}
}
