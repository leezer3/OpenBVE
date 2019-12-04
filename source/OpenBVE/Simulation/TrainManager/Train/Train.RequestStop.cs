using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using RouteManager2.MessageManager;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>The root class for a train within the simulation</summary>
		public partial class Train : AbstractTrain
		{
			/// <inheritdoc/>
			public override void RequestStop(RequestStop stopRequest)
			{
				if (stopRequest.MaxCars != 0 && NumberOfCars > stopRequest.MaxCars)
				{
					//Check whether our train length is valid for this before doing anything else
					Program.Sounds.PlayCarSound(Cars[DriverCar].Sounds.RequestStop[2], 1.0, 1.0, Cars[DriverCar], false);
					return;
				}
				if (Program.RandomNumberGenerator.Next(0, 100) <= stopRequest.Probability)
				{
					//We have hit our probability roll
					if (Program.CurrentRoute.Stations[stopRequest.StationIndex].StopMode == StationStopMode.AllRequestStop || (IsPlayerTrain && Program.CurrentRoute.Stations[stopRequest.StationIndex].StopMode == StationStopMode.PlayerRequestStop))
					{

						//If our train can stop at this station, set it's index accordingly
						Station = stopRequest.StationIndex;
						NextStopSkipped = StopSkipMode.None;
						//Play sound
						Program.Sounds.PlayCarSound(Cars[DriverCar].Sounds.RequestStop[0], 1.0, 1.0, Cars[DriverCar], false);
					}
					else
					{
						//We don't meet the conditions for this request stop
						if (stopRequest.FullSpeed)
						{
							//Pass at linespeed, rather than braking as if for stop
							NextStopSkipped = StopSkipMode.Linespeed;
						}
						else
						{
							NextStopSkipped = StopSkipMode.Decelerate;
						}

						//Play sound
						Program.Sounds.PlayCarSound(Cars[DriverCar].Sounds.RequestStop[1], 1.0, 1.0, Cars[DriverCar], false);
						//If message is not empty, add it
						if (!string.IsNullOrEmpty(stopRequest.PassMessage) && IsPlayerTrain)
						{
							Game.AddMessage(stopRequest.PassMessage, MessageDependency.None, GameMode.Normal, MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
						}

						return;
					}

					//Play sound
					Program.Sounds.PlayCarSound(Cars[DriverCar].Sounds.RequestStop[0], 1.0, 1.0, Cars[DriverCar], false);
					//If message is not empty, add it
					if (!string.IsNullOrEmpty(stopRequest.StopMessage) && IsPlayerTrain)
					{
						Game.AddMessage(stopRequest.StopMessage, MessageDependency.None, GameMode.Normal, MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
					}
				}
				else
				{
					Program.Sounds.PlayCarSound(Cars[DriverCar].Sounds.RequestStop[1], 1.0, 1.0, Cars[DriverCar], false);
					if (stopRequest.FullSpeed)
					{
						//Pass at linespeed, rather than braking as if for stop
						NextStopSkipped = StopSkipMode.Linespeed;
					}
					else
					{
						NextStopSkipped = StopSkipMode.Decelerate;
					}

					//Play sound
					Program.Sounds.PlayCarSound(Cars[DriverCar].Sounds.RequestStop[1], 1.0, 1.0, Cars[DriverCar], false);
					//If message is not empty, add it
					if (!string.IsNullOrEmpty(stopRequest.PassMessage) && IsPlayerTrain)
					{
						Game.AddMessage(stopRequest.PassMessage, MessageDependency.None, GameMode.Normal, MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
					}
				}
			}
		}
	}
}
