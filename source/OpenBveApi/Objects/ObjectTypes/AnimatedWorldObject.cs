using OpenBveApi.Trains;

namespace OpenBveApi.Objects
{
	public class AnimatedWorldObject : WorldObject
	{
		private readonly Hosts.HostInterface currentHost;
		
		/// <summary>The signalling section the object refers to (Only relevant for objects placed using Track.Sig</summary>
		public int SectionIndex;

		public AnimatedWorldObject(Hosts.HostInterface Host)
		{
			currentHost = Host;
		}

		public override void Update(AbstractTrain NearestTrain, double TimeElapsed, bool ForceUpdate, bool Visible)
		{
			if (Visible | ForceUpdate)
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
					currentHost.HideObject(ref Object.internalObject);
					base.Visible = false;
				}
			}
		}
	}
}
