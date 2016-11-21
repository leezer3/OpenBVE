using System;

namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>The passenger loading of a train at any given point in time</summary>
		internal struct TrainPassengers
		{
			/// <summary>The current passenger ratio: 
			/// Must be a number between 0 and 250, where 100 represents a nominally loaded train</summary>
			internal double PassengerRatio;
			/// <summary>The current acceleration as being felt by the passengers</summary>
			internal double CurrentAcceleration;
			/// <summary>The speed difference from that of the last frame</summary>
			internal double CurrentSpeedDifference;
			/// <summary>Whether the passengers have fallen over (Used for scoring purposes)</summary>
			internal bool FallenOver;
		}

		/// <summary>Called once a frame to update the passengers of the given train</summary>
		/// <param name="Train">The train</param>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		private static void UpdateTrainPassengers(Train Train, double TimeElapsed)
		{
			double accelerationDifference = Train.Specs.CurrentAverageAcceleration - Train.Passengers.CurrentAcceleration;
			double jerk = 0.25 + 0.10 * Math.Abs(accelerationDifference);
			double accelerationQuanta = jerk * TimeElapsed;
			if (Math.Abs(accelerationDifference) < accelerationQuanta)
			{
				Train.Passengers.CurrentAcceleration = Train.Specs.CurrentAverageAcceleration;
				accelerationDifference = 0.0;
			}
			else
			{
				Train.Passengers.CurrentAcceleration += (double)Math.Sign(accelerationDifference) * accelerationQuanta;
				accelerationDifference = Train.Specs.CurrentAverageAcceleration - Train.Passengers.CurrentAcceleration;
			}
			Train.Passengers.CurrentSpeedDifference += accelerationDifference * TimeElapsed;
			double acceleration = 0.10 + 0.35 * Math.Abs(Train.Passengers.CurrentSpeedDifference);
			double speedQuanta = acceleration * TimeElapsed;
			if (Math.Abs(Train.Passengers.CurrentSpeedDifference) < speedQuanta)
			{
				Train.Passengers.CurrentSpeedDifference = 0.0;
			}
			else
			{
				Train.Passengers.CurrentSpeedDifference -= (double)Math.Sign(Train.Passengers.CurrentSpeedDifference) * speedQuanta;
			}
			if (Train.Passengers.PassengerRatio > 0.0)
			{
				double threshold = 1.0 / Train.Passengers.PassengerRatio;
				if (Math.Abs(Train.Passengers.CurrentSpeedDifference) > threshold)
				{
					Train.Passengers.FallenOver = true;
				}
				else
				{
					Train.Passengers.FallenOver = false;
				}
			}
			else
			{
				Train.Passengers.FallenOver = false;
			}
		}

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
				Train.Cars[i].Specs.MassCurrent = Train.Cars[i].Specs.MassEmpty + passengerMass;
			}
		}
	}
}
