using System.Globalization;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using RouteManager2.MessageManager;
using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>A power handle</summary>
	public class PowerHandle : NotchedHandle
	{
		public PowerHandle(int max, int driverMax, double[] delayUp, double[] delayDown, TrainBase Train) : base(Train)
		{
			MaximumNotch = max;
			MaximumDriverNotch = driverMax;
			DelayUp = delayUp;
			DelayDown = delayDown;
			DelayedChanges = new HandleChange[] { };
		}

		public PowerHandle(int max, TrainBase train) : base(train)
		{
			MaximumNotch = max;
			MaximumDriverNotch = max;
			DelayUp = null;
			DelayDown = null;
			DelayedChanges = new HandleChange[] { };
		}

		public override void Update(double timeElapsed)
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
			int previousDriver = Driver;
			if (baseTrain.Handles.Brake.SpringType > SpringType.Single)
			{
				baseTrain.Handles.Brake.SpringTimer = TrainManagerBase.currentHost.InGameTime + SpringTime;
			}
			SpringTimer = TrainManagerBase.currentHost.InGameTime + SpringTime;
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

			if (baseTrain.Handles.HandleType != HandleType.TwinHandle && baseTrain.Handles.Brake.Driver != 0)
			{
				p = 0;
			}
			Driver = p;
			
			// plugin
			if (baseTrain.Plugin != null)
			{
				baseTrain.Plugin.UpdatePower();
				baseTrain.Plugin.UpdateBrake();
			}

			if (previousDriver == Driver)
			{
				return;
			}

			TrainManagerBase.currentHost.AddBlackBoxEntry();

			if (!TrainManagerBase.CurrentOptions.Accessibility) return;
			TrainManagerBase.currentHost.AddMessage(GetNotchDescription(out _), MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, 10.0, null);
			
		}

		public override void ApplySafetyState(int newState)
		{
			safetyState = newState;
		}

		public override string GetNotchDescription(out MessageColor color)
		{
			color = MessageColor.Gray;

			if (baseTrain.Handles.HandleType == HandleType.SingleHandle && (baseTrain.Handles.Brake.Driver != 0 || baseTrain.Handles.EmergencyBrake.Driver))
			{
				return baseTrain.Handles.Brake.GetNotchDescription(out color);
			}

			if (Driver > 0)
			{
				color = MessageColor.Blue;
			}
			if (NotchDescriptions == null || Driver >= NotchDescriptions.Length)
			{
				if (Driver > 0)
				{
					return Translations.QuickReferences.HandlePower + Driver.ToString(CultureInfo.InvariantCulture);
				}

				return Translations.QuickReferences.HandlePowerNull;
			}
			else
			{
				return NotchDescriptions[Driver];
			}
		}
	}
}
