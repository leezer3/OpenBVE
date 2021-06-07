using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;

namespace RouteManager2.Events
{
	/// <summary>Called when the AI driver should blow the horn</summary>
	public class HornBlowEvent : GeneralEvent
	{
		private readonly HornTypes Type;
		private readonly bool TriggerOnce;

		public HornBlowEvent(double TrackPositionDelta, HornTypes Type, bool TriggerOnce) : base(TrackPositionDelta)
		{
			DontTriggerAnymore = false;
			this.TriggerOnce = TriggerOnce;
			this.Type = Type;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			EventTriggerType triggerType = trackFollower.TriggerType;

			if (triggerType == EventTriggerType.FrontCarFrontAxle || triggerType == EventTriggerType.OtherCarFrontAxle)
			{
				AbstractCar car = trackFollower.Car;

				if (DontTriggerAnymore || car == null)
				{
					return;
				}
				if (car.Index == trackFollower.Train.DriverCar)
				{
					dynamic dynamicCar = car;
					dynamicCar.Horns[(int) Type].Play();
					dynamicCar.Horns[(int) Type].Stop();
					if (TriggerOnce)
					{
						DontTriggerAnymore = true;
					}
				}
			}
		}
	}
}
