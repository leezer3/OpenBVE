using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBve
{
	public static partial class TrainManager
	{
		internal struct CarAirBrake
		{
			internal AirBrakeType Type;
			internal AirCompressor Compressor;
			internal MainReservoir MainReservoir;
			internal EqualizingReservior EqualizingReservoir;

			internal double BrakePipeCurrentPressure;
			internal double BrakePipeNormalPressure;
			internal double BrakePipeFlowSpeed;
			internal double BrakePipeChargeRate;
			internal double BrakePipeServiceRate;
			internal double BrakePipeEmergencyRate;
			internal double AuxillaryReservoirCurrentPressure;
			internal double AuxillaryReservoirMaximumPressure;
			internal double AuxillaryReservoirChargeRate;
			internal double AuxillaryReservoirBrakePipeCoefficient;
			internal double AuxillaryReservoirBrakeCylinderCoefficient;
			internal double BrakeCylinderCurrentPressure;
			internal double BrakeCylinderEmergencyMaximumPressure;
			internal double BrakeCylinderServiceMaximumPressure;
			internal double BrakeCylinderEmergencyChargeRate;
			internal double BrakeCylinderServiceChargeRate;
			internal double BrakeCylinderReleaseRate;
			internal double BrakeCylinderSoundPlayedForPressure;
			internal double StraightAirPipeCurrentPressure;
			internal double StraightAirPipeReleaseRate;
			internal double StraightAirPipeServiceRate;
			internal double StraightAirPipeEmergencyRate;

			internal const double Tolerance = 5000.0;







			/// <summary>Updates the automatic air brake system</summary>
			/// <param name="Train">The train</param>
			/// <param name="CarIndex">The car index</param>
			/// <param name="TimeElapsed">The time elapsed since the last call to this function</param>
			/// <param name="Sound">The air sound to be played</param>
			internal void UpdateAutomaticAirBrake(Train Train, int CarIndex, double TimeElapsed, ref AirSound Sound)
			{
				if (BrakePipeCurrentPressure + Tolerance < AuxillaryReservoirCurrentPressure)
				{
					if (AuxillaryReservoirCurrentPressure + Tolerance < BrakeCylinderCurrentPressure)
					{
						// back-flow from brake cylinder to auxillary reservoir
						double u = (BrakeCylinderCurrentPressure - AuxillaryReservoirCurrentPressure - Tolerance) / Tolerance;
						if (u > 1.0) u = 1.0;
						double f = AuxillaryReservoirBrakeCylinderCoefficient;
						double r = BrakeCylinderServiceChargeRate * f;
						double d = BrakeCylinderCurrentPressure - AuxillaryReservoirCurrentPressure;
						double m = AuxillaryReservoirMaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
						if (AuxillaryReservoirCurrentPressure + r > m)
						{
							r = m - AuxillaryReservoirCurrentPressure;
						}
						if (r > d) r = d;
						double s = r / f;
						if (s > d)
						{
							r *= d / s;
							s = d;
						}
						if (s > BrakeCylinderCurrentPressure)
						{
							r *= BrakeCylinderCurrentPressure / s;
							s = BrakeCylinderCurrentPressure;
						}
						AuxillaryReservoirCurrentPressure += 0.5 * r;
						BrakeCylinderCurrentPressure -= 0.5 * s;
					}
					else if (AuxillaryReservoirCurrentPressure > BrakeCylinderCurrentPressure + Tolerance)
					{
						// refill brake cylinder from auxillary reservoir
						double u = (AuxillaryReservoirCurrentPressure - BrakeCylinderCurrentPressure - Tolerance) / Tolerance;
						if (u > 1.0) u = 1.0;
						double f = AuxillaryReservoirBrakeCylinderCoefficient;
						double r = BrakeCylinderServiceChargeRate * f;
						double d = AuxillaryReservoirCurrentPressure - BrakeCylinderCurrentPressure;
						double m = AuxillaryReservoirMaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
						if (r > AuxillaryReservoirCurrentPressure)
						{
							r = AuxillaryReservoirCurrentPressure;
						}
						if (r > d) r = d;
						double s = r / f;
						if (s > d)
						{
							r *= d / s;
							s = d;
						}
						d = BrakeCylinderEmergencyMaximumPressure - BrakeCylinderCurrentPressure;
						if (s > d)
						{
							r *= d / s;
							s = d;
						}
						AuxillaryReservoirCurrentPressure -= 0.5 * r;
						BrakeCylinderCurrentPressure += 0.5 * s;
					}
					// air sound
					BrakeCylinderSoundPlayedForPressure = BrakeCylinderEmergencyMaximumPressure;
				}
				else if (BrakePipeCurrentPressure > AuxillaryReservoirCurrentPressure + Tolerance)
				{
					double u = (BrakePipeCurrentPressure - AuxillaryReservoirCurrentPressure - Tolerance) / Tolerance;
					if (u > 1.0) u = 1.0;
					{ // refill auxillary reservoir from brake pipe
						double r = AuxillaryReservoirChargeRate;
						double d = BrakePipeCurrentPressure - AuxillaryReservoirCurrentPressure;
						double m = AuxillaryReservoirMaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
						if (r > BrakePipeCurrentPressure)
						{
							r = BrakePipeCurrentPressure;
						}
						if (r > d) r = d;
						d = AuxillaryReservoirMaximumPressure - AuxillaryReservoirCurrentPressure;
						if (r > d) r = d;
						double f = AuxillaryReservoirBrakePipeCoefficient;
						double s = r / f;
						if (s > BrakePipeCurrentPressure)
						{
							r *= BrakePipeCurrentPressure / s;
							s = BrakePipeCurrentPressure;
						}
						if (s > d)
						{
							r *= d / s;
							s = d;
						}
						AuxillaryReservoirCurrentPressure += 0.5 * r;
						BrakePipeCurrentPressure -= 0.5 * s;
					}
					{ // brake cylinder release
						double r = BrakeCylinderReleaseRate;
						double d = BrakeCylinderCurrentPressure;
						double m = BrakeCylinderEmergencyMaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
						if (r > BrakeCylinderCurrentPressure) r = BrakeCylinderCurrentPressure;
						BrakeCylinderCurrentPressure -= r;
						// air sound
						if (r > 0.0 & BrakeCylinderCurrentPressure < BrakeCylinderSoundPlayedForPressure)
						{
							double p = 0.8 * BrakeCylinderCurrentPressure - 0.2 * BrakeCylinderEmergencyMaximumPressure;
							if (p < 0.0) p = 0.0;
							BrakeCylinderSoundPlayedForPressure = p;
							Sound = (AirSound)(p < Tolerance ? 0 : BrakeCylinderCurrentPressure > m - Tolerance ? 2 : 1);
						}
					}
				}
				else
				{
					// air sound
					BrakeCylinderSoundPlayedForPressure = BrakeCylinderEmergencyMaximumPressure;
				}
			}
		}
	}
}
