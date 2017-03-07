namespace OpenBve
{
	public static partial class TrainManager
	{
		internal class StraightAirPipe
		{
			internal double CurrentPressure;
			internal double ReleaseRate;
			internal double ServiceRate;
			internal double EmergencyRate;
			internal CarAirBrake CarAirBrake;

			internal StraightAirPipe(CarAirBrake carAirBrake)
			{
				this.CurrentPressure = 0.0;
				this.ReleaseRate = 0.0;
				this.EmergencyRate = 0.0;
				this.CarAirBrake = carAirBrake;
			}
			
			internal void Update(Train Train, int CarIndex, double TimeElapsed)
			{
				if (CarAirBrake is ElectromagneticStraightAirBrake & CarAirBrake.Type == AirBrakeType.Main)
				{
					double p; if (Train.Specs.CurrentEmergencyBrake.Actual)
					{
						p = 0.0;
					}
					else
					{
						p = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
						p *= CarAirBrake.BrakeCylinder.ServiceMaximumPressure;
					}
					if (p + CarAirBrake.Tolerance < CurrentPressure)
					{
						double r;
						if (Train.Specs.CurrentEmergencyBrake.Actual)
						{
							r = EmergencyRate;
						}
						else
						{
							r = ReleaseRate;
						}
						double d = CurrentPressure - p;
						double m = CarAirBrake.BrakeCylinder.EmergencyMaximumPressure;
						r = GetRate(d / m, r * TimeElapsed);
						if (r > d) r = d;
						CurrentPressure -= r;
					}
					else if (p > CurrentPressure + CarAirBrake.Tolerance)
					{
						double r = ServiceRate;
						double d = p - CurrentPressure;
						double m = CarAirBrake.BrakeCylinder.EmergencyMaximumPressure;
						r = GetRate(d / m, r * TimeElapsed);
						if (r > d) r = d;
						CurrentPressure += r;
					}
				}
				else if (Train.Cars[CarIndex].Specs.BrakeType == CarBrakeType.ElectricCommandBrake)
				{
					double p; if (Train.Specs.CurrentEmergencyBrake.Actual)
					{
						p = CarAirBrake.BrakeCylinder.EmergencyMaximumPressure;
					}
					else
					{
						p = (double)Train.Specs.CurrentBrakeNotch.Actual / (double)Train.Specs.MaximumBrakeNotch;
						p *= CarAirBrake.BrakeCylinder.ServiceMaximumPressure;
					}
					CurrentPressure = p;
				}
			}
		}
	}
}
