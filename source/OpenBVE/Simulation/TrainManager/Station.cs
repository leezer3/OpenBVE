using System;
using OpenBve.RouteManager;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Runtime;
using OpenBveApi.Interface;
using OpenBveApi.Trains;
using SoundManager;

namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>Is called once a frame to update the station state for the given train</summary>
		/// <param name="Train">The train</param>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		private static void UpdateTrainStation(Train Train, double TimeElapsed)
		{
			if (Train.Station >= 0)
			{
				int i = Train.Station;
				int n = CurrentRoute.Stations[Train.Station].GetStopIndex(Train.NumberOfCars);
				double tf, tb;
				if (n >= 0)
				{
					double p0 = Train.Cars[0].FrontAxle.Follower.TrackPosition - Train.Cars[0].FrontAxle.Position + 0.5 * Train.Cars[0].Length;
					double p1 = CurrentRoute.Stations[i].Stops[n].TrackPosition;
					tf = CurrentRoute.Stations[i].Stops[n].ForwardTolerance;
					tb = CurrentRoute.Stations[i].Stops[n].BackwardTolerance;
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
						//Check whether all doors are controlled by the driver
						if (Train.Specs.DoorOpenMode != DoorMode.Manual)
						{
							//Check that we are not moving
							if (Math.Abs(Train.CurrentSpeed) < 0.1 / 3.6 &
							    Math.Abs(Train.Specs.CurrentAverageAcceleration) < 0.1 / 3.6)
							{
								//Check the interlock state for the doors
								switch (Train.Specs.DoorInterlockState)
								{
									case DoorInterlockStates.Unlocked:
										if (CurrentRoute.Stations[i].OpenLeftDoors || CurrentRoute.Stations[i].OpenRightDoors)
										{
											AttemptToOpenDoors(Train, i, tb, tf);
										}
										break;
									case DoorInterlockStates.Left:
										if (CurrentRoute.Stations[i].OpenLeftDoors && !CurrentRoute.Stations[i].OpenRightDoors)
										{
											AttemptToOpenDoors(Train, i, tb, tf);
										}
										break;
									case DoorInterlockStates.Right:
										if (!CurrentRoute.Stations[i].OpenLeftDoors && CurrentRoute.Stations[i].OpenRightDoors)
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
						if (Train.CurrentSpeed > -0.277777777777778 & Train.CurrentSpeed < 0.277777777777778)
						{
							bool left, right;
							if (CurrentRoute.Stations[i].OpenLeftDoors)
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
							if (CurrentRoute.Stations[i].OpenRightDoors)
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
								Train.StationState = TrainStopState.Boarding;
								Train.StationAdjust = false;
								Train.Specs.DoorClosureAttempted = false;
								Program.Sounds.StopSound(Train.Cars[Train.DriverCar].Sounds.Halt);
								SoundBuffer buffer = (SoundBuffer)CurrentRoute.Stations[i].ArrivalSoundBuffer;
								if (buffer != null)
								{
									OpenBveApi.Math.Vector3 pos = CurrentRoute.Stations[i].SoundOrigin;
									Program.Sounds.PlaySound(buffer, 1.0, 1.0, pos, false);
								}
								Train.StationArrivalTime = Game.SecondsSinceMidnight;
								Train.StationDepartureTime = CurrentRoute.Stations[i].DepartureTime - Train.TimetableDelta;
								if (Train.StationDepartureTime - Game.SecondsSinceMidnight < CurrentRoute.Stations[i].StopTime)
								{
									Train.StationDepartureTime = Game.SecondsSinceMidnight + CurrentRoute.Stations[i].StopTime;
								}
								Train.Passengers.PassengerRatio = CurrentRoute.Stations[i].PassengerRatio;
								UpdateTrainMassFromPassengerRatio(Train);
								if (Train.IsPlayerTrain)
								{
									double early = 0.0;
									if (CurrentRoute.Stations[i].ArrivalTime >= 0.0)
									{
										early = (CurrentRoute.Stations[i].ArrivalTime - Train.TimetableDelta) - Train.StationArrivalTime;
									}
									string s;
									if (early < -1.0)
									{
										s = Translations.GetInterfaceString("message_station_arrival_late");
									}
									else if (early > 1.0)
									{
										s = Translations.GetInterfaceString("message_station_arrival_early");
									}
									else
									{
										s = Translations.GetInterfaceString("message_station_arrival");
									}
									System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
									TimeSpan a = TimeSpan.FromSeconds(Math.Abs(early));
									string b = a.Hours.ToString("00", Culture) + ":" + a.Minutes.ToString("00", Culture) + ":" + a.Seconds.ToString("00", Culture);
									if (Train.StationDistanceToStopPoint < -0.1)
									{
										s += Translations.GetInterfaceString("message_delimiter") + Translations.GetInterfaceString("message_station_overrun");
									}
									else if (Train.StationDistanceToStopPoint > 0.1)
									{
										s += Translations.GetInterfaceString("message_delimiter") + Translations.GetInterfaceString("message_station_underrun");
									}
									double d = Math.Abs(Train.StationDistanceToStopPoint);
									string c = d.ToString("0.0", Culture);
									if (CurrentRoute.Stations[i].Type == StationType.Terminal)
									{
										s += Translations.GetInterfaceString("message_delimiter") + Translations.GetInterfaceString("message_station_terminal");
									}
									s = s.Replace("[name]", CurrentRoute.Stations[i].Name);
									s = s.Replace("[time]", b);
									s = s.Replace("[difference]", c);
									Game.AddMessage(s, MessageDependency.StationArrival, GameMode.Normal, MessageColor.White, Game.SecondsSinceMidnight + 10.0, null);
									if (CurrentRoute.Stations[i].Type == StationType.Normal)
									{
										s = Translations.GetInterfaceString("message_station_deadline");
										Game.AddMessage(s, MessageDependency.StationDeparture, GameMode.Normal, MessageColor.White, double.PositiveInfinity, null);
									}
									Timetable.UpdateCustomTimetable(CurrentRoute.Stations[i].TimetableDaytimeTexture, CurrentRoute.Stations[i].TimetableNighttimeTexture);
								}
								// schedule door locks (passengers stuck between the doors)
								for (int j = 0; j < Train.Cars.Length; j++)
								{
									for (int k = 0; k < Train.Cars[j].Doors.Length; k++)
									{
										Train.Cars[j].Doors[k].DoorLockDuration = 0.0;
										if (CurrentRoute.Stations[i].OpenLeftDoors & Train.Cars[j].Doors[k].Direction == -1 | CurrentRoute.Stations[i].OpenRightDoors & Train.Cars[j].Doors[k].Direction == 1)
										{
											double p = 0.005 * CurrentRoute.Stations[i].PassengerRatio * CurrentRoute.Stations[i].PassengerRatio * CurrentRoute.Stations[i].PassengerRatio * CurrentRoute.Stations[i].PassengerRatio;
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
							else if (Train.CurrentSpeed > -0.277777777777778 & Train.CurrentSpeed < 0.277777777777778)
							{
								// correct stop position
								if (!Train.StationAdjust & (Train.StationDistanceToStopPoint > tb | Train.StationDistanceToStopPoint < -tf))
								{
									SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.Adjust.Buffer;
									if (buffer != null)
									{
										OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.Adjust.Position;
										Program.Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train.Cars[Train.DriverCar], false);
									}
									if (Train.IsPlayerTrain)
									{
										Game.AddMessage(Translations.GetInterfaceString("message_station_correct"), MessageDependency.None, GameMode.Normal, MessageColor.Orange, Game.SecondsSinceMidnight + 5.0, null);
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
					for (int j = 0; j < Train.Cars.Length; j++)
					{
						if (GetDoorsState(Train, j, CurrentRoute.Stations[i].OpenLeftDoors, CurrentRoute.Stations[i].OpenRightDoors) == (TrainDoorState.Opened | TrainDoorState.AllOpened))
						{
							//Check whether all doors are controlled by the driver, and whether this is a non-standard station type
							//e.g. Change ends
							if (Train.Specs.DoorCloseMode != DoorMode.Manual & CurrentRoute.Stations[i].Type == StationType.Normal)
							{
								//Check the interlock state for the doors
								switch (Train.Specs.DoorInterlockState)
								{
									case DoorInterlockStates.Unlocked:
										AttemptToCloseDoors(Train);
										break;
									case DoorInterlockStates.Left:
										if (CurrentRoute.Stations[i].OpenLeftDoors)
										{
											AttemptToCloseDoors(Train);
										}
										break;
									case DoorInterlockStates.Right:
										if (CurrentRoute.Stations[i].OpenRightDoors)
										{
											AttemptToCloseDoors(Train);
										}
										break;
									case DoorInterlockStates.Locked:
										//All doors are currently locked, do nothing
										break;
								}

								if (Train.Specs.DoorInterlockState != DoorInterlockStates.Locked & Train.Specs.DoorClosureAttempted)
								{
									if (CurrentRoute.Stations[i].OpenLeftDoors && !Train.Cars[j].Doors[0].AnticipatedReopen && Program.RandomNumberGenerator.NextDouble() < CurrentRoute.Stations[i].ReopenDoor)
									{
										Train.Cars[j].Doors[0].ReopenLimit = Program.RandomNumberGenerator.Next(1, CurrentRoute.Stations[i].ReopenStationLimit);
										Train.Cars[j].Doors[0].ReopenCounter = 0;
										Train.Cars[j].Doors[0].InterferingObjectRate = Program.RandomNumberGenerator.Next(1, CurrentRoute.Stations[i].MaxInterferingObjectRate) * 0.01;
										if (Train.Cars[j].Doors[0].InterferingObjectRate * Train.Specs.DoorWidth >= Train.Specs.DoorMaxTolerance)
										{
											Train.Cars[j].Doors[0].AnticipatedReopen = true;
										}
									}
									if (CurrentRoute.Stations[i].OpenRightDoors && !Train.Cars[j].Doors[1].AnticipatedReopen && Program.RandomNumberGenerator.NextDouble() < CurrentRoute.Stations[i].ReopenDoor)
									{
										Train.Cars[j].Doors[1].ReopenLimit = Program.RandomNumberGenerator.Next(1, CurrentRoute.Stations[i].ReopenStationLimit);
										Train.Cars[j].Doors[1].ReopenCounter = 0;
										Train.Cars[j].Doors[1].InterferingObjectRate = Program.RandomNumberGenerator.Next(1, CurrentRoute.Stations[i].MaxInterferingObjectRate) * 0.01;
										if (Train.Cars[j].Doors[1].InterferingObjectRate * Train.Specs.DoorWidth >= Train.Specs.DoorMaxTolerance)
										{
											Train.Cars[j].Doors[1].AnticipatedReopen = true;
										}
									}
								}
							}
						}
					}
					// detect departure
					bool left, right;
					if (!CurrentRoute.Stations[i].OpenLeftDoors & !CurrentRoute.Stations[i].OpenRightDoors)
					{
						left = true;
						right = true;
					}
					else
					{
						if (CurrentRoute.Stations[i].OpenLeftDoors)
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
						if (CurrentRoute.Stations[i].OpenRightDoors)
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
					// departure sound
					if (!Train.StationDepartureSoundPlayed)
					{
						SoundBuffer buffer = (SoundBuffer)CurrentRoute.Stations[i].DepartureSoundBuffer;
						if (buffer != null)
						{
							double dur = Program.Sounds.GetDuration(buffer);
							if (Game.SecondsSinceMidnight >= Train.StationDepartureTime - dur)
							{
								Program.Sounds.PlaySound(buffer, 1.0, 1.0, CurrentRoute.Stations[i].SoundOrigin, false);
								Train.StationDepartureSoundPlayed = true;
							}
						}
					}
					for (int j = 0; j < Train.Cars.Length; j++)
					{
						if (Train.Cars[j].Doors[0].AnticipatedReopen && Train.Cars[j].Doors[0].State == Train.Cars[j].Doors[0].InterferingObjectRate)
						{
							if (Train.Cars[j].Doors[0].NextReopenTime == 0.0)
							{
								Train.Cars[j].Doors[0].NextReopenTime = Game.SecondsSinceMidnight + CurrentRoute.Stations[i].InterferenceInDoor;
							}
							else if (Train.Cars[j].Doors[0].ReopenCounter < Train.Cars[j].Doors[0].ReopenLimit)
							{
								if (Game.SecondsSinceMidnight >= Train.Cars[j].Doors[0].NextReopenTime)
								{
									OpenTrainDoors(Train, j, true, false);
								}
							}
							else
							{
								Train.Cars[j].Doors[0].AnticipatedReopen = false;
							}
						}
						if (Train.Cars[j].Doors[1].AnticipatedReopen && Train.Cars[j].Doors[1].State == Train.Cars[j].Doors[1].InterferingObjectRate)
						{
							if (Train.Cars[j].Doors[1].NextReopenTime == 0.0)
							{
								Train.Cars[j].Doors[1].NextReopenTime = Game.SecondsSinceMidnight + CurrentRoute.Stations[i].InterferenceInDoor;
							}
							else if (Train.Cars[j].Doors[1].ReopenCounter < Train.Cars[j].Doors[1].ReopenLimit)
							{
								if (Game.SecondsSinceMidnight >= Train.Cars[j].Doors[1].NextReopenTime)
								{
									OpenTrainDoors(Train, j, false, true);
								}
							}
							else
							{
								Train.Cars[j].Doors[1].AnticipatedReopen = false;
							}
						}
					}
					TrainDoorState doorState = GetDoorsState(Train, CurrentRoute.Stations[i].OpenLeftDoors, CurrentRoute.Stations[i].OpenRightDoors);
					if (left | right) 
					{
						/*
						 * Assume that passengers only board at a scheduled stop
						 * If the player has opened the doors somewhere else (lineside?)
						 * then passengers should not be boarding
						 */
						if(doorState != TrainDoorState.AllClosed && Interface.CurrentOptions.LoadingSway)
						{
							// passengers boarding
							for (int j = 0; j < Train.Cars.Length; j++)
							{
								if (!Train.Cars[j].EnableLoadingSway) continue;
								double r = 2.0 * CurrentRoute.Stations[i].PassengerRatio * TimeElapsed;
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
					if (Train.Specs.DoorCloseMode == DoorMode.Manual || doorState == TrainDoorState.None || doorState == (TrainDoorState.Closed | TrainDoorState.AllClosed))
					{
						if (left | right)
						{
							// departure message
							if (Game.SecondsSinceMidnight > Train.StationDepartureTime && (CurrentRoute.Stations[i].Type != StationType.Terminal || Train != PlayerTrain))
							{
								Train.StationState = TrainStopState.Completed;
								if (Train.IsPlayerTrain & CurrentRoute.Stations[i].Type == StationType.Normal)
								{
									if (!CurrentRoute.Stations[i].OpenLeftDoors & !CurrentRoute.Stations[i].OpenRightDoors | Train.Specs.DoorCloseMode != DoorMode.Manual)
									{
										Game.AddMessage(Translations.GetInterfaceString("message_station_depart"), MessageDependency.None, GameMode.Normal, MessageColor.White, Game.SecondsSinceMidnight + 5.0, null);
									}
									else
									{
										Game.AddMessage(Translations.GetInterfaceString("message_station_depart_closedoors"), MessageDependency.None, GameMode.Normal, MessageColor.White, Game.SecondsSinceMidnight + 5.0, null);
									}
								}
								else if (CurrentRoute.Stations[i].Type == StationType.ChangeEnds)
								{
									JumpTrain(Train, i + 1);
								}
							}
						}
						else
						{
							Train.StationState = TrainStopState.Completed;
							if (Train.IsPlayerTrain & CurrentRoute.Stations[i].Type == StationType.Normal)
							{
								Game.AddMessage(Translations.GetInterfaceString("message_station_depart"), MessageDependency.None, GameMode.Normal, MessageColor.White, Game.SecondsSinceMidnight + 5.0, null);
							}
						}
					}
				}
			}
			else
			{
				if (Train.StationState != TrainStopState.Jumping)
				{
					Train.StationState = TrainStopState.Pending;
				}
				
			}
			// automatically close doors
			if (Train.Specs.DoorCloseMode != DoorMode.Manual & Train.Specs.DoorInterlockState != DoorInterlockStates.Locked & !Train.Specs.DoorClosureAttempted)
			{
				if (Train.Station == -1 | Train.StationState == TrainStopState.Completed)
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
