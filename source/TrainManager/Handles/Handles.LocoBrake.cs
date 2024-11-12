using System;
using System.Globalization;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using RouteManager2.MessageManager;
using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>A locomotive brake handle</summary>
	public class LocoBrakeHandle : NotchedHandle
	{
		public LocoBrakeHandle(int max, EmergencyHandle eb, double[] delayUp, double[] delayDown, TrainBase train) : base (train)
		{
			this.MaximumNotch = max;
			this.EmergencyBrake = eb;
			this.DelayUp = delayUp;
			this.DelayDown = delayDown;
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
		}

		public override void ApplyState(int notchValue, bool relative, bool isOverMaxDriverNotch = false)
		{
			int b = relative ? notchValue + Driver : notchValue;
				if (b < 0)
				{
					b = 0;
				}
				else if (b > MaximumNotch)
				{
					b = MaximumNotch;
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
							BaseTrain.Handles.Brake.DecreaseFast.Play(BaseTrain.Cars[BaseTrain.DriverCar], false);
						}
						else
						{
							BaseTrain.Handles.Brake.Decrease.Play(BaseTrain.Cars[BaseTrain.DriverCar], false);
						}
					}
					else
					{
						// brake min 
						BaseTrain.Handles.Brake.Min.Play(BaseTrain.Cars[BaseTrain.DriverCar], false);

					}
				}
				else if (b > Driver)
				{
					// brake 
					if (b - Driver > 2 | ContinuousMovement && BaseTrain.Handles.Brake.IncreaseFast.Buffer != null)
					{
						BaseTrain.Handles.Brake.IncreaseFast.Play(BaseTrain.Cars[BaseTrain.DriverCar], false);
					}
					else
					{
						BaseTrain.Handles.Brake.Increase.Play(BaseTrain.Cars[BaseTrain.DriverCar], false);
					}
				}
				Driver = b;
				Actual = b; //TODO: FIXME
				TrainManagerBase.CurrentHost.AddBlackBoxEntry();

				if (!TrainManagerBase.CurrentOptions.Accessibility) return;
				TrainManagerBase.CurrentHost.AddMessage(GetNotchDescription(out _), MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, TrainManagerBase.CurrentHost.InGameTime + 10.0, null);
			
		}

		public override void ApplySafetyState(int newState)
		{
			SafetyState = newState;
		}
	}

	/// <summary>A locomotive air brake handle</summary>
	public class LocoAirBrakeHandle : AbstractHandle
	{
		private AirBrakeHandleState DelayedValue;
		private double DelayedTime;

		public LocoAirBrakeHandle(TrainBase train) : base(train)
		{
			this.MaximumNotch = 3;
		}

		public override void Update()
		{
			if (DelayedValue != AirBrakeHandleState.Invalid)
			{
				if (DelayedTime <= TrainManagerBase.CurrentHost.InGameTime)
				{
					Actual = (int) DelayedValue;
					DelayedValue = AirBrakeHandleState.Invalid;
				}
			}
			else
			{
				if (Safety == (int) AirBrakeHandleState.Release & Actual != (int) AirBrakeHandleState.Release)
				{
					DelayedValue = AirBrakeHandleState.Release;
					DelayedTime = TrainManagerBase.CurrentHost.InGameTime;
				}
				else if (Safety == (int) AirBrakeHandleState.Service & Actual != (int) AirBrakeHandleState.Service)
				{
					DelayedValue = AirBrakeHandleState.Service;
					DelayedTime = TrainManagerBase.CurrentHost.InGameTime;
				}
				else if (Safety == (int) AirBrakeHandleState.Lap)
				{
					Actual = (int) AirBrakeHandleState.Lap;
				}
			}
		}

		public override void ApplyState(AirBrakeHandleState newState)
		{
			if ((int) newState != Driver)
			{
				// sound when moved to service
				if (newState == AirBrakeHandleState.Service)
				{
					BaseTrain.Cars[BaseTrain.DriverCar].CarBrake.Release.Play(BaseTrain.Cars[BaseTrain.DriverCar], false);
				}

				// sound
				if ((int) newState < Driver)
				{
					// brake release
					if ((int) newState > 0)
					{
						// brake release (not min)
						if (Driver - (int)newState > 2 | ContinuousMovement && DecreaseFast.Buffer != null)
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
				else if ((int) newState > Driver)
				{
					// brake
					if ((int)newState - Driver > 2 | ContinuousMovement && IncreaseFast.Buffer != null)
					{
						IncreaseFast.Play(BaseTrain.Cars[BaseTrain.DriverCar], false);
					}
					else
					{
						Increase.Play(BaseTrain.Cars[BaseTrain.DriverCar], false);
					}
				}
				Driver = (int) newState;
				Actual = (int) newState; //TODO: FIXME
				TrainManagerBase.CurrentHost.AddBlackBoxEntry();
			}
		}

		public override void ApplySafetyState(int newState)
		{
			SafetyState = Math.Max(SafetyState, newState);
		}

		public override string GetNotchDescription(out MessageColor color)
		{
			color = MessageColor.Gray;
			if (NotchDescriptions == null || Driver >= NotchDescriptions.Length)
			{
				if (BaseTrain.Handles.EmergencyBrake.Driver)
				{
					color = MessageColor.Red;
					return Translations.QuickReferences.HandleEmergency;
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


				if (Driver != 0)
				{
					color = MessageColor.Orange;
					return NotchDescriptions[Driver + 1];
				}

				return NotchDescriptions[1];
			}

		}
	}
}
