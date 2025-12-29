using System;
using TrainManager.SafetySystems;

namespace TrainManager.Car
{
	public class CarConstSpeed : AbstractSafetySystem
	{
		/// <summary>The current acceleration output this device is providing</summary>
		private double CurrentAccelerationOutput;
		/// <summary>The next update time</summary>
		private double NextUpdateTime;
		/// <summary>The update interval (Constant 0.5 for BVE2 / BVE4 trains)</summary>
		private const double UpdateInterval = 0.5;

		public CarConstSpeed(CarBase car) : base(car)
		{
		}

		/// <summary>Called once a frame to update the constant speed device</summary>
		/// <param name="Acceleration">The current acceleration output of the train</param>
		public void Update(ref double Acceleration)
		{
			if (!baseCar.baseTrain.Specs.CurrentConstSpeed)
			{
				CurrentAccelerationOutput = Acceleration;
				return;
			}

			if (Math.Abs(TrainManagerBase.currentHost.InGameTime - NextUpdateTime) > 10)
			{
				// If next update time is not within 10s, reset (jumping train etc.)
				NextUpdateTime = TrainManagerBase.currentHost.InGameTime + 0.5;
			}

			if (TrainManagerBase.currentHost.InGameTime >= NextUpdateTime)
			{
				NextUpdateTime = TrainManagerBase.currentHost.InGameTime + UpdateInterval;
				CurrentAccelerationOutput -= 0.8 * baseCar.Specs.Acceleration * (double)baseCar.baseTrain.Handles.Reverser.Actual;
				if (CurrentAccelerationOutput < 0.0)
				{
					CurrentAccelerationOutput = 0.0;
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
