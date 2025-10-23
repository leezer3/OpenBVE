using System;
using OpenBve.Formats.MsTs;
using OpenBveApi.World;

namespace Train.MsTs
{
	internal class Friction
	{
		/// <summary>The friction constant</summary>
		internal double C1;
		/// <summary>The friction exponent</summary>
		internal double E1;
		/// <summary>The second velocity segment start value</summary>
		internal double V2;
		/// <summary>The second friction constant</summary>
		internal double C2;
		/// <summary>The second friction exponent</summary>
		internal double E2;

		internal Friction(Block block)
		{
			C1 = block.ReadSingle(UnitOfTorque.NewtonMetersPerSecond);
			E1 = block.ReadSingle();
			V2 = block.ReadSingle(UnitOfVelocity.MetersPerSecond);
			C2 = block.ReadSingle(UnitOfTorque.NewtonMetersPerSecond);
			E2 = block.ReadSingle();
		}

		internal double GetResistanceValue(double speed)
		{
			// see Eng_and_wag_file_reference_guideV2.doc
			// Gives the result in newton-meters per second
			if (V2 < 0 || speed <= V2)
			{
				return C1 * Math.Pow(speed, E1);
			}

			return C1 + Math.Pow(V2, E1) + C2 * (V2 + Math.Pow(speed - V2, E2));
		}
	}
}
