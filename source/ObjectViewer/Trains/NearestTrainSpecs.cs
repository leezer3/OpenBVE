namespace ObjectViewer.Trains
{
	internal class NearestTrainSpecs
	{
		internal int NumberOfCars;
		internal int PowerNotches;
		internal bool IsAirBrake;
		internal int BrakeNotches;
		internal bool HasHoldBrake;
		internal bool HasConstSpeed;

		internal NearestTrainSpecs()
		{
			NumberOfCars = 1;
			PowerNotches = 8;
			BrakeNotches = 8;
		}
	}
}
