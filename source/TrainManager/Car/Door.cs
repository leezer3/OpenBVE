using SoundManager;

namespace TrainManager.Car
{
	/// <summary>A train door</summary>
	public class Door
	{
		/// <summary>A value of -1 (left) or 1 (right).</summary>
		public readonly int Direction;
		/// <summary>A value between 0 (closed) and 1 (opened).</summary>
		public double State;
		/// <summary>The value of the state at which a door lock simulation is scheduled.</summary>
		public double DoorLockState;
		/// <summary>The duration of the scheduled door lock simulation.</summary>
		public double DoorLockDuration;
		/// <summary>Stores whether the doors button is currently pressed</summary>
		public bool ButtonPressed;
		/// <summary>Whether this door is anticipated to be open at the next station stop</summary>
		public bool AnticipatedOpen;
		/// <summary>Played once when this set of doors opens</summary>
		public CarSound OpenSound;
		/// <summary>Played once when this set of doors closes</summary>
		public CarSound CloseSound;
		/// <summary>Whether reopen the door or not</summary>
		public bool AnticipatedReopen;
		/// <summary>The number of times that reopened the door</summary>
		public int ReopenCounter;
		/// <summary>The upper limit of the number of times reopen the door</summary>
		public int ReopenLimit;
		/// <summary>The duration of interference in the door</summary>
		public double NextReopenTime;
		/// <summary>Ratio that width of the obstacle to the overall width of the door</summary>
		public double InterferingObjectRate;
		/// <summary>The width of the door opening in mm</summary>
		public readonly double Width;
		/// <summary>The maximum tolerance for an interfering object in the door opening before closure will not succeed</summary>
		public readonly double MaxTolerance;

		/// <summary>Creates a new door</summary>
		/// <param name="direction">The direction in which the door is to open</param>
		/// <param name="width">The width of the door in millimeters</param>
		/// <param name="tolerance">The maximum tolerance for the door to be considered closed</param>
		public Door(int direction, double width, double tolerance)
		{
			Direction = direction;
			Width = width;
			MaxTolerance = tolerance;
			OpenSound = new CarSound();
			CloseSound = new CarSound();
			State = 0.0;
		}
	}
}
