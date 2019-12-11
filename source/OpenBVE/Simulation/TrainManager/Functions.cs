using System;
using OpenBveApi.Trains;
using SoundManager;

namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>Un-derails all trains within the simulation</summary>
		internal static void UnderailTrains()
		{
			System.Threading.Tasks.Parallel.For(0, Trains.Length, i =>
			{
				UnderailTrain(Trains[i]);
			});
		}
		/// <summary>Un-derails a train</summary>
		/// <param name="Train">The train</param>
		internal static void UnderailTrain(Train Train)
		{
			Train.Derailed = false;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				Train.Cars[i].Specs.CurrentRollDueToTopplingAngle = 0.0;
				Train.Cars[i].Derailed = false;
			}
		}

		/// <summary>Jumps a train to a new station</summary>
		/// <param name="train">The train</param>
		/// <param name="stationIndex">The zero-based index of the station</param>
		internal static void JumpTrain(Train train, int stationIndex)
		{
			train.SafetySystems.PassAlarm.Halt();
			int currentTrackElement = train.Cars[0].FrontAxle.Follower.LastTrackElement;
			if (train.IsPlayerTrain)
			{
				for (int i = 0; i < ObjectManager.AnimatedWorldObjects.Length; i++)
				{
					var obj = ObjectManager.AnimatedWorldObjects[i] as OpenBveApi.Objects.TrackFollowingObject;
					if (obj != null)
					{
						//Track followers should be reset if we jump between stations
						obj.FrontAxleFollower.TrackPosition = ObjectManager.AnimatedWorldObjects[i].TrackPosition + obj.FrontAxlePosition;
						obj.FrontAxleFollower.TrackPosition = ObjectManager.AnimatedWorldObjects[i].TrackPosition + obj.RearAxlePosition;
						obj.FrontAxleFollower.UpdateWorldCoordinates(false);
						obj.RearAxleFollower.UpdateWorldCoordinates(false);
					}
				 
				}
			}
			train.StationState = TrainStopState.Jumping;
			int stopIndex = Program.CurrentRoute.Stations[stationIndex].GetStopIndex(train.NumberOfCars);
			if (stopIndex >= 0)
			{
				if (train.IsPlayerTrain)
				{
					if (train.Plugin != null)
					{
						train.Plugin.BeginJump((OpenBveApi.Runtime.InitializationModes)Game.TrainStart);
					}
				}
				for (int h = 0; h < train.Cars.Length; h++)
				{
					train.Cars[h].CurrentSpeed = 0.0;
				}
				double d = Program.CurrentRoute.Stations[stationIndex].Stops[stopIndex].TrackPosition - train.Cars[0].FrontAxle.Follower.TrackPosition + train.Cars[0].FrontAxle.Position - 0.5 * train.Cars[0].Length;
				if (train.IsPlayerTrain)
				{
					SoundsBase.SuppressSoundEvents = true;
				}
				while (d != 0.0)
				{
					double x;
					if (Math.Abs(d) > 1.0)
					{
						x = (double)Math.Sign(d);
					}
					else
					{
						x = d;
					}
					for (int h = 0; h < train.Cars.Length; h++)
					{
						train.Cars[h].Move(x);
					}
					if (Math.Abs(d) >= 1.0)
					{
						d -= x;
					}
					else
					{
						break;
					}
				}
				if (train.IsPlayerTrain)
				{
					TrainManager.UnderailTrains();
					SoundsBase.SuppressSoundEvents = false;
				}
				if (train.Handles.EmergencyBrake.Driver)
				{
					train.ApplyNotch(0, false, 0, true);
				}
				else
				{
					train.ApplyNotch(0, false, train.Handles.Brake.MaximumNotch, false);
					train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Service);
				}
				if (Program.CurrentRoute.Sections.Length > 0)
				{
					Program.CurrentRoute.UpdateAllSections();
				}
				if (train.IsPlayerTrain)
				{
					if (Game.CurrentScore.ArrivalStation <= stationIndex)
					{
						Game.CurrentScore.ArrivalStation = stationIndex + 1;
					}
				}
				if (train.IsPlayerTrain)
				{
					if (Program.CurrentRoute.Stations[stationIndex].ArrivalTime >= 0.0)
					{
						Program.CurrentRoute.SecondsSinceMidnight = Program.CurrentRoute.Stations[stationIndex].ArrivalTime;
					}
					else if (Program.CurrentRoute.Stations[stationIndex].DepartureTime >= 0.0)
					{
						Program.CurrentRoute.SecondsSinceMidnight = Program.CurrentRoute.Stations[stationIndex].DepartureTime - Program.CurrentRoute.Stations[stationIndex].StopTime;
					}
				}
				for (int i = 0; i < train.Cars.Length; i++)
				{
					train.Cars[i].Doors[0].AnticipatedOpen = Program.CurrentRoute.Stations[stationIndex].OpenLeftDoors;
					train.Cars[i].Doors[1].AnticipatedOpen = Program.CurrentRoute.Stations[stationIndex].OpenRightDoors;
				}
				if (train.IsPlayerTrain)
				{
					Game.CurrentScore.DepartureStation = stationIndex;
					Game.CurrentInterface = Game.InterfaceType.Normal;
					//Game.Messages = new Game.Message[] { };
				}
				ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
				TrainManager.UpdateTrainObjects(0.0, true);
				if (train.IsPlayerTrain)
				{
					if (train.Plugin != null)
					{
						train.Plugin.EndJump();
					}
				}
				train.StationState = TrainStopState.Pending;
				if (train.IsPlayerTrain)
				{
					train.LastStation = stationIndex;
				}
				int newTrackElement = train.Cars[0].FrontAxle.Follower.LastTrackElement;
				if (newTrackElement < currentTrackElement)
				{
					for (int i = newTrackElement; i < currentTrackElement; i++)
					{
						for (int j = 0; j < Program.CurrentHost.Tracks[0].Elements[i].Events.Length; j++)
						{
							Program.CurrentHost.Tracks[0].Elements[i].Events[j].Reset();
						}

					}
				}
			}
		}

		internal static void JumpTFO()
		{
			foreach (var Train in TFOs)
			{
				Train.Dispose();
				Train.State = TrainState.Pending;
				Game.TrackFollowingObjectAI AI = Train.AI as Game.TrackFollowingObjectAI;
				if (AI != null)
				{
					AI.SetupTravelData(Train.AppearanceTime);
				}
			}
		}
	}
}
