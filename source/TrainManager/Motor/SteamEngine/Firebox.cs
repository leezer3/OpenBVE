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

		/// <summary>The current heat output ratio</summary>
		/// <remarks>Assumed coal combustion temperature</remarks>
		public double HeatOutput => (FuelLevel / IdealFuelLevel) * (Temperature / 1000);

		public Firebox(TractionModel engine, double maxFuelLevel, double currentFuelLevel, double idealFuelLevel, double currentTemperature) : base(engine)
		{
			MaxFuelLevel = maxFuelLevel;
			FuelLevel = currentFuelLevel;
			IdealFuelLevel = idealFuelLevel;
			Temperature = currentTemperature;
		}

		public override void Update(double timeElapsed)
		{

		}
	}
}
