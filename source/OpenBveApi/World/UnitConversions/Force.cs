using System.Collections.Generic;

namespace OpenBveApi.World
{
	/// <summary>Available units of length</summary>
	public enum UnitOfForce
	{
		/// <summary>Newtons</summary>
		Newton,
		/// <summary>KiloNewtons</summary>
		KiloNewton,
	}

	/// <summary>Implements the length convertor</summary>
	public class ForceConverter : UnitConverter<UnitOfForce, double>
	{
		static ForceConverter()
		{
			BaseUnit = UnitOfForce.Newton;
			RegisterConversion(UnitOfForce.KiloNewton, v => v / 1000.0, v => v * 1000.0);
			KnownUnits = new Dictionary<string, UnitOfForce>
			{
				{"n", UnitOfForce.Newton}, {"newton", UnitOfForce.Newton}, {"kN", UnitOfForce.KiloNewton}, {"kilonewton", UnitOfForce.KiloNewton}

			};
		}

		/// <summary>Contains the known units</summary>
		public static Dictionary<string, UnitOfForce> KnownUnits;
	}
}
