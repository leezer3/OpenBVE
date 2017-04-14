namespace OpenBve.BrakeSystems
{
	/// <summary>Defines an air-brake</summary>
	public partial class AirBrake
	{
		/// <summary>The root abstract class defining a car air-brake system</summary>
		internal abstract class CarAirBrake
		{
			/// <summary>The type of air brake</summary>
			internal BrakeType Type;
			/// <summary>The compressor, or a null reference if not fitted</summary>
			internal AirCompressor Compressor;
			/// <summary>The main reservoir, or a null reference if not fitted</summary>
			internal MainReservoir MainReservoir;
			/// <summary>The equalizing reservoir, or a null reference if not fitted</summary>
			internal EqualizingReservior EqualizingReservoir;
			/// <summary>The brake pipe</summary>
			internal BrakePipe BrakePipe;
			/// <summary>The straight air pipe</summary>
			internal StraightAirPipe StraightAirPipe;
			/// <summary>The auxilary reservoir, or a null reference if not fitted</summary>
			internal AuxillaryReservoir AuxillaryReservoir;
			/// <summary>The brake cylinder, or a null reference if not fitted</summary>
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
