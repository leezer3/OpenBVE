using System.Collections.Generic;
using SoundManager;

namespace OpenBve
{
	/// <summary>Represents a train windscreen</summary>
	internal class Windscreen
	{
		/// <summary>Whether currently raining</summary>
		internal bool CurrentlyRaining
		{
			get
			{
				return RainIntensity != 0;
			}
		}

		/// <summary>The current rain intensity</summary>
		internal int RainIntensity
		{
			get
			{
				if (legacyRainEvents)
				{
					return rainIntensity;
				}
				return Car.FrontAxle.Follower.RainIntensity;
			}
		}

		private int rainIntensity;
		private bool legacyRainEvents;
		/// <summary>The raindrop array</summary>
		internal bool[] RainDrops;
		/// <summary>The sound played when a raindrop hits the windscreen</summary>
		internal CarSound DropSound = new CarSound();
		/// <summary>The windscreen wipers</summary>
		internal WindscreenWiper Wipers;

		internal readonly TrainManager.Car Car;
		
		private double dropTimer;
		internal int currentDrops;

		internal Windscreen(int numberOfDrops, TrainManager.Car car)
		{
			RainDrops = new bool[numberOfDrops];
			currentDrops = 0;
			Car = car;
		}

		internal void SetRainIntensity(int intensity)
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

		internal void Update(double TimeElapsed)
		{
			if (RainDrops == null || RainDrops.Length == 0)
			{
				return;
			}

			if (CurrentlyRaining)
			{
				int nextDrop = PickDrop();
				dropTimer += TimeElapsed * 10;
				var dev = (int)(0.4 * 200 / RainIntensity);
				int dropInterval = 200 / RainIntensity + Program.RandomNumberGenerator.Next(dev, dev * 2);
				if (dropTimer > dropInterval)
				{
					if (nextDrop != -1)
					{
						RainDrops[nextDrop] = true;
						Program.Sounds.PlayCarSound(DropSound, 1.0, 1.0, Car, false);
						currentDrops++;
					}
					dropTimer = 0.0;
				}
			}
			Wipers.Update(TimeElapsed);
			
		}

		private int PickDrop()
		{
			List<int> availableDrops = new List<int>();
			for (int i = 0; i < RainDrops.Length; i++)
			{
				if (RainDrops[i] == false)
				{
					availableDrops.Add(i);
				}
			}
			return availableDrops.Count != 0 ? availableDrops[Program.RandomNumberGenerator.Next(availableDrops.Count)] : -1;
		}
	}
}
