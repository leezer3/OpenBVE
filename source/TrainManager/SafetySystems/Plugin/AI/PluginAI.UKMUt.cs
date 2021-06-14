using OpenBveApi.Runtime;
using TrainManager.Car;

namespace TrainManager.SafetySystems
{
	/// <summary>An AI to control the legacy Win32 UKMUt plugin</summary>
	internal class UKMUtAI : PluginAI
	{
		/// <summary>Variable controlling whether the door startup hack has been performed</summary>
		private bool doorStart;

		internal UKMUtAI(Plugin plugin)
		{
			Plugin = plugin;
			currentStep = 0;
			nextPluginAction = 0;
		}

		internal override void Perform(AIData data)
		{
			if (Plugin.Panel[31] == 0)
			{
				//Panto is down, so run startup sequence
				switch (currentStep)
				{
					case 0:
						data.Handles.Reverser = 1;
						data.Response = AIResponse.Long;
						currentStep++;
						return;
					case 1:
						data.Handles.Reverser = 0;
						data.Response = AIResponse.Long;
						currentStep++;
						return;
					case 2:
						if (Plugin.Sound[2] == 0)
						{
							data.Response = AIResponse.Medium;
							currentStep++;
						}
						return;
					case 3:
						Plugin.KeyDown(VirtualKeys.A1);
						data.Response = AIResponse.Medium;
						currentStep++;
						return;
					case 4:
						Plugin.KeyUp(VirtualKeys.A1);
						data.Response = AIResponse.Medium;
						currentStep++;
						return;
					case 5:
						Plugin.KeyDown(VirtualKeys.D);
						data.Response = AIResponse.Short;
						currentStep++;
						return;
					case 6:
						currentStep++;
						return;
					case 7:
						Plugin.KeyUp(VirtualKeys.D);
						data.Response = AIResponse.Short;
						currentStep = 100;
						return;
				}
			}

			if (!doorStart)
			{
				if (Plugin.Train.GetDoorsState(true, true) == (TrainDoorState.Closed | TrainDoorState.AllClosed) && Plugin.Train.CurrentSpeed == 0)
				{
					/*
					 * HACK: Work around the fact that the guard AI isn't designed to start somewhere with no open doors
					 */
					data.Response = AIResponse.Medium;
					Plugin.DoorChange(DoorStates.None, DoorStates.Left);
					Plugin.DoorChange(DoorStates.Left, DoorStates.None);
					doorStart = true;
					return;
				}
			}

			if (Plugin.Panel[13] == 1)
			{
				//DRA is enabled, so toggle off
				Plugin.KeyDown(VirtualKeys.S);
				data.Response = AIResponse.Medium;
				currentStep = 99;
				return;
			}

			if (currentStep == 99)
			{
				Plugin.KeyUp(VirtualKeys.S);
				data.Response = AIResponse.Medium;
				currentStep++;
				return;
			}

			if (Plugin.Sound[2] == 0)
			{
				//AWS horn active, so wait a sec before triggering cancel
				switch (currentStep)
				{
					case 100:
						data.Response = AIResponse.Medium;
						currentStep++;
						return;
					case 101:
						Plugin.KeyDown(VirtualKeys.A1);
						data.Response = AIResponse.Medium;
						currentStep++;
						return;
				}
			}

			if (currentStep == 101)
			{
				//Raise the AWS horn cancel key
				Plugin.KeyUp(VirtualKeys.A1);
				data.Response = AIResponse.Medium;
				currentStep = 100;
				return;
			}

			/*
			 * Assume that with a brightness value below 180 we want night headlights
			 * Further assume that the driver only sets these at the initial station once
			 */
			if (!lightsSet)
			{
				float currentBrightness = Plugin.Train.Cars[Plugin.Train.DriverCar].Brightness.CurrentBrightness(TrainManagerBase.Renderer.Lighting.DynamicCabBrightness);
				switch (Plugin.Panel[20])
				{
					case 0:
						//off
						Plugin.KeyDown(VirtualKeys.G);
						Plugin.KeyUp(VirtualKeys.G);
						data.Response = AIResponse.Medium;
						return;
					case 1:
						//day
						if (currentBrightness < 180)
						{
							Plugin.KeyDown(VirtualKeys.G);
							Plugin.KeyUp(VirtualKeys.G);
							data.Response = AIResponse.Medium;
						}
						else
						{
							lightsSet = true;
						}
						return;
					case 2:
						//marker
						Plugin.KeyDown(VirtualKeys.G);
						Plugin.KeyUp(VirtualKeys.G);
						data.Response = AIResponse.Medium;
						return;
					case 3:
						//night
						if (currentBrightness > 180)
						{
							Plugin.KeyDown(VirtualKeys.G);
							Plugin.KeyUp(VirtualKeys.G);
							data.Response = AIResponse.Medium;
						}
						else
						{
							lightsSet = true;
						}
						return;
				}
			}
			//Set tail lights
			if (Plugin.Panel[21] == 0)
			{
				Plugin.KeyDown(VirtualKeys.F);
				Plugin.KeyUp(VirtualKeys.F);
				data.Response = AIResponse.Medium;
				return;
			}

			if (TrainManagerBase.currentHost.InGameTime > nextPluginAction)
			{
				//If nothing else has happened recently, hit the vigilance reset key
				Plugin.KeyDown(VirtualKeys.A2);
				Plugin.KeyUp(VirtualKeys.A2);
				data.Response = AIResponse.Short;
				nextPluginAction = TrainManagerBase.currentHost.InGameTime + 20.0;
				return;
			}

			//Count number of shown raindrops
			int numShownRaindrops = 0;
			for (int i = 200; i < 250; i++)
			{
				if (Plugin.Panel[i] == 1)
				{
					numShownRaindrops++;
				}
			}
			//Greater than 10 drops, always clear the screen
			bool shouldWipe = numShownRaindrops > 10;

			switch (Plugin.Panel[198])
			{
				case 0:
					if (currentRainIntensity > 30 || shouldWipe)
					{
						Plugin.KeyDown(VirtualKeys.B1);
						Plugin.KeyUp(VirtualKeys.B1);
						data.Response = AIResponse.Short;
					}
					return;
				case 1:
					if (currentRainIntensity > 45)
					{
						Plugin.KeyDown(VirtualKeys.B1);
						Plugin.KeyUp(VirtualKeys.B1);
						data.Response = AIResponse.Short;
					}
					else
					{
						Plugin.KeyDown(VirtualKeys.B2);
						Plugin.KeyUp(VirtualKeys.B2);
						data.Response = AIResponse.Short;
					}
					return;
				case 2:
					if (currentRainIntensity < 60)
					{
						Plugin.KeyDown(VirtualKeys.B2);
						Plugin.KeyUp(VirtualKeys.B2);
						data.Response = AIResponse.Short;
					}
					return;
			}

		}

		public override void BeginJump(InitializationModes mode)
		{
			if (mode == InitializationModes.OffEmergency)
			{
				currentStep = 0;
			}
			else
			{
				currentStep = 100;
			}
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
