using OpenBveApi.Routes;
using OpenBveApi.Trains;
using SoundManager;

namespace RouteManager2.Events
{
	public class PointSoundEvent : GeneralEvent
	{
		public PointSoundEvent()
		{
			this.DontTriggerAnymore = false;
		}

		/// <summary>Triggers the playback of a sound</summary>
		/// <param name="Direction">The direction of travel- 1 for forwards, and -1 for backwards</param>
		/// <param name="TriggerType">They type of event which triggered this sound</param>
		/// <param name="Train">The root train which triggered this sound</param>
		/// <param name="Car">The car which triggered this sound</param>
		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			if (SoundsBase.SuppressSoundEvents) return;
			switch (TriggerType)
			{
				case EventTriggerType.FrontCarFrontAxle:
				case EventTriggerType.OtherCarFrontAxle:
					Car.FrontAxle.PointSoundTriggered = true;
					DontTriggerAnymore = false;
					break;
				case EventTriggerType.OtherCarRearAxle:
				case EventTriggerType.RearCarRearAxle:
					Car.RearAxle.PointSoundTriggered = true;
					DontTriggerAnymore = false;
					break;
			}
		}
	}
}
