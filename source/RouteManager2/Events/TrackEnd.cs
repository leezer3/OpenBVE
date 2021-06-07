using OpenBveApi.Hosts;
using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace RouteManager2.Events
{
	/// <summary>This event is placed at the end of the track</summary>
	public class TrackEndEvent : GeneralEvent
	{
		private readonly HostInterface currentHost;

		public TrackEndEvent(HostInterface Host, double TrackPositionDelta) : base(TrackPositionDelta)
		{
			currentHost = Host;
			DontTriggerAnymore = false;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			AbstractTrain train = trackFollower.Train;

			if (train == null)
			{
				return;
			}

			if (trackFollower.TriggerType == EventTriggerType.RearCarRearAxle & !train.IsPlayerTrain)
			{
				train.Dispose();
			}
			else if (train.IsPlayerTrain)
			{
				train.Derail(trackFollower.Car, 0.0);
			}

			if (trackFollower.TriggerType == EventTriggerType.Camera)
			{
				currentHost.CameraAtWorldEnd();
			}
		}
	}
}
