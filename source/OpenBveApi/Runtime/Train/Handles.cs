using System.Runtime.Serialization;

namespace OpenBveApi.Runtime
{
	/// <summary>Represents the handles of the cab.</summary>
	[DataContract]
	public class Handles
	{
		/// <summary>The reverser position.</summary>
		[DataMember]
		private int MyReverser;

		/// <summary>The power notch.</summary>
		[DataMember]
		private int MyPowerNotch;

		/// <summary>The brake notch.</summary>
		[DataMember]
		private int MyBrakeNotch;

		/// <summary>The loco brake notch.</summary>
		[DataMember]
		private int MyLocoBrakeNotch;

		/// <summary>Whether the const speed system is enabled.</summary>
		[DataMember]
		private bool MyConstSpeed;

		/// <summary>Whether the hold brake system is enabled.</summary>
		[DataMember]
		private bool MyHoldBrake;

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

		/// <summary>Gets or sets whether the hold brake system is enabled.</summary>
		public bool HoldBrake
		{
			get
			{
				return this.MyHoldBrake;
			}
			set
			{
				this.MyHoldBrake = value;
			}
		}

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

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="reverser">The current reverser position.</param>
		/// <param name="powerNotch">The current power notch.</param>
		/// <param name="brakeNotch">The current brake notch.</param>
		/// /// <param name="locoBrakeNotch">The current loco brake notch.</param>
		/// <param name="constSpeed">Whether the const speed system is enabled.</param>
		/// <param name="holdBrake">Whether the hold brake system is enabled</param>
		public Handles(int reverser, int powerNotch, int brakeNotch, int locoBrakeNotch, bool constSpeed, bool holdBrake)
		{
			this.MyReverser = reverser;
			this.MyPowerNotch = powerNotch;
			this.MyBrakeNotch = brakeNotch;
			this.MyLocoBrakeNotch = locoBrakeNotch;
			this.MyConstSpeed = constSpeed;
			this.MyHoldBrake = holdBrake;
		}
	}
}
