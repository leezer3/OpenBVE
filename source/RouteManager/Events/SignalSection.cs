using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace OpenBve.RouteManager
{
	/// <summary>Is called when a train changes from one signalling section to another</summary>
		public class SectionChangeEvent : GeneralEvent
		{
			/// <summary>The index of the previous signalling section</summary>
			public readonly int PreviousSectionIndex;
			/// <summary>The index of the next signalling section</summary>
			public readonly int NextSectionIndex;

			public SectionChangeEvent(double TrackPositionDelta, int PreviousSectionIndex, int NextSectionIndex)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.PreviousSectionIndex = PreviousSectionIndex;
				this.NextSectionIndex = NextSectionIndex;
			}
			public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
			{
				if (Train != null)
				{
					if (TriggerType == EventTriggerType.FrontCarFrontAxle)
					{
						if (Direction < 0)
						{
							if (this.NextSectionIndex >= 0)
							{
								CurrentRoute.Sections[this.NextSectionIndex].TrainReachedStopPoint = false;
							}
							UpdateFrontBackward(Train, CurrentRoute.SecondsSinceMidnight);
						}
						else if (Direction > 0)
						{
							UpdateFrontForward(Train, CurrentRoute.SecondsSinceMidnight);
						}
					}
					else if (TriggerType == EventTriggerType.RearCarRearAxle)
					{
						if (Direction < 0)
						{
							UpdateRearBackward(Train, CurrentRoute.SecondsSinceMidnight);
						}
						else if (Direction > 0)
						{
							if (this.PreviousSectionIndex >= 0)
							{
								CurrentRoute.Sections[this.PreviousSectionIndex].TrainReachedStopPoint = false;
							}
							UpdateRearForward(Train, CurrentRoute.SecondsSinceMidnight);
						}
					}
				}
			}
			private void UpdateFrontBackward(AbstractTrain Train, double currentTime)
			{
				// update sections
				if (this.PreviousSectionIndex >= 0)
				{
					CurrentRoute.Sections[this.PreviousSectionIndex].Enter(Train);
					CurrentRoute.Sections[this.PreviousSectionIndex].Update(currentTime);
				}
				if (this.NextSectionIndex >= 0)
				{
					CurrentRoute.Sections[this.NextSectionIndex].Leave(Train);
					CurrentRoute.Sections[this.NextSectionIndex].Update(currentTime);
				}
				// update train
				if (this.PreviousSectionIndex >= 0)
				{
					if (!CurrentRoute.Sections[this.PreviousSectionIndex].Invisible)
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
			private void UpdateFrontForward(AbstractTrain Train, double currentTime)
			{
				// update train
				if (this.NextSectionIndex >= 0)
				{
					if (!CurrentRoute.Sections[this.NextSectionIndex].Invisible)
					{
						if (CurrentRoute.Sections[this.NextSectionIndex].CurrentAspect >= 0)
						{
							Train.CurrentSectionLimit = CurrentRoute.Sections[this.NextSectionIndex].Aspects[CurrentRoute.Sections[this.NextSectionIndex].CurrentAspect].Speed;
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
				if (this.NextSectionIndex < 0 || !CurrentRoute.Sections[this.NextSectionIndex].Invisible)
				{
					Train.SectionChange();
				}
				// update sections
				if (this.NextSectionIndex >= 0)
				{
					CurrentRoute.Sections[this.NextSectionIndex].Enter(Train);
					CurrentRoute.Sections[this.NextSectionIndex].Update(currentTime);
				}
			}
			private void UpdateRearBackward(AbstractTrain Train, double currentTime)
			{
				// update sections
				if (this.PreviousSectionIndex >= 0)
				{
					CurrentRoute.Sections[this.PreviousSectionIndex].Enter(Train);
					CurrentRoute.Sections[this.PreviousSectionIndex].Update(currentTime);
				}
			}
			private void UpdateRearForward(AbstractTrain Train, double currentTime)
			{
				// update sections
				if (this.PreviousSectionIndex >= 0)
				{
					CurrentRoute.Sections[this.PreviousSectionIndex].Leave(Train);
					CurrentRoute.Sections[this.PreviousSectionIndex].Update(currentTime);
				}
				if (this.NextSectionIndex >= 0)
				{
					CurrentRoute.Sections[this.NextSectionIndex].Enter(Train);
					CurrentRoute.Sections[this.NextSectionIndex].Update(currentTime);
				}
			}
		}
}
