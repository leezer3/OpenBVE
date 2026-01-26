//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;

namespace OpenBveApi.World
{
	/// <summary>Available units of Torque</summary>
	public enum UnitOfTorque
	{
		/// <summary>Newton-meters per second</summary>
		NewtonMetersPerSecond,
	/// <summary>Kilo-newton-meters per second</summary>
	KiloNewtonMetersPerSecond,

		/// <summary>Foot pounds</summary>
		FootPound
	}

	/// <summary>Implements the torque convertor</summary>
	public class TorqueConverter : UnitConverter<UnitOfTorque, double>
	{
		static TorqueConverter()
		{
			BaseUnit = UnitOfTorque.NewtonMetersPerSecond;
		RegisterConversion(UnitOfTorque.KiloNewtonMetersPerSecond, v => v / 1000, v => v * 1000);
		RegisterConversion(UnitOfTorque.FootPound, v => v * 0.7375621493, v => v / 1.3558179483);
		KnownUnits = new Dictionary<string, UnitOfTorque>
			{
				// n.b. assume that torque in plain newtons is actually newton meters
				{"n/m/s", UnitOfTorque.NewtonMetersPerSecond}, {"nms", UnitOfTorque.NewtonMetersPerSecond}, {"newtonmeterspersecond", UnitOfTorque.NewtonMetersPerSecond}, {"n", UnitOfTorque.NewtonMetersPerSecond}, {"knms", UnitOfTorque.KiloNewtonMetersPerSecond}, {"lb/ft", UnitOfTorque.FootPound}
			};
		}

		/// <summary>Contains the known units</summary>
		public static Dictionary<string, UnitOfTorque> KnownUnits;
	}
}
