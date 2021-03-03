using System;

namespace TrainManager.Car
{
	/// <summary>The potential states of the train's doors.</summary>
	[Flags]
	public enum TrainDoorState
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
