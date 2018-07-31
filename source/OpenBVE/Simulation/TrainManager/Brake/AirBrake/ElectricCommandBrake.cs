using System;

namespace OpenBve.BrakeSystems
{
	class ElectricCommandBrake : CarBrake
	{
		internal ElectricCommandBrake(EletropneumaticBrakeType type, TrainManager.EmergencyHandle EmergencyHandle, TrainManager.ReverserHandle ReverserHandle, bool IsMotorCar, double BrakeControlSpeed, double MotorDeceleration, TrainManager.AccelerationCurve[] DecelerationCurves)
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
			double targetPressure;
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
				targetPressure = brakeCylinder.EmergencyMaximumPressure;
			}
			else
			{
				targetPressure = (double) brakeHandle.Actual / (double) brakeHandle.MaximumNotch;
				targetPressure *= brakeCylinder.ServiceMaximumPressure;
			}

			if (!emergencyHandle.Actual & reverserHandle.Actual != 0)
			{
				// brake control system
				if (isMotorCar & Math.Abs(currentSpeed) > brakeControlSpeed)
				{
					switch (electropneumaticBrakeType)
					{
						case EletropneumaticBrakeType.ClosingElectromagneticValve:
							// closing electromagnetic valve (lock-out valve)
							targetPressure = 0.0;
							break;
						case EletropneumaticBrakeType.DelayFillingControl:
							// delay-filling control
							double a = motorDeceleration;
							double pr = targetPressure / brakeCylinder.ServiceMaximumPressure;
							double b;
							b = pr * DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed);
							double d = b - a;
							if (d > 0.0)
							{
								targetPressure = d / DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed);
								if (targetPressure > 1.0) targetPressure = 1.0;
								targetPressure *= brakeCylinder.ServiceMaximumPressure;
							}
							else
							{
								targetPressure = 0.0;
							}

							break;
					}
				}
			}

			if (brakeCylinder.CurrentPressure > targetPressure + Tolerance | targetPressure == 0.0)
			{
				// brake cylinder exhaust valve
				double r = brakeCylinder.ReleaseRate;
				double d = brakeCylinder.CurrentPressure;
				double m = brakeCylinder.EmergencyMaximumPressure;
				r = GetRate(d / m, r * TimeElapsed);
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
			else if ((brakeCylinder.CurrentPressure + Tolerance < targetPressure | targetPressure == brakeCylinder.EmergencyMaximumPressure) & brakeCylinder.CurrentPressure + Tolerance < mainReservoir.CurrentPressure)
			{
				// fill brake cylinder from main reservoir
				double r;
				if (emergencyHandle.Actual)
				{
					r = 2.0 * brakeCylinder.EmergencyChargeRate;
				}
				else
				{
					r = 2.0 * brakeCylinder.ServiceChargeRate;
				}

				double pm = targetPressure < mainReservoir.CurrentPressure ? targetPressure : mainReservoir.CurrentPressure;
				double d = pm - brakeCylinder.CurrentPressure;
				double m = brakeCylinder.EmergencyMaximumPressure;
				r = GetRate(d / m, r * TimeElapsed);
				if (r > d) r = d;
				double f1 = auxiliaryReservoir.BrakeCylinderCoefficient;
				double f2 = mainReservoir.BrakePipeCoefficient;
				double f3 = auxiliaryReservoir.BrakePipeCoefficient;
				double f = f1 * f2 / f3; // MainReservoirBrakeCylinderCoefficient
				double s = r * f;
				if (s > mainReservoir.CurrentPressure)
				{
					r *= mainReservoir.CurrentPressure / s;
					s = mainReservoir.CurrentPressure;
				}

				brakeCylinder.CurrentPressure += 0.5 * r;
				mainReservoir.CurrentPressure -= 0.5 * s;
				// air sound
				brakeCylinder.SoundPlayedForPressure = brakeCylinder.EmergencyMaximumPressure;
			}
			else
			{
				// air sound
				brakeCylinder.SoundPlayedForPressure = brakeCylinder.EmergencyMaximumPressure;
			}
			double pp;
			if (emergencyHandle.Actual)
			{
				pp = brakeCylinder.EmergencyMaximumPressure;
			}
			else
			{
				pp = (double)brakeHandle.Actual / (double)brakeHandle.MaximumNotch;
				pp *= brakeCylinder.ServiceMaximumPressure;
			}
			straightAirPipe.CurrentPressure = pp;
			double pressureratio = brakeCylinder.CurrentPressure / brakeCylinder.ServiceMaximumPressure;
			deceleration = pressureratio * DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed);
		}
	}
}
