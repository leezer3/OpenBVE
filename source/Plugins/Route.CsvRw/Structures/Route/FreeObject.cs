using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;

namespace CsvRwRouteParser
{
	internal class FreeObj : AbstractStructure
	{
		/// <summary>The routefile index of the object</summary>
		private readonly int Type;
		/// <summary>The position of the object</summary>
		private readonly Vector2 Position;
		/// <summary>The yaw of the object (radians)</summary>
		private readonly double Yaw;
		/// <summary>The pitch of the object (radians)</summary>
		private readonly double Pitch;
		/// <summary>The roll of the object (radians)</summary>
		private readonly double Roll;

		internal FreeObj(double trackPosition, int type, Vector2 position, double yaw, double pitch = 0, double roll = 0) : base(trackPosition)
		{
			Type = type;
			Position = position;
			Yaw = yaw;
			Pitch = pitch;
			Roll = roll;
		}

		internal void CreateRailAligned(ObjectDictionary FreeObjects, Vector3 WorldPosition, Transformation RailTransformation, double StartingDistance, double EndingDistance)
		{
			double dz = TrackPosition - StartingDistance;
			WorldPosition += Position.X * RailTransformation.X + Position.Y * RailTransformation.Y + dz * RailTransformation.Z;
			FreeObjects.TryGetValue(Type, out UnifiedObject obj);
			obj?.CreateObject(WorldPosition, RailTransformation, new Transformation(Yaw, Pitch, Roll), StartingDistance, EndingDistance, TrackPosition);
		}

		internal void CreateGroundAligned(ObjectDictionary FreeObjects, Vector3 WorldPosition, Transformation GroundTransformation, Vector2 Direction, double Height, double StartingDistance, double EndingDistance)
		{
			double d = TrackPosition - StartingDistance;
			Vector3 wpos = WorldPosition + new Vector3(Direction.X * d + Direction.Y * Position.X, Position.Y - Height, Direction.Y * d - Direction.X * Position.X);
			FreeObjects.TryGetValue(Type, out UnifiedObject obj);
			obj?.CreateObject(wpos, GroundTransformation, new Transformation(Yaw, Pitch, Roll), StartingDistance, EndingDistance, TrackPosition);
		}
	}
}
