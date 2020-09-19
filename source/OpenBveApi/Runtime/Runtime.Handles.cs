namespace OpenBveApi.Runtime
{
	/// <summary>Represents the handles of the cab.</summary>
	public class Handles
	{
		// --- members ---
		/// <summary>The reverser position.</summary>
		private int MyReverser;

		/// <summary>The power notch.</summary>
		private int MyPowerNotch;

		/// <summary>The brake notch.</summary>
		private int MyBrakeNotch;

		/// <summary>The loco brake notch.</summary>
		private int MyLocoBrakeNotch;

		/// <summary>Whether the const speed system is enabled.</summary>
		private bool MyConstSpeed;

		// --- properties ---
		/// <summary>Gets or sets the reverser position.</summary>
		public int Reverser
		{
			get
			{
				return this.MyReverser;
			}
			set
			{
				this.MyReverser = value;
			}
		}

		/// <summary>Gets or sets the power notch.</summary>
		public int PowerNotch
		{
			get
			{
				return this.MyPowerNotch;
			}
			set
			{
				this.MyPowerNotch = value;
			}
		}

		/// <summary>Gets or sets the brake notch.</summary>
		public int BrakeNotch
		{
			get
			{
				return this.MyBrakeNotch;
			}
			set
			{
				this.MyBrakeNotch = value;
			}
		}

		/// <summary>Gets or sets the brake notch.</summary>
		public int LocoBrakeNotch
		{
			get
			{
				return this.MyLocoBrakeNotch;
			}
			set
			{
				this.MyLocoBrakeNotch = value;
			}
		}

		/// <summary>Gets or sets whether the const speed system is enabled.</summary>
		public bool ConstSpeed
		{
			get
			{
				return this.MyConstSpeed;
			}
			set
			{
				this.MyConstSpeed = value;
			}
		}

		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="reverser">The current reverser position.</param>
		/// <param name="powerNotch">The current power notch.</param>
		/// <param name="brakeNotch">The current brake notch.</param>
		/// <param name="constSpeed">Whether the const speed system is enabled.</param>
		public Handles(int reverser, int powerNotch, int brakeNotch, bool constSpeed)
		{
			this.MyReverser = reverser;
			this.MyPowerNotch = powerNotch;
			this.MyBrakeNotch = brakeNotch;
			this.MyConstSpeed = constSpeed;
		}

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="reverser">The current reverser position.</param>
		/// <param name="powerNotch">The current power notch.</param>
		/// <param name="brakeNotch">The current brake notch.</param>
		/// /// <param name="locoBrakeNotch">The current loco brake notch.</param>
		/// <param name="constSpeed">Whether the const speed system is enabled.</param>
		public Handles(int reverser, int powerNotch, int brakeNotch, int locoBrakeNotch, bool constSpeed)
		{
			this.MyReverser = reverser;
			this.MyPowerNotch = powerNotch;
			this.MyBrakeNotch = brakeNotch;
			this.MyLocoBrakeNotch = locoBrakeNotch;
			this.MyConstSpeed = constSpeed;
		}
	}
}
