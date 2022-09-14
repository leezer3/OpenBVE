using System;
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
		public double Temperature;
		/// <summary>The maximum fire area</summary>
		internal double MaxArea;
		/// <summary>The current fire area</summary>
		public double FireArea;
		/// <summary>The fire mass</summary>
		public double FireMass;
		/// <summary>The max conversion rate of fire mass into temperature</summary>
		private readonly double MaxConversionRate;
		/// <summary>The conversion rate of fire mass into temperature</summary>
		public double ConversionRate => MaxConversionRate * ((FireArea * MaxArea) * (Temperature * MaxTemperature));
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
			//fixme
			FireArea = MaxArea / 2;
			FireMass = MaxArea * 200;
			Temperature = 1000;
		}

		internal void Update(double timeElapsed)
		{
			double burntUnits = ConversionRate * timeElapsed;
			if (Engine.Boiler.Blowers.Active)
			{
				burntUnits *= 2.0;
			}

			burntUnits = Math.Min(burntUnits, FireMass); // can't burn more than what's in the fire!

			FireMass -= burntUnits;
			if (FireMass < MaxArea)
			{
				FireArea -= burntUnits;
			}
			
			if (FireArea < 0)
			{
				FireArea = 0;
			}

			if (Temperature < MaxTemperature)
			{
				Temperature += burntUnits;
			}
		}

		internal void AddFuel()
		{
			FireMass += UnitsPerShovel;
			FireArea += UnitsPerShovel * 0.1; // 1kg of coal to ~10cm square
			Temperature -= UnitsPerShovel * 200;
			if (FireArea > MaxArea)
			{
				FireArea = MaxArea;
			}

			if (ShovelSound != null)
			{
				ShovelSound.Play(Engine.Car, false);
			}
		}
	}
}
