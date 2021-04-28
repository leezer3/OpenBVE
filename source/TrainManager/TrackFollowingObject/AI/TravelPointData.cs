namespace TrainManager.Trains
{
	public class TravelPointData : TravelData
	{
		// Parameters calculated by the SetupTravelData function
		private double PassingTime;
		internal override double ArrivalTime
		{
			get => PassingTime;
			set => PassingTime = value;
		}
		internal override double DepartureTime
		{
			get => PassingTime;
			set => PassingTime = value;
		}
	}
}
