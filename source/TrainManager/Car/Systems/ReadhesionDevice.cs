using System;
using TrainManager.Car.Systems;

namespace TrainManager.Car
{
	/// <summary>Implements a BVE2 / BVE4 readhesion device</summary>
	/// <remarks>Designed to simulate an electric motor control system reacting to wheelslip</remarks>
	public class BveReAdhesionDevice : AbstractReAdhesionDevice
	{
		/// <summary>The time between updates in seconds</summary>
		public double UpdateInterval;
		/// <summary>The maximum acceleration output available</summary>
		public double MaximumAccelerationOutput;
		/// <summary>The speed at which this device applies</summary>
		public double ApplicationFactor;
		/// <summary>The time in seconds with no wheelslip after an application before a release is possible</summary>
		public double ReleaseInterval;
		/// <summary>The speed at which this device releases</summary>
		public double ReleaseFactor;
		/// <summary>When the next update will be performed</summary>
		private double NextUpdateTime;
		/// <summary>The amount of time with NO wheelslip occuring</summary>
		private double TimeStable;
		/// <summary>The type of device</summary>
		public readonly ReadhesionDeviceType DeviceType;

		public BveReAdhesionDevice(CarBase car, ReadhesionDeviceType type) : base(car)
		{
			this.DeviceType = type;
			this.MaximumAccelerationOutput = double.PositiveInfinity;
			this.ApplicationFactor = 0.0;
			if (Car.TractionModel.ProvidesPower)
			{
				switch (type)
				{
					case ReadhesionDeviceType.TypeA:
						UpdateInterval = 1.0;
						ApplicationFactor = 0.0;
						ReleaseInterval = 1.0;
						ReleaseFactor = 8.0;
						break;
					case ReadhesionDeviceType.TypeB:
						UpdateInterval = 0.1;
						ApplicationFactor = 0.9935;
						ReleaseInterval = 4.0;
						ReleaseFactor = 1.125;
						break;
					case ReadhesionDeviceType.TypeC:
						UpdateInterval = 0.1;
						ApplicationFactor = 0.965;
						ReleaseInterval = 2.0;
						ReleaseFactor = 1.5;
						break;
					case ReadhesionDeviceType.TypeD:
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

		public override void Update(double TimeElapsed)
		{
			if (TrainManagerBase.currentHost.InGameTime < NextUpdateTime || Car.TractionModel.MaximumCurrentAcceleration == -1)
			{
				return;
			}

			NextUpdateTime = TrainManagerBase.currentHost.InGameTime + this.UpdateInterval;
			if (Car.FrontAxle.CurrentWheelSlip | Car.RearAxle.CurrentWheelSlip)
			{
				MaximumAccelerationOutput = Car.TractionModel.MaximumCurrentAcceleration * this.ApplicationFactor;
				TimeStable = 0.0;
			}
			else
			{
				TimeStable += this.UpdateInterval;
				if (TimeStable >= this.ReleaseInterval)
				{
					TimeStable -= this.ReleaseInterval;
					if (this.ReleaseFactor != 0.0 & MaximumAccelerationOutput <= Car.TractionModel.MaximumCurrentAcceleration + 1.0)
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

		/// <summary>Called to reset the readhesion device after a jump</summary>
		public override void Jump()
		{
			NextUpdateTime = 0;
			MaximumAccelerationOutput = double.PositiveInfinity;
		}
	}
}
