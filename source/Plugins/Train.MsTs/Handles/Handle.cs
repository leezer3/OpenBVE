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
using TrainManager.Handles;
using TrainManager.Trains;

namespace Train.MsTs
{
	internal partial class WagonParser
	{
		internal int HandleMinimum;

		internal int HandleMaximum;

		internal int HandleStep;

		internal int HandleStartingPosition;

		internal Tuple<double, string>[] NotchDescriptions;

		internal AbstractHandle ParseHandle(Block block, TrainBase train, bool isPower)
		{
			HandleMinimum = (int)(block.ReadSingle() * 100);
			HandleMaximum = (int)(block.ReadSingle() * 100);
			HandleStep = (int)(block.ReadSingle() * 100);
			HandleStartingPosition = (int)(block.ReadSingle() * 100);
			// some handles seem to have extra numbers here, I believe ignored by the MSTS parser
			Block notchDescriptions = block.GetSubBlock(KujuTokenID.NumNotches);
			ParseNotchDescriptionBlock(notchDescriptions, isPower);
			return new VariableHandle(train, isPower, NotchDescriptions);
		}
		
		private void ParseNotchDescriptionBlock(Block block, bool isPower)
		{
			int numNotches = block.ReadInt32();
			if (numNotches < 2)
			{
				// percentage based notch
				NotchDescriptions = null;
				return;
			}

			NotchDescriptions = new Tuple<double, string>[numNotches];
			for (int i = 0; i < numNotches; i++)
			{
				Block descriptionBlock = block.ReadSubBlock(KujuTokenID.Notch);
				double notchPower = descriptionBlock.ReadSingle();
				try
				{
					descriptionBlock.ReadSingle(); // ??
					string notchDescription = descriptionBlock.ReadString();
					if (notchDescription.Equals("dummy", StringComparison.InvariantCultureIgnoreCase))
					{
						if (i == 0)
						{
							notchDescription = "N";
						}
						else
						{
							notchDescription = isPower ? "P" + i : "B" + i;
						}

					}
					NotchDescriptions[i] = new Tuple<double, string>(notchPower, notchDescription);
				}
				catch
				{
					// ignore
					NotchDescriptions[i] = new Tuple<double, string>(notchPower, i.ToString());
				}
			}
		}
	}
}
