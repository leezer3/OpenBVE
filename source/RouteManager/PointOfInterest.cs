using OpenBveApi.Math;

namespace OpenBve.RouteManager
{
	/// <summary>Defines a point of interest within the game world</summary>
	public struct PointOfInterest
	{
		/// <summary>The track position</summary>
		public double TrackPosition;
		/// <summary>The offset from Track 0's position</summary>
		public Vector3 TrackOffset;
		/// <summary>The yaw</summary>
		public double TrackYaw;
		/// <summary>The pitch</summary>
		public double TrackPitch;
		/// <summary>The roll</summary>
		public double TrackRoll;
		/// <summary>The textual message to be displayed when jumping to this point</summary>
		public string Text;
	}
}
