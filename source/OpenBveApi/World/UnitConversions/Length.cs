using System.Collections.Generic;

namespace OpenBveApi.World
{
	/// <summary>Available units of length</summary>
	public enum UnitOfLength
	{
		/// <summary>Millimeters</summary>
		Millimeter,
		/// <summary>Centimeters</summary>
		Centimeter,
		/// <summary>Meters</summary>
		Meter,
		/// <summary>Yards</summary>
		Yard,
		/// <summary>Kilometers</summary>
		Kilometer,
		/// <summary>Inches</summary>
		Inches,
		/// <summary>Feet</summary>
		Feet,
	}

	/// <summary>Implements the length convertor</summary>
	public class LengthConverter : UnitConverter<UnitOfLength, double>
	{
		static LengthConverter()
		{
			BaseUnit = UnitOfLength.Meter;
			RegisterConversion(UnitOfLength.Millimeter, v => v * 1000, v => v / 1000);
			RegisterConversion(UnitOfLength.Centimeter, v => v * 100, v => v / 100);
			RegisterConversion(UnitOfLength.Kilometer, v => v / 1000, v => v * 1000);
			RegisterConversion(UnitOfLength.Yard, v => v / 1.094, v => v * 1.094);
			RegisterConversion(UnitOfLength.Inches, v => v * 39.37, v => v / 39.37);
			RegisterConversion(UnitOfLength.Feet, v => v * 3.281, v => v / 3.281);
			KnownUnits = new Dictionary<string, UnitOfLength>
			{
				{"mm", UnitOfLength.Millimeter}, {"millimeter", UnitOfLength.Millimeter}, {"millimeters", UnitOfLength.Millimeter},
				{"cm", UnitOfLength.Centimeter}, {"centimeter", UnitOfLength.Centimeter}, {"centimeters", UnitOfLength.Centimeter},
				{"m", UnitOfLength.Meter}, {"meter", UnitOfLength.Meter}, {"meters", UnitOfLength.Meter},
				{"km", UnitOfLength.Kilometer}, {"kilometer", UnitOfLength.Kilometer}, {"kilometers", UnitOfLength.Kilometer},
				{"in", UnitOfLength.Inches}, {"ins", UnitOfLength.Inches}, {"inch", UnitOfLength.Inches}, {"inches", UnitOfLength.Inches},
				{"ft", UnitOfLength.Feet}, {"foot", UnitOfLength.Feet}, {"feet", UnitOfLength.Feet},
				{"yd", UnitOfLength.Yard}, {"yard", UnitOfLength.Yard}, {"yards", UnitOfLength.Yard}
			};
		}

		/// <summary>Contains the known units</summary>
		public static Dictionary<string, UnitOfLength> KnownUnits;

		/// <summary>Attempts to convert a string containing a number to a double in the desired units</summary>
		/// <param name="s">The string</param>
		/// <param name="d">The returned double</param>
		/// <param name="stringUnit">The units of the string</param>
		/// <param name="desiredUnit">The desired units</param>
		/// <returns>True if conversion succeeds, false otherwise</returns>
		public bool TryParse(string s, out double d, UnitOfLength stringUnit, UnitOfLength desiredUnit)
		{
			if (!double.TryParse(s, out d))
			{
				return false;
			}

			d = Convert(d, stringUnit, desiredUnit);
			return true;
		}
	}
}
