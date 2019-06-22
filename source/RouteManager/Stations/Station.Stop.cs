namespace OpenBve.RouteManager
{
	/// <summary>Defines a station stop point</summary>
	public struct StationStop
	{
		/// <summary>The track position of the stop point</summary>
		public double TrackPosition;
		/// <summary>The forward stop tolerance in meters (Default: 5)</summary>
		public double ForwardTolerance;
		/// <summary>The backwards stop tolerance in meters (Default: 5)</summary>
		public double BackwardTolerance;
		/// <summary>The number of cars this stop point applies to, or 0 for all</summary>
		public int Cars;
	}
}
