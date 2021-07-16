using OpenBveApi.Routes;

namespace OpenBveApi.Trains
{
	/// <summary>An abstract train</summary>
	public abstract class AbstractTrain
	{
		/// <summary>The current state of the train</summary>
		public TrainState State;
		/// <summary>Holds the AI controlling the train if any</summary>
		public GeneralAI AI;
		/// <summary>The current station state</summary>
		public TrainStopState StationState;
		/// <summary>Defines whether the next stop is skipped</summary>
		public StopSkipMode NextStopSkipped = StopSkipMode.None;
		/// <summary>The car in which the driver's cab is located</summary>
		public int DriverCar;
		/// <summary>The next user-set destination</summary>
		public int Destination;
		/// <summary>The previous station at which the train called</summary>
		public int LastStation;
		/// <summary>The index of the signalling section that the train is currently in</summary>
		public int CurrentSectionIndex;
		/// <summary>The speed limit of the signalling section the train is currently in</summary>
		/// <remarks>Default units are km/h</remarks>
		public double CurrentSectionLimit;
		/// <summary>The route speed limts</summary>
		public double[] RouteLimits;
		/// <summary>The current route limit in effect</summary>
		public double CurrentRouteLimit;
		/// <summary>The current speed of the train (as an average of all cars)</summary>
		/// <remarks>Default units are m/s</remarks>
		public double CurrentSpeed;
		/// <summary>The index to the next station at which the train calls</summary>
		/// <remarks>If stationary at a timetabled station, this will return that station</remarks>
		public int Station;
		/// <summary>The timetable delta from the player train</summary>
		/// <remarks>Is negative for earlier trains, or negative for later trains</remarks>
		public double TimetableDelta;
		/// <summary>A string storing the absolute on-disk path to the current train folder</summary>
		public string TrainFolder;
		/// <summary>Gets the track position of the front car</summary>
		public abstract double FrontCarTrackPosition();

		/// <summary>Gets the track position of the rear car</summary>
		public abstract double RearCarTrackPosition();

		/// <summary>Returns true if this is the player driven train</summary>
		public virtual bool IsPlayerTrain
		{
			get
			{
				//An abstract train in and of itself cannot be the player train
				return false;
			}
		}

		/// <summary>Returns the number of cars in this train</summary>
		public virtual int NumberOfCars
		{
			get
			{
				return 0;
			}
		}

		/// <summary>Updates the train</summary>
		/// <param name="TimeElapsed">The time elapsed since the last call to update</param>
		public virtual void Update(double TimeElapsed)
		{

		}

		/// <summary>Derails a car within the train</summary>
		/// <param name="CarIndex">The index of the car to derail</param>
		/// <param name="ElapsedTime">The frame time elapsed</param>
		public virtual void Derail(int CarIndex, double ElapsedTime)
		{

		}

		/// <summary>Derails a car within the train</summary>
		/// <param name="Car">The car to derail</param>
		/// <param name="ElapsedTime">The frame time elapsed</param>
		public virtual void Derail(AbstractCar Car, double ElapsedTime)
		{

		}

		/// <summary>Call this method to reverse (flip) the entire train</summary>
		public virtual void Reverse()
		{

		}

		/// <summary>Disposes of the train and releases all resources</summary>
		public virtual void Dispose()
		{
		}

		/// <summary>Stops all sounds from this train</summary>
		public virtual void StopAllSounds()
		{

		}

		/// <summary>Is called when the train changes from one signalling section to another</summary>
		public virtual void SectionChange()
		{

		}

		/// <summary>Updates the safety systems on this train with data recieved from a beacon</summary>
		public virtual void UpdateBeacon(int transponderType, int sectionIndex, int optional)
		{

		}

		/// <summary>Is called when the train enters a station</summary>
		/// <param name="stationIndex">The index of the station</param>
		/// <param name="direction">The direction of travel</param>
		public virtual void EnterStation(int stationIndex, int direction)
		{

		}

		/// <summary>Is called when the train leaves a station</summary>
		/// <param name="stationIndex">The station index</param>
		/// <param name="direction">The direction of travel</param>
		public virtual void LeaveStation(int stationIndex, int direction)
		{

		}

		/// <summary>Called when a request stop is triggered</summary>
		/// <param name="stopRequest">The stop request to evaluate</param>
		public virtual void RequestStop(RequestStop stopRequest)
		{

		}

		/// <summary>Opens the train doors</summary>
		/// <param name="Left">Whether the left side is to open</param>
		/// <param name="Right">Whether the right side is to open</param>
		public virtual void OpenDoors(bool Left, bool Right)
		{

		}

		/// <summary>Closes the train doors</summary>
		/// <param name="Left">Whether the left side is to close</param>
		/// <param name="Right">Whether the right side is to close</param>
		public virtual void CloseDoors(bool Left, bool Right)
		{

		}

		/// <summary>Jumps the train to the specified station index</summary>
		/// <param name="StationIndex">The index of the station to jump to</param>
		public virtual void Jump(int StationIndex)
		{

		}
	}
}
