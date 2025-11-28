using OpenBveApi.Math;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenBveApi.Trains;
using RouteManager2.SignalManager;

namespace RouteManager2.Stations
{
	/// <summary>The derived route station class</summary>
	public class RouteStation : Station
	{
		/// <summary>The ratio of passengers at this station (100 is a standard, fully loaded train)</summary>
		public double PassengerRatio;

		/// <summary>The timetable to be shown from this point onwards (Daytime)</summary>
		public Texture TimetableDaytimeTexture;

		/// <summary>The timetable to be shown from this point onwards (Nighttime)</summary>
		public Texture TimetableNighttimeTexture;

		/// <summary>The origin vector for the arrival and departure sounds </summary>
		public Vector3 SoundOrigin;

		/// <summary>The sound buffer to be played when the train arrives at this station</summary>
		public OpenBveApi.Sounds.SoundHandle ArrivalSoundBuffer;

		/// <summary>The sound buffer to be played before the doors close pre-departure</summary>
		public OpenBveApi.Sounds.SoundHandle DepartureSoundBuffer;

		/// <summary>The safety system in use from this station onwards</summary>
		public SafetySystem SafetySystem;

		/// <summary>The available stop points for this station</summary>
		public StationStop[] Stops;

		/// <summary>Whether this station has been announced with accessibility in use</summary>
		public bool AccessibilityAnnounced;

		/// <summary>Whether this station is a dummy station for signalling purposes</summary>
		/// <remarks>Used by some BVE2 / BVE4 routes in conjunction with forced redsignal</remarks>
		public bool Dummy;

		/// <summary>The key for this station</summary>
		public readonly string Key;

		/// <summary>Gets the index of the stop for the train</summary>
		/// <param name="train">The train</param>
		public int GetStopIndex(AbstractTrain train)
		{
			int j = -1;
			int allCars = -1;
			for (int i = Stops.Length - 1; i >= 0; i--)
			{
				if (train.NumberOfCars == Stops[i].Cars)
				{
					 // If we have found the specified number of cars, stop searching
					 return i;
				}
				if (Stops[i].Cars == 0)
				{
					allCars = j;
				}
				if (Stops[i].Cars != 0 && train.NumberOfCars < Stops[i].Cars)
				{
					/*
					 * The stop has greater than the specified number of cars (hence all cars will be platformed)
					 * or is an all car stop, we should continue searching, in case there is a better candidate.
					 */
					if (j != -1 && Stops[i].Cars > Stops[j].Cars)
					{
						// If we already have a candidate, and the new candidate has more cars, ignore it (we should assume that we want to stop as close to the facilities as possible)
						continue;
					}
					j = i;
				}
			}

			if (allCars != -1)
			{
				// No specific stop point, but an all car stop- Return this
				return allCars;
			}
			if (j != -1)
			{
				// No specific stop point or all-car stop, so return the stop point with the smallest number of cars above
				return j;
			}
			// Return the final stop point, as nothing better :/
			return Stops.Length -1;
		}

		/// <summary>Gets the stop position corresponding to the train</summary>
		/// <param name="train">The train</param>
		public double GetStopPosition(AbstractTrain train)
		{
			int n = GetStopIndex(train);
			return Stops.Length > 0 ? Stops[n].TrackPosition : DefaultTrackPosition;
		}

		/// <summary>Indicates whether the specified train stops at this station.</summary>
		public bool StopsHere(AbstractTrain Train)
		{
			if (Train.IsPlayerTrain)
			{
				return StopMode == StationStopMode.AllStop | StopMode == StationStopMode.PlayerStop | StopMode == StationStopMode.PlayerRequestStop | StopMode == StationStopMode.AllRequestStop;
			}
			return StopMode == StationStopMode.AllStop | StopMode == StationStopMode.PlayerPass | StopMode == StationStopMode.AllRequestStop;
		}

		public RouteStation(string key = "")
		{
			Key = key;
		}
	}
}
