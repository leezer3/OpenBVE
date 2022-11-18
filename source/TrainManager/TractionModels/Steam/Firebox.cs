//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2022, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using SoundManager;

namespace TrainManager.TractionModels.Steam
{
	public class Firebox
	{
		/// <summary>Holds a reference to the base engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>The maximum fire temperature</summary>
		internal readonly double MaxTemperature;
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
		public double ConversionRate => MaxConversionRate * ((FireArea / MaxArea) * (Temperature / MaxTemperature));
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
			// TODO: More generic starting paramaters
			FireArea = MaxArea / 2;
			FireMass = MaxArea * 20;
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
			double unitsAdded = Math.Min(Engine.Tender.FuelLevel, UnitsPerShovel);
			FireMass += unitsAdded;
			FireArea += unitsAdded * 0.1; // 1kg of coal to ~10cm square
			Temperature -= unitsAdded * 10;
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
