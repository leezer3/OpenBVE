namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Defines fixed transponder types used by the built-in safety systems</summary>
		internal static class SpecialTransponderTypes
		{
			/// <summary>Marks the status of ATC.</summary>
			internal const int AtcTrackStatus = -16777215;
			/// <summary>Sets up an ATC speed limit.</summary>
			internal const int AtcSpeedLimit = -16777214;
			/// <summary>Sets up an ATS-P temporary speed limit.</summary>
			internal const int AtsPTemporarySpeedLimit = -16777213;
			/// <summary>Sets up an ATS-P permanent speed limit.</summary>
			internal const int AtsPPermanentSpeedLimit = -16777212;
			/// <summary>For internal use inside the CSV/RW parser only.</summary>
			internal const int InternalAtsPTemporarySpeedLimit = -16777201;
		}

		/// <summary>Called when a train passes over a transponder attached to the signalling system</summary>
		internal class TransponderEvent : GeneralEvent
		{
			/// <summary>The type of transponder</summary>
			internal readonly int Type;
			/// <summary>An optional data parameter passed to plugins recieving this event</summary>
			internal readonly int Data;
			/// <summary>The index of the section this is attached to</summary>
			private readonly int SectionIndex;
			/// <summary>Whether the section index this transponder returns is that of the first red section ahead of the train</summary>
			private readonly bool ClipToFirstRedSection;

			internal TransponderEvent(double trackPositionDelta, int type, int data, int sectionIndex, bool clipToFirstRedSection)
			{
				this.TrackPositionDelta = trackPositionDelta;
				this.DontTriggerAnymore = false;
				this.Type = type;
				this.Data = data;
				this.SectionIndex = sectionIndex;
				this.ClipToFirstRedSection = clipToFirstRedSection;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
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
								if (Game.Sections[s].Exists(Train))
								{
									s = this.SectionIndex;
									break;
								}
								int a = Game.Sections[s].CurrentAspect;
								if (a >= 0)
								{
									if (Game.Sections[s].Aspects[a].Number == 0)
									{
										break;
									}
								}
								s = Game.Sections[s].PreviousSection;
								if (s < 0)
								{
									s = this.SectionIndex;
									break;
								}
							}
						}
					}
					if (Train.Plugin != null)
					{
						Train.Plugin.UpdateBeacon((int)this.Type, s, this.Data);
					}
				}
			}
		}
	}
}
