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

		/// <summary>Gets the index of the stop corresponding to the train's number of cars</summary>
		/// <param name="Cars">The number of cars the train has</param>
		public int GetStopIndex(int Cars)
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

		/// <summary>Indicates whether the specified train stops at this station.</summary>
		public bool StopsHere(AbstractTrain Train)
		{
			if (Train.IsPlayerTrain)
			{
				return StopMode == StationStopMode.AllStop | StopMode == StationStopMode.PlayerStop | StopMode == StationStopMode.PlayerRequestStop | StopMode == StationStopMode.AllRequestStop;
			}
			return StopMode == StationStopMode.AllStop | StopMode == StationStopMode.PlayerPass | StopMode == StationStopMode.AllRequestStop;
		}
	}
}
