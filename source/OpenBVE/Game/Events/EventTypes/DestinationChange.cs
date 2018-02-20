namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Called when a train passes over a destination change event</summary>
		internal class DestinationEvent : GeneralEvent
		{
			/// <summary>An optional data parameter passed to plugins recieving this event</summary>
			internal readonly int NextDestination;
			internal readonly int PreviousDestination;
			internal readonly bool TriggerOnce;
			internal int Type;

			internal DestinationEvent(double trackPositionDelta, int type, int nextDestination, int previousDestination, bool triggerOnce)
			{
				this.TrackPositionDelta = trackPositionDelta;
				this.DontTriggerAnymore = false;
				this.NextDestination = nextDestination;
				this.PreviousDestination = previousDestination;
				this.TriggerOnce = triggerOnce;
				this.Type = type;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (this.Type == -1 && Train == TrainManager.PlayerTrain || this.Type == 1 && Train != TrainManager.PlayerTrain)
				{
					return;
				}
				if (this.DontTriggerAnymore)
				{
					return;
				}
				if (TriggerType == EventTriggerType.TrainFront)
				{
					if (Direction > 0)
					{
						if (NextDestination != -1)
						{
							Train.Destination = NextDestination;
						}
						if (TriggerOnce)
						{
							DontTriggerAnymore = true;
						}
					}
					else
					{
						if (PreviousDestination != -1)
						{
							Train.Destination = PreviousDestination;
						}
						if (TriggerOnce)
						{
							DontTriggerAnymore = true;
						}
					}
				}
			}
		}
	}
}
