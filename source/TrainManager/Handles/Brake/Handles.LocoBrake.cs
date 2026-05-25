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
		public LocoBrakeHandle(int max, EmergencyHandle eb, double[] delayUp, double[] delayDown, TrainBase Train) : base (Train)
		{
			MaximumNotch = max;
			EmergencyBrake = eb;
			DelayUp = delayUp;
			DelayDown = delayDown;
		}

		public LocoBrakeHandle(int max, EmergencyHandle eb, TrainBase Train) : base(Train)
		{
			MaximumNotch = max;
			EmergencyBrake = eb;
			DelayUp = null;
			DelayDown = null;
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
		}

		public override void ApplyState(int NotchValue, bool Relative, bool isOverMaxDriverNotch = false)
		{
			int b = Relative ? NotchValue + Driver : NotchValue;
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
					baseTrain.Cars[baseTrain.DriverCar].CarBrake.Release.Play(baseTrain.Cars[baseTrain.DriverCar], false);
					if (b > 0)
					{
						// brake release (not min) 
						if (Driver - b > 2 | ContinuousMovement && DecreaseFast.Buffer != null)
						{
							baseTrain.Handles.Brake.DecreaseFast.Play(baseTrain.Cars[baseTrain.DriverCar], false);
						}
						else
						{
							baseTrain.Handles.Brake.Decrease.Play(baseTrain.Cars[baseTrain.DriverCar], false);
						}
					}
					else
					{
						// brake min 
						baseTrain.Handles.Brake.Min.Play(baseTrain.Cars[baseTrain.DriverCar], false);

					}
				}
				else if (b > Driver)
				{
					// brake 
					if (b - Driver > 2 | ContinuousMovement && baseTrain.Handles.Brake.IncreaseFast.Buffer != null)
					{
						baseTrain.Handles.Brake.IncreaseFast.Play(baseTrain.Cars[baseTrain.DriverCar], false);
					}
					else
					{
						baseTrain.Handles.Brake.Increase.Play(baseTrain.Cars[baseTrain.DriverCar], false);
					}
				}
				Driver = b;
				Actual = b; //TODO: FIXME
				TrainManagerBase.currentHost.AddBlackBoxEntry();

				if (!TrainManagerBase.CurrentOptions.Accessibility) return;
				TrainManagerBase.currentHost.AddMessage(GetNotchDescription(out _), MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, 10.0, null);
			
		}

		public override void ApplySafetyState(int newState)
		{
			safetyState = newState;
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

		public override void Update(double timeElapsed)
		{
			if (DelayedValue != AirBrakeHandleState.Invalid)
			{
				if (DelayedTime <= TrainManagerBase.currentHost.InGameTime)
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
					DelayedTime = TrainManagerBase.currentHost.InGameTime;
				}
				else if (Safety == (int) AirBrakeHandleState.Service & Actual != (int) AirBrakeHandleState.Service)
				{
					DelayedValue = AirBrakeHandleState.Service;
					DelayedTime = TrainManagerBase.currentHost.InGameTime;
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
					baseTrain.Cars[baseTrain.DriverCar].CarBrake.Release.Play(baseTrain.Cars[baseTrain.DriverCar], false);
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
				else if ((int) newState > Driver)
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
				Driver = (int) newState;
				Actual = (int) newState; //TODO: FIXME
				TrainManagerBase.currentHost.AddBlackBoxEntry();
			}
		}

		public override void ApplySafetyState(int newState)
		{
			safetyState = Math.Max(safetyState, newState);
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
