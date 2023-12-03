//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2022, Christopher Lees, The OpenBVE Project
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

namespace TrainManager.SafetySystems
{
	/// <summary>An AI to control the Hei_ats.dll plugin found in Hirakami railway trains</summary>
	class HeiAtsAI : PluginAI
	{
		internal HeiAtsAI(Plugin plugin)
		{
			Plugin = plugin;
			currentStep = 2;
			nextPluginAction = 0;
		}
		/// <summary>Used to store the current state of the wipers</summary>
		private int wiperState;
		internal override void Perform(AIData data)
		{
			if (Plugin.Panel[17] == 0)
			{
				currentStep = 0;
			}

			switch (currentStep)
			{
				case 0:
					Plugin.KeyDown(VirtualKeys.H);
					data.Response = AIResponse.Short;
					currentStep++;
					break;
				case 1:
					Plugin.KeyUp(VirtualKeys.H);
					data.Response = AIResponse.Long;
					currentStep++;
					break;
				case 2:
					Plugin.KeyDown(VirtualKeys.S);
					data.Response = AIResponse.Short;
					currentStep++;
					break;
				case 3:
					Plugin.KeyUp(VirtualKeys.S);
					data.Response = AIResponse.Long;
					currentStep++;
					break;
				case 4:
					if (Plugin.Train.Cars[Plugin.Train.DriverCar].Sounds.Plugin[0].IsPlaying && Plugin.Train.CurrentSpeed == 0)
					{
						data.Response = AIResponse.Long;
						currentStep++;
					}
					break;
				case 5:
					data.Response = AIResponse.Long;
					currentStep++;
					break;
				case 6:
					Plugin.KeyDown(VirtualKeys.S);
					data.Response = AIResponse.Short;
					currentStep++;
					break;
				case 7:
					Plugin.KeyUp(VirtualKeys.S);
					data.Response = AIResponse.Short;
					currentStep = 4;
					break;
				case 100:
				case 101:
					//Decent pause before resetting the safety system
					data.Response = AIResponse.Long;
					currentStep++;
					break;
				case 102:
					Plugin.KeyDown(VirtualKeys.S);
					data.Response = AIResponse.Short;
					currentStep++;
					break;
				case 103:
					Plugin.KeyUp(VirtualKeys.S);
					data.Response = AIResponse.Short;
					currentStep = 4;
					break;
			}
			
			//This doesn't actually have raindrops, but let's at least play with the wipers if a rain beacon is encountered
			switch (wiperState)
			{
				case 0:
					if (currentRainIntensity > 30)
					{
						Plugin.KeyDown(VirtualKeys.D);
						Plugin.KeyUp(VirtualKeys.D);
						data.Response = AIResponse.Short;
						wiperState++;
					}
					return;
				case 1:
					if (currentRainIntensity > 60)
					{
						Plugin.KeyDown(VirtualKeys.D);
						Plugin.KeyUp(VirtualKeys.D);
						data.Response = AIResponse.Short;
						wiperState++;
					}
					else if(currentRainIntensity < 30)
					{
						Plugin.KeyDown(VirtualKeys.E);
						Plugin.KeyUp(VirtualKeys.E);
						data.Response = AIResponse.Short;
						wiperState--;
					}
					return;
				case 2:
					if (currentRainIntensity < 60)
					{
						Plugin.KeyDown(VirtualKeys.E);
						Plugin.KeyUp(VirtualKeys.E);
						data.Response = AIResponse.Short;
						wiperState--;
					}
					return;
			}
		}

		public override void BeginJump(InitializationModes mode)
		{
			currentStep = 100;
		}

		public override void EndJump()
		{
		}

		public override void SetBeacon(BeaconData beacon)
		{
			if (beacon.Type == 21)
			{
				currentRainIntensity = beacon.Optional;
			}
		}
	}
}
