using OpenBveApi.Math;
using OpenBveApi.World;
using RouteManager2.SignalManager;

namespace CsvRwRouteParser
{
	internal class Signal : AbstractStructure
	{
		internal Signal(double trackPosition, int sectionIndex, SignalObject signalObject, Vector2 position, double yaw, double pitch, double roll, bool showObject, bool showPost) : base(trackPosition)
		{
			SectionIndex = sectionIndex;
			SignalObject = signalObject;
			Position = position;
			Yaw = yaw;
			Pitch = pitch;
			Roll = roll;
			ShowObject = showObject;
			ShowPost = showPost;
		}

		internal void Create(Vector3 wpos, Transformation RailTransformation, double StartingDistance, double EndingDistance, double Brightness)
		{
			double dz = TrackPosition - StartingDistance;
			if (ShowPost)
			{
				/*
				 * Post-
				 * Need to clone a copy of the vector for transform
				 * Do it in the post as this is the lesser used option
				 */
				Vector3 wpos2 = new Vector3(wpos);
				wpos2 += Position.X * RailTransformation.X + dz * RailTransformation.Z;
				CompatibilityObjects.SignalPost.CreateObject(wpos2, RailTransformation, Transformation.NullTransformation, -1, StartingDistance, EndingDistance, TrackPosition, Brightness);
			}
			if (ShowObject)
			{
				// signal object
				wpos += Position.X * RailTransformation.X + Position.Y * RailTransformation.Y + dz * RailTransformation.Z;
				SignalObject.Create(wpos, RailTransformation, new Transformation(Yaw, Pitch, Roll), SectionIndex, StartingDistance, EndingDistance, TrackPosition, Brightness);
			}
		}

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
