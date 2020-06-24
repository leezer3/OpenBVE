using OpenBveApi.Math;

namespace CsvRwRouteParser
{
	internal class DestinationEvent
	{
		internal readonly double TrackPosition;
		internal readonly int Type;
		internal readonly bool TriggerOnce;
		internal readonly int BeaconStructureIndex;
		internal readonly int NextDestination;
		internal readonly int PreviousDestination;
		internal readonly Vector2 Position;
		internal readonly double Yaw;
		internal readonly double Pitch;
		internal readonly double Roll;

		internal DestinationEvent(double trackPosition, int type, bool triggerOnce, int beaconStructureIndex, int nextDestination, int previousDestination, Vector2 position, double yaw, double pitch, double roll)
		{
			TrackPosition = trackPosition;
			Type = type;
			TriggerOnce = triggerOnce;
			BeaconStructureIndex = beaconStructureIndex;
			NextDestination = nextDestination;
			PreviousDestination = previousDestination;
			Position = position;
			Yaw = yaw;
			Pitch = pitch;
			Roll = roll;
		}
	}
}
