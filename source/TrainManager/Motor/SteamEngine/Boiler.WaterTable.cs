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

namespace TrainManager.SteamEngine
{
	internal static class WaterTable
	{
		internal static WaterTableEntry[] Values =
		{
			new WaterTableEntry(0, 1.0),
			new WaterTableEntry(20, 1.00180),
			new WaterTableEntry(25, 1.00296),
			new WaterTableEntry(30, 1.00437),
			new WaterTableEntry(35, 1.00600),
			new WaterTableEntry(40, 1.00785),
			new WaterTableEntry(45, 1.00989),
			new WaterTableEntry(50, 1.01210),
			new WaterTableEntry(55, 1.01452),
			new WaterTableEntry(60, 1.01709),
			new WaterTableEntry(65, 1.01984),
			new WaterTableEntry(70, 1.02275),
			new WaterTableEntry(75, 1.02581),
			new WaterTableEntry(80, 1.02903),
			new WaterTableEntry(85, 1.03241),
			new WaterTableEntry(90, 1.03594),
			new WaterTableEntry(95, 1.03962),
			new WaterTableEntry(100, 1.04346),
			new WaterTableEntry(110, 1.05158),
			new WaterTableEntry(120, 1.06032),
			new WaterTableEntry(140, 1.07976),
		};


		internal static double GetDensity(double temperature)
		{
			int tableIndex;
			for (tableIndex = 0; tableIndex < Values.Length - 1; tableIndex++)
			{
				if (Values[tableIndex].Temperature > temperature)
				{
					break;
				}
			}

			if (tableIndex > 0)
			{
				double mu = (temperature - Values[tableIndex - 1].Temperature) / (Values[tableIndex].Temperature - Values[tableIndex - 1].Temperature);
				return (Values[tableIndex - 1].SpecificVolume + ((Values[tableIndex].SpecificVolume - Values[tableIndex - 1].SpecificVolume) * mu));
			}

			return Values[tableIndex].SpecificVolume;
		}

		internal struct WaterTableEntry
		{
			internal int Temperature;
			internal double SpecificVolume;

			internal WaterTableEntry(int temperature, double specificVolume)
			{
				Temperature = temperature;
				SpecificVolume = specificVolume;
			}
		}
	}
}
