using SoundManager;

namespace TrainManager.TractionModels.Steam
{
	public class CylinderCocks
	{
		/// <summary>Holds a reference to the base engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>Whether the cylinder cocks are open</summary>
		public bool Open
		{
			get => _Open;
			set
			{
				if (value)
				{
					if (!_Open)
					{
						if (StartSound != null)
						{
							StartSound.Play(Engine.Car, false);
						}
					}
				}
				else
				{
					if (_Open)
					{
						if (StopSound != null)
						{
							StopSound.Play(Engine.Car, false);
						}
					}
				}
				_Open = value;
			}
		}
		/// <summary>Backing property for Open</summary>
		private bool _Open;
		/// <summary>The sound played when the cylinder cocks are opened</summary>
		public CarSound StartSound;
		/// <summary>The sound played when the cylinder cocks are open with the regulator closed</summary>
		public CarSound IdleLoopSound;
		/// <summary>The sound played when the cylinder cocks are open with the regulator open</summary>
		public CarSound LoopSound;
		/// <summary>The sound played when the cylinder cocks are closed</summary>
		public CarSound StopSound;
		/// <summary>The amount of steam used when open at max regulator</summary>
		private readonly double SteamUse;

		public CylinderCocks(SteamEngine engine, double steamUse)
		{
			Engine = engine;
			SteamUse = steamUse;
		}

		public void Update(double timeElapsed)
		{
			if (Open)
			{
				Engine.Boiler.SteamPressure -= SteamUse * Engine.Car.baseTrain.Handles.Power.Actual * Engine.Car.baseTrain.Handles.Power.MaximumNotch * timeElapsed;
				if (StartSound == null || !StartSound.IsPlaying)
				{
					if (Engine.Car.baseTrain.Handles.Power.Actual != 0)
					{
						if (IdleLoopSound != null)
						{
							IdleLoopSound.Stop();
						}

						if (LoopSound != null)
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

						if (IdleLoopSound != null)
						{
							IdleLoopSound.Play(Engine.Car, true);
						}
					}

				}
			}
			else
			{
				if (LoopSound != null)
				{
					LoopSound.Stop();

				}

				if (IdleLoopSound != null)
				{
					IdleLoopSound.Stop();
				}
			}
		}
	}
}
