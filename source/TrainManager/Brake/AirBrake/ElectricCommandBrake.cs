﻿using System;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Power;

namespace TrainManager.BrakeSystems
{
	public class ElectricCommandBrake : CarBrake
	{
		public ElectricCommandBrake(EletropneumaticBrakeType type, CarBase car, double brakeControlSpeed, double motorDeceleration, double motorDecelerationDelayUp, double motorDecelerationDelayDown, AccelerationCurve[] decelerationCurves) : base(car)
		{
			ElectropneumaticBrakeType = type;
			BrakeControlSpeed = brakeControlSpeed;
			MotorDeceleration = motorDeceleration;
			MotorDecelerationDelayUp = motorDecelerationDelayUp;
			MotorDecelerationDelayDown = motorDecelerationDelayDown;
			DecelerationCurves = decelerationCurves;
		}

		public override void Update(double timeElapsed, double currentSpeed, AbstractHandle brakeHandle, out double deceleration)
		{
			AirSound = null;
			double targetPressure;
			if (Car.BaseTrain.Handles.EmergencyBrake.Actual)
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
				targetPressure = BrakeCylinder.EmergencyMaximumPressure;
			}
			else
			{
				targetPressure = brakeHandle.Actual / (double)brakeHandle.MaximumNotch;
				targetPressure *= BrakeCylinder.ServiceMaximumPressure;
			}

			if (!Car.BaseTrain.Handles.EmergencyBrake.Actual & Car.BaseTrain.Handles.Reverser.Actual != 0)
			{
				// brake control system
				if (Car.Specs.IsMotorCar & Math.Abs(currentSpeed) > BrakeControlSpeed)
				{
					switch (ElectropneumaticBrakeType)
					{
						case EletropneumaticBrakeType.ClosingElectromagneticValve:
							// closing electromagnetic valve (lock-out valve)
							targetPressure = 0.0;
							break;
						case EletropneumaticBrakeType.DelayFillingControl:
							// delay-filling control
							double a = MotorDeceleration;
							double pr = targetPressure / BrakeCylinder.ServiceMaximumPressure;
							double b = pr * DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed);
							double d = b - a;
							if (d > 0.0)
							{
								targetPressure = d / DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed);
								if (targetPressure > 1.0) targetPressure = 1.0;
								targetPressure *= BrakeCylinder.ServiceMaximumPressure;
							}
							else
							{
								targetPressure = 0.0;
							}

							break;
					}
				}
			}

			if (BrakeCylinder.CurrentPressure > targetPressure + Tolerance | targetPressure == 0.0)
			{
				// brake cylinder exhaust valve
				double r = BrakeCylinder.ReleaseRate;
				double d = BrakeCylinder.CurrentPressure;
				double m = BrakeCylinder.EmergencyMaximumPressure;
				r = GetRate(d / m, r * timeElapsed);
				if (r > d) r = d;
				// air sound
				if (r > 0.0 & BrakeCylinder.CurrentPressure < BrakeCylinder.SoundPlayedForPressure)
				{
					BrakeCylinder.SoundPlayedForPressure = targetPressure;
					AirSound = targetPressure < Tolerance ? AirZero : BrakeCylinder.CurrentPressure > m - Tolerance ? AirHigh : Air;
				}

				// pressure change
				BrakeCylinder.CurrentPressure -= r;
			}
			else if ((BrakeCylinder.CurrentPressure + Tolerance < targetPressure | targetPressure == BrakeCylinder.EmergencyMaximumPressure) & BrakeCylinder.CurrentPressure + Tolerance < MainReservoir.CurrentPressure)
			{
				// fill brake cylinder from main reservoir
				double r;
				if (Car.BaseTrain.Handles.EmergencyBrake.Actual)
				{
					r = 2.0 * BrakeCylinder.EmergencyChargeRate;
				}
				else
				{
					r = 2.0 * BrakeCylinder.ServiceChargeRate;
				}

				double pm = targetPressure < MainReservoir.CurrentPressure ? targetPressure : MainReservoir.CurrentPressure;
				double d = pm - BrakeCylinder.CurrentPressure;
				double m = BrakeCylinder.EmergencyMaximumPressure;
				r = GetRate(d / m, r * timeElapsed);
				if (r > d) r = d;
				double f1 = AuxiliaryReservoir.BrakeCylinderCoefficient;
				double f2 = MainReservoir.BrakePipeCoefficient;
				double f3 = AuxiliaryReservoir.BrakePipeCoefficient;
				double f = f1 * f2 / f3; // MainReservoirBrakeCylinderCoefficient
				double s = r * f;
				if (s > MainReservoir.CurrentPressure)
				{
					r *= MainReservoir.CurrentPressure / s;
					s = MainReservoir.CurrentPressure;
				}

				BrakeCylinder.CurrentPressure += 0.5 * r;
				MainReservoir.CurrentPressure -= 0.5 * s;
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
			double pp;
			if (Car.BaseTrain.Handles.EmergencyBrake.Actual)
			{
				pp = BrakeCylinder.EmergencyMaximumPressure;
			}
			else
			{
				pp = brakeHandle.Actual / (double)brakeHandle.MaximumNotch;
				pp *= BrakeCylinder.ServiceMaximumPressure;
			}
			StraightAirPipe.CurrentPressure = pp;
			double pressureratio = BrakeCylinder.CurrentPressure / BrakeCylinder.ServiceMaximumPressure;
			deceleration = pressureratio * DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed);
		}
		
		public override double CurrentMotorDeceleration(double timeElapsed, AbstractHandle brakeHandle)
		{
			double actualDeceleration = 0;
			if (LastHandlePosition != brakeHandle.Actual)
			{
				MotorDecelerationDelayTimer = brakeHandle.Actual > LastHandlePosition ? MotorDecelerationDelayUp : MotorDecelerationDelayDown;
				LastHandlePosition = brakeHandle.Actual;
			}
			if (brakeHandle.Actual != 0)
			{
				MotorDecelerationDelayTimer -= timeElapsed;
				if (MotorDecelerationDelayTimer < 0)
				{
					actualDeceleration = (brakeHandle.Actual / (double)brakeHandle.MaximumNotch) * MotorDeceleration;
					LastMotorDeceleration = actualDeceleration;
				}
				else if (LastHandlePosition != 0)
				{
					actualDeceleration = LastMotorDeceleration;
				}
			}
			return actualDeceleration;
		}
	}
}
