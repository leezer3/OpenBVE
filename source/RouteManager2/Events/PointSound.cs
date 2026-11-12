using OpenBveApi.Routes;
using OpenBveApi.Trains;
using SoundManager;

namespace RouteManager2.Events
{
	public class PointSoundEvent : GeneralEvent
	{
		/// <summary>The index of the point sound to play</summary>
		/// <remarks>-1 matches the current run sound</remarks>
		private readonly int Index;

		public PointSoundEvent(int index = -1)
		{
			DontTriggerAnymore = false;
			Index = index;
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
					car.FrontAxle.PointSoundIndex = Index;
					DontTriggerAnymore = false;
					break;
				case EventTriggerType.OtherCarRearAxle:
				case EventTriggerType.RearCarRearAxle:
					car.RearAxle.PointSoundTriggered = true;
					car.RearAxle.PointSoundIndex = Index;
					DontTriggerAnymore = false;
					break;
			}
		}
	}
}
