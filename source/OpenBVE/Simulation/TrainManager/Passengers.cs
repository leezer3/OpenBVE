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
			private double CurrentAcceleration;
			/// <summary>The speed difference from that of the last frame</summary>
			private double CurrentSpeedDifference;
			/// <summary>Whether the passengers have fallen over (Used for scoring purposes)</summary>
			internal bool FallenOver;

			/// <summary>Called once a frame to update the status of the passengers</summary>
			/// <param name="Acceleration">The current average acceleration of the train</param>
			/// <param name="TimeElapsed">The frame time elapsed</param>
			internal void Update(double Acceleration, double TimeElapsed)
			{
				double accelerationDifference = Acceleration - CurrentAcceleration;
				double jerk = 0.25 + 0.10 * Math.Abs(accelerationDifference);
				double accelerationQuanta = jerk * TimeElapsed;
				if (Math.Abs(accelerationDifference) < accelerationQuanta)
				{
					CurrentAcceleration = Acceleration;
					accelerationDifference = 0.0;
				}
				else
				{
					CurrentAcceleration += (double)Math.Sign(accelerationDifference) * accelerationQuanta;
					accelerationDifference = Acceleration - CurrentAcceleration;
				}
				CurrentSpeedDifference += accelerationDifference * TimeElapsed;
				double acceleration = 0.10 + 0.35 * Math.Abs(CurrentSpeedDifference);
				double speedQuanta = acceleration * TimeElapsed;
				if (Math.Abs(CurrentSpeedDifference) < speedQuanta)
				{
					CurrentSpeedDifference = 0.0;
				}
				else
				{
					CurrentSpeedDifference -= (double)Math.Sign(CurrentSpeedDifference) * speedQuanta;
				}
				if (PassengerRatio > 0.0)
				{
					double threshold = 1.0 / PassengerRatio;
					if (Math.Abs(CurrentSpeedDifference) > threshold)
					{
						FallenOver = true;
					}
					else
					{
						FallenOver = false;
					}
				}
				else
				{
					FallenOver = false;
				}
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
