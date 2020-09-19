using System;

namespace OpenBveApi.Runtime
{
	/// <summary>Represents the state of the doors.</summary>
	[Flags]
	public enum DoorStates 
	{
		/// <summary>No door is open.</summary>
		None = 0,
		/// <summary>The left doors are open.</summary>
		Left = 1,
		/// <summary>The right doors are open.</summary>
		Right = 2,
		/// <summary>All doors are open.</summary>
		Both = 3
	}
}
