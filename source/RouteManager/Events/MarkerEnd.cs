using System.Linq;
using OpenBveApi.Hosts;
using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace OpenBve.RouteManager
{
	/// <summary>Is calld when a marker or message is removed from the in-game display</summary>
	public class MarkerEndEvent : GeneralEvent
	{
		/// <summary>The marker or message to remove (Note: May have already timed-out)</summary>
		private readonly AbstractMessage Message;

		private readonly HostInterface currentHost;

		public MarkerEndEvent(double trackPositionDelta, AbstractMessage message, HostInterface Host)
		{
			this.TrackPositionDelta = trackPositionDelta;
			this.DontTriggerAnymore = false;
			this.Message = message;
			this.currentHost = Host;
		}
		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			if (TriggerType == EventTriggerType.FrontCarFrontAxle && Train.IsPlayerTrain || TriggerType == EventTriggerType.Camera && currentHost.Application == HostApplication.RouteViewer)
			{
				if (this.Message != null)
				{
					if (Direction < 0)
					{
						if (this.Message.Trains != null && !this.Message.Trains.Contains(new System.IO.DirectoryInfo(Train.TrainFolder).Name))
						{
							//Our train is NOT in the list of trains which this message triggers for
							return;
						}
						currentHost.AddMessage(this.Message);
					}
					else if (Direction > 0)
					{
						this.Message.QueueForRemoval = true;
					}
				}
			}
		}
	}
}
