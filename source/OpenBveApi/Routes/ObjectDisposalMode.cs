namespace OpenBveApi.Routes
{
	/// <summary>Controls the way in which objects are disposed by the renderer</summary>
	public enum ObjectDisposalMode
	{
		/// <summary>Objects will disappear when the block in which their command is placed is exited via by the camera</summary>
		/// <remarks>Certain BVE2 / BVE4 routes use this as an animation trick</remarks>
		Legacy = 0,
		/// <summary>The object will disappear when it's furthest vertex is passed by the camera</summary>
		Accurate = 1,
		/// <summary>Quad tree visibility is used based upon the camera position</summary>
		QuadTree = 2,
		/// <summary>Tiled mode for use with Mechanik</summary>
		Mechanik = 99
	}
}
