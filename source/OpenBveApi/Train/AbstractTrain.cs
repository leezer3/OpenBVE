﻿namespace OpenBveApi.Trains
{
	/// <summary>An abstract train</summary>
	public abstract class AbstractTrain
	{
		/// <summary>The current state of the train</summary>
		public TrainState State;
		/// <summary>The current station state</summary>
		public TrainStopState StationState;
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
		/// <remarks>Default units are km/h</remarks>
		public double CurrentSpeed;
		/// <summary>Gets the track position of the front car</summary>
		public abstract double FrontCarTrackPosition();

		/// <summary>Gets the track position of the rear car</summary>
		public abstract double RearCarTrackPosition();

		/// <summary>Derails a car within the train</summary>
		/// <param name="CarIndex">The index of the car to derail</param>
		/// <param name="ElapsedTime">The frame time elapsed</param>
		public virtual void Derail(int CarIndex, double ElapsedTime)
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
	}
}
