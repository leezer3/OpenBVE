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

using OpenBveApi;
using OpenBveApi.Motor;
using TrainManager.Motor;

namespace TrainManager.SteamEngine
{
	public class SafetyValve : AbstractComponent
	{
		/// <summary>The pressure at which the safety valve operates</summary>
		public readonly double OperatingPressure;
		/// <summary>The pressure at which the safety valve releases</summary>
		public readonly double ReleasePressure;
		/// <summary>The decrease in pressure per second</summary>
		public readonly double PressureDecrease;

		public SafetyValve(TractionModel engine, double operatingPressure, double releasePressure, double pressureDecrease) : base(engine)
		{
			OperatingPressure = operatingPressure;
			ReleasePressure = releasePressure;
			PressureDecrease = pressureDecrease;
		}

		public override void Update(double timeElapsed)
		{
			if (!baseEngine.Components.TryGetTypedValue(EngineComponent.Boiler, out Boiler boiler))
			{
				return;
			}

			if (boiler.CurrentPressure > OperatingPressure)
			{
				Active = true;
				ActivationSound?.Play(baseEngine.BaseCar, false);
			}
			else
			{
				if (boiler.CurrentPressure < ReleasePressure)
				{
					Active = false;
					LoopSound?.Stop();
					DeactivationSound?.Play(baseEngine.BaseCar, false);
				}
			}

			if (Active)
			{
				boiler.CurrentSteamMass -= PressureDecrease * timeElapsed;
				if (LoopSound != null && LoopSound.IsPlaying == false)
				{
					LoopSound.Play(baseEngine.BaseCar, true);
				}
			}
		}
	}
}
