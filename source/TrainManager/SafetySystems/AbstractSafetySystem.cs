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
using SoundManager;
using TrainManager.Car;

namespace TrainManager.SafetySystems
{
	public abstract class AbstractSafetySystem
	{
		/// <summary>The base car</summary>
		internal readonly CarBase baseCar;
		/// <summary>The type of device</summary>
		internal readonly SafetySystemType Type;
		/// <summary>The current safety system state</summary>
		public SafetySystemState CurrentState;
		/// <summary>The time after which the DSD will alert</summary>
		internal readonly double AlertTime;
		/// <summary>The time after which the DSD will intervene</summary>
		internal readonly double InterventionTime;
		/// <summary>The required stop time to reset the device</summary>
		internal readonly double RequiredStopTime;
		/// <summary>The update timer</summary>
		internal double Timer;
		/// <summary>The stop timer</summary>
		internal double StopTimer;
		/// <summary>The sound played when an alert is triggered</summary>
		public CarSound AlertSound;
		/// <summary>The sound played when the device is triggered</summary>
		public CarSound AlarmSound;
		/// <summary>The sound played when the device is reset</summary>
		public CarSound ResetSound;
		/// <summary>Whether the alert loops</summary>
		public bool LoopingAlert;
		/// <summary>Whether the alarm is to loop</summary>
		public bool LoopingAlarm;

		protected AbstractSafetySystem(CarBase car)
		{
			baseCar = car;
			AlertSound = new CarSound();
			AlarmSound = new CarSound();
			ResetSound = new CarSound();
		}

		protected AbstractSafetySystem(CarBase car, SafetySystemType type, double alertTime, double interventionTime, double requiredStopTime)
		{
			baseCar = car;
			Type = type;
			AlertTime = alertTime;
			InterventionTime = interventionTime;
			RequiredStopTime = requiredStopTime;
			LoopingAlert = true;
			LoopingAlarm = true;
			AlertSound = new CarSound();
			AlarmSound = new CarSound();
			ResetSound = new CarSound();
		}

		/// <summary>Updates the safety system</summary>
		/// <param name="timeElapsed">The elapsed time</param>
		public virtual void Update(double timeElapsed)
		{
		}

		/// <summary>Called when a control is raised</summary>
		/// <param name="Control">The control</param>
		public virtual void ControlUp(Translations.Command Control)
		{
		}

		/// <summary>Called when a control is pressed</summary>
		/// <param name="Control">The control</param>
		public virtual void ControlDown(Translations.Command Control)
		{
		}

		internal void AttemptReset()
		{
			Timer = 0;
			if (CurrentState == SafetySystemState.Triggered && StopTimer >= RequiredStopTime)
			{
				AlarmSound.Stop();
				Timer = 0.0;
				CurrentState = SafetySystemState.Monitoring;
				StopTimer = 0;
				ResetSound.Play(baseCar, false);
			}
		}
	}
}
