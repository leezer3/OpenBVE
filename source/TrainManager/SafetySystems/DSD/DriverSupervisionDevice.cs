using OpenBveApi.Interface;
using SoundManager;
using TrainManager.Car;

namespace TrainManager.SafetySystems
{
	/// <summary>Represents a driver supervision device</summary>
	public class DriverSupervisionDevice : AbstractSafetySystem
	{
		/// <summary>The update timer</summary>
		private double Timer;
		/// <summary>The time after which the DSD will alert</summary>
		private readonly double AlertTime;
		/// <summary>The time after which the DSD will intervene</summary>
		private readonly double InterventionTime;
		/// <summary>Whether a DSD intervention has been triggered</summary>
		public DriverSupervisionDeviceState CurrentState;
		/// <summary>The type of device</summary>
		private readonly DriverSupervisionDeviceTypes Type;
		/// <summary>The sound played when an alert is triggered</summary>
		public CarSound AlertSound;
		/// <summary>Whether the alert loops</summary>
		private bool LoopingAlert;
		/// <summary>The sound played when the device is triggered</summary>
		public CarSound AlarmSound;
		/// <summary>Whether the alarm is to loop</summary>
		private readonly bool LoopingAlarm;
		/// <summary>The sound played when the device is reset</summary>
		public CarSound ResetSound;
		/// <summary>The required stop time to reset the DSD</summary>
		private readonly double RequiredStopTime;
		/// <summary>The mode of the DSD</summary>
		private readonly DriverSupervisionDeviceMode Mode;
		/// <summary>The trigger mode of the DSD</summary>
		private readonly DriverSupervisionDeviceTriggerMode TriggerMode;

		private double StopTimer;

		public DriverSupervisionDevice(CarBase car, DriverSupervisionDeviceTypes type, DriverSupervisionDeviceMode mode, DriverSupervisionDeviceTriggerMode triggerMode, double alertTime, double interventionTime, double requiredStopTime, bool loopingAlert = true, bool loopingAlarm = true) : base(car)
		{
			Type = type;
			Mode = mode;
			TriggerMode = triggerMode;
			AlertTime = alertTime;
			InterventionTime = interventionTime;
			AlarmSound = new CarSound();
			ResetSound = new CarSound();
			RequiredStopTime = requiredStopTime;
			LoopingAlert = loopingAlert;
			LoopingAlarm = loopingAlarm;
		}

		public override void Update(double TimeElapsed)
		{
			if (Type == DriverSupervisionDeviceTypes.None)
			{
				return;
			}
			Timer += TimeElapsed;

			if (Timer > AlertTime && CurrentState == DriverSupervisionDeviceState.Monitoring)
			{
				AlertSound.Play(baseCar, LoopingAlert);
				CurrentState = DriverSupervisionDeviceState.Alarm;
			}

			if (Timer > InterventionTime && CurrentState < DriverSupervisionDeviceState.Triggered)
			{
				AlertSound.Stop();
				AlarmSound.Play(baseCar, LoopingAlarm);
				CurrentState = DriverSupervisionDeviceState.Triggered;
			}

			switch (TriggerMode)
			{
				case DriverSupervisionDeviceTriggerMode.Always:
					// nothing to do
					break;
				case DriverSupervisionDeviceTriggerMode.TrainMoving:
					if (baseCar.CurrentSpeed == 0)
					{
						// not moving, so reset our timer and return
						Timer = 0;
						return;
					}
					break;
				case DriverSupervisionDeviceTriggerMode.DirectionSet:
					if (baseCar.baseTrain.Handles.Reverser.Actual == 0 && baseCar.CurrentSpeed == 0)
					{
						// no direction set, not moving
						Timer = 0;
						return;
					}
					break;
			}

			if (CurrentState == DriverSupervisionDeviceState.Triggered)
			{
				switch (Type)
				{
					case DriverSupervisionDeviceTypes.CutsPower:
						baseCar.baseTrain.Handles.Power.ApplySafetyState(0);
						break;
					case DriverSupervisionDeviceTypes.ApplyBrake:
						baseCar.baseTrain.Handles.Power.ApplySafetyState(0);
						baseCar.baseTrain.Handles.Brake.ApplySafetyState(baseCar.baseTrain.Handles.Brake.MaximumNotch);
						break;
					case DriverSupervisionDeviceTypes.ApplyEmergencyBrake:
						baseCar.baseTrain.Handles.Power.ApplySafetyState(0);
						baseCar.baseTrain.Handles.Brake.ApplySafetyState(baseCar.baseTrain.Handles.Brake.MaximumNotch + 1);
						break;
				}

				if (RequiredStopTime != 0 && baseCar.Specs.PerceivedSpeed == 0)
				{
					StopTimer += TimeElapsed;
				}
			}
		}

		private void AttemptReset()
		{
			Timer = 0;
			if (CurrentState == DriverSupervisionDeviceState.Triggered && StopTimer >= RequiredStopTime)
			{
				AlarmSound.Stop();
				Timer = 0.0;
				CurrentState = DriverSupervisionDeviceState.Monitoring;
				StopTimer = 0;
				ResetSound.Play(baseCar, false);
			}
		}

		public void ControlDown(Translations.Command Control)
		{
			switch (Mode)
			{
				case DriverSupervisionDeviceMode.Power:
					if (Control >= Translations.Command.PowerIncrease && Control <= Translations.Command.PowerFullAxis)
					{
						AttemptReset();
					}
					break;
				case DriverSupervisionDeviceMode.Brake:
					if (Control >= Translations.Command.BrakeIncrease && Control <= Translations.Command.BrakeFullAxis)
					{
						AttemptReset();
					}
					break;
				case DriverSupervisionDeviceMode.AnyHandle:
					if ((Control >= Translations.Command.PowerIncrease && Control <= Translations.Command.PowerFullAxis) || (Control >= Translations.Command.BrakeIncrease && Control <= Translations.Command.BrakeFullAxis))
					{
						AttemptReset();
					}
					break;
				case DriverSupervisionDeviceMode.HeldKey:
				case DriverSupervisionDeviceMode.Independant:
					if (Control == Translations.Command.DriverSupervisionDevice)
					{
						AttemptReset();
					}
					break;
			}
		}

		public void ControlUp(Translations.Command Control)
		{
			switch (Mode)
			{
				case DriverSupervisionDeviceMode.Power:
					if (Control >= Translations.Command.PowerIncrease && Control <= Translations.Command.PowerFullAxis)
					{
						AttemptReset();
					}
					break;
				case DriverSupervisionDeviceMode.Brake:
					if (Control >= Translations.Command.BrakeIncrease && Control <= Translations.Command.BrakeFullAxis)
					{
						AttemptReset();
					}
					break;
				case DriverSupervisionDeviceMode.AnyHandle:
					if ((Control >= Translations.Command.PowerIncrease && Control <= Translations.Command.PowerFullAxis) || (Control >= Translations.Command.BrakeIncrease && Control <= Translations.Command.BrakeFullAxis))
					{
						AttemptReset();
					}
					break;
				case DriverSupervisionDeviceMode.Independant:
					if (Control == Translations.Command.DriverSupervisionDevice)
					{
						AttemptReset();
					}
					break;
			}
		}
	}
}
