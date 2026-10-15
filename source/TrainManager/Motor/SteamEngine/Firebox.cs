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

namespace TrainManager.Motor
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

		/// <summary>The current heat output ratio</summary>
		/// <remarks>Assumed coal combustion temperature</remarks>
		public double HeatOutput => (FuelLevel / IdealFuelLevel) * (Temperature / 1000);

		private bool doorOpenPressed;

		private bool doorClosePressed;

		public Firebox(TractionModel engine, double maxFuelLevel, double currentFuelLevel, double idealFuelLevel,
			double currentTemperature) : base(engine)
		{
			MaxFuelLevel = maxFuelLevel;
			FuelLevel = currentFuelLevel;
			IdealFuelLevel = idealFuelLevel;
			Temperature = currentTemperature;
			DoorState = 1; // fully open
			doorOpenPressed = false;
			doorClosePressed = false;
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
