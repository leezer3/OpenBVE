using SoundManager;

namespace TrainManager.TractionModels.Steam
{
	public class Firebox
	{
		/// <summary>Holds a reference to the base engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>The maximum fire temperature</summary>
		private readonly double MaxTemperature;
		/// <summary>The current fire temperature</summary>
		private double Temperature;
		/// <summary>The maximum fire area</summary>
		internal double MaxArea;
		/// <summary>The current fire area</summary>
		internal double Area;
		/// <summary>The fire mass</summary>
		internal double Mass;
		/// <summary>The max conversion rate of fire mass into temperature</summary>
		private readonly double MaxConversionRate;
		/// <summary>The conversion rate of fire mass into temperature</summary>
		internal double ConversionRate => MaxConversionRate * ((Area * MaxArea) * (Temperature * MaxTemperature));
		/// <summary>The number of units added per shovel of coal</summary>
		public double UnitsPerShovel;
		/// <summary>The shovel sound</summary>
		public CarSound ShovelSound;

		internal Firebox(SteamEngine engine, double maxArea, double maxTemperature, double conversionRate, double unitsPerShovel)
		{
			Engine = engine;
			MaxArea = maxArea;
			MaxTemperature = maxTemperature;
			MaxConversionRate = conversionRate;
			UnitsPerShovel = unitsPerShovel;
		}

		internal void Update(double timeElapsed)
		{
			double burntUnits = ConversionRate * timeElapsed;
			if (Engine.Boiler.Blowers.Active)
			{
				burntUnits *= 2.0;
			}
			Mass -= burntUnits;
			if (Mass < MaxArea)
			{
				Area -= burntUnits;
			}
			if (Temperature < MaxTemperature)
			{
				Temperature += burntUnits;
			}
		}

		internal void AddFuel()
		{
			Mass += UnitsPerShovel;
			Area += UnitsPerShovel * 0.1; // 1kg of coal to ~10cm square
			if (Area > MaxArea)
			{
				Area = MaxArea;
			}

			if (ShovelSound != null)
			{
				ShovelSound.Play(Engine.Car, false);
			}
		}
	}
}
