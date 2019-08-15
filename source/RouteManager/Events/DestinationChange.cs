using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace OpenBve.RouteManager
{
	/// <summary>Called when a train passes over a destination change event</summary>
	public class DestinationEvent : GeneralEvent
	{
		/// <summary>The destination value to set when passing over this event forwards, or -1 to disable</summary>
		public readonly int NextDestination;
		/// <summary>The destination value to set when passing over this event backwards, or -1 to disable</summary>
		public readonly int PreviousDestination;
		/// <summary>Whether this event should trigger once or multiple times</summary>
		public readonly bool TriggerOnce;
		/// <summary>Whether this event is triggered by AI only (-1), both (0) or player only (1)</summary>
		public readonly int Type;

		public DestinationEvent(double trackPositionDelta, int type, int nextDestination, int previousDestination, bool triggerOnce)
		{
			this.TrackPositionDelta = trackPositionDelta;
			this.DontTriggerAnymore = false;
			this.NextDestination = nextDestination;
			this.PreviousDestination = previousDestination;
			this.TriggerOnce = triggerOnce;
			this.Type = type;
		}
		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			if (Train == null || this.Type == -1 && Train.IsPlayerTrain || this.Type == 1 && !Train.IsPlayerTrain)
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
