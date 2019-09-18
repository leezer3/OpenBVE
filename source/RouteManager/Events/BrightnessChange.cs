using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace OpenBve.RouteManager
{
	/// <summary>Called when the cab brightness (lighting conditions) should be changed</summary>
		public class BrightnessChangeEvent : GeneralEvent
		{
			/// <summary>The brightness to be applied from this point</summary>
			public readonly float CurrentBrightness;
			/// <summary>The brightness to be applied prior to this point</summary>
			public readonly float PreviousBrightness;
			/// <summary>The distance to the preceeding brightness change (Used in interpolation)</summary>
			public readonly double PreviousDistance;
			/// <summary>The next brightness to be applied</summary>
			public float NextBrightness;
			/// <summary>The distance to the next brightness change (Used in interpolation)</summary>
			public double NextDistance;

			public BrightnessChangeEvent(double TrackPositionDelta, float CurrentBrightness, float PreviousBrightness, double PreviousDistance)
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
			public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
			{
				if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle)
				{
					if (Direction < 0)
					{
						Car.Brightness.NextBrightness = this.CurrentBrightness;
						Car.Brightness.NextTrackPosition = Car.TrackPosition;
						Car.Brightness.PreviousBrightness = this.PreviousBrightness;
						Car.Brightness.PreviousTrackPosition = Car.TrackPosition - this.PreviousDistance;
					}
					else if (Direction > 0)
					{
						Car.Brightness.PreviousBrightness = this.CurrentBrightness;
						Car.Brightness.PreviousTrackPosition = Car.TrackPosition;
						Car.Brightness.NextBrightness = this.NextBrightness;
						Car.Brightness.NextTrackPosition = Car.TrackPosition + this.NextDistance;
					}
				}
			}
		}
}
