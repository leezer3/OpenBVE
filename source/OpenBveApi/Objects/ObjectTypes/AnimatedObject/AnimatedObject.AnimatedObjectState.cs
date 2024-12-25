using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>A single state within an animated object</summary>
	public struct AnimatedObjectState
	{
		/// <summary>The position relative to the center of the animated object</summary>
		public Vector3 Position;
		/// <summary>The static object</summary>
		public StaticObject Object;

		/// <summary>Creates a new AnimatedObjectState</summary>
		/// <param name="stateObject">The 3D model</param>
		/// <param name="position">The relative position</param>
		public AnimatedObjectState(StaticObject stateObject, Vector3 position)
		{
			Object = stateObject;
			Position = position;
		}
	}
}
