﻿using System;
using OpenBveApi.Colors;

namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>The possible states for a station stop</summary>
		internal enum TrainStopState
		{
			/// <summary>The stop is still pending</summary>
			Pending = 0, 
			/// <summary>The train is currrently stopped and passengers are boarding</summary>
			Boarding = 1, 
			/// <summary>The stop has been completed, and the train is preparing to depart</summary>
			Completed = 2
		}

		/// <summary>Is called once a frame to update the station state for the given train</summary>
		/// <param name="Train">The train</param>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		private static void UpdateTrainStation(Train Train, double TimeElapsed)
		{
			if (Train.Station >= 0)
			{
				int i = Train.Station;
				int n = Game.GetStopIndex(Train.Station, Train.Cars.Length);
				double tf, tb;
				if (n >= 0)
				{
					double p0 = Train.Cars[0].FrontAxle.Follower.TrackPosition - Train.Cars[0].FrontAxlePosition + 0.5 * Train.Cars[0].Length;
					double p1 = Game.Stations[i].Stops[n].TrackPosition;
					tf = Game.Stations[i].Stops[n].ForwardTolerance;
					tb = Game.Stations[i].Stops[n].BackwardTolerance;
					Train.StationDistanceToStopPoint = p1 - p0;
				}
				else
				{
					Train.StationDistanceToStopPoint = 0.0;
					tf = 5.0;
					tb = 5.0;
				}
				if (Train.StationState == TrainStopState.Pending)
				{
					Train.StationDepartureSoundPlayed = false;
					if (Game.StopsAtStation(i, Train))
					{
						Train.StationDepartureSoundPlayed = false;
						// automatically open doors
						if (Train.Specs.DoorOpenMode != DoorMode.Manual)
						{
							if ((GetDoorsState(Train, Game.Stations[i].OpenLeftDoors, Game.Stations[i].OpenRightDoors) & TrainDoorState.AllOpened) == 0)
							{
								if (Math.Abs(Train.Specs.CurrentAverageSpeed) < 0.1 / 3.6 & Math.Abs(Train.Specs.CurrentAverageAcceleration) < 0.1 / 3.6)
								{
									if (Train.StationDistanceToStopPoint < tb & -Train.StationDistanceToStopPoint < tf)
									{
										OpenTrainDoors(Train, Game.Stations[i].OpenLeftDoors, Game.Stations[i].OpenRightDoors);
									}
								}
							}
						}
						// detect arrival
						if (Train.Specs.CurrentAverageSpeed > -0.277777777777778 & Train.Specs.CurrentAverageSpeed < 0.277777777777778)
						{
							bool left, right;
							if (Game.Stations[i].OpenLeftDoors)
							{
								left = false;
								for (int j = 0; j < Train.Cars.Length; j++)
								{
									if (Train.Cars[j].Specs.AnticipatedLeftDoorsOpened)
									{
										left = true; break;
									}
								}
							}
							else
							{
								left = true;
							}
							if (Game.Stations[i].OpenRightDoors)
							{
								right = false;
								for (int j = 0; j < Train.Cars.Length; j++)
								{
									if (Train.Cars[j].Specs.AnticipatedRightDoorsOpened)
									{
										right = true; break;
									}
								}
							}
							else
							{
								right = true;
							}
							if (left & right)
							{
								// arrival
								Train.StationState = TrainStopState.Boarding;
								Train.StationAdjust = false;
								Sounds.StopSound(Train.Cars[Train.DriverCar].Sounds.Halt.Source);
								Sounds.SoundBuffer buffer = Game.Stations[i].ArrivalSoundBuffer;
								if (buffer != null)
								{
									OpenBveApi.Math.Vector3 pos = Game.Stations[i].SoundOrigin;
									Sounds.PlaySound(buffer, 1.0, 1.0, pos, false);
								}
								Train.StationArrivalTime = Game.SecondsSinceMidnight;
								Train.StationDepartureTime = Game.Stations[i].DepartureTime - Train.TimetableDelta;
								if (Train.StationDepartureTime - Game.SecondsSinceMidnight < Game.Stations[i].StopTime)
								{
									Train.StationDepartureTime = Game.SecondsSinceMidnight + Game.Stations[i].StopTime;
								}
								Train.Passengers.PassengerRatio = Game.Stations[i].PassengerRatio;
								UpdateTrainMassFromPassengerRatio(Train);
								if (Train == PlayerTrain)
								{
									double early = 0.0;
									if (Game.Stations[i].ArrivalTime >= 0.0)
									{
										early = (Game.Stations[i].ArrivalTime - Train.TimetableDelta) - Train.StationArrivalTime;
									}
									string s;
									if (early < -1.0)
									{
										s = Interface.GetInterfaceString("message_station_arrival_late");
									}
									else if (early > 1.0)
									{
										s = Interface.GetInterfaceString("message_station_arrival_early");
									}
									else
									{
										s = Interface.GetInterfaceString("message_station_arrival");
									}
									System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
									TimeSpan a = TimeSpan.FromSeconds(Math.Abs(early));
									string b = a.Hours.ToString("00", Culture) + ":" + a.Minutes.ToString("00", Culture) + ":" + a.Seconds.ToString("00", Culture);
									if (Train.StationDistanceToStopPoint < -0.1)
									{
										s += Interface.GetInterfaceString("message_delimiter") + Interface.GetInterfaceString("message_station_overrun");
									}
									else if (Train.StationDistanceToStopPoint > 0.1)
									{
										s += Interface.GetInterfaceString("message_delimiter") + Interface.GetInterfaceString("message_station_underrun");
									}
									double d = Math.Abs(Train.StationDistanceToStopPoint);
									string c = d.ToString("0.0", Culture);
									if (Game.Stations[i].StationType == Game.StationType.Terminal)
									{
										s += Interface.GetInterfaceString("message_delimiter") + Interface.GetInterfaceString("message_station_terminal");
									}
									s = s.Replace("[name]", Game.Stations[i].Name);
									s = s.Replace("[time]", b);
									s = s.Replace("[difference]", c);
									Game.AddMessage(s, Game.MessageDependency.None, Interface.GameMode.Normal, MessageColor.Blue, Game.SecondsSinceMidnight + 10.0);
									if (Game.Stations[i].StationType == Game.StationType.Normal)
									{
										s = Interface.GetInterfaceString("message_station_deadline");
										Game.AddMessage(s, Game.MessageDependency.Station, Interface.GameMode.Normal, MessageColor.Blue, double.PositiveInfinity);
									}
									Timetable.UpdateCustomTimetable(Game.Stations[i].TimetableDaytimeTexture, Game.Stations[i].TimetableNighttimeTexture);
								}
								// schedule door locks (passengers stuck between the doors)
								for (int j = 0; j < Train.Cars.Length; j++)
								{
									for (int k = 0; k < Train.Cars[j].Specs.Doors.Length; k++)
									{
										Train.Cars[j].Specs.Doors[k].DoorLockDuration = 0.0;
										if (Game.Stations[i].OpenLeftDoors & Train.Cars[j].Specs.Doors[k].Direction == -1 | Game.Stations[i].OpenRightDoors & Train.Cars[j].Specs.Doors[k].Direction == 1)
										{
											double p = 0.005 * Game.Stations[i].PassengerRatio * Game.Stations[i].PassengerRatio * Game.Stations[i].PassengerRatio * Game.Stations[i].PassengerRatio;
											if (Program.RandomNumberGenerator.NextDouble() < p)
											{
												/*
												 * -- door lock at state --
												 * minimum: 0.2 (nearly closed)
												 * maximum: 0.8 (nearly opened)
												 * */
												Train.Cars[j].Specs.Doors[k].DoorLockState = 0.2 + 0.6 * Program.RandomNumberGenerator.NextDouble();
												/* -- waiting time --
												 * minimum: 2.9 s
												 * maximum: 40.0 s
												 * average: 7.6 s
												 * */
												p = Program.RandomNumberGenerator.NextDouble();
												Train.Cars[j].Specs.Doors[k].DoorLockDuration = (50.0 - 10.0 * p) / (17.0 - 16.0 * p);
											}
										}
									}
								}
							}
							else if (Train.Specs.CurrentAverageSpeed > -0.277777777777778 & Train.Specs.CurrentAverageSpeed < 0.277777777777778)
							{
								// correct stop position
								if (!Train.StationAdjust & (Train.StationDistanceToStopPoint > tb | Train.StationDistanceToStopPoint < -tf))
								{
									Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.Adjust.Buffer;
									if (buffer != null)
									{
										OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.Adjust.Position;
										Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
									}
									if (Train == TrainManager.PlayerTrain)
									{
										Game.AddMessage(Interface.GetInterfaceString("message_station_correct"), Game.MessageDependency.None, Interface.GameMode.Normal, MessageColor.Orange, Game.SecondsSinceMidnight + 5.0);
									}
									Train.StationAdjust = true;
								}
							}
							else
							{
								Train.StationAdjust = false;
							}
						}
					}
				}
				else if (Train.StationState == TrainStopState.Boarding)
				{
					// automatically close doors
					if (Train.Specs.DoorCloseMode != DoorMode.Manual & Game.Stations[i].StationType == Game.StationType.Normal)
					{
						if (Game.SecondsSinceMidnight >= Train.StationDepartureTime - 1.0 / Train.Cars[Train.DriverCar].Specs.DoorCloseFrequency)
						{
							if ((GetDoorsState(Train, true, true) & TrainDoorState.AllClosed) == 0)
							{
								CloseTrainDoors(Train, true, true);
							}
						}
					}
					// detect departure
					bool left, right;
					if (!Game.Stations[i].OpenLeftDoors & !Game.Stations[i].OpenRightDoors)
					{
						left = true;
						right = true;
					}
					else
					{
						if (Game.Stations[i].OpenLeftDoors)
						{
							left = false;
							for (int j = 0; j < Train.Cars.Length; j++)
							{
								for (int k = 0; k < Train.Cars[j].Specs.Doors.Length; k++)
								{
									if (Train.Cars[j].Specs.Doors[k].State != 0.0)
									{
										left = true; break;
									}
								} if (left) break;
							}
						}
						else
						{
							left = false;
						}
						if (Game.Stations[i].OpenRightDoors)
						{
							right = false;
							for (int j = 0; j < Train.Cars.Length; j++)
							{
								for (int k = 0; k < Train.Cars[j].Specs.Doors.Length; k++)
								{
									if (Train.Cars[j].Specs.Doors[k].State != 0.0)
									{
										right = true; break;
									}
								} if (right) break;
							}
						}
						else
						{
							right = false;
						}
					}
					if (left | right)
					{
						// departure message
						if (Game.SecondsSinceMidnight > Train.StationDepartureTime)
						{
							Train.StationState = TrainStopState.Completed;
							if (Train == PlayerTrain & Game.Stations[i].StationType == Game.StationType.Normal)
							{
								if (!Game.Stations[i].OpenLeftDoors & !Game.Stations[i].OpenRightDoors | Train.Specs.DoorCloseMode != DoorMode.Manual)
								{
									Game.AddMessage(Interface.GetInterfaceString("message_station_depart"), Game.MessageDependency.None, Interface.GameMode.Normal, MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
								}
								else
								{
									Game.AddMessage(Interface.GetInterfaceString("message_station_depart_closedoors"), Game.MessageDependency.None, Interface.GameMode.Normal, MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
								}
							}
							else if (Game.Stations[i].StationType == Game.StationType.ChangeEnds)
							{
								//Game.AddMessage("CHANGE ENDS", Game.MessageDependency.None, Interface.GameMode.Expert, MessageColor.Magenta, Game.SecondsSinceMidnight + 5.0);
								JumpTrain(Train, i + 1);
							}
						}
						// passengers boarding
						for (int j = 0; j < Train.Cars.Length; j++)
						{
							double r = 2.0 * Game.Stations[i].PassengerRatio * TimeElapsed;
							if (r >= Program.RandomNumberGenerator.NextDouble())
							{
								int d = (int)Math.Floor(Program.RandomNumberGenerator.NextDouble() * (double)Train.Cars[j].Specs.Doors.Length);
								if (Train.Cars[j].Specs.Doors[d].State == 1.0)
								{
									Train.Cars[j].Specs.CurrentRollShakeDirection += (double)Train.Cars[j].Specs.Doors[d].Direction;
								}
							}
						}
					}
					else
					{
						Train.StationState = TrainStopState.Completed;
						if (Train == PlayerTrain & Game.Stations[i].StationType == Game.StationType.Normal)
						{
							Game.AddMessage(Interface.GetInterfaceString("message_station_depart"), Game.MessageDependency.None, Interface.GameMode.Normal, MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
						}
					}
					// departure sound
					if (!Train.StationDepartureSoundPlayed)
					{
						Sounds.SoundBuffer buffer = Game.Stations[i].DepartureSoundBuffer;
						if (buffer != null)
						{
							double dur = Sounds.GetDuration(buffer);
							if (Game.SecondsSinceMidnight >= Train.StationDepartureTime - dur)
							{
								Sounds.PlaySound(buffer, 1.0, 1.0, Game.Stations[i].SoundOrigin, false);
								Train.StationDepartureSoundPlayed = true;
							}
						}
					}
				}
			}
			else
			{
				Train.StationState = TrainStopState.Pending;
			}
			// automatically close doors
			if (Train.Specs.DoorCloseMode == DoorMode.Automatic)
			{
				if (Train.Station == -1 | Train.StationState == TrainStopState.Completed)
				{
					if ((GetDoorsState(Train, true, true) & TrainDoorState.AllClosed) == 0)
					{
						CloseTrainDoors(Train, true, true);
					}
				}
			}
		}
	}
}
