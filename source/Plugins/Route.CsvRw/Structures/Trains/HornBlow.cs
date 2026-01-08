using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Runtime;
using OpenBveApi.World;

namespace CsvRwRouteParser
{
	internal class HornBlowEvent : AbstractStructure
	{
		internal readonly HornTypes Type;
		internal readonly bool TriggerOnce;
		internal readonly int BeaconStructureIndex;
		internal readonly Vector2 Position;
		internal readonly double Yaw;
		internal readonly double Pitch;
		internal readonly double Roll;

		internal HornBlowEvent(double trackPosition, HornTypes type, bool triggerOnce, int beaconStructureIndex, Vector2 position, double yaw, double pitch, double roll) : base(trackPosition)
		{
			Type = type;
			TriggerOnce = triggerOnce;
			BeaconStructureIndex = beaconStructureIndex;
			Position = position;
			Yaw = yaw;
			Pitch = pitch;
			Roll = roll;
		}

		internal void Create(Vector3 wpos, Transformation RailTransformation, double StartingDistance, double EndingDistance, ObjectDictionary Beacon)
		{
			Beacon.TryGetValue(BeaconStructureIndex, out UnifiedObject obj);
			if (obj != null)
			{
				double dx = Position.X;
				double dy = Position.Y;
				double dz = TrackPosition - StartingDistance;
				wpos += dx * RailTransformation.X + dy * RailTransformation.Y + dz * RailTransformation.Z;
				double tpos = TrackPosition;
				obj.CreateObject(wpos, RailTransformation, new Transformation(Yaw, Pitch, Roll), StartingDistance, EndingDistance, tpos);
			}
		}
	}
}
