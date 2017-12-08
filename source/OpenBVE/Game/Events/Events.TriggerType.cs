namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>The available trigger types for any event</summary>
		internal enum EventTriggerType
		{
			/// <summary>This event will not be triggered</summary>
			None = 0,
			/// <summary>This event is triggered by the track follower for the camera</summary>
			Camera = 1,
			/// <summary>This event is triggered by the leading axle of a train</summary>
			FrontCarFrontAxle = 2,
			/// <summary>This event is triggered by the trailing axle of a train</summary>
			RearCarRearAxle = 3,
			/// <summary>This event is triggered by the front axles of every car on the train</summary>
			OtherCarFrontAxle = 4,
			/// <summary>This event is triggered by the rear axles of every car on the train</summary>
			OtherCarRearAxle = 5,
			/// <summary>This event is triggered by the physical front of the train</summary>
			TrainFront = 6
		}
	}
}
