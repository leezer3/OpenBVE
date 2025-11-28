using OpenBveApi.Routes;

namespace OpenBveApi.Trains
{
	/// <summary>An axle of a car or bogie</summary>
	public abstract class AbstractAxle
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
		protected readonly AbstractCar baseCar;

		

		/// <summary>Creates a new axle</summary>
		public AbstractAxle(Hosts.HostInterface currentHost, AbstractTrain Train, AbstractCar Car)
		{
			Follower = new TrackFollower(currentHost, Train, Car);
			baseCar = Car;
			PointSounds = new object[] { };
		}

		/// <summary>Gets the resistance value for this axle in the current direction</summary>
		public abstract double GetResistance(double Speed, double FrontalArea, double AirDensity, double AccelerationDueToGravity);

		/// <summary>Gets the critical value at which wheelslip occurs from motor drive</summary>
		public abstract double CriticalWheelSlipAccelerationForElectricMotor(double AccelerationDueToGravity);


		/// <summary>Gets the critical value at which wheelslip occurs from brake application</summary>
		public abstract double CriticalWheelSlipAccelerationForFrictionBrake(double AccelerationDueToGravity);
	}
}
