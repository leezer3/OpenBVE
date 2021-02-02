using OpenBveApi.Trains;
using SoundManager;
using TrainManager;

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
					EndSound.Play(1.0, 1.0, baseCar, false);
					LoopSound.Stop();
				}
				else
				{
					mainReservoir.CurrentPressure += Rate * TimeElapsed;
					if (!LoopStarted && TrainManagerBase.currentHost.InGameTime > TimeStarted + 5.0)
					{
						LoopStarted = true;
						LoopSound.Play(1.0, 1.0, baseCar, true);
					}
				}
			}
			else
			{
				if (mainReservoir.CurrentPressure < mainReservoir.MinimumPressure)
				{
					Enabled = true;
					TimeStarted = TrainManagerBase.currentHost.InGameTime;
					StartSound.Play(1.0, 1.0, baseCar, false);
				}
			}
		}
	}
}
