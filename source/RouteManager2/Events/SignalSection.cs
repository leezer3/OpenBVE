using System;
using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace RouteManager2.Events
{
	/// <summary>Is called when a train changes from one signalling section to another</summary>
	public class SectionChangeEvent : GeneralEvent
	{
		private readonly CurrentRoute currentRoute;

		/// <summary>The index of the previous signalling section</summary>
		public readonly int PreviousSectionIndex;

		/// <summary>The index of the next signalling section</summary>
		public readonly int NextSectionIndex;

		public SectionChangeEvent(CurrentRoute CurrentRoute, double TrackPositionDelta, int PreviousSectionIndex, int NextSectionIndex)
		{
			currentRoute = CurrentRoute;

			this.TrackPositionDelta = TrackPositionDelta;
			DontTriggerAnymore = false;
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
						if (NextSectionIndex >= 0)
						{
							currentRoute.Sections[NextSectionIndex].TrainReachedStopPoint = false;
						}

						UpdateFrontBackward(Train);
					}
					else if (Direction > 0)
					{
						UpdateFrontForward(Train);
					}
				}
				else if (TriggerType == EventTriggerType.RearCarRearAxle)
				{
					if (Direction < 0)
					{
						UpdateRearBackward(Train);
					}
					else if (Direction > 0)
					{
						if (PreviousSectionIndex >= 0)
						{
							currentRoute.Sections[PreviousSectionIndex].TrainReachedStopPoint = false;
						}

						UpdateRearForward(Train);
					}
				}
			}
		}
		private void UpdateFrontBackward(AbstractTrain Train)
		{
			// update sections
			if (PreviousSectionIndex >= 0)
			{
				currentRoute.Sections[PreviousSectionIndex].Enter(Train);
				currentRoute.UpdateSection(PreviousSectionIndex);
			}

			if (NextSectionIndex >= 0)
			{
				currentRoute.Sections[NextSectionIndex].Leave(Train);
				currentRoute.UpdateSection(NextSectionIndex);
			}

			// update train
			if (PreviousSectionIndex >= 0)
			{
				if (!currentRoute.Sections[PreviousSectionIndex].Invisible)
				{
					Train.CurrentSectionIndex = PreviousSectionIndex;
				}
			}
			else
			{
				Train.CurrentSectionLimit = double.PositiveInfinity;
				Train.CurrentSectionIndex = -1;
			}
		}

		private void UpdateFrontForward(AbstractTrain Train)
		{
			// update train
			if (NextSectionIndex >= 0)
			{
				if (!currentRoute.Sections[NextSectionIndex].Invisible)
				{
					Train.CurrentSectionLimit = currentRoute.Sections[NextSectionIndex].CurrentAspect >= 0 ? currentRoute.Sections[NextSectionIndex].Aspects[currentRoute.Sections[NextSectionIndex].CurrentAspect].Speed : double.PositiveInfinity;
					Train.CurrentSectionIndex = NextSectionIndex;
				}
			}
			else
			{
				Train.CurrentSectionLimit = double.PositiveInfinity;
				Train.CurrentSectionIndex = -1;
			}

			// messages
			if (NextSectionIndex >= 0 || !currentRoute.Sections[NextSectionIndex].Invisible)
			{
				Train.SectionChange();
			}

			// update sections
			if (NextSectionIndex >= 0)
			{
				currentRoute.Sections[NextSectionIndex].Enter(Train);
				currentRoute.UpdateSection(NextSectionIndex);
			}
		}

		private void UpdateRearBackward(AbstractTrain Train)
		{
			// update sections
			if (PreviousSectionIndex >= 0)
			{
				currentRoute.Sections[PreviousSectionIndex].Enter(Train);
				currentRoute.UpdateSection(PreviousSectionIndex);
			}
		}

		private void UpdateRearForward(AbstractTrain Train)
		{
			// update sections
			if (PreviousSectionIndex >= 0)
			{
				currentRoute.Sections[PreviousSectionIndex].Leave(Train);
				currentRoute.UpdateSection(PreviousSectionIndex);
			}

			if (NextSectionIndex >= 0)
			{
				currentRoute.Sections[NextSectionIndex].Enter(Train);
				currentRoute.UpdateSection(NextSectionIndex);
			}
		}
	}
}
