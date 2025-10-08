using OpenBveApi.World;

using System.Collections.Generic;

/// <summary>Available units of Torque</summary>
public enum UnitOfTorque
{
	/// <summary>Newton-meters per second</summary>
	NewtonMetersPerSecond,
	/// <summary>Foot pounds</summary>
	FootPound
}

/// <summary>Implements the torque convertor</summary>
public class TorqueConverter : UnitConverter<UnitOfTorque, double>
{
	static TorqueConverter()
	{
		BaseUnit = UnitOfTorque.NewtonMetersPerSecond;
		RegisterConversion(UnitOfTorque.FootPound, v => v * 0.7375621493, v => v / 1.3558179483);
		KnownUnits = new Dictionary<string, UnitOfTorque>
			{
				// n.b. assume that torque in plain newtons is actually newton meters
				{"n/m/s", UnitOfTorque.NewtonMetersPerSecond}, {"nms", UnitOfTorque.NewtonMetersPerSecond}, {"newtonmeterspersecond", UnitOfTorque.NewtonMetersPerSecond}, {"n", UnitOfTorque.NewtonMetersPerSecond}, {"lb/ft", UnitOfTorque.FootPound}
			};
	}

	/// <summary>Contains the known units</summary>
	public static Dictionary<string, UnitOfTorque> KnownUnits;
}
