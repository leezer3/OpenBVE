using System;
using System.Collections.Generic;
using OpenBveApi.Routes;
using RouteManager2.Events;

namespace CsvRwRouteParser
{
	internal class LightingChange
	{
		internal LightDefinition previousLightDefinition;

		internal LightDefinition currentLightDefinition;

		internal int currentDynamicLightSet;

		internal int previousDynamicLightSet;

		internal LightingChange(LightDefinition PreviousLightDefinition, LightDefinition CurrentLightDefinition)
		{
			previousLightDefinition = PreviousLightDefinition;
			currentLightDefinition = CurrentLightDefinition;
			currentDynamicLightSet = -1;
		}

		internal LightingChange(int PreviousDynamicLightSet, int CurrentDynamicLightSet)
		{
			previousDynamicLightSet = PreviousDynamicLightSet;
			currentDynamicLightSet = CurrentDynamicLightSet;
			
		}

		internal void Create(ref TrackElement Element, Dictionary<int, LightDefinition[]> LightDefinitions)
		{
			if (Plugin.CurrentRoute.DynamicLighting)
			{
				if (currentDynamicLightSet == -1)
				{
					// Setting ambient / directional not supported in conjunction with dynamic lighting
					return;
				}

				if (!LightDefinitions.ContainsKey(previousDynamicLightSet) || !LightDefinitions.ContainsKey(currentDynamicLightSet))
				{
					// Light set not available
					return;
				}
				int m = Element.Events.Length;
				Array.Resize(ref Element.Events, m + 1);
				Element.Events[m] = new LightingChangeEvent(Plugin.CurrentRoute, LightDefinitions[previousDynamicLightSet], LightDefinitions[currentDynamicLightSet]);
			}
			else
			{
				int m = Element.Events.Length;
				Array.Resize(ref Element.Events, m + 1);
				Element.Events[m] = new LightingChangeEvent(Plugin.CurrentRoute, previousLightDefinition, currentLightDefinition);
			}
		}

	}
}

