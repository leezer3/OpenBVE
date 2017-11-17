namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		// axle
		internal struct Axle
		{
			internal TrackManager.TrackFollower Follower;
			internal bool CurrentWheelSlip;
			internal double Position;
			internal CarSound[] PointSounds;
			internal int FlangeIndex;
			internal int RunIndex;
		}
	}
}
