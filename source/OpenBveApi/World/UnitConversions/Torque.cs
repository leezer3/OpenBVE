using OpenBveApi.World;

using System.Collections.Generic;

/// <summary>Available units of Torque</summary>
public enum UnitOfTorque
{
	/// <summary>Newton-meters per second</summary>
	NewtonMetersPerSecond,
}

/// <summary>Implements the torque convertor</summary>
public class TorqueConverter : UnitConverter<UnitOfTorque, double>
{
	static TorqueConverter()
	{
		BaseUnit = UnitOfTorque.NewtonMetersPerSecond;
		KnownUnits = new Dictionary<string, UnitOfTorque>
			{
				// n.b. assume that torque in plain newtons is actually newton meters
				{"n/m/s", UnitOfTorque.NewtonMetersPerSecond}, {"nms", UnitOfTorque.NewtonMetersPerSecond}, {"newtonmeterspersecond", UnitOfTorque.NewtonMetersPerSecond}, {"n", UnitOfTorque.NewtonMetersPerSecond},
			};
	}

	/// <summary>Contains the known units</summary>
	public static Dictionary<string, UnitOfTorque> KnownUnits;
}
