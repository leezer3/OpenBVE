﻿using System;
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
					if (triggerDirection == 1)
					{
						trackFollower.Train.Switch = Index;
					}
					break;
				case EventTriggerType.TrainRear:
					if (triggerDirection == -1)
					{
						trackFollower.Train.Switch = Index;
					}
					break;
			}
		}
	}

	/// <summary>Event controlling a trailing switch</summary>
	public class TrailingSwitchEvent : GeneralEvent
	{
		/// <summary>The GUID of the switch</summary>
		public readonly Guid Index;

		private readonly int triggerDirection;

		private readonly CurrentRoute currentRoute;

		private readonly bool derailments;

		public TrailingSwitchEvent(Guid idx, int direction, CurrentRoute route, bool derail)
		{
			Index = idx;
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
				case EventTriggerType.RearBogieAxle:
				case EventTriggerType.RearCarRearAxle:
				case EventTriggerType.OtherCarRearAxle:
					if (derailments == false || currentRoute.Switches[Index].CurrentlySetTrack == trackFollower.TrackIndex)
					{
						trackFollower.UpdateWorldCoordinates(false);
					}
					else
					{
						trackFollower.Car.Derail();
					}
					/*
					 * Need to set the track index to the toe rail even if derailed, as otherwise we may be in an invalid
					 * element (e.g. if a RailEnd / RailStart command has been issued)
					 * In this case, the world co-oords may be missing causing things to glitch out badly
					 */
					trackFollower.TrackIndex = currentRoute.Switches[Index].ToeRail;
					break;
				case EventTriggerType.TrainFront:
					if (triggerDirection == -1)
					{
						trackFollower.Train.Switch = Index;
					}
					break;
				case EventTriggerType.TrainRear:
					if (triggerDirection == 1)
					{
						trackFollower.Train.Switch = Index;
					}
					break;
			}
		}
	}
}

