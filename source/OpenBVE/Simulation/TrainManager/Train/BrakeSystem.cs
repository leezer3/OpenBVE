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
			// air compressor
			if (Train.Cars[CarIndex].Specs.AirBrake.Type == AirBrakeType.Main)
			{
				if (Train.Cars[CarIndex].Specs.AirBrake.AirCompressorEnabled)
				{
					if (Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure > Train.Cars[CarIndex].Specs.AirBrake.AirCompressorMaximumPressure)
					{
						Train.Cars[CarIndex].Specs.AirBrake.AirCompressorEnabled = false;
						Train.Cars[CarIndex].Sounds.CpLoopStarted = false;
						Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.CpEnd.Buffer;
						if (buffer != null)
						{
							OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.CpEnd.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, false);
						}
						buffer = Train.Cars[CarIndex].Sounds.CpLoop.Buffer;
						if (buffer != null)
						{
							Sounds.StopSound(Train.Cars[CarIndex].Sounds.CpLoop.Source);
						}
					}
					else
					{
						Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure += Train.Cars[CarIndex].Specs.AirBrake.AirCompressorRate * TimeElapsed;
						if (!Train.Cars[CarIndex].Sounds.CpLoopStarted && Game.SecondsSinceMidnight > Train.Cars[CarIndex].Sounds.CpStartTimeStarted + 5.0)
						{
							Train.Cars[CarIndex].Sounds.CpLoopStarted = true;
							Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.CpLoop.Buffer;
							if (buffer != null)
							{
								OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.CpLoop.Position;
								Train.Cars[CarIndex].Sounds.CpLoop.Source = Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, true);
							}
						}
					}
				}
				else
				{
					if (Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure < Train.Cars[CarIndex].Specs.AirBrake.AirCompressorMinimumPressure)
					{
						Train.Cars[CarIndex].Specs.AirBrake.AirCompressorEnabled = true;
						Train.Cars[CarIndex].Sounds.CpStartTimeStarted = Game.SecondsSinceMidnight;
						Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.CpStart.Buffer;
						if (buffer != null)
						{
							OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.CpStart.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, false);
						}
					}
				}
			}
			// initialize
			const double Tolerance = 5000.0;
			int airsound = -1;
			// equalizing reservoir
			if (Train.Cars[CarIndex].Specs.AirBrake.Type == AirBrakeType.Main)
			{
				if (Train.Handles.EmergencyBrake.Actual)
				{
					double r = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirEmergencyRate;
					double d = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
					double m = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure) r = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
					Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure -= r;
				}
				else
				{
					if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.AutomaticAirBrake)
					{
						// automatic air brake
						if (Train.Handles.AirBrake.Handle.Actual == AirBrakeHandleState.Service)
						{
							double r = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirServiceRate; //50000
							double d = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure; 
							double m = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure; //1.05 * max service pressure from train.dat in pascals
							r = GetRate(d / m, r * TimeElapsed);
							if (r > Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure) r = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
							Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure -= r;
						}
						else if (Train.Handles.AirBrake.Handle.Actual == AirBrakeHandleState.Release)
						{
							double r = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirChargeRate;
							double d = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure - Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
							double m = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure;
							r = GetRate(d / m, r * TimeElapsed);
							if (r > d) r = d;
							d = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
							if (r > d) r = d;
							double f = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirEqualizingReservoirCoefficient;
							double s = r * f * TimeElapsed;
							if (s > Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure)
							{
								r *= Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure / s;
								s = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure;
							}
							Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure += 0.5 * r;
							Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure -= 0.5 * s;
						}
					}
					else if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.ElectromagneticStraightAirBrake)
					{
						// electromagnetic straight air brake
						double r = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirChargeRate;
						double d = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure - Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
						double m = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure;
						r = GetRate(d / m, r * TimeElapsed);
						if (r > d) r = d;
						d = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
						if (r > d) r = d;
						double f = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirEqualizingReservoirCoefficient;
						double s = r * f * TimeElapsed;
						if (s > Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure)
						{
							r *= Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure / s;
							s = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure;
						}
						Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure += 0.5 * r;
						Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure -= 0.5 * s;
					}
				}
			}
			// brake pipe (main reservoir)
			if ((Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.AutomaticAirBrake | Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.ElectromagneticStraightAirBrake) & Train.Cars[CarIndex].Specs.AirBrake.Type == AirBrakeType.Main)
			{
				if (Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure > Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure + Tolerance)
				{
					// brake pipe exhaust valve
					double r = Train.Handles.EmergencyBrake.Actual ? Train.Cars[CarIndex].Specs.AirBrake.BrakePipeEmergencyRate : Train.Cars[CarIndex].Specs.AirBrake.BrakePipeServiceRate;
					double d = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure;
					double m = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure;
					r = (0.5 + 1.5 * d / m) * r * TimeElapsed;
					if (r > d) r = d;
					Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure -= r;
				}
				else if (Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure + Tolerance < Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure)
				{
					// fill brake pipe from main reservoir
					double r = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeChargeRate;
					double d = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure;
					double m = Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoirNormalPressure;
					r = (0.5 + 1.5 * d / m) * r * TimeElapsed;
					if (r > d) r = d;
					d = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeNormalPressure - Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure;
					if (r > d) r = d;
					double f = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirBrakePipeCoefficient;
					double s = r * f;
					if (s > Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure)
					{
						r *= Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure / s;
						s = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure;
					}
					Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure += 0.5 * r;
					Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure -= 0.5 * s;
				}
			}
			// triple valve (auxillary reservoir, brake pipe, brake cylinder)
			if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.AutomaticAirBrake)
			{
				if (Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure + Tolerance < Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure)
				{
					if (Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure + Tolerance < Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure)
					{
						// back-flow from brake cylinder to auxillary reservoir
						double u = (Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure - Tolerance) / Tolerance;
						if (u > 1.0) u = 1.0;
						double f = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirBrakeCylinderCoefficient;
						double r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceChargeRate * f;
						double d = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
						double m = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
						if (Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure + r > m)
						{
							r = m - Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
						}
						if (r > d) r = d;
						double s = r / f;
						if (s > d)
						{
							r *= d / s;
							s = d;
						}
						if (s > Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure)
						{
							r *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure / s;
							s = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						}
						Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure += 0.5 * r;
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure -= 0.5 * s;
					}
					else if (Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure > Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure + Tolerance)
					{
						// refill brake cylinder from auxillary reservoir
						double u = (Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure - Tolerance) / Tolerance;
						if (u > 1.0) u = 1.0;
						double f = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirBrakeCylinderCoefficient;
						double r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceChargeRate * f;
						double d = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						double m = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
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
					}
					// air sound
					Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
				}
				else if (Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure > Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure + Tolerance)
				{
					double u = (Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure - Tolerance) / Tolerance;
					if (u > 1.0) u = 1.0;
					{ // refill auxillary reservoir from brake pipe
						double r = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirChargeRate;
						double d = Train.Cars[CarIndex].Specs.AirBrake.BrakePipeCurrentPressure - Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
						double m = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
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
					{ // brake cylinder release
						double r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderReleaseRate;
						double d = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
						if (r > Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure) r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure -= r;
						// air sound
						if (r > 0.0 & Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure < Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure)
						{
							double p = 0.8 * Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure - 0.2 * Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
							if (p < 0.0) p = 0.0;
							Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = p;
							airsound = p < Tolerance ? 0 : Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure > m - Tolerance ? 2 : 1;
						}
					}
				}
				else
				{
					// air sound
					Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
				}
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
						emergency = Train.Handles.EmergencyBrake.Actual;
					}
					double targetPressure;
					if (emergency)
					{
						//If EB is selected, then target pressure must be that required for EB
						targetPressure = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					}
					else
					{
						//Otherwise [BVE2 / BVE4 train.dat format] work out target pressure as a proportion of the max notch:
						targetPressure = (double)Train.Handles.Brake.Actual / (double)Train.Handles.Brake.MaximumNotch;
						switch (Train.Handles.LocoBrake.BrakeType)
						{
							case LocoBrakeType.Combined:
								if (Train.Handles.LocoBrake.Actual != 0 && CarIndex == Train.DriverCar)
								{
									double locoTargetPressure = (double)Train.Handles.LocoBrake.Actual / (double)Train.Handles.LocoBrake.MaximumNotch;
									targetPressure = Math.Max(locoTargetPressure, targetPressure);
								}
								break;
							case LocoBrakeType.Independant:
								if (CarIndex == Train.DriverCar)
								{
									//The locomotive brake is independant, so if the driver car return it's 
									targetPressure = (double)Train.Handles.LocoBrake.Actual / (double)Train.Handles.LocoBrake.MaximumNotch;
								}
								break;
							case LocoBrakeType.Blocking:
								if (Train.Handles.LocoBrake.Actual != 0)
								{
									//The locomotive brake application has blocked the release of the train brakes
									targetPressure = 1.0;
								}
								break;
						}
						targetPressure *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
					}
					if (Train.Cars[CarIndex].Specs.IsMotorCar & !Train.Handles.EmergencyBrake.Actual & Train.Handles.Reverser.Actual != 0)
					{
						//If we meet the conditions for brake control system to activate
						if (Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed) > Train.Cars[CarIndex].Specs.BrakeControlSpeed)
						{
							if (Train.Cars[CarIndex].Specs.ElectropneumaticType == EletropneumaticBrakeType.ClosingElectromagneticValve)
							{
								//When above the brake control speed, pressure to the BC is nil & electric brakes are used
								//Thus target pressure must be zero
								targetPressure = 0.0;
							}
							else if (Train.Cars[CarIndex].Specs.ElectropneumaticType == EletropneumaticBrakeType.DelayFillingControl)
							{
								//Motor is used to brake the train, until not enough deceleration, at which point the air brake is also used
								double a = Train.Cars[CarIndex].Specs.MotorDeceleration;
								double pr = targetPressure / Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
								double b;
								if (Train.Handles.LocoBrake.Actual != 0 && CarIndex == Train.DriverCar)
								{
									b = pr * Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure(Train.Handles.LocoBrake.Actual);
								}
								else
								{
									b = pr * Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure(Train.Handles.Brake.Actual);
								}

								double d = b - a;
								if (d > 0.0)
								{
									//Deceleration provided by the motor is not enough, so increase the BC target pressure
									targetPressure = d / Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure(Train.Handles.Brake.Actual);;
									switch (Train.Handles.LocoBrake.BrakeType)
									{
										case LocoBrakeType.Combined:
											if (Train.Handles.LocoBrake.Actual != 0 && CarIndex == Train.DriverCar)
											{
												double locoTargetPressure = d / Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure(Train.Handles.LocoBrake.Actual);;
												targetPressure = Math.Max(locoTargetPressure, targetPressure);
											}

											break;
										case LocoBrakeType.Independant:
											if (CarIndex == Train.DriverCar)
											{
												//The locomotive brake is independant, so if the driver car return it's 
												targetPressure = d / Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure(Train.Handles.LocoBrake.Actual);;
											}

											break;
										case LocoBrakeType.Blocking:
											if (Train.Handles.LocoBrake.Actual != 0)
											{
												//The locomotive brake application has blocked the release of the train brakes
												targetPressure = d / Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure(Train.Handles.LocoBrake.Actual);
											}
											break;
									}
									if (targetPressure > 1.0) targetPressure = 1.0;
									targetPressure *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
								}
								else
								{
									//Motor deceleration is enough, BC target pressure to zero
									targetPressure = 0.0;
								}
							}
						}
					}
					if (Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure > targetPressure + Tolerance | targetPressure == 0.0)
					{
						//BC pressure is greater than the target pressure, so release pressure
						double r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderReleaseRate;
						double d = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure - targetPressure;
						double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
						r = GetRate(d / m, r * TimeElapsed);
						if (r > Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure) r = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
						if (r > d) r = d;
						// air sound
						if (r > 0.0 & Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure < Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure)
						{
							Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderSoundPlayedForPressure = targetPressure;
							airsound = targetPressure < Tolerance ? 0 : Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure > m - Tolerance ? 2 : 1;
						}
						// pressure change
						Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure -= r;
					}
					else if (Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure + Tolerance < targetPressure)
					{
						//BC pressure is less than target pressure, so increase pressure
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
				double p; if (Train.Handles.EmergencyBrake.Actual)
				{
					p = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
				}
				else
				{
					p = (double)Train.Handles.Brake.Actual / (double)Train.Handles.Brake.MaximumNotch;
					switch (Train.Handles.LocoBrake.BrakeType)
					{
						case LocoBrakeType.Combined:
							if (Train.Handles.LocoBrake.Actual != 0 && CarIndex == Train.DriverCar)
							{
								double locoTargetPressure = (double)Train.Handles.LocoBrake.Actual / (double)Train.Handles.LocoBrake.MaximumNotch;
								p = Math.Max(locoTargetPressure, p);
							}

							break;
						case LocoBrakeType.Independant:
							if (CarIndex == Train.DriverCar)
							{
								//The locomotive brake is independant, so if the driver car return it's 
								p = (double)Train.Handles.LocoBrake.Actual / (double)Train.Handles.LocoBrake.MaximumNotch;
							}

							break;
						case LocoBrakeType.Blocking:
							if (Train.Handles.LocoBrake.Actual != 0)
							{
								//The locomotive brake application has blocked the release of the train brakes
								p = (double)Train.Handles.LocoBrake.Actual / (double)Train.Handles.LocoBrake.MaximumNotch;
							}
							break;
					}
					p *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
				}
				if (!Train.Handles.EmergencyBrake.Actual & Train.Handles.Reverser.Actual != 0)
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
							double a = Train.Cars[CarIndex].Specs.MotorDeceleration;
							double pr = p / Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
							double b;
							if (Train.Handles.LocoBrake.Actual != 0 && CarIndex == Train.DriverCar)
							{
								b = pr * Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure(Train.Handles.LocoBrake.Actual);
							}
							else
							{
								b = pr * Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure(Train.Handles.Brake.Actual);
							}
							double d = b - a;
							if (d > 0.0)
							{
								if (Train.Handles.LocoBrake.Actual != 0 && CarIndex == Train.DriverCar)
								{
									p = d / Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure(Train.Handles.LocoBrake.Actual);
								}
								else
								{
									p = d / Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure(Train.Handles.Brake.Actual);
								}

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
						airsound = p < Tolerance ? 0 : Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure > m - Tolerance ? 2 : 1;
					}
					// pressure change
					Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure -= r;
				}
				else if ((Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure + Tolerance < p | p == Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure) & Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure + Tolerance < Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure)
				{
					// fill brake cylinder from main reservoir
					double r;
					if (Train.Handles.EmergencyBrake.Actual)
					{
						r = 2.0 * Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyChargeRate;
					}
					else
					{
						r = 2.0 * Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceChargeRate;
					}
					double pm = p < Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure ? p : Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure;
					double d = pm - Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure;
					double m = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > d) r = d;
					double f1 = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirBrakeCylinderCoefficient;
					double f2 = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirBrakePipeCoefficient;
					double f3 = Train.Cars[CarIndex].Specs.AirBrake.AuxillaryReservoirBrakePipeCoefficient;
					double f = f1 * f2 / f3; // MainReservoirBrakeCylinderCoefficient
					double s = r * f;
					if (s > Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure)
					{
						r *= Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure / s;
						s = Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure;
					}
					Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure += 0.5 * r;
					Train.Cars[CarIndex].Specs.AirBrake.MainReservoirCurrentPressure -= 0.5 * s;
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
				double p; if (Train.Handles.EmergencyBrake.Actual)
				{
					p = 0.0;
				}
				else
				{
					p = (double)Train.Handles.Brake.Actual / (double)Train.Handles.Brake.MaximumNotch;
					switch (Train.Handles.LocoBrake.BrakeType)
					{
						case LocoBrakeType.Combined:
							if (Train.Handles.LocoBrake.Actual != 0 && CarIndex == Train.DriverCar)
							{
								double locoTargetPressure = (double)Train.Handles.LocoBrake.Actual / (double)Train.Handles.LocoBrake.MaximumNotch;
								p = Math.Max(locoTargetPressure, p);
							}

							break;
						case LocoBrakeType.Independant:
							if (CarIndex == Train.DriverCar)
							{
								//The locomotive brake is independant, so if the driver car return it's 
								p = (double)Train.Handles.LocoBrake.Actual / (double)Train.Handles.LocoBrake.MaximumNotch;
							}

							break;
						case LocoBrakeType.Blocking:
							if (Train.Handles.LocoBrake.Actual != 0)
							{
								//The locomotive brake application has blocked the release of the train brakes
								p = (double)Train.Handles.LocoBrake.Actual / (double)Train.Handles.LocoBrake.MaximumNotch;
							}
							break;
					}
					p *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
				}
				if (p + Tolerance < Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure)
				{
					double r;
					if (Train.Handles.EmergencyBrake.Actual)
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
				double p;
				if (Train.Handles.EmergencyBrake.Actual)
				{
					p = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
				}
				else
				{
					p = (double)Train.Handles.Brake.Actual / (double)Train.Handles.Brake.MaximumNotch;
					switch (Train.Handles.LocoBrake.BrakeType)
					{
						case LocoBrakeType.Combined:
							if (Train.Handles.LocoBrake.Actual != 0 && CarIndex == Train.DriverCar)
							{
								double locoTargetPressure = (double)Train.Handles.LocoBrake.Actual / (double)Train.Handles.LocoBrake.MaximumNotch;
								p = Math.Max(locoTargetPressure, p);
							}

							break;
						case LocoBrakeType.Independant:
							if (CarIndex == Train.DriverCar)
							{
								//The locomotive brake is independant, so if the driver car return it's 
								p = (double)Train.Handles.LocoBrake.Actual / (double)Train.Handles.LocoBrake.MaximumNotch;
							}

							break;
						case LocoBrakeType.Blocking:
							if (Train.Handles.LocoBrake.Actual != 0)
							{
								//The locomotive brake application has blocked the release of the train brakes
								p = (double)Train.Handles.LocoBrake.Actual / (double)Train.Handles.LocoBrake.MaximumNotch;
							}
							break;
					}
					p *= Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
				}
				Train.Cars[CarIndex].Specs.AirBrake.StraightAirPipeCurrentPressure = p;
			}
			// air sound
			if (airsound == 0)
			{
				// air zero
				Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.AirZero.Buffer;
				if (buffer != null)
				{
					OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.AirZero.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, false);
				}
			}
			else if (airsound == 1)
			{
				// air
				Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.Air.Buffer;
				if (buffer != null)
				{
					OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.Air.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, false);
				}
			}
			else if (airsound == 2)
			{
				// air high
				Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.AirHigh.Buffer;
				if (buffer != null)
				{
					OpenBveApi.Math.Vector3 pos = Train.Cars[CarIndex].Sounds.AirHigh.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, CarIndex, false);
				}
			}
			// deceleration provided by brake
			double pressureratio = Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderCurrentPressure / Train.Cars[CarIndex].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
			if (Train.Handles.LocoBrake.Actual != 0 && CarIndex == Train.DriverCar)
			{
				DecelerationDueToBrake = pressureratio * Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure(Train.Handles.LocoBrake.Actual);
			}
			else
			{
				DecelerationDueToBrake = pressureratio * Train.Cars[CarIndex].Specs.BrakeDecelerationAtServiceMaximumPressure(Train.Handles.Brake.Actual);
			}
			
			// deceleration provided by motor
			if (Train.Cars[CarIndex].Specs.BrakeType != CarBrakeType.AutomaticAirBrake && Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed) >= Train.Cars[CarIndex].Specs.BrakeControlSpeed & Train.Handles.Reverser.Actual != 0 & !Train.Handles.EmergencyBrake.Actual)
			{
				double f;
				if (Train.Handles.LocoBrake.Actual != 0 && CarIndex == Train.DriverCar)
				{
					f = (double)Train.Handles.LocoBrake.Actual / (double)Train.Handles.Brake.MaximumNotch;
				}
				else
				{
					f = (double)Train.Handles.Brake.Actual / (double)Train.Handles.Brake.MaximumNotch;
				}
				
				double a = Train.Cars[CarIndex].Specs.MotorDeceleration;
				DecelerationDueToMotor = f * a;
			}
			else
			{
				DecelerationDueToMotor = 0.0;
			}
			// hold brake
			Train.Cars[CarIndex].Specs.HoldBrake.Update(ref DecelerationDueToMotor, Train.Handles.HoldBrake.Actual);
			{ // rub sound
				Sounds.SoundBuffer buffer = Train.Cars[CarIndex].Sounds.Rub.Buffer;
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
			if (!Train.Handles.EmergencyBrake.Driver)
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
			Train.ApplyNotch(0, !Train.Handles.SingleHandle, Train.Handles.Brake.MaximumNotch, true);
			ApplyAirBrakeHandle(Train, AirBrakeHandleState.Service);
			Train.Handles.EmergencyBrake.Driver = true;
			Train.Handles.HoldBrake.Driver = false;
			Train.Specs.CurrentConstSpeed = false;
			if (Train.Handles.EmergencyBrake.Driver)
			{
				switch (Train.Handles.EmergencyBrake.OtherHandlesBehaviour)
				{
					case EbHandleBehaviour.PowerNeutral:
						if (!Train.Handles.SingleHandle)
						{
							Train.ApplyNotch(0, false, 0, true);
						}
						break;
					case EbHandleBehaviour.ReverserNeutral:
						TrainManager.ApplyReverser(Train, 0, false);
						break;
					case EbHandleBehaviour.PowerReverserNeutral:
						if (!Train.Handles.SingleHandle)
						{
							Train.ApplyNotch(0, false, 0, true);
						}
						TrainManager.ApplyReverser(Train, 0, false);
						break;
				}
			}

			// plugin
			if (Train.Plugin == null) return;
			Train.Plugin.UpdatePower();
			Train.Plugin.UpdateBrake();
		}

		/// <summary>Releases the emergency brake</summary>
		/// <param name="Train">The train</param>
		internal static void UnapplyEmergencyBrake(Train Train)
		{
			if (Train.Handles.EmergencyBrake.Driver)
			{
				// sound
				Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Buffer;
				if (buffer != null)
				{
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
				// apply
				Train.ApplyNotch(0, !Train.Handles.SingleHandle, Train.Handles.Brake.MaximumNotch, true);
				ApplyAirBrakeHandle(Train, AirBrakeHandleState.Service);
				Train.Handles.EmergencyBrake.Driver = false;
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
			Train.Handles.HoldBrake.Driver = Value;
			if (Train.Plugin == null) return;
			Train.Plugin.UpdatePower();
			Train.Plugin.UpdateBrake();
		}
	}
}
