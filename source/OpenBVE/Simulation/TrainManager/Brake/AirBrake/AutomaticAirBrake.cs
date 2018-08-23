namespace OpenBve.BrakeSystems
{
	class AutomaticAirBrake : CarBrake
	{
		internal AutomaticAirBrake(EletropneumaticBrakeType type, TrainManager.EmergencyHandle EmergencyHandle, TrainManager.ReverserHandle ReverserHandle, bool IsMotorCar, double BrakeControlSpeed, double MotorDeceleration, TrainManager.AccelerationCurve[] DecelerationCurves)
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
			//First update the main reservoir pressure from the equalizing reservoir
			TrainManager.AirBrakeHandleState state = (TrainManager.AirBrakeHandleState)brakeHandle.Actual;
			switch (state)
			{
				case TrainManager.AirBrakeHandleState.Service:
				{
					double r = equalizingReservoir.ServiceRate; //50000
					double d = equalizingReservoir.CurrentPressure;
					double m = equalizingReservoir.NormalPressure; //1.05 * max service pressure from train.dat in pascals
					r = GetRate(d / m, r * TimeElapsed);
					if (r > equalizingReservoir.CurrentPressure)
					{
						r = equalizingReservoir.CurrentPressure;
					}

					equalizingReservoir.CurrentPressure -= r;
					break;
				}

				case TrainManager.AirBrakeHandleState.Release:
				{
					double r = equalizingReservoir.ChargeRate;
					double d = equalizingReservoir.NormalPressure - equalizingReservoir.CurrentPressure;
					double m = equalizingReservoir.NormalPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > d)
					{
						r = d;
					}

					d = mainReservoir.CurrentPressure - equalizingReservoir.CurrentPressure;
					if (r > d)
					{
						r = d;
					}

					double f = mainReservoir.EqualizingReservoirCoefficient;
					double s = r * f * TimeElapsed;
					if (s > mainReservoir.CurrentPressure)
					{
						r *= mainReservoir.CurrentPressure / s;
						s = mainReservoir.CurrentPressure;
					}

					equalizingReservoir.CurrentPressure += 0.5 * r;
					mainReservoir.CurrentPressure -= 0.5 * s;
					break;
				}
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

			if (brakePipe.CurrentPressure + Tolerance < auxiliaryReservoir.CurrentPressure)
				{
					if (auxiliaryReservoir.CurrentPressure + Tolerance < brakeCylinder.CurrentPressure)
					{
						// back-flow from brake cylinder to auxillary reservoir
						double u = (brakeCylinder.CurrentPressure - auxiliaryReservoir.CurrentPressure - Tolerance) / Tolerance;
						if (u > 1.0) u = 1.0;
						double f = auxiliaryReservoir.BrakeCylinderCoefficient;
						double r = brakeCylinder.ServiceChargeRate * f;
						double d = brakeCylinder.CurrentPressure - auxiliaryReservoir.CurrentPressure;
						double m = auxiliaryReservoir.MaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
						if (auxiliaryReservoir.CurrentPressure + r > m)
						{
							r = m - auxiliaryReservoir.CurrentPressure;
						}
						if (r > d) r = d;
						double s = r / f;
						if (s > d)
						{
							r *= d / s;
							s = d;
						}
						if (s > brakeCylinder.CurrentPressure)
						{
							r *= brakeCylinder.CurrentPressure / s;
							s = brakeCylinder.CurrentPressure;
						}
						auxiliaryReservoir.CurrentPressure += 0.5 * r;
						brakeCylinder.CurrentPressure -= 0.5 * s;
					}
					else if (auxiliaryReservoir.CurrentPressure > brakeCylinder.CurrentPressure + Tolerance)
					{
						// refill brake cylinder from auxillary reservoir
						double u = (auxiliaryReservoir.CurrentPressure - brakeCylinder.CurrentPressure - Tolerance) / Tolerance;
						if (u > 1.0) u = 1.0;
						double f = auxiliaryReservoir.BrakeCylinderCoefficient;
						double r = brakeCylinder.ServiceChargeRate * f;
						double d = auxiliaryReservoir.CurrentPressure - brakeCylinder.CurrentPressure;
						double m = auxiliaryReservoir.MaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
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
					}
					// air sound
					brakeCylinder.SoundPlayedForPressure = brakeCylinder.EmergencyMaximumPressure;
				}
				else if (brakePipe.CurrentPressure > auxiliaryReservoir.CurrentPressure + Tolerance)
			{
				double u = (brakePipe.CurrentPressure - auxiliaryReservoir.CurrentPressure - Tolerance) / Tolerance;
				if (u > 1.0) u = 1.0;
				{
					// refill auxillary reservoir from brake pipe
					double r = auxiliaryReservoir.ChargeRate;
					double d = brakePipe.CurrentPressure - auxiliaryReservoir.CurrentPressure;
					double m = auxiliaryReservoir.MaximumPressure;
					r = GetRate(d * u / m, r * TimeElapsed);
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
				{
					// brake cylinder release
					double r = brakeCylinder.ReleaseRate;
					double d = brakeCylinder.CurrentPressure;
					double m = brakeCylinder.EmergencyMaximumPressure;
					r = GetRate(d * u / m, r * TimeElapsed);
					if (r > brakeCylinder.CurrentPressure) r = brakeCylinder.CurrentPressure;
					brakeCylinder.CurrentPressure -= r;
					// air sound
					if (r > 0.0 & brakeCylinder.CurrentPressure < brakeCylinder.SoundPlayedForPressure)
					{
						double p = 0.8 * brakeCylinder.CurrentPressure - 0.2 * brakeCylinder.EmergencyMaximumPressure;
						if (p < 0.0) p = 0.0;
						brakeCylinder.SoundPlayedForPressure = p;
						airSound = p < Tolerance ? AirSound.AirZero : brakeCylinder.CurrentPressure > m - Tolerance ? AirSound.AirHigh : AirSound.Air;
					}
				}
			}
			else
			{
				// air sound
				brakeCylinder.SoundPlayedForPressure = brakeCylinder.EmergencyMaximumPressure;
			}
			double pressureratio = brakeCylinder.CurrentPressure / brakeCylinder.ServiceMaximumPressure;
			deceleration = pressureratio * DecelerationAtServiceMaximumPressure(brakeHandle.Actual, currentSpeed);
		}
	}
}
