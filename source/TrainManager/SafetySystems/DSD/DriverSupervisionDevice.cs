using OpenBveApi.Interface;
using SoundManager;
using TrainManager.Car;

namespace TrainManager.SafetySystems
{
	/// <summary>Represents a driver supervision device</summary>
	public class DriverSupervisionDevice : AbstractSafetySystem
	{
		/// <summary>The mode of the DSD</summary>
		private readonly DriverSupervisionDeviceMode Mode;
		/// <summary>The trigger mode of the DSD</summary>
		private readonly DriverSupervisionDeviceTriggerMode TriggerMode;


		public DriverSupervisionDevice(CarBase Car, InterventionMode type, DriverSupervisionDeviceMode mode, DriverSupervisionDeviceTriggerMode triggerMode, double interventionTime, double requiredStopTime, bool loopingAlarm) : base(Car, type, loopingAlarm, interventionTime, requiredStopTime)
		{
			Mode = mode;
			TriggerMode = triggerMode;
			TriggerSound = new CarSound();
			ResetSound = new CarSound();
		}

		public override void Update(double TimeElapsed)
		{
			if (Type == InterventionMode.None)
			{
				return;
			}
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
					case InterventionMode.CutsPower:
						baseCar.baseTrain.Handles.Power.ApplySafetyState(0);
						break;
					case InterventionMode.ApplyBrake:
						baseCar.baseTrain.Handles.Power.ApplySafetyState(0);
						baseCar.baseTrain.Handles.Brake.ApplySafetyState(baseCar.baseTrain.Handles.Brake.MaximumNotch);
						break;
					case InterventionMode.ApplyEmergencyBrake:
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

		

		public override void ControlDown(Translations.Command Control)
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

		public override void ControlUp(Translations.Command Control)
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
