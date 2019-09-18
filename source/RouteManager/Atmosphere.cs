using System;

namespace OpenBve.RouteManager
{
	/// <summary>Holds the atmospheric methods and properties</summary>
	public struct Atmosphere
	{
		/// <summary>The acceleration due to gravity for this route in m/s²</summary>
		public static double AccelerationDueToGravity = 9.80665;
		/// <summary>The initial air pressure in kPa</summary>
		/// <remarks>Represents a pressure of 1 atmosphere</remarks>
		public static double InitialAirPressure = 101325.0;
		/// <summary>The initial air temperature in degrees kelvin</summary>
		/// <remarks>Represents 20°c</remarks>
		public static double InitialAirTemperature = 293.15;
		/// <summary>The initial sea-level air pressure of this  in kPa</summary>
		/// <remarks>Represents a pressure of 1 atmosphere</remarks>
		public static double SeaLevelAirPressure = 101325.0;
		/// <summary>The sea-level air temperature in degrees kelvin</summary>
		/// <remarks>Represents 20°c</remarks>
		public static double SeaLevelAirTemperature = 293.15;
		/// <summary>Molar Mass</summary>
		internal const double MolarMass = 0.0289644;
		/// <summary>The Universal Gas Constant</summary>
		internal const double UniversalGasConstant = 8.31447;
		/// <summary>The Temperature Lapse Rate</summary>
		/// Defines the rate at which temperature drops per meter climbed
		internal const double TemperatureLapseRate = -0.0065;

		internal const double CoefficientOfStiffness = 144117.325646911;

		/// <summary>Calculates the atmospheric constants for sea-level</summary>
		public static void CalculateSeaLevelConstants()
		{
			SeaLevelAirTemperature = InitialAirTemperature - TemperatureLapseRate * CurrentRoute.InitialElevation;
			double Exponent = AccelerationDueToGravity * MolarMass / (UniversalGasConstant * TemperatureLapseRate);
			double Base = 1.0 + TemperatureLapseRate * CurrentRoute.InitialElevation / SeaLevelAirTemperature;
			if (Base >= 0.0)
			{
				SeaLevelAirPressure = InitialAirPressure * Math.Pow(Base, Exponent);
				if (SeaLevelAirPressure < 0.001) SeaLevelAirPressure = 0.001;
			}
			else
			{
				SeaLevelAirPressure = 0.001;
			}
		}

		/// <summary>Calculates the air temperature for a given elevation</summary>
        /// <param name="Elevation">The elevation for which to calculate the air temperature</param>
        /// <returns>A temperature in degrees kelvin</returns>
        public static double GetAirTemperature(double Elevation)
        {
            double x = SeaLevelAirTemperature + TemperatureLapseRate * Elevation;
            return x >= 1.0 ? x : 1.0;
        }
        /// <summary>Calculates the air density for a given pressure and temperature</summary>
        /// <param name="AirPressure">The air pressure in Pa</param>
        /// <param name="AirTemperature">The air temperature in degrees kelvin</param>
        /// <returns>The air density in kg/m³</returns>
        public static double GetAirDensity(double AirPressure, double AirTemperature)
        {
            double x = AirPressure * MolarMass / (UniversalGasConstant * AirTemperature);
            return x >= 0.001 ? x : 0.001;
        }
        /// <summary>Calculates the air pressure for a given elevation and temperature</summary>
        /// <param name="Elevation">The elevation in m</param>
        /// <param name="AirTemperature">The air temperature in degrees kelvin</param>
        /// <returns>The air pressure in Pa</returns>
        public static double GetAirPressure(double Elevation, double AirTemperature)
        {
            double Exponent = -AccelerationDueToGravity * MolarMass / (UniversalGasConstant * TemperatureLapseRate);
            double Base = 1.0 + TemperatureLapseRate * Elevation / SeaLevelAirTemperature;
            if (!(Base >= 0.0)) return 0.001;
            double x = SeaLevelAirPressure * Math.Pow(Base, Exponent);
            return x >= 0.001 ? x : 0.001;
        }
        /// <summary>Calculates the speed of sound for a given air pressure and temperature</summary>
        /// <param name="AirPressure">The air pressure in Pa</param>
        /// <param name="AirTemperature">The air temperature in degrees kelvin</param>
        /// <returns>The speed of sound in m/s</returns>
        public static double GetSpeedOfSound(double AirPressure, double AirTemperature)
        {
            double AirDensity = GetAirDensity(AirPressure, AirTemperature);
            return Math.Sqrt(CoefficientOfStiffness / AirDensity);
        }
        /// <summary>Calculates the speed of sound for a given air density</summary>
        /// <param name="AirDensity">The air density</param>
        /// <returns>The speed of sound in m/s</returns>
        public static double GetSpeedOfSound(double AirDensity)
        {
            return Math.Sqrt(CoefficientOfStiffness / AirDensity);
        }
	}
}
