using System.Collections.Generic;

namespace OpenBveApi.World
{
	/// <summary>Available units of velocity</summary>
	public enum UnitOfVelocity
	{
		/// <summary>Newtons</summary>
		MetersPerSecond,
		/// <summary>KiloNewtons</summary>
		MilesPerHour,
		/// <summary>Pounds of force</summary>
		KilometersPerHour
	}

	/// <summary>Implements the force convertor</summary>
	public class VelocityConverter : UnitConverter<UnitOfVelocity, double>
	{
		static VelocityConverter()
		{
			BaseUnit = UnitOfVelocity.MetersPerSecond;
			RegisterConversion(UnitOfVelocity.KilometersPerHour, v => v * 3.6, v => v / 3.6);
			RegisterConversion(UnitOfVelocity.MilesPerHour, v => v * 2.237, v => v / 2.237);
			KnownUnits = new Dictionary<string, UnitOfVelocity>
			{
				{"m/s", UnitOfVelocity.MetersPerSecond}, {"mph", UnitOfVelocity.MilesPerHour}, {"kmh", UnitOfVelocity.KilometersPerHour}, {"km/h", UnitOfVelocity.KilometersPerHour}

			};
		}

		/// <summary>Contains the known units</summary>
		public static Dictionary<string, UnitOfVelocity> KnownUnits;
	}
}
