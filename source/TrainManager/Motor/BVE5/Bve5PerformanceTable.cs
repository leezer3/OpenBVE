//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Christopher Lees, The OpenBVE Project
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
using OpenBveApi.Math;

namespace TrainManager.Motor
{
	/// <summary>Describes a BVE5 Performance Table</summary>
	public class Bve5PerformanceTable
	{
		/// <summary>The performance curve entries</summary>
		public readonly Tuple<double, double[]>[] CurveEntries;
		/// <summary>The table version</summary>
		public readonly double Version;

		public Bve5PerformanceTable(Tuple<double, double[]>[] curveEntries, double version)
		{
			CurveEntries = curveEntries;
			Version = version;
		}

		public double GetValue(double speed, double notch)
		{
			if (CurveEntries.Length == 0)
			{
				return 0;
			}
			int entry = 0;
			// if between two entries, value is linearly interpolated
			for (int i = 0; i < CurveEntries.Length - 1; i++)
			{
				if (CurveEntries[i].Item1 >= speed && CurveEntries[i + 1].Item1 <= speed)
				{
					entry = i;
					break;
				}
			}

			// check if we have enough notches, if not return the last
			int notch1 = (int)Math.Min(notch, CurveEntries[entry].Item2.Length - 1);
			if (CurveEntries.Length == 1)
			{
				return Extensions.LinearInterpolation(0, 0, CurveEntries[0].Item1, CurveEntries[0].Item2[notch1], speed);
			}
			// no guarantee that notch lengths will match per entry either
			int notch2 = (int)Math.Min(notch, CurveEntries[entry + 1].Item2.Length - 1);

			return Extensions.LinearInterpolation(CurveEntries[entry].Item1, CurveEntries[entry].Item2[notch1], CurveEntries[entry + 1].Item1, CurveEntries[entry + 1].Item2[notch2], speed);

		}
	}
}
