using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Power;

namespace TrainManager.BrakeSystems
{
	public class AutomaticAirBrake : AirBrake
	{
		public AutomaticAirBrake(EletropneumaticBrakeType type, CarBase car) : base(car, new AccelerationCurve[] {})
		{
			electropneumaticBrakeType = type;
			BrakeControlSpeed = 0;
			motorDeceleration = 0;
		}

		public AutomaticAirBrake(EletropneumaticBrakeType type, CarBase car, double BrakeControlSpeed, double MotorDeceleration, AccelerationCurve[] DecelerationCurves) : base(car, DecelerationCurves)
		{
			electropneumaticBrakeType = type;
			this.BrakeControlSpeed = BrakeControlSpeed;
			motorDeceleration = MotorDeceleration;
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
			//First update the main reservoir pressure from the equalizing reservoir
			AirBrakeHandleState state = (AirBrakeHandleState)brakeHandle.Actual;
			switch (state)
			{
				case AirBrakeHandleState.Service:
				{
					double r = EqualizingReservoir.ServiceRate; //50000
					double d = EqualizingReservoir.CurrentPressure;
					double m = EqualizingReservoir.NormalPressure; //1.05 * max service pressure from train.dat in pascals
					r = GetRate(d / m, r * timeElapsed);
					if (r > EqualizingReservoir.CurrentPressure)
					{
						r = EqualizingReservoir.CurrentPressure;
					}

					EqualizingReservoir.CurrentPressure -= r;
					break;
				}

				case AirBrakeHandleState.Release:
				{
					double r = EqualizingReservoir.ChargeRate;
					double d = EqualizingReservoir.NormalPressure - EqualizingReservoir.CurrentPressure;
					double m = EqualizingReservoir.NormalPressure;
					r = GetRate(d / m, r * timeElapsed);
					if (r > d)
					{
						r = d;
					}

					d = MainReservoir.CurrentPressure - EqualizingReservoir.CurrentPressure;
					if (r > d)
					{
						r = d;
					}

					double f = MainReservoir.EqualizingReservoirCoefficient;
					double s = r * f * timeElapsed;
					if (s > MainReservoir.CurrentPressure)
					{
						r *= MainReservoir.CurrentPressure / s;
						s = MainReservoir.CurrentPressure;
					}

					EqualizingReservoir.CurrentPressure += 0.5 * r;
					MainReservoir.CurrentPressure -= 0.5 * s;
					break;
				}
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

			if (BrakePipe.CurrentPressure + Tolerance < AuxiliaryReservoir.CurrentPressure)
			{
				if (AuxiliaryReservoir.CurrentPressure + Tolerance < BrakeCylinder.CurrentPressure)
				{
					// back-flow from brake cylinder to auxillary reservoir
					double u = (BrakeCylinder.CurrentPressure - AuxiliaryReservoir.CurrentPressure - Tolerance) / Tolerance;
					if (u > 1.0) u = 1.0;
					double f = AuxiliaryReservoir.BrakeCylinderCoefficient;
					double r = BrakeCylinder.ServiceChargeRate * f;
					double d = BrakeCylinder.CurrentPressure - AuxiliaryReservoir.CurrentPressure;
					double m = AuxiliaryReservoir.MaximumPressure;
					r = GetRate(d * u / m, r * timeElapsed);
					if (AuxiliaryReservoir.CurrentPressure + r > m)
					{
						r = m - AuxiliaryReservoir.CurrentPressure;
					}

					if (r > d) r = d;
					double s = r / f;
					if (s > d)
					{
						r *= d / s;
						s = d;
					}

					if (s > BrakeCylinder.CurrentPressure)
					{
						r *= BrakeCylinder.CurrentPressure / s;
						s = BrakeCylinder.CurrentPressure;
					}

					AuxiliaryReservoir.CurrentPressure += 0.5 * r;
					BrakeCylinder.CurrentPressure -= 0.5 * s;
				}
				else if (AuxiliaryReservoir.CurrentPressure > BrakeCylinder.CurrentPressure + Tolerance)
				{
					// refill brake cylinder from auxillary reservoir
					double u = (AuxiliaryReservoir.CurrentPressure - BrakeCylinder.CurrentPressure - Tolerance) / Tolerance;
					if (u > 1.0) u = 1.0;
					double f = AuxiliaryReservoir.BrakeCylinderCoefficient;
					double r = BrakeCylinder.ServiceChargeRate * f;
					double d = AuxiliaryReservoir.CurrentPressure - BrakeCylinder.CurrentPressure;
					double m = AuxiliaryReservoir.MaximumPressure;
					r = GetRate(d * u / m, r * timeElapsed);
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
					// as the pressure is now *increasing* stop our decrease sounds
					AirHigh?.Stop();
					Air?.Stop();
					AirZero?.Stop();
				}

				// air sound
				BrakeCylinder.SoundPlayedForPressure = BrakeCylinder.EmergencyMaximumPressure;
			}
			else if (BrakePipe.CurrentPressure > AuxiliaryReservoir.CurrentPressure + Tolerance)
			{
				double u = (BrakePipe.CurrentPressure - AuxiliaryReservoir.CurrentPressure - Tolerance) / Tolerance;
				if (u > 1.0) u = 1.0;
				{
					// refill auxillary reservoir from brake pipe
					double r = AuxiliaryReservoir.ChargeRate;
					double d = BrakePipe.CurrentPressure - AuxiliaryReservoir.CurrentPressure;
					double m = AuxiliaryReservoir.MaximumPressure;
					r = GetRate(d * u / m, r * timeElapsed);
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
				{
					// brake cylinder release
					double r = BrakeCylinder.ReleaseRate;
					double d = BrakeCylinder.CurrentPressure;
					double m = BrakeCylinder.EmergencyMaximumPressure;
					r = GetRate(d * u / m, r * timeElapsed);
					if (r > BrakeCylinder.CurrentPressure) r = BrakeCylinder.CurrentPressure;
					BrakeCylinder.CurrentPressure -= r;
					// air sound
					if (r > 0.0 & BrakeCylinder.CurrentPressure < BrakeCylinder.SoundPlayedForPressure)
					{
						double p = 0.8 * BrakeCylinder.CurrentPressure - 0.2 * BrakeCylinder.EmergencyMaximumPressure;
						if (p < 0.0) p = 0.0;
						BrakeCylinder.SoundPlayedForPressure = p;
						airSound = p < Tolerance ? AirZero : BrakeCylinder.CurrentPressure > m - Tolerance ? AirHigh : Air;
					}
				}
			}
			else
			{
				// air sound
				BrakeCylinder.SoundPlayedForPressure = BrakeCylinder.EmergencyMaximumPressure;
			}
			double pressureratio = BrakeCylinder.CurrentPressure / BrakeCylinder.ServiceMaximumPressure;
			deceleration = pressureratio != 0 ? pressureratio * DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed) : 0;
		}
	}
}
