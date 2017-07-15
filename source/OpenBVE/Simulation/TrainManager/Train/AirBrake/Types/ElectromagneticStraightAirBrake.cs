using System;

namespace OpenBve.BrakeSystems
{
	public partial class AirBrake
	{
		internal class ElectromagneticStraightAirBrake : CarAirBrake
		{
			internal override void Update(TrainManager.Train Train, int CarIndex, double TimeElapsed)
			{
				if (Type == BrakeType.Main)
				{
					MainReservoir.UpdateBrakePipe(Train, CarIndex, TimeElapsed);
				}
				// refill auxillary reservoir from brake pipe
				if (BrakePipe.CurrentPressure > AuxillaryReservoir.CurrentPressure + Tolerance)
				{
					double r = 2.0 * AuxillaryReservoir.ChargeRate;
					double d = BrakePipe.CurrentPressure - AuxillaryReservoir.CurrentPressure;
					double m = AuxillaryReservoir.MaximumPressure;
					r = GetRate(d / m, r * TimeElapsed);
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
				{ // electric command
					bool emergency;
					if (BrakePipe.CurrentPressure + Tolerance < AuxillaryReservoir.CurrentPressure)
					{
						emergency = true;
					}
					else
					{
						emergency = Train.EmergencyBrake.Applied;
					}
					double p; if (emergency)
					{
						p = BrakeCylinder.EmergencyMaximumPressure;
					}
					else
					{
						p = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
						p *= BrakeCylinder.ServiceMaximumPressure;
					}
					if (Train.Cars[CarIndex].Specs.IsMotorCar & !Train.EmergencyBrake.Applied & Train.Specs.CurrentReverser.Actual != 0)
					{
						// brake control system
						if (Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed) > Train.Cars[CarIndex].Specs.AirBrake.ControlSpeed)
						{
							if (Train.Cars[CarIndex].Specs.ElectropneumaticType == TrainManager.EletropneumaticBrakeType.ClosingElectromagneticValve)
							{
								// closing electromagnetic valve (lock-out valve)
								p = 0.0;
							}
							else if (Train.Cars[CarIndex].Specs.ElectropneumaticType == TrainManager.EletropneumaticBrakeType.DelayFillingControl)
							{
								// delay-filling control
								//Variable f is never used, so don't calculate it
								//double f = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
								double a = Train.Cars[CarIndex].Specs.MotorDeceleration;
								double pr = p / BrakeCylinder.ServiceMaximumPressure;
								double b = pr * Train.Cars[CarIndex].Specs.AirBrake.DecelerationAtServiceMaximumPressure;
								double d = b - a;
								if (d > 0.0)
								{
									p = d / Train.Cars[CarIndex].Specs.AirBrake.DecelerationAtServiceMaximumPressure;
									if (p > 1.0) p = 1.0;
									p *= BrakeCylinder.ServiceMaximumPressure;
								}
								else
								{
									p = 0.0;
								}
							}
						}
					}
					if (BrakeCylinder.CurrentPressure > p + Tolerance | p == 0.0)
					{
						// brake cylinder release
						double r = BrakeCylinder.ReleaseRate;
						double d = BrakeCylinder.CurrentPressure - p;
						double m = BrakeCylinder.EmergencyMaximumPressure;
						r = GetRate(d / m, r * TimeElapsed);
						if (r > BrakeCylinder.CurrentPressure) r = BrakeCylinder.CurrentPressure;
						if (r > d) r = d;
						// air sound
						if (r > 0.0 & BrakeCylinder.CurrentPressure < BrakeCylinder.SoundPlayedForPressure)
						{
							BrakeCylinder.SoundPlayedForPressure = p;
							AirSound = (AirSound)(p < Tolerance ? 0 : BrakeCylinder.CurrentPressure > m - Tolerance ? 2 : 1);
						}
						// pressure change
						BrakeCylinder.CurrentPressure -= r;
					}
					else if (BrakeCylinder.CurrentPressure + Tolerance < p)
					{
						// refill brake cylinder from auxillary reservoir
						double f = AuxillaryReservoir.BrakeCylinderCoefficient;
						double r;
						if (emergency)
						{
							r = 2.0 * BrakeCylinder.EmergencyChargeRate * f;
						}
						else
						{
							r = 2.0 * BrakeCylinder.ServiceChargeRate * f;
						}
						double d = AuxillaryReservoir.CurrentPressure - BrakeCylinder.CurrentPressure;
						double m = BrakeCylinder.EmergencyMaximumPressure;
						r = GetRate(d / m, r * TimeElapsed);
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
						// air sound
						BrakeCylinder.SoundPlayedForPressure = BrakeCylinder.EmergencyMaximumPressure;
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
}
