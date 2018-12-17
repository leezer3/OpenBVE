using BackgroundManager;
using OpenBveApi.Trains;
using OpenBveShared;

namespace TrackManager
{
	/// <summary>Called when the displayed backgrond image or object should be changed</summary>
	public class BackgroundChangeEvent : GeneralEvent
	{
		/// <summary>The background which applies previously to this point</summary>
		private readonly BackgroundHandle PreviousBackground;
		/// <summary>The background which applies after this point</summary>
		private readonly BackgroundHandle NextBackground;

		public BackgroundChangeEvent(double TrackPositionDelta, BackgroundHandle PreviousBackground, BackgroundHandle NextBackground)
		{
			this.TrackPositionDelta = TrackPositionDelta;
			this.DontTriggerAnymore = false;
			this.PreviousBackground = PreviousBackground;
			this.NextBackground = NextBackground;
		}

		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, int CarIndex)
		{
			if (TriggerType == EventTriggerType.Camera)
			{
				if (Direction < 0)
				{
					Renderer.TargetBackground = this.PreviousBackground;
					Renderer.TargetBackground.Countdown = 0;
				}
				else if (Direction > 0)
				{
					Renderer.TargetBackground = this.NextBackground;
					Renderer.TargetBackground.Countdown = 0;
				}
			}
		}
	}
}
