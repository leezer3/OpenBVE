using OpenBveApi.Runtime;

namespace TrainManager.SafetySystems
{
	internal static class PluginAI
	{
		/// <summary>Control variable used to determine next AI step</summary>
		private static int currentStep;
		/*
		 * Variables used by AI for UKDT plugin
		 */
		private static bool overCurrentTrip;
		private static double overCurrentSpeed;
		private static int overCurrentNotch;

		internal static void Perform(Plugin Plugin, AIData data)
		{
			switch (Plugin.PluginTitle.ToLowerInvariant())
			{
				case "ukdt.dll":
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
								break;
							case 101:
								Plugin.KeyDown(VirtualKeys.A1);
								data.Response = AIResponse.Medium;
								currentStep++;
								break;
						}
					}

					if (currentStep == 101)
					{
						Plugin.KeyUp(VirtualKeys.A1);
						data.Response = AIResponse.Medium;
						currentStep = 100;
					}
					break;
			}
		}
	}
}
