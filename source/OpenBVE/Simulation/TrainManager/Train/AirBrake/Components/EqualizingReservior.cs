namespace OpenBve.BrakeSystems
{
	public partial class AirBrake
	{
		/// <summary>Represents the equalizing reservoir of an air-brake system</summary>
		internal class EqualizingReservior
		{
			/// <summary>The current pressure of this reservoir</summary>
			internal double CurrentPressure;
			/// <summary>The normal pressure of this reservoir</summary>
			internal double NormalPressure;
			/// <summary>The normal rate that this reservoir transfers pressure in pascals / second</summary>
			internal double ServiceRate;
			/// <summary>The emergency rate that this reservoir transfers pressure in pascals / second</summary>
			internal double EmergencyRate;
			/// <summary>The rate at which this reservoir charges pressure in pascals / second</summary>
			internal double ChargeRate;
			/// <summary>The parent air-brake</summary>
			internal CarAirBrake AirBrake;

			/// <summary>Creates a new equalizing reservoir</summary>
			/// <param name="airBrake">The parent air-brake</param>
			internal EqualizingReservior(CarAirBrake airBrake)
			{
				this.CurrentPressure = 0.0;
				this.NormalPressure = 0.0;
				this.ServiceRate = 0.0;
				this.EmergencyRate = 0.0;
				this.ChargeRate = 0.0;
				this.AirBrake = airBrake;
			}

			/// <summary>Updates the equalizing reservior pressure for the air brake system</summary>
			/// <param name="Train">The train</param>
			/// <param name="CarIndex">The car index</param>
			/// <param name="TimeElapsed">The time elaspsed since the last call to this function</param>
			internal void Update(TrainManager.Train Train, int CarIndex, double TimeElapsed)
			{
				//Check if we are in EB or normal braking
				if (Train.EmergencyBrake.Applied)
				{
					double r = EmergencyRate;
					double d = CurrentPressure;
					double m = NormalPressure;
					r = GetRate(d / m, r * TimeElapsed);
					if (r > CurrentPressure) r = CurrentPressure;
					CurrentPressure -= r;
				}
				else
				{
					if (AirBrake is AutomaticAirBrake)
					{
						// automatic air brake
						if (Train.Specs.AirBrake.Handle.Actual == TrainManager.AirBrakeHandleState.Service)
						{
							double r = ServiceRate; //50000
							double d = CurrentPressure;
							double m = NormalPressure; //1.05 * max service pressure from train.dat in pascals
							r = GetRate(d / m, r * TimeElapsed);
							if (r > CurrentPressure) r = CurrentPressure;
							CurrentPressure -= r;
						}
						else if (Train.Specs.AirBrake.Handle.Actual == TrainManager.AirBrakeHandleState.Release)
						{
							double r = ChargeRate;
							double d = NormalPressure - CurrentPressure;
							double m = NormalPressure;
							r = GetRate(d / m, r * TimeElapsed);
							if (r > d) r = d;
							d = AirBrake.MainReservoir.CurrentPressure - CurrentPressure;
							if (r > d) r = d;
							double f = AirBrake.MainReservoir.EqualizingReservoirCoefficient;
							double s = r * f * TimeElapsed;
							if (s > AirBrake.MainReservoir.CurrentPressure)
							{
								r *= AirBrake.MainReservoir.CurrentPressure / s;
								s = AirBrake.MainReservoir.CurrentPressure;
							}
							CurrentPressure += 0.5 * r;
							AirBrake.MainReservoir.CurrentPressure -= 0.5 * s;
						}
					}
					else if (AirBrake is ElectromagneticStraightAirBrake)
					{
						// electromagnetic straight air brake
						double r = ChargeRate;
						double d = NormalPressure - CurrentPressure;
						double m = NormalPressure;
						r = GetRate(d / m, r * TimeElapsed);
						if (r > d) r = d;
						d = AirBrake.MainReservoir.CurrentPressure - CurrentPressure;
						if (r > d) r = d;
						double f = AirBrake.MainReservoir.EqualizingReservoirCoefficient;
						double s = r * f * TimeElapsed;
						if (s > AirBrake.MainReservoir.CurrentPressure)
						{
							r *= AirBrake.MainReservoir.CurrentPressure / s;
							s = AirBrake.MainReservoir.CurrentPressure;
						}
						CurrentPressure += 0.5 * r;
						AirBrake.MainReservoir.CurrentPressure -= 0.5 * s;
					}
				}
			}
		}
	}
}
