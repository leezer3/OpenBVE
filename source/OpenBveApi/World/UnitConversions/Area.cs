using System.Collections.Generic;

namespace OpenBveApi.World
{
	/// <summary>Available units of length</summary>
	public enum UnitOfArea
	{
		/// <summary>Square meters</summary>
		SquareMeter,
		/// <summary>Square centimeters</summary>
		SquareCentimeter,
		/// <summary>Square feet</summary>
		SquareFoot
	}

	/// <summary>Implements the area convertor</summary>
	public class AreaConverter : UnitConverter<UnitOfArea, double>
	{
		static AreaConverter()
		{
			BaseUnit = UnitOfArea.SquareMeter;
			RegisterConversion(UnitOfArea.SquareCentimeter, v => v / 10000, v => v * 10000);
			RegisterConversion(UnitOfArea.SquareFoot, v => v / 10.764, v => v * 10.764);
			KnownUnits = new Dictionary<string, UnitOfArea>
			{
				{"sqm", UnitOfArea.SquareMeter}, {"sq m", UnitOfArea.SquareMeter}, {"square meters", UnitOfArea.SquareMeter},
				{"sqcm", UnitOfArea.SquareCentimeter}, {"sq cm", UnitOfArea.SquareCentimeter}, {"square centimeter", UnitOfArea.SquareMeter},
				{"sqft", UnitOfArea.SquareMeter}, {"sq ft", UnitOfArea.SquareMeter}, {"square feet", UnitOfArea.SquareMeter},
				
			};
		}

		/// <summary>Contains the known units</summary>
		public static Dictionary<string, UnitOfArea> KnownUnits;
	}
}
