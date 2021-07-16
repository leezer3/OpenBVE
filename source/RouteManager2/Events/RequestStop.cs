using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace RouteManager2.Events
{
	/// <summary>Called when a train passes over the trigger for a request stop</summary>
	public class RequestStopEvent : GeneralEvent
	{
		private readonly CurrentRoute currentRoute;

		/// <summary>The index of the station which this applies to</summary>
		private readonly int stationIndex;

		/// <summary>The request stop to be triggered if we are early</summary>
		private readonly RequestStop early;

		/// <summary>The request stop to be triggered if we are on-time</summary>
		private readonly RequestStop onTime;

		/// <summary>The request stop to be triggered if we are late</summary>
		private readonly RequestStop late;

		/// <summary>Whether we pass this stop at linespeed if it is not requested</summary>
		private readonly bool fullSpeed;

		/// <summary>This stop is only triggered for trains under MaxCars</summary>
		private readonly int maxCars;

		public RequestStopEvent(CurrentRoute CurrentRoute, int StationIndex, int MaxCars, bool FullSpeed, RequestStop OnTime, RequestStop Early, RequestStop Late)
		{
			currentRoute = CurrentRoute;

			fullSpeed = FullSpeed;
			onTime = OnTime;
			stationIndex = StationIndex;
			early = Early;
			late = Late;
			maxCars = MaxCars;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			if (trackFollower.TriggerType == EventTriggerType.FrontCarFrontAxle)
			{
				RequestStop stop; //Temp probability value
				if (early.Time != -1 && currentRoute.SecondsSinceMidnight < early.Time)
				{
					stop = early;
				}
				else if (early.Time != -1 && currentRoute.SecondsSinceMidnight > early.Time && late.Time != -1 && currentRoute.SecondsSinceMidnight < late.Time)
				{
					stop = onTime;
				}
				else if (late.Time != -1 && currentRoute.SecondsSinceMidnight > late.Time)
				{
					stop = late;
				}
				else
				{
					stop = onTime;
				}

				stop.MaxCars = maxCars;
				stop.StationIndex = stationIndex;
				stop.FullSpeed = fullSpeed;

				if (direction > 0)
				{
					trackFollower.Train.RequestStop(stop);
				}
			}
		}
	}
}
