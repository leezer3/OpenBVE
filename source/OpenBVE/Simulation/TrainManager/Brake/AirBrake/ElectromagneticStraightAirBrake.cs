using System;

namespace OpenBve.BrakeSystems
{
	class ElectromagneticStraightAirBrake : CarBrake
	{
		internal ElectromagneticStraightAirBrake(EletropneumaticBrakeType type, TrainManager.EmergencyHandle EmergencyHandle, TrainManager.ReverserHandle ReverserHandle, bool IsMotorCar, double BrakeControlSpeed, double MotorDeceleration, TrainManager.AccelerationCurve[] DecelerationCurves)
		{
			electropneumaticBrakeType = type;
			emergencyHandle = EmergencyHandle;
			reverserHandle = ReverserHandle;
			isMotorCar = IsMotorCar;
			brakeControlSpeed = BrakeControlSpeed;
			motorDeceleration = MotorDeceleration;
			decelerationCurves = DecelerationCurves;
		}

		internal override void Update(double TimeElapsed, double currentSpeed, TrainManager.AbstractHandle brakeHandle, out double deceleration)
		{
			airSound = AirSound.None;
			if (emergencyHandle.Actual == true)
			{
				if (brakeType == BrakeType.Main)
				{
					double r = equalizingReservoir.EmergencyRate;
					double d = equalizingReservoir.CurrentPressure;
					double m = equalizingReservoir.NormalPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > equalizingReservoir.CurrentPressure)
					{
						r = equalizingReservoir.CurrentPressure;
					}
					equalizingReservoir.CurrentPressure -= r;
				}
			}
			//First update the main reservoir pressure
			{
				double r = equalizingReservoir.ChargeRate;
				double d = equalizingReservoir.NormalPressure - equalizingReservoir.CurrentPressure;
				double m = equalizingReservoir.NormalPressure;
				r = GetRate(d / m, r * TimeElapsed);
				if (r > d) r = d;
				d = mainReservoir.CurrentPressure - equalizingReservoir.CurrentPressure;
				if (r > d) r = d;
				double f = mainReservoir.EqualizingReservoirCoefficient;
				double s = r * f * TimeElapsed;
				if (s > mainReservoir.CurrentPressure)
				{
					r *= mainReservoir.CurrentPressure / s;
					s = mainReservoir.CurrentPressure;
				}

				equalizingReservoir.CurrentPressure += 0.5 * r;
				mainReservoir.CurrentPressure -= 0.5 * s;
			}
			//Fill the brake pipe from the main reservoir
			if (brakeType == BrakeType.Main)
			{
				if (brakePipe.CurrentPressure > equalizingReservoir.CurrentPressure + Tolerance)
				{
					// brake pipe exhaust valve
					double r = emergencyHandle.Actual ? brakePipe.EmergencyRate : brakePipe.ServiceRate;
					double d = brakePipe.CurrentPressure - equalizingReservoir.CurrentPressure;
					double m = equalizingReservoir.NormalPressure;
					r = (0.5 + 1.5 * d / m) * r * TimeElapsed;
					if (r > d) r = d;
					brakePipe.CurrentPressure -= r;
				}
				else if (brakePipe.CurrentPressure + Tolerance < equalizingReservoir.CurrentPressure)
				{
					// fill brake pipe from main reservoir
					double r = brakePipe.ChargeRate;
					double d = equalizingReservoir.CurrentPressure - brakePipe.CurrentPressure;
					double m = equalizingReservoir.NormalPressure;
					r = (0.5 + 1.5 * d / m) * r * TimeElapsed;
					if (r > d) r = d;
					d = brakePipe.NormalPressure - brakePipe.CurrentPressure;
					if (r > d) r = d;
					double f = mainReservoir.BrakePipeCoefficient;
					double s = r * f;
					if (s > mainReservoir.CurrentPressure)
					{
						r *= mainReservoir.CurrentPressure / s;
						s = mainReservoir.CurrentPressure;
					}
					brakePipe.CurrentPressure += 0.5 * r;
					mainReservoir.CurrentPressure -= 0.5 * s;
				}
			}

			// refill auxillary reservoir from brake pipe
			if (brakePipe.CurrentPressure > auxiliaryReservoir.CurrentPressure + Tolerance)
			{
				double r = 2.0 * auxiliaryReservoir.ChargeRate;
				double d = brakePipe.CurrentPressure - auxiliaryReservoir.CurrentPressure;
				double m = auxiliaryReservoir.MaximumPressure;
				r = GetRate(d / m, r * TimeElapsed);
				if (r > brakePipe.CurrentPressure)
				{
					r = brakePipe.CurrentPressure;
				}

				if (r > d) r = d;
				d = auxiliaryReservoir.MaximumPressure - auxiliaryReservoir.CurrentPressure;
				if (r > d) r = d;
				double f = auxiliaryReservoir.BrakePipeCoefficient;
				double s = r / f;
				if (s > brakePipe.CurrentPressure)
				{
					r *= brakePipe.CurrentPressure / s;
					s = brakePipe.CurrentPressure;
				}

				if (s > d)
				{
					r *= d / s;
					s = d;
				}

				auxiliaryReservoir.CurrentPressure += 0.5 * r;
				brakePipe.CurrentPressure -= 0.5 * s;
			}

			// electric command
			bool emergency;
			if (brakePipe.CurrentPressure + Tolerance < auxiliaryReservoir.CurrentPressure)
			{
				emergency = true;
			}
			else
			{
				emergency = emergencyHandle.Actual;
			}

			double targetPressure;
			if (emergency)
			{
				//If EB is selected, then target pressure must be that required for EB
				targetPressure = brakeCylinder.EmergencyMaximumPressure;
			}
			else
			{
				//Otherwise [BVE2 / BVE4 train.dat format] work out target pressure as a proportion of the max notch:
				targetPressure = (double) brakeHandle.Actual / (double) brakeHandle.MaximumNotch;
				targetPressure *= brakeCylinder.ServiceMaximumPressure;
			}

			if (isMotorCar & !emergencyHandle.Actual & reverserHandle.Actual != 0)
			{
				//If we meet the conditions for brake control system to activate
				if (Math.Abs(currentSpeed) > brakeControlSpeed)
				{
					if (electropneumaticBrakeType == EletropneumaticBrakeType.ClosingElectromagneticValve)
					{
						//When above the brake control speed, pressure to the BC is nil & electric brakes are used
						//Thus target pressure must be zero
						targetPressure = 0.0;
					}
					else if (electropneumaticBrakeType == EletropneumaticBrakeType.DelayFillingControl)
					{
						//Motor is used to brake the train, until not enough deceleration, at which point the air brake is also used
						double a = motorDeceleration;
						double pr = targetPressure / brakeCylinder.ServiceMaximumPressure;
						double b;
						b = pr * DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed);

						double d = b - a;
						if (d > 0.0)
						{
							//Deceleration provided by the motor is not enough, so increase the BC target pressure
							targetPressure = d / DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed);
							if (targetPressure > 1.0) targetPressure = 1.0;
							targetPressure *= brakeCylinder.ServiceMaximumPressure;
						}
						else
						{
							//Motor deceleration is enough, BC target pressure to zero
							targetPressure = 0.0;
						}
					}
				}
			}

			if (brakeCylinder.CurrentPressure > targetPressure + Tolerance | targetPressure == 0.0)
			{
				//BC pressure is greater than the target pressure, so release pressure
				double r = brakeCylinder.ReleaseRate;
				double d = brakeCylinder.CurrentPressure - targetPressure;
				double m = brakeCylinder.EmergencyMaximumPressure;
				r = GetRate(d / m, r * TimeElapsed);
				if (r > brakeCylinder.CurrentPressure) r = brakeCylinder.CurrentPressure;
				if (r > d) r = d;
				// air sound
				if (r > 0.0 & brakeCylinder.CurrentPressure < brakeCylinder.SoundPlayedForPressure)
				{
					brakeCylinder.SoundPlayedForPressure = targetPressure;
					airSound = targetPressure < Tolerance ? AirSound.AirZero : brakeCylinder.CurrentPressure > m - Tolerance ? AirSound.AirHigh : AirSound.Air;
				}

				// pressure change
				brakeCylinder.CurrentPressure -= r;
			}
			else if (brakeCylinder.CurrentPressure + Tolerance < targetPressure)
			{
				//BC pressure is less than target pressure, so increase pressure
				double f = auxiliaryReservoir.BrakeCylinderCoefficient;
				double r;
				if (emergency)
				{
					r = 2.0 * brakeCylinder.EmergencyChargeRate * f;
				}
				else
				{
					r = 2.0 * brakeCylinder.ServiceChargeRate * f;
				}

				double d = auxiliaryReservoir.CurrentPressure - brakeCylinder.CurrentPressure;
				double m = brakeCylinder.EmergencyMaximumPressure;
				r = GetRate(d / m, r * TimeElapsed);
				if (r > auxiliaryReservoir.CurrentPressure)
				{
					r = auxiliaryReservoir.CurrentPressure;
				}

				if (r > d) r = d;
				double s = r / f;
				if (s > d)
				{
					r *= d / s;
					s = d;
				}

				d = brakeCylinder.EmergencyMaximumPressure - brakeCylinder.CurrentPressure;
				if (s > d)
				{
					r *= d / s;
					s = d;
				}

				auxiliaryReservoir.CurrentPressure -= 0.5 * r;
				brakeCylinder.CurrentPressure += 0.5 * s;
				// air sound
				brakeCylinder.SoundPlayedForPressure = brakeCylinder.EmergencyMaximumPressure;
			}
			else
			{
				// air sound
				brakeCylinder.SoundPlayedForPressure = brakeCylinder.EmergencyMaximumPressure;
			}

			double p;
			if (emergencyHandle.Actual)
			{
				p = 0.0;
			}
			else
			{
				p = (double) brakeHandle.Actual / (double) brakeHandle.MaximumNotch;
				p *= brakeCylinder.ServiceMaximumPressure;
			}

			if (p + Tolerance < straightAirPipe.CurrentPressure)
			{
				double r;
				if (emergencyHandle.Actual)
				{
					r = straightAirPipe.EmergencyRate;
				}
				else
				{
					r = straightAirPipe.ReleaseRate;
				}

				double d = straightAirPipe.CurrentPressure - p;
				double m = brakeCylinder.EmergencyMaximumPressure;
				r = GetRate(d / m, r * TimeElapsed);
				if (r > d) r = d;
				straightAirPipe.CurrentPressure -= r;
			}
			else if (p > straightAirPipe.CurrentPressure + Tolerance)
			{
				double r = straightAirPipe.ServiceRate;
				double d = p - straightAirPipe.CurrentPressure;
				double m = brakeCylinder.EmergencyMaximumPressure;
				r = GetRate(d / m, r * TimeElapsed);
				if (r > d) r = d;
				straightAirPipe.CurrentPressure += r;
			}

			double pressureratio = brakeCylinder.CurrentPressure / brakeCylinder.ServiceMaximumPressure;
			deceleration = pressureratio * DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed);
		}
	}
}
