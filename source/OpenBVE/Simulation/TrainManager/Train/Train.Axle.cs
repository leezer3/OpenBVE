namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>Defines the properties for an axle attached to a car</summary>
		internal struct Axle
		{
			internal TrackManager.TrackFollower Follower;
			internal bool CurrentWheelSlip;
			internal double Position;
			internal CarSound[] PointSounds;
			/// <summary>The current run index for this axle</summary>
			internal int currentRunIdx;
			/// <summary>The current flange index for this axle</summary>
			internal int currentFlangeIdx;
		}
	}
}
