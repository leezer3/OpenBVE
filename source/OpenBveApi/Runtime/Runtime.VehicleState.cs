namespace OpenBveApi.Runtime
{
	/// <summary>Represents the current state of the train.</summary>
	public class VehicleState
	{
		// --- members ---
		/// <summary>The location of the front of the train, in meters.</summary>
		private readonly double MyLocation;

		/// <summary>The speed of the train.</summary>
		private readonly Speed MySpeed;

		/// <summary>The pressure in the brake cylinder, in pascal.</summary>
		private readonly double MyBcPressure;

		/// <summary>The pressure in the main reservoir, in pascal.</summary>
		private readonly double MyMrPressure;

		/// <summary>The pressure in the emergency reservoir, in pascal.</summary>
		private readonly double MyErPressure;

		/// <summary>The pressure in the brake pipe, in pascal.</summary>
		private readonly double MyBpPressure;

		/// <summary>The pressure in the straight air pipe, in pascal.</summary>
		private readonly double MySapPressure;

		private readonly double MyRadius;
		private readonly double MyCant;
		private readonly double MyPitch;

		// --- properties ---
		/// <summary>Gets the location of the front of the train, in meters.</summary>
		public double Location
		{
			get
			{
				return this.MyLocation;
			}
		}

		/// <summary>Gets the speed of the train.</summary>
		public Speed Speed
		{
			get
			{
				return this.MySpeed;
			}
		}

		/// <summary>Gets the pressure in the brake cylinder, in pascal.</summary>
		public double BcPressure
		{
			get
			{
				return this.MyBcPressure;
			}
		}

		/// <summary>Gets the pressure in the main reservoir, in pascal.</summary>
		public double MrPressure
		{
			get
			{
				return this.MyMrPressure;
			}
		}

		/// <summary>Gets the pressure in the emergency reservoir, in pascal.</summary>
		public double ErPressure
		{
			get
			{
				return this.MyErPressure;
			}
		}

		/// <summary>Gets the pressure in the brake pipe, in pascal.</summary>
		public double BpPressure
		{
			get
			{
				return this.MyBpPressure;
			}
		}

		/// <summary>Gets the pressure in the straight air pipe, in pascal.</summary>
		public double SapPressure
		{
			get
			{
				return this.MySapPressure;
			}
		}

		/// <summary>Gets the curve radius at the front axle of the driver's car in m.</summary>
		public double Radius
		{
			get
			{
				return this.MyRadius;
			}
		}

		/// <summary>Gets the curve cant at the front axle of the driver's car in mm.</summary>
		public double Cant
		{
			get
			{
				return this.MyCant;
			}
		}

		/// <summary>Gets the track pitch value at the front axle of the driver's car.</summary>
		public double Pitch
		{
			get
			{
				return this.MyPitch;
			}
		}

		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
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
		/// Three paramaters added at the far end
		public VehicleState(double location, Speed speed, double bcPressure, double mrPressure, double erPressure, double bpPressure, double sapPressure, double Radius, double Cant, double Pitch)
		{
			this.MyRadius = Radius;
			this.MyCant = Cant;
			this.MyPitch = Pitch;
			this.MyLocation = location;
			this.MySpeed = speed;
			this.MyBcPressure = bcPressure;
			this.MyMrPressure = mrPressure;
			this.MyErPressure = erPressure;
			this.MyBpPressure = bpPressure;
			this.MySapPressure = sapPressure;
		}

		//This provides the overload for plugins built against versions of the OpenBVE API below 1.4.4.0
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="location">The location of the front of the train, in meters.</param>
		/// <param name="speed">The speed of the train.</param>
		/// <param name="bcPressure">The pressure in the brake cylinder, in pascal.</param>
		/// <param name="mrPressure">The pressure in the main reservoir, in pascal.</param>
		/// <param name="erPressure">The pressure in the emergency reservoir, in pascal.</param>
		/// <param name="bpPressure">The pressure in the brake pipe, in pascal.</param>
		/// <param name="sapPressure">The pressure in the straight air pipe, in pascal.</param>
		/// Three paramaters added at the far end
		public VehicleState(double location, Speed speed, double bcPressure, double mrPressure, double erPressure, double bpPressure, double sapPressure)
		{
			this.MyLocation = location;
			this.MySpeed = speed;
			this.MyBcPressure = bcPressure;
			this.MyMrPressure = mrPressure;
			this.MyErPressure = erPressure;
			this.MyBpPressure = bpPressure;
			this.MySapPressure = sapPressure;
		}
	}
}
