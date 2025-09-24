using OpenBveApi.Hosts;
using OpenBveApi.Trains;

namespace Train.MsTs
{
	/// <summary>Derived class for axles on MSTS cars</summary>
	public class MSTSAxle : AbstractAxle
	{
		/// <summary>The friction properties</summary>
		private readonly Friction FrictionProperties;
		/// <summary>The adhesion properties</summary>
		private readonly Adhesion AdhesionProperties;

		internal MSTSAxle(HostInterface currentHost, AbstractTrain train, AbstractCar car, Friction friction, Adhesion adhesion) : base(currentHost, train, car)
		{
			FrictionProperties = friction;
			AdhesionProperties = adhesion;
		}

		public override double GetResistance(double Speed, double FrontalArea, double AirDensity, double AccelerationDueToGravity)
		{
			return FrictionProperties.GetResistanceValue(Speed) / baseCar.CurrentMass;
		}

		public override double CriticalWheelSlipAccelerationForElectricMotor(double AccelerationDueToGravity)
		{
			return AdhesionProperties.GetWheelslipValue();
		}

		public override double CriticalWheelSlipAccelerationForFrictionBrake(double AccelerationDueToGravity)
		{
			// TODO: This is the BVE formula
			double NormalForceAcceleration = Follower.WorldUp.Y * AccelerationDueToGravity;
			return 0.35 * Follower.AdhesionMultiplier * NormalForceAcceleration;
		}
	}
}
