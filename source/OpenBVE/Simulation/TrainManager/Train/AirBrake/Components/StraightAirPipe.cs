namespace OpenBve.BrakeSystems
{
	public partial class AirBrake
	{
		/// <summary>Represents the straight air pipe of an air-brake system</summary>
		internal class StraightAirPipe
		{
			/// <summary>The current pressure in Pa</summary>
			internal double CurrentPressure;
			/// <summary>The normal release rate in Pa per second</summary>
			internal double ReleaseRate;
			/// <summary>The release rate when using service brakes in Pa per second</summary>
			internal double ServiceRate;
			/// <summary>The release rate when using emergency brakes in Pa per second</summary>
			internal double EmergencyRate;
			/// <summary>The parent air-brake</summary>
			internal CarAirBrake CarAirBrake;

			/// <summary>Creates a new straight air pipe</summary>
			/// <param name="carAirBrake">The parent air-brake</param>
			internal StraightAirPipe(CarAirBrake carAirBrake)
			{
				this.CurrentPressure = 0.0;
				this.ReleaseRate = 0.0;
				this.EmergencyRate = 0.0;
				this.CarAirBrake = carAirBrake;
			}
			
			internal void Update(TrainManager.Train Train, int CarIndex, double TimeElapsed)
			{
				if (CarAirBrake is ElectromagneticStraightAirBrake & CarAirBrake.Type == BrakeType.Main)
				{
					double p; if (Train.EmergencyBrake.Applied)
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
						if (Train.EmergencyBrake.Applied)
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
				else if (Train.Cars[CarIndex].Specs.BrakeType == TrainManager.CarBrakeType.ElectricCommandBrake)
				{
					double p; if (Train.EmergencyBrake.Applied)
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
