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
		public BrakeHandle(int max, int driverMax, EmergencyHandle eb, double[] delayUp, double[] delayDown, TrainBase Train) : base(Train)
		{
			MaximumNotch = max;
			MaximumDriverNotch = driverMax;
			EmergencyBrake = eb;
			DelayUp = delayUp;
			DelayDown = delayDown;
			DelayedChanges = new HandleChange[] { };
		}

		public BrakeHandle(int max, EmergencyHandle eb, TrainBase Train) : base(Train)
		{
			MaximumNotch = max;
			MaximumDriverNotch = max;
			EmergencyBrake = eb;
			DelayUp = null;
			DelayDown = null;
			DelayedChanges = new HandleChange[] { };
		}

		/// <summary>Provides a reference to the associated EB handle</summary>
		private readonly EmergencyHandle EmergencyBrake;

		public override void Update(double timeElapsed)
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
			if (SpringType != SpringType.Unsprung && SpringTime > 0)
			{
				if (TrainManagerBase.currentHost.InGameTime > SpringTimer)
				{
					ApplyState(1, true);
				}
			}
		}

		public override void ApplyState(int BrakeValue, bool BrakeRelative, bool IsOverMaxDriverNotch = false)
		{
			int previousDriver = Driver;
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
			int offset = baseTrain.Handles.HasHoldBrake ? 2 : 1;

			if (NotchDescriptions == null || offset + Driver >= NotchDescriptions.Length)
			{
				if (baseTrain.Handles.EmergencyBrake.Driver)
				{
					color = MessageColor.Red;
					return Translations.QuickReferences.HandleEmergency;
				}

				if (baseTrain.Handles.HasHoldBrake && baseTrain.Handles.HoldBrake.Driver)
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
				if (baseTrain.Handles.EmergencyBrake.Driver)
				{
					color = MessageColor.Red;
					return NotchDescriptions[0];
				}

				if (baseTrain.Handles.HasHoldBrake && baseTrain.Handles.HoldBrake.Driver) {
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
