using System.Collections.Generic;
using OpenTK;

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
		/// <summary>Holds the available screen resolutions</summary>
		public readonly List<ScreenResolution> AvailableResolutions;

		internal Screen()
		{
			//Find all resolutions our screen is capable of displaying, but don't store HZ info etc.
			AvailableResolutions = new List<ScreenResolution>();
			ScreenResolution lastResolution = new ScreenResolution(0,0);
			for (int i = 0; i < DisplayDevice.Default.AvailableResolutions.Count; i++)
			{
				if (DisplayDevice.Default.AvailableResolutions[i].Width != lastResolution.Width || DisplayDevice.Default.AvailableResolutions[i].Height != lastResolution.Height)
				{
					lastResolution = new ScreenResolution(DisplayDevice.Default.AvailableResolutions[i].Width, DisplayDevice.Default.AvailableResolutions[i].Height);
					AvailableResolutions.Add(lastResolution);
				}
			}
		}
	}
}
