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
