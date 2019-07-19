using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Called when the cab brightness (lighting conditions) should be changed</summary>
		internal class BrightnessChangeEvent : GeneralEvent<AbstractTrain, TrainManager.Car>
		{
			/// <summary>The brightness to be applied from this point</summary>
			internal readonly float CurrentBrightness;
			/// <summary>The brightness to be applied prior to this point</summary>
			internal readonly float PreviousBrightness;
			/// <summary>The distance to the preceeding brightness change (Used in interpolation)</summary>
			internal readonly double PreviousDistance;
			/// <summary>The next brightness to be applied</summary>
			internal float NextBrightness;
			/// <summary>The distance to the next brightness change (Used in interpolation)</summary>
			internal double NextDistance;

			internal BrightnessChangeEvent(double TrackPositionDelta, float CurrentBrightness, float PreviousBrightness, double PreviousDistance)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.CurrentBrightness = CurrentBrightness;
				this.PreviousBrightness = PreviousBrightness;
				this.PreviousDistance = PreviousDistance;
				/*
				 * The next brightness & distance will be set when the following brightness event is added
				 * Setting them to the current values is no interpolation from this point onwards
				 */
				this.NextBrightness = CurrentBrightness;
				this.NextDistance = 0.0;
			}
			public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, TrainManager.Car Car)
			{
				if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle)
				{
					if (Direction < 0)
					{
						Car.Brightness.NextBrightness = this.CurrentBrightness;
						Car.Brightness.NextTrackPosition = Car.FrontAxle.Follower.TrackPosition;
						Car.Brightness.PreviousBrightness = this.PreviousBrightness;
						Car.Brightness.PreviousTrackPosition = Car.FrontAxle.Follower.TrackPosition - this.PreviousDistance;
					}
					else if (Direction > 0)
					{
						Car.Brightness.PreviousBrightness = this.CurrentBrightness;
						Car.Brightness.PreviousTrackPosition = Car.FrontAxle.Follower.TrackPosition;
						Car.Brightness.NextBrightness = this.NextBrightness;
						Car.Brightness.NextTrackPosition = Car.FrontAxle.Follower.TrackPosition + this.NextDistance;
					}
				}
			}
		}
	}
}
