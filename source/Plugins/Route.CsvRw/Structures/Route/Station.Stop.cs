using System;
using OpenBveApi.Math;
using RouteManager2.Stations;

namespace OpenBve
{
	internal class Stop
	{
		internal Stop(double trackPosition, int stationIndex, int direction, double forwardTolerance, double backwardTolerance, int numberOfCars)
		{
			TrackPosition = trackPosition;
			StationIndex = stationIndex;
			Direction = direction;
			ForwardTolerance = forwardTolerance;
			BackwardTolerance = backwardTolerance;
			Cars = numberOfCars;
		}

		internal readonly double TrackPosition;
		private readonly int StationIndex;
		internal readonly int Direction;
		private readonly double ForwardTolerance;
		private readonly double BackwardTolerance;
		private readonly int Cars;

		internal void CreateEvent(ref RouteStation[] Stations, Vector3 Position, Vector3 Up, Vector3 Side)
		{
			int t = Stations[StationIndex].Stops.Length;
			Array.Resize(ref Stations[StationIndex].Stops, t + 1);
			Stations[StationIndex].Stops[t].TrackPosition = TrackPosition;
			Stations[StationIndex].Stops[t].ForwardTolerance = ForwardTolerance;
			Stations[StationIndex].Stops[t].BackwardTolerance = BackwardTolerance;
			Stations[StationIndex].Stops[t].Cars = Cars;
			double dx, dy = 2.0;
			if (Stations[StationIndex].OpenLeftDoors & !Stations[StationIndex].OpenRightDoors)
			{
				dx = -5.0;
			}
			else if (!Stations[StationIndex].OpenLeftDoors & Stations[StationIndex].OpenRightDoors)
			{
				dx = 5.0;
			}
			else
			{
				dx = 0.0;
			}

			Stations[StationIndex].SoundOrigin = Position + dx * Side + dy * Up;
		}
	}
}
