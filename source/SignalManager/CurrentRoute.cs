using OpenBve.BackgroundManager;
using OpenBve.SignalManager;
using OpenBveApi.Routes;

namespace OpenBve.RouteManager
{
	/// <summary>The current route</summary>
	public static class CurrentRoute
	{
		/// <summary>Holds all signal sections within the current route</summary>
		public static Section[] Sections = new Section[] { };
		/// <summary>Holds all .PreTrain instructions for the current route</summary>
		/// <remarks>Must be in distance and time ascending order</remarks>
		public static BogusPretrainInstruction[] BogusPretrainInstructions = new BogusPretrainInstruction[] { };
		/// <summary>The currently displayed background texture</summary>
		public static BackgroundHandle CurrentBackground = new StaticBackground(null, 6, false);
		/// <summary>The new background texture (Currently fading in)</summary>
		public static BackgroundHandle TargetBackground = new StaticBackground(null, 6, false);
	}
}
