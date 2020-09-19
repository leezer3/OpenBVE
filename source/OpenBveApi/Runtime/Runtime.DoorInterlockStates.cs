namespace OpenBveApi.Runtime
{
	/// <summary>Represents the states of the door interlock.</summary>
	public enum DoorInterlockStates
	{
		/// <summary>The train doors are fully unlocked.</summary>
		Unlocked = 0,
		/// <summary>The train doors are unlocked only on the left side.</summary>
		Left = 1,
		/// <summary>The train doors are unlocked only on the right side.</summary>
		Right = 2,
		/// <summary>The train doors are fully locked.</summary>
		Locked = 3,
	}
}
