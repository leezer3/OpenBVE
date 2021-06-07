using OpenBveApi.Routes;
using OpenBveApi.Trains;
using SoundManager;

namespace RouteManager2.Events
{
	public class PointSoundEvent : GeneralEvent
	{
		public PointSoundEvent()
		{
			DontTriggerAnymore = false;
		}

		/// <summary>Triggers the playback of a sound</summary>
		/// <param name="direction">The direction of travel- 1 for forwards, and -1 for backwards</param>
		/// <param name="trackFollower">The TrackFollower</param>
		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			if (SoundsBase.SuppressSoundEvents) return;
			AbstractCar car = trackFollower.Car;
			switch (trackFollower.TriggerType)
			{
				case EventTriggerType.FrontCarFrontAxle:
				case EventTriggerType.OtherCarFrontAxle:
					car.FrontAxle.PointSoundTriggered = true;
					DontTriggerAnymore = false;
					break;
				case EventTriggerType.OtherCarRearAxle:
				case EventTriggerType.RearCarRearAxle:
					car.RearAxle.PointSoundTriggered = true;
					DontTriggerAnymore = false;
					break;
			}
		}
	}
}
