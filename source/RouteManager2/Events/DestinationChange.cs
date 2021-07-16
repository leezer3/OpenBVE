using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace RouteManager2.Events
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

		public DestinationEvent(double TrackPositionDelta, int Type, int NextDestination, int PreviousDestination, bool TriggerOnce) : base(TrackPositionDelta)
		{
			DontTriggerAnymore = false;
			this.NextDestination = NextDestination;
			this.PreviousDestination = PreviousDestination;
			this.TriggerOnce = TriggerOnce;
			this.Type = Type;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			AbstractTrain train = trackFollower.Train;

			if (train == null || Type == -1 && train.IsPlayerTrain || Type == 1 && !train.IsPlayerTrain)
			{
				return;
			}

			if (DontTriggerAnymore)
			{
				return;
			}

			if (trackFollower.TriggerType == EventTriggerType.TrainFront)
			{
				if (direction > 0)
				{
					if (NextDestination != -1)
					{
						train.Destination = NextDestination;
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
						train.Destination = PreviousDestination;
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
