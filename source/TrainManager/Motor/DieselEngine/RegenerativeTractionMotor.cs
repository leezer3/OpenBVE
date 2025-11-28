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

using System;
using TrainManager.Handles;

namespace TrainManager.Motor
{
	public class RegenerativeTractionMotor : AbstractComponent
	{
		/// <summary>The current in Amps delivered by the traction motor</summary>
		public double CurrentAmps;
		/// <summary>The maximum amps able to be delivered by the traction motor</summary>
		private readonly double maxAmps;
		/// <summary>The maximum regenerative amps able to be delivered by the traction motor</summary>
		private readonly double maxRegenAmps;

		public RegenerativeTractionMotor(TractionModel engine, double max, double maxRegen) : base(engine)
		{
			maxAmps = max;
			maxRegenAmps = maxRegen;
		}

		public override void Update(double timeElapsed)
		{
			if (baseEngine is DieselEngine dieselEngine)
			{
				// n.b. assume that the engine makes no current at idle and when stopped
				if (baseEngine.BaseCar.baseTrain.Handles.Reverser.Actual != ReverserPosition.Neutral && Math.Abs(baseEngine.BaseCar.CurrentSpeed) > 0)
				{
					if (baseEngine.BaseCar.baseTrain.Handles.Brake.Actual > 0)
					{
						// we are currently regeneratively braking (ignore engine RPM)
						// return as negative number so single dial works OK
						CurrentAmps = -maxRegenAmps * ((double)baseEngine.BaseCar.baseTrain.Handles.Brake.Actual / baseEngine.BaseCar.baseTrain.Handles.Brake.MaximumDriverNotch);
					}
					else
					{
						// find the max possible amps at the current engine RPM
						CurrentAmps = maxAmps / (dieselEngine.MaxRPM - dieselEngine.IdleRPM) * (dieselEngine.CurrentRPM - dieselEngine.IdleRPM);
					}
						
				}
				else
				{
					CurrentAmps = 0;
				}
			}
			else
			{
				CurrentAmps = maxAmps / baseEngine.BaseCar.baseTrain.Handles.Power.MaximumDriverNotch * baseEngine.BaseCar.baseTrain.Handles.Power.Actual;
			}
		}
	}
}
