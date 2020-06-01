using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace RouteManager2.Events
{
	/// <summary>Called when the rail played for a train should be changed</summary>
		public class RailSoundsChangeEvent : GeneralEvent
		{
			private readonly int PreviousRunIndex;
			private readonly int PreviousFlangeIndex;
			private readonly int NextRunIndex;
			private readonly int NextFlangeIndex;
			public RailSoundsChangeEvent(double TrackPositionDelta, int PreviousRunIndex, int PreviousFlangeIndex, int NextRunIndex, int NextFlangeIndex)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.PreviousRunIndex = PreviousRunIndex;
				this.PreviousFlangeIndex = PreviousFlangeIndex;
				this.NextRunIndex = NextRunIndex;
				this.NextFlangeIndex = NextFlangeIndex;
			}
			/// <summary>Triggers a change in run and flange sounds</summary>
			/// <param name="Direction">The direction of travel- 1 for forwards, and -1 for backwards</param>
			/// <param name="TriggerType">They type of event which triggered this sound</param>
			/// <param name="Train">The root train which triggered this sound</param>
			/// <param name="Car">The car which triggered this sound</param>
			public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
			{
				if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle)
				{
					if (Direction < 0)
					{
						Car.FrontAxle.RunIndex = this.PreviousRunIndex;
						Car.FrontAxle.FlangeIndex = this.PreviousFlangeIndex;
					}
					else if (Direction > 0)
					{
						Car.FrontAxle.RunIndex = this.NextRunIndex;
						Car.FrontAxle.FlangeIndex = this.NextFlangeIndex;
					}
				}
				else if (TriggerType == EventTriggerType.RearCarRearAxle | TriggerType == EventTriggerType.OtherCarRearAxle)
				{
					if (Direction < 0)
					{
						Car.RearAxle.RunIndex = this.PreviousRunIndex;
						Car.RearAxle.FlangeIndex = this.PreviousFlangeIndex;
					}
					else if (Direction > 0)
					{
						Car.RearAxle.RunIndex = this.NextRunIndex;
						Car.RearAxle.FlangeIndex = this.NextFlangeIndex;
					}
				}
			}
		}
}
