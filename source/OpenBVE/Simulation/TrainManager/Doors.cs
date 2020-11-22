using System;
using SoundManager;

namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>A train door</summary>
		internal struct Door
		{
			/// <summary>A value of -1 (left) or 1 (right).</summary>
			internal int Direction;
			/// <summary>A value between 0 (closed) and 1 (opened).</summary>
			internal double State;
			/// <summary>The value of the state at which a door lock simulation is scheduled.</summary>
			internal double DoorLockState;
			/// <summary>The duration of the scheduled door lock simulation.</summary>
			internal double DoorLockDuration;
			/// <summary>Stores whether the doors button is currently pressed</summary>
			internal bool ButtonPressed;
			/// <summary>Whether this door is anticipated to be open at the next station stop</summary>
			internal bool AnticipatedOpen;
			/// <summary>Played once when this set of doors opens</summary>
			internal CarSound OpenSound;
			/// <summary>Played once when this set of doors closes</summary>
			internal CarSound CloseSound;
			/// <summary>Whether reopen the door or not</summary>
			internal bool AnticipatedReopen;
			/// <summary>The number of times that reopened the door</summary>
			internal int ReopenCounter;
			/// <summary>The upper limit of the number of times reopen the door</summary>
			internal int ReopenLimit;
			/// <summary>The duration of interference in the door</summary>
			internal double NextReopenTime;
			/// <summary>Ratio that width of the obstacle to the overall width of the door</summary>
			internal double InterferingObjectRate;
			/// <summary>The width of the door opening</summary>
			internal double Width;
			/// <summary>The maximum tolerance for an interfering object in the door opening before closure will not succeed</summary>
			internal double MaxTolerance;
		}

		/// <summary>The potential states of the train's doors.</summary>
		[Flags]
		internal enum TrainDoorState
		{
			None = 0,
			/// <summary>Fully closed doors are present in the train.</summary>
			Closed = 1,
			/// <summary>Fully opened doors are present in the train.</summary>
			Opened = 2,
			/// <summary>Doors are present in the train which are neither fully closed nor fully opened.</summary>
			Mixed = 4,
			/// <summary>All doors in the train are fully closed.</summary>
			AllClosed = 8,
			/// <summary>All doors in the train are fully opened.</summary>
			AllOpened = 16,
			/// <summary>All doors in the train are neither fully closed nor fully opened.</summary>
			AllMixed = 32
		}
	}
}
