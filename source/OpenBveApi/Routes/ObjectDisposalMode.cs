namespace OpenBveApi.Routes
{
	/// <summary>Controls the way in which objects are disposed by the renderer</summary>
	public enum ObjectDisposalMode
	{
		/// <summary>Objects will disappear when the block in which their command is placed is exited via by the camera</summary>
		/// <remarks>Certain BVE2 / BVE4 routes use this as an animation trick</remarks>
		Legacy,
		/// <summary>The object will disappear when it's furthest vertex is passed by the camera</summary>
		Accurate,
		/// <summary>Tiled mode for use with Mechanik</summary>
		Mechanik
	}
}
