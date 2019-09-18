using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace OpenBve.RouteManager
{
	/// <summary>Called when a train passes over a transponder attached to the signalling system</summary>
	public class TransponderEvent : GeneralEvent
	{
		/// <summary>The type of transponder</summary>
		public readonly int Type;
		/// <summary>An optional data parameter passed to plugins recieving this event</summary>
		public readonly int Data;
		/// <summary>The index of the section this is attached to</summary>
		private readonly int SectionIndex;
		/// <summary>Whether the section index this transponder returns is that of the first red section ahead of the train</summary>
		private readonly bool ClipToFirstRedSection;

		public TransponderEvent(double trackPositionDelta, int type, int data, int sectionIndex, bool clipToFirstRedSection)
		{
			this.TrackPositionDelta = trackPositionDelta;
			this.DontTriggerAnymore = false;
			this.Type = type;
			this.Data = data;
			this.SectionIndex = sectionIndex;
			this.ClipToFirstRedSection = clipToFirstRedSection;
		}

		public TransponderEvent(double trackPositionDelta, TransponderTypes type, int data, int sectionIndex, bool clipToFirstRedSection)
		{
			this.TrackPositionDelta = trackPositionDelta;
			this.DontTriggerAnymore = false;
			this.Type = (int)type;
			this.Data = data;
			this.SectionIndex = sectionIndex;
			this.ClipToFirstRedSection = clipToFirstRedSection;
		}

		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			if (TriggerType == EventTriggerType.TrainFront)
			{
				int s = this.SectionIndex;
				if (this.ClipToFirstRedSection)
				{
					if (s >= 0)
					{
						while (true)
						{
							if (CurrentRoute.Sections[s].Exists(Train))
							{
								s = this.SectionIndex;
								break;
							}
							int a = CurrentRoute.Sections[s].CurrentAspect;
							if (a >= 0)
							{
								if (CurrentRoute.Sections[s].Aspects[a].Number == 0)
								{
									break;
								}
							}
							s = CurrentRoute.Sections[s].PreviousSection;
							if (s < 0)
							{
								s = this.SectionIndex;
								break;
							}
						}
					}
				}
				Train.UpdateBeacon((int)this.Type, s, this.Data);
			}
		}
	}
}
