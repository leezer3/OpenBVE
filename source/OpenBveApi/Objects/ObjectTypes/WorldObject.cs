using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace OpenBveApi.Objects
{
	/// <summary>Represents an abstract object or sound placed within the game world</summary>
	public abstract class WorldObject
	{
		/// <summary>Holds a reference to the host application</summary>
		protected readonly Hosts.HostInterface currentHost;

		/// <summary>The position vector for this object</summary>
		public Vector3 Position;
		/// <summary>The track position for this object</summary>
		public double TrackPosition;
		/// <summary>The relative track position for this object</summary>
		public virtual double RelativeTrackPosition => TrackPosition;

		/// <summary>Whether the object is currently visible at the player's camera position</summary>
		public bool Visible;
		/// <summary>The world direction vector</summary>
		public Vector3 Direction;
		/// <summary>The world up vector</summary>
		public Vector3 Up;
		/// <summary>The world side vector</summary>
		public Vector3 Side;
		/// <summary>The radius of the object</summary>
		public double Radius;
		/// <summary>The actual animated object</summary>
		public AnimatedObject Object;

		/// <summary>Creates a new WorldObject</summary>
		protected WorldObject(HostInterface Host)
		{
			currentHost = Host;
		}

		/// <summary>Clones this object</summary>
		/// <returns>The new object</returns>
		public virtual WorldObject Clone()
		{
			WorldObject wo = (WorldObject)MemberwiseClone();
			wo.Object = Object?.Clone();

			if (wo.Object != null)
			{
				currentHost.CreateDynamicObject(ref wo.Object.internalObject);
			}

			return wo;
		}

		/// <summary>Updates the object</summary>
		/// <param name="NearestTrain">The nearest train to this object</param>
		/// <param name="TimeElapsed">The time elapsed in milliseconds</param>
		/// <param name="ForceUpdate">Whether this is a forced update (e.g. Change of viewpoint) or periodic</param>
		/// <param name="CurrentlyVisible">Whether the object is currently visble to the player</param>
		public abstract void Update(AbstractTrain NearestTrain, double TimeElapsed, bool ForceUpdate, bool CurrentlyVisible);
	}
}
