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
		/// <param name="Acceleration">The current acceleration output of the train</param>
		/// <param name="Enabled">Whether the constant speed device is enabled (As this refers to the whole train)</param>
		/// <param name="ReverserPosition">The current position of the reverser handle</param>
		public void Update(ref double Acceleration, bool Enabled, ReverserPosition ReverserPosition)
		{
			if (!Enabled)
			{
				this.CurrentAccelerationOutput = Acceleration;
				return;
			}
			if (TrainManagerBase.currentHost.InGameTime >= this.NextUpdateTime)
			{
				this.NextUpdateTime = TrainManagerBase.currentHost.InGameTime + UpdateInterval;
				this.CurrentAccelerationOutput -= 0.8 * this.Car.Specs.Acceleration * (double)ReverserPosition;
				if (this.CurrentAccelerationOutput < 0.0)
				{
					this.CurrentAccelerationOutput = 0.0;
				}
			}
			if (Acceleration > CurrentAccelerationOutput)
			{
				Acceleration = CurrentAccelerationOutput;
			}

			if (Acceleration < 0.0)
			{
				Acceleration = 0.0;
			}
		}
	}
}
