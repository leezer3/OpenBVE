namespace OpenBveApi.Trains
{
	/// <summary>Defines a request stop which may be made by a train</summary>
	public class RequestStop
	{
		/// <summary>The index of the station to stop at</summary>
		public int StationIndex;
		/// <summary>The maximum number of cars that a train may have to stop</summary>
		public int MaxCars;
		/// <summary>The probability that this stop may be called</summary>
		public int Probability;
		/// <summary>The time at which this stop may be called</summary>
		public double Time = -1;
		/// <summary>The message displayed when the train is to stop</summary>
		public string StopMessage;
		/// <summary>The message displayed when the train is to pass</summary>
		public string PassMessage;
		/// <summary>Whether the stop request is to be passed at line speed</summary>
		public bool FullSpeed;
	}
}
