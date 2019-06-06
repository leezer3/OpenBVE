namespace LibRender
{
	public static class Screen
	{
		/// <summary>Stores the current width of the screen.</summary>
		public static int Width = 0;
		/// <summary>Stores the current height of the screen.</summary>
		public static int Height = 0;
		/// <summary>The current aspect ratio</summary>
		public static double AspectRatio;
		/// <summary>Whether the screen is set to fullscreen mode.</summary>
		public static bool Fullscreen = false;
		/// <summary>Whether the window is currently minimized</summary>
		public static bool Minimized = false;
	}
}
