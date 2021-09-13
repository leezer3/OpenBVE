using System.Collections.Generic;

namespace OpenBveApi.World
{
	/// <summary>Available units of length</summary>
	public enum UnitOfWeight
	{
		/// <summary>Grams</summary>
		Grams,
		/// <summary>Kilograms</summary>
		Kilograms,
		/// <summary>Metric tonnes</summary>
		MetricTonnes,
		/// <summary>Imperial tons</summary>
		ImperialTons
	}

	/// <summary>Implements the length convertor</summary>
	public class WeightConverter : UnitConverter<UnitOfWeight, double>
	{
		static WeightConverter()
		{
			BaseUnit = UnitOfWeight.Kilograms;
			RegisterConversion(UnitOfWeight.Grams, v => v * 100.0, v => v / 100.0);
			RegisterConversion(UnitOfWeight.MetricTonnes, v => v / 1000.0, v => v * 1000.0);
			RegisterConversion(UnitOfWeight.ImperialTons, v => v / 1016.05, v => v * 1016.05);
			KnownUnits = new Dictionary<string, UnitOfWeight>
			{
				{"g", UnitOfWeight.Grams}, {"gram", UnitOfWeight.Grams}, {"grams", UnitOfWeight.Grams},
				{"kg", UnitOfWeight.Kilograms}, {"kilogram", UnitOfWeight.Kilograms}, {"kilograms", UnitOfWeight.Kilograms},
				/*
				 * NOTE: Unfortunately, metric and imperial tons are often confused / interchangable
				 *       They're close enough it's unlikely to matter for our purposes, but we're going to assume that
				 *       the T abbreviation refers to a metric ton for convienence.
				 */
				{"t", UnitOfWeight.MetricTonnes}, {"tonne", UnitOfWeight.MetricTonnes}, {"tonnes", UnitOfWeight.MetricTonnes},
				{"ton", UnitOfWeight.ImperialTons}, {"tons", UnitOfWeight.ImperialTons}
				
			};
		}

		/// <summary>Contains the known units</summary>
		public static Dictionary<string, UnitOfWeight> KnownUnits;
	}
}
