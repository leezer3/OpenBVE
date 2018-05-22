using OpenBveApi.Colors;
using OpenBveApi.Runtime;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Defines a request stop which may be made by a train</summary>
		internal class RequestStop
		{
			internal int Probability;
			internal double Time = -1;
			internal string StopMessage;
			internal string PassMessage;
		}

		/// <summary>Called when a train passes over the trigger for a request stop</summary>
		internal class RequestStopEvent : GeneralEvent
		{
			/// <summary>The index of the station which this applies to</summary>
			private readonly int StationIndex;
			/// <summary>The request stop to be triggered if we are early</summary>
			private readonly RequestStop Early;
			/// <summary>The request stop to be triggered if we are on-time</summary>
			private readonly RequestStop OnTime;
			/// <summary>The request stop to be triggered if we are late</summary>
			private readonly RequestStop Late;
			/// <summary>Whether we pass this stop at linespeed if it is not requested</summary>
			private readonly bool FullSpeed;
			/// <summary>This stop is only triggered for trains under MaxCars</summary>
			private readonly int MaxCars;

			internal RequestStopEvent(int stationIndex, int maxCars, bool fullSpeed, RequestStop onTime, RequestStop early, RequestStop late)
			{
				this.FullSpeed = fullSpeed;
				this.OnTime = onTime;
				this.StationIndex = stationIndex;
				this.Early = early;
				this.Late = late;
				this.MaxCars = maxCars;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (TriggerType == EventTriggerType.FrontCarFrontAxle)
				{
					RequestStop stop; //Temp probability value
					if (Early.Time != -1 && Game.SecondsSinceMidnight < Early.Time)
					{
						stop = Early;
					}
					else if (Early.Time != -1 && Game.SecondsSinceMidnight > Early.Time && Late.Time != -1 && Game.SecondsSinceMidnight < Late.Time)
					{
						stop = OnTime;
					}
					else if (Late.Time != -1 && Game.SecondsSinceMidnight > Late.Time)
					{
						stop = Late;
					}
					else
					{
						stop = OnTime;
					}
					if (MaxCars != 0 && Train.Cars.Length > MaxCars)
					{
						//Check whether our train length is valid for this before doing anything else
						Sounds.PlayCarSound(Train.Cars[Train.DriverCar].Sounds.RequestStop[2], 1.0, 1.0, Train, Train.DriverCar, false);
						return;
					}
					if (Direction > 0)
					{

						if (Program.RandomNumberGenerator.Next(0, 100) <= stop.Probability)
						{
							//We have hit our probability roll
							if (Game.Stations[StationIndex].StopMode == StationStopMode.AllRequestStop || (Train == TrainManager.PlayerTrain && Game.Stations[StationIndex].StopMode == StationStopMode.PlayerRequestStop))
							{

								//If our train can stop at this station, set it's index accordingly
								Train.Station = StationIndex;
								Train.NextStopSkipped = TrainManager.StopSkipMode.None;
								//Play sound
								Sounds.PlayCarSound(Train.Cars[Train.DriverCar].Sounds.RequestStop[0], 1.0, 1.0, Train, Train.DriverCar, false);
							}
							else
							{
								//We don't meet the conditions for this request stop
								if (FullSpeed)
								{
									//Pass at linespeed, rather than braking as if for stop
									Train.NextStopSkipped = TrainManager.StopSkipMode.Linespeed;
								}
								else
								{
									Train.NextStopSkipped = TrainManager.StopSkipMode.Decelerate;
								}
								//Play sound
								Sounds.PlayCarSound(Train.Cars[Train.DriverCar].Sounds.RequestStop[1], 1.0, 1.0, Train, Train.DriverCar, false);
								//If message is not empty, add it
								if (!string.IsNullOrEmpty(stop.PassMessage) && Train == TrainManager.PlayerTrain)
								{
									Game.AddMessage(stop.PassMessage, MessageManager.MessageDependency.None, Interface.GameMode.Normal, MessageColor.White, Game.SecondsSinceMidnight + 10.0, null);
								}
								return;
							}
							//Play sound
							Sounds.PlayCarSound(Train.Cars[Train.DriverCar].Sounds.RequestStop[0], 1.0, 1.0, Train, Train.DriverCar, false);
							//If message is not empty, add it
							if (!string.IsNullOrEmpty(stop.StopMessage) && Train == TrainManager.PlayerTrain)
							{
								Game.AddMessage(stop.StopMessage, MessageManager.MessageDependency.None, Interface.GameMode.Normal, MessageColor.White, Game.SecondsSinceMidnight + 10.0, null);
							}
						}
						else
						{
							Sounds.PlayCarSound(Train.Cars[Train.DriverCar].Sounds.RequestStop[1], 1.0, 1.0, Train, Train.DriverCar, false);
							if (FullSpeed)
							{
								//Pass at linespeed, rather than braking as if for stop
								Train.NextStopSkipped = TrainManager.StopSkipMode.Linespeed;
							}
							else
							{
								Train.NextStopSkipped = TrainManager.StopSkipMode.Decelerate;
							}
							//Play sound
							Sounds.PlayCarSound(Train.Cars[Train.DriverCar].Sounds.RequestStop[1], 1.0, 1.0, Train, Train.DriverCar, false);
							//If message is not empty, add it
							if (!string.IsNullOrEmpty(stop.PassMessage) && Train == TrainManager.PlayerTrain)
							{
								Game.AddMessage(stop.PassMessage, MessageManager.MessageDependency.None, Interface.GameMode.Normal, MessageColor.White, Game.SecondsSinceMidnight + 10.0, null);
							}
						}
					}
				}
			}
		}
	}
}
