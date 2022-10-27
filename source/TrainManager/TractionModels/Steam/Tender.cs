namespace TrainManager.TractionModels.Steam
{
	/// <summary>A tender carrying water, coal etc.</summary>
	public class Tender
	{
		/// <summary>The current fuel level </summary>
		public double FuelLevel;
		/// <summary>The maximum fuel level</summary>
		public readonly double MaxFuelLevel;
		/// <summary>The current water level</summary>
		public double WaterLevel;
		/// <summary>The max water level</summary>
		public readonly double MaxWaterLevel;

		public Tender(double fuelLevel, double maxFuelLevel, double waterLevel, double maxWaterLevel)
		{
			FuelLevel = fuelLevel;
			MaxFuelLevel = maxFuelLevel;
			WaterLevel = waterLevel;
			MaxWaterLevel = maxWaterLevel;
		}
	}
}
