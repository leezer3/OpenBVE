using System;
using OpenBveApi.Routes;

namespace RouteManager2.Events
{
	/// <summary>Event controlling a facing switch</summary>
	public class SwitchEvent : GeneralEvent
	{
		/// <summary>The GUID of the switch</summary>
		public readonly Guid Index;
		/// <summary>The track position of the switch</summary>
		public readonly double TrackPosition;

		private readonly int triggerDirection;

		private readonly CurrentRoute currentRoute;

		public SwitchEvent(Guid idx, int direction, double trackPosition, CurrentRoute route)
		{
			Index = idx;
			triggerDirection = direction;
			TrackPosition = trackPosition;
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
				case EventTriggerType.FrontBogieAxle:
				case EventTriggerType.RearBogieAxle:
				case EventTriggerType.RearCarRearAxle:
				case EventTriggerType.OtherCarRearAxle:
				case EventTriggerType.FrontCarFrontAxle:
				case EventTriggerType.OtherCarFrontAxle:
					trackFollower.TrackIndex = currentRoute.Switches[Index].CurrentlySetTrack;
					trackFollower.UpdateWorldCoordinates(false);
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
		/// <summary>The GUID of the switch</summary>
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
				case EventTriggerType.FrontBogieAxle:
				case EventTriggerType.FrontCarFrontAxle:
				case EventTriggerType.OtherCarFrontAxle:
					if (derailments == false || currentRoute.Switches[Index].CurrentlySetTrack == trackFollower.TrackIndex)
					{
						trackFollower.TrackIndex = toeRail;
						trackFollower.UpdateWorldCoordinates(false);
					}
					else
					{
						trackFollower.Car.Derail();
					}

					break;
				case EventTriggerType.RearBogieAxle:
				case EventTriggerType.RearCarRearAxle:
				case EventTriggerType.OtherCarRearAxle:
					if (derailments == false || currentRoute.Switches[Index].CurrentlySetTrack == trackFollower.TrackIndex)
					{
						trackFollower.TrackIndex = toeRail;
						trackFollower.UpdateWorldCoordinates(false);
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

