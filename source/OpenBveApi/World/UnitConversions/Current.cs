using System.Collections.Generic;
using OpenBveApi.World;

/// <summary>Available units of current</summary>
public enum UnitOfCurrent
{
	/// <summary>Amps</summary>
	Amps,
}

/// <summary>Implements the current convertor</summary>
public class CurrentConverter : UnitConverter<UnitOfCurrent, double>
{
	static CurrentConverter()
	{
		BaseUnit = UnitOfCurrent.Amps;
		
		KnownUnits = new Dictionary<string, UnitOfCurrent>
		{
			{"a", UnitOfCurrent.Amps}, {"amps", UnitOfCurrent.Amps}
		};
	}

	/// <summary>Contains the known units</summary>
	public static Dictionary<string, UnitOfCurrent> KnownUnits;
}
