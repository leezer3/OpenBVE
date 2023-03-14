namespace LibRender2.Objects
{
	/// <summary>The mode in which visibility updates are performed</summary>
	/// <remarks>Only relavant whilst using non QuadTree visibility modes</remarks>
	internal enum VisibilityUpdate
	{
		/// <summary>No update is performed</summary>
		None,
		/// <summary>A normal update is performed</summary>
		Normal,
		/// <summary>A forced update is performed</summary>
		/// <remarks>This moves the camera fractionally in each direction</remarks>
		Force
	}
}
