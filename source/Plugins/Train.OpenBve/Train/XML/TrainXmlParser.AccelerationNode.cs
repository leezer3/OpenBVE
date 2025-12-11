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

using Formats.OpenBve;
using OpenBveApi.Interface;
using System.Collections.Generic;
using TrainManager.Power;

namespace Train.OpenBve
{
	partial class TrainXmlParser
	{
		private AccelerationCurve[] ParseAccelerationBlock(Block<TrainXMLSection, TrainXMLKey> block, string fileName)
		{
			List<AccelerationCurve> accelerationCurves = new List<AccelerationCurve>();
			while (block.RemainingSubBlocks > 0)
			{
				if (block.ReadBlock(TrainXMLSection.openBVE, out Block<TrainXMLSection, TrainXMLKey> curveBlock))
				{
					BveAccelerationCurve curve = new BveAccelerationCurve();
					curveBlock.GetValue(TrainXMLKey.StageZeroAcceleration, out curve.StageZeroAcceleration);
					curve.StageZeroAcceleration *= 0.277777777777778;
					curveBlock.GetValue(TrainXMLKey.StageOneAcceleration, out curve.StageOneAcceleration);
					curve.StageOneAcceleration *= 0.277777777777778;
					curveBlock.GetValue(TrainXMLKey.StageOneSpeed, out curve.StageOneSpeed);
					curve.StageOneSpeed *= 0.277777777777778;
					curveBlock.GetValue(TrainXMLKey.StageTwoSpeed, out curve.StageTwoSpeed);
					curve.StageTwoSpeed *= 0.277777777777778;
					curveBlock.GetValue(TrainXMLKey.StageTwoExponent, out curve.StageTwoExponent);
					block.GetValue(TrainXMLKey.Multiplier, out double multiplier);
					if (multiplier <= 0 || multiplier > 50)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Multiplier was invalid for curve " + accelerationCurves.Count + " in XML file " + fileName);
						multiplier = Plugin.AccelerationCurves[0].Multiplier;
					}
					curve.Multiplier = multiplier;
					accelerationCurves.Add(curve);

				}
				else
				{
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Unexpected extra block(s) in AccelerationCurves in XML file " + fileName);
					break;
				}
			}
			
			return accelerationCurves.ToArray();
		}
	}
}
