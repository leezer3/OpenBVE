namespace OpenBveApi.Interface
{
	/// <summary>The possible states of a digital control (button)</summary>
	public enum DigitalControlState
	{
		/// <summary>The button has been released, but the simulation has not yet updated to reflect this</summary>
		ReleasedAcknowledged = 0,
		/// <summary>The button is released</summary>
		Released = 1,
		/// <summary>
		/// The button is pressed
		/// </summary>
		Pressed = 2,
		/// <summary>The button has been pressed, but the simulation has not yet updated to reflect this</summary>
		PressedAcknowledged = 3
	}
}
