namespace Plugin
{
	/// <summary>The available disposal modes for a GIF frame</summary>
	internal enum DisposeMode
	{
		/// <summary>The frame will be entirely replaced by the next frame</summary>
		NoAction = 0,
		/// <summary>The frame will be left in place, and the next frame will be drawn over the top using transparency</summary>
		LeaveInPlace = 1,
		/// <summary>The background color / image is drawn first, followed by the next frame using transparency</summary>
		RestoreToBackground = 2,
		/// <summary>Restores the state to the previous undisposed frame image before drawing the next frame using transparency</summary>
		RestoreToPrevious = 3
	}
}
