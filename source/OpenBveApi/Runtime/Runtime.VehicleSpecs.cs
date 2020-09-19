namespace OpenBveApi.Runtime
{
	/// <summary>Represents the specification of the train.</summary>
	public class VehicleSpecs
	{
		// --- members ---
		/// <summary>The number of power notches the train has.</summary>
		private readonly int MyPowerNotches;

		/// <summary>The type of brake the train uses.</summary>
		private readonly BrakeTypes MyBrakeType;

		/// <summary>Whether the train has a hold brake.</summary>
		private readonly bool MyHasHoldBrake;

		/// <summary>Whether the train has a hold brake.</summary>
		private readonly bool MyHasLocoBrake;

		/// <summary>The number of brake notches the train has, including the hold brake, but excluding the emergency brake.</summary>
		private readonly int MyBrakeNotches;

		/// <summary>The number of cars the train has.</summary>
		private readonly int MyCars;

		// --- properties ---
		/// <summary>Gets the number of power notches the train has.</summary>
		public int PowerNotches
		{
			get
			{
				return this.MyPowerNotches;
			}
		}

		/// <summary>Gets the type of brake the train uses.</summary>
		public BrakeTypes BrakeType
		{
			get
			{
				return this.MyBrakeType;
			}
		}

		/// <summary>Gets the number of brake notches the train has, including the hold brake, but excluding the emergency brake.</summary>
		public int BrakeNotches
		{
			get
			{
				return this.MyBrakeNotches;
			}
		}

		/// <summary>Gets whether the train has a hold brake.</summary>
		public bool HasHoldBrake
		{
			get
			{
				return this.MyHasHoldBrake;
			}
		}

		/// <summary>Gets whether the train has a hold brake.</summary>
		public bool HasLocoBrake
		{
			get
			{
				return this.MyHasLocoBrake;
			}
		}

		/// <summary>Gets the index of the brake notch that corresponds to B1 or LAP.</summary>
		/// <remarks>For trains without a hold brake, this returns 1. For trains with a hold brake, this returns 2.</remarks>
		public int AtsNotch
		{
			get
			{
				if (this.MyHasHoldBrake)
				{
					return 2;
				}

				return 1;
			}
		}

		/// <summary>Gets the index of the brake notch that corresponds to 70% of the available brake notches.</summary>
		public int B67Notch
		{
			get
			{
				return (int) System.Math.Round(0.7 * this.MyBrakeNotches);
			}
		}

		/// <summary>Gets the number of cars the train has.</summary>
		public int Cars
		{
			get
			{
				return this.MyCars;
			}
		}

		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="powerNotches">The number of power notches the train has.</param>
		/// <param name="brakeType">The type of brake the train uses.</param>
		/// <param name="brakeNotches">The number of brake notches the train has, including the hold brake, but excluding the emergency brake.</param>
		/// <param name="hasHoldBrake">Whether the train has a hold brake.</param>
		/// <param name="cars">The number of cars the train has.</param>
		public VehicleSpecs(int powerNotches, BrakeTypes brakeType, int brakeNotches, bool hasHoldBrake, int cars)
		{
			this.MyPowerNotches = powerNotches;
			this.MyBrakeType = brakeType;
			this.MyBrakeNotches = brakeNotches;
			this.MyHasHoldBrake = hasHoldBrake;
			this.MyHasLocoBrake = false;
			this.MyCars = cars;
		}

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="powerNotches">The number of power notches the train has.</param>
		/// <param name="brakeType">The type of brake the train uses.</param>
		/// <param name="brakeNotches">The number of brake notches the train has, including the hold brake, but excluding the emergency brake.</param>
		/// <param name="hasHoldBrake">Whether the train has a hold brake.</param>
		/// <param name="hasLocoBrake">Whether the train has a loco brake.</param>
		/// <param name="cars">The number of cars the train has.</param>
		public VehicleSpecs(int powerNotches, BrakeTypes brakeType, int brakeNotches, bool hasHoldBrake, bool hasLocoBrake, int cars)
		{
			this.MyPowerNotches = powerNotches;
			this.MyBrakeType = brakeType;
			this.MyBrakeNotches = brakeNotches;
			this.MyHasHoldBrake = hasHoldBrake;
			this.MyHasLocoBrake = hasLocoBrake;
			this.MyCars = cars;
		}
	}
}
