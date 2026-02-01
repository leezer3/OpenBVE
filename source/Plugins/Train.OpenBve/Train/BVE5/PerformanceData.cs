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

using Formats.OpenBve;
using System;
using TrainManager.Motor;

namespace Train.OpenBve
{
	internal partial class VehicleTxtParser
	{
		
		internal Bve5PerformanceData ParsePerformanceData(Block<PerformanceCurveTxtSection, PerformanceCurveTxtKey> curveBlock, string directoryName)
		{
			if (!curveBlock.GetPath(PerformanceCurveTxtKey.Params, directoryName, out string powerParamsFile))
			{
				throw new Exception("BVE5: Performance curves parameters file missing.");
			}
			Bve5PerformanceTable ParamsTable = ParsePerformanceTable(powerParamsFile, 0.01, 2.01);

			if (!curveBlock.GetPath(PerformanceCurveTxtKey.Force, directoryName, out string powerForceFile))
			{
				throw new Exception("BVE5: Force curves parameters file missing.");
			}
			Bve5PerformanceTable ForceTable = ParsePerformanceTable(powerForceFile, 0.01, 2.01);

			if (!curveBlock.GetPath(PerformanceCurveTxtKey.MaxForce, directoryName, out string powerMaxForceFile))
			{
				throw new Exception("BVE5: Force curves parameters file missing.");
			}
			Bve5PerformanceTable MaxForceTable = ParsePerformanceTable(powerMaxForceFile, 0.01, 2.01);

			if (!curveBlock.GetPath(PerformanceCurveTxtKey.Current, directoryName, out string powerCurrentFile))
			{
				throw new Exception("BVE5: Current curves parameters file missing.");
			}
			Bve5PerformanceTable CurrentTable = ParsePerformanceTable(powerCurrentFile, 0.01, 2.01);

			if (!curveBlock.GetPath(PerformanceCurveTxtKey.MaxCurrent, directoryName, out string powerMaxCurrentFile))
			{
				throw new Exception("BVE5: Current curves parameters file missing.");
			}
			Bve5PerformanceTable MaxCurrentTable = ParsePerformanceTable(powerMaxCurrentFile, 0.01, 2.01);

			if (!curveBlock.GetPath(PerformanceCurveTxtKey.NoLoadCurrent, directoryName, out string noLoadCurrentFile))
			{
				throw new Exception("BVE5: Current curves parameters file missing.");
			}
			Bve5PerformanceTable NoLoadCurrentTable = ParsePerformanceTable(powerMaxCurrentFile, 0.01, 2.01);

			return new Bve5PerformanceData(ParamsTable, ForceTable, MaxForceTable, CurrentTable, MaxCurrentTable, NoLoadCurrentTable);
		}
	}
}
