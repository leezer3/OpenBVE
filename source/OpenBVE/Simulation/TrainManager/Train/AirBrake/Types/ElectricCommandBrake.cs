using System;

namespace OpenBve.BrakeSystems
{
	public partial class AirBrake
	{
		internal class ElectricCommandBrake : CarAirBrake
		{
			internal override void Update(TrainManager.Train Train, int CarIndex, double TimeElapsed)
			{
				double p; if (Train.EmergencyBrake.Applied)
				{
					p = BrakeCylinder.EmergencyMaximumPressure;
				}
				else
				{
					p = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
					p *= BrakeCylinder.ServiceMaximumPressure;
				}
				if (!Train.EmergencyBrake.Applied & Train.Specs.CurrentReverser.Actual != 0)
				{
					// brake control system
					if (Train.Cars[CarIndex].Specs.IsMotorCar & Math.Abs(Train.Cars[CarIndex].Specs.CurrentSpeed) > Train.Cars[CarIndex].AirBrake.ControlSpeed)
					{
						if (Train.Cars[CarIndex].ElectropneumaticType == TrainManager.EletropneumaticBrakeType.ClosingElectromagneticValve)
						{
							// closing electromagnetic valve (lock-out valve)
							p = 0.0;
						}
						else if (Train.Cars[CarIndex].ElectropneumaticType == TrainManager.EletropneumaticBrakeType.DelayFillingControl)
						{
							// delay-filling control
							// Variable f is never used, so don't calculate it
							//double f = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
							double a = Train.Cars[CarIndex].Specs.MotorDeceleration;
							double pr = p / BrakeCylinder.ServiceMaximumPressure;
							double b = pr * Train.Cars[CarIndex].AirBrake.DecelerationAtServiceMaximumPressure;
							double d = b - a;
							if (d > 0.0)
							{
								p = d / Train.Cars[CarIndex].AirBrake.DecelerationAtServiceMaximumPressure;
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
					// brake cylinder exhaust valve
					double r = BrakeCylinder.ReleaseRate;
					double d = BrakeCylinder.CurrentPressure;
					double m = BrakeCylinder.EmergencyMaximumPressure;
					r = GetRate(d / m, r * TimeElapsed);
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
				else if ((BrakeCylinder.CurrentPressure + Tolerance < p | p == BrakeCylinder.EmergencyMaximumPressure) & BrakeCylinder.CurrentPressure + Tolerance < MainReservoir.CurrentPressure)
				{
					// fill brake cylinder from main reservoir
					double r;
					if (Train.EmergencyBrake.Applied)
					{
						r = 2.0 * BrakeCylinder.EmergencyChargeRate;
					}
					else
					{
						r = 2.0 * BrakeCylinder.ServiceChargeRate;
					}
					double pm = p < MainReservoir.CurrentPressure ? p : MainReservoir.CurrentPressure;
					double d = pm - BrakeCylinder.CurrentPressure;
					double m = BrakeCylinder.EmergencyMaximumPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > d) r = d;
					double f1 = AuxillaryReservoir.BrakeCylinderCoefficient;
					double f2 = MainReservoir.BrakePipeCoefficient;
					double f3 = AuxillaryReservoir.BrakePipeCoefficient;
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
