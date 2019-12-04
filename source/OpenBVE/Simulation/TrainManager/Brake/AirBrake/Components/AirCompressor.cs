using OpenBveApi.Trains;
using SoundManager;

namespace OpenBve.BrakeSystems
{
	/// <summary>An air compressor</summary>
	class Compressor
	{
		/// <summary>Whether this compressor is currently active</summary>
		private bool Enabled;
		/// <summary>The compression rate in Pa/s</summary>
		private readonly double Rate;
		/// <summary>The sound played when the compressor loop starts</summary>
		internal CarSound StartSound;
		/// <summary>The sound played whilst the compressor is running</summary>
		internal CarSound LoopSound;
		/// <summary>The sound played when the compressor loop stops</summary>
		internal CarSound EndSound;
		/// <summary>Whether the sound loop has started</summary>
		private bool LoopStarted;
		/// <summary>Stores the time at which the compressor started</summary>
		private double TimeStarted;
		/// <summary>Holds the reference to the main reservoir</summary>
		private readonly MainReservoir mainReservoir;
		/// <summary>Holds the reference to the car</summary>
		private readonly AbstractCar baseCar;

		internal Compressor(double rate, MainReservoir reservoir, AbstractCar car)
		{
			Rate = rate;
			Enabled = false;
			StartSound = new CarSound();
			LoopSound = new CarSound();
			EndSound = new CarSound();
			mainReservoir = reservoir;
			baseCar = car;
		}

		internal void Update(double TimeElapsed)
		{
			if (Enabled)
			{
				if (mainReservoir.CurrentPressure > mainReservoir.MaximumPressure)
				{
					Enabled = false;
					LoopStarted = false;
					SoundBuffer buffer = EndSound.Buffer;
					if (buffer != null)
					{
						Program.Sounds.PlaySound(buffer, 1.0, 1.0, EndSound.Position, baseCar, false);
					}

					buffer = LoopSound.Buffer;
					if (buffer != null)
					{
						LoopSound.Stop();
					}
				}
				else
				{
					mainReservoir.CurrentPressure += Rate * TimeElapsed;
					if (!LoopStarted && Program.CurrentRoute.SecondsSinceMidnight > TimeStarted + 5.0)
					{
						LoopStarted = true;
						SoundBuffer buffer = LoopSound.Buffer;
						if (buffer != null)
						{
							LoopSound.Source = Program.Sounds.PlaySound(buffer, 1.0, 1.0, LoopSound.Position, baseCar, true);
						}
					}
				}
			}
			else
			{
				if (mainReservoir.CurrentPressure < mainReservoir.MinimumPressure)
				{
					Enabled = true;
					TimeStarted = Program.CurrentRoute.SecondsSinceMidnight;
					SoundBuffer buffer = StartSound.Buffer;
					if (buffer != null)
					{
						Program.Sounds.PlaySound(buffer, 1.0, 1.0, StartSound.Position, baseCar, false);
					}
				}
			}
		}
	}
}
