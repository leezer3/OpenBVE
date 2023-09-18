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
			get => MyReverser;
			set => MyReverser = value;
		}

		/// <summary>Gets or sets the power notch.</summary>
		public int PowerNotch
		{
			get => MyPowerNotch;
			set => MyPowerNotch = value;
		}

		/// <summary>Gets or sets the brake notch.</summary>
		public int BrakeNotch
		{
			get => MyBrakeNotch;
			set => MyBrakeNotch = value;
		}

		/// <summary>Gets or sets the brake notch.</summary>
		public int LocoBrakeNotch
		{
			get => MyLocoBrakeNotch;
			set => MyLocoBrakeNotch = value;
		}

		/// <summary>Gets or sets whether the const speed system is enabled.</summary>
		public bool ConstSpeed
		{
			get => MyConstSpeed;
			set => MyConstSpeed = value;
		}

		/// <summary>Gets or sets whether the hold brake system is enabled.</summary>
		public bool HoldBrake
		{
			get => MyHoldBrake;
			set => MyHoldBrake = value;
		}

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="reverser">The current reverser position.</param>
		/// <param name="powerNotch">The current power notch.</param>
		/// <param name="brakeNotch">The current brake notch.</param>
		/// <param name="constSpeed">Whether the const speed system is enabled.</param>
		public Handles(int reverser, int powerNotch, int brakeNotch, bool constSpeed)
		{
			MyReverser = reverser;
			MyPowerNotch = powerNotch;
			MyBrakeNotch = brakeNotch;
			MyConstSpeed = constSpeed;
		}

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="reverser">The current reverser position.</param>
		/// <param name="powerNotch">The current power notch.</param>
		/// <param name="brakeNotch">The current brake notch.</param>
		/// /// <param name="locoBrakeNotch">The current loco brake notch.</param>
		/// <param name="constSpeed">Whether the const speed system is enabled.</param>
		public Handles(int reverser, int powerNotch, int brakeNotch, int locoBrakeNotch, bool constSpeed)
		{
			MyReverser = reverser;
			MyPowerNotch = powerNotch;
			MyBrakeNotch = brakeNotch;
			MyLocoBrakeNotch = locoBrakeNotch;
			MyConstSpeed = constSpeed;
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
			MyReverser = reverser;
			MyPowerNotch = powerNotch;
			MyBrakeNotch = brakeNotch;
			MyLocoBrakeNotch = locoBrakeNotch;
			MyConstSpeed = constSpeed;
			MyHoldBrake = holdBrake;
		}
	}
}
