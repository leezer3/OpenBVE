using OpenBveApi.Routes;

namespace OpenBve.BackgroundManager
{
	public static class CurrentRoute
	{
		/// <summary>The currently displayed background texture</summary>
		public static BackgroundHandle CurrentBackground = new StaticBackground(null, 6, false);
		/// <summary>The new background texture (Currently fading in)</summary>
		public static BackgroundHandle TargetBackground = new StaticBackground(null, 6, false);
	}
}
