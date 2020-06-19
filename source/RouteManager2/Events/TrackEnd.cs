using OpenBveApi.Hosts;
using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace RouteManager2.Events
{
	/// <summary>This event is placed at the end of the track</summary>
	public class TrackEndEvent : GeneralEvent
	{
		private readonly HostInterface currentHost;

		public TrackEndEvent(HostInterface Host, double TrackPositionDelta)
		{
			currentHost = Host;
			this.TrackPositionDelta = TrackPositionDelta;
			DontTriggerAnymore = false;
		}

		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			if (Train == null)
			{
				return;
			}

			if (TriggerType == EventTriggerType.RearCarRearAxle & !Train.IsPlayerTrain)
			{
				Train.Dispose();
			}
			else if (Train.IsPlayerTrain)
			{
				Train.Derail(Car, 0.0);
			}

			if (TriggerType == EventTriggerType.Camera)
			{
				currentHost.CameraAtWorldEnd();
			}
		}
	}
}
