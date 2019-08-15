using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace OpenBve.RouteManager
{
	/// <summary>Called when a train passes over the trigger for a request stop</summary>
	public class RequestStopEvent : GeneralEvent
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

		public RequestStopEvent(int stationIndex, int maxCars, bool fullSpeed, RequestStop onTime, RequestStop early, RequestStop late)
		{
			this.FullSpeed = fullSpeed;
			this.OnTime = onTime;
			this.StationIndex = stationIndex;
			this.Early = early;
			this.Late = late;
			this.MaxCars = maxCars;
		}

		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			if (TriggerType == EventTriggerType.FrontCarFrontAxle)
			{
				RequestStop stop; //Temp probability value
				if (Early.Time != -1 && CurrentRoute.SecondsSinceMidnight < Early.Time)
				{
					stop = Early;
				}
				else if (Early.Time != -1 && CurrentRoute.SecondsSinceMidnight > Early.Time && Late.Time != -1 && CurrentRoute.SecondsSinceMidnight < Late.Time)
				{
					stop = OnTime;
				}
				else if (Late.Time != -1 && CurrentRoute.SecondsSinceMidnight > Late.Time)
				{
					stop = Late;
				}
				else
				{
					stop = OnTime;
				}

				stop.MaxCars = MaxCars;
				stop.StationIndex = StationIndex;
				stop.FullSpeed = FullSpeed;
				if (Direction > 0)
				{
					Train.RequestStop(stop);
				}
			}
		}
	}
}
