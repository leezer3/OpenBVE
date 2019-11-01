using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace RouteManager2.Events
{
	/// <summary>Is called when a train passes a station with the Pass Alarm enabled without stopping</summary>
	public class StationPassAlarmEvent : GeneralEvent
	{
		public StationPassAlarmEvent(double TrackPositionDelta)
		{
			this.TrackPositionDelta = TrackPositionDelta;
			this.DontTriggerAnymore = false;
		}
		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			if (TriggerType == EventTriggerType.FrontCarFrontAxle)
			{
				if (Direction > 0) //FIXME: This only works for routes written in the forwards direction
				{
					dynamic t = Train;
					t.SafetySystems.PassAlarm.Trigger();
					this.DontTriggerAnymore = true;
				}
			}
		}

		public override void Reset()
		{
			this.DontTriggerAnymore = false;
		}
	}
}
