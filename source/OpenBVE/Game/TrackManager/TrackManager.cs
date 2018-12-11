using TrackManager;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Whether sound events are currently suppressed</summary>
		internal static bool SuppressSoundEvents = false;

		/// <summary>The current in-use track</summary>
		internal static Track CurrentTrack;
	}
}
