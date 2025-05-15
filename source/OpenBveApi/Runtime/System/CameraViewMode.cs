using System;

namespace OpenBveApi.Runtime
{

	/// <summary>Represents the available camera view modes.</summary>
	[Flags]
	public enum CameraViewMode
	{
		/// <summary>Unknown</summary>
		/// <remarks>Used for bitwise operations</remarks>
		NotDefined = 0,
		/// <summary>The interior of a 2D cab</summary>
		Interior = 2,
		/// <summary>The interior of a 3D cab</summary>
		InteriorLookAhead = 4,
		/// <summary>An exterior camera attached to a train</summary>
		Exterior = 8,
		/// <summary>A camera attached to the track</summary>
		Track = 16,
		/// <summary>A fly-by camera attached to a point on the track</summary>
		FlyBy = 32,
		/// <summary>A fly-by zooming camera attached to a point on the track</summary>
		FlyByZooming = 64
	}
}
