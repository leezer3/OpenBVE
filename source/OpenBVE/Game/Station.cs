using OpenBveApi.Math;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenBve.SignalManager;

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
		
		internal class Station : OpenBveApi.Runtime.Station
		{
			/// <summary>The ratio of passengers at this station (100 is a standard, fully loaded train)</summary>
			internal double PassengerRatio;
			/// <summary>The timetable to be shown from this point onwards (Daytime)</summary>
			internal Texture TimetableDaytimeTexture;
			/// <summary>The timetable to be shown from this point onwards (Nighttime)</summary>
			internal Texture TimetableNighttimeTexture;
			/// <summary>The origin vector for the arrival and departure sounds </summary>
			internal Vector3 SoundOrigin;
			/// <summary>The sound buffer to be played when the train arrives at this station</summary>
			internal OpenBveApi.Sounds.SoundHandle ArrivalSoundBuffer;
			/// <summary>The sound buffer to be played before the doors close pre-departure</summary>
			internal OpenBveApi.Sounds.SoundHandle DepartureSoundBuffer;
			/// <summary>The safety system in use from this station onwards</summary>
			internal SafetySystem SafetySystem;
			/// <summary>The available stop points for this station</summary>
			internal StationStop[] Stops;

			/// <summary>Gets the index of the stop corresponding to the train's number of cars</summary>
			/// <param name="Cars">The number of cars the train has</param>
			internal int GetStopIndex(int Cars)
			{
				int j = -1;
				for (int i = Stops.Length - 1; i >= 0; i--)
				{
					if (Cars <= Stops[i].Cars | Stops[i].Cars == 0)
					{
						j = i;
					}
				}
				if (j == -1)
				{
					return Stops.Length - 1;
				}
				return j;
			}
		}
		internal static Station[] Stations = new Station[] { };

		/// <summary>The name of the initial station on game startup, if set via command-line arguments</summary>
		internal static string InitialStationName;
		/// <summary>The start time at the initial station, if set via command-line arguments</summary>
		internal static double InitialStationTime = -1;

		/// <summary>Indicates whether the specified train stops at a station.</summary>
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
