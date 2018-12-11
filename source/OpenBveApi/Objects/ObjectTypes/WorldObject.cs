using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>Represents an abstract object or sound which may move within the game world</summary>
	public abstract class WorldObject
	{
	/// <summary>The position vector for this object</summary>
		public Vector3 Position;
		/// <summary>The track position for this object</summary>
		public double TrackPosition;
		/// <summary>Whether the object is currently visible at the player's camera position</summary>
		public bool Visible;
		/// <summary>The Direction vector</summary>
		public Vector3 Direction;
		/// <summary>The Up vector</summary>
		public Vector3 Up;
		/// <summary>The Side vector</summary>
		public Vector3 Side;
		/// <summary>Updates the functions for this object</summary>
		/// <param name="TimeElapsed">The time elapsed since the last call to Update</param>
		/// <param name="ForceUpdate">Whether this is a forced update (e.g. station jump)</param>
		public abstract void Update(double TimeElapsed, bool ForceUpdate);
	}
}
