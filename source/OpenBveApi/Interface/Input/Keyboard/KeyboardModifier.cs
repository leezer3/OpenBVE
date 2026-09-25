using System;

namespace OpenBveApi.Interface
{
	/// <summary>Keyboard modifiers</summary>
	[Flags]
	public enum KeyboardModifier
	{
		/// <summary>No modifier is applied</summary>
		None = 0,
		/// <summary>The control is active when SHIFT is also pressed</summary>
		Shift = 1,
		/// <summary>The control is active when CTRL is also pressed</summary>
		Ctrl = 2,
		/// <summary>The control is active when ALT is also pressed</summary>
		Alt = 4
	}
}
