using System;

namespace TrainManager.Car
{
	public class CarHoldBrake
	{
		/// <summary>The current deceleration output of this device</summary>
		private double CurrentDecelerationOutput;
		/// <summary>The next update time</summary>
		private double NextUpdateTime;
		/// <summary>The update interval</summary>
		/// <remarks>Constant 0.5 for BVE2 / BVE4 trains</remarks>
		private const double UpdateInterval = 0.5;
		/// <summary>Holds a reference to the base car</summary>
		private readonly CarBase Car;

		public CarHoldBrake(CarBase car)
		{
			Car = car;
		}

		/// <summary>Called once a frame to update the Car Hold Brake</summary>
		/// <param name="Deceleration">The current motor deceleration output of the train</param>
		/// <param name="Enabled">Whether the Car Hold Brake is enabled (As this refers to the whole train)</param>
		public void Update(ref double Deceleration, bool Enabled)
		{
			if (!Enabled || Deceleration != 0.0)
			{
				CurrentDecelerationOutput = 0.0;
				return;
			}
			if (TrainManagerBase.currentHost.InGameTime >= NextUpdateTime)
			{
				NextUpdateTime = TrainManagerBase.currentHost.InGameTime + UpdateInterval;
				CurrentDecelerationOutput += 0.8 * Car.Specs.Acceleration * Math.Sign(Car.Specs.PerceivedSpeed);
				if (CurrentDecelerationOutput < 0.0)
				{
					CurrentDecelerationOutput = 0.0;
				}

				double a = Car.CarBrake.motorDeceleration;
				if (CurrentDecelerationOutput > a)
				{
					CurrentDecelerationOutput = a;
				}
			}

			Deceleration = CurrentDecelerationOutput;
		}
	}
}
