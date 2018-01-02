using System;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		internal class CarHoldBrake
		{
			/// <summary>The current deceleration output of this device</summary>
			private double CurrentDecelerationOutput;
			/// <summary>The next update time</summary>
			private double NextUpdateTime;
			/// <summary>The update interval (Constant 0.5 for BVE2 / BVE4 trains)</summary>
			private const double UpdateInterval = 0.5;
			/// <summary>Holds a reference to the base car</summary>
			private readonly Car Car;

			internal CarHoldBrake(Car car)
			{
				this.Car = car;
			}

			/// <summary>Called once a frame to update the Car Hold Brake</summary>
			/// <param name="Deceleration">The current motor deceleration output of the train</param>
			/// <param name="Enabled">Whether the Car Hold Brake is enabled (As this refers to the whole train)</param>
			internal void Update(ref double Deceleration, bool Enabled)
			{
				if (Enabled & Deceleration == 0.0)
				{
					if (Game.SecondsSinceMidnight >= NextUpdateTime)
					{
						NextUpdateTime = Game.SecondsSinceMidnight + UpdateInterval;
						this.CurrentDecelerationOutput += 0.8 * Car.Specs.CurrentAcceleration * (double)Math.Sign(Car.Specs.CurrentPerceivedSpeed);
						if (this.CurrentDecelerationOutput < 0.0)
						{
							this.CurrentDecelerationOutput = 0.0;
						}
						double a = Car.Specs.MotorDeceleration;
						if (this.CurrentDecelerationOutput > a)
						{
							this.CurrentDecelerationOutput = a;
						}
					}
					Deceleration = this.CurrentDecelerationOutput;
				}
				else
				{
					this.CurrentDecelerationOutput = 0.0;
				}
			}
		}
	}
}
