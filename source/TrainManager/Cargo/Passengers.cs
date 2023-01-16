using System;
using TrainManager.Car;

namespace TrainManager.Cargo
{
	/// <summary>Represents the loading in a passenger coach of a train at any given point in time</summary>
	public class Passengers : CargoBase
	{
		private readonly CarBase baseCar;

		/// <summary>The current acceleration as being felt by the passengers</summary>
		private double CurrentAcceleration;

		/// <summary>The speed difference from that of the last frame</summary>
		private double CurrentSpeedDifference;

		private double passengerMass;

		public override double Mass => passengerMass;

		public Passengers(CarBase car)
		{
			baseCar = car;
		}

		public override void UpdateLoading(double ratio)
		{
			Ratio = ratio;
			double area = baseCar.Width * baseCar.Length;
			const double passengersPerArea = 1.0; //Nominal 1 passenger per meter of interior space
			double randomFactor = 0.9 + 0.2 * TrainManagerBase.RandomNumberGenerator.NextDouble();
			double passengers = Math.Round(randomFactor * Ratio * passengersPerArea * area);
			const double massPerPassenger = 70.0; //70kg mass per passenger
			passengerMass = passengers * massPerPassenger;
		}

		/// <summary>Called once a frame to update the status of the passengers</summary>
		/// <param name="Acceleration">The current average acceleration of the train</param>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		public override void Update(double Acceleration, double TimeElapsed)
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
				CurrentAcceleration += Math.Sign(accelerationDifference) * accelerationQuanta;
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
				CurrentSpeedDifference -= Math.Sign(CurrentSpeedDifference) * speedQuanta;
			}

			if (Ratio > 0.0)
			{
				double threshold = 1.0 / Ratio;
				if (Math.Abs(CurrentSpeedDifference) > threshold)
				{
					Damaged = true;
				}
				else
				{
					Damaged = false;
				}
			}
			else
			{
				Damaged = false;
			}
		}
	}
}
