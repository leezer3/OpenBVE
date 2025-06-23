using System;
using OpenBveApi.Routes;

namespace RouteManager2.Events
{
	/// <summary>Event controlling a switch</summary>
	public class SwitchEvent : GeneralEvent
	{
		/// <summary>The GUID of the switch</summary>
		public readonly Guid Index;

		private readonly CurrentRoute currentRoute;

		public SwitchEvent(Guid idx, CurrentRoute route)
		{
			Index = idx;
			currentRoute = route;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			switch (trackFollower.TriggerType)
			{
				case EventTriggerType.FrontBogieAxle:
				case EventTriggerType.RearBogieAxle:
				case EventTriggerType.RearCarRearAxle:
				case EventTriggerType.OtherCarRearAxle:
				case EventTriggerType.FrontCarFrontAxle:
				case EventTriggerType.OtherCarFrontAxle:
					if (direction == (int)currentRoute.Switches[Index].Direction)
					{
						// Moving from the toe direction means we are always correct and go to the set track
						trackFollower.TrackIndex = currentRoute.Switches[Index].CurrentlySetTrack;
					}
					else
					{
						if (trackFollower.TrackIndex != currentRoute.Switches[Index].CurrentlySetTrack && trackFollower.TrackIndex != currentRoute.Switches[Index].ToeRail)
						{
							// Our track follower is not on the set track or the toe rail, so the switch must be against us
							// If derailments are off, this will just return in the car derail function
							trackFollower.Car.Derail();
							currentRoute.Switches[Index].RunThrough = true;
						}
						trackFollower.TrackIndex = currentRoute.Switches[Index].ToeRail;
					}
					trackFollower.UpdateWorldCoordinates(false);
					break;
				case EventTriggerType.TrainFront:
				case EventTriggerType.SingleCarTrain:
					if ((int)currentRoute.Switches[Index].Direction == 1)
					{
						trackFollower.Train.Switch = Index;
					}

					SetTrackIndex(trackFollower, direction);
					break;
				case EventTriggerType.TrainRear:
					if ((int)currentRoute.Switches[Index].Direction == -1)
					{
						trackFollower.Train.Switch = Index;
					}
					SetTrackIndex(trackFollower, direction);
					break;
				case EventTriggerType.Camera:
					// Camera should always follow the track and will just go through switches set against us
					SetTrackIndex(trackFollower, direction);
					break;
			}
		}

		private void SetTrackIndex(TrackFollower trackFollower, int direction)
		{
			if (direction == (int)currentRoute.Switches[Index].Direction)
			{
				trackFollower.TrackIndex = currentRoute.Switches[Index].CurrentlySetTrack;
			}
			else
			{
				trackFollower.TrackIndex = currentRoute.Switches[Index].ToeRail;
			}
		}
	}
}

