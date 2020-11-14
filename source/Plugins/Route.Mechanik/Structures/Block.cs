using System.Collections.Generic;

namespace MechanikRouteParser
{
	internal class Block
	{
		internal double StartingTrackPosition;
		internal List<RouteObject> Objects = new List<RouteObject>();
		internal double Turn = 0.0;
		internal double SpeedLimit = -1;
		internal List<SoundEvent> Sounds = new List<SoundEvent>();

		internal Block(double TrackPosition)
		{
			this.StartingTrackPosition = TrackPosition;
		}
	}
}
