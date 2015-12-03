using System;

namespace OpenBve
{
    internal static partial class Game
    {
        /// <summary>The route's comment (For display in the main menu)</summary>
        internal static string RouteComment = "";
        /// <summary>The route's image file (For display in the main menu)</summary>
        internal static string RouteImage = "";
        /// <summary>The acceleration due to gravity for this route in m/s²</summary>
        internal static double RouteAccelerationDueToGravity = 9.80665;
        /// <summary>The rail gauge of this route in meters.</summary>
        internal static double RouteRailGauge = 1.435;
        /// <summary>The initial air pressure of this route in kPa</summary>
        /// Represents a pressure of 1 atmosphere
        internal static double RouteInitialAirPressure = 101325.0;
        /// <summary>The initial air temperature in degrees kelvin</summary>
        /// Represents 20°c
        internal static double RouteInitialAirTemperature = 293.15;
        /// <summary>The initial elevation in meters</summary>
        internal static double RouteInitialElevation = 0.0;
        /// <summary>The initial sea-level air pressure of this route in kPa</summary>
        /// Represents a pressure of 1 atmosphere
        internal static double RouteSeaLevelAirPressure = 101325.0;
        /// <summary>The sea-level air temperature in degrees kelvin</summary>
        /// Represents 20°c
        internal static double RouteSeaLevelAirTemperature = 293.15;
        internal static double[] RouteUnitOfLength = new double[] { 1.0 };
        internal const double CoefficientOfGroundFriction = 0.5;
        /// <summary>The speed difference in m/s above which derailments etc. will occur</summary>
        internal const double CriticalCollisionSpeedDifference = 8.0;
        /// <summary>The number of pascals leaked by the brake pipe each second</summary>
        internal const double BrakePipeLeakRate = 500000.0;
        /// <summary>Molar Mass</summary>
        internal const double MolarMass = 0.0289644;
        /// <summary>The Universal Gas Constant</summary>
        internal const double UniversalGasConstant = 8.31447;
        /// <summary>The Temperature Lapse Rate</summary>
        /// Defines the rate at which temperature drops per meter climbed
        internal const double TemperatureLapseRate = -0.0065;

        internal const double CoefficientOfStiffness = 144117.325646911;

        /*
         * This group of functions is used to calculate atmospheric constants
         * These are used in determining things such as the speed of sound
         */

        /// <summary>Calculates the atmospheric constants for sea-level</summary>
        internal static void CalculateSeaLevelConstants()
        {
            RouteSeaLevelAirTemperature = RouteInitialAirTemperature - TemperatureLapseRate * RouteInitialElevation;
            double Exponent = RouteAccelerationDueToGravity * MolarMass / (UniversalGasConstant * TemperatureLapseRate);
            double Base = 1.0 + TemperatureLapseRate * RouteInitialElevation / RouteSeaLevelAirTemperature;
            if (Base >= 0.0)
            {
                RouteSeaLevelAirPressure = RouteInitialAirPressure * Math.Pow(Base, Exponent);
                if (RouteSeaLevelAirPressure < 0.001) RouteSeaLevelAirPressure = 0.001;
            }
            else
            {
                RouteSeaLevelAirPressure = 0.001;
            }
        }
        /// <summary>Calculates the air temperature for a given elevation</summary>
        /// <param name="Elevation">The elevation for which to calculate the air temperature</param>
        /// <returns>A temperature in degrees kelvin</returns>
        internal static double GetAirTemperature(double Elevation)
        {
            double x = RouteSeaLevelAirTemperature + TemperatureLapseRate * Elevation;
            return x >= 1.0 ? x : 1.0;
        }
        /// <summary>Calculates the air density for a given pressure and temperature</summary>
        /// <param name="AirPressure">The air pressure in Pa</param>
        /// <param name="AirTemperature">The air temperature in degrees kelvin</param>
        /// <returns>The air density in kg/m³</returns>
        internal static double GetAirDensity(double AirPressure, double AirTemperature)
        {
            double x = AirPressure * MolarMass / (UniversalGasConstant * AirTemperature);
            return x >= 0.001 ? x : 0.001;
        }
        /// <summary>Calculates the air pressure for a given elevation and temperature</summary>
        /// <param name="Elevation">The elevation in m</param>
        /// <param name="AirTemperature">The air temperature in degrees kelvin</param>
        /// <returns>The air pressure in Pa</returns>
        internal static double GetAirPressure(double Elevation, double AirTemperature)
        {
            double Exponent = -RouteAccelerationDueToGravity * MolarMass / (UniversalGasConstant * TemperatureLapseRate);
            double Base = 1.0 + TemperatureLapseRate * Elevation / RouteSeaLevelAirTemperature;
            if (!(Base >= 0.0)) return 0.001;
            double x = RouteSeaLevelAirPressure * Math.Pow(Base, Exponent);
            return x >= 0.001 ? x : 0.001;
        }
        /// <summary>Calculates the speed of sound for a given air pressure and temperature</summary>
        /// <param name="AirPressure">The air pressure in Pa</param>
        /// <param name="AirTemperature">The air temperature in degrees kelvin</param>
        /// <returns>The speed of sound in m/s</returns>
        internal static double GetSpeedOfSound(double AirPressure, double AirTemperature)
        {
            double AirDensity = GetAirDensity(AirPressure, AirTemperature);
            return Math.Sqrt(CoefficientOfStiffness / AirDensity);
        }
        /// <summary>Calculates the speed of sound for a given air density</summary>
        /// <param name="AirDensity">The air density</param>
        /// <returns>The speed of sound in m/s</returns>
        internal static double GetSpeedOfSound(double AirDensity)
        {
            return Math.Sqrt(CoefficientOfStiffness / AirDensity);
        }
    }
}
