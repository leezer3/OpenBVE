using System;

namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>Updates the brake system for a complete train</summary>
		/// <param name="Train">The train to update the brake system values for</param>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		/// <param name="DecelerationDueToBrake">An array containing the deceleration due to brakes for each car in the train</param>
		/// <param name="DecelerationDueToMotor">An array containing the deceleration due to motor retardation for each car in the train</param>
		private static void UpdateBrakeSystem(Train Train, double TimeElapsed, out double[] DecelerationDueToBrake, out double[] DecelerationDueToMotor)
		{
			// individual brake systems
			DecelerationDueToBrake = new double[Train.Cars.Length];
			DecelerationDueToMotor = new double[Train.Cars.Length];
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				UpdateBrakeSystem(Train, i, TimeElapsed, out DecelerationDueToBrake[i], out DecelerationDueToMotor[i]);
			}
			// brake pipe pressure distribution dummy (just averages)
			double TotalPressure = 0.0;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				if (i > 0)
				{
					if (Train.Cars[i - 1].Derailed | Train.Cars[i].Derailed)
					{
						Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure -= Game.BrakePipeLeakRate * TimeElapsed;
						if (Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure < 0.0) Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure = 0.0;
					}
				}
				if (i < Train.Cars.Length - 1)
				{
					if (Train.Cars[i].Derailed | Train.Cars[i + 1].Derailed)
					{
						Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure -= Game.BrakePipeLeakRate * TimeElapsed;
						if (Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure < 0.0) Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure = 0.0;
					}
				}
				TotalPressure += Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure;
			}
			double AveragePressure = TotalPressure / (double)Train.Cars.Length;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				Train.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure = AveragePressure;
			}
		}

		/// <summary>Updates the brake system for a car within a train</summary>
		/// <param name="Train">The train</param>
		/// <param name="CarIndex">The induvidual car</param>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		/// <param name="DecelerationDueToBrake">The total brake deceleration this car provides</param>
		/// <param name="DecelerationDueToMotor">The total motor deceleration this car provides</param>
		private static void UpdateBrakeSystem(Train Train, int CarIndex, double TimeElapsed, out double DecelerationDueToBrake, out double DecelerationDueToMotor)
		{
			// initialize
			const double Tolerance = 5000.0;
			AirSound airsound = AirSound.None;
			if (Train.Cars[CarIndex].Specs.AirBrake.Type == AirBrakeType.Main)
			{
				Train.Cars[CarIndex].Specs.AirBrake.Compressor.Update(Train, CarIndex, TimeElapsed);
				Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoir.Update(Train, CarIndex, TimeElapsed);
			}
			if ((Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.AutomaticAirBrake | Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.ElectromagneticStraightAirBrake) & Train.Cars[CarIndex].Specs.AirBrake.Type == AirBrakeType.Main)
			{
				Train.Cars[CarIndex].Specs.AirBrake.MainReservoir.UpdateBrakePipe(Train, CarIndex, TimeElapsed);
			}
			// triple valve (auxillary reservoir, brake pipe, brake cylinder)
			if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.AutomaticAirBrake)
			{
				Train.Cars[CarIndex].Specs.AirBrake.UpdateAutomaticAirBrake(Train, CarIndex, TimeElapsed, ref airsound);
			}
			// solenoid valve for electromagnetic straight air brake (auxillary reservoir, electric command, brake cylinder)
			if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.ElectromagneticStraightAirBrake)
			{
				// refill auxillary reservoir from brake pipe
				if (Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure > Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure + Tolerance)
				{
					double r = 2.0 * Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirChargeRate;
					double d = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
					double m = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure)
					{
						r = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure;
					}
					if (r > d) r = d;
					d = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirMaximumPressure - Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
					if (r > d) r = d;
					double f = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirBrakePipeCoefficient;
					double s = r / f;
					if (s > Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure)
					{
						r *= Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure / s;
						s = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure;
					}
					if (s > d)
					{
						r *= d / s;
						s = d;
					}
					Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure += 0.5 * r;
					Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure -= 0.5 * s;
				}
				{ // electric command
					bool emergency;
					if (Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure + Tolerance < Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure)
					{
						emergency = true;
					}
					else
					{
						emergency = Train.Specs.CurrentEmergencyBrake.Actual;
					}
					double p; if (emergency)
					{
						p = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					}
					else
					{
						p = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
						p *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
					}
					if (Train.Cars[CarIndex].Specs.IsMotorCar & !Train.Specs.CurrentEmergencyBrake.Actual & Train.Specs.CurrentReverser.Actual != 0)
					{
						// brake control system
						if (Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed) > Train.Cars[CarIndex].Specs.BrakeControlSpeed)
						{
							if (Train.Cars[CarIndex].Specs.ElectropneumaticType == EletropneumaticBrakeType.ClosingElectromagneticValve)
							{
								// closing electromagnetic valve (lock-out valve)
								p = 0.0;
							}
							else if (Train.Cars[CarIndex].Specs.ElectropneumaticType == EletropneumaticBrakeType.DelayFillingControl)
							{
								// delay-filling control
								//Variable f is never used, so don't calculate it
								//double f = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
								double a = Train.Cars[CarIndex].Specs.MotorDeceleration;
								double pr = p / Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
								double b = pr * Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure;
								double d = b - a;
								if (d > 0.0)
								{
									p = d / Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure;
									if (p > 1.0) p = 1.0;
									p *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
								}
								else
								{
									p = 0.0;
								}
							}
						}
					}
					if (Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure > p + Tolerance | p == 0.0)
					{
						// brake cylinder release
						double r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderReleaseRate;
						double d = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure - p;
						double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
						r = GetRate(d / m, r * TimeElapsed);
						if (r > Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure) r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						if (r > d) r = d;
						// air sound
						if (r > 0.0 & Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure < Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure)
						{
							Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = p;
							airsound = (AirSound)(p < Tolerance ? 0 : Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure > m - Tolerance ? 2 : 1);
						}
						// pressure change
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure -= r;
					}
					else if (Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure + Tolerance < p)
					{
						// refill brake cylinder from auxillary reservoir
						double f = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirBrakeCylinderCoefficient;
						double r;
						if (emergency)
						{
							r = 2.0 * Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyChargeRate * f;
						}
						else
						{
							r = 2.0 * Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceChargeRate * f;
						}
						double d = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
						r = GetRate(d / m, r * TimeElapsed);
						if (r > Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure)
						{
							r = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
						}
						if (r > d) r = d;
						double s = r / f;
						if (s > d)
						{
							r *= d / s;
							s = d;
						}
						d = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure - Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						if (s > d)
						{
							r *= d / s;
							s = d;
						}
						Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure -= 0.5 * r;
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure += 0.5 * s;
						// air sound
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					}
					else
					{
						// air sound
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					}
				}
			}
			// valves for electric command brake (main reservoir, electric command, brake cylinder)
			if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.ElectricCommandBrake)
			{
				double p; if (Train.Specs.CurrentEmergencyBrake.Actual)
				{
					p = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
				}
				else
				{
					p = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
					p *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
				}
				if (!Train.Specs.CurrentEmergencyBrake.Actual & Train.Specs.CurrentReverser.Actual != 0)
				{
					// brake control system
					if (Train.Cars[CarIndex].Specs.IsMotorCar & Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed) > Train.Cars[CarIndex].Specs.BrakeControlSpeed)
					{
						if (Train.Cars[CarIndex].Specs.ElectropneumaticType == EletropneumaticBrakeType.ClosingElectromagneticValve)
						{
							// closing electromagnetic valve (lock-out valve)
							p = 0.0;
						}
						else if (Train.Cars[CarIndex].Specs.ElectropneumaticType == EletropneumaticBrakeType.DelayFillingControl)
						{
							// delay-filling control
							// Variable f is never used, so don't calculate it
							//double f = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
							double a = Train.Cars[CarIndex].Specs.MotorDeceleration;
							double pr = p / Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
							double b = pr * Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure;
							double d = b - a;
							if (d > 0.0)
							{
								p = d / Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure;
								if (p > 1.0) p = 1.0;
								p *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
							}
							else
							{
								p = 0.0;
							}
						}
					}
				}
				if (Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure > p + Tolerance | p == 0.0)
				{
					// brake cylinder exhaust valve
					double r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderReleaseRate;
					double d = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
					double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > d) r = d;
					// air sound
					if (r > 0.0 & Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure < Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure)
					{
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = p;
						airsound = (AirSound)(p < Tolerance ? 0 : Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure > m - Tolerance ? 2 : 1);
					}
					// pressure change
					Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure -= r;
				}
				else if ((Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure + Tolerance < p | p == Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure) & Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure + Tolerance < Train.Cars[CarIndex].Specs.AirBrake.MainReservoir.CurrentPressure)
				{
					// fill brake cylinder from main reservoir
					double r;
					if (Train.Specs.CurrentEmergencyBrake.Actual)
					{
						r = 2.0 * Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyChargeRate;
					}
					else
					{
						r = 2.0 * Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceChargeRate;
					}
					double pm = p < Train.Cars[CarIndex].Specs.AirBrake.MainReservoir.CurrentPressure ? p : Train.Cars[CarIndex].Specs.AirBrake.MainReservoir.CurrentPressure;
					double d = pm - Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
					double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > d) r = d;
					double f1 = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirBrakeCylinderCoefficient;
					double f2 = Train.Cars[CarIndex].Specs.AirBrake.MainReservoir.BrakePipeCoefficient;
					double f3 = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirBrakePipeCoefficient;
					double f = f1 * f2 / f3; // MainReservoirBrakeCylinderCoefficient
					double s = r * f;
					if (s > Train.Cars[CarIndex].Specs.AirBrake.MainReservoir.CurrentPressure)
					{
						r *= Train.Cars[CarIndex].Specs.AirBrake.MainReservoir.CurrentPressure / s;
						s = Train.Cars[CarIndex].Specs.AirBrake.MainReservoir.CurrentPressure;
					}
					Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure += 0.5 * r;
					Train.Cars[CarIndex].Specs.AirBrake.MainReservoir.CurrentPressure -= 0.5 * s;
					// air sound
					Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
				}
				else
				{
					// air sound
					Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
				}
			}
			// straight air pipe (for compatibility needle only)
			if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.ElectromagneticStraightAirBrake & Train.Cars[CarIndex].Specs.AirBrake.Type == AirBrakeType.Main)
			{
				double p; if (Train.Specs.CurrentEmergencyBrake.Actual)
				{
					p = 0.0;
				}
				else
				{
					p = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
					p *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
				}
				if (p + Tolerance < Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure)
				{
					double r;
					if (Train.Specs.CurrentEmergencyBrake.Actual)
					{
						r = Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeEmergencyRate;
					}
					else
					{
						r = Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeReleaseRate;
					}
					double d = Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure - p;
					double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > d) r = d;
					Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure -= r;
				}
				else if (p > Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure + Tolerance)
				{
					double r = Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeServiceRate;
					double d = p - Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure;
					double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > d) r = d;
					Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure += r;
				}
			}
			else if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.ElectricCommandBrake)
			{
				double p; if (Train.Specs.CurrentEmergencyBrake.Actual)
				{
					p = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
				}
				else
				{
					p = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
					p *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
				}
				Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure = p;
			}
			// air sound
			Sounds.SoundBuffer buffer;
			switch (airsound)
			{
				case AirSound.Zero:
					buffer = Train.Cars[CarIndex].Sounds.AirZero.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.AirZero.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, false);
					}
					break;
				case AirSound.Normal:
					buffer = Train.Cars[CarIndex].Sounds.Air.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.Air.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, false);
					}
					break;
				case AirSound.High:
					buffer = Train.Cars[CarIndex].Sounds.AirHigh.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.AirHigh.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, false);
					}
					break;
			}
			// deceleration provided by brake
			double pressureratio = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure / Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
			DecelerationDueToBrake = pressureratio * Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure;
			// deceleration provided by motor
			if (Train.Cars[CarIndex].Specs.BrakeType != CarBrakeType.AutomaticAirBrake && Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed) >= Train.Cars[CarIndex].Specs.BrakeControlSpeed & Train.Specs.CurrentReverser.Actual != 0 & !Train.Specs.CurrentEmergencyBrake.Actual)
			{
				double f = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
				double a = Train.Cars[CarIndex].Specs.MotorDeceleration;
				DecelerationDueToMotor = f * a;
			}
			else
			{
				DecelerationDueToMotor = 0.0;
			}
			// hold brake
			if (Train.Specs.CurrentHoldBrake.Actual & DecelerationDueToMotor == 0.0)
			{
				if (Game.SecondsSinceMidnight >= Train.Cars[CarIndex].Specs.HoldBrake.NextUpdateTime)
				{
					Train.Cars[CarIndex].Specs.HoldBrake.NextUpdateTime = Game.SecondsSinceMidnight + Train.Cars[CarIndex].Specs.HoldBrake.UpdateInterval;
					Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput += 0.8 * Train.Cars[CarIndex].Specs.CurrentAcceleration * (double)Math.Sign(Train.Cars[CarIndex].Specs.CurrentPerceivedSpeed);
					if (Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput < 0.0) Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput = 0.0;
					double a = Train.Cars[CarIndex].Specs.MotorDeceleration;
					if (Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput > a) Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput = a;
				}
				DecelerationDueToMotor = Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput;
			}
			else
			{
				Train.Cars[CarIndex].Specs.HoldBrake.CurrentAccelerationOutput = 0.0;
			}
			{ // rub sound
				buffer = Train.Cars[CarIndex].Sounds.Rub.Buffer;
				if (buffer != null)
				{
					double spd = Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed);
					double pitch = 1.0 / (spd + 1.0) + 1.0;
					double gain = Train.Cars[CarIndex].Derailed ? 0.0 : pressureratio;
					if (spd < 1.38888888888889)
					{
						double t = spd * spd;
						gain *= 1.5552 * t - 0.746496 * spd * t;
					}
					else if (spd > 12.5)
					{
						double t = spd - 12.5;
						const double fadefactor = 0.1;
						gain *= 1.0 / (fadefactor * t * t + 1.0);
					}
					if (Sounds.IsPlaying(Train.Cars[CarIndex].Sounds.Rub.Source))
					{
						if (pitch > 0.01 & gain > 0.001)
						{
							Train.Cars[CarIndex].Sounds.Rub.Source.Pitch = pitch;
							Train.Cars[CarIndex].Sounds.Rub.Source.Volume = gain;
						}
						else
						{
							Sounds.StopSound(Train.Cars[CarIndex].Sounds.Rub.Source);
						}
					}
					else if (pitch > 0.02 & gain > 0.01)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.Rub.Position;
						Train.Cars[CarIndex].Sounds.Rub.Source = Sounds.PlaySound(buffer, pitch, gain, pos, Train, CarIndex, true);
					}
				}
			}
		}
		private static double GetRate(double Ratio, double Factor)
		{
			Ratio = Ratio < 0.0 ? 0.0 : Ratio > 1.0 ? 1.0 : Ratio;
			Ratio = 1.0 - Ratio;
			return 1.5 * Factor * (1.01 - Ratio * Ratio);
		}

		/// <summary>Applies the emergency brake</summary>
		/// <param name="Train">The train</param>
		internal static void ApplyEmergencyBrake(Train Train)
		{
			// sound
			if (!Train.Specs.CurrentEmergencyBrake.Driver)
			{
				Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMax.Buffer;
				if (buffer != null)
				{
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMax.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					buffer = Train.Cars[Train.DriverCar].Sounds.EmrBrake.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[i].Sounds.EmrBrake.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
					}
				}
			}
			// apply
			ApplyNotch(Train, 0, !Train.Specs.SingleHandle, Train.Specs.MaximumBrakeNotch, true);
			ApplyAirBrakeHandle(Train, AirBrakeHandleState.Service);
			Train.Specs.CurrentEmergencyBrake.Driver = true;
			Train.Specs.CurrentHoldBrake.Driver = false;
			Train.Specs.CurrentConstSpeed = false;
			// plugin
			if (Train.Plugin == null) return;
			Train.Plugin.UpdatePower();
			Train.Plugin.UpdateBrake();
		}

		/// <summary>Releases the emergency brake</summary>
		/// <param name="Train">The train</param>
		internal static void UnapplyEmergencyBrake(Train Train)
		{
			if (Train.Specs.CurrentEmergencyBrake.Driver)
			{
				// sound
				Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Buffer;
				if (buffer != null)
				{
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
				// apply
				ApplyNotch(Train, 0, !Train.Specs.SingleHandle, Train.Specs.MaximumBrakeNotch, true);
				ApplyAirBrakeHandle(Train, AirBrakeHandleState.Service);
				Train.Specs.CurrentEmergencyBrake.Driver = false;
				// plugin
				if (Train.Plugin == null) return;
				Train.Plugin.UpdatePower();
				Train.Plugin.UpdateBrake();
			}
		}

		/// <summary>Applies or releases the hold brake</summary>
		/// <param name="Train">The train</param>
		/// <param name="Value">Whether to apply (TRUE) or release (FALSE)</param>
		internal static void ApplyHoldBrake(Train Train, bool Value)
		{
			Train.Specs.CurrentHoldBrake.Driver = Value;
			if (Train.Plugin == null) return;
			Train.Plugin.UpdatePower();
			Train.Plugin.UpdateBrake();
		}
	}
}
