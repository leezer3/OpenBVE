namespace OpenBveApi.Trains
{
	/// <summary>The different methods by which a stop may be skipped</summary>
	public enum StopSkipMode
	{
		/// <summary>This stop is not skipped</summary>
		None = 0,
		/// <summary>The train decelerates through the station to a near stop</summary>
		Decelerate = 1,
		/// <summary>The train skips the stop at linespeed</summary>
		Linespeed = 2
	}
}
