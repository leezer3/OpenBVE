namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		internal enum StopSkipMode
		{
			/// <summary>This stop is not skipped</summary>
			None = 0,
			/// <summary>The train decelerates through the station to a near stop</summary>
			Decelerate = 1,
			/// <summary>The train skips the stop at linespeed</summary>
			Linespeed = 2
		}
	}
}
