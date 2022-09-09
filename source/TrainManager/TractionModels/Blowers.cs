using SoundManager;
using TrainManager.TractionModels.Steam;

namespace TrainManager.TractionModels
{
	public class Blowers
	{
		/// <summary>Holds a reference to the base engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>Whether the blowers are active</summary>
		public bool Active;
		/// <summary>The sound played when the blowers are activated</summary>
		public CarSound StartSound;
		/// <summary>Whether the start sound has been played</summary>
		private bool startSoundPlayed;
		/// <summary>The sound played when the blowers are active and working</summary>
		public CarSound LoopSound;
		/// <summary>The sound played when the blowers are deactivated</summary>
		public CarSound StopSound;
		/// <summary>The temperature increase ratio</summary>
		public readonly double Ratio;
		/// <summary>The amount of steam used</summary>
		private readonly double SteamUse;

		public Blowers(SteamEngine engine, double ratio, double steamUse)
		{
			Engine = engine;
			Ratio = ratio;
			SteamUse = steamUse;
		}

		public void Update(double timeElapsed)
		{
			if (Active)
			{
				Engine.Boiler.SteamPressure -= SteamUse * timeElapsed;
				if (!startSoundPlayed)
				{
					if (StartSound != null)
					{
						StartSound.Play(Engine.Car, false);
					}
					
					startSoundPlayed = true;
				}
				else if(!StartSound.IsPlaying)
				{
					if (LoopSound != null)
					{
						LoopSound.Play(Engine.Car, true);
					}
				}
			}
			else
			{
				if (LoopSound != null)
				{
					LoopSound.Stop();
					
				}
				if (startSoundPlayed && StopSound != null)
				{
					StopSound.Play(Engine.Car, false);
				}
				startSoundPlayed = false;
			}
		}
	}
}
