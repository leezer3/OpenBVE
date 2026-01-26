using System;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Power;

namespace TrainManager.BrakeSystems
{
	public class ElectromagneticStraightAirBrake : AirBrake
	{
		public ElectromagneticStraightAirBrake(EletropneumaticBrakeType type, CarBase car) : base(car, new AccelerationCurve[] { })
		{
			electropneumaticBrakeType = type;
			BrakeControlSpeed = 0;
			motorDeceleration = 0;
			motorDecelerationDelayUp = 0;
			motorDecelerationDelayDown = 0;
		}
		public ElectromagneticStraightAirBrake(EletropneumaticBrakeType type, CarBase car, double brakeControlSpeed, double MotorDeceleration, double MotorDecelerationDelayUp, double MotorDecelerationDelayDown, AccelerationCurve[] decelerationCurves) : base(car, decelerationCurves)
		{
			electropneumaticBrakeType = type;
			BrakeControlSpeed = brakeControlSpeed;
			motorDeceleration = MotorDeceleration;
			motorDecelerationDelayUp = MotorDecelerationDelayUp;
			motorDecelerationDelayDown = MotorDecelerationDelayDown;
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

			// refill auxiliary reservoir from brake pipe
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
				targetPressure = brakeHandle.Actual / (double) brakeHandle.MaximumNotch;
				targetPressure *= BrakeCylinder.ServiceMaximumPressure;
			}

			if (Car.TractionModel.ProvidesPower & !Car.baseTrain.Handles.EmergencyBrake.Actual & Car.baseTrain.Handles.Reverser.Actual != 0)
			{
				//If we meet the conditions for brake control system to activate
				if (Math.Abs(currentSpeed) > BrakeControlSpeed)
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
						double pr = targetPressure / BrakeCylinder.ServiceMaximumPressure;
						double b = pr * DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed);

						double d = b - a;
						if (d > 0.0)
						{
							//Deceleration provided by the motor is not enough, so increase the BC target pressure
							targetPressure = d / DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed);
							if (targetPressure > 1.0) targetPressure = 1.0;
							targetPressure *= BrakeCylinder.ServiceMaximumPressure;
						}
						else
						{
							//Motor deceleration is enough, BC target pressure to zero
							targetPressure = 0.0;
						}
					}
				}
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

			if (p + Tolerance < StraightAirPipe.CurrentPressure)
			{
				double r = Car.baseTrain.Handles.EmergencyBrake.Actual ? StraightAirPipe.EmergencyRate : StraightAirPipe.ReleaseRate;
				double d = StraightAirPipe.CurrentPressure - p;
				double m = BrakeCylinder.EmergencyMaximumPressure;
				r = GetRate(d / m, r * timeElapsed);
				if (r > d) r = d;
				StraightAirPipe.CurrentPressure -= r;
			}
			else if (p > StraightAirPipe.CurrentPressure + Tolerance)
			{
				double r = StraightAirPipe.ServiceRate;
				double d = p - StraightAirPipe.CurrentPressure;
				double m = BrakeCylinder.EmergencyMaximumPressure;
				r = GetRate(d / m, r * timeElapsed);
				if (r > d) r = d;
				StraightAirPipe.CurrentPressure += r;
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
