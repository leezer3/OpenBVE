using static LibRender.CameraProperties;
using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace OpenBve.RouteManager
{
	/// <summary>This event is placed at the end of the track</summary>
	public class TrackEndEvent : GeneralEvent
	{
		public TrackEndEvent(double TrackPositionDelta)
		{
			this.TrackPositionDelta = TrackPositionDelta;
			this.DontTriggerAnymore = false;
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
				Camera.AtWorldEnd = !Camera.AtWorldEnd;
			}
		}
	}
}
