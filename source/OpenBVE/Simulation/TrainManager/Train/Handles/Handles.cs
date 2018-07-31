namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>The cab handles (controls) of a train</summary>
		internal struct Handles
		{
			/// <summary>The Reverser</summary>
			internal ReverserHandle Reverser;
			/// <summary>The Power</summary>
			internal PowerHandle Power;
			/// <summary>The Brake</summary>
			internal AbstractHandle Brake;
			/// <summary>The Loco brake handle</summary>
			internal AbstractHandle LocoBrake;
			/// <summary>The Emergency Brake</summary>
			internal EmergencyHandle EmergencyBrake;
			/// <summary>The Hold Brake</summary>
			internal HoldBrakeHandle HoldBrake;
			/// <summary>Whether the train has a combined power and brake handle</summary>
			internal bool SingleHandle;
			/// <summary>Whether the train has the Hold Brake fitted</summary>
			internal bool HasHoldBrake;
			/// <summary>Whether the train has a locomotive brake</summary>
			internal bool HasLocoBrake;
			/// <summary>The loco brake type</summary>
			internal LocoBrakeType LocoBrakeType;
		}

		internal struct HandleChange
		{
			internal int Value;
			internal double Time;
		}
	}
}
