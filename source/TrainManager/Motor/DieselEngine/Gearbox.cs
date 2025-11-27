using SoundManager;

namespace TrainManager.Motor
{
	public class Gearbox : AbstractComponent
	{
		/// <summary>The operation mode of the gearbox</summary>
		public GearboxOperation OperationMode;
		/// <summary>The list of available gears</summary>
		internal readonly Gear[] Gears;
		/// <summary>The current gear</summary>
		public int CurrentGear;
		/// <summary>The sound played when the gear is increased</summary>
		public CarSound GearUpSound;
		/// <summary>The sound played when the gear is decreased</summary>
		public CarSound GearDownSound;
		/// <summary>The sound played when the gearbox returns to neutral</summary>
		internal CarSound NeutralSound;
		/// <summary>The maximum speed attainable in the current gear</summary>
		public double MaximumGearSpeed
		{
			get
			{
				if (CurrentGear < 1)
				{
					return 0;
				}

				return Gears[CurrentGear - 1].MaximumSpeed;
			}
		}

		/// <summary>The maximum speed attainable in the current gear</summary>
		public double PreviousMaximumGearSpeed
		{
			get
			{
				if (CurrentGear < 2)
				{
					return 0;
				}

				return Gears[CurrentGear - 2].MaximumSpeed;
			}
		}
		public Gearbox(TractionModel engine, Gear[] gears, GearboxOperation operation) : base(engine)
		{
			Gears = gears;
			CurrentGear = 0;
			OperationMode = operation;
		}

		public void GearUp()
		{
			if (CurrentGear < Gears.Length)
			{
				CurrentGear++;
				if (GearUpSound != null)
				{
					GearUpSound.Play(1.0,1.0, baseEngine.BaseCar, false);
				}
			}
		}

		public void GearDown()
		{
			if (CurrentGear > 0)
			{
				CurrentGear--;
				if (CurrentGear == 0)
				{
					if (NeutralSound != null)
					{
						NeutralSound.Play(1.0, 1.0, baseEngine.BaseCar, false);
					}
					else if (GearDownSound != null)
					{
						GearDownSound.Play(1.0, 1.0, baseEngine.BaseCar, false);
					}
				}
				else
				{
					if (GearDownSound != null)
					{
						GearDownSound.Play(1.0, 1.0, baseEngine.BaseCar, false);
					}
				}
			}
		}
	}
}
