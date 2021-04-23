using System.Runtime.Serialization;

namespace OpenBveApi.Runtime
{
	/// <summary>Represents a station.</summary>
	[DataContract]
	public class Station
	{
		/// <summary>The name of the station.</summary>
		[DataMember]
		public string Name;
		/// <summary>The expected arrival time.</summary>
		[DataMember]
		public double ArrivalTime;
		/// <summary>The expected departure time.</summary>
		[DataMember]
		public double DepartureTime;
		/// <summary>Whether the next signal is held red until departure.</summary>
		[DataMember]
		public bool ForceStopSignal;
		/// <summary>Whether the left doors are to open.</summary>
		[DataMember]
		public bool OpenLeftDoors;
		/// <summary>Whether the right doors are to open.</summary>
		[DataMember]
		public bool OpenRightDoors;
		/// <summary>The track position of this station.</summary>
		[DataMember]
		public double DefaultTrackPosition;
		/// <summary>The stop position applicable to the current train.</summary>
		[DataMember]
		public double StopPosition;
		/// <summary>The stop mode for this station</summary>
		[DataMember]
		public StationStopMode StopMode;
		/// <summary>The type of this station</summary>
		[DataMember]
		public StationType Type;
		/// <summary>The incidence of reopening of the door</summary>
		[DataMember]
		public double ReopenDoor;
		/// <summary>The upper limit of the number of times reopen the door</summary>
		[DataMember]
		public int ReopenStationLimit;
		/// <summary>The duration of interference in the door</summary>
		[DataMember]
		public double InterferenceInDoor;
		/// <summary>The maximum width of the obstacle to the overall width of the door</summary>
		[DataMember]
		public int MaxInterferingObjectRate;
		/// <summary>If departing this station triggers a jump, contains the index of the station to jump to</summary>
		[DataMember]
		public int JumpIndex;
		/// <summary>The expected time stopped.</summary>
		public double StopTime
		{
			get
			{
				//If all trains must pass the expcted stop should be zero, regardless of what's set in the route
				if (StopMode == StationStopMode.AllPass)
				{
					return 0;
				}
				//A little sanity checking: Must be stopped for more than 0 and less than an hour
				if (ExpectedTimeStopped <= 0 || ExpectedTimeStopped > 3600)
				{
					return 15;
				}
				return ExpectedTimeStopped;
			}
			set
			{
				ExpectedTimeStopped = value;
			}
		}
		/// <summary>Backing property for StopTime</summary>
		private double ExpectedTimeStopped;

		/// <summary>Creates a new station with default (empty) values</summary>
		public Station()
		{
			Name = string.Empty;
			StopMode = StationStopMode.AllStop;
			Type = StationType.Normal;
		}

		/// <summary>Creates a train-specific station</summary>
		/// <param name="s">The base station</param>
		/// <param name="stopPosition">The stop position applicable to our train</param>
		public Station(Station s, double stopPosition)
		{
			Name = s.Name;
			ArrivalTime = s.ArrivalTime;
			DepartureTime = s.DepartureTime;
			StopTime = s.DepartureTime - s.ArrivalTime;
			ForceStopSignal = s.ForceStopSignal;
			OpenLeftDoors = s.OpenLeftDoors;
			OpenRightDoors = s.OpenRightDoors;
			DefaultTrackPosition = s.DefaultTrackPosition;
			StopPosition = stopPosition;
			StopMode = s.StopMode;
			Type = s.Type;
		}

		/// <summary>Indicates whether the player's train stops at a station.</summary>
		public bool PlayerStops()
		{
			return StopMode == StationStopMode.AllStop | StopMode == StationStopMode.PlayerStop | StopMode == StationStopMode.PlayerRequestStop | StopMode == StationStopMode.AllRequestStop;
		}
	}
	
	/// <summary>The differing stop modes available for a station</summary>
	public enum StationStopMode
	{
		/// <summary>All trains stop at this station</summary>
		AllStop = 0,
		/// <summary>All trains pass this station</summary>
		AllPass = 1,
		/// <summary>The player train stops at this station, AI trains do not</summary>
		PlayerStop = 2,
		/// <summary>The player train passes this station, AI trains stop</summary>
		PlayerPass = 3,
		/// <summary>This station is a random request stop for all trains</summary>
		AllRequestStop = 4,
		/// <summary>This station is a random request stop for the player train only</summary>
		PlayerRequestStop = 5
	}

	/// <summary>Defines the available station types</summary>
	public enum StationType
	{
		/// <summary>This station is a normal station</summary>
		Normal = 0,
		/// <summary>This station triggers the change end mechanic on departure</summary>
		ChangeEnds = 1,
		/// <summary>This station is the terminal station</summary>
		Terminal = 2,
		/// <summary>This station is a request stop</summary>
		RequestStop = 3,
		/// <summary>This station triggers the jump mechanic on departure</summary>
		Jump
	}
}
