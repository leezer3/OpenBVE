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
}
