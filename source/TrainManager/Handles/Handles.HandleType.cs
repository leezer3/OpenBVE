namespace TrainManager.Handles
{
	/// <summary>The differing types of handles available</summary>
	public enum HandleType
	{
		/// <summary>Separate, non-interlocked power and brake handles</summary>
		TwinHandle = 0,
		/// <summary>Single combined power and brake handle</summary>
		SingleHandle = 1,
		/// <summary>Separate power and brake handles, interlocked so that power may not be applied whilst brake is active</summary>
		InterlockedTwinHandle = 2,
		/// <summary>Twin handles with an interlocked power / reverser</summary>
		InterlockedReverserHandle = 3
	}
}
