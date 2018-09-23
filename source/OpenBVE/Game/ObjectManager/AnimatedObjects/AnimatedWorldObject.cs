using OpenBveApi.Objects;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		private class AnimatedWorldObject : WorldObject
		{
			/// <summary>The actual animated object</summary>
			internal AnimatedObject Object;
			/// <summary>The signalling section the object refers to (Only relevant for objects placed using Track.Sig</summary>
			internal int SectionIndex;
			/// <summary>The curve radius at the object's track position</summary>
			internal double Radius;

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
						Object.Update(false, train, train == null ? 0 : train.DriverCar, SectionIndex, TrackPosition, Position, Direction, Up, Side, false, true, true, timeDelta, true);
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
		}
	}
}
