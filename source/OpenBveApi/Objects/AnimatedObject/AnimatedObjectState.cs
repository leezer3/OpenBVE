using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>A state (static object) contained within an animated object</summary>
	public struct AnimatedObjectState
	{
		/// <summary>The position within the object</summary>
		public Vector3 Position;
		/// <summary>The object</summary>
		public StaticObject Object;
	}
}
