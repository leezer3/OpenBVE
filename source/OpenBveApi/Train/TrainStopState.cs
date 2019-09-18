namespace OpenBveApi.Trains
{
	/// <summary>The possible states for a station stop</summary>
	public enum TrainStopState
	{
		/// <summary>The stop is still pending</summary>
		Pending = 0, 
		/// <summary>The train is currrently stopped and passengers are boarding</summary>
		Boarding = 1, 
		/// <summary>The stop has been completed, and the train is preparing to depart</summary>
		Completed = 2,
		/// <summary>The train is jumping between stations, and all stops should be ignored</summary>
		Jumping = 3
	}
}
