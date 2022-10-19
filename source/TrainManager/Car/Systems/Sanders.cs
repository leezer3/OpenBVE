using SoundManager;

namespace TrainManager.Car.Systems
{
	public class Sanders : AbstractReAdhesionDevice
	{
		/// <summary>The sanding rate</summary>
		public double SandingRate;
		/// <summary>The level of sand in the sandbox</summary>
		public double SandLevel;
		/// <summary>The application time available before the sander cuts off</summary>
		public double ApplicationTime;
		/// <summary>The time required before activation if using automatic mode</summary>
		public double ActivationTime;
		/// <summary>If a number of shots is available, the number remaining</summary>
		public int NumberOfShots;
		/// <summary>Whether the sanders are currently active</summary>
		public bool Active;
		/// <summary>The type of sanders</summary>
		public readonly SandersType Type;
		/// <summary>The sound played when the sanders are activated</summary>
		public CarSound ActivationSound;
		/// <summary>The sound played when the sanders are deactivated</summary>
		public CarSound DeActivationSound;
		/// <summary>The sound loop played when the sanders are active</summary>
		public CarSound LoopSound;
		/// <summary>The sound played when the sand is emptied</summary>
		public CarSound EmptySound;
		/// <summary>The sound played when activation is attempted whilst empty</summary>
		public CarSound EmptyActivationSound;

		private bool emptied = false;
		private double timer;
		public Sanders(CarBase car, SandersType type) : base(car)
		{
			Type = type;
		}

		public override void Update(double timeElapsed)
		{
			if (Type == SandersType.NotFitted)
			{
				return;
			}

			if (Active)
			{
				if ((ActivationSound != null && !ActivationSound.IsPlaying) || ActivationSound == null)
				{
					if (LoopSound != null)
					{
						LoopSound.Play(Car, true);
					}
				}
			}

			if (SandLevel <= 0)
			{
				SandLevel = 0;
				if (SandingRate != 0)
				{
					Active = false;
					if (!emptied)
					{
						if (EmptySound != null)
						{
							EmptySound.Play(Car, false);
						}

						emptied = true;
					}
				}
			}
			else
			{
				if (SandingRate > 0 && SandingRate < double.MaxValue)
				{
					SandLevel -= SandingRate * timeElapsed;
				}
				
			}
			switch (Type)
			{
				case SandersType.Automatic:
					if (Car.FrontAxle.CurrentWheelSlip)
					{
						timer += timeElapsed;
						if (timer > ActivationTime && !Active)
						{
							Toggle();
						}
					}
					else
					{
						timer = 0;
						if (Active)
						{
							Toggle();
						}
					}
					break;
				case SandersType.NumberOfShots:
					if (Active)
					{
						timer -= timeElapsed;
						if (timer <= 0)
						{
							Toggle();
						}
					}
					
					break;
			}
		}

		public void Toggle()
		{
			if (Type == SandersType.NotFitted)
			{
				return;
			}

			if (emptied)
			{
				if (Type != SandersType.Automatic && EmptyActivationSound != null)
				{
					// Assume that whatever is controlling the automatic activation has the brains not to do so
					// when the sand is empty
					EmptyActivationSound.Play(Car, false);
				}
				return;
			}
			if (Active)
			{
				if (Type == SandersType.NumberOfShots && timer > 0)
				{
					// Can't cancel shot based
					return;
				}
				Active = false;
				if (DeActivationSound != null)
				{
					DeActivationSound.Play(Car, false);	
				}
			}
			else
			{
				if (Type == SandersType.NumberOfShots)
				{
					if (NumberOfShots <= 0)
					{
						return;
					}
					NumberOfShots--;
					timer = ApplicationTime;
				}
				Active = true;
				if (ActivationSound != null)
				{
					ActivationSound.Play(Car, false);	
				}
			}
		}
	}
}
