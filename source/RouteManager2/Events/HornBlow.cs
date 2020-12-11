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

		public HornBlowEvent(double TrackPositionDelta, HornTypes Type, bool TriggerOnce)
		{
			this.TrackPositionDelta = TrackPositionDelta;
			DontTriggerAnymore = false;
			this.TriggerOnce = TriggerOnce;
			this.Type = Type;
		}

		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			if (TriggerType == EventTriggerType.FrontCarFrontAxle || TriggerType == EventTriggerType.OtherCarFrontAxle)
			{
				if (this.DontTriggerAnymore || Car == null)
				{
					return;
				}
				if (Car.Index == Train.DriverCar)
				{
					dynamic dynamicCar = Car;
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
