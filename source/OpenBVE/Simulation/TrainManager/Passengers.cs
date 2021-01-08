using System;

namespace OpenBve
{
	public partial class TrainManager
	{
		/// <summary>Called after passengers have finished boarding, in order to update the train's mass from the new passenger ratio</summary>
		/// <param name="Train">The train</param>
		private static void UpdateTrainMassFromPassengerRatio(Train Train)
		{
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				double area = Train.Cars[i].Width * Train.Cars[i].Length;
				const double passengersPerArea = 1.0;
				double randomFactor = 0.9 + 0.2 * Program.RandomNumberGenerator.NextDouble();
				double passengers = Math.Round(randomFactor * Train.Passengers.PassengerRatio * passengersPerArea * area);
				const double massPerPassenger = 70.0;
				double passengerMass = passengers * massPerPassenger;
				Train.Cars[i].CargoMass = passengerMass;
			}
		}
	}
}
