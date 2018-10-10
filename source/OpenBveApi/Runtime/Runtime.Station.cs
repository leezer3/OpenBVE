namespace OpenBveApi.Runtime
{
	/// <summary>Represents a station.</summary>
	public class Station
	{
		/// <summary>The name of the station.</summary>
		public string Name;
		/// <summary>The expected arrival time.</summary>
		public double ArrivalTime;
		/// <summary>The expected departure time.</summary>
		public double DepartureTime;
		/// <summary>The expected time stopped.</summary>
		public double StopTime;
		/// <summary>Whether the next signal is held red until departure.</summary>
		public bool ForceStopSignal;
		/// <summary>Whether the left doors are to open.</summary>
		public bool OpenLeftDoors;
		/// <summary>Whether the right doors are to open.</summary>
		public bool OpenRightDoors;
		/// <summary>The track position of this station.</summary>
		public double DefaultTrackPosition;
		/// <summary>The stop position applicable to the current train.</summary>
		public double StopPosition;
		/// <summary>The stop mode for this station</summary>
		public StationStopMode StopMode;
		/// <summary>The type of this station</summary>
		public StationType Type;
		/// <summary>The incidence of reopening of the door</summary>
		public double ReopenDoor;
		/// <summary>The upper limit of the number of times reopen the door</summary>
		public int ReopenStationLimit;
		/// <summary>The duration of interference in the door</summary>
		public double InterferenceInDoor;
		/// <summary>The maximum width of the obstacle to the overall width of the door</summary>
		public int MaxInterferingObjectRate;

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
		RequestStop = 3
	}
}
