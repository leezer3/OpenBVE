using OpenBveApi.Routes;
using OpenBveApi.Trains;
using RouteManager2.Climate;

namespace RouteManager2.Events
{
	/// <summary>Is called when the in-game fog should be changed</summary>
	public class FogChangeEvent : GeneralEvent
	{
		private readonly CurrentRoute currentRoute;

		/// <summary>The fog which applies previously to this point</summary>
		private readonly Fog PreviousFog;

		/// <summary>The fog which applies after this point</summary>
		private readonly Fog CurrentFog;

		/// <summary>The next upcoming fog (Used for distance based interpolation)</summary>
		public Fog NextFog;

		public FogChangeEvent(CurrentRoute CurrentRoute, double TrackPositionDelta, Fog PreviousFog, Fog CurrentFog, Fog NextFog)
		{
			currentRoute = CurrentRoute;

			this.TrackPositionDelta = TrackPositionDelta;
			DontTriggerAnymore = false;
			this.PreviousFog = PreviousFog;
			this.CurrentFog = CurrentFog;
			this.NextFog = NextFog;
		}

		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			if (TriggerType == EventTriggerType.Camera)
			{
				if (Direction < 0)
				{
					currentRoute.PreviousFog = PreviousFog;
					currentRoute.NextFog = CurrentFog;
				}
				else if (Direction > 0)
				{
					currentRoute.PreviousFog = CurrentFog;
					currentRoute.NextFog = NextFog;
				}
			}
		}
	}
}
