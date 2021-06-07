using OpenBveApi.Hosts;
using OpenBveApi.Routes;

namespace RouteManager2.Events
{
	/// <summary>Placed at the start of every station</summary>
	public class StationStartEvent : GeneralEvent
	{
		/// <summary>The index of the station this event describes</summary>
		public readonly int StationIndex;

		public StationStartEvent(double TrackPositionDelta, int StationIndex) : base(TrackPositionDelta)
		{
			DontTriggerAnymore = false;
			this.StationIndex = StationIndex;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			trackFollower.EnterStation(StationIndex, direction);

			if (trackFollower.TriggerType == EventTriggerType.TrainFront)
			{
				trackFollower.Train.EnterStation(StationIndex, direction);
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

		public StationEndEvent(double TrackPositionDelta, int StationIndex, CurrentRoute Route, HostInterface Host) : base(TrackPositionDelta)
		{
			this.DontTriggerAnymore = false;
			this.StationIndex = StationIndex;
			this.Route = Route;
			this.currentHost = Host;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			trackFollower.LeaveStation(StationIndex, direction);

			switch (trackFollower.TriggerType)
			{
				case EventTriggerType.FrontCarFrontAxle:
					if (direction > 0)
					{
						if (trackFollower.Train.IsPlayerTrain)
						{
							currentHost.UpdateCustomTimetable(Route.Stations[StationIndex].TimetableDaytimeTexture, Route.Stations[StationIndex].TimetableNighttimeTexture);
						}
					}
					break;
				case EventTriggerType.RearCarRearAxle:
					trackFollower.Train.LeaveStation(StationIndex, direction);
					break;
			}
		}
	}
}
