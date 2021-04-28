namespace TrainManager.Handles
{
	/// <summary>The cab handles (controls) of a train</summary>
	public struct CabHandles
	{
		/// <summary>The Reverser</summary>
		public ReverserHandle Reverser;
		/// <summary>The Power</summary>
		public PowerHandle Power;
		/// <summary>The Brake</summary>
		public AbstractHandle Brake;
		/// <summary>The Loco brake handle</summary>
		public AbstractHandle LocoBrake;
		/// <summary>The Emergency Brake</summary>
		public EmergencyHandle EmergencyBrake;
		/// <summary>The Hold Brake</summary>
		public HoldBrakeHandle HoldBrake;
		/// <summary>Whether the train has a combined power and brake handle</summary>
		public HandleType HandleType;
		/// <summary>Whether the train has the Hold Brake fitted</summary>
		public bool HasHoldBrake;
		/// <summary>Whether the train has a locomotive brake</summary>
		public bool HasLocoBrake;
		/// <summary>The loco brake type</summary>
		public LocoBrakeType LocoBrakeType;
	}
}
