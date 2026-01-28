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

using OpenBveApi.Math;

namespace TrainManager.Motor
{
    public class Bve5PerformanceData
    {
		/// <summary>The controlling parameters</summary>
	    public readonly Bve5PerformanceTable ParamsTable;
		/// <summary>The acceleration force, when empty</summary>
	    public readonly Bve5PerformanceTable ForceTable;
		/// <summary>The acceleration force, when fully loaded</summary>
	    public readonly Bve5PerformanceTable MaxForceTable;
		/// <summary>The ammeter current, when empty</summary>
	    public readonly Bve5PerformanceTable CurrentTable;
		/// <summary>The ammeter current when fully loaded</summary>
	    public readonly Bve5PerformanceTable MaxCurrentTable;
		/// <summary>The ammeter current when the force is zero</summary>
	    public readonly Bve5PerformanceTable NoLoadCurrentTable;

	    public Bve5PerformanceData(Bve5PerformanceTable paramsTable, Bve5PerformanceTable forceTable, Bve5PerformanceTable maxForceTable, Bve5PerformanceTable currentTable, Bve5PerformanceTable maxCurrentTable, Bve5PerformanceTable noLoadCurrentTable)
	    {
			ParamsTable = paramsTable;
			ForceTable = forceTable;
			MaxForceTable = maxForceTable;
			CurrentTable = currentTable;
			MaxCurrentTable = maxCurrentTable;
			NoLoadCurrentTable = noLoadCurrentTable;
	    }

	    public double GetForce(double speed, int notch, double loadingRatio)
	    {
		    double noLoad = ForceTable.GetValue(speed, notch);
		    double maxLoad = MaxForceTable.GetValue(speed, notch);
		    return Extensions.LinearInterpolation(noLoad, 0, maxLoad, 1, loadingRatio);
		}

	    public double GetCurrent(double speed, int notch, double loadingRatio)
	    {
		    double newtons = GetForce(speed, notch, loadingRatio);
		    if (newtons == 0)
		    {
			    return NoLoadCurrentTable.GetValue(speed, notch);
		    }
			double noLoad = CurrentTable.GetValue(speed, notch);
		    double maxLoad = MaxCurrentTable.GetValue(speed, notch);
		    return Extensions.LinearInterpolation(noLoad, 0, maxLoad, 1, loadingRatio);
		}
	}
}
