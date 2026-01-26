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

using OpenBveApi.Trains;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Power;

namespace TrainManager.BrakeSystems
{
	public class VacuumBrake : CarBrake
	{
		public VacuumBrake(CarBase car, AccelerationCurve[] decelerationCurves) : base(car, decelerationCurves)
		{
		}

		public override void Update(double timeElapsed, double currentSpeed, AbstractHandle brakeHandle, out double deceleration)
		{
			airSound = null;
			if (Car.baseTrain.Handles.EmergencyBrake.Actual)
			{
				if (BrakeType == BrakeType.Main)
				{
					double r = EqualizingReservoir.EmergencyRate;
					double d = EqualizingReservoir.CurrentPressure;
					double m = EqualizingReservoir.NormalPressure;
					r = GetRate(d / m, r * timeElapsed);
					if (r > EqualizingReservoir.CurrentPressure)
					{
						r = EqualizingReservoir.CurrentPressure;
					}
					EqualizingReservoir.CurrentPressure -= r;
				}
			}
			//First update the main reservoir pressure
			{
				double r = EqualizingReservoir.ChargeRate;
				double d = EqualizingReservoir.NormalPressure - EqualizingReservoir.CurrentPressure;
				double m = EqualizingReservoir.NormalPressure;
				r = GetRate(d / m, r * timeElapsed);
				if (r > d) r = d;
				d = MainReservoir.CurrentPressure - EqualizingReservoir.CurrentPressure;
				if (r > d) r = d;
				double f = MainReservoir.EqualizingReservoirCoefficient;
				double s = r * f * timeElapsed;
				if (s > MainReservoir.CurrentPressure)
				{
					r *= MainReservoir.CurrentPressure / s;
					s = MainReservoir.CurrentPressure;
				}

				EqualizingReservoir.CurrentPressure += 0.5 * r;
				MainReservoir.CurrentPressure -= 0.5 * s;
			}
			//Fill the brake pipe from the main reservoir
			if (BrakeType == BrakeType.Main)
			{
				if (BrakePipe.CurrentPressure > EqualizingReservoir.CurrentPressure + Tolerance)
				{
					// brake pipe exhaust valve
					double r = Car.baseTrain.Handles.EmergencyBrake.Actual ? BrakePipe.EmergencyRate : BrakePipe.ServiceRate;
					double d = BrakePipe.CurrentPressure - EqualizingReservoir.CurrentPressure;
					double m = EqualizingReservoir.NormalPressure;
					r = (0.5 + 1.5 * d / m) * r * timeElapsed;
					if (r > d) r = d;
					BrakePipe.CurrentPressure -= r;
				}
				else if (BrakePipe.CurrentPressure + Tolerance < EqualizingReservoir.CurrentPressure)
				{
					// fill brake pipe from main reservoir
					double r = BrakePipe.ChargeRate;
					double d = EqualizingReservoir.CurrentPressure - BrakePipe.CurrentPressure;
					double m = EqualizingReservoir.NormalPressure;
					r = (0.5 + 1.5 * d / m) * r * timeElapsed;
					if (r > d) r = d;
					d = BrakePipe.NormalPressure - BrakePipe.CurrentPressure;
					if (r > d) r = d;
					double f = MainReservoir.BrakePipeCoefficient;
					double s = r * f;
					if (s > MainReservoir.CurrentPressure)
					{
						r *= MainReservoir.CurrentPressure / s;
						s = MainReservoir.CurrentPressure;
					}
					BrakePipe.CurrentPressure += 0.5 * r;
					MainReservoir.CurrentPressure -= 0.5 * s;
				}
			}

			// refill auxillary reservoir from brake pipe
			if (BrakePipe.CurrentPressure > AuxiliaryReservoir.CurrentPressure + Tolerance)
			{
				double r = 2.0 * AuxiliaryReservoir.ChargeRate;
				double d = BrakePipe.CurrentPressure - AuxiliaryReservoir.CurrentPressure;
				double m = AuxiliaryReservoir.MaximumPressure;
				r = GetRate(d / m, r * timeElapsed);
				if (r > BrakePipe.CurrentPressure)
				{
					r = BrakePipe.CurrentPressure;
				}

				if (r > d) r = d;
				d = AuxiliaryReservoir.MaximumPressure - AuxiliaryReservoir.CurrentPressure;
				if (r > d) r = d;
				double f = AuxiliaryReservoir.BrakePipeCoefficient;
				double s = r / f;
				if (s > BrakePipe.CurrentPressure)
				{
					r *= BrakePipe.CurrentPressure / s;
					s = BrakePipe.CurrentPressure;
				}

				if (s > d)
				{
					r *= d / s;
					s = d;
				}

				AuxiliaryReservoir.CurrentPressure += 0.5 * r;
				BrakePipe.CurrentPressure -= 0.5 * s;
			}

			// electric command
			bool emergency = BrakePipe.CurrentPressure + Tolerance < AuxiliaryReservoir.CurrentPressure || Car.baseTrain.Handles.EmergencyBrake.Actual;

			double targetPressure;
			if (emergency)
			{
				//If EB is selected, then target pressure must be that required for EB
				targetPressure = BrakeCylinder.EmergencyMaximumPressure;
			}
			else
			{
				//Otherwise [BVE2 / BVE4 train.dat format] work out target pressure as a proportion of the max notch:
				targetPressure = brakeHandle.Actual / (double)brakeHandle.MaximumNotch;
				targetPressure *= BrakeCylinder.ServiceMaximumPressure;
			}

			if (BrakeCylinder.CurrentPressure > targetPressure + Tolerance | targetPressure == 0.0)
			{
				//BC pressure is greater than the target pressure, so release pressure
				double r = BrakeCylinder.ReleaseRate;
				double d = BrakeCylinder.CurrentPressure - targetPressure;
				double m = BrakeCylinder.EmergencyMaximumPressure;
				r = GetRate(d / m, r * timeElapsed);
				if (r > BrakeCylinder.CurrentPressure) r = BrakeCylinder.CurrentPressure;
				if (r > d) r = d;
				// air sound
				if (r > 0.0 & BrakeCylinder.CurrentPressure < BrakeCylinder.SoundPlayedForPressure)
				{
					BrakeCylinder.SoundPlayedForPressure = targetPressure;
					airSound = targetPressure < Tolerance ? AirZero : BrakeCylinder.CurrentPressure > m - Tolerance ? AirHigh : Air;
				}

				// pressure change
				BrakeCylinder.CurrentPressure -= r;
			}
			else if (BrakeCylinder.CurrentPressure + Tolerance < targetPressure)
			{
				//BC pressure is less than target pressure, so increase pressure
				double f = AuxiliaryReservoir.BrakeCylinderCoefficient;
				double r;
				if (emergency)
				{
					r = 2.0 * BrakeCylinder.EmergencyChargeRate * f;
				}
				else
				{
					r = 2.0 * BrakeCylinder.ServiceChargeRate * f;
				}

				double d = AuxiliaryReservoir.CurrentPressure - BrakeCylinder.CurrentPressure;
				double m = BrakeCylinder.EmergencyMaximumPressure;
				r = GetRate(d / m, r * timeElapsed);
				if (r > AuxiliaryReservoir.CurrentPressure)
				{
					r = AuxiliaryReservoir.CurrentPressure;
				}

				if (r > d) r = d;
				double s = r / f;
				if (s > d)
				{
					r *= d / s;
					s = d;
				}

				d = BrakeCylinder.EmergencyMaximumPressure - BrakeCylinder.CurrentPressure;
				if (s > d)
				{
					r *= d / s;
					s = d;
				}

				AuxiliaryReservoir.CurrentPressure -= 0.5 * r;
				BrakeCylinder.CurrentPressure += 0.5 * s;
				// air sound
				BrakeCylinder.SoundPlayedForPressure = BrakeCylinder.EmergencyMaximumPressure;
				// as the pressure is now *increasing* stop our decrease sounds
				AirHigh?.Stop();
				Air?.Stop();
				AirZero?.Stop();
			}
			else
			{
				// air sound
				BrakeCylinder.SoundPlayedForPressure = BrakeCylinder.EmergencyMaximumPressure;
			}

			double p;
			if (Car.baseTrain.Handles.EmergencyBrake.Actual)
			{
				p = 0.0;
			}
			else
			{
				p = brakeHandle.Actual / (double)brakeHandle.MaximumNotch;
				p *= BrakeCylinder.ServiceMaximumPressure;
			}

			double pressureratio = BrakeCylinder.CurrentPressure / BrakeCylinder.ServiceMaximumPressure;
			if (pressureratio == 0)
			{
				// we shouldn't really have zero pressure in the BC, but if we do multiplying by zero gives NaN
				// this fudges everything else up
				deceleration = 0;
			}
			else
			{
				deceleration = pressureratio * DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed);
			}
		}

		public override void Initialize(TrainStartMode startMode)
		{
			switch (startMode)
			{
				case TrainStartMode.ServiceBrakesAts:
					BrakeCylinder.CurrentPressure = BrakeCylinder.ServiceMaximumPressure;
					BrakePipe.CurrentPressure = BrakePipe.NormalPressure;
					EqualizingReservoir.CurrentPressure = EqualizingReservoir.NormalPressure;
					break;
				case TrainStartMode.EmergencyBrakesAts:
					BrakeCylinder.CurrentPressure = BrakeCylinder.EmergencyMaximumPressure;
					BrakePipe.CurrentPressure = 0.0;
					EqualizingReservoir.CurrentPressure = 0.0;
					break;
				default:
					BrakeCylinder.CurrentPressure = BrakeCylinder.EmergencyMaximumPressure;
					BrakePipe.CurrentPressure = 0.0;
					EqualizingReservoir.CurrentPressure = 0.0;
					break;
			}
		}
	}
}
