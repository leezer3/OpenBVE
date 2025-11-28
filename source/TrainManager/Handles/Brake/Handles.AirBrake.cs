using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using RouteManager2.MessageManager;
using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>Represents an air-brake handle</summary>
	public class AirBrakeHandle : AbstractHandle
	{
		private AirBrakeHandleState DelayedValue;
		private double DelayedTime;
		
		public AirBrakeHandle(TrainBase train) : base(train)
		{
			this.MaximumNotch = 3;
			this.MaximumDriverNotch = 3;
		}

		public override void Update(double timeElapsed)
		{
			if (DelayedValue != AirBrakeHandleState.Invalid)
			{
				if (DelayedTime <= TrainManagerBase.currentHost.InGameTime)
				{
					Actual = (int)DelayedValue;
					DelayedValue = AirBrakeHandleState.Invalid;
				}
			}
			else
			{
				if (Safety == (int)AirBrakeHandleState.Release & Actual != (int)AirBrakeHandleState.Release)
				{
					DelayedValue = AirBrakeHandleState.Release;
					DelayedTime = TrainManagerBase.currentHost.InGameTime;
				}
				else if (Safety == (int)AirBrakeHandleState.Service & Actual != (int)AirBrakeHandleState.Service)
				{
					DelayedValue = AirBrakeHandleState.Service;
					DelayedTime = TrainManagerBase.currentHost.InGameTime;
				}
				else if (Safety == (int)AirBrakeHandleState.Lap)
				{
					Actual = (int)AirBrakeHandleState.Lap;
				}
			}
		}

		public override void ApplyState(AirBrakeHandleState newState)
		{
			if ((int)newState != Driver)
			{
				// sound when moved to service
				if (newState == AirBrakeHandleState.Service)
				{
					baseTrain.Cars[baseTrain.DriverCar].CarBrake.Release.Play(baseTrain.Cars[baseTrain.DriverCar], false);
				}

				// sound
				if ((int)newState < Driver)
				{
					// brake release
					if (newState > 0)
					{
						// brake release (not min)
						if (Driver - (int)newState > 2 | ContinuousMovement && DecreaseFast.Buffer != null)
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
				else if ((int)newState > Driver)
				{
					// brake
					if ((int)newState - Driver > 2 | ContinuousMovement && IncreaseFast.Buffer != null)
					{
						IncreaseFast.Play(baseTrain.Cars[baseTrain.DriverCar], false);
					}
					else
					{
						Increase.Play(baseTrain.Cars[baseTrain.DriverCar], false);
					}
				}

				Driver = (int)newState;
				TrainManagerBase.currentHost.AddBlackBoxEntry();
				// plugin
				if (baseTrain.Plugin != null)
				{
					baseTrain.Plugin.UpdatePower();
					baseTrain.Plugin.UpdateBrake();
				}

				if (!TrainManagerBase.CurrentOptions.Accessibility) return;
				TrainManagerBase.currentHost.AddMessage(GetNotchDescription(out _), MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, 10.0, null);
			}
		}

		public override void ApplySafetyState(int newState)
		{
			safetyState = newState;
		}

		public override string GetNotchDescription(out MessageColor color)
		{
			color = MessageColor.Gray;
			if (NotchDescriptions == null || Driver >= NotchDescriptions.Length)
			{
				if (baseTrain.Handles.EmergencyBrake.Driver)
				{
					color = MessageColor.Red;
					return Translations.QuickReferences.HandleEmergency;
				}

				switch ((AirBrakeHandleState)Driver)
				{
					case AirBrakeHandleState.Release:
						color = MessageColor.Gray;
						return Translations.QuickReferences.HandleRelease;
					case AirBrakeHandleState.Lap:
						color = MessageColor.Blue;
						return Translations.QuickReferences.HandleLap;
					case AirBrakeHandleState.Service:
						color = MessageColor.Orange;
						return Translations.QuickReferences.HandleService;
				}
			}
			else
			{
				if (baseTrain.Handles.EmergencyBrake.Driver)
				{
					color = MessageColor.Red;
					return NotchDescriptions[0];
				}

				switch ((AirBrakeHandleState)Driver)
				{
					case AirBrakeHandleState.Release:
						color = MessageColor.Gray;
						return NotchDescriptions[1];
					case AirBrakeHandleState.Lap:
						color = MessageColor.Blue;
						return NotchDescriptions[2];
					case AirBrakeHandleState.Service:
						color = MessageColor.Orange;
						return NotchDescriptions[3];
				}
			}
			return string.Empty;
		}
	}
}
