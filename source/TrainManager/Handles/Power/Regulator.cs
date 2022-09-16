using TrainManager.Trains;

namespace TrainManager.Handles
{
	public class Regulator : AbstractHandle
	{
		/*
		 * Very similar to the PowerHandle, but less BVE related stuff, so use a dedicated class
		 * TODO: Should the current PowerHandle be renamed BVEPowerHandle or something?
		 */
		public Regulator(TrainBase Train) : base(Train)
		{
			MaximumNotch = 100;
			MaximumDriverNotch = 100;
		}

		public override void Update()
		{
			// no support for delayed changes
			Actual = Safety;
			// Spring return
			if (SpringType != SpringType.Unsprung && SpringTime > 0)
			{
				if (TrainManagerBase.currentHost.InGameTime > SpringTimer)
				{
					ApplyState(-1, true);
				}
			}
		}

		public override void ApplyState(int newState, bool relativeChange, bool isOverMaxDriverNotch = false)
		{
			// determine notch
			int p = relativeChange ? newState + Driver : newState;
			if (p < 0)
			{
				p = 0;
			}
			else if (p > MaximumNotch)
			{
				p = MaximumNotch;
			}
			if (!isOverMaxDriverNotch && p > MaximumDriverNotch)
			{
				p = MaximumDriverNotch;
			}

			// power sound
			if (p < Driver)
			{
				if (p > 0)
				{
					// down (not min)
					if (Driver - p > 2 | ContinuousMovement && DecreaseFast.Buffer != null)
					{
						DecreaseFast.Play(baseTrain.Cars[baseTrain.DriverCar], false);
					}
					else
					{
						Decrease.Play(baseTrain.Cars[baseTrain.DriverCar], false);
					}
				}
				else
				{
					// min
					Min.Play(baseTrain.Cars[baseTrain.DriverCar], false);
				}
			}
			else if (p > Driver)
			{
				if (p < MaximumDriverNotch)
				{
					// up (not max)
					if (Driver - p > 2 | ContinuousMovement && IncreaseFast.Buffer != null)
					{
						IncreaseFast.Play(baseTrain.Cars[baseTrain.DriverCar], false);
					}
					else
					{
						Increase.Play(baseTrain.Cars[baseTrain.DriverCar], false);
					}
				}
				else
				{
					// max
					Max.Play(baseTrain.Cars[baseTrain.DriverCar], false);
				}
			}

			Driver = p;
			TrainManagerBase.currentHost.AddBlackBoxEntry();
			// plugin
			if (baseTrain.Plugin != null)
			{
				baseTrain.Plugin.UpdatePower();
				baseTrain.Plugin.UpdateBrake();
			}
		}
	}
}
