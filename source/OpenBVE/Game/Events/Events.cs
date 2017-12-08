namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>The general event is the abstract event type which all events must inherit from</summary>
		internal abstract class GeneralEvent
		{
			/// <summary>The delta track position that this event is placed at</summary>
			internal double TrackPositionDelta;
			/// <summary>Whether this event should trigger once for the specified train, or multiple times</summary>
			internal bool DontTriggerAnymore;

			/// <summary>The unconditional event trigger function</summary>
			/// <param name="Direction">The direction:
			///  1 - Forwards
			/// -1 - Reverse</param>
			/// <param name="TriggerType">The trigger type (Car axle, camera follower etc.)</param>
			/// <param name="Train">The train, or a null reference</param>
			/// <param name="CarIndex">The car index, or a null reference</param>
			internal abstract void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex);

			/// <summary>This method is called to attempt to trigger an event</summary>
			/// <param name="Direction">The direction:
			///  1 - Forwards
			/// -1 - Reverse</param>
			/// <param name="TriggerType">The trigger type (Car axle, camera follower etc.)</param>
			/// <param name="Train">The train, or a null reference</param>
			/// <param name="CarIndex">The car index, or a null reference</param>
			internal void TryTrigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (!DontTriggerAnymore)
				{
					Trigger(Direction, TriggerType, Train, CarIndex);
				}
			}
		}
	}
}
