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
				return Program.CurrentRoute.Stations[StationIndex].StopMode == StationStopMode.AllStop | Program.CurrentRoute.Stations[StationIndex].StopMode == StationStopMode.PlayerStop | Program.CurrentRoute.Stations[StationIndex].StopMode == StationStopMode.PlayerRequestStop | Program.CurrentRoute.Stations[StationIndex].StopMode == StationStopMode.AllRequestStop;
			}
			return Program.CurrentRoute.Stations[StationIndex].StopMode == StationStopMode.AllStop | Program.CurrentRoute.Stations[StationIndex].StopMode == StationStopMode.PlayerPass | Program.CurrentRoute.Stations[StationIndex].StopMode == StationStopMode.AllRequestStop;
		}
	}
}
