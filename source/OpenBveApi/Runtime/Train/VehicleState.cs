using System.Runtime.Serialization;
using OpenBveApi.Routes;

namespace OpenBveApi.Runtime
{
	/// <summary>Represents the current state of the train.</summary>
	[DataContract]
	public class VehicleState
	{
		/// <summary>The location of the front of the train, in meters.</summary>
		[DataMember]
		private readonly double MyLocation;

		/// <summary>The speed of the train.</summary>
		[DataMember]
		private readonly Speed MySpeed;

		/// <summary>The pressure in the brake cylinder, in pascal.</summary>
		[DataMember]
		private readonly double MyBcPressure;

		/// <summary>The pressure in the main reservoir, in pascal.</summary>
		[DataMember]
		private readonly double MyMrPressure;

		/// <summary>The pressure in the emergency reservoir, in pascal.</summary>
		[DataMember]
		private readonly double MyErPressure;

		/// <summary>The pressure in the brake pipe, in pascal.</summary>
		[DataMember]
		private readonly double MyBpPressure;

		/// <summary>The pressure in the straight air pipe, in pascal.</summary>
		[DataMember]
		private readonly double MySapPressure;

		[DataMember]
		private readonly double MyRadius;
		[DataMember]
		private readonly double MyCant;
		[DataMember]
		private readonly double MyPitch;
		[DataMember]
		private readonly int MyRainIntensity;
		[DataMember]
		private readonly double MyAdhesion;
		[DataMember]
		private readonly bool MyWheelSlip;

		/// <summary>Gets the location of the front of the train, in meters.</summary>
		public double Location => MyLocation;

		/// <summary>Gets the speed of the train.</summary>
		public Speed Speed => MySpeed;

		/// <summary>Gets the pressure in the brake cylinder, in pascal.</summary>
		public double BcPressure => MyBcPressure;

		/// <summary>Gets the pressure in the main reservoir, in pascal.</summary>
		public double MrPressure => MyMrPressure;

		/// <summary>Gets the pressure in the emergency reservoir, in pascal.</summary>
		public double ErPressure => MyErPressure;

		/// <summary>Gets the pressure in the brake pipe, in pascal.</summary>
		public double BpPressure => MyBpPressure;

		/// <summary>Gets the pressure in the straight air pipe, in pascal.</summary>
		public double SapPressure => MySapPressure;

		/// <summary>Gets the curve radius at the front axle of the driver's car in m.</summary>
		public double Radius => MyRadius;

		/// <summary>Gets the curve cant at the front axle of the driver's car in mm.</summary>
		public double Cant => MyCant;

		/// <summary>Gets the track pitch value at the front axle of the driver's car.</summary>
		public double Pitch => MyPitch;

		/// <summary>Gets the track pitch value at the front axle of the driver's car.</summary>
		public double RainIntensity => MyRainIntensity;

		/// <summary>Gets the track pitch value at the front axle of the driver's car.</summary>
		public double Adhesion => MyAdhesion;

		/// <summary>Gets the track pitch value at the front axle of the driver's car.</summary>
		public bool WheelSlip => MyWheelSlip;

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="location">The location of the front of the train, in meters.</param>
		/// <param name="speed">The speed of the train.</param>
		/// <param name="bcPressure">The pressure in the brake cylinder, in pascal.</param>
		/// <param name="mrPressure">The pressure in the main reservoir, in pascal.</param>
		/// <param name="erPressure">The pressure in the emergency reservoir, in pascal.</param>
		/// <param name="bpPressure">The pressure in the brake pipe, in pascal.</param>
		/// <param name="sapPressure">The pressure in the straight air pipe, in pascal.</param>
		/// <param name="wheelSlip">Whether wheelslip is occuring</param>
		/// <param name="Follower"></param>
		public VehicleState(double location, Speed speed, double bcPressure, double mrPressure, double erPressure, double bpPressure, double sapPressure, bool wheelSlip, TrackFollower Follower)
		{
			MyRadius = Radius;
			MyCant = Cant;
			MyPitch = Pitch;
			MyLocation = location;
			MySpeed = speed;
			MyBcPressure = bcPressure;
			MyMrPressure = mrPressure;
			MyErPressure = erPressure;
			MyBpPressure = bpPressure;
			MySapPressure = sapPressure;
			MyRadius = Follower.CurveRadius;
			MyCant = Follower.CurveCant;
			MyPitch = Follower.Pitch;
			MyRainIntensity = Follower.RainIntensity;
			MyAdhesion = Follower.AdhesionMultiplier;
			MyWheelSlip = wheelSlip;
		}

		/// <summary>Provides the overload for plugins built against versions of the OpenBVE API below 1.8.4.2.</summary>
		/// <param name="location">The location of the front of the train, in meters.</param>
		/// <param name="speed">The speed of the train.</param>
		/// <param name="bcPressure">The pressure in the brake cylinder, in pascal.</param>
		/// <param name="mrPressure">The pressure in the main reservoir, in pascal.</param>
		/// <param name="erPressure">The pressure in the emergency reservoir, in pascal.</param>
		/// <param name="bpPressure">The pressure in the brake pipe, in pascal.</param>
		/// <param name="sapPressure">The pressure in the straight air pipe, in pascal.</param>
		/// <param name="Follower"></param>
		public VehicleState(double location, Speed speed, double bcPressure, double mrPressure, double erPressure, double bpPressure, double sapPressure, TrackFollower Follower)
		:this(location, speed, bcPressure, mrPressure, erPressure, bpPressure, sapPressure, false, Follower)
		{

		}

		
		/// <summary>Provides the overload for plugins built against versions of the OpenBVE API below 1.7.3.0.</summary>
		/// <param name="location">The location of the front of the train, in meters.</param>
		/// <param name="speed">The speed of the train.</param>
		/// <param name="bcPressure">The pressure in the brake cylinder, in pascal.</param>
		/// <param name="mrPressure">The pressure in the main reservoir, in pascal.</param>
		/// <param name="erPressure">The pressure in the emergency reservoir, in pascal.</param>
		/// <param name="bpPressure">The pressure in the brake pipe, in pascal.</param>
		/// <param name="sapPressure">The pressure in the straight air pipe, in pascal.</param>
		/// <param name="Radius">The curve radius at the front of the train, in meters.</param>
		/// <param name="Cant">The cant value for this curve radius.</param>
		/// <param name="Pitch">The pitch value at the front of the train.</param>
		public VehicleState(double location, Speed speed, double bcPressure, double mrPressure, double erPressure, double bpPressure, double sapPressure, double Radius, double Cant, double Pitch)
		{
			MyRadius = Radius;
			MyCant = Cant;
			MyPitch = Pitch;
			MyLocation = location;
			MySpeed = speed;
			MyBcPressure = bcPressure;
			MyMrPressure = mrPressure;
			MyErPressure = erPressure;
			MyBpPressure = bpPressure;
			MySapPressure = sapPressure;
			MyRadius = Radius;
			MyCant = Cant;
			MyPitch = Pitch;
			MyWheelSlip = false;
		}

		/// <summary>Provides the overload for plugins built against versions of the OpenBVE API below 1.4.4.0.</summary>
		/// <param name="location">The location of the front of the train, in meters.</param>
		/// <param name="speed">The speed of the train.</param>
		/// <param name="bcPressure">The pressure in the brake cylinder, in pascal.</param>
		/// <param name="mrPressure">The pressure in the main reservoir, in pascal.</param>
		/// <param name="erPressure">The pressure in the emergency reservoir, in pascal.</param>
		/// <param name="bpPressure">The pressure in the brake pipe, in pascal.</param>
		/// <param name="sapPressure">The pressure in the straight air pipe, in pascal.</param>
		public VehicleState(double location, Speed speed, double bcPressure, double mrPressure, double erPressure, double bpPressure, double sapPressure) 
			: this(location, speed, bcPressure, mrPressure, erPressure, bpPressure, sapPressure, 0.0, 0.0, 0.0)
		{

		}

	}
}
