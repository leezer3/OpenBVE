using System.Runtime.Serialization;

namespace OpenBveApi.Runtime
{
	/// <summary>Represents the current state of the preceding train.</summary>
	[DataContract]
	public class PrecedingVehicleState
	{
		/// <summary>The location of the back of the preceding train, in meters.</summary>
		[DataMember]
		private readonly double MyLocation;

		/// <summary>The distance from the front of the current train to the back of the preceding train, in meters.</summary>
		[DataMember]
		private readonly double MyDistance;

		/// <summary>The current speed of the preceding train.</summary>
		[DataMember]
		private readonly Speed MySpeed;

		/// <summary>Gets the location of the back of the preceding train, in meters.</summary>
		public double Location => MyLocation;

		/// <summary>Gets the distance from the front of the current train to the back of the preceding train, in meters.</summary>
		public double Distance => MyDistance;

		/// <summary>Gets the speed of the preceding train.</summary>
		public Speed Speed => MySpeed;

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="location">Gets the location of the back of the preceding train, in meters.</param>
		/// <param name="distance">The distance from the front of the current train to the back of the preceding train, in meters.</param>
		/// <param name="speed">Gets the speed of the preceding train.</param>
		public PrecedingVehicleState(double location, double distance, Speed speed)
		{
			MyLocation = location;
			MyDistance = distance;
			MySpeed = speed;
		}
	}
}
