using OpenBveApi.Trains;
using SoundManager;

namespace TrainManager.BrakeSystems
{
	/// <summary>An air compressor</summary>
	public class Compressor
	{
		/// <summary>Whether this compressor is currently active</summary>
		private bool Enabled;
		/// <summary>The compression rate in Pa/s</summary>
		private readonly double Rate;
		/// <summary>The sound played when the compressor loop starts</summary>
		public CarSound StartSound;
		/// <summary>The sound played whilst the compressor is running</summary>
		public CarSound LoopSound;
		/// <summary>The sound played when the compressor loop stops</summary>
		public CarSound EndSound;
		/// <summary>Whether the sound loop has started</summary>
		private bool LoopStarted;
		/// <summary>Stores the time at which the compressor started</summary>
		private double TimeStarted;
		/// <summary>Holds the reference to the main reservoir</summary>
		private readonly MainReservoir mainReservoir;
		/// <summary>Holds the reference to the car</summary>
		private readonly AbstractCar baseCar;

		public Compressor(double rate, MainReservoir reservoir, AbstractCar car)
		{
			Rate = rate;
			Enabled = false;
			StartSound = new CarSound();
			LoopSound = new CarSound();
			EndSound = new CarSound();
			mainReservoir = reservoir;
			baseCar = car;
		}

		public void Update(double TimeElapsed)
		{
			if (Enabled)
			{
				if (mainReservoir.CurrentPressure > mainReservoir.MaximumPressure)
				{
					Enabled = false;
					LoopStarted = false;
					EndSound.Play(baseCar, false);
					LoopSound.Stop();
				}
				else
				{
					mainReservoir.CurrentPressure += Rate * TimeElapsed;
					if (!LoopStarted)
					{
						if ((StartSound.Buffer == null && TrainManagerBase.currentHost.InGameTime > TimeStarted + 5.0) || (StartSound.Buffer != null && !StartSound.IsPlaying))
						{
							/*
							 * If no start sound, assume a run-up time of 5s for the compressor
							 * before the sound loop starts
							 */
							LoopStarted = true;
							LoopSound.Play(baseCar, true);
						}
					}
				}
			}
			else
			{
				if (mainReservoir.CurrentPressure < mainReservoir.MinimumPressure)
				{
					Enabled = true;
					TimeStarted = TrainManagerBase.currentHost.InGameTime;
					StartSound.Play(baseCar, false);
				}
			}
		}
	}
}
