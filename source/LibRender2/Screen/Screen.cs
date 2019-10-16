namespace LibRender2.Screens
{
	public class Screen
	{
		/// <summary>Stores the current width of the screen.</summary>
		public int Width = 0;
		/// <summary>Stores the current height of the screen.</summary>
		public int Height = 0;
		/// <summary>The current aspect ratio</summary>
		public double AspectRatio;
		/// <summary>Whether the screen is set to fullscreen mode.</summary>
		public bool Fullscreen = false;
		/// <summary>Whether the window is currently minimized</summary>
		public bool Minimized = false;

		internal Screen()
		{
		}
	}
}
