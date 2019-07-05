namespace OpenBve.RouteManager
{
	/// <summary>Defines a request stop which may be made by a train</summary>
	public class RequestStop
	{
		/// <summary>The probability that this stop may be called</summary>
		public int Probability;
		/// <summary>The time at which this stop may be called</summary>
		public double Time = -1;
		/// <summary>The message displayed when the train is to stop</summary>
		public string StopMessage;
		/// <summary>The message displayed when the train is to pass</summary>
		public string PassMessage;
	}
}
