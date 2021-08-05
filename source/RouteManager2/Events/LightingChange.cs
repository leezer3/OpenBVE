using OpenBveApi.Routes;

namespace RouteManager2.Events
{
	/// <summary>Called when the light definition should be changed</summary>
	public class LightingChangeEvent : GeneralEvent
	{
		private readonly CurrentRoute currentRoute;
		/// <summary>The previous light definition if single</summary>
		private readonly LightDefinition previousLightDefinition;
		/// <summary>The next light definition if single</summary>
		private readonly LightDefinition nextLightDefinition;
		/// <summary>The previous light definition if dynamic</summary>
		private readonly LightDefinition[] previousLightDefinitions;
		/// <summary>The next light definition if dynamic</summary>
		private readonly LightDefinition[] nextLightDefinitions;
		
		public LightingChangeEvent(CurrentRoute CurrentRoute, object PreviousLightDefinition, object NextLightDefinition)
		{
			currentRoute = CurrentRoute;
			if (PreviousLightDefinition is LightDefinition)
			{
				previousLightDefinition = (LightDefinition)PreviousLightDefinition;
			}
			else
			{
				previousLightDefinitions = (LightDefinition[]) PreviousLightDefinition;
			}
			if (NextLightDefinition is LightDefinition)
			{
				nextLightDefinition = (LightDefinition)NextLightDefinition;
			}
			else
			{
				nextLightDefinitions = (LightDefinition[]) NextLightDefinition;
			}
		}

		
		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			if (trackFollower.TriggerType == EventTriggerType.Camera)
			{
				if (direction < 0)
				{
					if (previousLightDefinitions != null)
					{
						currentRoute.LightDefinitions = previousLightDefinitions;
						currentRoute.DynamicLighting = true;
					}
					else
					{
						currentRoute.Atmosphere.AmbientLightColor = previousLightDefinition.AmbientColor;
						currentRoute.Atmosphere.DiffuseLightColor = previousLightDefinition.DiffuseColor;
						currentRoute.Atmosphere.LightPosition = previousLightDefinition.LightPosition;
						currentRoute.DynamicLighting = false;
					}
				}
				else if (direction > 0)
				{
					if (nextLightDefinitions != null)
					{
						currentRoute.LightDefinitions = nextLightDefinitions;
						currentRoute.DynamicLighting = true;
					}
					else
					{
						currentRoute.Atmosphere.AmbientLightColor = nextLightDefinition.AmbientColor;
						currentRoute.Atmosphere.DiffuseLightColor = nextLightDefinition.DiffuseColor;
						currentRoute.Atmosphere.LightPosition = nextLightDefinition.LightPosition;
						currentRoute.DynamicLighting = false;
					}
					
				}
			}
		}
	}
}
