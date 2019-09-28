using System;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using RouteManager2.Events;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>This class forms an AI representation of a simple human driver</summary>
		internal class SimpleHumanDriverAI : GeneralAI
		{
			// members
			private double TimeLastProcessed;
			private double CurrentInterval;
			private bool BrakeMode;
			private double CurrentSpeedFactor;
			private readonly double PersonalitySpeedFactor;
			private int PowerNotchAtWhichWheelSlipIsObserved;
			private int LastStation;

			private readonly TrainManager.Train Train;
			// functions
			internal SimpleHumanDriverAI(TrainManager.Train train)
			{
				this.Train = train;
				this.TimeLastProcessed = 0.0;
				this.CurrentInterval = 1.0;
				this.BrakeMode = false;
				this.PersonalitySpeedFactor = 0.90 + 0.10 * Program.RandomNumberGenerator.NextDouble();
				this.CurrentSpeedFactor = this.PersonalitySpeedFactor;
				this.PowerNotchAtWhichWheelSlipIsObserved = Train.Handles.Power.MaximumNotch + 1;
				if (Train.Station >= 0 & Train.StationState == TrainStopState.Boarding)
				{
					this.LastStation = Train.Station;
				}
				else
				{
					this.LastStation = -1;
				}
			}
			private AIResponse PerformPlugin()
			{
				AIResponse response = Train.Plugin.UpdateAI();
				if (response == AIResponse.Short)
				{
					CurrentInterval = 0.2 + 0.1 * Program.RandomNumberGenerator.NextDouble();
				}
				else if (response == AIResponse.Medium)
				{
					CurrentInterval = 0.4 + 0.2 * Program.RandomNumberGenerator.NextDouble();
				}
				else if (response == AIResponse.Long)
				{
					CurrentInterval = 0.8 + 0.4 * Program.RandomNumberGenerator.NextDouble();
				}
				return response;
			}
			private void PerformDefault()
			{
				if (Train.Derailed)
				{
					if (Train.Handles.EmergencyBrake.Driver != true)
					{
						Train.ApplyEmergencyBrake();
					}
					return;
				}
				// personality
				double spd = Train.CurrentSpeed;
				if (Train.Station >= 0 & Train.StationState == TrainStopState.Boarding)
				{
					if (Train.Station != this.LastStation)
					{
						this.LastStation = Train.Station;
						double time;
						if (Program.CurrentRoute.Stations[Train.Station].ArrivalTime >= 0.0)
						{
							time = Program.CurrentRoute.Stations[Train.Station].ArrivalTime - Train.TimetableDelta;
						}
						else if (Program.CurrentRoute.Stations[Train.Station].DepartureTime >= 0.0)
						{
							time = Program.CurrentRoute.Stations[Train.Station].DepartureTime - Train.TimetableDelta;
							if (time > Program.CurrentRoute.SecondsSinceMidnight)
							{
								time -= Program.CurrentRoute.Stations[Train.Station].StopTime;
								if (time > Program.CurrentRoute.SecondsSinceMidnight)
								{
									time = double.MinValue;
								}
							}
						}
						else
						{
							time = double.MinValue;
						}
						if (time != double.MinValue)
						{
							const double largeThreshold = 30.0;
							const double largeChangeFactor = 0.0025;
							const double smallThreshold = 15.0;
							const double smallChange = 0.05;
							double diff = Program.CurrentRoute.SecondsSinceMidnight - time;
							if (diff < -largeThreshold)
							{
								/* The AI is too fast. Decrease the preferred speed. */
								this.CurrentSpeedFactor -= largeChangeFactor * (-diff - largeThreshold);
								if (this.CurrentSpeedFactor < 0.7)
								{
									this.CurrentSpeedFactor = 0.7;
								}
							}
							else if (diff > largeThreshold)
							{
								/* The AI is too slow. Increase the preferred speed. */
								this.CurrentSpeedFactor += largeChangeFactor * (diff - largeThreshold);
								if (this.CurrentSpeedFactor > 1.1)
								{
									this.CurrentSpeedFactor = 1.1;
								}
							}
							else if (Math.Abs(diff) < smallThreshold)
							{
								/* The AI is at about the right speed. Change the preferred speed toward the personality default. */
								if (this.CurrentSpeedFactor < this.PersonalitySpeedFactor)
								{
									this.CurrentSpeedFactor += smallChange;
									if (this.CurrentSpeedFactor > this.PersonalitySpeedFactor)
									{
										this.CurrentSpeedFactor = this.PersonalitySpeedFactor;
									}
								}
								else if (this.CurrentSpeedFactor > this.PersonalitySpeedFactor)
								{
									this.CurrentSpeedFactor -= smallChange;
									if (this.CurrentSpeedFactor < this.PersonalitySpeedFactor)
									{
										this.CurrentSpeedFactor = this.PersonalitySpeedFactor;
									}
								}
							}
						}
					}
				}
				// door states
				bool doorsopen = false;
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					for (int j = 0; j < Train.Cars[i].Doors.Length; j++)
					{
						if (Train.Cars[i].Doors[j].State != 0.0)
						{
							doorsopen = true;
							break;
						}
						if (doorsopen) break;
					}
				}
				// do the ai
				Train.Specs.CurrentConstSpeed = false;
				Train.ApplyHoldBrake(false);
				int stopIndex = Train.Station >= 0 ? Program.CurrentRoute.Stations[Train.Station].GetStopIndex(Train.NumberOfCars) : -1;
				if (Train.CurrentSectionLimit == 0.0)
				{
					// passing red signal
					Train.ApplyEmergencyBrake();
					Train.ApplyNotch(-1, true, 1, true);
					CurrentInterval = 0.5;
				}
				else if (doorsopen | Train.StationState == TrainStopState.Boarding)
				{
					// door opened or boarding at station
					this.PowerNotchAtWhichWheelSlipIsObserved = Train.Handles.Power.MaximumNotch + 1;
					if (Train.Station >= 0 && Program.CurrentRoute.Stations[Train.Station].Type != StationType.Normal && Program.CurrentRoute.Stations[Train.Station].Type != StationType.RequestStop && Train.IsPlayerTrain)
					{
						// player's terminal station
						if (Train.Plugin == null || Train.Plugin.LastReverser == -2)
						{
							Train.ApplyReverser(0, false);
						}
						Train.ApplyNotch(-1, true, 1, true);
						Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Service);
						Train.ApplyEmergencyBrake();
						CurrentInterval = 1.0;
					}
					else
					{
						CurrentInterval = 1.0;
						Train.ApplyNotch(-1, true, 0, true);
						if (Train.Handles.Brake is TrainManager.AirBrakeHandle)
						{
							if (Train.StationDepartureTime - Program.CurrentRoute.SecondsSinceMidnight > 10 || Train.Cars[Train.DriverCar].CarBrake.brakeCylinder.CurrentPressure < 0.3 * Train.Cars[Train.DriverCar].CarBrake.brakeCylinder.ServiceMaximumPressure)
							{
								Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Service);
							}
							else if (Train.Cars[Train.DriverCar].CarBrake.brakeCylinder.CurrentPressure > 0.9 * Train.Cars[Train.DriverCar].CarBrake.brakeCylinder.EmergencyMaximumPressure)
							{
								Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
							}
							else
							{
								Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Lap);
							}
						}
						else
						{
							int b;
							if (Math.Abs(spd) < 0.02)
							{
								b = (int)Math.Ceiling(0.5 * (double)Train.Handles.Brake.MaximumNotch);
								CurrentInterval = 0.3;
							}
							else
							{
								b = Train.Handles.Brake.MaximumNotch;
							}
							if (Train.Handles.Brake.Driver < b)
							{
								Train.ApplyNotch(0, true, 1, true);
							}
							else if (Train.Handles.Brake.Driver > b)
							{
								Train.ApplyNotch(0, true, -1, true);
							}
						}
						Train.UnapplyEmergencyBrake();
						if (Train.Station >= 0 & Train.StationState == TrainStopState.Completed)
						{
							// ready for departure - close doors
							if (Train.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic && Train.SafetySystems.DoorInterlockState == DoorInterlockStates.Unlocked)
							{
								TrainManager.CloseTrainDoors(Train, true, true);
							}
						}
						else if (Train.Station >= 0 & Train.StationState == TrainStopState.Boarding)
						{
						}
						else
						{
							// not at station - close doors
							if (Train.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic && Train.SafetySystems.DoorInterlockState == DoorInterlockStates.Unlocked)
							{
								TrainManager.CloseTrainDoors(Train, true, true);
							}
						}
					}
				}
				else if (Train.Station >= 0 && stopIndex >= 0 && Train.StationDistanceToStopPoint < Program.CurrentRoute.Stations[Train.Station].Stops[stopIndex].BackwardTolerance && (StopsAtStation(Train.Station, Train) & (Program.CurrentRoute.Stations[Train.Station].OpenLeftDoors | Program.CurrentRoute.Stations[Train.Station].OpenRightDoors) & Math.Abs(Train.CurrentSpeed) < 0.25 & Train.StationState == TrainStopState.Pending))
				{
					// arrived at station - open doors
					if (Train.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic && Train.SafetySystems.DoorInterlockState == DoorInterlockStates.Unlocked)
					{
						TrainManager.OpenTrainDoors(Train, Program.CurrentRoute.Stations[Train.Station].OpenLeftDoors, Program.CurrentRoute.Stations[Train.Station].OpenRightDoors);
					}
					CurrentInterval = 1.0;
				}
				else if (Train.Station >= 0 && stopIndex >= 0 && Program.CurrentRoute.Stations[Train.Station].Type != StationType.Normal && Program.CurrentRoute.Stations[Train.Station].Type != StationType.RequestStop && Train.IsPlayerTrain && Train.StationDistanceToStopPoint < Program.CurrentRoute.Stations[Train.Station].Stops[stopIndex].BackwardTolerance && -Train.StationDistanceToStopPoint < Program.CurrentRoute.Stations[Train.Station].Stops[stopIndex].ForwardTolerance && Math.Abs(Train.CurrentSpeed) < 0.25)
				{
					// player's terminal station (not boarding any longer)
					if (Train.Plugin != null || Train.Plugin.LastReverser == -2)
					{
						Train.ApplyReverser(0, false);
					}
					Train.ApplyNotch(-1, true, 1, true);
					Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Service);
					Train.ApplyEmergencyBrake();
					CurrentInterval = 10.0;
				}
				else
				{
					// drive
					Train.ApplyReverser(1, false);
					if (Train.Cars[Train.DriverCar].FrontAxle.CurrentWheelSlip | Train.Cars[Train.DriverCar].RearAxle.CurrentWheelSlip)
					{
						// react to wheel slip
						if (Train.Handles.Power.Driver > 1)
						{
							this.PowerNotchAtWhichWheelSlipIsObserved = Train.Handles.Power.Driver;
							Train.ApplyNotch(-1, true, -1, true);
							Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
							this.CurrentInterval = 2.5;
							return;
						}
					}
					// initialize
					double acc = Train.Specs.CurrentAverageAcceleration;
					double lim = PrecedingTrainSpeedLimit * 1.2;
					if (Train.CurrentRouteLimit < lim)
					{
						lim = Train.CurrentRouteLimit;
					}
					if (Train.CurrentSectionLimit < lim)
					{
						lim = Train.CurrentSectionLimit;
					}
					double powerstart, powerend, brakestart;
					if (double.IsPositiveInfinity(lim))
					{
						powerstart = lim;
						powerend = lim;
						brakestart = lim;
					}
					else
					{
						lim *= this.CurrentSpeedFactor;
						if (spd < 8.0)
						{
							powerstart = 0.75 * lim;
							powerend = 0.95 * lim;
						}
						else
						{
							powerstart = lim - 2.5;
							powerend = lim - 1.5;
						}
						if (this.BrakeMode)
						{
							brakestart = powerend;
						}
						else
						{
							brakestart = lim + 0.5;
						}
					}
					double dec = 0.0;
					double decelerationCruise;   /* power below this deceleration, cruise above */
					double decelerationStart;    /* brake above this deceleration, cruise below */
					double decelerationStep;     /* the deceleration step per brake notch */
					double BrakeDeceleration = Train.Cars[Train.DriverCar].CarBrake.DecelerationAtServiceMaximumPressure(Train.Handles.Brake.Actual, Train.Cars[Train.DriverCar].CurrentSpeed);
					for (int i = 0; i < Train.Cars.Length; i++)
					{
						if (Train.Cars[i].Specs.IsMotorCar)
						{
							if (Train.Cars[Train.DriverCar].Specs.MotorDeceleration != 0 && Train.Cars[Train.DriverCar].Specs.MotorDeceleration < BrakeDeceleration)
							{
								BrakeDeceleration = Train.Cars[Train.DriverCar].Specs.MotorDeceleration;
							}
							break;
						}
					}
					if (Train.Handles.Brake is TrainManager.AirBrakeHandle | Train.Handles.Brake.MaximumNotch <= 0)
					{
						decelerationCruise = 0.3 * BrakeDeceleration;
						decelerationStart = 0.5 * BrakeDeceleration;
						decelerationStep = 0.1 * BrakeDeceleration;
					}
					else if (Train.Handles.Brake.MaximumNotch <= 2)
					{
						decelerationCruise = 0.2 * BrakeDeceleration;
						decelerationStart = 0.4 * BrakeDeceleration;
						decelerationStep = 0.5 * BrakeDeceleration;
					}
					else
					{
						decelerationCruise = 0.2 * BrakeDeceleration;
						decelerationStart = 0.5 * BrakeDeceleration;
						decelerationStep = BrakeDeceleration / (double)Train.Handles.Brake.MaximumNotch;
					}
					if (this.CurrentSpeedFactor >= 1.0)
					{
						decelerationCruise *= 1.25;
						decelerationStart *= 1.25;
						decelerationStep *= 1.25;
					}

					if (spd > 0.0 & spd > brakestart)
					{
						dec = decelerationStep + 0.1 * (spd - brakestart);
					}
					bool reduceDecelerationCruiseAndStart = false;
					// look ahead
					double lookahead = (Train.Station >= 0 ? 150.0 : 50.0) + (spd * spd) / (2.0 * decelerationCruise);
					double tp = Train.FrontCarTrackPosition();
					double stopDistance = double.MaxValue;
					{
						// next station stop
						int te = Train.Cars[0].FrontAxle.Follower.LastTrackElement;
						int currentTrack = Train.Cars[0].FrontAxle.Follower.TrackIndex;
						for (int i = te; i < Program.CurrentRoute.Tracks[currentTrack].Elements.Length; i++)
						{
							double stp = Program.CurrentRoute.Tracks[currentTrack].Elements[i].StartingTrackPosition;
							if (tp + lookahead <= stp) break;
							for (int j = 0; j < Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events.Length; j++)
							{
								if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is StationStartEvent && Train.NextStopSkipped == StopSkipMode.None)
								{
									StationStartEvent e = (StationStartEvent)Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j];
									if (StopsAtStation(e.StationIndex, Train) & Train.LastStation != e.StationIndex)
									{
										int s = Program.CurrentRoute.Stations[e.StationIndex].GetStopIndex(Train.NumberOfCars);
										if (s >= 0)
										{
											double dist = Program.CurrentRoute.Stations[e.StationIndex].Stops[s].TrackPosition - tp;
											if (dist > 0.0 & dist < stopDistance)
											{
												stopDistance = dist;
											}
										}
									}
								}
							}
						}
					}
					{
						// events
						int te = Train.Cars[0].FrontAxle.Follower.LastTrackElement;
						int currentTrack = Train.Cars[0].FrontAxle.Follower.TrackIndex;
						for (int i = te; i < Program.CurrentRoute.Tracks[currentTrack].Elements.Length; i++)
						{
							double stp = Program.CurrentRoute.Tracks[currentTrack].Elements[i].StartingTrackPosition;
							if (tp + lookahead <= stp) break;
							for (int j = 0; j < Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events.Length; j++)
							{
								if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is LimitChangeEvent)
								{
									// speed limit
									LimitChangeEvent e = (LimitChangeEvent)Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j];
									if (e.NextSpeedLimit < spd)
									{
										double dist = stp + e.TrackPositionDelta - tp;
										double edec = (spd * spd - e.NextSpeedLimit * e.NextSpeedLimit * this.CurrentSpeedFactor) / (2.0 * dist);
										if (edec > dec) dec = edec;
									}
								}
								else if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is SectionChangeEvent)
								{
									// section
									SectionChangeEvent e = (SectionChangeEvent)Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j];
									if (stp + e.TrackPositionDelta > tp)
									{
										if (!Program.CurrentRoute.Sections[e.NextSectionIndex].Invisible & Program.CurrentRoute.Sections[e.NextSectionIndex].CurrentAspect >= 0)
										{
											double elim = Program.CurrentRoute.Sections[e.NextSectionIndex].Aspects[Program.CurrentRoute.Sections[e.NextSectionIndex].CurrentAspect].Speed * this.CurrentSpeedFactor;
											if (elim < spd | spd <= 0.0)
											{
												double dist = stp + e.TrackPositionDelta - tp;
												double edec;
												if (elim == 0.0)
												{
													double redstopdist;
													if (Train.Station >= 0 & Train.StationState == TrainStopState.Completed & dist < 120.0)
													{
														dist = 1.0;
														redstopdist = 25.0;
													}
													else if (Train.Station >= 0 & Train.StationState == TrainStopState.Pending | stopDistance < dist)
													{
														redstopdist = 1.0;
													}
													else if (spd > 9.72222222222222)
													{
														redstopdist = 55.0;
													}
													else
													{
														redstopdist = 35.0;
													}
													if (dist > redstopdist)
													{
														edec = (spd * spd) / (2.0 * (dist - redstopdist));
													}
													else
													{
														edec = BrakeDeceleration;
													}
													if (dist < 100.0)
													{
														reduceDecelerationCruiseAndStart = true;
													}
												}
												else
												{
													if (dist >= 1.0)
													{
														edec = (spd * spd - elim * elim) / (2.0 * dist);
													}
													else
													{
														edec = 0.0;
													}
												}
												if (edec > dec) dec = edec;
											}
										}
									}
								}
								else if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is StationStartEvent && Train.NextStopSkipped == StopSkipMode.None)
								{
									// station start
									if (Train.Station == -1)
									{
										StationStartEvent e = (StationStartEvent)Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j];
										if (StopsAtStation(e.StationIndex, Train) & Train.LastStation != e.StationIndex)
										{
											int s = Program.CurrentRoute.Stations[e.StationIndex].GetStopIndex(Train.NumberOfCars);
											if (s >= 0)
											{
												double dist = Program.CurrentRoute.Stations[e.StationIndex].Stops[s].TrackPosition - tp;
												if (dist > -Program.CurrentRoute.Stations[e.StationIndex].Stops[s].ForwardTolerance)
												{
													if (dist < 25.0)
													{
														reduceDecelerationCruiseAndStart = true;
													}
													else if (this.CurrentSpeedFactor < 1.0)
													{
														dist -= 5.0;
													}
													var edec = spd * spd / (2.0 * dist);
													if (edec > dec) dec = edec;
												}
											}
										}
									}
								}
								else if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is StationStartEvent && Train.NextStopSkipped == StopSkipMode.Decelerate)
								{
									// Brakes the train when passing through a request stop, which is not to be passed at linespeed
									if (Train.Station == -1)
									{
										StationStartEvent e = (StationStartEvent)Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j];
										if (StopsAtStation(e.StationIndex, Train) & Train.LastStation != e.StationIndex)
										{
											int s = Program.CurrentRoute.Stations[e.StationIndex].GetStopIndex(Train.NumberOfCars);
											if (s >= 0)
											{
												double dist = Program.CurrentRoute.Stations[e.StationIndex].Stops[s].TrackPosition - tp;
												if (dist > -Program.CurrentRoute.Stations[e.StationIndex].Stops[s].ForwardTolerance)
												{
													if (dist < 25.0)
													{
														reduceDecelerationCruiseAndStart = true;
													}
													else if (this.CurrentSpeedFactor < 1.0)
													{
														dist -= 5.0;
													}
													if (dist > 25)
													{
														var edec = spd * spd / (2.0 * dist);
														if (edec > dec) dec = edec;
													}

												}
											}
										}
									}
								}
								else if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is TrackManager.StationEndEvent && Train.NextStopSkipped == StopSkipMode.None)
								{
									// station end
									if (Train.Station == -1)
									{
										TrackManager.StationEndEvent e = (TrackManager.StationEndEvent)Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j];
										if (StopsAtStation(e.StationIndex, Train) & Train.LastStation != e.StationIndex)
										{
											int s = Program.CurrentRoute.Stations[e.StationIndex].GetStopIndex(Train.NumberOfCars);
											if (s >= 0)
											{
												double dist = Program.CurrentRoute.Stations[e.StationIndex].Stops[s].TrackPosition - tp;
												if (dist > -Program.CurrentRoute.Stations[e.StationIndex].Stops[s].ForwardTolerance)
												{
													if (dist < 25.0)
													{
														reduceDecelerationCruiseAndStart = true;
													}
													else if (this.CurrentSpeedFactor < 1.0)
													{
														dist -= 5.0;
													}
													var edec = spd * spd / (2.0 * dist);
													if (edec > dec) dec = edec;
												}
											}
										}
									}
								}
								else if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is TrackEndEvent)
								{
									// track end
									if (Train.IsPlayerTrain)
									{
										TrackEndEvent e = (TrackEndEvent)Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j];
										double dist = stp + e.TrackPositionDelta - tp;
										double edec;
										if (dist >= 15.0)
										{
											edec = spd * spd / (2.0 * dist);
										}
										else
										{
											edec = BrakeDeceleration;
										}
										if (edec > dec) dec = edec;
									}
								}
							}
						}
					}
					// buffers ahead
					if (Train.IsPlayerTrain)
					{
						for (int i = 0; i < BufferTrackPositions.Length; i++)
						{
							double dist = BufferTrackPositions[i] - tp;
							if (dist > 0.0)
							{
								double edec;
								if (dist >= 10.0)
								{
									edec = spd * spd / (2.0 * dist);
								}
								else if (dist >= 5.0)
								{
									Train.ApplyNotch(-1, true, 1, true);
									Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Service);
									this.CurrentInterval = 0.1;
									return;
								}
								else
								{
									Train.ApplyNotch(-1, true, 1, true);
									Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Service);
									Train.ApplyEmergencyBrake();
									this.CurrentInterval = 10.0;
									return;
								}
								if (edec > dec) dec = edec;
							}
						}
					}
					// trains ahead
					for (int i = 0; i < TrainManager.Trains.Length; i++)
					{
						if (TrainManager.Trains[i] != Train && TrainManager.Trains[i].State == TrainState.Available)
						{
							double pos =
								TrainManager.Trains[i].Cars[TrainManager.Trains[i].Cars.Length - 1].RearAxle.Follower.TrackPosition -
								TrainManager.Trains[i].Cars[TrainManager.Trains[i].Cars.Length - 1].RearAxle.Position -
								0.5 * TrainManager.Trains[i].Cars[TrainManager.Trains[i].Cars.Length - 1].Length;
							double dist = pos - tp;
							if (dist > -10.0 & dist < lookahead)
							{
								const double minDistance = 10.0;
								const double maxDistance = 100.0;
								double edec;
								if (dist > minDistance)
								{
									double shift = 0.75 * minDistance + 1.0 * spd;
									edec = spd * spd / (2.0 * (dist - shift));
								}
								else if (dist > 0.5 * minDistance)
								{
									Train.ApplyNotch(-1, true, 1, true);
									Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Service);
									this.CurrentInterval = 0.1;
									return;
								}
								else
								{
									Train.ApplyNotch(-1, true, 1, true);
									Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Service);
									Train.ApplyEmergencyBrake();
									this.CurrentInterval = 1.0;
									return;
								}
								if (dist < maxDistance)
								{
									reduceDecelerationCruiseAndStart = true;
								}
								if (edec > dec) dec = edec;
							}
						}
					}
					Train.UnapplyEmergencyBrake();
					// current station
					if (Train.Station >= 0 & Train.StationState == TrainStopState.Pending)
					{
						if (StopsAtStation(Train.Station, Train))
						{
							int s = Program.CurrentRoute.Stations[Train.Station].GetStopIndex(Train.NumberOfCars);
							if (s >= 0)
							{
								double dist = Program.CurrentRoute.Stations[Train.Station].Stops[s].TrackPosition - tp;
								if (dist > 0.0)
								{
									if (dist < 25.0)
									{
										reduceDecelerationCruiseAndStart = true;
									}
									else if (this.CurrentSpeedFactor < 1.0)
									{
										dist -= 5.0;
									}
									var edec = spd * spd / (2.0 * dist);
									if (edec > dec) dec = edec;
								}
								else
								{
									dec = BrakeDeceleration;
								}
							}
						}
					}
					// power / brake
					if (reduceDecelerationCruiseAndStart)
					{
						decelerationCruise *= 0.3;
						decelerationStart *= 0.3;
					}
					double brakeModeBrakeThreshold = 0.75 * decelerationStart + 0.25 * decelerationCruise;
					if (!BrakeMode & dec > decelerationStart | BrakeMode & dec > brakeModeBrakeThreshold | false)
					{
						// brake
						BrakeMode = true;
						double decdiff = -acc - dec;
						if (decdiff < -decelerationStep)
						{
							// brake start
							if (Train.Handles.Power.Driver == 0)
							{
								Train.ApplyNotch(0, true, 1, true);
								Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Service);
							}
							else
							{
								Train.ApplyNotch(-1, true, 0, true);
							}
							CurrentInterval *= 0.4;
							if (CurrentInterval < 0.3) CurrentInterval = 0.3;
						}
						else if (decdiff > decelerationStep)
						{
							// brake stop
							Train.ApplyNotch(-1, true, -1, true);
							Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
							CurrentInterval *= 0.4;
							if (CurrentInterval < 0.3) CurrentInterval = 0.3;
						}
						else
						{
							// keep brake
							Train.ApplyNotch(-1, true, 0, true);
							Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Lap);
							CurrentInterval *= 1.2;
							if (CurrentInterval > 1.0) CurrentInterval = 1.0;
						}
						if (Train.Handles.Power.Driver == 0 & Train.Handles.Brake.Driver == 0)
						{
							Train.ApplyHoldBrake(Train.Handles.HasHoldBrake);
						}
						if (Train.Handles.Brake is TrainManager.AirBrakeHandle)
						{
							CurrentInterval = 0.1;
						}
					}
					else if (dec > decelerationCruise)
					{
						// cut power/brake
						BrakeMode = false;
						Train.ApplyNotch(-1, true, -1, true);
						Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
						if (Train.Handles.Power.Driver == 0 & Train.Handles.Brake.Driver == 0)
						{
							Train.ApplyHoldBrake(Train.Handles.HasHoldBrake);
						}
						CurrentInterval *= 0.4;
						if (CurrentInterval < 0.3) CurrentInterval = 0.3;
					}
					else
					{
						// power
						BrakeMode = false;
						double acclim;
						if (!double.IsInfinity(lim))
						{
							double d = lim - spd;
							if (d > 0.0)
							{
								acclim = 0.1 / (0.1 * d + 1.0) - 0.12;
							}
							else
							{
								acclim = -1.0;
							}
						}
						else
						{
							acclim = -1.0;
						}
						if (spd < powerstart)
						{
							// power start (under-speed)
							if (Train.Handles.Brake.Driver == 0)
							{
								if (Train.Handles.Power.Driver < this.PowerNotchAtWhichWheelSlipIsObserved - 1)
								{
									Train.ApplyNotch(1, true, 0, true);
								}
							}
							else
							{
								Train.ApplyNotch(0, true, -1, true);
							}
							Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
							if (double.IsPositiveInfinity(powerstart))
							{
								CurrentInterval = 0.3 + 0.1 * Train.Handles.Power.Driver;
							}
							else
							{
								double p = (double)Train.Handles.Power.Driver / (double)Train.Handles.Power.MaximumNotch;
								CurrentInterval = 0.3 + 15.0 * p / (powerstart - spd + 1.0);
							}
							if (CurrentInterval > 1.3) CurrentInterval = 1.3;
						}
						else if (spd > powerend)
						{
							// power end (over-speed)
							Train.ApplyNotch(-1, true, -1, true);
							Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
							CurrentInterval *= 0.3;
							if (CurrentInterval < 0.2) CurrentInterval = 0.2;
						}
						else if (acc < acclim)
						{
							// power start (under-acceleration)
							if (Train.Handles.Brake.Driver == 0)
							{
								if (Train.Handles.Power.Driver < this.PowerNotchAtWhichWheelSlipIsObserved - 1)
								{
									if (Train.Handles.Power.Driver == Train.Handles.Power.Actual)
									{
										Train.ApplyNotch(1, true, 0, true);
									}
								}
							}
							else
							{
								Train.ApplyNotch(0, true, -1, true);
							}
							Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
							CurrentInterval = 1.3;
						}
						else
						{
							// keep power
							Train.ApplyNotch(0, true, -1, true);
							Train.ApplyAirBrakeHandle(TrainManager.AirBrakeHandleState.Release);
							if (Train.Handles.Power.Driver != 0)
							{
								Train.Specs.CurrentConstSpeed = Train.Specs.HasConstSpeed;
							}
							if (Train.Handles.Power.Driver == 0 & Train.Handles.Brake.Driver == 0)
							{
								Train.ApplyHoldBrake(Train.Handles.HasHoldBrake);
							}
							CurrentInterval *= 1.1;
							if (CurrentInterval > 1.5) CurrentInterval = 1.5;
						}
					}
				}
			}
			internal override void Trigger(double TimeElapsed)
			{
				if (TimeLastProcessed > Program.CurrentRoute.SecondsSinceMidnight)
				{
					TimeLastProcessed = Program.CurrentRoute.SecondsSinceMidnight;
				}
				else if (Program.CurrentRoute.SecondsSinceMidnight - TimeLastProcessed >= CurrentInterval)
				{
					TimeLastProcessed = Program.CurrentRoute.SecondsSinceMidnight;
					if (Train.Plugin != null && Train.Plugin.SupportsAI)
					{
						if (PerformPlugin() != AIResponse.None)
						{
							return;
						}
					}
					PerformDefault();
				}
			}
		}
	}
}
