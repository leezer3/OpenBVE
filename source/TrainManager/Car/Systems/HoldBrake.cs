using System;

namespace TrainManager.Car
{
	public class CarHoldBrake
	{
		/// <summary>The current deceleration output of this device</summary>
		private double CurrentDecelerationOutput;
		/// <summary>The next update time</summary>
		private double NextUpdateTime;
		/// <summary>The update interval (Constant 0.5 for BVE2 / BVE4 trains)</summary>
		private const double UpdateInterval = 0.5;
		/// <summary>Holds a reference to the base car</summary>
		private readonly CarBase Car;

		public CarHoldBrake(CarBase car)
		{
			this.Car = car;
		}

		/// <summary>Called once a frame to update the Car Hold Brake</summary>
		/// <param name="deceleration">The current motor deceleration output of the train</param>
		/// <param name="enabled">Whether the Car Hold Brake is enabled (As this refers to the whole train)</param>
		public void Update(ref double deceleration, bool enabled)
		{
			if (enabled & deceleration == 0.0)
			{
				if (TrainManagerBase.CurrentHost.InGameTime >= NextUpdateTime)
				{
					NextUpdateTime = TrainManagerBase.CurrentHost.InGameTime + UpdateInterval;
					this.CurrentDecelerationOutput += 0.8 * Car.Specs.Acceleration * Math.Sign(Car.Specs.PerceivedSpeed);
					if (this.CurrentDecelerationOutput < 0.0)
					{
						this.CurrentDecelerationOutput = 0.0;
					}
					double a = Car.CarBrake.MotorDeceleration;
					if (this.CurrentDecelerationOutput > a)
					{
						this.CurrentDecelerationOutput = a;
					}
				}
				deceleration = this.CurrentDecelerationOutput;
			}
			else
			{
				this.CurrentDecelerationOutput = 0.0;
			}
		}
	}
}
