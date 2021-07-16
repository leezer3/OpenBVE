using OpenBveApi.Routes;

namespace RouteManager2.Events
{
	/// <summary>Is called when a train passes a station with the Pass Alarm enabled without stopping</summary>
	public class StationPassAlarmEvent : GeneralEvent
	{
		public StationPassAlarmEvent(double TrackPositionDelta) : base(TrackPositionDelta)
		{
			DontTriggerAnymore = false;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			if (trackFollower.TriggerType == EventTriggerType.FrontCarFrontAxle)
			{
				if (direction > 0) //FIXME: This only works for routes written in the forwards direction
				{
					dynamic t = trackFollower.Train;
					t.SafetySystems.PassAlarm.Trigger();
					DontTriggerAnymore = true;
				}
			}
		}

		public override void Reset()
		{
			DontTriggerAnymore = false;
		}
	}
}
