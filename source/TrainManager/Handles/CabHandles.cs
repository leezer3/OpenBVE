using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>The cab handles (controls) of a train</summary>
	public class CabHandles
	{
		/// <summary>The Reverser</summary>
		public readonly ReverserHandle Reverser;
		/// <summary>The Power</summary>
		public PowerHandle Power;
		/// <summary>The Brake</summary>
		public AbstractHandle Brake;
		/// <summary>The Loco brake handle</summary>
		public AbstractHandle LocoBrake;
		/// <summary>The Emergency Brake</summary>
		public readonly EmergencyHandle EmergencyBrake;
		/// <summary>The Hold Brake</summary>
		public readonly HoldBrakeHandle HoldBrake;
		/// <summary>Whether the train has a combined power and brake handle</summary>
		public HandleType HandleType;
		/// <summary>Whether the train has the Hold Brake fitted</summary>
		public bool HasHoldBrake;
		/// <summary>Whether the train has a locomotive brake</summary>
		public bool HasLocoBrake;
		/// <summary>The loco brake type</summary>
		public LocoBrakeType LocoBrakeType;

		public CabHandles(TrainBase train)
		{
			Reverser = new ReverserHandle(train);
			EmergencyBrake = new EmergencyHandle(train);
			HoldBrake = new HoldBrakeHandle(train);
		}
	}
}
