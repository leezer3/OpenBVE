using OpenBveApi.Math;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		/// <summary>Represents an abstract object or sound placed within the game world</summary>
		internal abstract class WorldObject
		{
			/// <summary>The position vector for this object</summary>
			internal Vector3 Position;
			/// <summary>The track position for this object</summary>
			internal double TrackPosition;
			/// <summary>Whether the object is currently visible at the player's camera position</summary>
			internal bool Visible;

			internal Vector3 Direction;
			internal Vector3 Up;
			internal Vector3 Side;

			internal abstract void Update(double TimeElapsed, bool ForceUpdate);
		}
	}
}
