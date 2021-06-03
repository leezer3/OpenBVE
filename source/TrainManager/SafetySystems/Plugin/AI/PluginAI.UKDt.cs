using OpenBveApi.Runtime;

namespace TrainManager.SafetySystems
{
	/// <summary>An AI to control the legacy Win32 UKDt plugin</summary>
	internal class UKDTAI : PluginAI
	{
		/// <summary>Control variable used to determine next AI step</summary>
		private int currentStep;
		/// <summary>Whether the overcurrent trip has occurred</summary>
		private bool overCurrentTrip;
		/// <summary>The speed at which the overcurrent trip occurred</summary>
		private double overCurrentSpeed;
		/// <summary>The notch at which the overcurrent trip occurred</summary>
		private int overCurrentNotch;

		private double nextPluginAction;

		internal UKDTAI(Plugin plugin)
		{
			Plugin = plugin;
			currentStep = 0;
			nextPluginAction = 0;
		}

		internal override void Perform(AIData data)
		{
			if (Plugin.Panel[5] == 2)
			{
				//Engine is currently not running, so run startup sequence
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
						data.Response = AIResponse.Long;
						currentStep++;
						return;
					case 6:
						currentStep++;
						return;
					case 7:
						Plugin.KeyUp(VirtualKeys.D);
						data.Response = AIResponse.Medium;
						currentStep = 100;
						return;
				}
			}
			else if (Plugin.Panel[5] == 3)
			{
				//Engine either stalled on start, or has been stopped by user
				currentStep = 5;
			}

			if (Plugin.Panel[51] == 1)
			{
				/*
				 * Over current has tripped
				 * Let's back off to N and drop the max notch by 1
				 *
				 * Repeat until we move off properly
				 * NOTE: UKDT does have an ammeter, but we'll cheat this way, to
				 * avoid having to configure the max on a per-train basis
				 */
				if (!overCurrentTrip)
				{
					overCurrentSpeed = Plugin.Train.CurrentSpeed;
					overCurrentNotch = data.Handles.PowerNotch - 1;
					data.Handles.PowerNotch = 0;
					data.Response = AIResponse.Long;
					overCurrentTrip = true;
					return;
				}

				data.Response = AIResponse.Long;
				return;
			}

			overCurrentTrip = false;
			if (overCurrentSpeed != double.MaxValue)
			{
				if (Plugin.Train.CurrentSpeed < overCurrentSpeed + 10)
				{
					data.Handles.PowerNotch = overCurrentNotch;
				}
				else
				{
					overCurrentSpeed = double.MaxValue;
				}
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

			if (TrainManagerBase.currentHost.InGameTime > nextPluginAction)
			{
				//If nothing else has happened recently, hit the vigilance reset key
				Plugin.KeyDown(VirtualKeys.A2);
				Plugin.KeyUp(VirtualKeys.A2);
				data.Response = AIResponse.Short;
				nextPluginAction = TrainManagerBase.currentHost.InGameTime + 20.0;
				return;
			}

		}
	}
}
