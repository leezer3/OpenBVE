using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using RouteManager2.MessageManager;

namespace TrainManager.Trains
{
	public partial class TrainBase
	{
		/// <inheritdoc/>
		public override void RequestStop(RequestStop stopRequest)
		{
			if (stopRequest.MaxCars != 0 && NumberOfCars > stopRequest.MaxCars)
			{
				//Check whether our train length is valid for this before doing anything else
				Cars[DriverCar].Sounds.RequestStop[2].Play(Cars[DriverCar], false);
				return;
			}

			if (TrainManagerBase.RandomNumberGenerator.Next(0, 100) <= stopRequest.Probability)
			{
				//We have hit our probability roll
				if (TrainManagerBase.CurrentRoute.Stations[stopRequest.StationIndex].StopMode == StationStopMode.AllRequestStop || (IsPlayerTrain && TrainManagerBase.CurrentRoute.Stations[stopRequest.StationIndex].StopMode == StationStopMode.PlayerRequestStop))
				{

					//If our train can stop at this station, set it's index accordingly
					Station = stopRequest.StationIndex;
					NextStopSkipped = StopSkipMode.None;
					//Play sound
					Cars[DriverCar].Sounds.RequestStop[0].Play(Cars[DriverCar], false);
				}
				else
				{
					//We don't meet the conditions for this request stop
					//Pass at linespeed, rather than braking as if for stop
					NextStopSkipped = stopRequest.FullSpeed ? StopSkipMode.Linespeed : StopSkipMode.Decelerate;

					//Play sound
					Cars[DriverCar].Sounds.RequestStop[1].Play(Cars[DriverCar], false);
					//If message is not empty, add it
					if (!string.IsNullOrEmpty(stopRequest.PassMessage) && IsPlayerTrain)
					{
						TrainManagerBase.currentHost.AddMessage(stopRequest.PassMessage, MessageDependency.None, GameMode.Normal, MessageColor.White, 10.0, null);
					}

					return;
				}

				//Play sound
				Cars[DriverCar].Sounds.RequestStop[0].Play(Cars[DriverCar], false);
				//If message is not empty, add it
				if (!string.IsNullOrEmpty(stopRequest.StopMessage) && IsPlayerTrain)
				{
					TrainManagerBase.currentHost.AddMessage(stopRequest.StopMessage, MessageDependency.None, GameMode.Normal, MessageColor.White, 10.0, null);
				}
			}
			else
			{
				Cars[DriverCar].Sounds.RequestStop[1].Play(Cars[DriverCar], false);
				//Pass at linespeed, rather than braking as if for stop
				NextStopSkipped = stopRequest.FullSpeed ? StopSkipMode.Linespeed : StopSkipMode.Decelerate;

				//Play sound
				Cars[DriverCar].Sounds.RequestStop[1].Play(Cars[DriverCar], false);
				//If message is not empty, add it
				if (!string.IsNullOrEmpty(stopRequest.PassMessage) && IsPlayerTrain)
				{
					TrainManagerBase.currentHost.AddMessage(stopRequest.PassMessage, MessageDependency.None, GameMode.Normal, MessageColor.White, 10.0, null);
				}
			}
		}
	}
}
