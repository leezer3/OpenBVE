﻿using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using SoundManager;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Is called when a train passes a station with the Pass Alarm enabled without stopping</summary>
		internal class StationPassAlarmEvent : GeneralEvent<TrainManager.Train>
		{
			internal StationPassAlarmEvent(double TrackPositionDelta)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
			}
			public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (TriggerType == EventTriggerType.FrontCarFrontAxle)
				{
					if (Direction > 0) //FIXME: This only works for routes written in the forwards direction
					{
						int d = Train.DriverCar;
						SoundsBase.SoundBuffer buffer = Train.Cars[d].Sounds.Halt.Buffer;
						if (buffer != null)
						{
							OpenBveApi.Math.Vector3 pos = Train.Cars[d].Sounds.Halt.Position;
							if (Train.Specs.PassAlarm == TrainManager.PassAlarmType.Single)
							{
								Train.Cars[d].Sounds.Halt.Source = Program.Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, d, false);
							}
							else if (Train.Specs.PassAlarm == TrainManager.PassAlarmType.Loop)
							{
								Train.Cars[d].Sounds.Halt.Source = Program.Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, d, true);
							}
						}
						this.DontTriggerAnymore = true;
					}
				}
			}
		}
		
		/// <summary>Placed at the start of every station</summary>
		internal class StationStartEvent : GeneralEvent<TrainManager.Train>
		{
			/// <summary>The index of the station this event describes</summary>
			internal readonly int StationIndex;

			internal StationStartEvent(double TrackPositionDelta, int StationIndex)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.StationIndex = StationIndex;
			}
			public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (TriggerType == EventTriggerType.TrainFront)
				{
					if (Direction < 0)
					{
						if (Train.Handles.Reverser.Actual == TrainManager.ReverserPosition.Forwards && Train.Handles.Power.Driver != 0 && !Game.MinimalisticSimulation && StationIndex == Train.Station)
						{
							//Our reverser and power are in F, but we are rolling backwards
							//Leave the station index alone, and we won't trigger again when we actually move forwards
							return;
						}
						Train.Station = -1;
					}
					else if (Direction > 0)
					{
						if (Train.Station == StationIndex || Train.NextStopSkipped != TrainManager.StopSkipMode.None)
						{
							return;
						}
						Train.Station = StationIndex;
						if (Train.StationState != TrainManager.TrainStopState.Jumping)
						{
							Train.StationState = TrainManager.TrainStopState.Pending;
						}
						Train.LastStation = this.StationIndex;
					}
				}
			}
		}
		
		/// <summary>Placed at the end of every station (as defined by the last possible stop point)</summary>
		internal class StationEndEvent : GeneralEvent<TrainManager.Train>
		{
			/// <summary>The index of the station this event describes</summary>
			internal readonly int StationIndex;

			internal StationEndEvent(double TrackPositionDelta, int StationIndex)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.StationIndex = StationIndex;
			}
			public override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (TriggerType == EventTriggerType.FrontCarFrontAxle)
				{
					if (Direction > 0)
					{
						if (Train == TrainManager.PlayerTrain)
						{
							Timetable.UpdateCustomTimetable(Game.Stations[this.StationIndex].TimetableDaytimeTexture, Game.Stations[this.StationIndex].TimetableNighttimeTexture);
						}
					}
				}
				else if (TriggerType == EventTriggerType.RearCarRearAxle)
				{
					if (Direction < 0)
					{
						Train.Station = this.StationIndex;
						if (Train.NextStopSkipped != TrainManager.StopSkipMode.None)
						{
							Train.LastStation = this.StationIndex;
						}
						Train.NextStopSkipped = TrainManager.StopSkipMode.None;
					}
					else if (Direction > 0)
					{
						if (Train.Station == StationIndex)
						{
							if (Train == TrainManager.PlayerTrain)
							{
								if (Game.Stations[StationIndex].PlayerStops() & TrainManager.PlayerTrain.StationState == TrainManager.TrainStopState.Pending)
								{
									string s = Translations.GetInterfaceString("message_station_passed");
									s = s.Replace("[name]", Game.Stations[StationIndex].Name);
									Game.AddMessage(s, MessageManager.MessageDependency.None, Interface.GameMode.Normal, MessageColor.Orange, Game.SecondsSinceMidnight + 10.0, null);
								}
								else if (Game.Stations[StationIndex].PlayerStops() & TrainManager.PlayerTrain.StationState == TrainManager.TrainStopState.Boarding)
								{
									string s = Translations.GetInterfaceString("message_station_passed_boarding");
									s = s.Replace("[name]", Game.Stations[StationIndex].Name);
									Game.AddMessage(s, MessageManager.MessageDependency.None, Interface.GameMode.Normal, MessageColor.Red, Game.SecondsSinceMidnight + 10.0, null);
								}
							}
							Train.Station = -1;
							if (Train.StationState != TrainManager.TrainStopState.Jumping)
							{
								Train.StationState = TrainManager.TrainStopState.Pending;
							}
							
							int d = Train.DriverCar;
							Program.Sounds.StopSound(Train.Cars[d].Sounds.Halt.Source);
						}
					}
				}
			}
		}
	}
}
