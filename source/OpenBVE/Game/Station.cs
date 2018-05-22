using OpenBveApi.Math;
using OpenBveApi.Runtime;

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
		
		internal class Station : OpenBveApi.Runtime.Station
		{
			/// <summary>The ratio of passengers at this station (100 is a standard, fully loaded train)</summary>
			internal double PassengerRatio;
			/// <summary>The timetable to be shown from this point onwards (Daytime)</summary>
			internal Textures.Texture TimetableDaytimeTexture;
			/// <summary>The timetable to be shown from this point onwards (Nighttime)</summary>
			internal Textures.Texture TimetableNighttimeTexture;
			/// <summary>The origin vector for the arrival and departure sounds </summary>
			internal Vector3 SoundOrigin;
			/// <summary>The sound buffer to be played when the train arrives at this station</summary>
			internal Sounds.SoundBuffer ArrivalSoundBuffer;
			/// <summary>The sound buffer to be played before the doors close pre-departure</summary>
			internal Sounds.SoundBuffer DepartureSoundBuffer;
			/// <summary>The safety system in use from this station onwards</summary>
			internal SafetySystem SafetySystem;
			/// <summary>The available stop points for this station</summary>
			internal StationStop[] Stops;
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
