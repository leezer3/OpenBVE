namespace TrainManager.Handles
{
	/// <summary>The different handle states</summary>
	public enum HandleState
	{
		/// <summary>Gets the actual handle state as used by the physics system</summary>
		Actual,
		/// <summary>Gets the handle state set by the driver (player)</summary>
		Driver,
		/// <summary>Gets the handle state set by the safety system plugin</summary>
		Safety
	}
}
