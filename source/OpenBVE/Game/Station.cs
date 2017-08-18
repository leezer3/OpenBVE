using OpenBveApi.Math;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>Defines a station stop point</summary>
		internal struct StationStop
		{
			/// <summary>The track position of the stop point</summary>
			internal double TrackPosition;
			/// <summary>The forward stop tolerance in meters (Default: 5)</summary>
			internal double ForwardTolerance;
			/// <summary>The backwards stop tolerance in meters (Default: 5)</summary>
			internal double BackwardTolerance;
			/// <summary>The number of cars this stop point applies to, or 0 for all</summary>
			internal int Cars;
		}

		/// <summary>Defines the possible safety system to be used (Only currently applies to the default JA safety systems)</summary>
		internal enum SafetySystem
		{
			/// <summary>Any available safety system should be used (Either that from the previous station if defined or NONE)</summary>
			Any = -1,
			/// <summary>ATS should be used- The track is NOT fitted with ATC</summary>
			Ats = 0,
			/// <summary>ATC should be used</summary>
			Atc = 1
		}
		internal enum StationStopMode
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
		internal enum StationType
		{
			Normal = 0,
			ChangeEnds = 1,
			Terminal = 2,
			RequestStop = 3
		}

		/// <summary>Defines a station</summary>
		internal struct Station
		{
			/// <summary>The textual name of the station displayed in-game</summary>
			internal string Name;
			/// <summary>The expected arrival time at this station, expressed in seconds after midnight on the first day</summary>
			internal double ArrivalTime;
			/// <summary>The sound buffer to be played when the train arrives at this station</summary>
			internal Sounds.SoundBuffer ArrivalSoundBuffer;
			/// <summary>The expected departure time from this station, expressed in seconds after midnight on the first day</summary>
			internal double DepartureTime;
			/// <summary>The sound buffer to be played before the doors close pre-departure</summary>
			internal Sounds.SoundBuffer DepartureSoundBuffer;
			/// <summary>The minimum time in seconds trains must stop for at this station</summary>
			internal double StopTime;
			internal Vector3 SoundOrigin;
			/// <summary>Defines which trains will stop at this station</summary>
			internal StationStopMode StopMode;
			/// <summary>The type of this station</summary>
			internal StationType StationType;
			/// <summary>Whether the signal associated with this station is held at red until the scheduled departure time</summary>
			internal bool ForceStopSignal;
			/// <summary>Whether the left doors should open</summary>
			internal bool OpenLeftDoors;
			/// <summary>Whether the right doors should open</summary>
			internal bool OpenRightDoors;
			/// <summary>The safety system in use from this station onwards</summary>
			internal SafetySystem SafetySystem;
			/// <summary>The available stop points for this station</summary>
			internal StationStop[] Stops;
			/// <summary>The ratio of passengers at this station (100 is a standard, fully loaded train)</summary>
			internal double PassengerRatio;
			/// <summary>The timetable to be shown from this point onwards (Daytime)</summary>
			internal Textures.Texture TimetableDaytimeTexture;
			/// <summary>The timetable to be shown from this point onwards (Nighttime)</summary>
			internal Textures.Texture TimetableNighttimeTexture;
			internal double DefaultTrackPosition;
		}
		internal static Station[] Stations = new Station[] { };

		/// <summary>The name of the initial station on game startup, if set via command-line arguments</summary>
		internal static string InitialStationName;
		/// <summary>The start time at the initial station, if set via command-line arguments</summary>
		internal static double InitialStationTime = -1;

		/// <summary>Gets the index of the stop corresponding to the train's number of cars</summary>
		/// <param name="StationIndex">The index of the station in the stations array</param>
		/// <param name="Cars">The number of cars the train has</param>
		internal static int GetStopIndex(int StationIndex, int Cars)
		{
			int j = -1;
			for (int i = Stations[StationIndex].Stops.Length - 1; i >= 0; i--)
			{
				if (Cars <= Stations[StationIndex].Stops[i].Cars | Stations[StationIndex].Stops[i].Cars == 0)
				{
					j = i;
				}
			}
			if (j == -1)
			{
				return Stations[StationIndex].Stops.Length - 1;
			}
			return j;
		}
		/// <summary>Indicates whether the player's train stops at a station.</summary>
		internal static bool PlayerStopsAtStation(int StationIndex)
		{
			return Stations[StationIndex].StopMode == StationStopMode.AllStop | Stations[StationIndex].StopMode == StationStopMode.PlayerStop | Stations[StationIndex].StopMode == StationStopMode.PlayerRequestStop | Stations[StationIndex].StopMode == StationStopMode.AllRequestStop;
		}
		/// <summary>Indicates whether a train stops at a station.</summary>
		internal static bool StopsAtStation(int StationIndex, TrainManager.Train Train)
		{
			if (Train == TrainManager.PlayerTrain)
			{
				return Stations[StationIndex].StopMode == StationStopMode.AllStop | Stations[StationIndex].StopMode == StationStopMode.PlayerStop | Stations[StationIndex].StopMode == StationStopMode.PlayerRequestStop | Stations[StationIndex].StopMode == StationStopMode.AllRequestStop;
			}
			return Stations[StationIndex].StopMode == StationStopMode.AllStop | Stations[StationIndex].StopMode == StationStopMode.PlayerPass | Stations[StationIndex].StopMode == StationStopMode.AllRequestStop;
		}
	}
}
