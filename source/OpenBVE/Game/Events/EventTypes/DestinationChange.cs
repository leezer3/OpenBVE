namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Called when a train passes over a destination change event</summary>
		internal class DestinationEvent : GeneralEvent
		{
			/// <summary>The destination value to set when passing over this event forwards, or -1 to disable</summary>
			internal readonly int NextDestination;
			/// <summary>The destination value to set when passing over this event backwards, or -1 to disable</summary>
			internal readonly int PreviousDestination;
			/// <summary>Whether this event should trigger once or multiple times</summary>
			internal readonly bool TriggerOnce;
			/// <summary>Whether this event is triggered by AI only (-1), both (0) or player only (1)</summary>
			internal readonly int Type;

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
