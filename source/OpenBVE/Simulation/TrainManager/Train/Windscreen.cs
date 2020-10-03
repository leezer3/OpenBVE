using System.Collections.Generic;
using SoundManager;

namespace OpenBve
{
	/// <summary>Represents a train windscreen</summary>
	internal class Windscreen
	{
		/// <summary>Whether currently raining</summary>
		internal bool CurrentlyRaining;
		/// <summary>The current rain intensity</summary>
		internal int RainIntensity;
		/// <summary>The raindrop array</summary>
		internal bool[] RainDrops;
		/// <summary>The sound played when a raindrop hits the windscreen</summary>
		internal CarSound DropSound;
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

		internal void Update(double TimeElapsed)
		{
			if (RainDrops == null || RainDrops.Length == 0)
			{
				return;
			}

			if (CurrentlyRaining)
			{
				int nextDrop = PickDrop();
				dropTimer += TimeElapsed;
				var dev = (int)(0.4 * 2000 / RainIntensity);
				int dropInterval = 2000 / RainIntensity + Program.RandomNumberGenerator.Next(dev, dev * 2);
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
