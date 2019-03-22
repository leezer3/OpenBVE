using System;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;

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
							if (TrainManager.Trains[j].State == TrainState.Available)
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
							double delta = UpdateTrackFollowerScript(false, train, train == null ? 0 : train.DriverCar, SectionIndex, TrackPosition, Position, true, timeDelta);
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
				Direction = new Vector3(FrontAxleFollower.WorldPosition - RearAxleFollower.WorldPosition);
				{
					double t = 1.0 / Direction.Norm();
					Direction *= t;
					t = 1.0 / Math.Sqrt(Direction.X * Direction.X + Direction.Z * Direction.Z);
					double ex = Direction.X * t;
					double ez = Direction.Z * t;
					Side = new Vector3(ez, 0.0, -ex);
					Up = Vector3.Cross(Direction, Side);
				}

				// apply position due to cant/toppling
				{
					double a = CurrentRollDueToTopplingAngle + CurrentRollDueToCantAngle;
					double x = Math.Sign(a) * 0.5 * Game.RouteRailGauge * (1.0 - Math.Cos(a));
					double y = Math.Abs(0.5 * Game.RouteRailGauge * Math.Sin(a));
					Vector3 c = Side * x + Up * y;

					FrontAxleFollower.WorldPosition += c;
					RearAxleFollower.WorldPosition += c;
				}
				// apply rolling
				{
					double a = CurrentRollDueToTopplingAngle - CurrentRollDueToCantAngle;
					double cosa = Math.Cos(a);
					double sina = Math.Sin(a);
					Side.Rotate(Direction, cosa, sina);
					Up.Rotate(Direction, cosa, sina);
				}
			}

			private double UpdateTrackFollowerScript(bool IsPartOfTrain, TrainManager.Train Train, int CarIndex, int currentSectionIndex, double currentTrackPosition, Vector3 WorldPosition, bool UpdateFunctions, double TimeElapsed)
			{
				double x = 0.0;
				if (Object.TrackFollowerFunction != null)
				{
					if (UpdateFunctions)
					{
						x = Object.TrackFollowerFunction.Perform(Train, CarIndex, WorldPosition, currentTrackPosition, currentSectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
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
