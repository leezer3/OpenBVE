using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>A power handle</summary>
	public class PowerHandle : NotchedHandle
	{
		public PowerHandle(int max, int driverMax, double[] delayUp, double[] delayDown, TrainBase Train) : base(Train)
		{
			this.MaximumNotch = max;
			this.MaximumDriverNotch = driverMax;
			this.DelayUp = delayUp;
			this.DelayDown = delayDown;
		}

		public override void Update()
		{
			if (DelayedChanges.Length == 0)
			{
				if (Safety < Actual)
				{
					if (ReduceSteps <= 1)
					{
						AddChange(Actual - 1, GetDelay(false));
					}
					else if (Safety + ReduceSteps <= Actual | Safety == 0)
					{
						AddChange(Safety, GetDelay(false));
					}
				}
				else if (Safety > Actual)
				{
					AddChange(Actual + 1, GetDelay(true));
				}
			}
			else
			{
				int m = DelayedChanges.Length - 1;
				if (Safety < DelayedChanges[m].Value)
				{
					AddChange(Safety, GetDelay(false));
				}
				else if (Safety > DelayedChanges[m].Value)
				{
					AddChange(Safety, GetDelay(true));
				}
			}
			if (DelayedChanges.Length >= 1)
			{
				if (DelayedChanges[0].Time <= TrainManagerBase.currentHost.InGameTime)
				{
					Actual = DelayedChanges[0].Value;
					RemoveChanges(1);
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

			if (baseTrain.Handles.SingleHandle && baseTrain.Handles.Brake.Driver != 0)
			{
				p = 0;
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
