using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace RouteManager2.Events
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

		public BrightnessChangeEvent(double TrackPositionDelta, float CurrentBrightness, float PreviousBrightness, double PreviousDistance) : base(TrackPositionDelta)
		{
			DontTriggerAnymore = false;
			this.CurrentBrightness = CurrentBrightness;
			this.PreviousBrightness = PreviousBrightness;
			this.PreviousDistance = PreviousDistance;
			/*
			 * The next brightness & distance will be set when the following brightness event is added
			 * Setting them to the current values is no interpolation from this point onwards
			 */
			NextBrightness = CurrentBrightness;
			NextDistance = 0.0;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			EventTriggerType triggerType = trackFollower.TriggerType;

			if (triggerType == EventTriggerType.FrontCarFrontAxle | triggerType == EventTriggerType.OtherCarFrontAxle)
			{
				AbstractCar car = trackFollower.Car;

				if (direction < 0)
				{
					car.Brightness.NextBrightness = CurrentBrightness;
					car.Brightness.NextTrackPosition = car.TrackPosition;
					car.Brightness.PreviousBrightness = PreviousBrightness;
					car.Brightness.PreviousTrackPosition = car.TrackPosition - PreviousDistance;
				}
				else if (direction > 0)
				{
					car.Brightness.PreviousBrightness = CurrentBrightness;
					car.Brightness.PreviousTrackPosition = car.TrackPosition;
					car.Brightness.NextBrightness = NextBrightness;
					car.Brightness.NextTrackPosition = car.TrackPosition + NextDistance;
				}
			}
		}
	}
}
