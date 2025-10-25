using OpenBveApi.Motor;
using TrainManager.Car;
using TrainManager.SafetySystems;

namespace TrainManager.Trains
{
	/// <summary>Holds information about the train</summary>
	public struct TrainSpecs
	{
		/// <summary>The current average acceleration across all cars</summary>
		public double CurrentAverageAcceleration;
		/// <summary>The default safety systems installed</summary>
		public DefaultSafetySystems DefaultSafetySystems;
		/// <summary>Whether the train has a constant speed device</summary>
		public bool HasConstSpeed;
		/// <summary>Whether the constant speed device is enabled</summary>
		public bool CurrentConstSpeed;
		/// <summary>The door open mode</summary>
		public DoorMode DoorOpenMode;
		/// <summary>The door close mode</summary>
		public DoorMode DoorCloseMode;
		/// <summary>Whether door closure has been attempted</summary>
		public bool DoorClosureAttempted;
		/// <summary>Whether the pressure distribution uses the legacy averages algorithm</summary>
		public bool AveragesPressureDistribution;
		/// <summary>The pantograph state for the train</summary>
		public PantographState PantographState;
	}
}
