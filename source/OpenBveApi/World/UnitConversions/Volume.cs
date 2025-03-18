using System.Collections.Generic;
using OpenBveApi.World;

/// <summary>Available units of volume</summary>
public enum UnitOfVolume
{
	/// <summary>Litres</summary>
	Litres,
	/// <summary>Gallons (US)</summary>
	Gallons,
}

/// <summary>Implements the volume convertor</summary>
public class VolumeConverter : UnitConverter<UnitOfVolume, double>
{
	static VolumeConverter()
	{
		BaseUnit = UnitOfVolume.Litres;
		RegisterConversion(UnitOfVolume.Gallons, v => v / 3.78541, v => v * 3.78541);
		KnownUnits = new Dictionary<string, UnitOfVolume>
		{
			{"l", UnitOfVolume.Litres}, {"litres", UnitOfVolume.Litres}, {"gal", UnitOfVolume.Gallons}, {"gallons", UnitOfVolume.Gallons}
		};
	}

	/// <summary>Contains the known units</summary>
	public static Dictionary<string, UnitOfVolume> KnownUnits;
}
