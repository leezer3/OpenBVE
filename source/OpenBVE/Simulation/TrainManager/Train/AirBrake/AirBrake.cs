namespace OpenBve.BrakeSystems
{
	public partial class AirBrake
	{
		/// <summary>The root abstract class defining a car air-brake system</summary>
		internal abstract class CarAirBrake
		{
			internal BrakeType Type;
			internal AirCompressor Compressor;
			internal MainReservoir MainReservoir;
			internal EqualizingReservior EqualizingReservoir;
			internal BrakePipe BrakePipe;
			internal StraightAirPipe StraightAirPipe;
			internal AuxillaryReservoir AuxillaryReservoir;
			internal BrakeCylinder BrakeCylinder;
			internal const double Tolerance = 5000.0;

			internal void UpdateSystem(TrainManager.Train Train, int CarIndex, double TimeElapsed, ref TrainManager.AirSound Sound)
			{
				//If we are in a car with a compressor & equalizing reservoir, update them
				if (Type == BrakeType.Main)
				{
					Train.Cars[CarIndex].Specs.AirBrake.Compressor.Update(Train, CarIndex, TimeElapsed);
					Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoir.Update(Train, CarIndex, TimeElapsed);
				}
				//Update the abstract brake system method
				Update(Train, CarIndex, TimeElapsed, ref Sound);
			}

			internal abstract void Update(TrainManager.Train Train, int CarIndex, double TimeElapsed, ref TrainManager.AirSound Sound);
		}

		private static double GetRate(double Ratio, double Factor)
		{
			Ratio = Ratio < 0.0 ? 0.0 : Ratio > 1.0 ? 1.0 : Ratio;
			Ratio = 1.0 - Ratio;
			return 1.5 * Factor * (1.01 - Ratio * Ratio);
		}
	}
}
