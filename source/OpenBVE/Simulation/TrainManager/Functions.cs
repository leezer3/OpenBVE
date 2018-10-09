using System;

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
			if (train == PlayerTrain)
			{
				for (int i = 0; i < ObjectManager.AnimatedWorldObjects.Length; i++)
				{
					var obj = ObjectManager.AnimatedWorldObjects[i] as ObjectManager.TrackFollowingObject;
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
			int stopIndex = Game.GetStopIndex(stationIndex, train.Cars.Length);
			if (stopIndex >= 0)
			{
				if (train == PlayerTrain)
				{
					if (train.Plugin != null)
					{
						train.Plugin.BeginJump((OpenBveApi.Runtime.InitializationModes)Game.TrainStart);
					}
				}
				for (int h = 0; h < train.Cars.Length; h++)
				{
					train.Cars[h].Specs.CurrentSpeed = 0.0;
				}
				double d = Game.Stations[stationIndex].Stops[stopIndex].TrackPosition - train.Cars[0].FrontAxle.Follower.TrackPosition + train.Cars[0].FrontAxle.Position - 0.5 * train.Cars[0].Length;
				if (train == PlayerTrain)
				{
					TrackManager.SuppressSoundEvents = true;
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
						train.Cars[h].Move(x, 0.0);
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
				if (train == PlayerTrain)
				{
					TrainManager.UnderailTrains();
					TrackManager.SuppressSoundEvents = false;
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
				if (Game.Sections.Length > 0)
				{
					Game.UpdateSection(Game.Sections.Length - 1);
				}
				if (train == PlayerTrain)
				{
					if (Game.CurrentScore.ArrivalStation <= stationIndex)
					{
						Game.CurrentScore.ArrivalStation = stationIndex + 1;
					}
				}
				if (train == PlayerTrain)
				{
					if (Game.Stations[stationIndex].ArrivalTime >= 0.0)
					{
						Game.SecondsSinceMidnight = Game.Stations[stationIndex].ArrivalTime;
					}
					else if (Game.Stations[stationIndex].DepartureTime >= 0.0)
					{
						Game.SecondsSinceMidnight = Game.Stations[stationIndex].DepartureTime - Game.Stations[stationIndex].StopTime;
					}
				}
				for (int i = 0; i < train.Cars.Length; i++)
				{
					train.Cars[i].Doors[0].AnticipatedOpen = Game.Stations[stationIndex].OpenLeftDoors;
					train.Cars[i].Doors[1].AnticipatedOpen = Game.Stations[stationIndex].OpenRightDoors;
				}
				if (train == PlayerTrain)
				{
					Game.CurrentScore.DepartureStation = stationIndex;
					Game.CurrentInterface = Game.InterfaceType.Normal;
					//Game.Messages = new Game.Message[] { };
				}
				ObjectManager.UpdateAnimatedWorldObjects(0.0, true);
				TrainManager.UpdateTrainObjects(0.0, true);
				if (train == PlayerTrain)
				{
					if (train.Plugin != null)
					{
						train.Plugin.EndJump();
					}
				}
				train.StationState = TrainStopState.Pending;
				if (train == PlayerTrain)
				{
					train.LastStation = stationIndex;
				}
			}
		}
	}
}
