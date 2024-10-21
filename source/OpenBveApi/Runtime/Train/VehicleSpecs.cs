using System.Runtime.Serialization;
// ReSharper disable UnusedMember.Global

namespace OpenBveApi.Runtime
{
	/// <summary>Represents the specification of the train.</summary>
	[DataContract]
	public class VehicleSpecs
	{
		/// <summary>The number of power notches the train has.</summary>
		[DataMember]
		private readonly int MyPowerNotches;

		/// <summary>The type of brake the train uses.</summary>
		[DataMember]
		private readonly BrakeTypes MyBrakeType;

		/// <summary>Whether the train has a hold brake.</summary>
		[DataMember]
		private readonly bool MyHasHoldBrake;

		/// <summary>Whether the train has a hold brake.</summary>
		[DataMember]
		private readonly bool MyHasLocoBrake;

		/// <summary>The number of brake notches the train has, including the hold brake, but excluding the emergency brake.</summary>
		[DataMember]
		private readonly int MyBrakeNotches;

		/// <summary>The number of cars the train has.</summary>
		[DataMember]
		private readonly int MyCars;

		/// <summary>The number of headlight states the train has.</summary>
		[DataMember]
		private readonly int MyHeadlightStates;

		/// <summary>Gets the number of power notches the train has.</summary>
		public int PowerNotches => MyPowerNotches;

		/// <summary>Gets the type of brake the train uses.</summary>
		public BrakeTypes BrakeType => MyBrakeType;

		/// <summary>Gets the number of brake notches the train has, including the hold brake, but excluding the emergency brake.</summary>
		public int BrakeNotches => MyBrakeNotches;

		/// <summary>Gets whether the train has a hold brake.</summary>
		public bool HasHoldBrake => MyHasHoldBrake;

		/// <summary>Gets whether the train has a hold brake.</summary>
		public bool HasLocoBrake => MyHasLocoBrake;

		/// <summary>Gets the index of the brake notch that corresponds to B1 or LAP.</summary>
		/// <remarks>For trains without a hold brake, this returns 1. For trains with a hold brake, this returns 2.</remarks>
		public int AtsNotch => MyHasHoldBrake ? 2 : 1;

		/// <summary>Gets the index of the brake notch that corresponds to 70% of the available brake notches.</summary>
		public int B67Notch => (int) System.Math.Round(0.7 * MyBrakeNotches);

		/// <summary>Gets the number of cars the train has.</summary>
		public int Cars => MyCars;

		/// <summary>Gets the number of headlight states the train has.</summary>
		public int HeadlightStates => MyHeadlightStates;

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="powerNotches">The number of power notches the train has.</param>
		/// <param name="brakeType">The type of brake the train uses.</param>
		/// <param name="brakeNotches">The number of brake notches the train has, including the hold brake, but excluding the emergency brake.</param>
		/// <param name="hasHoldBrake">Whether the train has a hold brake.</param>
		/// <param name="cars">The number of cars the train has.</param>
		public VehicleSpecs(int powerNotches, BrakeTypes brakeType, int brakeNotches, bool hasHoldBrake, int cars)
		{
			MyPowerNotches = powerNotches;
			MyBrakeType = brakeType;
			MyBrakeNotches = brakeNotches;
			MyHasHoldBrake = hasHoldBrake;
			MyHasLocoBrake = false;
			MyCars = cars;
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
			MyPowerNotches = powerNotches;
			MyBrakeType = brakeType;
			MyBrakeNotches = brakeNotches;
			MyHasHoldBrake = hasHoldBrake;
			MyHasLocoBrake = hasLocoBrake;
			MyCars = cars;
		}

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="powerNotches">The number of power notches the train has.</param>
		/// <param name="brakeType">The type of brake the train uses.</param>
		/// <param name="brakeNotches">The number of brake notches the train has, including the hold brake, but excluding the emergency brake.</param>
		/// <param name="hasHoldBrake">Whether the train has a hold brake.</param>
		/// <param name="hasLocoBrake">Whether the train has a loco brake.</param>
		/// <param name="cars">The number of cars the train has.</param>
		/// <param name="headlightStates">The number of headlight states the train has</param>
		public VehicleSpecs(int powerNotches, BrakeTypes brakeType, int brakeNotches, bool hasHoldBrake, bool hasLocoBrake, int cars, int headlightStates)
		{
			MyPowerNotches = powerNotches;
			MyBrakeType = brakeType;
			MyBrakeNotches = brakeNotches;
			MyHasHoldBrake = hasHoldBrake;
			MyHasLocoBrake = hasLocoBrake;
			MyCars = cars;
			MyHeadlightStates = headlightStates;
		}
	}
}
