namespace OpenBveApi.Graphics
{
	/// <summary>The different visibility modes used by the camera / object disposal</summary>
	public enum VisibilityModes
	{
		/// <summary>An object is made invisible after the end of the track block it is situated in is passed by the camera</summary>
		/// <remarks>Default setting</remarks>
		Legacy,
		/// <summary>An object is made invisible after the last vertex is passed by the camera using the track as a reference point</summary>
		/// <remarks>With Options.ObjectVisibility 1 set in CSV routes</remarks>
		TrackBased,
		/// <summary>World-space quad trees are used to determine object visibility based upon the camera position</summary>
		/// <remarks>With Options.ObjectVisibility 2 set in CSV routes, or non BVE content</remarks>
		QuadTree
	}
}
