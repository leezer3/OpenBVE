using System;
using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace OpenBveApi.Objects
{
	/// <summary>A container object for animated objects within the world</summary>
	public class AnimatedWorldObject : WorldObject
	{
		/// <summary>The signalling section the object refers to (Only relevant for objects placed using Track.Sig</summary>
		public int SectionIndex;

		/// <summary>Creates a new Animated World Object</summary>
		/// <param name="host">The host application</param>
		public AnimatedWorldObject(Hosts.HostInterface host) : base(host)
		{
		}

		/// <inheritdoc/>
		public override WorldObject Clone()
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc/>
		public override void Update(AbstractTrain nearestTrain, double timeElapsed, bool forceUpdate, bool currentlyVisible)
		{
			if (currentlyVisible | forceUpdate)
			{
				if (Object.SecondsSinceLastUpdate >= Object.RefreshRate | forceUpdate)
				{
					double timeDelta = Object.SecondsSinceLastUpdate + timeElapsed;
					Object.SecondsSinceLastUpdate = 0.0;
					Object.Update(nearestTrain, nearestTrain?.DriverCar ?? 0, TrackPosition, Position, Direction, Up, Side, true, true, timeDelta, true);
				}
				else
				{
					Object.SecondsSinceLastUpdate += timeElapsed;
				}
				if (!Visible)
				{
					if (Object.CurrentState != -1)
					{
						currentHost.ShowObject(Object.internalObject, ObjectType.Dynamic);
					}
					Visible = true;
				}
			}
			else
			{
				Object.SecondsSinceLastUpdate += timeElapsed;
				if (Visible)
				{
					currentHost.HideObject(Object.internalObject);
					Visible = false;
				}
			}
		}
		
		/// <inheritdoc/>
		public override bool IsVisible(Vector3 cameraPosition, double backgroundImageDistance, double extraViewingDistance)
		{
			double z = 0;
			if (Object != null && Object.TranslateZFunction != null)
			{
				/*
				 * FIXME:
				 * Low priority, the translate Z-direction may have changed
				 */
				z += Object.TranslateZFunction.LastResult;
			}
			double pa = TrackPosition + z - Radius - 10.0;
			double pb = TrackPosition + z + Radius + 10.0;
			double ta = cameraPosition.Z - backgroundImageDistance - extraViewingDistance;
			double tb = cameraPosition.Z + backgroundImageDistance + extraViewingDistance;
			return pb >= ta & pa <= tb;
		}
	}
}
