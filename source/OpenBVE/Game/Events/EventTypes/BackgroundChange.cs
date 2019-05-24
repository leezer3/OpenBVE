using OpenBveApi.Routes;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Called when the displayed backgrond image or object should be changed</summary>
		internal class BackgroundChangeEvent : GeneralEvent
		{
			/// <summary>The background which applies previously to this point</summary>
			private readonly BackgroundHandle PreviousBackground;
			/// <summary>The background which applies after this point</summary>
			private readonly BackgroundHandle NextBackground;

			internal BackgroundChangeEvent(double TrackPositionDelta, BackgroundHandle PreviousBackground, BackgroundHandle NextBackground)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.PreviousBackground = PreviousBackground;
				this.NextBackground = NextBackground;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (TriggerType == EventTriggerType.Camera)
				{
					if (Direction < 0)
					{
						BackgroundManager.TargetBackground = this.PreviousBackground;
						BackgroundManager.TargetBackground.Countdown = 0;
					}
					else if (Direction > 0)
					{
						BackgroundManager.TargetBackground = this.NextBackground;
						BackgroundManager.TargetBackground.Countdown = 0;
					}
				}
			}
		}
	}
}
