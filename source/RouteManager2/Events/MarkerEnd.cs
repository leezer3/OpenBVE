using System.Linq;
using OpenBveApi.Hosts;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using RouteManager2.MessageManager;

namespace RouteManager2.Events
{
	/// <summary>Is calld when a marker or message is removed from the in-game display</summary>
	public class MarkerEndEvent : GeneralEvent
	{
		/// <summary>The marker or message to remove (Note: May have already timed-out)</summary>
		private readonly AbstractMessage message;

		private readonly HostInterface currentHost;

		public MarkerEndEvent(double TrackPositionDelta, AbstractMessage Message, HostInterface Host) : base(TrackPositionDelta)
		{
			DontTriggerAnymore = false;
			message = Message;
			currentHost = Host;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			AbstractTrain train = trackFollower.Train;
			EventTriggerType triggerType = trackFollower.TriggerType;

			if (triggerType == EventTriggerType.FrontCarFrontAxle && train.IsPlayerTrain || triggerType == EventTriggerType.Camera && currentHost.Application == HostApplication.RouteViewer)
			{
				if (message != null && train != null)
				{
					if (direction < 0)
					{
						if (message.Trains != null && !message.Trains.Contains(new System.IO.DirectoryInfo(train.TrainFolder).Name))
						{
							//Our train is NOT in the list of trains which this message triggers for
							return;
						}

						currentHost.AddMessage(message);
					}
					else if (direction > 0)
					{
						message.QueueForRemoval = true;
					}
				}
			}
		}
	}
}
