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
using System;
using TrainManager.Handles;
using TrainManager.Trains;

namespace Train.OpenBve
{
	partial class TrainXmlParser
	{
		private void ParseHandleNode(Block<TrainXMLSection, TrainXMLKey> block, ref AbstractHandle Handle, int Car, TrainBase Train, string fileName)
		{
			if (Car != Train.DriverCar)
			{
				// only valid on driver car at the minute, need to implement car based handles
				return;
			}

			if (block.GetValue(TrainXMLKey.Notches, out int numberOfNotches))
			{
				if (Handle is AirBrakeHandle)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Unable to define a number of notches for an AirBrake handle for Car " + Car + " in XML file " + fileName);
				}
				else
				{
					// remember to increase the max driver notch too
					Handle.MaximumDriverNotch += numberOfNotches - Handle.MaximumNotch;
					Handle.MaximumNotch = numberOfNotches;
				}
			}

			block.GetEnumValue(TrainXMLKey.SpringType, out Handle.SpringType);
			if (!block.GetValue(TrainXMLKey.SpringTime, out Handle.SpringTime, NumberRange.Positive))
			{
				Handle.SpringType = SpringType.Unsprung;
			}

			if (block.GetValue(TrainXMLKey.MaxSprungNotch, out int maxSpring, NumberRange.Positive))
			{
				if (maxSpring > Handle.MaximumNotch)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Invalid maximum handle spring value defined for Car " + Car + " in XML file " + fileName);
				}

				Handle.MaxSpring = Math.Min(maxSpring, Handle.MaximumNotch);
			}

			block.TryGetValue(TrainXMLKey.MotorBrakeNotch, ref Train.Cars[Car].CarBrake.MotorBrakeNotch, NumberRange.NonNegative);
		}
	}
}
