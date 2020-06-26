using System;
using OpenBveApi.Math;

namespace Bve5RouteParser
{
	internal class Object
	{
		internal readonly double TrackPosition;
		internal readonly string Name;
		internal readonly int Type;
		internal readonly Vector3 Position;
		internal readonly double Yaw;
		internal readonly double Pitch;
		internal readonly double Roll;
		internal readonly RailTransformationTypes BaseTransformation;

		internal Object(double trackPosition, string name, int type, Vector3 position, double yaw, double pitch, double roll, RailTransformationTypes baseTransformation)
		{
			TrackPosition = trackPosition;
			Name = name;
			Type = type;
			Position = position;
			Yaw = yaw;
			Pitch = pitch;
			Roll = roll;
			BaseTransformation = baseTransformation;
		}

		internal Object(Repeater repeater, RailTransformationTypes baseTransformation) : this(repeater.TrackPosition, repeater, baseTransformation)
		{
		}

		internal Object(double trackPosition, Repeater repeater, RailTransformationTypes baseTransformation)
		{
			TrackPosition = trackPosition;
			Name = String.Empty;
			Type = repeater.StructureTypes[0];
			Position = repeater.Position;
			Yaw = repeater.Yaw;
			Pitch = repeater.Pitch;
			Roll = repeater.Roll;
			BaseTransformation = baseTransformation;
		}
	}
}
