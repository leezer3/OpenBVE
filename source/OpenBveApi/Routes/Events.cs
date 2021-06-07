namespace OpenBveApi.Routes
{
	/// <summary>The general event is the abstract event type which all events must inherit from</summary>
	public abstract class GeneralEvent
	{
		/// <summary>The delta track position that this event is placed at</summary>
		public double TrackPositionDelta;

		/// <summary>Whether this event should trigger once for the specified train, or multiple times</summary>
		protected bool DontTriggerAnymore;

		/// <summary>
		/// Creates a new general event
		/// </summary>
		protected GeneralEvent() : this(0.0)
		{
		}

		/// <summary>
		/// Creates a new general event
		/// </summary>
		/// <param name="trackPositionDelta">The delta track position</param>
		protected GeneralEvent(double trackPositionDelta)
		{
			TrackPositionDelta = trackPositionDelta;
		}

		/// <summary>The unconditional event trigger function</summary>
		/// <param name="direction">The direction:
		///  1 - Forwards
		/// -1 - Reverse</param>
		/// <param name="trackFollower">The TrackFollower</param>
		public abstract void Trigger(int direction, TrackFollower trackFollower);

		/// <summary>This method is called to attempt to trigger an event</summary>
		/// <param name="direction">The direction:
		///  1 - Forwards
		/// -1 - Reverse</param>
		/// <param name="trackFollower">The TrackFollower</param>
		public void TryTrigger(int direction, TrackFollower trackFollower)
		{
			if (!DontTriggerAnymore)
			{
				Trigger(direction, trackFollower);
			}
		}

		/// <summary>Resets the event</summary>
		public virtual void Reset()
		{

		}
	}
}
