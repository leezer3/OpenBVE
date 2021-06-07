using OpenBveApi.Routes;

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

		public BackgroundChangeEvent(CurrentRoute CurrentRoute, double TrackPositionDelta, BackgroundHandle PreviousBackground, BackgroundHandle NextBackground) : base(TrackPositionDelta)
		{
			currentRoute = CurrentRoute;
			DontTriggerAnymore = false;
			previousBackground = PreviousBackground;
			nextBackground = NextBackground;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			if (trackFollower.TriggerType == EventTriggerType.Camera)
			{
				if (direction < 0)
				{
					currentRoute.TargetBackground = previousBackground;
					currentRoute.TargetBackground.Countdown = 0;
				}
				else if (direction > 0)
				{
					currentRoute.TargetBackground = nextBackground;
					currentRoute.TargetBackground.Countdown = 0;
				}
			}
		}
	}
}
