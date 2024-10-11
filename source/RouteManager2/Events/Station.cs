using OpenBveApi.Hosts;
using OpenBveApi.Routes;

namespace RouteManager2.Events
{
	/// <summary>Placed at the start of every station</summary>
	public class StationStartEvent : GeneralEvent
	{
		/// <summary>The index of the station this event describes</summary>
		public readonly int StationIndex;

		private readonly CurrentRoute currentRoute;

		public StationStartEvent(CurrentRoute CurrentRoute, double TrackPositionDelta, int StationIndex) : base(TrackPositionDelta)
		{
			DontTriggerAnymore = false;
			this.StationIndex = StationIndex;
			currentRoute = CurrentRoute;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			if (currentRoute.Tracks[trackFollower.TrackIndex].Direction == TrackDirection.Reverse)
			{
				direction = -direction;
			}

			trackFollower.EnterStation(StationIndex, direction);

			if (trackFollower.TriggerType == EventTriggerType.TrainFront || trackFollower.TriggerType == EventTriggerType.SingleCarTrain)
			{
				trackFollower.Train.EnterStation(StationIndex, direction);
			}
		}
	}

	/// <summary>Placed at the end of every station (as defined by the last possible stop point)</summary>
	public class StationEndEvent : GeneralEvent
	{
		private readonly HostInterface currentHost;

		private readonly CurrentRoute currentRoute;

		/// <summary>The index of the station this event describes</summary>
		public readonly int StationIndex;

		public StationEndEvent(HostInterface Host, CurrentRoute CurrentRoute, double TrackPositionDelta, int StationIndex) : base(TrackPositionDelta)
		{
			currentHost = Host;
			currentRoute = CurrentRoute;
			DontTriggerAnymore = false;
			this.StationIndex = StationIndex;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			if (currentRoute.Tracks[trackFollower.TrackIndex].Direction == TrackDirection.Reverse)
			{
				direction = -direction;
			}
			trackFollower.LeaveStation(StationIndex, direction);

			switch (trackFollower.TriggerType)
			{
				case EventTriggerType.FrontCarFrontAxle:
					if (direction > 0)
					{
						if (trackFollower.Train.IsPlayerTrain)
						{
							currentHost.UpdateCustomTimetable(currentRoute.Stations[StationIndex].TimetableDaytimeTexture, currentRoute.Stations[StationIndex].TimetableNighttimeTexture);
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
