namespace OpenBveApi.Trains
{
	/// <summary>Describes the travel direction of a train relative to the route</summary>
	/// <remarks>This assumes that the route is written in linear forwards notation(e.g. BVE)</remarks>
	public enum TravelDirection
	{
		/// <summary>Forwards</summary>
		Forward = 1,
		/// <summary>No movement</summary>
		Static = 0,
		/// <summary>Backwards</summary>
		Backward = -1
	}
}
