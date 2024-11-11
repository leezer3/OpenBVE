using System;
using System.Globalization;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using RouteManager2.MessageManager;
using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>A brake handle</summary>
	public class BrakeHandle : NotchedHandle
	{
		public BrakeHandle(int max, int driverMax, EmergencyHandle eb, double[] delayUp, double[] delayDown, TrainBase train) : base(train)
		{
			MaximumNotch = max;
			MaximumDriverNotch = driverMax;
			EmergencyBrake = eb;
			DelayUp = delayUp;
			DelayDown = delayDown;
			DelayedChanges = new HandleChange[] { };
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
				if (DelayedChanges[0].Time <= TrainManagerBase.CurrentHost.InGameTime)
				{
					Actual = DelayedChanges[0].Value;
					RemoveChanges(1);
				}
			}

			// Spring increase
			if (SpringType != SpringType.Unsprung && SpringTime > 0)
			{
				if (TrainManagerBase.CurrentHost.InGameTime > SpringTimer)
				{
					ApplyState(1, true);
				}
			}
		}

		public override void ApplyState(int brakeValue, bool brakeRelative, bool isOverMaxDriverNotch = false)
		{
			int previousDriver = Driver;
			if (BaseTrain.Handles.Power.SpringType > SpringType.Single)
			{
				BaseTrain.Handles.Power.SpringTimer = TrainManagerBase.CurrentHost.InGameTime + SpringTime;
			}
			SpringTimer = TrainManagerBase.CurrentHost.InGameTime + SpringTime;
			int b = brakeRelative ? brakeValue + Driver : brakeValue;
			if (b < 0)
			{
				b = 0;
			}
			else if (b > MaximumNotch)
			{
				b = MaximumNotch;
			}
			if (!isOverMaxDriverNotch && b > MaximumDriverNotch)
			{
				b = MaximumDriverNotch;
			}

			// brake sound
			if (b < Driver)
			{
				// brake release
				BaseTrain.Cars[BaseTrain.DriverCar].CarBrake.Release.Play(BaseTrain.Cars[BaseTrain.DriverCar], false);
					
				if (b > 0)
				{
					// brake release (not min)
					if (Driver - b > 2 | ContinuousMovement && DecreaseFast.Buffer != null)
					{
						DecreaseFast.Play(BaseTrain.Cars[BaseTrain.DriverCar], false);
					}
					else
					{
						Decrease.Play(BaseTrain.Cars[BaseTrain.DriverCar], false);
					}
				}
				else
				{
					// brake min
					Min.Play(BaseTrain.Cars[BaseTrain.DriverCar], false);
				}
			}
			else if (b > Driver)
			{
				// brake
				if (b - Driver > 2 | ContinuousMovement && IncreaseFast.Buffer != null)
				{
					IncreaseFast.Play(BaseTrain.Cars[BaseTrain.DriverCar], false);
				}
				else
				{
					Increase.Play(BaseTrain.Cars[BaseTrain.DriverCar], false);
				}
			}

			Driver = b;
			
			// plugin
			if (BaseTrain.Plugin != null)
			{
				BaseTrain.Plugin.UpdatePower();
				BaseTrain.Plugin.UpdateBrake();
			}

			if (previousDriver == Driver)
			{
				return;
			}
			TrainManagerBase.CurrentHost.AddBlackBoxEntry();
			if (!TrainManagerBase.CurrentOptions.Accessibility) return;
			TrainManagerBase.CurrentHost.AddMessage(GetNotchDescription(out _), MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, TrainManagerBase.CurrentHost.InGameTime + 10.0, null);
		}

		public override void ApplySafetyState(int newState)
		{
			SafetyState = newState;
		}

		public override string GetNotchDescription(out MessageColor color)
		{
			color = MessageColor.Gray;
			int offset = BaseTrain.Handles.HasHoldBrake ? 2 : 1;

			if (NotchDescriptions == null || offset + Driver >= NotchDescriptions.Length)
			{
				if (BaseTrain.Handles.EmergencyBrake.Driver)
				{
					color = MessageColor.Red;
					return Translations.QuickReferences.HandleEmergency;
				}

				if (BaseTrain.Handles.HasHoldBrake && BaseTrain.Handles.HoldBrake.Driver)
				{
					color = MessageColor.Green;
					return Translations.QuickReferences.HandleHoldBrake;
				}

				if (Driver != 0)
				{
					color = MessageColor.Orange;
					return Translations.QuickReferences.HandleBrake + Driver.ToString(CultureInfo.InvariantCulture);
				}

				return Translations.QuickReferences.HandleBrakeNull;
			}
			else
			{
				if (BaseTrain.Handles.EmergencyBrake.Driver)
				{
					color = MessageColor.Red;
					return NotchDescriptions[0];
				}

				if (BaseTrain.Handles.HasHoldBrake && BaseTrain.Handles.HoldBrake.Driver) {
					color = MessageColor.Green;
					return NotchDescriptions[1];
				}

				if (Driver != 0)
				{
					color = MessageColor.Orange;
					return NotchDescriptions[offset + Driver];
				}

				return NotchDescriptions[offset];
			}

		}
	}
}
