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
			RegisterConversion(UnitOfLength.Inches, v => v * 39.37, v => v / 39.37);
			RegisterConversion(UnitOfLength.Feet, v => v * 3.281, v => v / 3.281);
			KnownUnits = new Dictionary<string, UnitOfLength>
			{
				{"mm", UnitOfLength.Millimeter}, {"millimeter", UnitOfLength.Millimeter}, {"millimeters", UnitOfLength.Millimeter},
				{"cm", UnitOfLength.Centimeter}, {"centimeter", UnitOfLength.Centimeter}, {"centimeters", UnitOfLength.Centimeter},
				{"m", UnitOfLength.Meter}, {"meter", UnitOfLength.Meter}, {"meters", UnitOfLength.Meter},
				{"in", UnitOfLength.Inches}, {"ins", UnitOfLength.Inches}, {"inch", UnitOfLength.Inches}, {"inches", UnitOfLength.Inches},
				{"ft", UnitOfLength.Feet}, {"foot", UnitOfLength.Feet}, {"feet", UnitOfLength.Feet}
			};
		}

		/// <summary>Contains the known units</summary>
		public static Dictionary<string, UnitOfLength> KnownUnits;
	}
}
