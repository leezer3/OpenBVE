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

// ReSharper disable InconsistentNaming

using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Trains;

namespace TrainManager.Power
{
	/// <summary>Represents a MSTS acceleration curve</summary>
	public class MSTSAccelerationCurve : AccelerationCurve
	{
		/// <summary>Holds a reference to the car</summary>
		/// <remarks>Can't store the train reference directly as we may couple to another</remarks>
		private readonly CarBase baseCar;
		/// <summary>The maximum force supplied by the engine</summary>
		private readonly double MaxForce;
		/// <summary>The maximum velocity attainable</summary>
		private readonly double MaxVelocity;
		/// <summary>The maximum continuous force supplied by the engine</summary>
		/// <remarks>Used by electric model</remarks>
		private readonly double MaxContinuousForce;

		public MSTSAccelerationCurve(CarBase car, double maxForce, double maxContinuousForce, double maxVelocity)
		{
			baseCar = car;
			MaxForce = maxForce;
			MaxContinuousForce = maxContinuousForce;
			MaxVelocity = maxVelocity;
		}

		public override double GetAccelerationOutput(double Speed)
		{
			if (baseCar.Specs.PerceivedSpeed > MaxVelocity)
			{
				return 0;
			}

			/*
			 * According to Newton's second law, Acceleration = Force / Mass
			 * For the minute at least, we'll assume that the Force value specified in an
			 * ENG file is proportionate across power notches, and remains constant throughout
			 * the speed range. In practice, MSTS will actually have internally simulated this
			 * via engine RPM, boiler pressure etc. etc. but at present we obviously don't
			 * have these available.
			 *
			 * We don't in this case need to take the speed or loading values into account, but
			 * retain them as legacy
			 */
			double totalMass = 0;

			TrainBase baseTrain = baseCar.baseTrain;
			for (int i = 0; i < baseTrain.Cars.Length; i++)
			{
				totalMass += baseTrain.Cars[i].CurrentMass;
			}

			if (baseTrain.Handles.EmergencyBrake.Actual)
			{
				return totalMass / MaxForce / 3.6;
			}

			if (baseTrain.Handles.Brake.Actual > 0)
			{
				return ((baseTrain.Handles.Brake.Actual / (double)baseTrain.Handles.Brake.MaximumNotch) *  (totalMass / MaxForce));
			}

			if (baseCar.TractionModel is DieselEngine dieselEngine)
			{
				// diesel engine uses simulated power level of MaxForce
				return dieselEngine.CurrentPower * (totalMass / MaxForce);
			}

			if (baseCar.TractionModel is ElectricEngine electricEngine)
			{
				if (baseCar.Specs.PerceivedSpeed < 3.61111 || baseCar.Specs.PerceivedSpeed > 7.22222)
				{
					// for electrics, MaxContinuousForce is used between 13km/h and 26km/h as per original Kuju physics model
					return electricEngine.CurrentPower * (totalMass / MaxContinuousForce);
				}
				else
				{
					return electricEngine.CurrentPower * (totalMass / MaxForce);
				}
			}

			return ((baseTrain.Handles.Power.Actual / (double)baseTrain.Handles.Power.MaximumNotch) * (totalMass / MaxForce));
		}

		public override double MaximumAcceleration
		{
			get
			{
				double totalMass = 0;
				TrainBase baseTrain = baseCar.baseTrain;
				for (int i = 0; i < baseTrain.Cars.Length; i++)
				{
					totalMass += baseTrain.Cars[i].CurrentMass;
				}

				return totalMass / MaxForce;
			}
		}

	}
	
	public class MSTSDecelerationCurve : AccelerationCurve
	{
		private readonly TrainBase Train;
		/// <summary>The maximum force supplied by the engine</summary>
		private readonly double MaxForce;

		public MSTSDecelerationCurve(TrainBase train, double maxForce)
		{
			Train = train;
			MaxForce = maxForce;
		}
		public override double GetAccelerationOutput(double Speed)
		{
			/*
			 * According to Newton's second law, Acceleration = Force / Mass
			 * For the minute at least, we'll assume that the Force value specified in an
			 * ENG file is proportionate across power notches, and remains constant throughout
			 * the speed range. In practice, MSTS will actually have internally simulated this
			 * via engine RPM, boiler pressure etc. etc. but at present we obviously don't
			 * have these available.
			 *
			 * We don't in this case need to take the speed or loading values into account, but
			 * retain them as legacy
			 * REFACTOR: Store the train reference in BVE acceleration curves???
			 */
			double totalMass = 0;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				totalMass += Train.Cars[i].CurrentMass;
			}

			double maxBrakeForce = totalMass / MaxForce / 3.6;
			if (Train.Handles.Brake is VariableHandle variableHandle)
			{
				maxBrakeForce *= variableHandle.GetPowerModifier;
			}
			return maxBrakeForce;
		}

		public override double MaximumAcceleration
		{
			get
			{
				double totalMass = 0;
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					totalMass += Train.Cars[i].CurrentMass;
				}

				return totalMass / MaxForce / 3.6;
			}
		}
	}
}
