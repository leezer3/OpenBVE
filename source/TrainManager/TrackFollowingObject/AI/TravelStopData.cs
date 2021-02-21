using OpenBveApi.Trains;

namespace TrainManager.Trains
{
	/// <summary>Defines a stopping point in the travel data</summary>
	public class TravelStopData : TravelData
	{
		// Parameters from XML
		public override double PassingSpeed => 0.0;
		/// <summary>The time at which the stop is performed</summary>
		public double StopTime;
		/// <summary>Whether the left doors are to be opened</summary>
		public bool OpenLeftDoors;
		/// <summary>Whether the right doors are to be opened</summary>
		public bool OpenRightDoors;
		/// <summary>The new travel direction</summary>
		public TravelDirection Direction;

		// Parameters calculated by the SetupTravelData function
		internal double OpeningDoorsEndTime;
		internal double ClosingDoorsStartTime;
	}
}
