namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>The available trigger types for any event</summary>
		internal enum EventTriggerType
		{
			None = 0,
			Camera = 1,
			FrontCarFrontAxle = 2,
			RearCarRearAxle = 3,
			OtherCarFrontAxle = 4,
			OtherCarRearAxle = 5,
			TrainFront = 6
		}
	}
}
