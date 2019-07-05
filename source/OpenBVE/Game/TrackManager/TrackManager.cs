using OpenBveApi.Routes;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Whether sound events are currently suppressed</summary>
		internal static bool SuppressSoundEvents = false;

		/// <summary>The list of tracks available in the simulation.</summary>
		internal static Track[] Tracks = new Track[] { new Track() };

		
	}
}
