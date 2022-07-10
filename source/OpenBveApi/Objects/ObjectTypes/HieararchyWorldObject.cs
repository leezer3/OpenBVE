using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace OpenBveApi.Objects
{
	/// <summary>A container object for Hierarchy Animated Objects within the world</summary>
	public class HieararchyWorldObject : WorldObject
	{
		/// <summary>The internal object</summary>
		public new HierarchyAnimatedObject Object;

		/// <summary>Creates a new HiearchyWorldObject</summary>
		/// <param name="Host">The host application</param>
		public HieararchyWorldObject(HostInterface Host) : base(Host)
		{
		}

		/// <inheritdoc/>
		public override void Update(AbstractTrain NearestTrain, double TimeElapsed, bool ForceUpdate, bool CurrentlyVisible)
		{
			if (CurrentlyVisible | ForceUpdate)
			{
				if (Object.SecondsSinceLastUpdate >= 1.0 | ForceUpdate)
				{
					double timeDelta = Object.SecondsSinceLastUpdate + TimeElapsed;
					Object.SecondsSinceLastUpdate = 0.0;
					Object.Update(false, NearestTrain, NearestTrain?.DriverCar ?? 0, -1, TrackPosition, Position, Direction, Up, Side, true, true, timeDelta, true);
				}
				else
				{
					Object.SecondsSinceLastUpdate += TimeElapsed;
				}
				if (!base.Visible)
				{
					for (int i = 0; i < Object.Objects.Length; i++)
					{
						currentHost.ShowObject(Object.Objects[i].State, ObjectType.Dynamic);
					}
					base.Visible = true;
				}
			}
			else
			{
				Object.SecondsSinceLastUpdate += TimeElapsed;
				if (base.Visible)
				{
					for (int i = 0; i < Object.Objects.Length; i++)
					{
						currentHost.HideObject(Object.Objects[i].State);
					}
					base.Visible = false;
				}
			}
		}

		/// <inheritdoc/>
		public override bool IsVisible(Vector3 CameraPosition, double BackgroundImageDistance, double ExtraViewingDistance)
		{
			if (Object == null)
			{
				return false;
			}
			double pa = TrackPosition - Radius - 10.0;
			double pb = TrackPosition + Radius + 10.0;
			double ta = CameraPosition.Z - BackgroundImageDistance - ExtraViewingDistance;
			double tb = CameraPosition.Z + BackgroundImageDistance + ExtraViewingDistance;
			return pb >= ta & pa <= tb;
		}
	}
}
