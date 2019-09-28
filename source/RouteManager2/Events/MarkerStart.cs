using System.Linq;
using OpenBveApi.Hosts;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using RouteManager2.MessageManager;

namespace RouteManager2.Events
{
	/// <summary>Is called when a marker or message is added to the in-game display</summary>
	public class MarkerStartEvent : GeneralEvent
	{
		/// <summary>The marker or message to add</summary>
		private readonly AbstractMessage message;

		private readonly HostInterface currentHost;

		public MarkerStartEvent(double TrackPositionDelta, AbstractMessage Message, HostInterface Host)
		{
			this.TrackPositionDelta = TrackPositionDelta;
			DontTriggerAnymore = false;
			message = Message;
			currentHost = Host;
		}

		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			if (TriggerType == EventTriggerType.FrontCarFrontAxle && Train.IsPlayerTrain || TriggerType == EventTriggerType.Camera && currentHost.Application == HostApplication.RouteViewer)
			{
				if (message != null)
				{
					if (Direction < 0)
					{
						message.QueueForRemoval = true;
					}
					else if (Direction > 0)
					{
						if (message.Trains != null && !message.Trains.Contains(new System.IO.DirectoryInfo(Train.TrainFolder).Name))
						{
							//Our train is NOT in the list of trains which this message triggers for
							return;
						}

						currentHost.AddMessage(message);
					}
				}
			}
		}
	}
}
