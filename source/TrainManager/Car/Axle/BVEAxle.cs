using System;
using OpenBveApi.Hosts;
using OpenBveApi.Trains;

namespace TrainManager.Car
{
	/// <summary>Derived class for axles on BVE cars</summary>
	internal class BVEAxle : AbstractAxle
	{
		private readonly double coefficientOfFriction;

		private readonly double coefficientOfRollingResistance;

		private readonly double aerodynamicDragCoefficient;

		public BVEAxle(HostInterface currentHost, AbstractTrain Train, AbstractCar Car, double CoefficientOfFriction = 0.35, double CoefficentOfRollingResistance = 0.0025, double AerodynamicDragCoefficient = 1.1) : base(currentHost, Train, Car)
		{
			coefficientOfFriction = CoefficientOfFriction;
			coefficientOfRollingResistance = CoefficentOfRollingResistance;
			aerodynamicDragCoefficient = AerodynamicDragCoefficient;
		}

		public override double GetResistance(double Speed, double FrontalArea, double AirDensity, double AccelerationDueToGravity)
		{
			double f = FrontalArea * aerodynamicDragCoefficient * AirDensity / (2.0 * Math.Max(1.0, baseCar.CurrentMass));
			double a = AccelerationDueToGravity * coefficientOfRollingResistance + f * Speed * Speed;
			return a;
		}

		public override double CriticalWheelSlipAccelerationForElectricMotor(double AccelerationDueToGravity)
		{
			double NormalForceAcceleration = Follower.WorldUp.Y * AccelerationDueToGravity;
			return coefficientOfFriction * Follower.AdhesionMultiplier * NormalForceAcceleration;
		}

		public override double CriticalWheelSlipAccelerationForFrictionBrake(double AccelerationDueToGravity)
		{
			double NormalForceAcceleration = Follower.WorldUp.Y * AccelerationDueToGravity;
			return coefficientOfFriction * Follower.AdhesionMultiplier * NormalForceAcceleration;
		}
	}
}
