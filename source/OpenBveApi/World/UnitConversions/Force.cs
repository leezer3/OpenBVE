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
		/// <summary>Pounds of force</summary>
		PoundsOfForce,
		/// <summary>Tons of force</summary>
		TonForce
	}

	/// <summary>Implements the force convertor</summary>
	public class ForceConverter : UnitConverter<UnitOfForce, double>
	{
		static ForceConverter()
		{
			BaseUnit = UnitOfForce.Newton;
			RegisterConversion(UnitOfForce.KiloNewton, v => v / 1000.0, v => v * 1000.0);
			RegisterConversion(UnitOfForce.PoundsOfForce, v => v / 4.44822, v => v * 4.44822);
			RegisterConversion(UnitOfForce.TonForce, v => v / 9806.65, v => v * 9806.65);
			KnownUnits = new Dictionary<string, UnitOfForce>
			{
				{"n", UnitOfForce.Newton}, {"newton", UnitOfForce.Newton}, {"kn", UnitOfForce.KiloNewton}, {"kilonewton", UnitOfForce.KiloNewton}, {"lbf", UnitOfForce.PoundsOfForce}, {"t", UnitOfForce.TonForce}

			};
		}

		/// <summary>Contains the known units</summary>
		public static Dictionary<string, UnitOfForce> KnownUnits;
	}
}
