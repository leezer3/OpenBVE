using System;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Motor;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using RouteManager2.Events;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Trains;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>This class forms an AI representation of a simple human driver</summary>
		internal class SimpleHumanDriverAI : GeneralAI
		{
			/// <summary>The time last processed, in seconds since midnight</summary>
			private double TimeLastProcessed;
			/// <summary>The processing interval in seconds</summary>
			private double CurrentInterval;
			/// <summary>Whether the train is currently braking to a stop</summary>
			private bool BrakeMode;
			/// <summary>The current percentage of the limit the driver is aiming for</summary>
			private double CurrentSpeedFactor;
			/// <summary>A random base speed factor for the driver personality</summary>
			/// <remarks>Controls the percentage of the speed limit the driver aims for</remarks>
			private readonly double PersonalitySpeedFactor;
			/// <summary>The last notch at which wheelslip was observed</summary>
			private int PowerNotchAtWhichWheelSlipIsObserved;
			/// <summary>The index of the last station the train called at</summary>
			private int LastStation;
			/// <summary>The AI speed limit (if set by routefile)</summary>
			private readonly double SpeedLimit;
			/// <summary>Holds a reference to the train the AI is driving</summary>
			private readonly TrainBase Train;
			/// <summary>The index to the first motor car, if the driver car is not a motor car</summary>
			private readonly int MotorCar;
			// functions
			internal SimpleHumanDriverAI(TrainBase train, double Limit)
			{
				Train = train;
				TimeLastProcessed = 0.0;
				CurrentInterval = 1.0;
				BrakeMode = false;
				PersonalitySpeedFactor = 0.90 + 0.10 * Program.RandomNumberGenerator.NextDouble();
				CurrentSpeedFactor = PersonalitySpeedFactor;
				PowerNotchAtWhichWheelSlipIsObserved = Train.Handles.Power.MaximumNotch + 1;
				if (Train.Station >= 0 & Train.StationState == TrainStopState.Boarding)
				{
					LastStation = Train.Station;
				}
				else
				{
					LastStation = -1;
				}
				SpeedLimit = Limit;
				MotorCar = train.DriverCar;
				if (!train.Cars[train.DriverCar].TractionModel.ProvidesPower)
				{
					for (int i = 0; i < train.Cars.Length; i++)
					{
						if (train.Cars[i].TractionModel.ProvidesPower)
						{
							MotorCar = i;
							break;
						}
					}
				}
			}

			/// <summary>Sets the response time for actions triggered by a runtime plugin</summary>
			/// <param name="timeElapsed">The elapsed time</param>
			/// <returns>The response time</returns>
			private AIResponse PerformPlugin(double timeElapsed)
			{
				AIResponse response = Train.Plugin.UpdateAI(timeElapsed);
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

			// Temp variables
			private double dec;
			private double decelerationCruise;   /* power below this deceleration, cruise above */
			private double decelerationStart;    /* brake above this deceleration, cruise below */
			private double decelerationStep;     /* the deceleration step per brake notch */
			private double brakeDeceleration;
			private bool reduceDecelerationCruiseAndStart;

			/// <summary>Performs all default actions</summary>
			private void PerformDefault()
			{
				if (Train.Derailed)
				{
					if (Train.Handles.EmergencyBrake.Driver != true)
					{
						Train.Handles.EmergencyBrake.Apply();
					}
					return;
				}
				// personality
				if (Train.Station >= 0 & Train.StationState == TrainStopState.Boarding)
				{
					if (Train.Station != LastStation)
					{
						LastStation = Train.Station;
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
								CurrentSpeedFactor -= largeChangeFactor * (-diff - largeThreshold);
								if (CurrentSpeedFactor < 0.7)
								{
									CurrentSpeedFactor = 0.7;
								}
							}
							else if (diff > largeThreshold)
							{
								/* The AI is too slow. Increase the preferred speed. */
								CurrentSpeedFactor += largeChangeFactor * (diff - largeThreshold);
								if (CurrentSpeedFactor > 1.1)
								{
									CurrentSpeedFactor = 1.1;
								}
							}
							else if (Math.Abs(diff) < smallThreshold)
							{
								/* The AI is at about the right speed. Change the preferred speed toward the personality default. */
								if (CurrentSpeedFactor < PersonalitySpeedFactor)
								{
									CurrentSpeedFactor += smallChange;
									if (CurrentSpeedFactor > PersonalitySpeedFactor)
									{
										CurrentSpeedFactor = PersonalitySpeedFactor;
									}
								}
								else if (CurrentSpeedFactor > PersonalitySpeedFactor)
								{
									CurrentSpeedFactor -= smallChange;
									if (CurrentSpeedFactor < PersonalitySpeedFactor)
									{
										CurrentSpeedFactor = PersonalitySpeedFactor;
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
				Train.Handles.HoldBrake.ApplyState(false);
				foreach (CarBase car in Train.Cars)
				{
					if (car.TractionModel.Components.TryGetTypedValue(EngineComponent.Pantograph, out Pantograph pantograph) && pantograph.State == PantographState.Lowered)
					{
						pantograph.Raise();
					}
					if (car.TractionModel.Components.TryGetTypedValue(EngineComponent.Gearbox, out Gearbox gearbox))
					{
						if (Train.CurrentSpeed == 0 && Train.Handles.Power.Actual == 0 && gearbox.CurrentGear > 0)
						{
							// we should be in N when at rest
							gearbox.GearDown();
						}
						else
						{
							if (Train.CurrentSpeed < gearbox.PreviousMaximumGearSpeed)
							{
								// slow enough for previous gear
								gearbox.GearDown();
							}
							else if (Train.CurrentSpeed >= gearbox.MaximumGearSpeed)
							{
								// gear change up
								gearbox.GearUp();
							}
						}
					}
				}
				
				int stopIndex = Train.Station >= 0 ? Program.CurrentRoute.Stations[Train.Station].GetStopIndex(Train) : -1;
				if (Train.CurrentSectionLimit == 0.0)
				{
					// passing red signal
					Train.Handles.EmergencyBrake.Apply();
					Train.Handles.Brake.ApplyState(1, true);
					Train.Handles.Power.ApplyState(-1, true);
					CurrentInterval = 0.5;
				}
				else if (doorsopen | Train.StationState == TrainStopState.Boarding)
				{
					// door opened or boarding at station
					PowerNotchAtWhichWheelSlipIsObserved = Train.Handles.Power.MaximumNotch + 1;
					if (Train.Station >= 0 && Program.CurrentRoute.Stations[Train.Station].Type != StationType.Normal && Program.CurrentRoute.Stations[Train.Station].Type != StationType.RequestStop && Train.IsPlayerTrain)
					{
						// player's terminal station
						if (Train.Plugin == null || Train.Plugin.LastReverser == -2)
						{
							Train.Handles.Reverser.ApplyState(ReverserPosition.Neutral);
						}
						Train.Handles.Brake.ApplyState(1, true);
						Train.Handles.Power.ApplyState(-1, true);
						if (!Train.Handles.EmergencyBrake.Driver)
						{
							Train.Handles.EmergencyBrake.Apply();
						}
						if (Train.Station >= 0 && stopIndex >= 0 && Train.StationDistanceToStopPoint < Program.CurrentRoute.Stations[Train.Station].Stops[stopIndex].BackwardTolerance && (Program.CurrentRoute.Stations[Train.Station].StopsHere(Train) & (Program.CurrentRoute.Stations[Train.Station].OpenLeftDoors | Program.CurrentRoute.Stations[Train.Station].OpenRightDoors) & Math.Abs(Train.CurrentSpeed) < 0.25 & Train.StationState == TrainStopState.Pending))
						{
							// doors not fully open at station - open doors
							if (Train.Specs.DoorOpenMode != DoorMode.Automatic)
							{
								AttemptToOpenDoors();
							}
						}
						CurrentInterval = 1.0;
					}
					else
					{
						CurrentInterval = 1.0;
						if (Train.Handles.Power.Actual > 1)
						{
							Train.Handles.Power.ApplyState(-1, true);
						}
						
						if (Train.Handles.Brake is AirBrakeHandle)
						{
							if (Train.StationDepartureTime - Program.CurrentRoute.SecondsSinceMidnight > 10 || Train.Cars[Train.DriverCar].CarBrake.BrakeCylinder.CurrentPressure < 0.3 * Train.Cars[Train.DriverCar].CarBrake.BrakeCylinder.ServiceMaximumPressure)
							{
								Train.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
							}
							else if (Train.Cars[Train.DriverCar].CarBrake.BrakeCylinder.CurrentPressure > 0.9 * Train.Cars[Train.DriverCar].CarBrake.BrakeCylinder.EmergencyMaximumPressure)
							{
								Train.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
							}
							else
							{
								Train.Handles.Brake.ApplyState(AirBrakeHandleState.Lap);
							}
						}
						else
						{
							int b;
							if (Math.Abs(Train.CurrentSpeed) < 0.02)
							{
								b = (int)Math.Ceiling(0.5 * Train.Handles.Brake.MaximumNotch);
								CurrentInterval = 0.3;
							}
							else
							{
								b = Train.Handles.Brake.MaximumNotch;
							}
							if (Train.Handles.Brake.Driver < b)
							{
								Train.Handles.Brake.ApplyState(1, true);
							}
							else if (Train.Handles.Brake.Driver > b)
							{
								Train.Handles.Brake.ApplyState(-1, true);
							}
						}
						Train.Handles.EmergencyBrake.Release();
						if (Train.Station >= 0 & Train.StationState == TrainStopState.Completed)
						{
							// ready for departure - close doors
							if (Train.Specs.DoorOpenMode != DoorMode.Automatic)
							{
								doorOpenAttempted = false;
								Train.CloseDoors(true, true);
							}
						}
						else if (Train.Station >= 0 & Train.StationState == TrainStopState.Boarding)
						{
						}
						else if (Train.Station >= 0 && stopIndex >= 0 && Train.StationDistanceToStopPoint < Program.CurrentRoute.Stations[Train.Station].Stops[stopIndex].BackwardTolerance && (Program.CurrentRoute.Stations[Train.Station].StopsHere(Train) & (Program.CurrentRoute.Stations[Train.Station].OpenLeftDoors | Program.CurrentRoute.Stations[Train.Station].OpenRightDoors) & Math.Abs(Train.CurrentSpeed) < 0.25 & Train.StationState == TrainStopState.Pending))
						{
							// doors not fully open at station - open doors
							if (Train.Specs.DoorOpenMode != DoorMode.Automatic)
							{
								AttemptToOpenDoors();
							}
							CurrentInterval = 1.0;
						}
						else
						{
							// not at station - close doors
							if (Train.Specs.DoorOpenMode != DoorMode.Automatic)
							{
								doorOpenAttempted = false;
								Train.CloseDoors(true, true);
							}
						}
					}
				}
				else if (Train.Station >= 0 && stopIndex >= 0 && Train.StationDistanceToStopPoint < Program.CurrentRoute.Stations[Train.Station].Stops[stopIndex].BackwardTolerance && (Program.CurrentRoute.Stations[Train.Station].StopsHere(Train) & (Program.CurrentRoute.Stations[Train.Station].OpenLeftDoors | Program.CurrentRoute.Stations[Train.Station].OpenRightDoors) & Math.Abs(Train.CurrentSpeed) < 0.25 & Train.StationState == TrainStopState.Pending))
				{
					// arrived at station - open doors
					if (Train.Specs.DoorOpenMode != DoorMode.Automatic)
					{
						AttemptToOpenDoors();
					}
					CurrentInterval = 1.0;
				}
				else if (Train.Station >= 0 && stopIndex >= 0 && Program.CurrentRoute.Stations[Train.Station].Type != StationType.Normal && Program.CurrentRoute.Stations[Train.Station].Type != StationType.RequestStop && Train.IsPlayerTrain && Train.StationDistanceToStopPoint < Program.CurrentRoute.Stations[Train.Station].Stops[stopIndex].BackwardTolerance && -Train.StationDistanceToStopPoint < Program.CurrentRoute.Stations[Train.Station].Stops[stopIndex].ForwardTolerance && Math.Abs(Train.CurrentSpeed) < 0.25)
				{
					// player's terminal station (not boarding any longer)
					if (Train.Plugin != null && Train.Plugin.LastReverser == -2)
					{
						Train.Handles.Reverser.ApplyState(ReverserPosition.Neutral);
					}
					Train.Handles.Brake.ApplyState(1, true);
					Train.Handles.Power.ApplyState(-1, true);
					Train.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
					Train.Handles.EmergencyBrake.Apply();
					CurrentInterval = 10.0;
				}
				else
				{
					// drive
					Train.Handles.Reverser.ApplyState(ReverserPosition.Forwards);

					/*
					 * BVETS motor cars detect wheelslip, others do not at present
					 * FIXME: When new traction modelling is merged, this changes
					 */
					if (Train.Cars[MotorCar].FrontAxle.CurrentWheelSlip | Train.Cars[MotorCar].RearAxle.CurrentWheelSlip)
					{
						// react to wheel slip
						if (Train.Handles.Power.Driver > 1)
						{
							PowerNotchAtWhichWheelSlipIsObserved = Train.Handles.Power.Driver;
							Train.Handles.Brake.ApplyState(-1, true);
							Train.Handles.Power.ApplyState(-1, true);
							Train.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
							CurrentInterval = 2.5;
							return;
						}
					}

					if (Interface.CurrentOptions.Toppling && Math.Abs(Train.Cars[Train.DriverCar].Specs.RollDueToTopplingAngle) > Train.Cars[Train.DriverCar].Specs.CriticalTopplingAngle * 0.3)
					{
						// react to train toppling angle on a curve
						if (Train.Handles.Power.Driver > 0)
						{
							//Dump the power
							Train.Handles.Power.ApplyState(0, false);
						}
						else
						{
							//Still toppling, so brake
							Train.Handles.Brake.ApplyState(1, true);
						}
						CurrentInterval = 2.5;
						return;
					}
					// initialize
					double acc = Train.Specs.CurrentAverageAcceleration;
					double lim = SpeedLimit * 1.2;
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
						lim *= CurrentSpeedFactor;
						if (Train.CurrentSpeed < 8.0)
						{
							powerstart = 0.75 * lim;
							powerend = 0.95 * lim;
						}
						else
						{
							powerstart = lim - 2.5;
							powerend = lim - 1.5;
						}
						if (BrakeMode)
						{
							brakestart = powerend;
						}
						else
						{
							brakestart = lim + 0.5;
						}
					}
					dec = 0.0;
					brakeDeceleration = Train.Cars[Train.DriverCar].CarBrake.DecelerationAtServiceMaximumPressure(Train.Handles.Brake.Actual, Train.Cars[Train.DriverCar].CurrentSpeed);
					for (int i = 0; i < Train.Cars.Length; i++)
					{
						if (Train.Cars[i].TractionModel.ProvidesPower)
						{
							if (Train.Cars[Train.DriverCar].CarBrake.motorDeceleration != 0 && Train.Cars[Train.DriverCar].CarBrake.motorDeceleration < brakeDeceleration)
							{
								brakeDeceleration = Train.Cars[Train.DriverCar].CarBrake.motorDeceleration;
							}
							break;
						}
					}
					if (Train.Handles.Brake is AirBrakeHandle | Train.Handles.Brake.MaximumNotch <= 0)
					{
						decelerationCruise = 0.3 * brakeDeceleration;
						decelerationStart = 0.5 * brakeDeceleration;
						decelerationStep = 0.1 * brakeDeceleration;
					}
					else if (Train.Handles.Brake.MaximumNotch <= 2)
					{
						decelerationCruise = 0.2 * brakeDeceleration;
						decelerationStart = 0.4 * brakeDeceleration;
						decelerationStep = 0.5 * brakeDeceleration;
					}
					else
					{
						decelerationCruise = 0.2 * brakeDeceleration;
						decelerationStart = 0.5 * brakeDeceleration;
						decelerationStep = brakeDeceleration / Train.Handles.Brake.MaximumNotch;
					}
					if (CurrentSpeedFactor >= 1.0)
					{
						decelerationCruise *= 1.25;
						decelerationStart *= 1.25;
						decelerationStep *= 1.25;
					}

					if (Train.CurrentSpeed > 0.0 & Train.CurrentSpeed > brakestart)
					{
						dec = decelerationStep + 0.1 * (Train.CurrentSpeed - brakestart);
					}
					reduceDecelerationCruiseAndStart = false;
					// look ahead
					if (Train.CurrentDirection == TrackDirection.Reverse)
					{
						LookAheadBackwards();
					}
					else
					{
						LookAheadForwards();
					}
					
					Train.Handles.EmergencyBrake.Release();
					// current station
					if (Train.Station >= 0 & Train.StationState == TrainStopState.Pending)
					{
						if (Program.CurrentRoute.Stations[Train.Station].StopsHere(Train))
						{
							int s = Program.CurrentRoute.Stations[Train.Station].GetStopIndex(Train);
							if (s >= 0)
							{
								double dist = Program.CurrentRoute.Stations[Train.Station].Stops[s].TrackPosition - Train.FrontCarTrackPosition;
								if (dist > 0.0)
								{
									if (dist < 25.0)
									{
										reduceDecelerationCruiseAndStart = true;
									}
									else if (CurrentSpeedFactor < 1.0)
									{
										dist -= 5.0;
									}
									var edec = Train.CurrentSpeed * Train.CurrentSpeed / (2.0 * dist);
									if (edec > dec) dec = edec;
								}
								else
								{
									dec = brakeDeceleration;
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
								Train.Handles.Brake.ApplyState(1, true);
								Train.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
							}
							else
							{
								Train.Handles.Power.ApplyState(-1, true);
							}
							CurrentInterval *= 0.4;
							if (CurrentInterval < 0.3) CurrentInterval = 0.3;
						}
						else if (decdiff > decelerationStep)
						{
							// brake stop
							Train.Handles.Brake.ApplyState(-1, true);
							Train.Handles.Power.ApplyState(-1, true);
							Train.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
							CurrentInterval *= 0.4;
							if (CurrentInterval < 0.3) CurrentInterval = 0.3;
						}
						else
						{
							// keep brake
							Train.Handles.Power.ApplyState(-1, true);
							Train.Handles.Brake.ApplyState(AirBrakeHandleState.Lap);
							CurrentInterval *= 1.2;
							if (CurrentInterval > 1.0) CurrentInterval = 1.0;
						}
						if (Train.Handles.Power.Driver == 0 & Train.Handles.Brake.Driver == 0)
						{
							Train.Handles.HoldBrake.ApplyState(Train.Handles.HasHoldBrake);
						}
						if (Train.Handles.Brake is AirBrakeHandle)
						{
							CurrentInterval = 0.1;
						}
					}
					else if (dec > decelerationCruise)
					{
						BrakeMode = false;
						if (Train.Handles.Power.Driver == 0 && Train.CurrentSpeed > powerend && Train.Specs.CurrentAverageAcceleration > 0)
						{
							// We are above the power end threshold, but still accelerating e.g. on a gradient
							// Blip the brakes to slow us down
							Train.Handles.Brake.ApplyState(1, true);
							Train.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
						}
						else if (Train.Specs.CurrentAverageAcceleration < 0)
						{
							if (Train.CurrentSpeed < powerend)
							{
								// No longer accelerating, so cut the brakes for cruise
								Train.Handles.Brake.ApplyState(-1, true);
								Train.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
							}
						}

						Train.Handles.Power.ApplyState(-1, true);
						
						if (Train.Handles.Power.Driver == 0 & Train.Handles.Brake.Driver == 0)
						{
							Train.Handles.HoldBrake.ApplyState(Train.Handles.HasHoldBrake);
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
							double d = lim - Train.CurrentSpeed;
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
						if (Train.CurrentSpeed < powerstart)
						{
							// power start (under-speed)
							if (Train.Handles.Brake.Driver == 0)
							{
								if (Train.Handles.Power.Driver < PowerNotchAtWhichWheelSlipIsObserved - 1)
								{
									Train.Handles.Power.ApplyState(1, true);
								}
							}
							else
							{
								Train.Handles.Brake.ApplyState(-1, true);
							}
							Train.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
							if (double.IsPositiveInfinity(powerstart))
							{
								CurrentInterval = 0.3 + 0.1 * Train.Handles.Power.Driver;
							}
							else
							{
								double p = Train.Handles.Power.Driver / (double)Train.Handles.Power.MaximumNotch;
								CurrentInterval = 0.3 + 15.0 * p / (powerstart - Train.CurrentSpeed + 1.0);
							}
							if (CurrentInterval > 1.3) CurrentInterval = 1.3;
						}
						else if (Train.CurrentSpeed > powerend)
						{
							// power end (over-speed)
							if (Train.Handles.Power.Driver > 0)
							{
								// Only release brakes if we're coming off the power
								// Otherwise some trains on a steep gradient may overspeed
								Train.Handles.Brake.ApplyState(-1, true);
								Train.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
							}
							Train.Handles.Power.ApplyState(-1, true);
							
							CurrentInterval *= 0.3;
							if (CurrentInterval < 0.2) CurrentInterval = 0.2;
						}
						else if (acc < acclim)
						{
							// power start (under-acceleration)
							if (Train.Handles.Brake.Driver == 0)
							{
								if (Train.Handles.Power.Driver < PowerNotchAtWhichWheelSlipIsObserved - 1)
								{
									if (Train.Handles.Power.Driver == Train.Handles.Power.Actual)
									{
										Train.Handles.Power.ApplyState(1, true);
									}
								}
							}
							else
							{
								Train.Handles.Brake.ApplyState(-1, true);
							}
							Train.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
							CurrentInterval = 1.3;
						}
						else
						{
							// keep power
							Train.Handles.Brake.ApplyState(-1, true);
							Train.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
							if (Train.Handles.Power.Driver != 0)
							{
								Train.Specs.CurrentConstSpeed = Train.Specs.HasConstSpeed;
							}
							if (Train.Handles.Power.Driver == 0 & Train.Handles.Brake.Driver == 0)
							{
								Train.Handles.HoldBrake.ApplyState(Train.Handles.HasHoldBrake);
							}
							CurrentInterval *= 1.1;
							if (CurrentInterval > 1.5) CurrentInterval = 1.5;
						}
					}
				}

				if (Train.Cars[Train.DriverCar].Windscreen != null)
				{
					if (wiperTimer < 0)
					{
						if (Train.Cars[Train.DriverCar].Windscreen.currentDrops < Train.Cars[Train.DriverCar].Windscreen.RainDrops.Length / 4)
						{
							if(Train.Cars[Train.DriverCar].Windscreen.Wipers.CurrentSpeed != WiperSpeed.Off)
							{
								Train.Cars[Train.DriverCar].Windscreen.Wipers.ChangeSpeed(Translations.Command.WiperSpeedDown);
							}
						}
						else if (Train.Cars[Train.DriverCar].Windscreen.currentDrops > Train.Cars[Train.DriverCar].Windscreen.RainDrops.Length / 4 && Train.Cars[Train.DriverCar].Windscreen.currentDrops < Train.Cars[Train.DriverCar].Windscreen.RainDrops.Length / 2)
						{
							switch (Train.Cars[Train.DriverCar].Windscreen.Wipers.CurrentSpeed)
							{
								case WiperSpeed.Off:
									Train.Cars[Train.DriverCar].Windscreen.Wipers.ChangeSpeed(Translations.Command.WiperSpeedUp);
									break;
								case WiperSpeed.Fast:
									Train.Cars[Train.DriverCar].Windscreen.Wipers.ChangeSpeed(Translations.Command.WiperSpeedDown);
									break;
							}
							
						}
						else
						{
							if (Train.Cars[Train.DriverCar].Windscreen.Wipers.CurrentSpeed != WiperSpeed.Fast)
							{
								Train.Cars[Train.DriverCar].Windscreen.Wipers.ChangeSpeed(Translations.Command.WiperSpeedUp);
							}
						}
						wiperTimer = 5.0;
					}

				}
			}


			private void LookAheadForwards()
			{
				double lookahead = (Train.Station >= 0 ? 150.0 : 50.0) + (Train.CurrentSpeed * Train.CurrentSpeed) / (2.0 * decelerationCruise);
				double stopDistance = double.MaxValue;
				{
					// next station stop
					int te = Train.Cars[0].FrontAxle.Follower.LastTrackElement;
					int currentTrack = Train.Cars[0].FrontAxle.Follower.TrackIndex;
					for (int i = te; i < Program.CurrentRoute.Tracks[currentTrack].Elements.Length; i++)
					{
						double stp = Program.CurrentRoute.Tracks[currentTrack].Elements[i].StartingTrackPosition;
						if (Train.FrontCarTrackPosition + lookahead <= stp) break;
						for (int j = 0; j < Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events.Count; j++)
						{
							if (!(Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is StationStartEvent stationEvent) || Train.NextStopSkipped != StopSkipMode.None)
							{
								continue;
							}
							if (Program.CurrentRoute.Stations[stationEvent.StationIndex].StopsHere(Train) & Train.LastStation != stationEvent.StationIndex)
							{
								int s = Program.CurrentRoute.Stations[stationEvent.StationIndex].GetStopIndex(Train);
								if (s >= 0)
								{
									double dist = Program.CurrentRoute.Stations[stationEvent.StationIndex].Stops[s].TrackPosition - Train.FrontCarTrackPosition;
									if (dist > 0.0 & dist < stopDistance)
									{
										stopDistance = dist;
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
						if (Train.FrontCarTrackPosition + lookahead <= stp) break;
						for (int j = 0; j < Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events.Count; j++)
						{
							if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is LimitChangeEvent limitEvent)
							{
								// speed limit
								if (limitEvent.NextSpeedLimit < Train.CurrentSpeed)
								{
									double dist = stp + limitEvent.TrackPositionDelta - Train.FrontCarTrackPosition;
									double edec = (Train.CurrentSpeed * Train.CurrentSpeed - limitEvent.NextSpeedLimit * limitEvent.NextSpeedLimit * CurrentSpeedFactor) / (2.0 * dist);
									if (edec > dec) dec = edec;
								}
							}
							else if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is SectionChangeEvent sectionEvent)
							{
								// section
								if (stp + sectionEvent.TrackPositionDelta > Train.FrontCarTrackPosition)
								{
									if (!Program.CurrentRoute.Sections[sectionEvent.NextSectionIndex].Invisible & Program.CurrentRoute.Sections[sectionEvent.NextSectionIndex].CurrentAspect >= 0)
									{
										double elim = Program.CurrentRoute.Sections[sectionEvent.NextSectionIndex].Aspects[Program.CurrentRoute.Sections[sectionEvent.NextSectionIndex].CurrentAspect].Speed * CurrentSpeedFactor;
										if (elim < Train.CurrentSpeed | Train.CurrentSpeed <= 0.0)
										{
											double dist = stp + sectionEvent.TrackPositionDelta - Train.FrontCarTrackPosition;
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
												else if (Train.CurrentSpeed > 9.72222222222222)
												{
													redstopdist = 55.0;
												}
												else
												{
													redstopdist = 35.0;
												}

												if (dist > redstopdist)
												{
													edec = (Train.CurrentSpeed * Train.CurrentSpeed) / (2.0 * (dist - redstopdist));
												}
												else
												{
													edec = brakeDeceleration;
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
													edec = (Train.CurrentSpeed * Train.CurrentSpeed - elim * elim) / (2.0 * dist);
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
							else if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is StationStartEvent e && Train.NextStopSkipped == StopSkipMode.None)
							{
								// station start
								if (Train.Station == -1)
								{
									if (Program.CurrentRoute.Stations[e.StationIndex].StopsHere(Train) & Train.LastStation != e.StationIndex)
									{
										int s = Program.CurrentRoute.Stations[e.StationIndex].GetStopIndex(Train);
										if (s >= 0)
										{
											double dist = Program.CurrentRoute.Stations[e.StationIndex].Stops[s].TrackPosition - Train.FrontCarTrackPosition;
											if (dist > -Program.CurrentRoute.Stations[e.StationIndex].Stops[s].ForwardTolerance)
											{
												if (dist < 25.0)
												{
													reduceDecelerationCruiseAndStart = true;
												}
												else if (CurrentSpeedFactor < 1.0)
												{
													dist -= 5.0;
												}

												var edec = Train.CurrentSpeed * Train.CurrentSpeed / (2.0 * dist);
												if (edec > dec) dec = edec;
											}
										}
									}
								}
							}
							else if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is StationStartEvent stationEvent && Train.NextStopSkipped == StopSkipMode.Decelerate)
							{
								// Brakes the train when passing through a request stop, which is not to be passed at linespeed
								if (Train.Station == -1)
								{
									if (Program.CurrentRoute.Stations[stationEvent.StationIndex].StopsHere(Train) & Train.LastStation != stationEvent.StationIndex)
									{
										int s = Program.CurrentRoute.Stations[stationEvent.StationIndex].GetStopIndex(Train);
										if (s >= 0)
										{
											double dist = Program.CurrentRoute.Stations[stationEvent.StationIndex].Stops[s].TrackPosition - Train.FrontCarTrackPosition;
											if (dist > -Program.CurrentRoute.Stations[stationEvent.StationIndex].Stops[s].ForwardTolerance)
											{
												if (dist < 25.0)
												{
													reduceDecelerationCruiseAndStart = true;
												}
												else if (CurrentSpeedFactor < 1.0)
												{
													dist -= 5.0;
												}

												if (dist > 25)
												{
													var edec = Train.CurrentSpeed * Train.CurrentSpeed / (2.0 * dist);
													if (edec > dec) dec = edec;
												}

											}
										}
									}
								}
							}
							else if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is StationEndEvent stationEndEvent && Train.NextStopSkipped == StopSkipMode.None)
							{
								// station end
								if (Train.Station == -1)
								{
									if (Program.CurrentRoute.Stations[stationEndEvent.StationIndex].StopsHere(Train) & Train.LastStation != stationEndEvent.StationIndex)
									{
										int s = Program.CurrentRoute.Stations[stationEndEvent.StationIndex].GetStopIndex(Train);
										if (s >= 0)
										{
											double dist = Program.CurrentRoute.Stations[stationEndEvent.StationIndex].Stops[s].TrackPosition - Train.FrontCarTrackPosition;
											if (dist > -Program.CurrentRoute.Stations[stationEndEvent.StationIndex].Stops[s].ForwardTolerance)
											{
												if (dist < 25.0)
												{
													reduceDecelerationCruiseAndStart = true;
												}
												else if (CurrentSpeedFactor < 1.0)
												{
													dist -= 5.0;
												}

												var edec = Train.CurrentSpeed * Train.CurrentSpeed / (2.0 * dist);
												if (edec > dec) dec = edec;
											}
										}
									}
								}
							}
							else if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is TrackEndEvent trackEndEvent)
							{
								// track end
								if (Train.IsPlayerTrain)
								{
									double dist = stp + trackEndEvent.TrackPositionDelta - Train.FrontCarTrackPosition;
									double edec;
									if (dist >= 15.0)
									{
										edec = Train.CurrentSpeed * Train.CurrentSpeed / (2.0 * dist);
									}
									else
									{
										edec = brakeDeceleration;
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
					for (int i = 0; i < Program.CurrentRoute.BufferTrackPositions.Count; i++)
					{
						if (Program.CurrentRoute.BufferTrackPositions[i].TrackIndex != Train.Cars[0].FrontAxle.Follower.TrackIndex)
						{
							// different rail
							continue;
						}
						double dist = Program.CurrentRoute.BufferTrackPositions[i].TrackPosition - Train.FrontCarTrackPosition;
						if (dist > 0.0)
						{
							double edec;
							if (dist >= 10.0)
							{
								edec = Train.CurrentSpeed * Train.CurrentSpeed / (2.0 * dist);
							}
							else if (dist >= 5.0)
							{
								Train.Handles.Brake.ApplyState(1, true);
								Train.Handles.Power.ApplyState(-1, true);
								Train.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
								CurrentInterval = 0.1;
								return;
							}
							else
							{
								Train.Handles.Brake.ApplyState(1, true);
								Train.Handles.Power.ApplyState(-1, true);
								Train.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
								Train.Handles.EmergencyBrake.Apply();
								CurrentInterval = 10.0;
								return;
							}

							if (edec > dec) dec = edec;
						}
					}
				}

				// trains ahead
				for (int i = 0; i < Program.TrainManager.Trains.Count; i++)
				{
					if (Program.TrainManager.Trains[i] != Train && Program.TrainManager.Trains[i].State == TrainState.Available)
					{
						double pos =
							Program.TrainManager.Trains[i].Cars[Program.TrainManager.Trains[i].Cars.Length - 1].RearAxle.Follower.TrackPosition -
							Program.TrainManager.Trains[i].Cars[Program.TrainManager.Trains[i].Cars.Length - 1].RearAxle.Position -
							0.5 * Program.TrainManager.Trains[i].Cars[Program.TrainManager.Trains[i].Cars.Length - 1].Length;
						double dist = pos - Train.FrontCarTrackPosition;
						if (dist > -10.0 & dist < lookahead)
						{
							const double minDistance = 10.0;
							const double maxDistance = 100.0;
							double edec;
							if (dist > minDistance)
							{
								double shift = 0.75 * minDistance + 1.0 * Train.CurrentSpeed;
								edec = Train.CurrentSpeed * Train.CurrentSpeed / (2.0 * (dist - shift));
							}
							else if (dist > 0.5 * minDistance)
							{
								Train.Handles.Brake.ApplyState(1, true);
								Train.Handles.Power.ApplyState(-1, true);
								Train.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
								CurrentInterval = 0.1;
								return;
							}
							else
							{
								Train.Handles.Brake.ApplyState(1, true);
								Train.Handles.Power.ApplyState(-1, true);
								Train.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
								Train.Handles.EmergencyBrake.Apply();
								CurrentInterval = 1.0;
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
			}

			private void LookAheadBackwards()
			{
				double lookahead = (Train.Station >= 0 ? 150.0 : 50.0) + (Train.CurrentSpeed * Train.CurrentSpeed) / (2.0 * decelerationCruise);
				double stopDistance = double.MaxValue;
				{
					// next station stop
					int te = Train.Cars[Train.Cars.Length - 1].FrontAxle.Follower.LastTrackElement;
					int currentTrack = Train.Cars[Train.Cars.Length - 1].FrontAxle.Follower.TrackIndex;
					for (int i = te; i > 0; i--)
					{
						double stp = Program.CurrentRoute.Tracks[currentTrack].Elements[i].StartingTrackPosition;
						if (Train.FrontCarTrackPosition + lookahead <= stp) break;
						for (int j = 0; j < Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events.Count; j++)
						{
							if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is StationStartEvent e && Train.NextStopSkipped == StopSkipMode.None)
							{
								if (Program.CurrentRoute.Stations[e.StationIndex].StopsHere(Train) & Train.LastStation != e.StationIndex)
								{
									int s = Program.CurrentRoute.Stations[e.StationIndex].GetStopIndex(Train);
									if (s >= 0)
									{
										double dist = Train.FrontCarTrackPosition - Program.CurrentRoute.Stations[e.StationIndex].Stops[s].TrackPosition;
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
					int te = Train.Cars[Train.Cars.Length - 1].FrontAxle.Follower.LastTrackElement;
					int currentTrack = Train.Cars[Train.Cars.Length - 1].FrontAxle.Follower.TrackIndex;
					for (int i = te; i > 0; i--)
					{
						double stp = Program.CurrentRoute.Tracks[currentTrack].Elements[i].StartingTrackPosition;
						if (Train.FrontCarTrackPosition + lookahead <= stp) break;
						for (int j = 0; j < Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events.Count; j++)
						{
							if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is LimitChangeEvent limitEvent)
							{
								// speed limit
								if (limitEvent.NextSpeedLimit < Train.CurrentSpeed)
								{
									double dist = stp + Train.FrontCarTrackPosition - limitEvent.TrackPositionDelta;
									double edec = (Train.CurrentSpeed * Train.CurrentSpeed - limitEvent.NextSpeedLimit * limitEvent.NextSpeedLimit * CurrentSpeedFactor) / (2.0 * dist);
									if (edec > dec) dec = edec;
								}
							}
							else if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is SectionChangeEvent sectionEvent)
							{
								// section
								if (stp + Train.FrontCarTrackPosition > sectionEvent.TrackPositionDelta)
								{
									if (!Program.CurrentRoute.Sections[sectionEvent.NextSectionIndex].Invisible & Program.CurrentRoute.Sections[sectionEvent.NextSectionIndex].CurrentAspect >= 0)
									{
										double elim = Program.CurrentRoute.Sections[sectionEvent.NextSectionIndex].Aspects[Program.CurrentRoute.Sections[sectionEvent.NextSectionIndex].CurrentAspect].Speed * CurrentSpeedFactor;
										if (elim < Train.CurrentSpeed | Train.CurrentSpeed <= 0.0)
										{
											double dist = stp + Train.FrontCarTrackPosition - sectionEvent.TrackPositionDelta;
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
												else if (Train.CurrentSpeed > 9.72222222222222)
												{
													redstopdist = 55.0;
												}
												else
												{
													redstopdist = 35.0;
												}

												if (dist > redstopdist)
												{
													edec = (Train.CurrentSpeed * Train.CurrentSpeed) / (2.0 * (dist - redstopdist));
												}
												else
												{
													edec = brakeDeceleration;
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
													edec = (Train.CurrentSpeed * Train.CurrentSpeed - elim * elim) / (2.0 * dist);
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
							else if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is StationStartEvent stationStartEvent && Train.NextStopSkipped == StopSkipMode.None)
							{
								// station start
								if (Train.Station == -1)
								{
									if (Program.CurrentRoute.Stations[stationStartEvent.StationIndex].StopsHere(Train) & Train.LastStation != stationStartEvent.StationIndex)
									{
										int s = Program.CurrentRoute.Stations[stationStartEvent.StationIndex].GetStopIndex(Train);
										if (s >= 0)
										{
											double dist = Train.FrontCarTrackPosition - Program.CurrentRoute.Stations[stationStartEvent.StationIndex].Stops[s].TrackPosition;
											if (dist > -Program.CurrentRoute.Stations[stationStartEvent.StationIndex].Stops[s].ForwardTolerance)
											{
												if (dist < 25.0)
												{
													reduceDecelerationCruiseAndStart = true;
												}
												else if (CurrentSpeedFactor < 1.0)
												{
													dist -= 5.0;
												}

												var edec = Train.CurrentSpeed * Train.CurrentSpeed / (2.0 * dist);
												if (edec > dec) dec = edec;
											}
										}
									}
								}
							}
							else if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is StationStartEvent e && Train.NextStopSkipped == StopSkipMode.Decelerate)
							{
								// Brakes the train when passing through a request stop, which is not to be passed at linespeed
								if (Train.Station == -1)
								{
									if (Program.CurrentRoute.Stations[e.StationIndex].StopsHere(Train) & Train.LastStation != e.StationIndex)
									{
										int s = Program.CurrentRoute.Stations[e.StationIndex].GetStopIndex(Train);
										if (s >= 0)
										{
											double dist = Train.FrontCarTrackPosition - Program.CurrentRoute.Stations[e.StationIndex].Stops[s].TrackPosition;
											if (dist > -Program.CurrentRoute.Stations[e.StationIndex].Stops[s].ForwardTolerance)
											{
												if (dist < 25.0)
												{
													reduceDecelerationCruiseAndStart = true;
												}
												else if (CurrentSpeedFactor < 1.0)
												{
													dist -= 5.0;
												}

												if (dist > 25)
												{
													var edec = Train.CurrentSpeed * Train.CurrentSpeed / (2.0 * dist);
													if (edec > dec) dec = edec;
												}

											}
										}
									}
								}
							}
							else if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is StationEndEvent stationEndEvent && Train.NextStopSkipped == StopSkipMode.None)
							{
								// station end
								if (Train.Station == -1)
								{
									if (Program.CurrentRoute.Stations[stationEndEvent.StationIndex].StopsHere(Train) & Train.LastStation != stationEndEvent.StationIndex)
									{
										int s = Program.CurrentRoute.Stations[stationEndEvent.StationIndex].GetStopIndex(Train);
										if (s >= 0)
										{
											double dist = Train.FrontCarTrackPosition - Program.CurrentRoute.Stations[stationEndEvent.StationIndex].Stops[s].TrackPosition;
											if (dist > -Program.CurrentRoute.Stations[stationEndEvent.StationIndex].Stops[s].ForwardTolerance)
											{
												if (dist < 25.0)
												{
													reduceDecelerationCruiseAndStart = true;
												}
												else if (CurrentSpeedFactor < 1.0)
												{
													dist -= 5.0;
												}

												var edec = Train.CurrentSpeed * Train.CurrentSpeed / (2.0 * dist);
												if (edec > dec) dec = edec;
											}
										}
									}
								}
							}
							else if (Program.CurrentRoute.Tracks[currentTrack].Elements[i].Events[j] is TrackEndEvent trackEndEvent && Train.IsPlayerTrain)
							{
								// track end (AI trains will simply carry on and disappear)
								double dist = stp + Train.FrontCarTrackPosition - trackEndEvent.TrackPositionDelta;
								double edec;
								if (dist >= 15.0)
								{
									edec = Train.CurrentSpeed * Train.CurrentSpeed / (2.0 * dist);
								}
								else
								{
									edec = brakeDeceleration;
								}

								if (edec > dec) dec = edec;
							}
						}
					}
				}
				// buffers ahead
				if (Train.IsPlayerTrain)
				{
					for (int i = 0; i < Program.CurrentRoute.BufferTrackPositions.Count; i++)
					{
						if (Program.CurrentRoute.BufferTrackPositions[i].TrackIndex != Train.Cars[0].FrontAxle.Follower.TrackIndex)
						{
							// different rail
							continue;
						}
						double dist = Train.RearCarTrackPosition - Program.CurrentRoute.BufferTrackPositions[i].TrackPosition;
						if (dist > 0.0)
						{
							double edec;
							if (dist >= 10.0)
							{
								edec = Train.CurrentSpeed * Train.CurrentSpeed / (2.0 * dist);
							}
							else if (dist >= 5.0)
							{
								Train.Handles.Brake.ApplyState(1, true);
								Train.Handles.Power.ApplyState(-1, true);
								Train.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
								CurrentInterval = 0.1;
								return;
							}
							else
							{
								Train.Handles.Brake.ApplyState(1, true);
								Train.Handles.Power.ApplyState(-1, true);
								Train.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
								Train.Handles.EmergencyBrake.Apply();
								CurrentInterval = 10.0;
								return;
							}

							if (edec > dec) dec = edec;
						}
					}
				}

				// trains ahead
				for (int i = 0; i < Program.TrainManager.Trains.Count; i++)
				{
					if (Program.TrainManager.Trains[i] != Train && Program.TrainManager.Trains[i].State == TrainState.Available)
					{
						double pos =
							Program.TrainManager.Trains[i].Cars[Program.TrainManager.Trains[i].Cars.Length - 1].RearAxle.Follower.TrackPosition -
							Program.TrainManager.Trains[i].Cars[Program.TrainManager.Trains[i].Cars.Length - 1].RearAxle.Position -
							0.5 * Program.TrainManager.Trains[i].Cars[Program.TrainManager.Trains[i].Cars.Length - 1].Length;
						double dist = Train.FrontCarTrackPosition - pos;
						if (dist > -10.0 & dist < lookahead)
						{
							const double minDistance = 10.0;
							const double maxDistance = 100.0;
							double edec;
							if (dist > minDistance)
							{
								double shift = 0.75 * minDistance + 1.0 * Train.CurrentSpeed;
								edec = Train.CurrentSpeed * Train.CurrentSpeed / (2.0 * (dist - shift));
							}
							else if (dist > 0.5 * minDistance)
							{
								Train.Handles.Brake.ApplyState(1, true);
								Train.Handles.Power.ApplyState(-1, true);
								Train.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
								CurrentInterval = 0.1;
								return;
							}
							else
							{
								Train.Handles.Brake.ApplyState(1, true);
								Train.Handles.Power.ApplyState(-1, true);
								Train.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
								Train.Handles.EmergencyBrake.Apply();
								CurrentInterval = 1.0;
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
			}

			/// <summary>The timer unti the doors may be opened</summary>
			private double doorWaitingTimer = 2.0;
			/// <summary>Controls the AI operating the wipers</summary>
			private double wiperTimer;
			/// <summary>Whether a door open has yet been attempted</summary>
			private bool doorOpenAttempted = false;


			/// <summary>Provides a simple random delay timer, so the driver does not appear to open the doors instantly</summary>
			private void AttemptToOpenDoors()
			{
				if (doorOpenAttempted == false)
				{
					doorWaitingTimer = Program.RandomNumberGenerator.Next(0, 5);
					doorOpenAttempted = true;
				}
				if (doorWaitingTimer < 0)
				{
					Train.OpenDoors(Program.CurrentRoute.Stations[Train.Station].OpenLeftDoors, Program.CurrentRoute.Stations[Train.Station].OpenRightDoors);
				}
				
			}
			public override void Trigger(double timeElapsed)
			{
				if (TimeLastProcessed > Program.CurrentRoute.SecondsSinceMidnight)
				{
					TimeLastProcessed = Program.CurrentRoute.SecondsSinceMidnight;
				}
				else if (Program.CurrentRoute.SecondsSinceMidnight - TimeLastProcessed >= CurrentInterval)
				{
					doorWaitingTimer -= Program.CurrentRoute.SecondsSinceMidnight - TimeLastProcessed;
					wiperTimer -= Program.CurrentRoute.SecondsSinceMidnight - TimeLastProcessed;
					TimeLastProcessed = Program.CurrentRoute.SecondsSinceMidnight;
					if (Train.Plugin != null && Train.Plugin.SupportsAI != AISupport.None)
					{
						if (PerformPlugin(timeElapsed) != AIResponse.None)
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
