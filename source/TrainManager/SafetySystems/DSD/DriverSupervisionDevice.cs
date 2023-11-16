using OpenBveApi.Interface;
using SoundManager;
using TrainManager.Car;

namespace TrainManager.SafetySystems
{
	/// <summary>Represents a driver supervision device</summary>
	public class DriverSupervisionDevice
	{
		/// <summary>Holds a reference to the base car</summary>
		private readonly CarBase baseCar;
		/// <summary>The update timer</summary>
		private double Timer;
		/// <summary>The time after which the DSD will intervene</summary>
		private readonly double InterventionTime;
		/// <summary>Whether a DSD intervention has been triggered</summary>
		public bool Triggered;
		/// <summary>The type of device</summary>
		public readonly DriverSupervisionDeviceTypes Type;
		/// <summary>The sound played when the device is triggered</summary>
		public CarSound TriggerSound;
		/// <summary>Whether the alarm is to loop</summary>
		public readonly bool LoopingAlarm;
		/// <summary>The sound played when the device is reset</summary>
		public CarSound ResetSound;
		/// <summary>The required stop time to reset the DSD</summary>
		public double RequiredStopTime;
		/// <summary>The mode of the DSD</summary>
		public readonly DriverSupervisionDeviceMode Mode;
		/// <summary>The trigger mode of the DSD</summary>
		public readonly DriverSupervisionDeviceTriggerMode TriggerMode;

		private double StopTimer;

		public DriverSupervisionDevice(CarBase Car, DriverSupervisionDeviceTypes type, DriverSupervisionDeviceMode mode, DriverSupervisionDeviceTriggerMode triggerMode, double interventionTime, double requiredStopTime, bool loopingAlarm)
		{
			baseCar = Car;
			Type = type;
			Mode = mode;
			TriggerMode = triggerMode;
			InterventionTime = interventionTime;
			TriggerSound = new CarSound();
			ResetSound = new CarSound();
			RequiredStopTime = requiredStopTime;
			LoopingAlarm = loopingAlarm;
		}

		public void Update(double TimeElapsed)
		{
			Timer += TimeElapsed;
			if (Timer > InterventionTime && !Triggered)
			{
				Triggered = true;
				TriggerSound.Play(baseCar, LoopingAlarm);
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

			if (Triggered)
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
			if (Triggered && StopTimer >= RequiredStopTime)
			{
				TriggerSound.Stop();
				Timer = 0.0;
				Triggered = false;
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
