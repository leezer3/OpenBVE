namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>The root abstract class defining a car air-brake system</summary>
		internal abstract class CarAirBrake
		{
			internal AirBrakeType Type;
			internal AirCompressor Compressor;
			internal MainReservoir MainReservoir;
			internal EqualizingReservior EqualizingReservoir;
			internal BrakePipe BrakePipe;
			internal StraightAirPipe StraightAirPipe;
			internal AuxillaryReservoir AuxillaryReservoir;
			internal BrakeCylinder BrakeCylinder;
			internal const double Tolerance = 5000.0;

			internal void UpdateSystem(Train Train, int CarIndex, double TimeElapsed, ref AirSound Sound)
			{
				//If we are in a car with a compressor & equalizing reservoir, update them
				if (Type == AirBrakeType.Main)
				{
					Train.Cars[CarIndex].Specs.AirBrake.Compressor.Update(Train, CarIndex, TimeElapsed);
					Train.Cars[CarIndex].Specs.AirBrake.EqualizingReservoir.Update(Train, CarIndex, TimeElapsed);
				}
				//Update the abstract brake system method
				Update(Train, CarIndex, TimeElapsed, ref Sound);
			}

			internal abstract void Update(Train Train, int CarIndex, double TimeElapsed, ref AirSound Sound);
		}
	}
}
