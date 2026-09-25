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
	internal static class SteamTable
	{
		// https://web.archive.org/web/20260000000000*/https://www.engineeringtoolbox.com/saturated-steam-properties-d_457.html
		// This is a pretty standard set of physics constants, known from c. 18th century
		internal static SteamTableEntry[] Values =
		{
			new SteamTableEntry(0.03, 24.1, 45.667, 0.022, 101, 2545.64, 2444.65, 1.8694),
			new SteamTableEntry(0.04, 28.98, 34.802, 0.029, 121.41, 2554.51, 2433.1, 1.8736),
			new SteamTableEntry(0.05, 32.9, 28.194, 0.035, 137.77, 2561.59, 2423.82, 1.8774),
			new SteamTableEntry(0.06, 36.18, 23.741, 0.042, 151.5, 2567.51, 2416.01, 1.8808),
			new SteamTableEntry(0.07, 39.02, 20.531, 0.049, 163.38, 2572.62, 2409.24, 1.884),
			new SteamTableEntry(0.08, 41.53, 18.105, 0.055, 173.87, 2577.11, 2403.25, 1.8871),
			new SteamTableEntry(0.09, 43.79, 16.204, 0.062, 183.28, 2581.14, 2397.85, 1.8899),
			new SteamTableEntry(0.1, 45.83, 14.675, 0.068, 191.84, 2584.78, 2392.94, 1.8927),
			new SteamTableEntry(0.2, 60.09, 7.65, 0.131, 251.46, 2609.86, 2358.4, 1.9156),
			new SteamTableEntry(0.3, 69.13, 5.229, 0.191, 289.31, 2625.43, 2336.13, 1.9343),
			new SteamTableEntry(0.4, 75.89, 3.993, 0.25, 317.65, 2636.88, 2319.23, 1.9506),
			new SteamTableEntry(0.5, 81.35, 3.24, 0.309, 340.57, 2645.99, 2305.42, 1.9654),
			new SteamTableEntry(0.6, 85.95, 2.732, 0.366, 359.93, 2653.57, 2293.64, 1.979),
			new SteamTableEntry(0.7, 89.96, 2.365, 0.423, 376.77, 2660.07, 2283.3, 1.9919),
			new SteamTableEntry(0.8, 93.51, 2.087, 0.479, 391.73, 2665.77, 2274.05, 2.004),
			new SteamTableEntry(0.9, 96.71, 1.869, 0.535, 405.21, 2670.85, 2265.65, 2.0156),
			new SteamTableEntry(1, 99.63, 1.694, 0.59, 417.51, 2675.43, 2257.92, 2.0267),
			new SteamTableEntry(1.1, 102.32, 1.549, 0.645, 428.84, 2679.61, 2250.76, 2.0373),
			new SteamTableEntry(1.2, 104.81, 1.428, 0.7, 439.36, 2683.44, 2244.08, 2.0476),
			new SteamTableEntry(1.3, 107.13, 1.325, 0.755, 449.19, 2686.98, 2237.79, 2.0576),
			new SteamTableEntry(1.4, 109.32, 1.236, 0.809, 458.42, 2690.28, 2231.86, 2.0673),
			new SteamTableEntry(1.5, 111.37, 1.159, 0.863, 467.13, 2693.36, 2226.23, 2.0768),
			new SteamTableEntry(1.6, 113.32, 1.091, 0.916, 475.38, 2696.25, 2220.87, 2.086),
			new SteamTableEntry(1.7, 115.17, 1.031, 0.97, 483.22, 2698.97, 2215.75, 2.095),
			new SteamTableEntry(1.8, 116.93, 0.977, 1.023, 490.7, 2701.54, 2210.84, 2.1037),
			new SteamTableEntry(1.9, 118.62, 0.929, 1.076, 497.85, 2703.98, 2206.13, 2.1124),
			new SteamTableEntry(2, 120.23, 0.885, 1.129, 504.71, 2706.29, 2201.59, 2.1208),
			new SteamTableEntry(2.2, 123.27, 0.81, 1.235, 517.63, 2710.6, 2192.98, 2.1372),
			new SteamTableEntry(2.4, 126.09, 0.746, 1.34, 529.64, 2714.55, 2184.91, 2.1531),
			new SteamTableEntry(2.6, 128.73, 0.693, 1.444, 540.88, 2718.17, 2177.3, 2.1685),
			new SteamTableEntry(2.8, 131.2, 0.646, 1.548, 551.45, 2721.54, 2170.08, 2.1835),
			new SteamTableEntry(3, 133.54, 0.606, 1.651, 561.44, 2724.66, 2163.22, 2.1981),
			new SteamTableEntry(3.5, 138.87, 0.524, 1.908, 584.28, 2731.63, 2147.35, 2.2331),
			new SteamTableEntry(4, 143.63, 0.462, 2.163, 604.68, 2737.63, 2132.95, 2.2664),
			new SteamTableEntry(4.5, 147.92, 0.414, 2.417, 623.17, 2742.88, 2119.71, 2.2983),
			new SteamTableEntry(5, 151.85, 0.375, 2.669, 640.12, 2747.54, 2107.42, 2.3289),
			new SteamTableEntry(5.5, 155.47, 0.342, 2.92, 655.81, 2751.7, 2095.9, 2.3585),
			new SteamTableEntry(6, 158.84, 0.315, 3.17, 670.43, 2755.46, 2085.03, 2.3873),
			new SteamTableEntry(6.5, 161.99, 0.292, 3.419, 684.14, 2758.87, 2074.73, 2.4152),
			new SteamTableEntry(7, 164.96, 0.273, 3.667, 697.07, 2761.98, 2064.92, 2.4424),
			new SteamTableEntry(7.5, 167.76, 0.255, 3.915, 709.3, 2764.84, 2055.53, 2.469),
			new SteamTableEntry(8, 170.42, 0.24, 4.162, 720.94, 2767.46, 2046.53, 2.4951),
			new SteamTableEntry(8.5, 172.94, 0.227, 4.409, 732.03, 2769.89, 2037.86, 2.5206),
			new SteamTableEntry(9, 175.36, 0.215, 4.655, 742.64, 2772.13, 2029.49, 2.5456),
			new SteamTableEntry(9.5, 177.67, 0.204, 4.901, 752.82, 2774.22, 2021.4, 2.5702),
			new SteamTableEntry(10, 179.88, 0.194, 5.147, 762.6, 2776.16, 2013.56, 2.5944),
			new SteamTableEntry(11, 184.06, 0.177, 5.638, 781.11, 2779.66, 1998.55, 2.6418),
			new SteamTableEntry(12, 187.96, 0.163, 6.127, 798.42, 2782.73, 1984.31, 2.6878),
			new SteamTableEntry(13, 191.6, 0.151, 6.617, 814.68, 2785.42, 1970.73, 2.7327),
			new SteamTableEntry(14, 195.04, 0.141, 7.106, 830.05, 2787.79, 1957.73, 2.7767),
			new SteamTableEntry(15, 198.28, 0.132, 7.596, 844.64, 2789.88, 1945.24, 2.8197),
			new SteamTableEntry(16, 201.37, 0.124, 8.085, 858.54, 2791.73, 1933.19, 2.862),
			new SteamTableEntry(17, 204.3, 0.117, 8.575, 871.82, 2793.37, 1921.55, 2.9036),
			new SteamTableEntry(18, 207.11, 0.11, 9.065, 884.55, 2794.81, 1910.27, 2.9445),
			new SteamTableEntry(19, 209.79, 0.105, 9.556, 896.78, 2796.09, 1899.31, 2.9849),
			new SteamTableEntry(20, 212.37, 0.1, 10.047, 908.56, 2797.21, 1888.65, 3.0248),
			new SteamTableEntry(21, 214.85, 0.095, 10.539, 919.93, 2798.18, 1878.25, 3.0643),
			new SteamTableEntry(22, 217.24, 0.091, 11.032, 930.92, 2799.03, 1868.11, 3.1034),
			new SteamTableEntry(23, 219.55, 0.087, 11.525, 941.57, 2799.77, 1858.2, 3.1421),
			new SteamTableEntry(24, 221.78, 0.083, 12.02, 951.9, 2800.39, 1848.49, 3.1805),
			new SteamTableEntry(25, 223.94, 0.08, 12.515, 961.93, 2800.91, 1838.98, 3.2187),
			new SteamTableEntry(26, 226.03, 0.077, 13.012, 971.69, 2801.35, 1829.66, 3.2567),
			new SteamTableEntry(27, 228.06, 0.074, 13.509, 981.19, 2801.69, 1820.5, 3.2944),
			new SteamTableEntry(28, 230.04, 0.071, 14.008, 990.46, 2801.96, 1811.5, 3.332),
			new SteamTableEntry(29, 231.96, 0.069, 14.508, 999.5, 2802.15, 1802.65, 3.3695),
			new SteamTableEntry(30, 233.84, 0.067, 15.009, 1008.33, 2802.27, 1793.94, 3.4069),
		};

		internal const double DischargeCoefficient = 0.88;

		internal const double SteamEnthalpyNominal = 2773;

		internal const double InjectorWaterEnthalpyNominal = 743;

		internal static double GetSteamDensity(double pressure)
		{
			pressure /= 14.5038; // PSI to Bar
			int tableIndex;
			for (tableIndex = 0; tableIndex < Values.Length; tableIndex++)
			{
				if (Values[tableIndex].Pressure > pressure)
				{
					break;
				}
			}

			if (tableIndex <= Values.Length - 1)
			{
				double mu = (pressure - Values[tableIndex].Pressure) / (Values[tableIndex + 1].Pressure - Values[tableIndex].Pressure);
				return Values[tableIndex].SpecificVolume + ((Values[tableIndex + 1].SpecificVolume - Values[tableIndex].SpecificVolume) * mu);
			}

			return Values[tableIndex].SpecificVolume;
		}

		internal static double GetSteamVolume(double pressure)
		{
			pressure /= 14.5038; // PSI to Bar
			int tableIndex;
			for (tableIndex = 0; tableIndex < Values.Length -1; tableIndex++)
			{
				if (Values[tableIndex].Pressure > pressure)
				{
					break;
				}
			}

			if (tableIndex > 0)
			{
				double mu = (pressure - Values[tableIndex - 1].Pressure) / (Values[tableIndex].Pressure - Values[tableIndex - 1].Pressure);
				return Values[tableIndex - 1].Density + ((Values[tableIndex].Density - Values[tableIndex -1].Density) * mu);
			}

			return Values[tableIndex].Density;
		}

		internal static double GetSteamEnthalpy(double pressure)
		{
			pressure /= 14.5038; // PSI to Bar
			int tableIndex;
			for (tableIndex = 0; tableIndex < Values.Length - 1; tableIndex++)
			{
				if (Values[tableIndex].Pressure > pressure)
				{
					break;
				}
			}

			if (tableIndex <= Values.Length - 1)
			{
				double mu = (pressure - Values[tableIndex].Pressure) / (Values[tableIndex + 1].Pressure - Values[tableIndex].Pressure);
				return Values[tableIndex].SteamEnthalpy + ((Values[tableIndex + 1].SteamEnthalpy - Values[tableIndex].SteamEnthalpy) * mu);
			}

			return Values[tableIndex].SteamEnthalpy;
		}

		internal static double GetPressure(double volume)
		{
			int tableIndex;
			for (tableIndex = 0; tableIndex < Values.Length -1; tableIndex++)
			{
				if (Values[tableIndex].SpecificVolume < volume)
				{
					break;
				}
			}

			if (tableIndex <= Values.Length -1)
			{
				double mu = (volume - Values[tableIndex].SpecificVolume) / (Values[tableIndex + 1].SpecificVolume - Values[tableIndex].SpecificVolume);
				return Values[tableIndex].Pressure + ((Values[tableIndex + 1].Pressure - Values[tableIndex].Pressure) * mu);
			}

			return Values[tableIndex].Pressure;
		}
	}

	internal struct SteamTableEntry
	{
		internal double Pressure;

		internal double BoilingPoint;

		internal double SpecificVolume;

		internal double Density;

		internal double WaterEnthalpy;

		internal double SteamEnthalpy;

		internal double LatentHeat;

		internal double SpecificHeat;
		
		internal SteamTableEntry(double pressure, double boilingPoint, double specificVolume, double density,
			double waterEntalpy, double steamEnthalpy, double latentHeat, double specificHeat)
		{
			Pressure = pressure;
			BoilingPoint = boilingPoint;
			SpecificVolume = specificVolume;
			Density = density;
			WaterEnthalpy = waterEntalpy;
			SteamEnthalpy = steamEnthalpy;
			LatentHeat = latentHeat;
			SpecificHeat = specificHeat;
		}
	}
}
