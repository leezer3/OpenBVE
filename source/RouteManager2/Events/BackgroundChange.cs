using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace RouteManager2.Events
{
	/// <summary>Called when the displayed backgrond image or object should be changed</summary>
	public class BackgroundChangeEvent : GeneralEvent
	{
		private readonly CurrentRoute currentRoute;

		/// <summary>The background which applies previously to this point</summary>
		private readonly BackgroundHandle previousBackground;
		
		/// <summary>The background which applies after this point</summary>
		private readonly BackgroundHandle nextBackground;

		public BackgroundChangeEvent(CurrentRoute CurrentRoute, double TrackPositionDelta, BackgroundHandle PreviousBackground, BackgroundHandle NextBackground)
		{
			currentRoute = CurrentRoute;

			this.TrackPositionDelta = TrackPositionDelta;
			DontTriggerAnymore = false;
			previousBackground = PreviousBackground;
			nextBackground = NextBackground;
		}

		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			if (TriggerType == EventTriggerType.Camera)
			{
				if (Direction < 0)
				{
					currentRoute.TargetBackground = previousBackground;
					currentRoute.TargetBackground.Countdown = 0;
				}
				else if (Direction > 0)
				{
					currentRoute.TargetBackground = nextBackground;
					currentRoute.TargetBackground.Countdown = 0;
				}
			}
		}
	}
}
