using OpenBveApi.Trains;

namespace OpenBveApi.Objects
{
	/// <summary>A container object for animated objects within the world</summary>
	public class AnimatedWorldObject : WorldObject
	{
		/// <summary>Holds a reference to the host application</summary>
		private readonly Hosts.HostInterface currentHost;
		/// <summary>The signalling section the object refers to (Only relevant for objects placed using Track.Sig</summary>
		public int SectionIndex;

		/// <summary>Creates a new Animated World Object</summary>
		/// <param name="Host">The host application</param>
		public AnimatedWorldObject(Hosts.HostInterface Host)
		{
			currentHost = Host;
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
					Object.Update(false, NearestTrain, NearestTrain == null ? 0 : NearestTrain.DriverCar, SectionIndex, TrackPosition, Position, Direction, Up, Side, true, true, timeDelta, true);
				}
				else
				{
					Object.SecondsSinceLastUpdate += TimeElapsed;
				}
				if (!base.Visible)
				{
					currentHost.ShowObject(Object.internalObject, ObjectType.Dynamic);
					base.Visible = true;
				}
			}
			else
			{
				Object.SecondsSinceLastUpdate += TimeElapsed;
				if (base.Visible)
				{
					currentHost.HideObject(Object.internalObject);
					base.Visible = false;
				}
			}
		}
	}
}
