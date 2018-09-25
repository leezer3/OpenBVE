using OpenBveApi.Colors;
using OpenBveApi.Interface;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Is called when a train changes from one signalling section to another</summary>
		internal class SectionChangeEvent : GeneralEvent
		{
			/// <summary>The index of the previous signalling section</summary>
			internal readonly int PreviousSectionIndex;
			/// <summary>The index of the next signalling section</summary>
			internal readonly int NextSectionIndex;

			internal SectionChangeEvent(double TrackPositionDelta, int PreviousSectionIndex, int NextSectionIndex)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.PreviousSectionIndex = PreviousSectionIndex;
				this.NextSectionIndex = NextSectionIndex;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (Train != null)
				{
					if (TriggerType == EventTriggerType.FrontCarFrontAxle)
					{
						if (Direction < 0)
						{
							if (this.NextSectionIndex >= 0)
							{
								Game.Sections[this.NextSectionIndex].TrainReachedStopPoint = false;
							}
							UpdateFrontBackward(Train, true);
						}
						else if (Direction > 0)
						{
							UpdateFrontForward(Train, true, true);
						}
					}
					else if (TriggerType == EventTriggerType.RearCarRearAxle)
					{
						if (Direction < 0)
						{
							UpdateRearBackward(Train, true);
						}
						else if (Direction > 0)
						{
							if (this.PreviousSectionIndex >= 0)
							{
								Game.Sections[this.PreviousSectionIndex].TrainReachedStopPoint = false;
							}
							UpdateRearForward(Train, true);
						}
					}
				}
			}
			private void UpdateFrontBackward(TrainManager.Train Train, bool UpdateTrain)
			{
				// update sections
				if (this.PreviousSectionIndex >= 0)
				{
					Game.Sections[this.PreviousSectionIndex].Enter(Train);
					Game.UpdateSection(this.PreviousSectionIndex);
				}
				if (this.NextSectionIndex >= 0)
				{
					Game.Sections[this.NextSectionIndex].Leave(Train);
					Game.UpdateSection(this.NextSectionIndex);
				}
				if (UpdateTrain)
				{
					// update train
					if (this.PreviousSectionIndex >= 0)
					{
						if (!Game.Sections[this.PreviousSectionIndex].Invisible)
						{
							Train.CurrentSectionIndex = this.PreviousSectionIndex;
						}
					}
					else
					{
						Train.CurrentSectionLimit = double.PositiveInfinity;
						Train.CurrentSectionIndex = -1;
					}
				}
			}
			private void UpdateFrontForward(TrainManager.Train Train, bool UpdateTrain, bool UpdateSection)
			{
				if (UpdateTrain)
				{
					// update train
					if (this.NextSectionIndex >= 0)
					{
						if (!Game.Sections[this.NextSectionIndex].Invisible)
						{
							if (Game.Sections[this.NextSectionIndex].CurrentAspect >= 0)
							{
								Train.CurrentSectionLimit = Game.Sections[this.NextSectionIndex].Aspects[Game.Sections[this.NextSectionIndex].CurrentAspect].Speed;
							}
							else
							{
								Train.CurrentSectionLimit = double.PositiveInfinity;
							}
							Train.CurrentSectionIndex = this.NextSectionIndex;
						}
					}
					else
					{
						Train.CurrentSectionLimit = double.PositiveInfinity;
						Train.CurrentSectionIndex = -1;
					}
					// messages
					if (this.NextSectionIndex < 0 || !Game.Sections[this.NextSectionIndex].Invisible)
					{
						if (Train.CurrentSectionLimit == 0.0 && Game.MinimalisticSimulation == false)
						{
							Game.AddMessage(Translations.GetInterfaceString("message_signal_stop"), MessageManager.MessageDependency.PassedRedSignal, Interface.GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
						}
						else if (Train.Specs.CurrentAverageSpeed > Train.CurrentSectionLimit)
						{
							Game.AddMessage(Translations.GetInterfaceString("message_signal_overspeed"), MessageManager.MessageDependency.SectionLimit, Interface.GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
						}
					}
				}
				if (UpdateSection)
				{
					// update sections
					if (this.NextSectionIndex >= 0)
					{
						Game.Sections[this.NextSectionIndex].Enter(Train);
						Game.UpdateSection(this.NextSectionIndex);
					}
				}
			}
			private void UpdateRearBackward(TrainManager.Train Train, bool UpdateSection)
			{
				if (UpdateSection)
				{
					// update sections
					if (this.PreviousSectionIndex >= 0)
					{
						Game.Sections[this.PreviousSectionIndex].Enter(Train);
						Game.UpdateSection(this.PreviousSectionIndex);
					}
				}
			}
			private void UpdateRearForward(TrainManager.Train Train, bool UpdateSection)
			{
				if (UpdateSection)
				{
					// update sections
					if (this.PreviousSectionIndex >= 0)
					{
						Game.Sections[this.PreviousSectionIndex].Leave(Train);
						Game.UpdateSection(this.PreviousSectionIndex);
					}
					if (this.NextSectionIndex >= 0)
					{
						Game.Sections[this.NextSectionIndex].Enter(Train);
						Game.UpdateSection(this.NextSectionIndex);
					}
				}
			}
		}
	}
}
