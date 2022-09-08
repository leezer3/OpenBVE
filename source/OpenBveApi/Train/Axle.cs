using OpenBveApi.Routes;

namespace OpenBveApi.Trains
{
	/// <summary>An axle of a car or bogie</summary>
	public class Axle
	{
		/// <summary>The track follower</summary>
		public TrackFollower Follower;
		/// <summary>Whether this axle is currently experiencing wheelslip</summary>
		public bool CurrentWheelSlip;
		/// <summary>The relative position of the axle</summary>
		public double Position;
		/// <summary>The point sounds array</summary>
		public object[] PointSounds;
		/// <summary>The current flange sound index at this axle's location</summary>
		public int FlangeIndex;
		/// <summary>The current run sound index at this axle's location</summary>
		public int RunIndex;
		/// <summary>Stores whether the point sound has been triggered</summary>
		public bool PointSoundTriggered;
		/// <summary>A reference to the base car</summary>
		private readonly AbstractCar baseCar;
		/// <summary>The wheel radius</summary>
		public double WheelRadius;

		private readonly double coefficientOfFriction;

		private readonly double coefficientOfRollingResistance;

		private readonly double aerodynamicDragCoefficient;

		/// <summary>Creates a new axle</summary>
		public Axle(Hosts.HostInterface currentHost, AbstractTrain Train, AbstractCar Car, double CoefficientOfFriction = 0.35, double CoefficentOfRollingResistance = 0.0025, double AerodynamicDragCoefficient = 1.1)
		{
			Follower = new TrackFollower(currentHost, Train, Car);
			baseCar = Car;
			coefficientOfFriction = CoefficientOfFriction;
			coefficientOfRollingResistance = CoefficentOfRollingResistance;
			aerodynamicDragCoefficient = AerodynamicDragCoefficient;
			PointSounds = new object[] { };
		}

		/// <summary>Gets the resistance value for this axle in the current direction</summary>
		public double GetResistance(double Speed, double FrontalArea, double AirDensity, double AccelerationDueToGravity)
		{
			double f = FrontalArea * aerodynamicDragCoefficient * AirDensity / (2.0 * baseCar.CurrentMass);
			double a = AccelerationDueToGravity * coefficientOfRollingResistance + f * Speed * Speed;
			return a;
		}

		/// <summary>Gets the critical value at which wheelslip occurs from motor drive</summary>
		public double CriticalWheelSlipAccelerationForElectricMotor(double AccelerationDueToGravity)
		{
			double NormalForceAcceleration = Follower.WorldUp.Y * AccelerationDueToGravity;
			// TODO: Implement formula that depends on speed here.
			return coefficientOfFriction * Follower.AdhesionMultiplier * NormalForceAcceleration;
		}

		/// <summary>Gets the critical value at which wheelslip occurs from brake application</summary>
		public double CriticalWheelSlipAccelerationForFrictionBrake(double AccelerationDueToGravity)
		{
			double NormalForceAcceleration = Follower.WorldUp.Y * AccelerationDueToGravity;
			// TODO: Implement formula that depends on speed here.
			return coefficientOfFriction * Follower.AdhesionMultiplier * NormalForceAcceleration;
		}
	}
}
