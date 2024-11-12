using TrainManager.Handles;

namespace TrainManager.Car
{
	public class CarConstSpeed
	{
		/// <summary>The current acceleration output this device is providing</summary>
		private double CurrentAccelerationOutput;
		/// <summary>The next update time</summary>
		private double NextUpdateTime;
		/// <summary>The update interval (Constant 0.5 for BVE2 / BVE4 trains)</summary>
		private const double UpdateInterval = 0.5;
		/// <summary>Holds a reference to the base car</summary>
		private readonly CarBase Car;

		public CarConstSpeed(CarBase car)
		{
			this.Car = car;
		}

		/// <summary>Called once a frame to update the constant speed device</summary>
		/// <param name="acceleration">The current acceleration output of the train</param>
		/// <param name="enabled">Whether the constant speed device is enabled (As this refers to the whole train)</param>
		/// <param name="reverserPosition">The current position of the reverser handle</param>
		public void Update(ref double acceleration, bool enabled, ReverserPosition reverserPosition)
		{
			if (!enabled)
			{
				this.CurrentAccelerationOutput = acceleration;
				return;
			}
			if (TrainManagerBase.CurrentHost.InGameTime >= this.NextUpdateTime)
			{
				this.NextUpdateTime = TrainManagerBase.CurrentHost.InGameTime + UpdateInterval;
				this.CurrentAccelerationOutput -= 0.8 * this.Car.Specs.Acceleration * (double)reverserPosition;
				if (this.CurrentAccelerationOutput < 0.0)
				{
					this.CurrentAccelerationOutput = 0.0;
				}
			}
			if (acceleration > CurrentAccelerationOutput)
			{
				acceleration = CurrentAccelerationOutput;
			}

			if (acceleration < 0.0)
			{
				acceleration = 0.0;
			}
		}
	}
}
