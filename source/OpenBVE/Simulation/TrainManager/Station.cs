using System;
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
			Completed = 2,
			/// <summary>The train is jumping between stations, and all stops should be ignored</summary>
			Jumping = 3
		}

		/// <summary>Is called once a frame to update the station state for the given train</summary>
		/// <param name="Train">The train</param>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		private static void UpdateTrainStation(Train Train, double TimeElapsed)
		{
			if (Train.StationInfo.NextStation >= 0)
			{
				int i = Train.StationInfo.NextStation;
				int n = Game.GetStopIndex(Train.StationInfo.NextStation, Train.Cars.Length);
				double tf, tb;
				if (n >= 0)
				{
					double p0 = Train.Cars[0].FrontAxle.Follower.TrackPosition - Train.Cars[0].FrontAxle.Position + 0.5 * Train.Cars[0].Length;
					double p1 = Game.Stations[i].Stops[n].TrackPosition;
					tf = Game.Stations[i].Stops[n].ForwardTolerance;
					tb = Game.Stations[i].Stops[n].BackwardTolerance;
					Train.StationInfo.DistanceToStopPosition = p1 - p0;
				}
				else
				{
					Train.StationInfo.DistanceToStopPosition = 0.0;
					tf = 5.0;
					tb = 5.0;
				}
				if (Train.StationInfo.CurrentStopState == TrainStopState.Pending)
				{
					Train.StationInfo.DepartureSoundPlayed = false;
					if (Game.StopsAtStation(i, Train))
					{
						Train.StationInfo.DepartureSoundPlayed = false;
						//Check whether all doors are controlled by the driver
						if (Train.Specs.DoorOpenMode != DoorMode.Manual)
						{
							//Check that we are not moving
							if (Math.Abs(Train.Specs.CurrentAverageSpeed) < 0.1 / 3.6 &
							    Math.Abs(Train.Specs.CurrentAverageAcceleration) < 0.1 / 3.6)
							{
								//Check the interlock state for the doors
								switch (Train.Specs.DoorInterlockState)
								{
									case DoorInterlockStates.Unlocked:
										if (Game.Stations[i].OpenLeftDoors || Game.Stations[i].OpenRightDoors)
										{
											AttemptToOpenDoors(Train, i, tb, tf);
										}
										break;
									case DoorInterlockStates.Left:
										if (Game.Stations[i].OpenLeftDoors && !Game.Stations[i].OpenRightDoors)
										{
											AttemptToOpenDoors(Train, i, tb, tf);
										}
										break;
									case DoorInterlockStates.Right:
										if (!Game.Stations[i].OpenLeftDoors && Game.Stations[i].OpenRightDoors)
										{
											AttemptToOpenDoors(Train, i, tb, tf);
										}
										break;
									case DoorInterlockStates.Locked:
										//All doors are currently locked, do nothing
										break;

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
									if (Train.Cars[j].Doors[0].AnticipatedOpen)
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
									if (Train.Cars[j].Doors[1].AnticipatedOpen)
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
								Train.StationInfo.CurrentStopState = TrainStopState.Boarding;
								Train.StationInfo.AdjustStopPosition = false;
								Train.Specs.DoorClosureAttempted = false;
								Sounds.StopSound(Train.Cars[Train.DriverCar].Sounds.Halt.Source);
								Sounds.SoundBuffer buffer = Game.Stations[i].ArrivalSoundBuffer;
								if (buffer != null)
								{
									OpenBveApi.Math.Vector3 pos = Game.Stations[i].SoundOrigin;
									Sounds.PlaySound(buffer, 1.0, 1.0, pos, false);
								}
								Train.StationInfo.ExpectedArrivalTime = Game.SecondsSinceMidnight;
								Train.StationInfo.ExpectedDepartureTime = Game.Stations[i].DepartureTime - Train.TimetableDelta;
								if (Train.StationInfo.ExpectedDepartureTime - Game.SecondsSinceMidnight < Game.Stations[i].StopTime)
								{
									Train.StationInfo.ExpectedDepartureTime = Game.SecondsSinceMidnight + Game.Stations[i].StopTime;
								}
								Train.Passengers.PassengerRatio = Game.Stations[i].PassengerRatio;
								UpdateTrainMassFromPassengerRatio(Train);
								if (Train == PlayerTrain)
								{
									double early = 0.0;
									if (Game.Stations[i].ArrivalTime >= 0.0)
									{
										early = (Game.Stations[i].ArrivalTime - Train.TimetableDelta) - Train.StationInfo.ExpectedArrivalTime;
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
									if (Train.StationInfo.DistanceToStopPosition < -0.1)
									{
										s += Interface.GetInterfaceString("message_delimiter") + Interface.GetInterfaceString("message_station_overrun");
									}
									else if (Train.StationInfo.DistanceToStopPosition > 0.1)
									{
										s += Interface.GetInterfaceString("message_delimiter") + Interface.GetInterfaceString("message_station_underrun");
									}
									double d = Math.Abs(Train.StationInfo.DistanceToStopPosition);
									string c = d.ToString("0.0", Culture);
									if (Game.Stations[i].StationType == Game.StationType.Terminal)
									{
										s += Interface.GetInterfaceString("message_delimiter") + Interface.GetInterfaceString("message_station_terminal");
									}
									s = s.Replace("[name]", Game.Stations[i].Name);
									s = s.Replace("[time]", b);
									s = s.Replace("[difference]", c);
									Game.AddMessage(s, MessageManager.MessageDependency.StationArrival, Interface.GameMode.Normal, MessageColor.White, Game.SecondsSinceMidnight + 10.0, null);
									if (Game.Stations[i].StationType == Game.StationType.Normal)
									{
										s = Interface.GetInterfaceString("message_station_deadline");
										Game.AddMessage(s, MessageManager.MessageDependency.StationDeparture, Interface.GameMode.Normal, MessageColor.White, double.PositiveInfinity, null);
									}
									Timetable.UpdateCustomTimetable(Game.Stations[i].TimetableDaytimeTexture, Game.Stations[i].TimetableNighttimeTexture);
								}
								// schedule door locks (passengers stuck between the doors)
								for (int j = 0; j < Train.Cars.Length; j++)
								{
									for (int k = 0; k < Train.Cars[j].Doors.Length; k++)
									{
										Train.Cars[j].Doors[k].DoorLockDuration = 0.0;
										if (Game.Stations[i].OpenLeftDoors & Train.Cars[j].Doors[k].Direction == -1 | Game.Stations[i].OpenRightDoors & Train.Cars[j].Doors[k].Direction == 1)
										{
											double p = 0.005 * Game.Stations[i].PassengerRatio * Game.Stations[i].PassengerRatio * Game.Stations[i].PassengerRatio * Game.Stations[i].PassengerRatio;
											if (Program.RandomNumberGenerator.NextDouble() < p)
											{
												/*
												 * -- door lock at state --
												 * minimum: 0.2 (nearly closed)
												 * maximum: 0.8 (nearly opened)
												 * */
												Train.Cars[j].Doors[k].DoorLockState = 0.2 + 0.6 * Program.RandomNumberGenerator.NextDouble();
												/* -- waiting time --
												 * minimum: 2.9 s
												 * maximum: 40.0 s
												 * average: 7.6 s
												 * */
												p = Program.RandomNumberGenerator.NextDouble();
												Train.Cars[j].Doors[k].DoorLockDuration = (50.0 - 10.0 * p) / (17.0 - 16.0 * p);
											}
										}
									}
								}
							}
							else if (Train.Specs.CurrentAverageSpeed > -0.277777777777778 & Train.Specs.CurrentAverageSpeed < 0.277777777777778)
							{
								// correct stop position
								if (!Train.StationInfo.AdjustStopPosition & (Train.StationInfo.DistanceToStopPosition > tb | Train.StationInfo.DistanceToStopPosition < -tf))
								{
									Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.Adjust.Buffer;
									if (buffer != null)
									{
										OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.Adjust.Position;
										Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
									}
									if (Train == TrainManager.PlayerTrain)
									{
										Game.AddMessage(Interface.GetInterfaceString("message_station_correct"), MessageManager.MessageDependency.None, Interface.GameMode.Normal, MessageColor.Orange, Game.SecondsSinceMidnight + 5.0, null);
									}
									Train.StationInfo.AdjustStopPosition= true;
								}
							}
							else
							{
								Train.StationInfo.AdjustStopPosition = false;
							}
						}
					}
				}
				else if (Train.StationInfo.CurrentStopState == TrainStopState.Boarding)
				{
					//Check whether all doors are controlled by the driver, and whether this is a non-standard station type
					//e.g. Change ends
					if (Train.Specs.DoorOpenMode != DoorMode.Manual & Game.Stations[i].StationType == Game.StationType.Normal)
					{
						//Check the interlock state for the doors
						switch (Train.Specs.DoorInterlockState)
						{
							case DoorInterlockStates.Unlocked:
								AttemptToCloseDoors(Train);
								break;
							case DoorInterlockStates.Left:
								if (Game.Stations[i].OpenLeftDoors)
								{
									AttemptToCloseDoors(Train);
								}
								break;
							case DoorInterlockStates.Right:
								if (Game.Stations[i].OpenRightDoors)
								{
									AttemptToCloseDoors(Train);
								}
								break;
							case DoorInterlockStates.Locked:
								//All doors are currently locked, do nothing
								break;
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
								for (int k = 0; k < Train.Cars[j].Doors.Length; k++)
								{
									if (Train.Cars[j].Doors[k].State != 0.0)
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
								for (int k = 0; k < Train.Cars[j].Doors.Length; k++)
								{
									if (Train.Cars[j].Doors[k].State != 0.0)
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
						if (Game.SecondsSinceMidnight > Train.StationInfo.ExpectedDepartureTime && (Game.Stations[i].StationType != Game.StationType.Terminal || Train != PlayerTrain))
						{
							Train.StationInfo.CurrentStopState = TrainStopState.Completed;
							if (Train == PlayerTrain & Game.Stations[i].StationType == Game.StationType.Normal)
							{
								if (!Game.Stations[i].OpenLeftDoors & !Game.Stations[i].OpenRightDoors | Train.Specs.DoorCloseMode != DoorMode.Manual)
								{
									Game.AddMessage(Interface.GetInterfaceString("message_station_depart"), MessageManager.MessageDependency.None, Interface.GameMode.Normal, MessageColor.White, Game.SecondsSinceMidnight + 5.0, null);
								}
								else
								{
									Game.AddMessage(Interface.GetInterfaceString("message_station_depart_closedoors"), MessageManager.MessageDependency.None, Interface.GameMode.Normal, MessageColor.White, Game.SecondsSinceMidnight + 5.0, null);
								}
							}
							else if (Game.Stations[i].StationType == Game.StationType.ChangeEnds)
							{
								//Game.AddMessage("CHANGE ENDS", MessageManager.MessageDependency.None, Interface.GameMode.Expert, MessageColor.Magenta, Game.SecondsSinceMidnight + 5.0);
								JumpTrain(Train, i + 1);
							}
						}
						if (Interface.CurrentOptions.LoadingSway)
						{
							// passengers boarding
							for (int j = 0; j < Train.Cars.Length; j++)
							{
								double r = 2.0 * Game.Stations[i].PassengerRatio * TimeElapsed;
								if (r >= Program.RandomNumberGenerator.NextDouble())
								{
									int d =
										(int) Math.Floor(Program.RandomNumberGenerator.NextDouble() * (double) Train.Cars[j].Doors.Length);
									if (Train.Cars[j].Doors[d].State == 1.0)
									{
										Train.Cars[j].Specs.CurrentRollShakeDirection += (double) Train.Cars[j].Doors[d].Direction;
									}
								}
							}
						}
					}
					else
					{
						Train.StationInfo.CurrentStopState = TrainStopState.Completed;
						if (Train == PlayerTrain & Game.Stations[i].StationType == Game.StationType.Normal)
						{
							Game.AddMessage(Interface.GetInterfaceString("message_station_depart"), MessageManager.MessageDependency.None, Interface.GameMode.Normal, MessageColor.White, Game.SecondsSinceMidnight + 5.0, null);
						}
					}
					// departure sound
					if (!Train.StationInfo.DepartureSoundPlayed)
					{
						Sounds.SoundBuffer buffer = Game.Stations[i].DepartureSoundBuffer;
						if (buffer != null)
						{
							double dur = Sounds.GetDuration(buffer);
							if (Game.SecondsSinceMidnight >= Train.StationInfo.ExpectedDepartureTime - dur)
							{
								Sounds.PlaySound(buffer, 1.0, 1.0, Game.Stations[i].SoundOrigin, false);
								Train.StationInfo.DepartureSoundPlayed = true;
							}
						}
					}
				}
			}
			else
			{
				Train.StationInfo.CurrentStopState = TrainStopState.Pending;
			}
			// automatically close doors
			if (Train.Specs.DoorCloseMode != DoorMode.Manual & Train.Specs.DoorInterlockState != DoorInterlockStates.Locked & !Train.Specs.DoorClosureAttempted)
			{
				if (Train.StationInfo.NextStation == -1 | Train.StationInfo.CurrentStopState == TrainStopState.Completed)
				{
					if ((GetDoorsState(Train, true, true) & TrainDoorState.AllClosed) == 0)
					{
						CloseTrainDoors(Train, true, true);
						Train.Specs.DoorClosureAttempted = true;
					}
				}
			}
		}
	}
}
