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

using TrainManager.Car;

namespace TrainManager.SafetySystems
{
	public class OverspeedDevice : AbstractSafetySystem
	{
		/// <summary>The speed at which the overspeed alarm sounds</summary>
		internal double AlertSpeed;
		/// <summary>The speed at which it is possible to reset the device</summary>
		internal double ResetSpeed;

		public OverspeedDevice(CarBase car, SafetySystemType type, double alertSpeed, double resetSpeed, double alertTime, double interventionTime, double requiredStopTime)
			: base (car, type, alertTime, interventionTime, requiredStopTime)
		{
			AlertSpeed = alertSpeed;
			ResetSpeed = resetSpeed;
		}

		public override void Update(double timeElapsed)
		{
			if (baseCar.CurrentSpeed > AlertSpeed)
			{
				AlertSound.Play(baseCar, LoopingAlert);
				CurrentState = SafetySystemState.Alarm;
			}

			if (CurrentState == SafetySystemState.Alarm)
			{
				Timer += timeElapsed;
				if (Timer > InterventionTime)
				{
					AlarmSound.Play(baseCar, LoopingAlarm);
					CurrentState = SafetySystemState.Triggered;
				}
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

				if (baseCar.Specs.PerceivedSpeed <= ResetSpeed)
				{
					if (RequiredStopTime != 0 && baseCar.Specs.PerceivedSpeed == 0)
					{
						StopTimer += timeElapsed;
					}
					AttemptReset();
				}
			}
		}
	}
}
