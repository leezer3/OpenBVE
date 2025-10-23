using OpenBveApi.Hosts;
using OpenBveApi.Trains;

namespace Train.MsTs
{
	/// <summary>Derived class for axles on MSTS cars</summary>
	public class MSTSAxle : AbstractAxle
	{
		/// <summary>The friction properties</summary>
		private readonly Friction FrictionProperties;

		internal MSTSAxle(HostInterface currentHost, AbstractTrain train, AbstractCar car, Friction friction) : base(currentHost, train, car)
		{
			FrictionProperties = friction;
		}

		public override double GetResistance(double Speed, double FrontalArea, double AirDensity, double AccelerationDueToGravity)
		{
			return FrictionProperties.GetResistanceValue(Speed) / baseCar.CurrentMass;
		}

		public override double CriticalWheelSlipAccelerationForElectricMotor(double AccelerationDueToGravity)
		{
			// TODO: This is the BVE formula
			double NormalForceAcceleration = Follower.WorldUp.Y * AccelerationDueToGravity;
			return 0.35 * Follower.AdhesionMultiplier * NormalForceAcceleration;
		}

		public override double CriticalWheelSlipAccelerationForFrictionBrake(double AccelerationDueToGravity)
		{
			// TODO: This is the BVE formula
			double NormalForceAcceleration = Follower.WorldUp.Y * AccelerationDueToGravity;
			return 0.35 * Follower.AdhesionMultiplier * NormalForceAcceleration;
		}
	}
}
