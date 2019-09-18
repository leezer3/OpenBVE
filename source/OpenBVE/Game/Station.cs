using OpenBve.RouteManager;
using OpenBveApi.Runtime;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>The name of the initial station on game startup, if set via command-line arguments</summary>
		internal static string InitialStationName;
		/// <summary>The start time at the initial station, if set via command-line arguments</summary>
		internal static double InitialStationTime = -1;

		/// <summary>Indicates whether the specified train stops at a station.</summary>
		internal static bool StopsAtStation(int StationIndex, TrainManager.Train Train)
		{
			if (Train.IsPlayerTrain)
			{
				return CurrentRoute.Stations[StationIndex].StopMode == StationStopMode.AllStop | CurrentRoute.Stations[StationIndex].StopMode == StationStopMode.PlayerStop | CurrentRoute.Stations[StationIndex].StopMode == StationStopMode.PlayerRequestStop | CurrentRoute.Stations[StationIndex].StopMode == StationStopMode.AllRequestStop;
			}
			return CurrentRoute.Stations[StationIndex].StopMode == StationStopMode.AllStop | CurrentRoute.Stations[StationIndex].StopMode == StationStopMode.PlayerPass | CurrentRoute.Stations[StationIndex].StopMode == StationStopMode.AllRequestStop;
		}
	}
}
