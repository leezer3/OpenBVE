using OpenBve.RouteManager;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Is called when the in-game fog should be changed</summary>
		internal class FogChangeEvent : GeneralEvent
		{
			/// <summary>The fog which applies previously to this point</summary>
			private readonly Fog PreviousFog;
			/// <summary>The fog which applies after this point</summary>
			private readonly Fog CurrentFog;
			/// <summary>The next upcoming fog (Used for distance based interpolation)</summary>
			internal Fog NextFog;

			internal FogChangeEvent(double TrackPositionDelta, Fog PreviousFog, Fog CurrentFog, Fog NextFog)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.PreviousFog = PreviousFog;
				this.CurrentFog = CurrentFog;
				this.NextFog = NextFog;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (TriggerType == EventTriggerType.Camera)
				{
					if (Direction < 0)
					{
						CurrentRoute.PreviousFog = this.PreviousFog;
						CurrentRoute.NextFog = this.CurrentFog;
					}
					else if (Direction > 0)
					{
						CurrentRoute.PreviousFog = this.CurrentFog;
						CurrentRoute.NextFog = this.NextFog;
					}
				}
			}
		}
	}
}
