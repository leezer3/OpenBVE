﻿using System;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Power;

namespace TrainManager.BrakeSystems
{
	public class ElectricCommandBrake : CarBrake
	{
		public ElectricCommandBrake(EletropneumaticBrakeType type, CarBase car, double BrakeControlSpeed, double MotorDeceleration, double MotorDecelerationDelayUp, double MotorDecelerationDelayDown, AccelerationCurve[] DecelerationCurves) : base(car)
		{
			electropneumaticBrakeType = type;
			brakeControlSpeed = BrakeControlSpeed;
			motorDeceleration = MotorDeceleration;
			motorDecelerationDelayUp = MotorDecelerationDelayUp;
			motorDecelerationDelayDown = MotorDecelerationDelayDown;
			decelerationCurves = DecelerationCurves;
		}

		public override void Update(double TimeElapsed, double currentSpeed, AbstractHandle brakeHandle, out double deceleration)
		{
			airSound = null;
			double targetPressure;
			if (Car.baseTrain.Handles.EmergencyBrake.Actual)
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
				targetPressure = brakeHandle.Actual / (double)brakeHandle.MaximumNotch;
				targetPressure *= brakeCylinder.ServiceMaximumPressure;
			}

			if (!Car.baseTrain.Handles.EmergencyBrake.Actual & Car.baseTrain.Handles.Reverser.Actual != 0)
			{
				// brake control system
				if (Car.Specs.IsMotorCar & Math.Abs(currentSpeed) > brakeControlSpeed)
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
							double b = pr * DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed);
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
					airSound = targetPressure < Tolerance ? AirZero : brakeCylinder.CurrentPressure > m - Tolerance ? AirHigh : Air;
				}

				// pressure change
				brakeCylinder.CurrentPressure -= r;
			}
			else if ((brakeCylinder.CurrentPressure + Tolerance < targetPressure | targetPressure == brakeCylinder.EmergencyMaximumPressure) & brakeCylinder.CurrentPressure + Tolerance < mainReservoir.CurrentPressure)
			{
				// fill brake cylinder from main reservoir
				double r;
				if (Car.baseTrain.Handles.EmergencyBrake.Actual)
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
				// as the pressure is now *increasing* stop our decrease sounds
				AirHigh?.Stop();
				Air?.Stop();
				AirZero?.Stop();
			}
			else
			{
				// air sound
				brakeCylinder.SoundPlayedForPressure = brakeCylinder.EmergencyMaximumPressure;
			}
			double pp;
			if (Car.baseTrain.Handles.EmergencyBrake.Actual)
			{
				pp = brakeCylinder.EmergencyMaximumPressure;
			}
			else
			{
				pp = brakeHandle.Actual / (double)brakeHandle.MaximumNotch;
				pp *= brakeCylinder.ServiceMaximumPressure;
			}
			straightAirPipe.CurrentPressure = pp;
			double pressureratio = brakeCylinder.CurrentPressure / brakeCylinder.ServiceMaximumPressure;
			deceleration = pressureratio * DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed);
		}
		
		public override double CurrentMotorDeceleration(double TimeElapsed, AbstractHandle BrakeHandle)
		{
			double actualDeceleration = 0;
			if (lastHandlePosition != BrakeHandle.Actual)
			{
				motorDecelerationDelayTimer = BrakeHandle.Actual > lastHandlePosition ? motorDecelerationDelayUp : motorDecelerationDelayDown;
				lastHandlePosition = BrakeHandle.Actual;
			}
			if (BrakeHandle.Actual != 0)
			{
				motorDecelerationDelayTimer -= TimeElapsed;
				if (motorDecelerationDelayTimer < 0)
				{
					actualDeceleration = (BrakeHandle.Actual / (double)BrakeHandle.MaximumNotch) * motorDeceleration;
					lastMotorDeceleration = actualDeceleration;
				}
				else if (lastHandlePosition != 0)
				{
					actualDeceleration = lastMotorDeceleration;
				}
			}
			return actualDeceleration;
		}
	}
}
