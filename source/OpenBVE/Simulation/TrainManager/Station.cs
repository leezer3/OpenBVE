using System;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Runtime;
using OpenBveApi.Interface;
using OpenBveApi.Trains;
using RouteManager2.MessageManager;
using SoundManager;
using TrainManager.Car;

namespace OpenBve
{
	public partial class TrainManager
	{
		public partial class Train
		{
			/// <summary>Is called once a frame to update the station state for the train</summary>
			/// <param name="TimeElapsed">The frame time elapsed</param>
			private void UpdateStation(double TimeElapsed)
			{
				if (Station >= 0)
				{
					int i = Station;
					int n = Program.CurrentRoute.Stations[Station].GetStopIndex(NumberOfCars);
					double tf, tb;
					if (n >= 0)
					{
						double p0 = Cars[0].FrontAxle.Follower.TrackPosition - Cars[0].FrontAxle.Position + 0.5 * Cars[0].Length;
						double p1 = Program.CurrentRoute.Stations[i].Stops[n].TrackPosition;
						tf = Program.CurrentRoute.Stations[i].Stops[n].ForwardTolerance;
						tb = Program.CurrentRoute.Stations[i].Stops[n].BackwardTolerance;
						StationDistanceToStopPoint = p1 - p0;
					}
					else
					{
						StationDistanceToStopPoint = 0.0;
						tf = 5.0;
						tb = 5.0;
					}

					if (StationState == TrainStopState.Pending)
					{
						StationDepartureSoundPlayed = false;
						if (Program.CurrentRoute.Stations[i].StopsHere(this))
						{
							StationDepartureSoundPlayed = false;
							//Check whether all doors are controlled by the driver
							if (Specs.DoorOpenMode != DoorMode.Manual)
							{
								//Check that we are not moving
								if (Math.Abs(CurrentSpeed) < 0.1 / 3.6 &
								    Math.Abs(Specs.CurrentAverageAcceleration) < 0.1 / 3.6)
								{
									AttemptToOpenDoors(Program.CurrentRoute.Stations[i], tb, tf);
								}
							}

							// detect arrival
							if (CurrentSpeed > -0.277777777777778 & CurrentSpeed < 0.277777777777778)
							{
								bool left, right;
								if (Program.CurrentRoute.Stations[i].OpenLeftDoors)
								{
									left = false;
									for (int j = 0; j < Cars.Length; j++)
									{
										if (Cars[j].Doors[0].AnticipatedOpen)
										{
											left = true;
											break;
										}
									}
								}
								else
								{
									left = true;
								}

								if (Program.CurrentRoute.Stations[i].OpenRightDoors)
								{
									right = false;
									for (int j = 0; j < Cars.Length; j++)
									{
										if (Cars[j].Doors[1].AnticipatedOpen)
										{
											right = true;
											break;
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
									StationState = TrainStopState.Boarding;
									SafetySystems.StationAdjust.Lit = false;
									Specs.DoorClosureAttempted = false;
									SafetySystems.PassAlarm.Halt();
									SoundBuffer buffer = (SoundBuffer) Program.CurrentRoute.Stations[i].ArrivalSoundBuffer;
									if (buffer != null)
									{
										OpenBveApi.Math.Vector3 pos = Program.CurrentRoute.Stations[i].SoundOrigin;
										Program.Sounds.PlaySound(buffer, 1.0, 1.0, pos, false);
									}

									StationArrivalTime = Program.CurrentRoute.SecondsSinceMidnight;
									StationDepartureTime = Program.CurrentRoute.Stations[i].DepartureTime - TimetableDelta;
									if (StationDepartureTime - Program.CurrentRoute.SecondsSinceMidnight < Program.CurrentRoute.Stations[i].StopTime)
									{
										StationDepartureTime = Program.CurrentRoute.SecondsSinceMidnight + Program.CurrentRoute.Stations[i].StopTime;
									}

									Passengers.PassengerRatio = Program.CurrentRoute.Stations[i].PassengerRatio;
									UpdateTrainMassFromPassengerRatio(this);
									if (IsPlayerTrain)
									{
										double early = 0.0;
										if (Program.CurrentRoute.Stations[i].ArrivalTime >= 0.0)
										{
											early = (Program.CurrentRoute.Stations[i].ArrivalTime - TimetableDelta) - StationArrivalTime;
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
										if (StationDistanceToStopPoint < -0.1)
										{
											s += Translations.GetInterfaceString("message_delimiter") + Translations.GetInterfaceString("message_station_overrun");
										}
										else if (StationDistanceToStopPoint > 0.1)
										{
											s += Translations.GetInterfaceString("message_delimiter") + Translations.GetInterfaceString("message_station_underrun");
										}

										double d = Math.Abs(StationDistanceToStopPoint);
										string c = d.ToString("0.0", Culture);
										if (Program.CurrentRoute.Stations[i].Type == StationType.Terminal)
										{
											s += Translations.GetInterfaceString("message_delimiter") + Translations.GetInterfaceString("message_station_terminal");
										}

										s = s.Replace("[name]", Program.CurrentRoute.Stations[i].Name);
										s = s.Replace("[time]", b);
										s = s.Replace("[difference]", c);
										MessageManager.AddMessage(s, MessageDependency.StationArrival, GameMode.Normal, MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
										if (Program.CurrentRoute.Stations[i].Type == StationType.Normal)
										{
											s = Translations.GetInterfaceString("message_station_deadline");
											MessageManager.AddMessage(s, MessageDependency.StationDeparture, GameMode.Normal, MessageColor.White, double.PositiveInfinity, null);
										}

										Timetable.UpdateCustomTimetable(Program.CurrentRoute.Stations[i].TimetableDaytimeTexture, Program.CurrentRoute.Stations[i].TimetableNighttimeTexture);
									}

									// schedule door locks (passengers stuck between the doors)
									for (int j = 0; j < Cars.Length; j++)
									{
										for (int k = 0; k < Cars[j].Doors.Length; k++)
										{
											Cars[j].Doors[k].DoorLockDuration = 0.0;
											if (Program.CurrentRoute.Stations[i].OpenLeftDoors & Cars[j].Doors[k].Direction == -1 | Program.CurrentRoute.Stations[i].OpenRightDoors & Cars[j].Doors[k].Direction == 1)
											{
												double p = 0.005 * Program.CurrentRoute.Stations[i].PassengerRatio * Program.CurrentRoute.Stations[i].PassengerRatio * Program.CurrentRoute.Stations[i].PassengerRatio * Program.CurrentRoute.Stations[i].PassengerRatio;
												if (Program.RandomNumberGenerator.NextDouble() < p)
												{
													/*
													 * -- door lock at state --
													 * minimum: 0.2 (nearly closed)
													 * maximum: 0.8 (nearly opened)
													 * */
													Cars[j].Doors[k].DoorLockState = 0.2 + 0.6 * Program.RandomNumberGenerator.NextDouble();
													/* -- waiting time --
													 * minimum: 2.9 s
													 * maximum: 40.0 s
													 * average: 7.6 s
													 * */
													p = Program.RandomNumberGenerator.NextDouble();
													Cars[j].Doors[k].DoorLockDuration = (50.0 - 10.0 * p) / (17.0 - 16.0 * p);
												}
											}
										}
									}
								}
								else
								{
									if (SafetySystems.StationAdjust != null)
									{
										SafetySystems.StationAdjust.Update(tb, tf);
									}
								}
							}
						}
					}
					else if (StationState == TrainStopState.Boarding)
					{
						for (int j = 0; j < Cars.Length; j++)
						{
							if (Cars[j].GetDoorsState(Program.CurrentRoute.Stations[i].OpenLeftDoors, Program.CurrentRoute.Stations[i].OpenRightDoors) == (TrainDoorState.Opened | TrainDoorState.AllOpened))
							{
								//Check whether all doors are controlled by the driver, and whether this is a non-standard station type
								//e.g. Change ends
								if (Specs.DoorCloseMode != DoorMode.Manual & Program.CurrentRoute.Stations[i].Type == StationType.Normal)
								{
									AttemptToCloseDoors();

									if (Specs.DoorClosureAttempted)
									{
										if (Program.CurrentRoute.Stations[i].OpenLeftDoors && !Cars[j].Doors[0].AnticipatedReopen && Program.RandomNumberGenerator.NextDouble() < Program.CurrentRoute.Stations[i].ReopenDoor)
										{
											Cars[j].Doors[0].ReopenLimit = Program.RandomNumberGenerator.Next(1, Program.CurrentRoute.Stations[i].ReopenStationLimit);
											Cars[j].Doors[0].ReopenCounter = 0;
											Cars[j].Doors[0].InterferingObjectRate = Program.RandomNumberGenerator.Next(1, Program.CurrentRoute.Stations[i].MaxInterferingObjectRate) * 0.01;
											if (Cars[j].Doors[0].InterferingObjectRate * Cars[j].Doors[0].Width >= Cars[j].Doors[0].MaxTolerance)
											{
												Cars[j].Doors[0].AnticipatedReopen = true;
											}
										}

										if (Program.CurrentRoute.Stations[i].OpenRightDoors && !Cars[j].Doors[1].AnticipatedReopen && Program.RandomNumberGenerator.NextDouble() < Program.CurrentRoute.Stations[i].ReopenDoor)
										{
											Cars[j].Doors[1].ReopenLimit = Program.RandomNumberGenerator.Next(1, Program.CurrentRoute.Stations[i].ReopenStationLimit);
											Cars[j].Doors[1].ReopenCounter = 0;
											Cars[j].Doors[1].InterferingObjectRate = Program.RandomNumberGenerator.Next(1, Program.CurrentRoute.Stations[i].MaxInterferingObjectRate) * 0.01;
											if (Cars[j].Doors[1].InterferingObjectRate * Cars[j].Doors[1].Width >= Cars[j].Doors[1].MaxTolerance)
											{
												Cars[j].Doors[1].AnticipatedReopen = true;
											}
										}
									}
								}
							}
						}

						// detect departure
						bool left, right;
						if (!Program.CurrentRoute.Stations[i].OpenLeftDoors & !Program.CurrentRoute.Stations[i].OpenRightDoors)
						{
							left = true;
							right = true;
						}
						else
						{
							if (Program.CurrentRoute.Stations[i].OpenLeftDoors)
							{
								left = false;
								for (int j = 0; j < Cars.Length; j++)
								{
									for (int k = 0; k < Cars[j].Doors.Length; k++)
									{
										if (Cars[j].Doors[k].State != 0.0)
										{
											left = true;
											break;
										}
									}

									if (left) break;
								}
							}
							else
							{
								left = false;
							}

							if (Program.CurrentRoute.Stations[i].OpenRightDoors)
							{
								right = false;
								for (int j = 0; j < Cars.Length; j++)
								{
									for (int k = 0; k < Cars[j].Doors.Length; k++)
									{
										if (Cars[j].Doors[k].State != 0.0)
										{
											right = true;
											break;
										}
									}

									if (right) break;
								}
							}
							else
							{
								right = false;
							}
						}

						// departure sound
						if (!StationDepartureSoundPlayed)
						{
							SoundBuffer buffer = (SoundBuffer) Program.CurrentRoute.Stations[i].DepartureSoundBuffer;
							if (buffer != null)
							{
								if (Program.CurrentRoute.SecondsSinceMidnight >= StationDepartureTime - buffer.Duration)
								{
									if (!Game.MinimalisticSimulation)
									{
										Program.Sounds.PlaySound(buffer, 1.0, 1.0, Program.CurrentRoute.Stations[i].SoundOrigin, false);
									}

									StationDepartureSoundPlayed = true;
								}
							}
						}

						for (int j = 0; j < Cars.Length; j++)
						{
							if (Cars[j].Doors[0].AnticipatedReopen && Cars[j].Doors[0].State == Cars[j].Doors[0].InterferingObjectRate)
							{
								if (Cars[j].Doors[0].NextReopenTime == 0.0)
								{
									Cars[j].Doors[0].NextReopenTime = Program.CurrentRoute.SecondsSinceMidnight + Program.CurrentRoute.Stations[i].InterferenceInDoor;
								}
								else if (Cars[j].Doors[0].ReopenCounter < Cars[j].Doors[0].ReopenLimit)
								{
									if (Program.CurrentRoute.SecondsSinceMidnight >= Cars[j].Doors[0].NextReopenTime)
									{
										Cars[j].OpenDoors(true, false);
									}
								}
								else
								{
									Cars[j].Doors[0].AnticipatedReopen = false;
								}
							}

							if (Cars[j].Doors[1].AnticipatedReopen && Cars[j].Doors[1].State == Cars[j].Doors[1].InterferingObjectRate)
							{
								if (Cars[j].Doors[1].NextReopenTime == 0.0)
								{
									Cars[j].Doors[1].NextReopenTime = Program.CurrentRoute.SecondsSinceMidnight + Program.CurrentRoute.Stations[i].InterferenceInDoor;
								}
								else if (Cars[j].Doors[1].ReopenCounter < Cars[j].Doors[1].ReopenLimit)
								{
									if (Program.CurrentRoute.SecondsSinceMidnight >= Cars[j].Doors[1].NextReopenTime)
									{
										Cars[j].OpenDoors(false, true);
									}
								}
								else
								{
									Cars[j].Doors[1].AnticipatedReopen = false;
								}
							}
						}

						TrainDoorState doorState = GetDoorsState(Program.CurrentRoute.Stations[i].OpenLeftDoors, Program.CurrentRoute.Stations[i].OpenRightDoors);
						if (left | right)
						{
							/*
							 * Assume that passengers only board at a scheduled stop
							 * If the player has opened the doors somewhere else (lineside?)
							 * then passengers should not be boarding
							 */
							if (doorState != TrainDoorState.AllClosed && Interface.CurrentOptions.LoadingSway)
							{
								// passengers boarding
								for (int j = 0; j < Cars.Length; j++)
								{
									if (!Cars[j].EnableLoadingSway) continue;
									double r = 2.0 * Program.CurrentRoute.Stations[i].PassengerRatio * TimeElapsed;
									if (r >= Program.RandomNumberGenerator.NextDouble())
									{
										int d =
											(int) Math.Floor(Program.RandomNumberGenerator.NextDouble() * Cars[j].Doors.Length);
										if (Cars[j].Doors[d].State == 1.0)
										{
											Cars[j].Specs.RollShakeDirection += Cars[j].Doors[d].Direction;
										}
									}
								}
							}
						}

						if (Specs.DoorCloseMode == DoorMode.Manual || doorState == TrainDoorState.None || doorState == (TrainDoorState.Closed | TrainDoorState.AllClosed) || (Program.CurrentRoute.Stations[Station].Type == StationType.ChangeEnds || Program.CurrentRoute.Stations[Station].Type == StationType.Jump))
						{
							if (left | right)
							{
								// departure message
								if (Program.CurrentRoute.SecondsSinceMidnight > StationDepartureTime && (Program.CurrentRoute.Stations[i].Type != StationType.Terminal || IsPlayerTrain == false))
								{
									StationState = TrainStopState.Completed;
									switch (Program.CurrentRoute.Stations[i].Type)
									{
										case StationType.Normal:
											if (!IsPlayerTrain)
												break; // Only trigger messages for the player train
											if (!Program.CurrentRoute.Stations[i].OpenLeftDoors & !Program.CurrentRoute.Stations[i].OpenRightDoors | Specs.DoorCloseMode != DoorMode.Manual)
											{
												MessageManager.AddMessage(Translations.GetInterfaceString("message_station_depart"), MessageDependency.None, GameMode.Normal, MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
											}
											else
											{
												MessageManager.AddMessage(Translations.GetInterfaceString("message_station_depart_closedoors"), MessageDependency.None, GameMode.Normal, MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
											}

											break;
										case StationType.ChangeEnds:
											// Change ends always jumps to the NEXT station
											Jump(i + 1);
											break;
										case StationType.Jump:
											// Jumps to an arbritrary station as defined in the routefile
											Jump(Program.CurrentRoute.Stations[i].JumpIndex);
											break;
									}
								}
							}
							else
							{
								StationState = TrainStopState.Completed;
								if (IsPlayerTrain & Program.CurrentRoute.Stations[i].Type == StationType.Normal)
								{
									MessageManager.AddMessage(Translations.GetInterfaceString("message_station_depart"), MessageDependency.None, GameMode.Normal, MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
								}
							}
						}
					}
				}
				else
				{
					if (StationState != TrainStopState.Jumping)
					{
						StationState = TrainStopState.Pending;
					}

				}

				// automatically close doors
				if (Specs.DoorCloseMode != DoorMode.Manual & !Specs.DoorClosureAttempted)
				{
					if (Station == -1 | StationState == TrainStopState.Completed)
					{
						if ((GetDoorsState(true, true) & TrainDoorState.AllClosed) == 0)
						{
							CloseDoors(true, true);
							Specs.DoorClosureAttempted = true;
						}
					}
				}
			}
		}
	}
}
