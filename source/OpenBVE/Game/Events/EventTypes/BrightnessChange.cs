namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Called when the cab brightness (lighting conditions) should be changed</summary>
		internal class BrightnessChangeEvent : GeneralEvent
		{
			/// <summary>The brightness to be applied from this point</summary>
			internal float CurrentBrightness;
			/// <summary>The brightness to be applied prior to this point</summary>
			internal float PreviousBrightness;
			/// <summary>The distance to the preceeding brightness change (Used in interpolation)</summary>
			internal double PreviousDistance;
			/// <summary>The next brightness to be applied</summary>
			internal float NextBrightness;
			/// <summary>The distance to the next brightness change (Used in interpolation)</summary>
			internal double NextDistance;

			internal BrightnessChangeEvent(double TrackPositionDelta, float CurrentBrightness, float PreviousBrightness, double PreviousDistance, float NextBrightness, double NextDistance)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.CurrentBrightness = CurrentBrightness;
				this.PreviousBrightness = PreviousBrightness;
				this.PreviousDistance = PreviousDistance;
				this.NextBrightness = NextBrightness;
				this.NextDistance = NextDistance;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle)
				{
					if (Direction < 0)
					{
						//Train.Cars[CarIndex].Brightness.NextBrightness = Train.Cars[CarIndex].Brightness.PreviousBrightness;
						Train.Cars[CarIndex].Brightness.NextBrightness = this.CurrentBrightness;
						Train.Cars[CarIndex].Brightness.NextTrackPosition = Train.Cars[CarIndex].FrontAxle.Follower.TrackPosition;
						Train.Cars[CarIndex].Brightness.PreviousBrightness = this.PreviousBrightness;
						Train.Cars[CarIndex].Brightness.PreviousTrackPosition = Train.Cars[CarIndex].FrontAxle.Follower.TrackPosition - this.PreviousDistance;
					}
					else if (Direction > 0)
					{
						//Train.Cars[CarIndex].Brightness.PreviousBrightness = Train.Cars[CarIndex].Brightness.NextBrightness;
						Train.Cars[CarIndex].Brightness.PreviousBrightness = this.CurrentBrightness;
						Train.Cars[CarIndex].Brightness.PreviousTrackPosition = Train.Cars[CarIndex].FrontAxle.Follower.TrackPosition;
						Train.Cars[CarIndex].Brightness.NextBrightness = this.NextBrightness;
						Train.Cars[CarIndex].Brightness.NextTrackPosition = Train.Cars[CarIndex].FrontAxle.Follower.TrackPosition + this.NextDistance;
					}
				}
			}
		}
	}
}
