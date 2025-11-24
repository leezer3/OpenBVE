using System;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace OpenBveApi.Objects
{
	/// <summary>A container object for an animated object which follows a track</summary>
	public class TrackFollowingObject : WorldObject
	{
		/// <summary>The front axle follower</summary>
		public readonly TrackFollower FrontAxleFollower;
		/// <summary>The rear axle follower</summary>
		public readonly TrackFollower RearAxleFollower;
		/// <summary>The front axle position relative to the center of the object</summary>
		public double FrontAxlePosition;
		/// <summary>The rear axle position relative to the center of the object</summary>
		public double RearAxlePosition;
#pragma warning disable 0649, 1591
//TODO: Track following objects currently do not take into account toppling or cant, reserved for future use
		public double CurrentRollDueToTopplingAngle;
		public double CurrentRollDueToCantAngle;
#pragma warning restore 0649, 1591

		/// <summary>Creates a new Track Following Object</summary>
		/// <param name="Host">The host application</param>
		public TrackFollowingObject(Hosts.HostInterface Host) : base(Host)
		{
			FrontAxleFollower = new TrackFollower(currentHost);
			RearAxleFollower = new TrackFollower(currentHost);
		}

		/// <inheritdoc/>
		public override WorldObject Clone()
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc/>
		public override void Update(AbstractTrain NearestTrain, double TimeElapsed, bool ForceUpdate, bool CurrentlyVisible)
		{

			if (CurrentlyVisible | ForceUpdate)
			{
				if (Object.SecondsSinceLastUpdate >= Object.RefreshRate | ForceUpdate)
				{
					double timeDelta = Object.SecondsSinceLastUpdate + TimeElapsed;
					Object.SecondsSinceLastUpdate = 0.0;

					if (Visible)
					{
						//Calculate the distance travelled
						double delta = UpdateTrackFollowerScript(false, NearestTrain, NearestTrain?.DriverCar ?? 0, Object.SectionIndex, TrackPosition, Position, true, timeDelta);
						//Update the front and rear axle track followers
						FrontAxleFollower.UpdateAbsolute((TrackPosition + FrontAxlePosition) + delta, true, true);
						RearAxleFollower.UpdateAbsolute((TrackPosition + RearAxlePosition) + delta, true, true);
						//Update the base object position
						FrontAxleFollower.UpdateWorldCoordinates(false);
						RearAxleFollower.UpdateWorldCoordinates(false);
						UpdateObjectPosition();
					}

					//Update the actual animated object- This must be done last in case the user has used Translation or Rotation
					Object.Update(NearestTrain, NearestTrain?.DriverCar ?? 0, FrontAxleFollower.TrackPosition, FrontAxleFollower.WorldPosition, Direction, Up, Side, true, true, timeDelta, true);
				}
				else
				{
					Object.SecondsSinceLastUpdate += TimeElapsed;
				}

				if (!Visible)
				{
					currentHost.ShowObject(Object.internalObject, ObjectType.Dynamic);
					Visible = true;
				}
			}
			else
			{
				Object.SecondsSinceLastUpdate += TimeElapsed;
				if (Visible)
				{
					currentHost.HideObject(Object.internalObject);
					Visible = false;
				}
			}
		}

		/// <inheritdoc/>
		public override bool IsVisible(Vector3 CameraPosition, double ExtraViewingDistance, double BackgroundImageDistance)
		{
			double z = 0;
			if (Object != null && Object.TranslateZFunction != null)
			{
				z += Object.TranslateZFunction.LastResult;
			}
			double pa = TrackPosition + z - Radius - 10.0;
			double pb = TrackPosition + z + Radius + 10.0;
			double ta = CameraPosition.Z - BackgroundImageDistance - ExtraViewingDistance;
			double tb = CameraPosition.Z + BackgroundImageDistance + ExtraViewingDistance;
			bool isVisible = pb >= ta & pa <= tb;
			if (isVisible == false)
			{
				//Not found at the inital track position, so let's check to see if it's moved
				pa = FrontAxleFollower.TrackPosition + z - Radius - 10.0;
				pb = FrontAxleFollower.TrackPosition + z + Radius + 10.0;
				return pb >= ta & pa <= tb;
			}

			return true;
		}

		/// <summary>Updates the position and rotation of an animated object which follows a track</summary>
		private void UpdateObjectPosition()
		{
			//Get vectors
			Direction = new Vector3(FrontAxleFollower.WorldPosition - RearAxleFollower.WorldPosition);
			{
				if (Direction == Vector3.Zero)
				{
					// Both axles are in the same location (bug, or end of track) so just use the direction vector for the world piece
					Direction = FrontAxleFollower.WorldDirection;
				}
				double t = Direction.Magnitude();
				Direction *= t;
				t = 1.0 / System.Math.Sqrt(Direction.X * Direction.X + Direction.Z * Direction.Z);
				double ex = Direction.X * t;
				double ez = Direction.Z * t;
				Side = new Vector3(ez, 0.0, -ex);
				Up = Vector3.Cross(Direction, Side);
			}

			// apply position due to cant/toppling
			{
				double a = CurrentRollDueToTopplingAngle + CurrentRollDueToCantAngle;
				double x = System.Math.Sign(a) * 0.5 * FrontAxleFollower.RailGauge * (1.0 - System.Math.Cos(a));
				double y = System.Math.Abs(0.5 * FrontAxleFollower.RailGauge * System.Math.Sin(a));
				Vector3 c = Side * x + Up * y;

				FrontAxleFollower.WorldPosition += c;
				RearAxleFollower.WorldPosition += c;
			}
			// apply rolling
			{
				double a = CurrentRollDueToTopplingAngle - CurrentRollDueToCantAngle;
				Side.Rotate(Direction, a);
				Up.Rotate(Direction, a);
			}
		}

		private double UpdateTrackFollowerScript(bool IsPartOfTrain, AbstractTrain Train, int CarIndex, int currentSectionIndex, double currentTrackPosition, Vector3 WorldPosition, bool UpdateFunctions, double TimeElapsed)
		{
			double x = 0.0;
			if (Object.TrackFollowerFunction != null)
			{
				x = UpdateFunctions ? Object.TrackFollowerFunction.ExecuteScript(Train, CarIndex, WorldPosition, currentTrackPosition, currentSectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState) : Object.TrackFollowerFunction.LastResult;
			}

			return x;
		}
	}
}
