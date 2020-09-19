namespace OpenBveApi.Runtime
{
	/// <summary>Represents the available camera view modes.</summary>
	public enum CameraViewMode
	{
		/// <summary>The interior of a 2D cab</summary>
		Interior,
		/// <summary>The interior of a 3D cab</summary>
		InteriorLookAhead,
		/// <summary>An exterior camera attached to a train</summary>
		Exterior,
		/// <summary>A camera attached to the track</summary>
		Track,
		/// <summary>A fly-by camera attached to a point on the track</summary>
		FlyBy,
		/// <summary>A fly-by zooming camera attached to a point on the track</summary>
		FlyByZooming
	}
}
