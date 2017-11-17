using System.Linq;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Is called when a marker or message is added to the in-game display</summary>
		internal class MarkerStartEvent : GeneralEvent
		{
			/// <summary>The marker or message to add</summary>
			private readonly MessageManager.Message Message;

			internal MarkerStartEvent(double trackPositionDelta, MessageManager.Message message)
			{
				this.TrackPositionDelta = trackPositionDelta;
				this.DontTriggerAnymore = false;
				this.Message = message;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (Train == TrainManager.PlayerTrain & TriggerType == EventTriggerType.FrontCarFrontAxle)
				{
					if (this.Message != null)
					{
						if (Direction < 0)
						{
							this.Message.QueueForRemoval = true;
						}
						else if (Direction > 0)
						{
							if (this.Message.Trains != null && !this.Message.Trains.Contains(new System.IO.DirectoryInfo(Game.RouteInformation.TrainFolder).Name))
							{
								//Our train is NOT in the list of trains which this message triggers for
								return;
							}
							MessageManager.AddMessage(this.Message);

						}
					}

				}
			}
		}
		
		/// <summary>Is calld when a marker or message is removed from the in-game display</summary>
		internal class MarkerEndEvent : GeneralEvent
		{
			/// <summary>The marker or message to remove (Note: May have already timed-out)</summary>
			internal MessageManager.Message Message;

			internal MarkerEndEvent(double trackPositionDelta, MessageManager.Message message)
			{
				this.TrackPositionDelta = trackPositionDelta;
				this.DontTriggerAnymore = false;
				this.Message = message;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (Train == TrainManager.PlayerTrain & TriggerType == EventTriggerType.FrontCarFrontAxle)
				{
					if (this.Message != null)
					{
						if (Direction < 0)
						{
							if (this.Message.Trains != null && !this.Message.Trains.Contains(new System.IO.DirectoryInfo(Game.RouteInformation.TrainFolder).Name))
							{
								//Our train is NOT in the list of trains which this message triggers for
								return;
							}
							MessageManager.AddMessage(this.Message);
						}
						else if (Direction > 0)
						{
							this.Message.QueueForRemoval = true;
						}
					}
				}
			}
		}
	}
}
