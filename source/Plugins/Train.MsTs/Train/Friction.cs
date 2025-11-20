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

using System;
using OpenBve.Formats.MsTs;
using OpenBveApi.Interface;
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

		internal Friction()
		{
			C1 = 100;
			E1 = 1;
			V2 = -1;
			C2 = 0;
			E2 = 1;
		}

		internal Friction(Block block)
		{
			try
			{
				C1 = block.ReadSingle(UnitOfTorque.NewtonMetersPerSecond);
				E1 = block.ReadSingle();
				V2 = block.ReadSingle(UnitOfVelocity.MetersPerSecond);
				C2 = block.ReadSingle(UnitOfTorque.NewtonMetersPerSecond);
				E2 = block.ReadSingle();

				if (E1 <= 0 || E2 <= 0 || V2 < 0)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: The Friction properties contain nonsensical data.");
					C1 = 100;
					E1 = 1;
					V2 = -1;
					C2 = 0;
					E2 = 1;
				}
			}
			catch
			{
				Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "MSTS Vehicle Parser: The Friction properties contain invalid data.");
				C1 = 100;
				E1 = 1;
				V2 = -1;
				C2 = 0;
				E2 = 1;
			}
			
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
