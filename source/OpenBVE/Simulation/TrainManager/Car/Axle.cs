using OpenBveApi.Routes;
using SoundManager;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		// axle
		internal class Axle
		{
			internal TrackFollower Follower;
			internal bool CurrentWheelSlip;
			internal double Position;
			internal CarSound[] PointSounds;
#pragma warning disable 649
			//set by dynamic elsewhere
			internal int FlangeIndex;
			internal int RunIndex;
#pragma warning restore 649
			/// <summary>Stores whether the point sound has been triggered</summary>
			internal bool PointSoundTriggered;

			internal Axle(TrainManager.Train Train, TrainManager.Car Car)
			{
				Follower = new TrackFollower(Program.CurrentHost, Train, Car);
			}
		}
	}
}
