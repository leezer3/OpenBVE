using System;
using System.Diagnostics;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		internal class TrackFollowingObject : WorldObject
		{
			/// <summary>The actual animated object</summary>
			internal AnimatedObject Object;
			/// <summary>The signalling section the object refers to (Only relevant for objects placed using Track.Sig</summary>
			internal int SectionIndex;
			/// <summary>The curve radius at the object's track position</summary>
			internal double Radius;

			internal TrackManager.TrackFollower FrontAxleFollower;
			internal TrackManager.TrackFollower RearAxleFollower;
			internal double FrontAxlePosition;
			internal double RearAxlePosition;
#pragma warning disable 0649
//TODO: Track following objects currently do not take into account toppling or cant, reserved for future use
			internal double CurrentRollDueToTopplingAngle;
			internal double CurrentRollDueToCantAngle;
#pragma warning restore 0649       

			internal override void Update(double TimeElapsed, bool ForceUpdate)
			{
				const double extraRadius = 10.0;
				double z = Object.TranslateZFunction == null ? 0.0 : Object.TranslateZFunction.LastResult;
				double pa = TrackPosition + z - Radius - extraRadius;
				double pb = TrackPosition + z + Radius + extraRadius;
				double ta = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z - World.BackgroundImageDistance - World.ExtraViewingDistance;
				double tb = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z + World.BackgroundImageDistance + World.ExtraViewingDistance;
				bool visible = pb >= ta & pa <= tb;
				if (visible | ForceUpdate)
				{
					if (Object.SecondsSinceLastUpdate >= Object.RefreshRate | ForceUpdate)
					{
						double timeDelta = Object.SecondsSinceLastUpdate + TimeElapsed;
						Object.SecondsSinceLastUpdate = 0.0;
						TrainManager.Train train = null;
						double trainDistance = double.MaxValue;
						for (int j = 0; j < TrainManager.Trains.Length; j++)
						{
							if (TrainManager.Trains[j].State == TrainManager.TrainState.Available)
							{
								double distance;
								if (TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition < TrackPosition)
								{
									distance = TrackPosition - TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition;
								}
								else if (TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition > TrackPosition)
								{
									distance = TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition - TrackPosition;
								}
								else
								{
									distance = 0;
								}
								if (distance < trainDistance)
								{
									train = TrainManager.Trains[j];
									trainDistance = distance;
								}
							}
						}
						if (Visible)
						{
							//Calculate the distance travelled
							double delta = UpdateTrackFollowerScript(false, train, train == null ? 0 : train.DriverCar, SectionIndex, TrackPosition, Position, Direction, Up, Side, false, true, true, timeDelta);
							//Update the front and rear axle track followers
							FrontAxleFollower.Update((TrackPosition + FrontAxlePosition) + delta, true, true);
							RearAxleFollower.Update((TrackPosition + RearAxlePosition) + delta, true, true);
							//Update the base object position
							FrontAxleFollower.UpdateWorldCoordinates(false);
							RearAxleFollower.UpdateWorldCoordinates(false);
							UpdateObjectPosition();
						}
						//Update the actual animated object- This must be done last in case the user has used Translation or Rotation
						Object.Update(false, train, train == null ? 0 : train.DriverCar, SectionIndex, FrontAxleFollower.TrackPosition, FrontAxleFollower.WorldPosition, Direction, Up, Side, false, true, true, timeDelta, true);
					}
					else
					{
						Object.SecondsSinceLastUpdate += TimeElapsed;
					}
					if (!Visible)
					{
						Renderer.ShowObject(Object.ObjectIndex, ObjectType.Dynamic);
						Visible = true;
					}
				}
				else
				{
					Object.SecondsSinceLastUpdate += TimeElapsed;
					if (Visible)
					{
						Renderer.HideObject(Object.ObjectIndex);
						Visible = false;
					}
				}
			}

			/// <summary>Updates the position and rotation of an animated object which follows a track</summary>
			private void UpdateObjectPosition()
			{
				//Get vectors
				Vector3 d = new Vector3();
				Vector3 u;
				Vector3 s;
				{
					d.X = FrontAxleFollower.WorldPosition.X - RearAxleFollower.WorldPosition.X;
					d.Y = FrontAxleFollower.WorldPosition.Y - RearAxleFollower.WorldPosition.Y;
					d.Z = FrontAxleFollower.WorldPosition.Z - RearAxleFollower.WorldPosition.Z;
					double t = 1.0 / Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z);
					d *= t;
					t = 1.0 / Math.Sqrt(d.X * d.X + d.Z * d.Z);
					double ex = d.X * t;
					double ez = d.Z * t;
					s = new Vector3(ez, 0.0, -ex);
					u = Vector3.Cross(d, s);
				}

				// apply position due to cant/toppling
				{
					double a = CurrentRollDueToTopplingAngle + CurrentRollDueToCantAngle;
					double x = Math.Sign(a) * 0.5 * Game.RouteRailGauge * (1.0 - Math.Cos(a));
					double y = Math.Abs(0.5 * Game.RouteRailGauge * Math.Sin(a));
					double cx = s.X * x + u.X * y;
					double cy = s.Y * x + u.Y * y;
					double cz = s.Z * x + u.Z * y;
					FrontAxleFollower.WorldPosition.X += cx;
					FrontAxleFollower.WorldPosition.Y += cy;
					FrontAxleFollower.WorldPosition.Z += cz;
					RearAxleFollower.WorldPosition.X += cx;
					RearAxleFollower.WorldPosition.Y += cy;
					RearAxleFollower.WorldPosition.Z += cz;
				}
				// apply rolling
				{
					double a = CurrentRollDueToTopplingAngle - CurrentRollDueToCantAngle;
					double cosa = Math.Cos(a);
					double sina = Math.Sin(a);
					s.Rotate(d, cosa, sina);
					u.Rotate(d, cosa, sina);
					Up = u;
				}
				Direction = d;
				Side = s;
			}

			private double UpdateTrackFollowerScript(bool IsPartOfTrain, TrainManager.Train Train, int CarIndex, int SectionIndex, double TrackPosition, Vector3 WorldPosition, Vector3 Direction, Vector3 Up, Vector3 Side, bool Overlay, bool UpdateFunctions, bool Show, double TimeElapsed)
			{
				double x = 0.0;
				if (Object.TrackFollowerFunction != null)
				{
					if (UpdateFunctions)
					{
						x = Object.TrackFollowerFunction.Perform(Train, CarIndex, WorldPosition, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
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
