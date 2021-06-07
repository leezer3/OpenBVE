using OpenBveApi.Hosts;
using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace RouteManager2.Events
{
	/// <summary>Placed at the start of every station</summary>
	public class StationStartEvent : GeneralEvent
	{
		/// <summary>The index of the station this event describes</summary>
		public readonly int StationIndex;

		public StationStartEvent(double TrackPositionDelta, int StationIndex)
		{
			this.TrackPositionDelta = TrackPositionDelta;
			DontTriggerAnymore = false;
			this.StationIndex = StationIndex;
		}

		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			if (TriggerType == EventTriggerType.TrainFront)
			{
				Train.EnterStation(StationIndex, Direction);
			}
		}
	}

	/// <summary>Placed at the end of every station (as defined by the last possible stop point)</summary>
	public class StationEndEvent : GeneralEvent
	{
		/// <summary>The index of the station this event describes</summary>
		public readonly int StationIndex;

		private readonly HostInterface currentHost;

		private readonly CurrentRoute Route;

		public StationEndEvent(double TrackPositionDelta, int StationIndex, CurrentRoute Route, HostInterface Host)
		{
			this.TrackPositionDelta = TrackPositionDelta;
			this.DontTriggerAnymore = false;
			this.StationIndex = StationIndex;
			this.Route = Route;
			this.currentHost = Host;
		}

		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			if (TriggerType == EventTriggerType.FrontCarFrontAxle)
			{
				if (Direction > 0)
				{
					if (Train.IsPlayerTrain)
					{
						currentHost.UpdateCustomTimetable(Route.Stations[this.StationIndex].TimetableDaytimeTexture, Route.Stations[this.StationIndex].TimetableNighttimeTexture);
					}
				}
			}
			else if (TriggerType == EventTriggerType.RearCarRearAxle)
			{
				Train.LeaveStation(StationIndex, Direction);
			}
		}
	}
}
