using SoundManager;

namespace TrainManager.TractionModels.Steam
{
	public class Blowers
	{
		/// <summary>Holds a reference to the base engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>Whether the blowers are active</summary>
		public bool Active
		{
			get => _Active;
			set
			{
				if (value)
				{
					if (!_Active)
					{
						if (StartSound != null)
						{
							StartSound.Play(Engine.Car, false);
						}
					}
				}
				else
				{
					if (_Active)
					{
						if (StopSound != null)
						{
							StopSound.Play(Engine.Car, false);
						}
					}
				}
				_Active = value;
			}
		}
		/// <summary>Backing property for Active</summary>
		private bool _Active;
		/// <summary>The sound played when the blowers are activated</summary>
		public CarSound StartSound;
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
				if(StartSound == null || !StartSound.IsPlaying && LoopSound != null)
				{
					LoopSound.Play(Engine.Car, true);
				}
			}
			else
			{
				if (LoopSound != null)
				{
					LoopSound.Stop();
				}
			}
		}
	}
}
