namespace OpenBveApi.Interface
{
	/// <summary>Represents the mouse element (button or wheel movement) a control is bound to.
	/// Values below 13 map directly onto the OpenTK <c>MouseButton</c> enum; wheel movements are placed above the button range so that they can never collide with additional mouse buttons.</summary>
	public static class MouseElement
	{
		/// <summary>The left mouse button</summary>
		public const int Left = 0;
		/// <summary>The middle mouse button</summary>
		public const int Middle = 1;
		/// <summary>The right mouse button</summary>
		public const int Right = 2;
		/// <summary>The scroll wheel rotated up</summary>
		public const int ScrollUp = 13;
		/// <summary>The scroll wheel rotated down</summary>
		public const int ScrollDown = 14;
	}
}
