//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using OpenBveApi.Interface;
using TrainManager.Car;

namespace TrainManager.SafetySystems
{
	/// <summary>Represents a driver supervision device</summary>
	public class DriverSupervisionDevice : AbstractSafetySystem
	{
		/// <summary>The mode of the DSD</summary>
		private readonly DriverSupervisionDeviceMode Mode;
		/// <summary>The trigger mode of the DSD</summary>
		private readonly SafetySystemTriggerMode TriggerMode;
		

		public DriverSupervisionDevice(CarBase car, SafetySystemType type, DriverSupervisionDeviceMode mode, SafetySystemTriggerMode triggerMode, double alertTime, double interventionTime, double requiredStopTime) 
			: base(car, type, alertTime, interventionTime, requiredStopTime)
		{
			Mode = mode;
			TriggerMode = triggerMode;
		}

		public override void Update(double timeElapsed)
		{
			if (Type == SafetySystemType.None)
			{
				return;
			}
			Timer += timeElapsed;

			if (Timer > AlertTime && CurrentState == SafetySystemState.Monitoring)
			{
				AlertSound.Play(baseCar, LoopingAlert);
				CurrentState = SafetySystemState.Alarm;
			}

			if (Timer > InterventionTime && CurrentState < SafetySystemState.Triggered)
			{
				AlertSound.Stop();
				AlarmSound.Play(baseCar, LoopingAlarm);
				CurrentState = SafetySystemState.Triggered;
			}

			switch (TriggerMode)
			{
				case SafetySystemTriggerMode.Always:
					// nothing to do
					break;
				case SafetySystemTriggerMode.TrainMoving:
					if (baseCar.CurrentSpeed == 0)
					{
						// not moving, so reset our timer and return
						Timer = 0;
						return;
					}
					break;
				case SafetySystemTriggerMode.DirectionSet:
					if (baseCar.baseTrain.Handles.Reverser.Actual == 0 && baseCar.CurrentSpeed == 0)
					{
						// no direction set, not moving
						Timer = 0;
						return;
					}
					break;
			}

			if (CurrentState == SafetySystemState.Triggered)
			{
				switch (Type)
				{
					case SafetySystemType.CutsPower:
						baseCar.baseTrain.Handles.Power.ApplySafetyState(0);
						break;
					case SafetySystemType.ApplyBrake:
						baseCar.baseTrain.Handles.Power.ApplySafetyState(0);
						baseCar.baseTrain.Handles.Brake.ApplySafetyState(baseCar.baseTrain.Handles.Brake.MaximumNotch);
						break;
					case SafetySystemType.ApplyEmergencyBrake:
						baseCar.baseTrain.Handles.Power.ApplySafetyState(0);
						baseCar.baseTrain.Handles.Brake.ApplySafetyState(baseCar.baseTrain.Handles.Brake.MaximumNotch + 1);
						break;
				}

				if (RequiredStopTime != 0 && baseCar.Specs.PerceivedSpeed == 0)
				{
					StopTimer += timeElapsed;
				}
			}
		}

		private void AttemptReset(Translations.Command control)
		{
			Timer = 0;
			if (CurrentState == SafetySystemState.Triggered && StopTimer >= RequiredStopTime && control == Translations.Command.DriverSupervisionDevice)
			{
				AlarmSound.Stop();
				Timer = 0.0;
				CurrentState = SafetySystemState.Monitoring;
				StopTimer = 0;
				ResetSound.Play(baseCar, false);
			}
		}

		public override void ControlDown(Translations.Command controlDown)
		{
			switch (Mode)
			{
				case DriverSupervisionDeviceMode.Power:
					if (controlDown >= Translations.Command.PowerIncrease && controlDown <= Translations.Command.PowerFullAxis)
					{
						AttemptReset(controlDown);
					}
					break;
				case DriverSupervisionDeviceMode.Brake:
					if (controlDown >= Translations.Command.BrakeIncrease && controlDown <= Translations.Command.BrakeFullAxis)
					{
						AttemptReset(controlDown);
					}
					break;
				case DriverSupervisionDeviceMode.AnyHandle:
					if ((controlDown >= Translations.Command.PowerIncrease && controlDown <= Translations.Command.PowerFullAxis) || (controlDown >= Translations.Command.BrakeIncrease && controlDown <= Translations.Command.BrakeFullAxis))
					{
						AttemptReset(controlDown);
					}
					break;
				case DriverSupervisionDeviceMode.HeldKey:
				case DriverSupervisionDeviceMode.Independant:
					if (controlDown == Translations.Command.DriverSupervisionDevice)
					{
						AttemptReset(controlDown);
					}
					break;
			}
		}

		public override void ControlUp(Translations.Command controlUp)
		{
			switch (Mode)
			{
				case DriverSupervisionDeviceMode.Power:
					if (controlUp >= Translations.Command.PowerIncrease && controlUp <= Translations.Command.PowerFullAxis)
					{
						AttemptReset(controlUp);
					}
					break;
				case DriverSupervisionDeviceMode.Brake:
					if (controlUp >= Translations.Command.BrakeIncrease && controlUp <= Translations.Command.BrakeFullAxis)
					{
						AttemptReset(controlUp);
					}
					break;
				case DriverSupervisionDeviceMode.AnyHandle:
					if ((controlUp >= Translations.Command.PowerIncrease && controlUp <= Translations.Command.PowerFullAxis) || (controlUp >= Translations.Command.BrakeIncrease && controlUp <= Translations.Command.BrakeFullAxis))
					{
						AttemptReset(controlUp);
					}
					break;
				case DriverSupervisionDeviceMode.Independant:
					// don't attempt to reset on key-up (as otherwise we can hold the key through a new trigger cycle)
					if (controlUp == Translations.Command.DriverSupervisionDevice && CurrentState != SafetySystemState.Triggered)
					{
						AttemptReset(controlUp);
					}
					break;
			}
		}
	}
}
