using System;
using OpenBveApi.Routes;

namespace RouteManager2.Events
{
	/// <summary>Called when a train passes over a transponder attached to the signalling system</summary>
	public class TransponderEvent : GeneralEvent
	{
		private readonly CurrentRoute currentRoute;

		/// <summary>The type of transponder</summary>
		public readonly int Type;

		/// <summary>An optional data parameter passed to plugins recieving this event</summary>
		public readonly int Data;

		/// <summary>The index of the section this is attached to</summary>
		private readonly int SectionIndex;

		/// <summary>Whether the section index this transponder returns is that of the first red section ahead of the train</summary>
		private readonly bool ClipToFirstRedSection;

		public TransponderEvent(CurrentRoute CurrentRoute, double TrackPositionDelta, TransponderTypes Type, int Data, int SectionIndex, bool ClipToFirstRedSection)
			: this(CurrentRoute, TrackPositionDelta, (int)Type, Data, SectionIndex, ClipToFirstRedSection)
		{
		}

		public TransponderEvent(CurrentRoute CurrentRoute, double TrackPositionDelta, int Type, int Data, int SectionIndex, bool ClipToFirstRedSection) : base(TrackPositionDelta)
		{
			currentRoute = CurrentRoute;

			DontTriggerAnymore = false;
			this.Type = Type;
			this.Data = Data;
			this.SectionIndex = SectionIndex;
			this.ClipToFirstRedSection = ClipToFirstRedSection;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			if (trackFollower.TriggerType == EventTriggerType.TrainFront)
			{
				int s = SectionIndex;

				if (ClipToFirstRedSection)
				{
					if (s >= 0)
					{
						while (true)
						{
							if (currentRoute.Sections[s].Exists(trackFollower.Train))
							{
								s = SectionIndex;
								break;
							}

							int a = currentRoute.Sections[s].CurrentAspect;

							if (a >= 0)
							{
								if (currentRoute.Sections[s].Aspects[a].Number == 0)
								{
									break;
								}
							}

							s = Array.IndexOf(currentRoute.Sections, currentRoute.Sections[s].PreviousSection);

							if (s >= 0)
							{
								s = SectionIndex;
								break;
							}
						}
					}
				}

				trackFollower.Train.UpdateBeacon(Type, s, Data);
			}
		}
	}
}
