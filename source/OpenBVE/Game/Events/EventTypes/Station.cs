using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Is called when a train passes a station with the Pass Alarm enabled without stopping</summary>
		internal class StationPassAlarmEvent : GeneralEvent
		{
			internal StationPassAlarmEvent(double TrackPositionDelta)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
			}
			public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
			{
				if (TriggerType == EventTriggerType.FrontCarFrontAxle)
				{
					if (Direction > 0) //FIXME: This only works for routes written in the forwards direction
					{
						TrainManager.Train t = (TrainManager.Train) Train;
						t.SafetySystems.PassAlarm.Trigger();
						this.DontTriggerAnymore = true;
					}
				}
			}

			public override void Reset()
			{
				this.DontTriggerAnymore = false;
			}
		}
		
		/// <summary>Placed at the end of every station (as defined by the last possible stop point)</summary>
		internal class StationEndEvent : GeneralEvent
		{
			/// <summary>The index of the station this event describes</summary>
			internal readonly int StationIndex;

			internal StationEndEvent(double TrackPositionDelta, int StationIndex)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.StationIndex = StationIndex;
			}
			public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
			{
				if (TriggerType == EventTriggerType.FrontCarFrontAxle)
				{
					if (Direction > 0)
					{
						if (Train.IsPlayerTrain)
						{
							Timetable.UpdateCustomTimetable(Program.CurrentRoute.Stations[this.StationIndex].TimetableDaytimeTexture, Program.CurrentRoute.Stations[this.StationIndex].TimetableNighttimeTexture);
						}
					}
				}
				else if (TriggerType == EventTriggerType.RearCarRearAxle)
				{
					Train.LeaveStation(StationIndex, Direction);
				}
			}
		}
	}
}
