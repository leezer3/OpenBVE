using OpenBveApi.Routes;
using RouteManager2.Events;

namespace CsvRwRouteParser
{
	internal class RailSound
	{
		internal readonly int NewSound;

		internal readonly double TrackPosition;

		internal RailSound(double trackPosition, int newSound)
		{
			TrackPosition = trackPosition;
			NewSound = newSound;
		}

		internal void Create(int PreviousSound)
		{
			TrackFollower t = new TrackFollower(Plugin.CurrentHost);
			t.UpdateAbsolute(TrackPosition, true, false);
			TrackElement te = Plugin.CurrentRoute.Tracks[0].Elements[t.LastTrackElement];
			te.Events.Add(new RailSoundsChangeEvent(te.StartingTrackPosition - TrackPosition, NewSound, PreviousSound, NewSound, PreviousSound));
		}
	}
}
