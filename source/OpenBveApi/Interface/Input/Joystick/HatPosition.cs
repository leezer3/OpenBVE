namespace OpenBveApi.Interface
{
	/// <summary>The positions for a joystick hat</summary>
	public enum JoystickHatPosition : byte
	{
		/// <summary>The hat is in its centered (neutral) position</summary>
		Centered,
		/// <summary>The hat is in its top position.</summary>
		Up,
		/// <summary>The hat is in its top-right position.</summary>
		UpRight,
		/// <summary>The hat is in its right position.</summary>
		Right,
		/// <summary>The hat is in its bottom-right position.</summary>
		DownRight,
		/// <summary>The hat is in its bottom position.</summary>
		Down,
		/// <summary>The hat is in its bottom-left position.</summary>
		DownLeft,
		/// <summary>The hat is in its left position.</summary>
		Left,
		/// <summary>The hat is in its top-left position.</summary>
		UpLeft,
	}
}
