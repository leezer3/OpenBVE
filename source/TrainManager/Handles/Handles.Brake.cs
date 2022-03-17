using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>A brake handle</summary>
	public class BrakeHandle : NotchedHandle
	{
		public BrakeHandle(int max, int driverMax, EmergencyHandle eb, double[] delayUp, double[] delayDown, TrainBase Train) : base(Train)
		{
			this.MaximumNotch = max;
			this.MaximumDriverNotch = driverMax;
			this.EmergencyBrake = eb;
			this.DelayUp = delayUp;
			this.DelayDown = delayDown;
			this.DelayedChanges = new HandleChange[] { };
		}

		/// <summary>Provides a reference to the associated EB handle</summary>
		private readonly EmergencyHandle EmergencyBrake;

		public override void Update()
		{
			int sec = EmergencyBrake.Safety ? MaximumNotch : Safety;
			if (DelayedChanges.Length == 0)
			{
				if (sec < Actual)
				{
					AddChange(Actual - 1, GetDelay(false));
				}
				else if (sec > Actual)
				{
					AddChange(Actual + 1, GetDelay(true));
				}
			}
			else
			{
				int m = DelayedChanges.Length - 1;
				if (sec < DelayedChanges[m].Value)
				{
					AddChange(sec, GetDelay(false));
				}
				else if (sec > DelayedChanges[m].Value)
				{
					AddChange(sec, GetDelay(true));
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

			// Spring increase
			if (SpringType != SpringType.Unsprung)
			{
				if (TrainManagerBase.currentHost.InGameTime > SpringTimer)
				{
					ApplyState(1, true);
				}
			}
		}

		public override void ApplyState(int BrakeValue, bool BrakeRelative, bool IsOverMaxDriverNotch = false)
		{
			if (baseTrain.Handles.Power.SpringType > SpringType.Single)
			{
				baseTrain.Handles.Power.SpringTimer = TrainManagerBase.currentHost.InGameTime + SpringTime;
			}
			SpringTimer = TrainManagerBase.currentHost.InGameTime + SpringTime;
			int b = BrakeRelative ? BrakeValue + Driver : BrakeValue;
			if (b < 0)
			{
				b = 0;
			}
			else if (b > MaximumNotch)
			{
				b = MaximumNotch;
			}
			if (!IsOverMaxDriverNotch && b > MaximumDriverNotch)
			{
				b = MaximumDriverNotch;
			}

			// brake sound
			if (b < Driver)
			{
				// brake release
				baseTrain.Cars[baseTrain.DriverCar].CarBrake.Release.Play(baseTrain.Cars[baseTrain.DriverCar], false);
					
				if (b > 0)
				{
					// brake release (not min)
					if (Driver - b > 2 | ContinuousMovement && DecreaseFast.Buffer != null)
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
					// brake min
					Min.Play(baseTrain.Cars[baseTrain.DriverCar], false);
				}
			}
			else if (b > Driver)
			{
				// brake
				if (b - Driver > 2 | ContinuousMovement && IncreaseFast.Buffer != null)
				{
					IncreaseFast.Play(baseTrain.Cars[baseTrain.DriverCar], false);
				}
				else
				{
					Increase.Play(baseTrain.Cars[baseTrain.DriverCar], false);
				}
			}

			Driver = b;
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
