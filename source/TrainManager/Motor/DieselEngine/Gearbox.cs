using SoundManager;

namespace TrainManager.Motor
{
	public class Gearbox : AbstractComponent
	{
		/// <summary>The list of available gears</summary>
		internal readonly Gear[] Gears;
		/// <summary>The current gear</summary>
		internal int CurrentGear;
		/// <summary>The sound played when the gear is increased</summary>
		internal CarSound GearUpSound;
		/// <summary>The sound played when the gear is decreased</summary>
		internal CarSound GearDownSound;
		/// <summary>The sound played when the gearbox returns to neutral</summary>
		internal CarSound NeutralSound;
		public Gearbox(TractionModel engine, Gear[] gears) : base(engine)
		{
			Gears = gears;
			CurrentGear = 0;
		}

		public void GearUp()
		{
			if (CurrentGear < Gears.Length - 1)
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
