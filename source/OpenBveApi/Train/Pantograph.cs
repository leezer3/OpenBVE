using System.Collections.Generic;
using OpenBveApi.Routes;

namespace OpenBveApi.Trains
{
	/// <summary>A pantograph which supplies current from the overhead line</summary>
	public class Pantograph
	{
		/// <summary>The track follower used</summary>
		private readonly TrackFollower follower;
		/// <summary>The physical location of the pantograph head relative to the front of the car</summary>
		public readonly double Location;
		/// <summary>Whether this pantograph collects power</summary>
		public readonly bool CollectsPower;
		/// <summary>The power supplies available to this pantograph</summary>
		public Dictionary<PowerSupplyTypes, PowerSupply> AvailablePowerSupplies
		{
			get
			{
				return follower.AvailablePowerSupplies;
			}
		}

		/// <summary>Creates a new pantograph</summary>
		/// <param name="currentHost">The host application</param>
		/// <param name="location">The pantograph position relative to the front of the car</param>
		/// <param name="collectsPower">Whether this pantograph collects power</param>
		public Pantograph(Hosts.HostInterface currentHost, double location, bool collectsPower)
		{
			follower = new TrackFollower(currentHost)
			{
				TriggerType = EventTriggerType.None
			};
			Location = location;
			CollectsPower = collectsPower;
		}

		/// <summary>Updates the position of the pantograph</summary>
		/// <param name="delta">The new position</param>
		/// <param name="absolute">Whether this is a relative or an absolute update</param>
		public void Update(double delta, bool absolute)
		{
			if (absolute)
			{
				follower.UpdateAbsolute(delta + Location, false, false);
			}
			else
			{
				follower.UpdateRelative(delta, true, true);
			}
		}
	}
}
