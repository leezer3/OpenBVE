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
using TrainManager.Handles;
using TrainManager.Power;
using TrainManager.Trains;

namespace TrainManager.Motor
{
	public class TenderEngine : TractionModel
	{
		/// <summary>Holds a reference to the connected tender, or null if no tender connected</summary>
		public Tender Tender;

		public TenderEngine(CarBase baseCar, AccelerationCurve[] accelerationCurves) : base(baseCar, accelerationCurves, true)
		{
		}

		public override void Update(double timeElapsed)
		{
			IsRunning = true;
		}

		// TODO: PLACEHOLDER VALUES

		public override double CurrentPower
		{
			get
			{
				if (BaseCar.baseTrain.AI is TrackFollowingObjectAI)
				{
					// HACK: TFO AI doesn't update power properly
					return BaseCar.CurrentSpeed == 0 ? 0 : 1;
				}

				if (BaseCar.baseTrain.Handles.Power is VariableHandle variableHandle)
				{
					Message = @"Power " + variableHandle.GetPowerModifier;
					return variableHandle.GetPowerModifier;
				}

				Message = @"Power " + (double)BaseCar.baseTrain.Handles.Power.Actual / BaseCar.baseTrain.Handles.Power.MaximumDriverNotch;
				return (double)BaseCar.baseTrain.Handles.Power.Actual / BaseCar.baseTrain.Handles.Power.MaximumDriverNotch;
			}
		}

		public override double TargetAcceleration => AccelerationCurves[0].GetAccelerationOutput(BaseCar.CurrentSpeed);
	}
}
