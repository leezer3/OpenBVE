using System;
using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace OpenBveApi.Objects
{
	/// <summary>A container object for animated objects within the world</summary>
	public class KeyframeWorldObject : WorldObject
	{
		/// <summary>The signalling section the object refers to (Only relevant for objects placed using Track.Sig</summary>
		public int SectionIndex;
		/// <summary>The animated object</summary>
		public new KeyframeAnimatedObject Object;

		/// <summary>Creates a new Animated World Object</summary>
		/// <param name="Host">The host application</param>
		public KeyframeWorldObject(Hosts.HostInterface Host) : base(Host)
		{
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
					Object.Update(TrackPosition, Position, Direction, Up, Side, timeDelta);
				}
				else
				{
					Object.SecondsSinceLastUpdate += TimeElapsed;
				}
				if (!Visible)
				{
					for (int i = 0; i < Object.Objects.Length; i++)
					{
						currentHost.ShowObject(Object.Objects[i], ObjectType.Dynamic);
					}
					Visible = true;
				}
			}
			else
			{
				Object.SecondsSinceLastUpdate += TimeElapsed;
				if (Visible)
				{
					for (int i = 0; i < Object.Objects.Length; i++)
					{
						currentHost.HideObject(Object.Objects[i]);
					}
					Visible = false;
				}
			}
		}

		/// <inheritdoc/>
		public override bool IsVisible(Vector3 CameraPosition, double BackgroundImageDistance, double ExtraViewingDistance)
		{
			double z = 0;
			double pa = TrackPosition + z - Radius - 10.0;
			double pb = TrackPosition + z + Radius + 10.0;
			double ta = CameraPosition.Z - BackgroundImageDistance - ExtraViewingDistance;
			double tb = CameraPosition.Z + BackgroundImageDistance + ExtraViewingDistance;
			return pb >= ta & pa <= tb;
		}
	}
}
