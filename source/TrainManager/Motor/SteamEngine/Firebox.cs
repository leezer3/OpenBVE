//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
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
using OpenBveApi.Interface;
using TrainManager.Motor;

namespace TrainManager.SteamEngine
{
	public class Firebox : AbstractComponent
	{
		/// <summary>The maximum fuel level in the firebox</summary>
		private readonly double MaxFuelLevel;
		/// <summary>The ideal fuel level in the firebox</summary>
		private readonly double IdealFuelLevel;
		/// <summary>The current firebox temperature in °C</summary>
		public double Temperature;
		/// <summary>The current fuel level in the firebox</summary>
		public double FuelLevel;
		/// <summary>The state of the firebox door</summary>
		public double DoorState;
		/// <summary>The base mass of one shovel of coal</summary>
		public readonly double ShovelMass;
		/// <summary>The specific heat value for the fuel</summary>
		public readonly double SpecificHeat;
		/// <summary>The ambient temperature value of the fuel</summary>
		public const double AmbientTemp = 20;
		/// <summary>Combustion temperature for standard anthracite coal</summary>
		public readonly double CombustionTemp;

		private readonly double BurnTime;
		/// <summary>The current heat output ratio</summary>
		public double HeatOutput => (FuelLevel / IdealFuelLevel) * (Temperature / CombustionTemp);



		private bool doorOpenPressed;

		private bool doorClosePressed;

		private double burnRate;

		public Firebox(TractionModel engine, FireboxFuel fuel, double maxFuelLevel, double shovelMass, double currentFuelLevel, double idealFuelLevel,
			double currentTemperature) : base(engine)
		{
			SpecificHeat = FuelTable.Values[fuel].SpecificHeat;
			CombustionTemp = FuelTable.Values[fuel].CombustionTemperature;
			BurnTime = FuelTable.Values[fuel].BurnTime;
			MaxFuelLevel = maxFuelLevel;
			FuelLevel = currentFuelLevel;
			IdealFuelLevel = idealFuelLevel;
			Temperature = currentTemperature;
			DoorState = 1; // fully open
			doorOpenPressed = false;
			doorClosePressed = false;
			ShovelMass = shovelMass;
		}

		public override void Update(double timeElapsed)
		{
			if (doorOpenPressed)
			{
				DoorState += timeElapsed;
			}

			if (doorClosePressed)
			{
				DoorState -= timeElapsed;
			}

			DoorState = Math.Min(Math.Max(DoorState, 0), 1.0);

			// Some figure fudging: coal will take ~30 minutes to get to full combustion temperature
			// thus an increase of ~0.544c per second in overall fire temp when below max
			if (Temperature < CombustionTemp)
			{
				Temperature += 0.544 * timeElapsed;
			}
			
			FuelLevel -= burnRate * timeElapsed;
		}

		public override void ControlDown(Translations.Command command)
		{
			switch (command)
			{
				case Translations.Command.FireboxDoorOpen:
					doorOpenPressed = true;
					break;
				case Translations.Command.FireboxDoorClose:
					doorClosePressed = true;
					break;
				case Translations.Command.AddFuel:
					if (FuelLevel < MaxFuelLevel)
					{
						// one shovel per keypress, but use a little randomisation for the mass
						double fuelAdded = ShovelMass * (TrainManagerBase.currentHost.Random.Next(9, 11) / 10.0);
						Temperature = (Temperature * (FuelLevel * SpecificHeat) - AmbientTemp * (fuelAdded * SpecificHeat)) / (FuelLevel * fuelAdded * SpecificHeat * SpecificHeat);
						FuelLevel += fuelAdded;
						FuelLevel = Math.Min(FuelLevel, MaxFuelLevel);
						// we assume 1 / BurnTime of the fire mass burns per second
						burnRate = FuelLevel / BurnTime;
					}
					break;
			}
		}

		public override void ControlUp(Translations.Command command)
		{
			switch (command)
			{
				case Translations.Command.FireboxDoorOpen:
					doorOpenPressed = false;
					break;
				case Translations.Command.FireboxDoorClose:
					doorClosePressed = false;
					break;
			}
		}
	}
}
