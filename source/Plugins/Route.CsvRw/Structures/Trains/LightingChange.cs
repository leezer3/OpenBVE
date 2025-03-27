using System.Collections.Generic;
using OpenBveApi.Routes;
using RouteManager2.Events;

namespace CsvRwRouteParser
{
	internal class LightingChange
	{
		private readonly LightDefinition previousLightDefinition;

		private readonly LightDefinition currentLightDefinition;

		private readonly int currentDynamicLightSet;

		private readonly int previousDynamicLightSet;

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
				Element.Events.Add(new LightingChangeEvent(Plugin.CurrentRoute, LightDefinitions[previousDynamicLightSet], LightDefinitions[currentDynamicLightSet]));
			}
			else
			{
				Element.Events.Add(new LightingChangeEvent(Plugin.CurrentRoute, previousLightDefinition, currentLightDefinition));
			}
		}

	}
}

