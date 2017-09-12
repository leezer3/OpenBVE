namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>This event is placed at the end of the track</summary>
		internal class TrackEndEvent : GeneralEvent
		{
			internal TrackEndEvent(double TrackPositionDelta)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (TriggerType == EventTriggerType.RearCarRearAxle & Train != TrainManager.PlayerTrain)
				{
					Train.Dispose();
				}
				else if (Train == TrainManager.PlayerTrain)
				{
					Train.Derail(CarIndex, 0.0);
				}
			}
		}
	}
}
