namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		internal class CarConstSpeed
		{
			/// <summary>The current acceleration output this device is providing</summary>
			private double CurrentAccelerationOutput;
			/// <summary>The next update time</summary>
			private double NextUpdateTime;
			/// <summary>The update interval (Constant 0.5 for BVE2 / BVE4 trains)</summary>
			private const double UpdateInterval = 0.5;
			/// <summary>Holds a reference to the base car</summary>
			private readonly Car Car;

			internal CarConstSpeed(Car car)
			{
				this.Car = car;
			}

			/// <summary>Called once a frame to update the constant speed device</summary>
			/// <param name="Acceleration">The current acceleration output of the train</param>
			/// <param name="Enabled">Whether the constant speed device is enabled (As this refers to the whole train)</param>
			/// <param name="ReverserPosition">The current position of the reverser handle</param>
			internal void Update(ref double Acceleration, bool Enabled, int ReverserPosition)
			{
				if (!Enabled)
				{
					this.CurrentAccelerationOutput = Acceleration;
					return;
				}
				if (Game.SecondsSinceMidnight < this.NextUpdateTime)
				{
					return;
				}
				this.NextUpdateTime = Game.SecondsSinceMidnight + UpdateInterval;
				this.CurrentAccelerationOutput -= 0.8 * this.Car.Specs.CurrentAcceleration * (double)ReverserPosition;
				if (this.CurrentAccelerationOutput < 0.0)
				{
					this.CurrentAccelerationOutput = 0.0;
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
}
