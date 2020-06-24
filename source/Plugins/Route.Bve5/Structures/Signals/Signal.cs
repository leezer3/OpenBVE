using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;
using RouteManager2.SignalManager;

namespace Bve5RouteParser
{
	internal class Signal
	{
		internal Signal(double trackPosition, int sectionIndex, SignalObject signalObject, Vector2 position, double yaw, double pitch, double roll, bool showObject, bool showPost)
		{
			TrackPosition = trackPosition;
			SectionIndex = sectionIndex;
			SignalObject = signalObject;
			Position = position;
			Yaw = yaw;
			Pitch = pitch;
			Roll = roll;
			ShowObject = showObject;
			ShowPost = showPost;
		}

		internal void Create(Vector3 wpos, Transformation RailTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockInterval, double Brightness, StaticObject SignalPost)
		{
			double dz = TrackPosition - StartingDistance;
			if (ShowPost)
			{
				// post
				double dx = Position.X;
				wpos += dx * RailTransformation.X + dz * RailTransformation.Z;
				SignalPost.CreateObject(wpos, RailTransformation, Transformation.NullTransformation, -1, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockInterval, TrackPosition, Brightness, false);
			}
			if (ShowObject)
			{
				// signal object
				double dx = Position.X;
				double dy = Position.Y;
				wpos += dx * RailTransformation.X + dy * RailTransformation.Y + dz * RailTransformation.Z;
				SignalObject.Create(wpos, RailTransformation, new Transformation(Yaw, Pitch, Roll), SectionIndex, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockInterval, TrackPosition, Brightness);
			}
		}

		internal readonly double TrackPosition;
		private readonly int SectionIndex;
		private readonly SignalObject SignalObject;
		private readonly Vector2 Position;
		private readonly double Yaw;
		internal readonly double Pitch;
		private readonly double Roll;
		private readonly bool ShowObject;
		private readonly bool ShowPost;
	}
}
