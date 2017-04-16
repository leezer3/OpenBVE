namespace OpenBve.BrakeSystems
{
	public partial class AirBrake
	{
		internal class AutomaticAirBrake : CarAirBrake
		{
			/// <summary>Updates the automatic air brake system</summary>
			/// <param name="Train">The train</param>
			/// <param name="CarIndex">The car index</param>
			/// <param name="TimeElapsed">The time elapsed since the last call to this function</param>
			/// <param name="Sound">The air sound to be played</param>
			internal override void Update(TrainManager.Train Train, int CarIndex, double TimeElapsed)
			{
				if (Type == BrakeType.Main)
				{
					Train.Cars[CarIndex].Specs.AirBrake.MainReservoir.UpdateBrakePipe(Train, CarIndex, TimeElapsed);
				}
				if (BrakePipe.CurrentPressure + Tolerance < AuxillaryReservoir.CurrentPressure)
				{
					if (AuxillaryReservoir.CurrentPressure + Tolerance < BrakeCylinder.CurrentPressure)
					{
						// back-flow from brake cylinder to auxillary reservoir
						double u = (BrakeCylinder.CurrentPressure - AuxillaryReservoir.CurrentPressure - Tolerance) / Tolerance;
						if (u > 1.0) u = 1.0;
						double f = AuxillaryReservoir.BrakeCylinderCoefficient;
						double r = BrakeCylinder.ServiceChargeRate * f;
						double d = BrakeCylinder.CurrentPressure - AuxillaryReservoir.CurrentPressure;
						double m = AuxillaryReservoir.MaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
						if (AuxillaryReservoir.CurrentPressure + r > m)
						{
							r = m - AuxillaryReservoir.CurrentPressure;
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
						AuxillaryReservoir.CurrentPressure += 0.5 * r;
						BrakeCylinder.CurrentPressure -= 0.5 * s;
					}
					else if (AuxillaryReservoir.CurrentPressure > BrakeCylinder.CurrentPressure + Tolerance)
					{
						// refill brake cylinder from auxillary reservoir
						double u = (AuxillaryReservoir.CurrentPressure - BrakeCylinder.CurrentPressure - Tolerance) / Tolerance;
						if (u > 1.0) u = 1.0;
						double f = AuxillaryReservoir.BrakeCylinderCoefficient;
						double r = BrakeCylinder.ServiceChargeRate * f;
						double d = AuxillaryReservoir.CurrentPressure - BrakeCylinder.CurrentPressure;
						double m = AuxillaryReservoir.MaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
						if (r > AuxillaryReservoir.CurrentPressure)
						{
							r = AuxillaryReservoir.CurrentPressure;
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
						AuxillaryReservoir.CurrentPressure -= 0.5 * r;
						BrakeCylinder.CurrentPressure += 0.5 * s;
					}
					// air sound
					BrakeCylinder.SoundPlayedForPressure = BrakeCylinder.EmergencyMaximumPressure;
				}
				else if (BrakePipe.CurrentPressure > AuxillaryReservoir.CurrentPressure + Tolerance)
				{
					double u = (BrakePipe.CurrentPressure - AuxillaryReservoir.CurrentPressure - Tolerance) / Tolerance;
					if (u > 1.0) u = 1.0;
					{ // refill auxillary reservoir from brake pipe
						double r = AuxillaryReservoir.ChargeRate;
						double d = BrakePipe.CurrentPressure - AuxillaryReservoir.CurrentPressure;
						double m = AuxillaryReservoir.MaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
						if (r > BrakePipe.CurrentPressure)
						{
							r = BrakePipe.CurrentPressure;
						}
						if (r > d) r = d;
						d = AuxillaryReservoir.MaximumPressure - AuxillaryReservoir.CurrentPressure;
						if (r > d) r = d;
						double f = AuxillaryReservoir.BrakePipeCoefficient;
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
						AuxillaryReservoir.CurrentPressure += 0.5 * r;
						BrakePipe.CurrentPressure -= 0.5 * s;
					}
					{ // brake cylinder release
						double r = BrakeCylinder.ReleaseRate;
						double d = BrakeCylinder.CurrentPressure;
						double m = BrakeCylinder.EmergencyMaximumPressure;
						r = GetRate(d * u / m, r * TimeElapsed);
						if (r > BrakeCylinder.CurrentPressure) r = BrakeCylinder.CurrentPressure;
						BrakeCylinder.CurrentPressure -= r;
						// air sound
						if (r > 0.0 & BrakeCylinder.CurrentPressure < BrakeCylinder.SoundPlayedForPressure)
						{
							double p = 0.8 * BrakeCylinder.CurrentPressure - 0.2 * BrakeCylinder.EmergencyMaximumPressure;
							if (p < 0.0) p = 0.0;
							BrakeCylinder.SoundPlayedForPressure = p;
							AirSound = (AirSound)(p < Tolerance ? 0 : BrakeCylinder.CurrentPressure > m - Tolerance ? 2 : 1);
						}
					}
				}
				else
				{
					// air sound
					BrakeCylinder.SoundPlayedForPressure = BrakeCylinder.EmergencyMaximumPressure;
				}
			}
		}
	}
}
