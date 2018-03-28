namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
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
			internal BrakeHandle Brake;
			/// <summary>The Emergency Brake</summary>
			internal EmergencyHandle EmergencyBrake;
			/// <summary>The Hold Brake</summary>
			internal HoldBrakeHandle HoldBrake;
			/// <summary>The Air Brake</summary>
			internal TrainAirBrake AirBrake;
		}
	}
}
