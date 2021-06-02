using OpenBveApi.Runtime;

namespace TrainManager.SafetySystems
{
	internal static class PluginAI
	{
		private static int currentStep;
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
						if (!overCurrentTrip)
						{
							//over current trip
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
