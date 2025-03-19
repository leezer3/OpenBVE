using System.Collections.Generic;

namespace OpenBveApi.World
{
	/// <summary>Available units of length</summary>
	public enum UnitOfPressure
	{
		/// <summary>Pounds per Square Inch</summary>
		PoundsPerSquareInch,
		/// <summary>KiloPascal</summary>
		KiloPascal,
		/// <summary>Pascal</summary>
		Pascal
	}

	/// <summary>Implements the force convertor</summary>
	public class PressureConverter : UnitConverter<UnitOfPressure, double>
	{
		static PressureConverter()
		{
			BaseUnit = UnitOfPressure.KiloPascal;
			RegisterConversion(UnitOfPressure.PoundsPerSquareInch, v => v / 6.89476, v => v * 6.89476);
			RegisterConversion(UnitOfPressure.Pascal, v => v * 1000, v => v / 1000);
			
			KnownUnits = new Dictionary<string, UnitOfPressure>
			{
				{"psi", UnitOfPressure.PoundsPerSquareInch}, {"poundspersquareinch", UnitOfPressure.PoundsPerSquareInch}, {"kilopascal", UnitOfPressure.KiloPascal}, {"kpa", UnitOfPressure.KiloPascal}, {"pascal", UnitOfPressure.Pascal}, {"pa", UnitOfPressure.Pascal}
			};
		}

		/// <summary>Contains the known units</summary>
		public static Dictionary<string, UnitOfPressure> KnownUnits;
	}
}
