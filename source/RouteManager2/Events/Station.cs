using OpenBveApi.Hosts;
using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace RouteManager2.Events
{
	/// <summary>Placed at the start of every station</summary>
	public class StationStartEvent : GeneralEvent
	{
		private readonly HostInterface currentHost;

		/// <summary>The index of the station this event describes</summary>
		public readonly int StationIndex;

		public StationStartEvent(HostInterface Host, double TrackPositionDelta, int StationIndex)
		{
			currentHost = Host;
			this.TrackPositionDelta = TrackPositionDelta;
			DontTriggerAnymore = false;
			this.StationIndex = StationIndex;
		}

		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			switch (TriggerType)
			{
				case EventTriggerType.TrainFront:
					Train.EnterStation(StationIndex, Direction);
					break;
				case EventTriggerType.Camera:
					currentHost.CameraEnterStation(StationIndex, Direction);
					break;
			}
		}
	}

	/// <summary>Placed at the end of every station (as defined by the last possible stop point)</summary>
	public class StationEndEvent : GeneralEvent
	{
		private readonly HostInterface currentHost;

		private readonly CurrentRoute Route;

		/// <summary>The index of the station this event describes</summary>
		public readonly int StationIndex;

		public StationEndEvent(HostInterface Host, CurrentRoute Route, double TrackPositionDelta, int StationIndex)
		{
			currentHost = Host;
			this.Route = Route;
			this.TrackPositionDelta = TrackPositionDelta;
			DontTriggerAnymore = false;
			this.StationIndex = StationIndex;
		}

		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			switch (TriggerType)
			{
				case EventTriggerType.FrontCarFrontAxle:
					if (Direction > 0)
					{
						if (Train.IsPlayerTrain)
						{
							currentHost.UpdateCustomTimetable(Route.Stations[StationIndex].TimetableDaytimeTexture, Route.Stations[StationIndex].TimetableNighttimeTexture);
						}
					}
					break;
				case EventTriggerType.RearCarRearAxle:
					Train.LeaveStation(StationIndex, Direction);
					break;
				case EventTriggerType.Camera:
					currentHost.CameraLeaveStation(StationIndex, Direction);
					break;
			}
		}
	}
}
