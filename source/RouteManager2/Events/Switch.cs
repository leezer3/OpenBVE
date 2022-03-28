using System;
using OpenBveApi.Routes;

namespace RouteManager2.Events
{
	/// <summary>Event controlling a facing switch</summary>
	public class SwitchEvent : GeneralEvent
	{

		public readonly Guid Index;

		private readonly int triggerDirection;

		private readonly CurrentRoute currentRoute;

		public SwitchEvent(Guid idx, int direction, CurrentRoute route)
		{
			Index = idx;
			triggerDirection = direction;
			currentRoute = route;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			if (direction != triggerDirection)
			{
				//Check traversal direction
				return;
			}

			switch (trackFollower.TriggerType)
			{
				case EventTriggerType.FrontCarFrontAxle:
				case EventTriggerType.OtherCarFrontAxle:
					trackFollower.Car.FrontAxle.Follower.TrackIndex = currentRoute.Switches[Index].CurrentlySetTrack();
					trackFollower.Car.FrontAxle.Follower.UpdateWorldCoordinates(false);
					break;
				case EventTriggerType.RearCarRearAxle:
				case EventTriggerType.OtherCarRearAxle:
					trackFollower.Car.RearAxle.Follower.TrackIndex = currentRoute.Switches[Index].CurrentlySetTrack();
					trackFollower.Car.RearAxle.Follower.UpdateWorldCoordinates(false);
					break;
				case EventTriggerType.TrainFront:
					trackFollower.Train.Switch = Index;
					break;
			}
		}
	}

	/// <summary>Event controlling a trailing switch</summary>
	public class TrailingSwitchEvent : GeneralEvent
	{

		public readonly Guid Index;

		private readonly int toeRail;

		private readonly int triggerDirection;

		private readonly CurrentRoute currentRoute;

		private readonly bool derailments;

		public TrailingSwitchEvent(Guid idx, int trackIndex, int direction, CurrentRoute route, bool derail)
		{
			Index = idx;
			toeRail = trackIndex;
			triggerDirection = direction;
			currentRoute = route;
			derailments = derail;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			if (direction != triggerDirection)
			{
				//Check traversal direction
				return;
			}

			switch (trackFollower.TriggerType)
			{
				case EventTriggerType.FrontCarFrontAxle:
				case EventTriggerType.OtherCarFrontAxle:
					if (derailments == false || currentRoute.Switches[Index].CurrentlySetTrack() == trackFollower.Car.FrontAxle.Follower.TrackIndex)
					{
						trackFollower.Car.FrontAxle.Follower.TrackIndex = toeRail;
						trackFollower.Car.FrontAxle.Follower.UpdateWorldCoordinates(false);
					}
					else
					{
						trackFollower.Car.Derail();
					}

					break;
				case EventTriggerType.RearCarRearAxle:
				case EventTriggerType.OtherCarRearAxle:
					if (derailments == false || currentRoute.Switches[Index].CurrentlySetTrack() == trackFollower.Car.RearAxle.Follower.TrackIndex)
					{
						trackFollower.Car.RearAxle.Follower.TrackIndex = toeRail;
						trackFollower.Car.RearAxle.Follower.UpdateWorldCoordinates(false);
					}
					else
					{
						trackFollower.Car.Derail();
					}
					break;
				case EventTriggerType.TrainFront:
					trackFollower.Train.Switch = Index;
					break;
			}
		}
	}
}

