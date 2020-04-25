using System;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		internal class CarReAdhesionDevice
		{
			/// <summary>The time between updates in seconds</summary>
			internal double UpdateInterval;
			/// <summary>The maximum acceleration output available</summary>
			internal double MaximumAccelerationOutput;
			/// <summary>The speed at which this device applies</summary>
			internal double ApplicationFactor;
			/// <summary>The time in seconds with no wheelslip after an application before a release is possible</summary>
			internal double ReleaseInterval;
			/// <summary>The speed at which this device releases</summary>
			internal double ReleaseFactor;
			/// <summary>When the next update will be performed</summary>
			private double NextUpdateTime;
			/// <summary>The amount of time with NO wheelslip occuring</summary>
			private double TimeStable;
			/// <summary>Holds a reference to the base car</summary>
			private readonly Car Car;
			/// <summary>The type of device</summary>
			internal readonly ReadhesionDeviceType DeviceType;

			internal CarReAdhesionDevice(Car car, ReadhesionDeviceType type)
			{
				this.Car = car;
				this.DeviceType = type;
				this.MaximumAccelerationOutput = Double.PositiveInfinity;
				this.ApplicationFactor = 0.0;
				if (Car.Specs.IsMotorCar)
				{
					switch (type) {
						case TrainManager.ReadhesionDeviceType.TypeA:
							UpdateInterval = 1.0;
							ApplicationFactor = 0.0;
							ReleaseInterval = 1.0;
							ReleaseFactor = 8.0;
							break;
						case TrainManager.ReadhesionDeviceType.TypeB:
							UpdateInterval = 0.1;
							ApplicationFactor = 0.9935;
							ReleaseInterval = 4.0;
							ReleaseFactor = 1.125;
							break;
						case TrainManager.ReadhesionDeviceType.TypeC:
							UpdateInterval = 0.1;
							ApplicationFactor = 0.965;
							ReleaseInterval = 2.0;
							ReleaseFactor = 1.5;
							break;
						case TrainManager.ReadhesionDeviceType.TypeD:
							UpdateInterval = 0.05;
							ApplicationFactor = 0.935;
							ReleaseInterval = 0.3;
							ReleaseFactor = 2.0;
							break;
						default:
							UpdateInterval = 1.0;
							ApplicationFactor = 1.0;
							ReleaseInterval = 1.0;
							ReleaseFactor = 99.0;
							break;
					}
				}
			}

			/// <summary>Called once a frame to update the re-adhesion device when powering</summary>
			/// <param name="CurrentAcceleration">The current acceleration output</param>
			internal void Update(double CurrentAcceleration)
			{
				if (Program.CurrentRoute.SecondsSinceMidnight < NextUpdateTime)
				{
					return;
				}
				NextUpdateTime = Program.CurrentRoute.SecondsSinceMidnight + this.UpdateInterval;
				if (Car.FrontAxle.CurrentWheelSlip | Car.RearAxle.CurrentWheelSlip)
				{
					MaximumAccelerationOutput = CurrentAcceleration * this.ApplicationFactor;
					TimeStable = 0.0;
				}
				else
				{
					TimeStable += this.UpdateInterval;
					if (TimeStable >= this.ReleaseInterval)
					{
						TimeStable -= this.ReleaseInterval;
						if (this.ReleaseFactor != 0.0 & MaximumAccelerationOutput <= CurrentAcceleration + 1.0)
						{
							if (MaximumAccelerationOutput < 0.025)
							{
								MaximumAccelerationOutput = 0.025;
							}
							else
							{
								MaximumAccelerationOutput *= this.ReleaseFactor;
							}
						}
						else
						{
							MaximumAccelerationOutput = double.PositiveInfinity;
						}
					}
				}
			}
		}
	}
}
