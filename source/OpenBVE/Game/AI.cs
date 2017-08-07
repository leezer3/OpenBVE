﻿using System;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>An abstract class representing a general purpose AI</summary>
		internal abstract class GeneralAI
		{
			internal abstract void Trigger(TrainManager.Train Train, double TimeElapsed);
		}

		internal static bool InitialAIDriver;

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
			// functions
			internal SimpleHumanDriverAI(TrainManager.Train Train)
			{
				this.TimeLastProcessed = 0.0;
				this.CurrentInterval = 1.0;
				this.BrakeMode = false;
				this.PersonalitySpeedFactor = 0.90 + 0.10 * Program.RandomNumberGenerator.NextDouble();
				this.CurrentSpeedFactor = this.PersonalitySpeedFactor;
				this.PowerNotchAtWhichWheelSlipIsObserved = Train.Specs.MaximumPowerNotch + 1;
				if (Train.StationInfo.NextStation >= 0 & Train.StationInfo.CurrentStopState == TrainManager.TrainStopState.Boarding)
				{
					this.LastStation = Train.StationInfo.NextStation;
				}
				else
				{
					this.LastStation = -1;
				}
			}
			private OpenBveApi.Runtime.AIResponse PerformPlugin(TrainManager.Train Train)
			{
				OpenBveApi.Runtime.AIResponse response = Train.Plugin.UpdateAI();
				if (response == OpenBveApi.Runtime.AIResponse.Short)
				{
					CurrentInterval = 0.2 + 0.1 * Program.RandomNumberGenerator.NextDouble();
				}
				else if (response == OpenBveApi.Runtime.AIResponse.Medium)
				{
					CurrentInterval = 0.4 + 0.2 * Program.RandomNumberGenerator.NextDouble();
				}
				else if (response == OpenBveApi.Runtime.AIResponse.Long)
				{
					CurrentInterval = 0.8 + 0.4 * Program.RandomNumberGenerator.NextDouble();
				}
				return response;
			}
			private void PerformDefault(TrainManager.Train Train)
			{
				// personality
				double spd = Train.Specs.CurrentAverageSpeed;
				if (Train.StationInfo.NextStation >= 0 & Train.StationInfo.CurrentStopState == TrainManager.TrainStopState.Boarding)
				{
					if (Train.StationInfo.NextStation != this.LastStation)
					{
						this.LastStation = Train.StationInfo.NextStation;
						double time;
						if (Stations[Train.StationInfo.NextStation].ArrivalTime >= 0.0)
						{
							time = Stations[Train.StationInfo.NextStation].ArrivalTime - Train.TimetableDelta;
						}
						else if (Stations[Train.StationInfo.NextStation].DepartureTime >= 0.0)
						{
							time = Stations[Train.StationInfo.NextStation].DepartureTime - Train.TimetableDelta;
							if (time > SecondsSinceMidnight)
							{
								time -= Stations[Train.StationInfo.NextStation].StopTime;
								if (time > SecondsSinceMidnight)
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
							double diff = SecondsSinceMidnight - time;
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
				TrainManager.ApplyHoldBrake(Train, false);
				int stopIndex = Train.StationInfo.NextStation >= 0 ? GetStopIndex(Train.StationInfo.NextStation, Train.Cars.Length) : -1;
				if (Train.CurrentSectionLimit == 0.0)
				{
					// passing red signal
					Train.EmergencyBrake.Apply();
					Train.ApplyNotch(-1, true, 1, true);
					CurrentInterval = 0.5;
				}
				else if (doorsopen | Train.StationInfo.CurrentStopState == TrainManager.TrainStopState.Boarding)
				{
					// door opened or boarding at station
					this.PowerNotchAtWhichWheelSlipIsObserved = Train.Specs.MaximumPowerNotch + 1;
					if (Train.StationInfo.NextStation >= 0 && Stations[Train.StationInfo.NextStation].StationType != StationType.Normal && Train == TrainManager.PlayerTrain)
					{
						// player's terminal station
	                    if (Train.Plugin == null || Train.Plugin.LastReverser == -2)
	                    {
							TrainManager.ApplyReverser(Train, 0, false);
	                    }
	                    Train.ApplyNotch(-1, true, 1, true);
						TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
						Train.EmergencyBrake.Apply();
						CurrentInterval = 1.0;
					}
					else
					{
						CurrentInterval = 1.0;
						Train.ApplyNotch(-1, true, 0, true);
						if (Train.Cars[Train.DriverCar].BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake)
						{
							if (Train.Cars[Train.DriverCar].AirBrake.BrakeCylinder.CurrentPressure < 0.3 * Train.Cars[Train.DriverCar].AirBrake.BrakeCylinder.ServiceMaximumPressure)
							{
								TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
							}
							else if (Train.Cars[Train.DriverCar].AirBrake.BrakeCylinder.CurrentPressure > 0.9 * Train.Cars[Train.DriverCar].AirBrake.BrakeCylinder.EmergencyMaximumPressure)
							{
								TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
							}
							else
							{
								TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Lap);
							}
						}
						else
						{
							int b;
							if (Math.Abs(spd) < 0.02)
							{
								b = (int)Math.Ceiling(0.5 * (double)Train.Specs.MaximumBrakeNotch);
								CurrentInterval = 0.3;
							}
							else
							{
								b = Train.Specs.MaximumBrakeNotch;
							}
							if (Train.Specs.CurrentBrakeNotch.Driver < b)
							{
								Train.ApplyNotch(0, true, 1, true);
							}
							else if (Train.Specs.CurrentBrakeNotch.Driver > b)
							{
								Train.ApplyNotch(0, true, -1, true);
							}
						}
						Train.EmergencyBrake.Release();
						if (Train.StationInfo.NextStation >= 0 & Train.StationInfo.CurrentStopState == TrainManager.TrainStopState.Completed)
						{
							// ready for departure - close doors
							if (Train.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic && Train.Specs.DoorInterlockState == TrainManager.DoorInterlockStates.Unlocked)
							{
								TrainManager.CloseTrainDoors(Train, true, true);
							}
						}
						else if (Train.StationInfo.NextStation >= 0 & Train.StationInfo.CurrentStopState == TrainManager.TrainStopState.Boarding)
						{
						}
						else
						{
							// not at station - close doors
							if (Train.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic && Train.Specs.DoorInterlockState == TrainManager.DoorInterlockStates.Unlocked)
							{
								TrainManager.CloseTrainDoors(Train, true, true);
							}
						}
					}
				}
				else if (Train.StationInfo.NextStation >= 0 && stopIndex >= 0 && Train.StationInfo.DistanceToStopPosition < Stations[Train.StationInfo.NextStation].Stops[stopIndex].BackwardTolerance && (StopsAtStation(Train.StationInfo.NextStation, Train) & (Stations[Train.StationInfo.NextStation].OpenLeftDoors | Stations[Train.StationInfo.NextStation].OpenRightDoors) & Math.Abs(Train.Specs.CurrentAverageSpeed) < 0.25 & Train.StationInfo.CurrentStopState == TrainManager.TrainStopState.Pending))
				{
					// arrived at station - open doors
					if (Train.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic && Train.Specs.DoorInterlockState == TrainManager.DoorInterlockStates.Unlocked)
					{
						TrainManager.OpenTrainDoors(Train, Stations[Train.StationInfo.NextStation].OpenLeftDoors, Stations[Train.StationInfo.NextStation].OpenRightDoors);
					}
					CurrentInterval = 1.0;
				}
				else if (Train.StationInfo.NextStation >= 0 && stopIndex >= 0 && Stations[Train.StationInfo.NextStation].StationType != StationType.Normal && Train == TrainManager.PlayerTrain && Train.StationInfo.DistanceToStopPosition < Stations[Train.StationInfo.NextStation].Stops[stopIndex].BackwardTolerance && -Train.StationInfo.DistanceToStopPosition < Stations[Train.StationInfo.NextStation].Stops[stopIndex].ForwardTolerance && Math.Abs(Train.Specs.CurrentAverageSpeed) < 0.25)
				{
					// player's terminal station (not boarding any longer)
	                if (Train.Plugin != null || Train.Plugin.LastReverser == -2)
	                {
		                TrainManager.ApplyReverser(Train, 0, false);
			}
			                Train.ApplyNotch(-1, true, 1, true);
					TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
					Train.EmergencyBrake.Apply();
					CurrentInterval = 10.0;
				}
				else
				{
					// drive
					TrainManager.ApplyReverser(Train, 1, false);
					if (Train.Cars[Train.DriverCar].FrontAxle.CurrentWheelSlip | Train.Cars[Train.DriverCar].RearAxle.CurrentWheelSlip)
					{
						// react to wheel slip
						if (Train.Specs.CurrentPowerNotch.Driver > 1)
						{
							this.PowerNotchAtWhichWheelSlipIsObserved = Train.Specs.CurrentPowerNotch.Driver;
							Train.ApplyNotch(-1, true, -1, true);
							TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
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
					double BrakeDeceleration = Train.Cars[Train.DriverCar].AirBrake.DecelerationAtServiceMaximumPressure;
					for (int i = 0; i < Train.Cars.Length; i++)
					{
						if (Train.Cars[i].Specs.IsMotorCar)
						{
							if (Train.Cars[Train.DriverCar].Specs.MotorDeceleration < BrakeDeceleration)
							{
								BrakeDeceleration = Train.Cars[Train.DriverCar].Specs.MotorDeceleration;
							}
							break;
						}
					}
					if (Train.Cars[Train.DriverCar].BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake | Train.Specs.MaximumBrakeNotch <= 0)
					{
						decelerationCruise = 0.3 * BrakeDeceleration;
						decelerationStart = 0.5 * BrakeDeceleration;
						decelerationStep = 0.1 * BrakeDeceleration;
					}
					else if (Train.Specs.MaximumBrakeNotch <= 2)
					{
						decelerationCruise = 0.2 * BrakeDeceleration;
						decelerationStart = 0.4 * BrakeDeceleration;
						decelerationStep = 0.5 * BrakeDeceleration;
					}
					else
					{
						decelerationCruise = 0.2 * BrakeDeceleration;
						decelerationStart = 0.5 * BrakeDeceleration;
						decelerationStep = BrakeDeceleration / (double)Train.Specs.MaximumBrakeNotch;
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
					double lookahead = (Train.StationInfo.NextStation >= 0 ? 150.0 : 50.0) + (spd * spd) / (2.0 * decelerationCruise);
                    double tp = Train.Cars[0].FrontAxle.Follower.TrackPosition - Train.Cars[0].FrontAxle.Position + 0.5 * Train.Cars[0].Length;
					double stopDistance = double.MaxValue;
					{
						// next station stop
						int te = Train.Cars[0].FrontAxle.Follower.LastTrackElement;
						for (int i = te; i < TrackManager.CurrentTrack.Elements.Length; i++)
						{
							double stp = TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
							if (tp + lookahead <= stp) break;
							for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++)
							{
								if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent)
								{
									TrackManager.StationStartEvent e = (TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
									if (StopsAtStation(e.StationIndex, Train) & Train.StationInfo.PreviousStation != e.StationIndex)
									{
										int s = GetStopIndex(e.StationIndex, Train.Cars.Length);
										if (s >= 0)
										{
											double dist = Stations[e.StationIndex].Stops[s].TrackPosition - tp;
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
						for (int i = te; i < TrackManager.CurrentTrack.Elements.Length; i++)
						{
							double stp = TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
							if (tp + lookahead <= stp) break;
							for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++)
							{
								if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.LimitChangeEvent)
								{
									// speed limit
									TrackManager.LimitChangeEvent e = (TrackManager.LimitChangeEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
									if (e.NextSpeedLimit < spd)
									{
										double dist = stp + e.TrackPositionDelta - tp;
										double edec = (spd * spd - e.NextSpeedLimit * e.NextSpeedLimit * this.CurrentSpeedFactor) / (2.0 * dist);
										if (edec > dec) dec = edec;
									}
								}
								else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.SectionChangeEvent)
								{
									// section
									TrackManager.SectionChangeEvent e = (TrackManager.SectionChangeEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
									if (stp + e.TrackPositionDelta > tp)
									{
										if (!Game.Sections[e.NextSectionIndex].Invisible & Game.Sections[e.NextSectionIndex].CurrentAspect >= 0)
										{
											double elim = Game.Sections[e.NextSectionIndex].Aspects[Game.Sections[e.NextSectionIndex].CurrentAspect].Speed * this.CurrentSpeedFactor;
											if (elim < spd | spd <= 0.0)
											{
												double dist = stp + e.TrackPositionDelta - tp;
												double edec;
												if (elim == 0.0)
												{
													double redstopdist;
													if (Train.StationInfo.NextStation >= 0 & Train.StationInfo.CurrentStopState == TrainManager.TrainStopState.Completed & dist < 120.0)
													{
														dist = 1.0;
														redstopdist = 25.0;
													}
													else if (Train.StationInfo.NextStation >= 0 & Train.StationInfo.CurrentStopState == TrainManager.TrainStopState.Pending | stopDistance < dist)
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
								else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent)
								{
									// station start
									if (Train.StationInfo.NextStation == -1)
									{
										TrackManager.StationStartEvent e = (TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
										if (StopsAtStation(e.StationIndex, Train) & Train.StationInfo.PreviousStation != e.StationIndex)
										{
											int s = GetStopIndex(e.StationIndex, Train.Cars.Length);
											if (s >= 0)
											{
												double dist = Stations[e.StationIndex].Stops[s].TrackPosition - tp;
												if (dist > -Stations[e.StationIndex].Stops[s].ForwardTolerance)
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
								else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationEndEvent)
								{
									// station end
									if (Train.StationInfo.NextStation == -1)
									{
										TrackManager.StationEndEvent e = (TrackManager.StationEndEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
										if (StopsAtStation(e.StationIndex, Train) & Train.StationInfo.PreviousStation != e.StationIndex)
										{
											int s = GetStopIndex(e.StationIndex, Train.Cars.Length);
											if (s >= 0)
											{
												double dist = Stations[e.StationIndex].Stops[s].TrackPosition - tp;
												if (dist > -Stations[e.StationIndex].Stops[s].ForwardTolerance)
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
								else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.TrackEndEvent)
								{
									// track end
									if (Train == TrainManager.PlayerTrain)
									{
										TrackManager.TrackEndEvent e = (TrackManager.TrackEndEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
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
					if (Train == TrainManager.PlayerTrain)
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
									TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
									this.CurrentInterval = 0.1;
									return;
								}
								else
								{
									Train.ApplyNotch(-1, true, 1, true);
									TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
									Train.EmergencyBrake.Apply();
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
						if (TrainManager.Trains[i] != Train && TrainManager.Trains[i].State == TrainManager.TrainState.Available)
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
									TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
									this.CurrentInterval = 0.1;
									return;
								}
								else
								{
									Train.ApplyNotch(-1, true, 1, true);
									TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
									Train.EmergencyBrake.Apply();
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
					Train.EmergencyBrake.Release();
					// current station
					if (Train.StationInfo.NextStation >= 0 & Train.StationInfo.CurrentStopState == TrainManager.TrainStopState.Pending)
					{
						if (StopsAtStation(Train.StationInfo.NextStation, Train))
						{
							int s = GetStopIndex(Train.StationInfo.NextStation, Train.Cars.Length);
							if (s >= 0)
							{
								double dist = Stations[Train.StationInfo.NextStation].Stops[s].TrackPosition - tp;
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
							if (Train.Specs.CurrentPowerNotch.Driver == 0)
							{
								Train.ApplyNotch(0, true, 1, true);
								TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
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
							TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
							CurrentInterval *= 0.4;
							if (CurrentInterval < 0.3) CurrentInterval = 0.3;
						}
						else
						{
							// keep brake
							Train.ApplyNotch(-1, true, 0, true);
							TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Lap);
							CurrentInterval *= 1.2;
							if (CurrentInterval > 1.0) CurrentInterval = 1.0;
						}
						if (Train.Specs.CurrentPowerNotch.Driver == 0 & Train.Specs.CurrentBrakeNotch.Driver == 0)
						{
							TrainManager.ApplyHoldBrake(Train, Train.Specs.HasHoldBrake);
						}
						if (Train.Cars[Train.DriverCar].BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake)
						{
							CurrentInterval = 0.1;
						}
					}
					else if (dec > decelerationCruise)
					{
						// cut power/brake
						BrakeMode = false;
						Train.ApplyNotch(-1, true, -1, true);
						TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
						if (Train.Specs.CurrentPowerNotch.Driver == 0 & Train.Specs.CurrentBrakeNotch.Driver == 0)
						{
							TrainManager.ApplyHoldBrake(Train, Train.Specs.HasHoldBrake);
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
							if (Train.Specs.CurrentBrakeNotch.Driver == 0)
							{
								if (Train.Specs.CurrentPowerNotch.Driver < this.PowerNotchAtWhichWheelSlipIsObserved - 1)
								{
									Train.ApplyNotch(1, true, 0, true);
								}
							}
							else
							{
								Train.ApplyNotch(0, true, -1, true);
							}
							TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
							if (double.IsPositiveInfinity(powerstart))
							{
								CurrentInterval = 0.3 + 0.1 * Train.Specs.CurrentPowerNotch.Driver;
							}
							else
							{
								double p = (double)Train.Specs.CurrentPowerNotch.Driver / (double)Train.Specs.MaximumPowerNotch;
								CurrentInterval = 0.3 + 15.0 * p / (powerstart - spd + 1.0);
							}
							if (CurrentInterval > 1.3) CurrentInterval = 1.3;
						}
						else if (spd > powerend)
						{
							// power end (over-speed)
							Train.ApplyNotch(-1, true, -1, true);
							TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
							CurrentInterval *= 0.3;
							if (CurrentInterval < 0.2) CurrentInterval = 0.2;
						}
						else if (acc < acclim)
						{
							// power start (under-acceleration)
							if (Train.Specs.CurrentBrakeNotch.Driver == 0)
							{
								if (Train.Specs.CurrentPowerNotch.Driver < this.PowerNotchAtWhichWheelSlipIsObserved - 1)
								{
									if (Train.Specs.CurrentPowerNotch.Driver == Train.Specs.CurrentPowerNotch.Actual)
									{
										Train.ApplyNotch(1, true, 0, true);
									}
								}
							}
							else
							{
								Train.ApplyNotch(0, true, -1, true);
							}
							TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
							CurrentInterval = 1.3;
						}
						else
						{
							// keep power
							Train.ApplyNotch(0, true, -1, true);
							TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
							if (Train.Specs.CurrentPowerNotch.Driver != 0)
							{
								Train.Specs.CurrentConstSpeed = Train.Specs.HasConstSpeed;
							}
							if (Train.Specs.CurrentPowerNotch.Driver == 0 & Train.Specs.CurrentBrakeNotch.Driver == 0)
							{
								TrainManager.ApplyHoldBrake(Train, Train.Specs.HasHoldBrake);
							}
							CurrentInterval *= 1.1;
							if (CurrentInterval > 1.5) CurrentInterval = 1.5;
						}
					}
				}
			}
			internal override void Trigger(TrainManager.Train Train, double TimeElapsed)
			{
				if (TimeLastProcessed > SecondsSinceMidnight)
				{
					TimeLastProcessed = SecondsSinceMidnight;
				}
				else if (SecondsSinceMidnight - TimeLastProcessed >= CurrentInterval)
				{
					TimeLastProcessed = SecondsSinceMidnight;
					if (Train.Plugin != null && Train.Plugin.SupportsAI)
					{
						if (PerformPlugin(Train) != OpenBveApi.Runtime.AIResponse.None)
						{
							return;
						}
					}
					PerformDefault(Train);
				}
			}
		}

		// bogus pretrain
		internal struct BogusPretrainInstruction
		{
			internal double TrackPosition;
			internal double Time;
		}
		internal class BogusPretrainAI : GeneralAI
		{
			private double TimeLastProcessed;
			private double CurrentInterval;
			internal BogusPretrainAI(TrainManager.Train Train)
			{
				this.TimeLastProcessed = 0.0;
				this.CurrentInterval = 1.0;
			}
			internal override void Trigger(TrainManager.Train Train, double TimeElapsed)
			{
				if (SecondsSinceMidnight - TimeLastProcessed >= CurrentInterval)
				{
					TimeLastProcessed = SecondsSinceMidnight;
					CurrentInterval = 5.0;
					double ap = double.MaxValue, at = double.MaxValue;
					double bp = double.MinValue, bt = double.MinValue;
					for (int i = 0; i < BogusPretrainInstructions.Length; i++)
					{
						if (BogusPretrainInstructions[i].Time < SecondsSinceMidnight | at == double.MaxValue)
						{
							at = BogusPretrainInstructions[i].Time;
							ap = BogusPretrainInstructions[i].TrackPosition;
						}
					}
					for (int i = BogusPretrainInstructions.Length - 1; i >= 0; i--)
					{
						if (BogusPretrainInstructions[i].Time > at | bt == double.MinValue)
						{
							bt = BogusPretrainInstructions[i].Time;
							bp = BogusPretrainInstructions[i].TrackPosition;
						}
					}
					if (at != double.MaxValue & bt != double.MinValue & SecondsSinceMidnight <= BogusPretrainInstructions[BogusPretrainInstructions.Length - 1].Time)
					{
						double r = bt - at;
						if (r > 0.0)
						{
							r = (SecondsSinceMidnight - at) / r;
							if (r < 0.0) r = 0.0;
							if (r > 1.0) r = 1.0;
						}
						else
						{
							r = 0.0;
						}
						double p = ap + r * (bp - ap);
						double d = p - Train.Cars[0].FrontAxle.Follower.TrackPosition;
						for (int j = 0; j < Train.Cars.Length; j++)
						{
							Train.Cars[j].Move(d, 0.1);
						}
					}
					else
					{
						Train.Dispose();
					}
				}
			}
		}
	}
}
