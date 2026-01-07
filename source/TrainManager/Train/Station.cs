using System;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using RouteManager2.MessageManager;
using SoundManager;
using TrainManager.Car;
using TrainManager.Handles;

namespace TrainManager.Trains
{
	public partial class TrainBase
	{
		/// <inheritdoc/>
		public override void EnterStation(int stationIndex, int direction)
		{
			if (direction < 0)
			{
				if (Handles.Reverser.Actual == ReverserPosition.Forwards && Handles.Power.Driver != 0 && TrainManagerBase.currentHost.SimulationState != SimulationState.MinimalisticSimulation && stationIndex == Station)
				{
					//Our reverser and power are in F, but we are rolling backwards
					//Leave the station index alone, and we won't trigger again when we actually move forwards
					return;
				}

				Station = -1;
			}
			else if (direction > 0)
			{
				if (Station == stationIndex || NextStopSkipped != StopSkipMode.None)
				{
					return;
				}

				Station = stationIndex;
				if (StationState != TrainStopState.Jumping)
				{
					StationState = TrainStopState.Pending;
				}

				LastStation = stationIndex;
			}
		}

		/// <inheritdoc/>
		public override void LeaveStation(int stationIndex, int direction)
		{
			if (direction < 0)
			{
				Station = stationIndex;
				if (NextStopSkipped != StopSkipMode.None)
				{
					LastStation = stationIndex;
				}

				NextStopSkipped = StopSkipMode.None;
			}
			else if (direction > 0)
			{
				if (Station == stationIndex)
				{
					if (IsPlayerTrain)
					{
						if (TrainManagerBase.CurrentRoute.Stations[stationIndex].PlayerStops() & StationState == TrainStopState.Pending)
						{
							string s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","station_passed"});
							s = s.Replace("[name]", TrainManagerBase.CurrentRoute.Stations[stationIndex].Name);
							TrainManagerBase.currentHost.AddMessage(s, MessageDependency.None, GameMode.Normal, MessageColor.Orange, 10.0, null);
						}
						else if (TrainManagerBase.CurrentRoute.Stations[stationIndex].PlayerStops() & StationState == TrainStopState.Boarding)
						{
							string s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "message","station_passed_boarding"});
							s = s.Replace("[name]", TrainManagerBase.CurrentRoute.Stations[stationIndex].Name);
							TrainManagerBase.currentHost.AddMessage(s, MessageDependency.None, GameMode.Normal, MessageColor.Red, 10.0, null);
						}
					}

					Station = -1;
					if (StationState != TrainStopState.Jumping)
					{
						StationState = TrainStopState.Pending;
					}

					SafetySystems.PassAlarm?.Halt();
				}
			}
		}

		/// <summary>Is called once a frame to update the station state for the train</summary>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		internal void UpdateStation(double TimeElapsed)
		{
			if (Station >= 0)
			{
				int i = Station;
				int n = TrainManagerBase.CurrentRoute.Stations[Station].GetStopIndex(this);
				double tf, tb;
				if (n >= 0)
				{
					int frontCar = CurrentDirection == TrackDirection.Reverse ? Cars.Length -1 : 0;

					double p0 = Cars[frontCar].FrontAxle.Follower.TrackPosition - Cars[frontCar].FrontAxle.Position + 0.5 * Cars[frontCar].Length;
					double p1 = TrainManagerBase.CurrentRoute.Stations[i].Stops[n].TrackPosition;
					tf = TrainManagerBase.CurrentRoute.Stations[i].Stops[n].ForwardTolerance;
					tb = TrainManagerBase.CurrentRoute.Stations[i].Stops[n].BackwardTolerance;
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
					if (TrainManagerBase.CurrentRoute.Stations[i].StopsHere(this))
					{
						StationDepartureSoundPlayed = false;
						//Check whether all doors are controlled by the driver
						if (Specs.DoorOpenMode != DoorMode.Manual)
						{
							//Check that we are not moving
							if (Math.Abs(CurrentSpeed) < 0.1 / 3.6 &
							    Math.Abs(Specs.CurrentAverageAcceleration) < 0.1 / 3.6)
							{
								AttemptToOpenDoors(TrainManagerBase.CurrentRoute.Stations[i], tb, tf);
							}
						}

						// detect arrival
						if (CurrentSpeed > -0.277777777777778 & CurrentSpeed < 0.277777777777778)
						{
							bool left, right;
							if (TrainManagerBase.CurrentRoute.Stations[i].OpenLeftDoors)
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

							if (TrainManagerBase.CurrentRoute.Stations[i].OpenRightDoors)
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
								if (SafetySystems.StationAdjust != null)
								{
									SafetySystems.StationAdjust.Lit = false;
								}
								Specs.DoorClosureAttempted = false;
								SafetySystems.PassAlarm?.Halt();
								SoundBuffer buffer = (SoundBuffer) TrainManagerBase.CurrentRoute.Stations[i].ArrivalSoundBuffer;
								if (buffer != null)
								{
									OpenBveApi.Math.Vector3 pos = TrainManagerBase.CurrentRoute.Stations[i].SoundOrigin;
									TrainManagerBase.currentHost.PlaySound(buffer, 1.0, 1.0, pos, null, false);
								}

								StationArrivalTime = TrainManagerBase.CurrentRoute.SecondsSinceMidnight;
								StationDepartureTime = TrainManagerBase.CurrentRoute.Stations[i].DepartureTime - TimetableDelta;
								if (StationDepartureTime - TrainManagerBase.CurrentRoute.SecondsSinceMidnight < TrainManagerBase.CurrentRoute.Stations[i].StopTime)
								{
									StationDepartureTime = TrainManagerBase.CurrentRoute.SecondsSinceMidnight + TrainManagerBase.CurrentRoute.Stations[i].StopTime;
								}

								for (int j = 0; j < Cars.Length; j++)
								{
									Cars[j].Cargo.UpdateLoading(TrainManagerBase.CurrentRoute.Stations[i].PassengerRatio);
								}
								if (IsPlayerTrain)
								{
									double early = 0.0;
									if (TrainManagerBase.CurrentRoute.Stations[i].ArrivalTime >= 0.0)
									{
										early = (TrainManagerBase.CurrentRoute.Stations[i].ArrivalTime - TimetableDelta) - StationArrivalTime;
									}

									string s;
									if (early < -1.0)
									{
										s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","station_arrival_late"});
									}
									else if (early > 1.0)
									{
										s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","station_arrival_early"});
									}
									else
									{
										s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","station_arrival"});
									}

									System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
									TimeSpan a = TimeSpan.FromSeconds(Math.Abs(early));
									string b = a.Hours.ToString("00", Culture) + ":" + a.Minutes.ToString("00", Culture) + ":" + a.Seconds.ToString("00", Culture);
									if (StationDistanceToStopPoint < -0.1)
									{
										s += Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","delimiter"}) + Translations.GetInterfaceString(HostApplication.OpenBve, CurrentDirection == TrackDirection.Forwards ? new[] {"message","station_overrun"} : new[] {"message","station_underrun"});
									}
									else if (StationDistanceToStopPoint > 0.1)
									{
										s += Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","delimiter"}) + Translations.GetInterfaceString(HostApplication.OpenBve, CurrentDirection == TrackDirection.Forwards ? new[] {"message","station_underrun"} : new[] {"message","station_overrun"});
									}

									double d = Math.Abs(StationDistanceToStopPoint);
									string c = d.ToString("0.0", Culture);
									if (TrainManagerBase.CurrentRoute.Stations[i].Type == StationType.Terminal)
									{
										s += Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","delimiter"}) + Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","station_terminal"});
									}

									s = s.Replace("[name]", TrainManagerBase.CurrentRoute.Stations[i].Name);
									s = s.Replace("[time]", b);
									s = s.Replace("[difference]", c);
									TrainManagerBase.currentHost.AddMessage(s, MessageDependency.StationArrival, GameMode.Normal, MessageColor.White, 10.0, null);
									if (TrainManagerBase.CurrentRoute.Stations[i].Type == StationType.Normal)
									{
										s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","station_deadline"});
										TrainManagerBase.currentHost.AddMessage(s, MessageDependency.StationDeparture, GameMode.Normal, MessageColor.White, double.PositiveInfinity, null);
									}

									TrainManagerBase.currentHost.UpdateCustomTimetable(TrainManagerBase.CurrentRoute.Stations[i].TimetableDaytimeTexture, TrainManagerBase.CurrentRoute.Stations[i].TimetableNighttimeTexture);
								}

								// schedule door locks (passengers stuck between the doors)
								for (int j = 0; j < Cars.Length; j++)
								{
									for (int k = 0; k < Cars[j].Doors.Length; k++)
									{
										Cars[j].Doors[k].DoorLockDuration = 0.0;
										if (TrainManagerBase.CurrentRoute.Stations[i].OpenLeftDoors & Cars[j].Doors[k].Direction == -1 | TrainManagerBase.CurrentRoute.Stations[i].OpenRightDoors & Cars[j].Doors[k].Direction == 1)
										{
											double p = 0.005 * TrainManagerBase.CurrentRoute.Stations[i].PassengerRatio * TrainManagerBase.CurrentRoute.Stations[i].PassengerRatio * TrainManagerBase.CurrentRoute.Stations[i].PassengerRatio * TrainManagerBase.CurrentRoute.Stations[i].PassengerRatio;
											if (TrainManagerBase.RandomNumberGenerator.NextDouble() < p)
											{
												/*
												 * -- door lock at state --
												 * minimum: 0.2 (nearly closed)
												 * maximum: 0.8 (nearly opened)
												 * */
												Cars[j].Doors[k].DoorLockState = 0.2 + 0.6 * TrainManagerBase.RandomNumberGenerator.NextDouble();
												/* -- waiting time --
												 * minimum: 2.9 s
												 * maximum: 40.0 s
												 * average: 7.6 s
												 * */
												p = TrainManagerBase.RandomNumberGenerator.NextDouble();
												Cars[j].Doors[k].DoorLockDuration = (50.0 - 10.0 * p) / (17.0 - 16.0 * p);
											}
										}
									}
								}
							}
							else
							{
								SafetySystems.StationAdjust?.Update(tb, tf);
							}
						}
					}
				}
				else if (StationState == TrainStopState.Boarding)
				{
					for (int j = 0; j < Cars.Length; j++)
					{
						if (Cars[j].GetDoorsState(TrainManagerBase.CurrentRoute.Stations[i].OpenLeftDoors, TrainManagerBase.CurrentRoute.Stations[i].OpenRightDoors) == (TrainDoorState.Opened | TrainDoorState.AllOpened))
						{
							//Check whether all doors are controlled by the driver, and whether this is a non-standard station type
							//e.g. Change ends
							if (Specs.DoorCloseMode != DoorMode.Manual & TrainManagerBase.CurrentRoute.Stations[i].Type == StationType.Normal)
							{
								AttemptToCloseDoors();

								if (Specs.DoorClosureAttempted)
								{
									if (TrainManagerBase.CurrentRoute.Stations[i].OpenLeftDoors && !Cars[j].Doors[0].AnticipatedReopen && TrainManagerBase.RandomNumberGenerator.NextDouble() < TrainManagerBase.CurrentRoute.Stations[i].ReopenDoor)
									{
										Cars[j].Doors[0].ReopenLimit = TrainManagerBase.RandomNumberGenerator.Next(1, TrainManagerBase.CurrentRoute.Stations[i].ReopenStationLimit);
										Cars[j].Doors[0].ReopenCounter = 0;
										Cars[j].Doors[0].InterferingObjectRate = TrainManagerBase.RandomNumberGenerator.Next(1, TrainManagerBase.CurrentRoute.Stations[i].MaxInterferingObjectRate) * 0.01;
										if (Cars[j].Doors[0].InterferingObjectRate * Cars[j].Doors[0].Width >= Cars[j].Doors[0].MaxTolerance)
										{
											Cars[j].Doors[0].AnticipatedReopen = true;
										}
									}

									if (TrainManagerBase.CurrentRoute.Stations[i].OpenRightDoors && !Cars[j].Doors[1].AnticipatedReopen && TrainManagerBase.RandomNumberGenerator.NextDouble() < TrainManagerBase.CurrentRoute.Stations[i].ReopenDoor)
									{
										Cars[j].Doors[1].ReopenLimit = TrainManagerBase.RandomNumberGenerator.Next(1, TrainManagerBase.CurrentRoute.Stations[i].ReopenStationLimit);
										Cars[j].Doors[1].ReopenCounter = 0;
										Cars[j].Doors[1].InterferingObjectRate = TrainManagerBase.RandomNumberGenerator.Next(1, TrainManagerBase.CurrentRoute.Stations[i].MaxInterferingObjectRate) * 0.01;
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
					if (!TrainManagerBase.CurrentRoute.Stations[i].OpenLeftDoors & !TrainManagerBase.CurrentRoute.Stations[i].OpenRightDoors)
					{
						left = true;
						right = true;
					}
					else
					{
						if (TrainManagerBase.CurrentRoute.Stations[i].OpenLeftDoors)
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

						if (TrainManagerBase.CurrentRoute.Stations[i].OpenRightDoors)
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
						SoundBuffer buffer = (SoundBuffer) TrainManagerBase.CurrentRoute.Stations[i].DepartureSoundBuffer;
						if (buffer != null)
						{
							if (TrainManagerBase.CurrentRoute.SecondsSinceMidnight >= StationDepartureTime - buffer.Duration)
							{
								if (TrainManagerBase.currentHost.SimulationState == SimulationState.Running)
								{
									TrainManagerBase.currentHost.PlaySound(buffer, 1.0, 1.0, TrainManagerBase.CurrentRoute.Stations[i].SoundOrigin, null, false);
									StationDepartureSoundPlayed = true;
								}
							}
						}
					}

					for (int j = 0; j < Cars.Length; j++)
					{
						if (Cars[j].Doors[0].AnticipatedReopen && Cars[j].Doors[0].State == Cars[j].Doors[0].InterferingObjectRate)
						{
							if (Cars[j].Doors[0].NextReopenTime == 0.0)
							{
								Cars[j].Doors[0].NextReopenTime = TrainManagerBase.CurrentRoute.SecondsSinceMidnight + TrainManagerBase.CurrentRoute.Stations[i].InterferenceInDoor;
							}
							else if (Cars[j].Doors[0].ReopenCounter < Cars[j].Doors[0].ReopenLimit)
							{
								if (TrainManagerBase.CurrentRoute.SecondsSinceMidnight >= Cars[j].Doors[0].NextReopenTime)
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
								Cars[j].Doors[1].NextReopenTime = TrainManagerBase.CurrentRoute.SecondsSinceMidnight + TrainManagerBase.CurrentRoute.Stations[i].InterferenceInDoor;
							}
							else if (Cars[j].Doors[1].ReopenCounter < Cars[j].Doors[1].ReopenLimit)
							{
								if (TrainManagerBase.CurrentRoute.SecondsSinceMidnight >= Cars[j].Doors[1].NextReopenTime)
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

					TrainDoorState doorState = GetDoorsState(TrainManagerBase.CurrentRoute.Stations[i].OpenLeftDoors, TrainManagerBase.CurrentRoute.Stations[i].OpenRightDoors);
					if (left | right)
					{
						/*
						 * Assume that passengers only board at a scheduled stop
						 * If the player has opened the doors somewhere else (lineside?)
						 * then passengers should not be boarding
						 */
						if (doorState != TrainDoorState.AllClosed && TrainManagerBase.CurrentOptions.LoadingSway)
						{
							// passengers boarding
							for (int j = 0; j < Cars.Length; j++)
							{
								if (!Cars[j].EnableLoadingSway) continue;
								double r = 2.0 * TrainManagerBase.CurrentRoute.Stations[i].PassengerRatio * TimeElapsed;
								if (r >= TrainManagerBase.RandomNumberGenerator.NextDouble())
								{
									int d =
										(int) Math.Floor(TrainManagerBase.RandomNumberGenerator.NextDouble() * Cars[j].Doors.Length);
									if (Cars[j].Doors[d].State == 1.0)
									{
										Cars[j].Specs.RollShakeDirection += Cars[j].Doors[d].Direction;
									}
								}
							}
						}
					}

					if (Specs.DoorCloseMode == DoorMode.Manual || doorState == TrainDoorState.None || doorState == (TrainDoorState.Closed | TrainDoorState.AllClosed) || (TrainManagerBase.CurrentRoute.Stations[Station].Type == StationType.ChangeEnds || TrainManagerBase.CurrentRoute.Stations[Station].Type == StationType.Jump))
					{
						if (left | right)
						{
							// departure message
							if (TrainManagerBase.CurrentRoute.SecondsSinceMidnight > StationDepartureTime && (TrainManagerBase.CurrentRoute.Stations[i].Type != StationType.Terminal || IsPlayerTrain == false))
							{
								StationState = TrainStopState.Completed;
								switch (TrainManagerBase.CurrentRoute.Stations[i].Type)
								{
									case StationType.Normal:
										if (!IsPlayerTrain)
											break; // Only trigger messages for the player train
										if (!TrainManagerBase.CurrentRoute.Stations[i].OpenLeftDoors & !TrainManagerBase.CurrentRoute.Stations[i].OpenRightDoors | Specs.DoorCloseMode != DoorMode.Manual)
										{
											TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","station_depart"}), MessageDependency.None, GameMode.Normal, MessageColor.White, 5.0, null);
										}
										else
										{
											TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","station_depart_closedoors"}), MessageDependency.None, GameMode.Normal, MessageColor.White, 5.0, null);
										}

										break;
									case StationType.ChangeEnds:
										// Change ends always jumps to the NEXT station
										Jump(i + 1, 0);
										break;
									case StationType.Jump:
										// Jumps to an arbitrary station as defined in the routefile
										Jump(TrainManagerBase.CurrentRoute.Stations[i].JumpIndex, 0);
										break;
								}
							}
						}
						else
						{
							StationState = TrainStopState.Completed;
							if (IsPlayerTrain & TrainManagerBase.CurrentRoute.Stations[i].Type == StationType.Normal)
							{
								TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","station_depart"}), MessageDependency.None, GameMode.Normal, MessageColor.White, 5.0, null);
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
