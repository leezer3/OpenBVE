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

using System.Collections.Generic;

namespace TrainManager.SteamEngine
{
	internal static class FuelTable
	{
		internal static Dictionary<FireboxFuel, FuelProperties> Values = new Dictionary<FireboxFuel, FuelProperties>
		{
			{ FireboxFuel.Coal, new FuelProperties(1000, 18000, 0.33) },
			{ FireboxFuel.Wood, new FuelProperties(700, 5400, 0.48) },
			// assume kerosene or similar heavy oil
			{ FireboxFuel.Oil, new FuelProperties(1600, 1, 0.7)}
		};
	}

	internal struct FuelProperties
	{
		internal double CombustionTemperature;

		internal double BurnTime;

		internal double SpecificHeat;

		internal FuelProperties(double combustionTemperature, double burnTime, double specificHeat)
		{
			CombustionTemperature = combustionTemperature;
			BurnTime = burnTime;
			SpecificHeat = specificHeat;
		}
	}


	public enum FireboxFuel
	{
		Coal,
		Wood,
		Oil
	}
}
