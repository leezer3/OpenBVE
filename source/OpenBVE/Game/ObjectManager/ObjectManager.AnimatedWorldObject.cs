using System;
using OpenBveApi.Math;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		internal class AnimatedWorldObject
		{
			internal bool FollowsTrack = false;
			internal TrackManager.TrackFollower FrontAxleFollower;
			internal TrackManager.TrackFollower RearAxleFollower;
			internal double FrontAxlePosition;
			internal double RearAxlePosition;
			/// <summary>Holds the properties for an animated object within the simulation world</summary>
			internal Vector3 Position;
			/// <summary>The object's relative track position</summary>
			internal double TrackPosition;
			internal Vector3 Direction;
			internal Vector3 Up;
			internal Vector3 Side;
			/// <summary>The actual animated object</summary>
			internal AnimatedObject Object;
			/// <summary>The signalling section the object refers to (Only relevant for objects placed using Track.Sig</summary>
			internal int SectionIndex;
			/// <summary>The curve radius at the object's track position</summary>
			internal double Radius;
			/// <summary>Whether the object is currently visible</summary>
			internal bool Visible;
			/*
			 * NOT IMPLEMENTED, BUT REQUIRED LATER
			 */
			internal double CurrentRollDueToTopplingAngle = 0;
			internal double CurrentRollDueToCantAngle = 0;

			/// <summary>Updates the position and rotation of an animated object which follows a track</summary>
			internal void UpdateTrackFollowingObject()
			{
				//Get vectors
				double dx, dy, dz;
				double ux, uy, uz;
				double sx, sy, sz;
				{
					dx = FrontAxleFollower.WorldPosition.X -
					     RearAxleFollower.WorldPosition.X;
					dy = FrontAxleFollower.WorldPosition.Y -
					     RearAxleFollower.WorldPosition.Y;
					dz = FrontAxleFollower.WorldPosition.Z -
					     RearAxleFollower.WorldPosition.Z;
					double t = 1.0 / Math.Sqrt(dx * dx + dy * dy + dz * dz);
					dx *= t;
					dy *= t;
					dz *= t;
					t = 1.0 / Math.Sqrt(dx * dx + dz * dz);
					double ex = dx * t;
					double ez = dz * t;
					sx = ez;
					sy = 0.0;
					sz = -ex;
					World.Cross(dx, dy, dz, sx, sy, sz, out ux, out uy, out uz);
				}

				// apply position due to cant/toppling
				{
					double a = CurrentRollDueToTopplingAngle +
					           CurrentRollDueToCantAngle;
					double x = Math.Sign(a) * 0.5 * Game.RouteRailGauge * (1.0 - Math.Cos(a));
					double y = Math.Abs(0.5 * Game.RouteRailGauge * Math.Sin(a));
					double cx = sx * x + ux * y;
					double cy = sy * x + uy * y;
					double cz = sz * x + uz * y;
					FrontAxleFollower.WorldPosition.X += cx;
					FrontAxleFollower.WorldPosition.Y += cy;
					FrontAxleFollower.WorldPosition.Z += cz;
					RearAxleFollower.WorldPosition.X += cx;
					RearAxleFollower.WorldPosition.Y += cy;
					RearAxleFollower.WorldPosition.Z += cz;
				}
				// apply rolling
				{
					double a = CurrentRollDueToTopplingAngle -
					           CurrentRollDueToCantAngle;
					double cosa = Math.Cos(a);
					double sina = Math.Sin(a);
					World.Rotate(ref sx, ref sy, ref sz, dx, dy, dz, cosa, sina);
					World.Rotate(ref ux, ref uy, ref uz, dx, dy, dz, cosa, sina);
					Up.X = ux;
					Up.Y = uy;
					Up.Z = uz;
				}
				Direction.X = dx;
				Direction.Y = dy;
				Direction.Z = dz;
				Side.X = sx;
				Side.Y = sy;
				Side.Z = sz;
			}

			internal double UpdateTrackFollowerScript(bool IsPartOfTrain, TrainManager.Train Train, int CarIndex, int SectionIndex, double TrackPosition, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool Overlay, bool UpdateFunctions, bool Show, double TimeElapsed)
			{
				double x = 0.0;
				if (Object.TrackFollowerFunction != null)
				{
					if (UpdateFunctions)
					{
						x = Object.TrackFollowerFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain,
							TimeElapsed, Object.CurrentState);
					}
					else
					{
						x = Object.TrackFollowerFunction.LastResult;
					}
				}
				return x;
			}
		}
	}
}
