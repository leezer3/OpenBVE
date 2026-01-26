using System;
using System.Collections.Generic;
using SoundManager;

namespace TrainManager.Car
{
	/// <summary>Represents a train windscreen</summary>
	public class Windscreen
	{
		/// <summary>Whether currently raining</summary>
		internal bool CurrentlyRaining => Intensity != 0;

		/// <summary>The current rain intensity</summary>
		internal int Intensity
		{
			get
			{
				if (legacyRainEvents)
				{
					return rainIntensity;
				}
				//The weather intensity returns the maximum of the rain and snow intensities
				return Math.Max(Car.FrontAxle.Follower.RainIntensity, Car.FrontAxle.Follower.SnowIntensity);
			}
		}
		/// <summary>Backing property storing the current legacy rain intensity</summary>
		private int rainIntensity;
		/// <summary>Whether legacy rain events are in use</summary>
		public bool legacyRainEvents;
		/// <summary>The raindrop array</summary>
		public Raindrop[] RainDrops;
		/// <summary>The sound played when a raindrop hits the windscreen</summary>
		public CarSound DropSound = new CarSound();
		/// <summary>The windscreen wipers</summary>
		public WindscreenWiper Wipers;
		/// <summary>Holds a reference to the base car</summary>
		internal readonly CarBase Car;
		/// <summary>The median shortest life of a drop on the windscreen before it dries</summary>
		internal readonly double DropLife;
		private double dropTimer;
		/// <summary>The number of drops currently visible on the windscreen</summary>
		public int currentDrops;

		public Windscreen(int numberOfDrops, double dropLife, CarBase car)
		{
			RainDrops = new Raindrop[numberOfDrops];
			DropLife = dropLife;
			currentDrops = 0;
			Car = car;
		}

		/// <summary>Sets the legacy rain intensity</summary>
		public void SetRainIntensity(int intensity)
		{
			//Must assume that the .rain command and legacy rain are not mixed in a routefile
			legacyRainEvents = true;
			if (intensity < 0)
			{
				intensity = 0;
			}
			if (intensity > 100)
			{
				intensity = 100;
			}
			rainIntensity = intensity;
		}

		/// <summary>Updates the windscreen</summary>
		/// <param name="TimeElapsed">The time elapsed since the previous call to this method</param>
		public void Update(double TimeElapsed)
		{
			Wipers.Update(TimeElapsed);
			if (RainDrops == null || RainDrops.Length == 0)
			{
				return;
			}

			if (CurrentlyRaining)
			{
				int nextDrop = PickDrop();
				dropTimer += TimeElapsed * 1000;
				var dev = (int)(0.4 * 2000 / Intensity);
				int dropInterval =  2000 / Intensity + TrainManagerBase.RandomNumberGenerator.Next(dev, dev * 2);
				if (dropTimer > dropInterval)
				{
					if (nextDrop != -1)
					{
						if (!RainDrops[nextDrop].Visible)
						{
							currentDrops++;
						}
						RainDrops[nextDrop].Visible = true;
						if (!legacyRainEvents)
						{
							if (TrainManagerBase.RandomNumberGenerator.Next(100) < Car.FrontAxle.Follower.SnowIntensity || Car.FrontAxle.Follower.RainIntensity == 0)
							{
								//Either we've met the snow probability roll (mixed snow and rain) or not raining
								RainDrops[nextDrop].IsSnowFlake = true;
							}
						}
						RainDrops[nextDrop].RemainingLife = TrainManagerBase.RandomNumberGenerator.NextDouble() * DropLife;
					}
					//We want to play the drop sound even if all drops are currently visible (e.g. the wipers are off and it's still raining)
					DropSound.Play(Car, false);
					dropTimer = 0.0;
				}
			}

			for (int i = 0; i < RainDrops.Length; i++)
			{
				RainDrops[i].RemainingLife -= TimeElapsed;
				if (RainDrops[i].RemainingLife <= 0 && RainDrops[i].Visible)
				{
					RainDrops[i].Visible = false;
					RainDrops[i].IsSnowFlake = false;
					currentDrops--;
					RainDrops[i].RemainingLife = 0.5 * TrainManagerBase.RandomNumberGenerator.NextDouble() * DropLife;
				}
			}
		}

		private int PickDrop()
		{
			List<int> availableDrops = new List<int>();
			for (int i = 0; i < RainDrops.Length; i++)
			{
				if (RainDrops[i].Visible == false && RainDrops[i].RemainingLife <= 0)
				{
					availableDrops.Add(i);
				}
			}
			return availableDrops.Count != 0 ? availableDrops[TrainManagerBase.RandomNumberGenerator.Next(availableDrops.Count)] : -1;
		}
	}
}
