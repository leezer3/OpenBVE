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
		internal bool legacyRainEvents;
		/// <summary>The raindrop array</summary>
		internal Raindrop[] RainDrops;
		/// <summary>The sound played when a raindrop hits the windscreen</summary>
		internal CarSound DropSound = new CarSound();
		/// <summary>The windscreen wipers</summary>
		internal WindscreenWiper Wipers;
		/// <summary>Holds a reference to the base car</summary>
		internal readonly TrainManager.Car Car;
		/// <summary>The median shortest life of a drop on the windscreen before it dries</summary>
		internal readonly double DropLife;
		private double dropTimer;
		internal int currentDrops;

		internal Windscreen(int numberOfDrops, double dropLife, TrainManager.Car car)
		{
			RainDrops = new Raindrop[numberOfDrops];
			DropLife = dropLife;
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
				dropTimer += TimeElapsed * 1000;
				var dev = (int)(0.4 * 2000 / RainIntensity);
				int dropInterval =  2000 / RainIntensity + Program.RandomNumberGenerator.Next(dev, dev * 2);
				if (dropTimer > dropInterval)
				{
					if (nextDrop != -1)
					{
						if (!RainDrops[nextDrop].Visible)
						{
							currentDrops++;
						}
						RainDrops[nextDrop].Visible = true;
						RainDrops[nextDrop].RemainingLife = Program.RandomNumberGenerator.NextDouble() * DropLife;
					}
					//We want to play the drop sound even if all drops are currently visible (e.g. the wipers are off and it's still raining)
					Program.Sounds.PlayCarSound(DropSound, 1.0, 1.0, Car, false);
					dropTimer = 0.0;
				}
			}

			for (int i = 0; i < RainDrops.Length; i++)
			{
				RainDrops[i].RemainingLife -= TimeElapsed;
				if (RainDrops[i].RemainingLife <= 0 && RainDrops[i].Visible)
				{
					RainDrops[i].Visible = false;
					currentDrops--;
					RainDrops[i].RemainingLife = 0.5 * Program.RandomNumberGenerator.NextDouble() * DropLife;
				}
			}
			Wipers.Update(TimeElapsed);
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
			return availableDrops.Count != 0 ? availableDrops[Program.RandomNumberGenerator.Next(availableDrops.Count)] : -1;
		}
	}
}
