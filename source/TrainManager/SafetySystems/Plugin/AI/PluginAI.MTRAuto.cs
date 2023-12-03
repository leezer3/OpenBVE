//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2023, Christopher Lees, The OpenBVE Project
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


using OpenBveApi.Runtime;
using OpenBveApi.Trains;

namespace TrainManager.SafetySystems
{
	/// <summary>An AI to control the TASC plugin used by some MTR trains for automatic operation</summary>
	class MTRAutoAI : PluginAI
	{
		internal MTRAutoAI(Plugin plugin)
		{
			Plugin = plugin;
			currentStep = 0;
		}

		internal override void Perform(AIData data)
		{
			
			switch (currentStep)
			{
				case 0:
					// start in N + EB
					data.Handles.Reverser = 0;
					data.Handles.BrakeNotch = Plugin.Train.Handles.Brake.MaximumNotch + 1;
					data.Response = AIResponse.Medium;
					currentStep++;
					break;
				case 1:
					// enable system
					data.Handles.Reverser = 0;
					data.Handles.BrakeNotch = Plugin.Train.Handles.Brake.MaximumNotch + 1;
					Plugin.KeyDown(VirtualKeys.J);
					data.Response = AIResponse.Short;
					currentStep++;
					break;
				case 2:
					data.Handles.Reverser = 0;
					data.Handles.BrakeNotch = Plugin.Train.Handles.Brake.MaximumNotch + 1;
					Plugin.KeyUp(VirtualKeys.J);
					data.Response = AIResponse.Short;
					currentStep++;
					break;
				case 3:
					// set train to drive mode
					data.Handles.Reverser = 1;
					data.Handles.PowerNotch = 0;
					data.Handles.BrakeNotch = 0;
					data.Response = AIResponse.Medium;
					currentStep++;
					break;
				case 4:
					data.Handles.PowerNotch = 0;
					data.Handles.BrakeNotch = 0;
					if (Plugin.Train.StationState == TrainStopState.Completed)
					{
						Plugin.KeyDown(VirtualKeys.A1);
						Plugin.KeyDown(VirtualKeys.A2);
						currentStep++;
					}
					break;
				case 5:
					data.Handles.PowerNotch = 0;
					data.Handles.BrakeNotch = 0;
					if (Plugin.Train.StationState != TrainStopState.Pending)
					{
						Plugin.KeyUp(VirtualKeys.A1);
						Plugin.KeyUp(VirtualKeys.A2);
						currentStep--;
					}
					break;
			}
			

		}

		public override void BeginJump(InitializationModes mode)
		{
		}

		public override void EndJump()
		{
			currentStep = 0;
		}

		public override void SetBeacon(BeaconData beacon)
		{
		}
	}
}
