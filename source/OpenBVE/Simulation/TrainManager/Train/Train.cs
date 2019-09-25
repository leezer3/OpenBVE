using System;
using System.Globalization;
using System.Linq;
using OpenBve.BrakeSystems;
using OpenBveApi.Colors;
using OpenBveApi.Runtime;
using OpenBveApi.Interface;
using OpenBveApi.Trains;
using OpenBveApi;
using OpenBveApi.Math;
using RouteManager2.MessageManager;
using SoundManager;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>The root class for a train within the simulation</summary>
		public partial class Train : AbstractTrain
		{
			/// <summary>Holds the safety systems for the train</summary>
			internal TrainSafetySystems SafetySystems;
			/// <summary>The plugin used by this train.</summary>
			internal PluginManager.Plugin Plugin;
			/// <summary>The driver body</summary>
			internal DriverBody DriverBody;
			internal Handles Handles;
			internal Car[] Cars;
			internal TrainSpecs Specs;
			internal TrainPassengers Passengers;
			
			internal double StationArrivalTime;
			internal double StationDepartureTime;
			internal bool StationDepartureSoundPlayed;
			internal double StationDistanceToStopPoint;
			
			
			
			internal Game.GeneralAI AI;
			private double InternalTimerTimeElapsed;
			internal bool Derailed;
			
			internal string[] PowerNotchDescriptions;
			internal string[] LocoBrakeNotchDescriptions;
			internal string[] BrakeNotchDescriptions;
			internal string[] ReverserDescriptions;
			/// <summary>The max width used in px for the power notch HUD string</summary>
			internal int MaxPowerNotchWidth = 48;
			/// <summary>The max width used in px for the brake notch HUD string</summary>
			internal int MaxBrakeNotchWidth = 48;
			/// <summary>The max width used in px for the loco brake notch HUD string</summary>
			internal int MaxLocoBrakeNotchWidth = 48;
			/// <summary>The max width used in px for the reverser HUD string</summary>
			internal int MaxReverserWidth = 48;

			private double previousRouteLimit = 0.0;

			internal Train(TrainState state)
			{
				State = state;
				Destination = Game.InitialDestination;
				Station = -1;
				RouteLimits = new double[] { double.PositiveInfinity };
				CurrentRouteLimit = double.PositiveInfinity;
				CurrentSectionLimit = double.PositiveInfinity;
				Cars = new TrainManager.Car[] { };
				
				Specs.DoorOpenMode = DoorMode.AutomaticManualOverride;
				Specs.DoorCloseMode = DoorMode.AutomaticManualOverride;
				Specs.DoorWidth = 1000.0;
				Specs.DoorMaxTolerance = 0.0;
			}

			internal void Initialize()
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Initialize();
				}
				UpdateAtmosphericConstants();
				Update(0.0);
			}

			/// <summary>Disposes of the train</summary>
			public override void Dispose()
			{
				State = TrainState.Disposed;
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].ChangeCarSection(CarSectionType.NotVisible);
					Cars[i].FrontBogie.ChangeSection(-1);
					Cars[i].RearBogie.ChangeSection(-1);
					Cars[i].Coupler.ChangeSection(-1);
				}
				Program.Sounds.StopAllSounds(this);

				for (int i = 0; i < Program.CurrentRoute.Sections.Length; i++)
				{
					Program.CurrentRoute.Sections[i].Leave(this);
				}
				if (Program.CurrentRoute.Sections.Length != 0)
				{
					Game.UpdateAllSections();
				}
			}

			/// <inheritdoc/>
			public override bool IsPlayerTrain
			{
				get
				{
					return this == PlayerTrain;
				}
			}

			/// <inheritdoc/>
			public override int NumberOfCars
			{
				get
				{
					return this.Cars.Length;
				}
			}

			/// <inheritdoc/>
			public override void SectionChange()
			{
				if (CurrentSectionLimit == 0.0 && Game.MinimalisticSimulation == false)
				{
					Game.AddMessage(Translations.GetInterfaceString("message_signal_stop"), MessageDependency.PassedRedSignal, GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
				}
				else if (CurrentSpeed > CurrentSectionLimit)
				{
					Game.AddMessage(Translations.GetInterfaceString("message_signal_overspeed"), MessageDependency.SectionLimit, GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
				}
			}

			/// <inheritdoc/>
			public override void UpdateBeacon(int transponderType, int sectionIndex, int optional)
			{
				if (Plugin != null)
				{
					Plugin.UpdateBeacon(transponderType, sectionIndex, optional);
				}
			}

			/// <summary>Call this method to update the train</summary>
			/// <param name="TimeElapsed">The elapsed time this frame</param>
			internal void Update(double TimeElapsed)
			{
				if (State == TrainState.Pending)
				{
					// pending train
					bool forceIntroduction = !IsPlayerTrain && !Game.MinimalisticSimulation;
					double time = 0.0;
					if (!forceIntroduction)
					{
						for (int i = 0; i < Program.CurrentRoute.Stations.Length; i++)
						{
							if (Program.CurrentRoute.Stations[i].StopMode == StationStopMode.AllStop | Program.CurrentRoute.Stations[i].StopMode == StationStopMode.PlayerPass)
							{
								if (Program.CurrentRoute.Stations[i].ArrivalTime >= 0.0)
								{
									time = Program.CurrentRoute.Stations[i].ArrivalTime;
								}
								else if (Program.CurrentRoute.Stations[i].DepartureTime >= 0.0)
								{
									time = Program.CurrentRoute.Stations[i].DepartureTime - Program.CurrentRoute.Stations[i].StopTime;
								}
								break;
							}
						}
						time -= TimetableDelta;
					}
					if (Program.CurrentRoute.SecondsSinceMidnight >= time | forceIntroduction)
					{
						bool introduce = true;
						if (!forceIntroduction)
						{
							if (CurrentSectionIndex >= 0)
							{
								if (!Program.CurrentRoute.Sections[CurrentSectionIndex].IsFree())
								{
									introduce = false;
								}
							}
						}

						if (this == PlayerTrain && Loading.SimulationSetup)
						{
							/* Loading has finished, but we still have an AI train in the current section
							 * This may be caused by an iffy RunInterval value, or simply by having no sections							 *
							 *
							 * We must introduce the player's train as otherwise the cab and loop sounds are missing
							 * NOTE: In this case, the signalling cannot prevent the player from colliding with
							 * the AI train
							 */

							introduce = true;
						}
						if (introduce)
						{
							// train is introduced
							State = TrainState.Available;
							for (int j = 0; j < Cars.Length; j++)
							{
								if (Cars[j].CarSections.Length != 0)
								{
									if (j == this.DriverCar && IsPlayerTrain)
									{
										this.Cars[j].ChangeCarSection(CarSectionType.Interior);
									}
									else
									{
										/*
										 * HACK: Load in exterior mode first to ensure everything is cached
										 * before switching immediately to not visible
										 * https://github.com/leezer3/OpenBVE/issues/226
										 * Stuff like the R142A really needs to downsize the textures supplied,
										 * but we have no control over external factors....
										 */
										this.Cars[j].ChangeCarSection(CarSectionType.Exterior);
										if (IsPlayerTrain)
										{
											this.Cars[j].ChangeCarSection(CarSectionType.NotVisible);

										}
									}

								}
								Cars[j].FrontBogie.ChangeSection(!IsPlayerTrain ? 0 : -1);
								Cars[j].RearBogie.ChangeSection(!IsPlayerTrain ? 0 : -1);
								Cars[j].Coupler.ChangeSection(!IsPlayerTrain ? 0 : -1);
								
								if (Cars[j].Specs.IsMotorCar)
								{
									if (Cars[j].Sounds.Loop.Buffer != null)
									{
										Cars[j].Sounds.Loop.Source = Program.Sounds.PlaySound(Cars[j].Sounds.Loop.Buffer, 1.0, 1.0, Cars[j].Sounds.Loop.Position, Cars[j], true);
									}
								}
							}
						}
					}
				}
				else if (State == TrainState.Available)
				{
					// available train
					UpdatePhysicsAndControls(TimeElapsed);
					if (CurrentSpeed > CurrentRouteLimit)
					{
						if (previousRouteLimit != CurrentRouteLimit || Interface.CurrentOptions.GameMode == GameMode.Arcade)
						{
							/*
							 * HACK: If the limit has changed, or we are in arcade mode, notify the player
							 *       This conforms to the original behaviour, but doesn't need to raise the message from the event.
							 */
							 Game.AddMessage(Translations.GetInterfaceString("message_route_overspeed"), MessageDependency.RouteLimit, GameMode.Normal, MessageColor.Orange, Double.PositiveInfinity, null);
						}
						
					}
					previousRouteLimit = CurrentRouteLimit;
					if (Interface.CurrentOptions.GameMode == GameMode.Arcade)
					{
						if (CurrentSectionLimit == 0.0)
						{
							Game.AddMessage(Translations.GetInterfaceString("message_signal_stop"), MessageDependency.PassedRedSignal, GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
						}
						else if (CurrentSpeed > CurrentSectionLimit)
						{
							Game.AddMessage(Translations.GetInterfaceString("message_signal_overspeed"), MessageDependency.SectionLimit, GameMode.Normal, MessageColor.Orange, Double.PositiveInfinity, null);
						}
					}
					if (AI != null)
					{
						AI.Trigger(TimeElapsed);
					}
				}
				else if (State == TrainState.Bogus)
				{
					// bogus train
					if (AI != null)
					{
						AI.Trigger(TimeElapsed);
					}
				}
				//Trigger point sounds if appropriate
				for (int i = 0; i < Cars.Length; i++)
				{
					Vector3 p = Vector3.Zero;
					SoundBuffer buffer = null;
					if (Cars[i].FrontAxle.PointSoundTriggered)
					{
						Cars[i].FrontAxle.PointSoundTriggered = false;
						int bufferIndex = Cars[i].FrontAxle.RunIndex;
						if (Cars[i].FrontAxle.PointSounds == null || Cars[i].FrontAxle.PointSounds.Length == 0)
						{
							//No point sounds defined at all
							continue;
						}
						if (bufferIndex > Cars[i].FrontAxle.PointSounds.Length - 1
						    || Cars[i].FrontAxle.PointSounds[bufferIndex].Buffer == null)
						{
							//If the switch sound does not exist, return zero
							//Required to handle legacy trains which don't have idx specific run sounds defined
							bufferIndex = 0;
						}
						buffer = Cars[i].FrontAxle.PointSounds[bufferIndex].Buffer;
						p = Cars[i].FrontAxle.PointSounds[bufferIndex].Position;
					}
					if (buffer != null)
					{
						double spd = Math.Abs(CurrentSpeed);
						double pitch = spd / 12.5;
						double gain = pitch < 0.5 ? 2.0 * pitch : 1.0;
						if (pitch < 0.2 | gain < 0.2)
						{
							buffer = null;
						}
						if (buffer != null)
						{
							Program.Sounds.PlaySound(buffer, pitch, gain, p, Cars[i], false);
						}
					}
				}
			}

			

			/// <summary>Updates the physics and controls for this train</summary>
			/// <param name="TimeElapsed">The time elapsed</param>
			private void UpdatePhysicsAndControls(double TimeElapsed)
			{
				if (TimeElapsed == 0.0 || TimeElapsed > 1000)
				{
					//HACK: The physics engine really does not like update times above 1000ms
					//This works around a bug experienced when jumping to a station on a steep hill
					//causing exessive acceleration
					return;
				}
				// move cars
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Move(Cars[i].CurrentSpeed * TimeElapsed);
					if (State == TrainState.Disposed)
					{
						return;
					}
				}
				// update station and doors
				UpdateTrainStation(this, TimeElapsed);
				UpdateTrainDoors(this, TimeElapsed);
				// delayed handles
				if (Plugin == null)
				{
					Handles.Power.Safety = Handles.Power.Driver;
					Handles.Brake.Safety = Handles.Brake.Driver;
					Handles.EmergencyBrake.Safety = Handles.EmergencyBrake.Driver;
				}
				Handles.Power.Update();
				Handles.Brake.Update();
				Handles.Brake.Update();
				Handles.EmergencyBrake.Update();
				Handles.HoldBrake.Actual = Handles.HoldBrake.Driver;
				// update speeds
				UpdateSpeeds(TimeElapsed);
				// Update Run and Motor sounds
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].UpdateRunSounds(TimeElapsed);
					Cars[i].UpdateMotorSounds(TimeElapsed);
				}

				// safety system
				if (!Game.MinimalisticSimulation | !IsPlayerTrain)
				{
					UpdateSafetySystem();
				}
				{
					// breaker sound
					bool breaker;
					if (Cars[DriverCar].CarBrake is AutomaticAirBrake)
					{
						breaker = Handles.Reverser.Actual != 0 & Handles.Power.Safety >= 1 & Handles.Brake.Safety == (int)AirBrakeHandleState.Release & !Handles.EmergencyBrake.Safety & !Handles.HoldBrake.Actual;
					}
					else
					{
						breaker = Handles.Reverser.Actual != 0 & Handles.Power.Safety >= 1 & Handles.Brake.Safety == 0 & !Handles.EmergencyBrake.Safety & !Handles.HoldBrake.Actual;
					}
					if (breaker & !Cars[DriverCar].Sounds.BreakerResumed)
					{
						// resume
						if (Cars[DriverCar].Sounds.BreakerResume.Buffer != null)
						{
							Program.Sounds.PlaySound(Cars[DriverCar].Sounds.BreakerResume.Buffer, 1.0, 1.0, Cars[DriverCar].Sounds.BreakerResume.Position, Cars[DriverCar], false);
						}
						if (Cars[DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer != null)
						{
							Program.Sounds.PlaySound(Cars[DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer, 1.0, 1.0, Cars[DriverCar].Sounds.BreakerResumeOrInterrupt.Position, Cars[DriverCar], false);
						}
						Cars[DriverCar].Sounds.BreakerResumed = true;
					}
					else if (!breaker & Cars[DriverCar].Sounds.BreakerResumed)
					{
						// interrupt
						if (Cars[DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer != null)
						{
							Program.Sounds.PlaySound(Cars[DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer, 1.0, 1.0, Cars[DriverCar].Sounds.BreakerResumeOrInterrupt.Position, Cars[DriverCar], false);
						}
						Cars[DriverCar].Sounds.BreakerResumed = false;
					}
				}
				// passengers
				Passengers.Update(Specs.CurrentAverageAcceleration, TimeElapsed);
				// signals
				if (CurrentSectionLimit == 0.0)
				{
					if (Handles.EmergencyBrake.Driver & CurrentSpeed > -0.03 & CurrentSpeed < 0.03)
					{
						CurrentSectionLimit = 6.94444444444444;
						if (IsPlayerTrain)
						{
							string s = Translations.GetInterfaceString("message_signal_proceed");
							double a = (3.6 * CurrentSectionLimit) * Game.SpeedConversionFactor;
							s = s.Replace("[speed]", a.ToString("0", CultureInfo.InvariantCulture));
							s = s.Replace("[unit]", Game.UnitOfSpeed);
							Game.AddMessage(s, MessageDependency.None, GameMode.Normal, MessageColor.Red, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
						}
					}
				}
				// infrequent updates
				InternalTimerTimeElapsed += TimeElapsed;
				if (InternalTimerTimeElapsed > 10.0)
				{
					InternalTimerTimeElapsed -= 10.0;
					Synchronize();
					UpdateAtmosphericConstants();
				}
			}

			private void UpdateSpeeds(double TimeElapsed)
			{
				if (Game.MinimalisticSimulation & IsPlayerTrain)
				{
					// hold the position of the player's train during startup
					for (int i = 0; i < Cars.Length; i++)
					{
						Cars[i].CurrentSpeed = 0.0;
						Cars[i].Specs.CurrentAccelerationOutput = 0.0;
					}
					return;
				}
				// update brake system
				double[] DecelerationDueToBrake, DecelerationDueToMotor;
				UpdateBrakeSystem(TimeElapsed, out DecelerationDueToBrake, out DecelerationDueToMotor);
				// calculate new car speeds
				double[] NewSpeeds = new double[Cars.Length];
				for (int i = 0; i < Cars.Length; i++)
				{
					double PowerRollingCouplerAcceleration;
					// rolling on an incline
					{
						double a = Cars[i].FrontAxle.Follower.WorldDirection.Y;
						double b = Cars[i].RearAxle.Follower.WorldDirection.Y;
						PowerRollingCouplerAcceleration = -0.5 * (a + b) * Program.CurrentRoute.Atmosphere.AccelerationDueToGravity;
					}
					// friction
					double FrictionBrakeAcceleration;
					{
						double v = Math.Abs(Cars[i].CurrentSpeed);
						double a = GetResistance(this, i, ref Cars[i].FrontAxle, v);
						double b = GetResistance(this, i, ref Cars[i].RearAxle, v);
						FrictionBrakeAcceleration = 0.5 * (a + b);
					}
					// power
					double wheelspin = 0.0;
					double wheelSlipAccelerationMotorFront;
					double wheelSlipAccelerationMotorRear;
					double wheelSlipAccelerationBrakeFront;
					double wheelSlipAccelerationBrakeRear;
					if (Cars[i].Derailed)
					{
						wheelSlipAccelerationMotorFront = 0.0;
						wheelSlipAccelerationBrakeFront = 0.0;
						wheelSlipAccelerationMotorRear = 0.0;
						wheelSlipAccelerationBrakeRear = 0.0;
					}
					else
					{
						wheelSlipAccelerationMotorFront = GetCriticalWheelSlipAccelerationForElectricMotor(this, i, Cars[i].FrontAxle.Follower.AdhesionMultiplier, Cars[i].FrontAxle.Follower.WorldUp.Y, Cars[i].CurrentSpeed);
						wheelSlipAccelerationMotorRear = GetCriticalWheelSlipAccelerationForElectricMotor(this, i, Cars[i].RearAxle.Follower.AdhesionMultiplier, Cars[i].RearAxle.Follower.WorldUp.Y, Cars[i].CurrentSpeed);
						wheelSlipAccelerationBrakeFront = GetCriticalWheelSlipAccelerationForFrictionBrake(this, i, Cars[i].FrontAxle.Follower.AdhesionMultiplier, Cars[i].FrontAxle.Follower.WorldUp.Y, Cars[i].CurrentSpeed);
						wheelSlipAccelerationBrakeRear = GetCriticalWheelSlipAccelerationForFrictionBrake(this, i, Cars[i].RearAxle.Follower.AdhesionMultiplier, Cars[i].RearAxle.Follower.WorldUp.Y, Cars[i].CurrentSpeed);
					}
					if (DecelerationDueToMotor[i] == 0.0)
					{
						double a;
						if (Cars[i].Specs.IsMotorCar)
						{
							if (DecelerationDueToMotor[i] == 0.0)
							{
								if (Handles.Reverser.Actual != 0 & Handles.Power.Actual > 0 & !Handles.HoldBrake.Actual & !Handles.EmergencyBrake.Actual)
								{
									// target acceleration
									if (Handles.Power.Actual - 1 < Cars[i].Specs.AccelerationCurves.Length)
									{
										// Load factor is a constant 1.0 for anything prior to BVE5
										// This will need to be changed when the relevant branch is merged in
										a = Cars[i].Specs.AccelerationCurves[Handles.Power.Actual - 1].GetAccelerationOutput((double)Handles.Reverser.Actual * Cars[i].CurrentSpeed, 1.0);
									}
									else
									{
										a = 0.0;
									}
									// readhesion device
									if (a > Cars[i].Specs.ReAdhesionDevice.MaximumAccelerationOutput)
									{
										a = Cars[i].Specs.ReAdhesionDevice.MaximumAccelerationOutput;
									}
									// wheel slip
									if (a < wheelSlipAccelerationMotorFront)
									{
										Cars[i].FrontAxle.CurrentWheelSlip = false;
									}
									else
									{
										Cars[i].FrontAxle.CurrentWheelSlip = true;
										wheelspin += (double)Handles.Reverser.Actual * a * Cars[i].Specs.MassCurrent;
									}
									if (a < wheelSlipAccelerationMotorRear)
									{
										Cars[i].RearAxle.CurrentWheelSlip = false;
									}
									else
									{
										Cars[i].RearAxle.CurrentWheelSlip = true;
										wheelspin += (double)Handles.Reverser.Actual * a * Cars[i].Specs.MassCurrent;
									}
									// Update readhesion device
									this.Cars[i].Specs.ReAdhesionDevice.Update(a);
									// Update constant speed device

									this.Cars[i].Specs.ConstSpeed.Update(ref a, this.Specs.CurrentConstSpeed, this.Handles.Reverser.Actual);
									
									// finalize
									if (wheelspin != 0.0) a = 0.0;
								}
								else
								{
									a = 0.0;
									Cars[i].FrontAxle.CurrentWheelSlip = false;
									Cars[i].RearAxle.CurrentWheelSlip = false;
								}
							}
							else
							{
								a = 0.0;
								Cars[i].FrontAxle.CurrentWheelSlip = false;
								Cars[i].RearAxle.CurrentWheelSlip = false;
							}
						}
						else
						{
							a = 0.0;
							Cars[i].FrontAxle.CurrentWheelSlip = false;
							Cars[i].RearAxle.CurrentWheelSlip = false;
						}
						if (!Cars[i].Derailed)
						{
							if (Cars[i].Specs.CurrentAccelerationOutput < a)
							{
								if (Cars[i].Specs.CurrentAccelerationOutput < 0.0)
								{
									Cars[i].Specs.CurrentAccelerationOutput += Cars[i].Specs.JerkBrakeDown * TimeElapsed;
								}
								else
								{
									Cars[i].Specs.CurrentAccelerationOutput += Cars[i].Specs.JerkPowerUp * TimeElapsed;
								}
								if (Cars[i].Specs.CurrentAccelerationOutput > a)
								{
									Cars[i].Specs.CurrentAccelerationOutput = a;
								}
							}
							else
							{
								Cars[i].Specs.CurrentAccelerationOutput -= Cars[i].Specs.JerkPowerDown * TimeElapsed;
								if (Cars[i].Specs.CurrentAccelerationOutput < a)
								{
									Cars[i].Specs.CurrentAccelerationOutput = a;
								}
							}
						}
						else
						{
							Cars[i].Specs.CurrentAccelerationOutput = 0.0;
						}
					}
					// brake
					bool wheellock = wheelspin == 0.0 & Cars[i].Derailed;
					if (!Cars[i].Derailed & wheelspin == 0.0)
					{
						double a;
						// motor
						if (Cars[i].Specs.IsMotorCar & DecelerationDueToMotor[i] != 0.0)
						{
							a = -DecelerationDueToMotor[i];
							if (Cars[i].Specs.CurrentAccelerationOutput > a)
							{
								if (Cars[i].Specs.CurrentAccelerationOutput > 0.0)
								{
									Cars[i].Specs.CurrentAccelerationOutput -= Cars[i].Specs.JerkPowerDown * TimeElapsed;
								}
								else
								{
									Cars[i].Specs.CurrentAccelerationOutput -= Cars[i].Specs.JerkBrakeUp * TimeElapsed;
								}
								if (Cars[i].Specs.CurrentAccelerationOutput < a)
								{
									Cars[i].Specs.CurrentAccelerationOutput = a;
								}
							}
							else
							{
								Cars[i].Specs.CurrentAccelerationOutput += Cars[i].Specs.JerkBrakeDown * TimeElapsed;
								if (Cars[i].Specs.CurrentAccelerationOutput > a)
								{
									Cars[i].Specs.CurrentAccelerationOutput = a;
								}
							}
						}
						// brake
						a = DecelerationDueToBrake[i];
						if (Cars[i].CurrentSpeed >= -0.01 & Cars[i].CurrentSpeed <= 0.01)
						{
							double rf = Cars[i].FrontAxle.Follower.WorldDirection.Y;
							double rr = Cars[i].RearAxle.Follower.WorldDirection.Y;
							double ra = Math.Abs(0.5 * (rf + rr) * Program.CurrentRoute.Atmosphere.AccelerationDueToGravity);
							if (a > ra) a = ra;
						}
						double factor = Cars[i].Specs.MassEmpty / Cars[i].Specs.MassCurrent;
						if (a >= wheelSlipAccelerationBrakeFront)
						{
							wheellock = true;
						}
						else
						{
							FrictionBrakeAcceleration += 0.5 * a * factor;
						}
						if (a >= wheelSlipAccelerationBrakeRear)
						{
							wheellock = true;
						}
						else
						{
							FrictionBrakeAcceleration += 0.5 * a * factor;
						}
					}
					else if (Cars[i].Derailed)
					{
						FrictionBrakeAcceleration += Game.CoefficientOfGroundFriction * Program.CurrentRoute.Atmosphere.AccelerationDueToGravity;
					}
					// motor
					if (Handles.Reverser.Actual != 0)
					{
						double factor = Cars[i].Specs.MassEmpty / Cars[i].Specs.MassCurrent;
						if (Cars[i].Specs.CurrentAccelerationOutput > 0.0)
						{
							PowerRollingCouplerAcceleration += (double)Handles.Reverser.Actual * Cars[i].Specs.CurrentAccelerationOutput * factor;
						}
						else
						{
							double a = -Cars[i].Specs.CurrentAccelerationOutput;
							if (a >= wheelSlipAccelerationMotorFront)
							{
								Cars[i].FrontAxle.CurrentWheelSlip = true;
							}
							else if (!Cars[i].Derailed)
							{
								FrictionBrakeAcceleration += 0.5 * a * factor;
							}
							if (a >= wheelSlipAccelerationMotorRear)
							{
								Cars[i].RearAxle.CurrentWheelSlip = true;
							}
							else
							{
								FrictionBrakeAcceleration += 0.5 * a * factor;
							}
						}
					}
					else
					{
						Cars[i].Specs.CurrentAccelerationOutput = 0.0;
					}
					// perceived speed
					{
						double target;
						if (wheellock)
						{
							target = 0.0;
						}
						else if (wheelspin == 0.0)
						{
							target = Cars[i].CurrentSpeed;
						}
						else
						{
							target = Cars[i].CurrentSpeed + wheelspin / 2500.0;
						}
						double diff = target - Cars[i].Specs.CurrentPerceivedSpeed;
						double rate = (diff < 0.0 ? 5.0 : 1.0) * Program.CurrentRoute.Atmosphere.AccelerationDueToGravity * TimeElapsed;
						rate *= 1.0 - 0.7 / (diff * diff + 1.0);
						double factor = rate * rate;
						factor = 1.0 - factor / (factor + 1000.0);
						rate *= factor;
						if (diff >= -rate & diff <= rate)
						{
							Cars[i].Specs.CurrentPerceivedSpeed = target;
						}
						else
						{
							Cars[i].Specs.CurrentPerceivedSpeed += rate * (double)Math.Sign(diff);
						}
					}
					// calculate new speed
					{
						int d = Math.Sign(Cars[i].CurrentSpeed);
						double a = PowerRollingCouplerAcceleration;
						double b = FrictionBrakeAcceleration;
						if (Math.Abs(a) < b)
						{
							if (Math.Sign(a) == d)
							{
								if (d == 0)
								{
									NewSpeeds[i] = 0.0;
								}
								else
								{
									double c = (b - Math.Abs(a)) * TimeElapsed;
									if (Math.Abs(Cars[i].CurrentSpeed) > c)
									{
										NewSpeeds[i] = Cars[i].CurrentSpeed - (double)d * c;
									}
									else
									{
										NewSpeeds[i] = 0.0;
									}
								}
							}
							else
							{
								double c = (Math.Abs(a) + b) * TimeElapsed;
								if (Math.Abs(Cars[i].CurrentSpeed) > c)
								{
									NewSpeeds[i] = Cars[i].CurrentSpeed - (double)d * c;
								}
								else
								{
									NewSpeeds[i] = 0.0;
								}
							}
						}
						else
						{
							NewSpeeds[i] = Cars[i].CurrentSpeed + (a - b * (double)d) * TimeElapsed;
						}
					}
				}
				// calculate center of mass position
				double[] CenterOfCarPositions = new double[Cars.Length];
				double CenterOfMassPosition = 0.0;
				double TrainMass = 0.0;
				for (int i = 0; i < Cars.Length; i++)
				{
					double pr = Cars[i].RearAxle.Follower.TrackPosition - Cars[i].RearAxle.Position;
					double pf = Cars[i].FrontAxle.Follower.TrackPosition - Cars[i].FrontAxle.Position;
					CenterOfCarPositions[i] = 0.5 * (pr + pf);
					CenterOfMassPosition += CenterOfCarPositions[i] * Cars[i].Specs.MassCurrent;
					TrainMass += Cars[i].Specs.MassCurrent;
				}
				if (TrainMass != 0.0)
				{
					CenterOfMassPosition /= TrainMass;
				}
				{ // coupler
				  // determine closest cars
					int p = -1; // primary car index
					int s = -1; // secondary car index
					{
						double PrimaryDistance = double.MaxValue;
						for (int i = 0; i < Cars.Length; i++)
						{
							double d = Math.Abs(CenterOfCarPositions[i] - CenterOfMassPosition);
							if (d < PrimaryDistance)
							{
								PrimaryDistance = d;
								p = i;
							}
						}
						double SecondDistance = Double.MaxValue;
						for (int i = p - 1; i <= p + 1; i++)
						{
							if (i >= 0 & i < Cars.Length & i != p)
							{
								double d = Math.Abs(CenterOfCarPositions[i] - CenterOfMassPosition);
								if (d < SecondDistance)
								{
									SecondDistance = d;
									s = i;
								}
							}
						}
						if (s >= 0 && PrimaryDistance <= 0.25 * (PrimaryDistance + SecondDistance))
						{
							s = -1;
						}
					}
					// coupler
					bool[] CouplerCollision = new bool[Cars.Length - 1];
					int cf, cr;
					if (s >= 0)
					{
						// use two cars as center of mass
						if (p > s)
						{
							int t = p; p = s; s = t;
						}
						double min = Cars[p].Coupler.MinimumDistanceBetweenCars;
						double max = Cars[p].Coupler.MaximumDistanceBetweenCars;
						double d = CenterOfCarPositions[p] - CenterOfCarPositions[s] - 0.5 * (Cars[p].Length + Cars[s].Length);
						if (d < min)
						{
							double t = (min - d) / (Cars[p].Specs.MassCurrent + Cars[s].Specs.MassCurrent);
							double tp = t * Cars[s].Specs.MassCurrent;
							double ts = t * Cars[p].Specs.MassCurrent;
							Cars[p].UpdateTrackFollowers(tp, false, false);
							Cars[s].UpdateTrackFollowers(-ts, false, false);
							CenterOfCarPositions[p] += tp;
							CenterOfCarPositions[s] -= ts;
							CouplerCollision[p] = true;
						}
						else if (d > max & !Cars[p].Derailed & !Cars[s].Derailed)
						{
							double t = (d - max) / (Cars[p].Specs.MassCurrent + Cars[s].Specs.MassCurrent);
							double tp = t * Cars[s].Specs.MassCurrent;
							double ts = t * Cars[p].Specs.MassCurrent;

							Cars[p].UpdateTrackFollowers(-tp, false, false);
							Cars[s].UpdateTrackFollowers(ts, false, false);
							CenterOfCarPositions[p] -= tp;
							CenterOfCarPositions[s] += ts;
							CouplerCollision[p] = true;
						}
						cf = p;
						cr = s;
					}
					else
					{
						// use one car as center of mass
						cf = p;
						cr = p;
					}
					// front cars
					for (int i = cf - 1; i >= 0; i--)
					{
						double min = Cars[i].Coupler.MinimumDistanceBetweenCars;
						double max = Cars[i].Coupler.MaximumDistanceBetweenCars;
						double d = CenterOfCarPositions[i] - CenterOfCarPositions[i + 1] - 0.5 * (Cars[i].Length + Cars[i + 1].Length);
						if (d < min)
						{
							double t = min - d + 0.0001;
							Cars[i].UpdateTrackFollowers(t, false, false);
							CenterOfCarPositions[i] += t;
							CouplerCollision[i] = true;
						}
						else if (d > max & !Cars[i].Derailed & !Cars[i + 1].Derailed)
						{
							double t = d - max + 0.0001;
							Cars[i].UpdateTrackFollowers(-t, false, false);
							CenterOfCarPositions[i] -= t;
							CouplerCollision[i] = true;
						}
					}
					// rear cars
					for (int i = cr + 1; i < Cars.Length; i++)
					{
						double min = Cars[i - 1].Coupler.MinimumDistanceBetweenCars;
						double max = Cars[i - 1].Coupler.MaximumDistanceBetweenCars;
						double d = CenterOfCarPositions[i - 1] - CenterOfCarPositions[i] - 0.5 * (Cars[i].Length + Cars[i - 1].Length);
						if (d < min)
						{
							double t = min - d + 0.0001;
							Cars[i].UpdateTrackFollowers(-t, false, false);
							CenterOfCarPositions[i] -= t;
							CouplerCollision[i - 1] = true;
						}
						else if (d > max & !Cars[i].Derailed & !Cars[i - 1].Derailed)
						{
							double t = d - max + 0.0001;
							Cars[i].UpdateTrackFollowers(t, false, false);

							CenterOfCarPositions[i] += t;
							CouplerCollision[i - 1] = true;
						}
					}
					// update speeds
					for (int i = 0; i < Cars.Length - 1; i++)
					{
						if (CouplerCollision[i])
						{
							int j;
							for (j = i + 1; j < Cars.Length - 1; j++)
							{
								if (!CouplerCollision[j])
								{
									break;
								}
							}
							double v = 0.0;
							double m = 0.0;
							for (int k = i; k <= j; k++)
							{
								v += NewSpeeds[k] * Cars[k].Specs.MassCurrent;
								m += Cars[k].Specs.MassCurrent;
							}
							if (m != 0.0)
							{
								v /= m;
							}
							for (int k = i; k <= j; k++)
							{
								if (Interface.CurrentOptions.Derailments && Math.Abs(v - NewSpeeds[k]) > 0.5 * Game.CriticalCollisionSpeedDifference)
								{
									Derail(k, TimeElapsed);
								}
								NewSpeeds[k] = v;
							}
							i = j - 1;
						}
					}
				}
				// update average data
				CurrentSpeed = 0.0;
				Specs.CurrentAverageAcceleration = 0.0;
				double invtime = TimeElapsed != 0.0 ? 1.0 / TimeElapsed : 1.0;
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Specs.CurrentAcceleration = (NewSpeeds[i] - Cars[i].CurrentSpeed) * invtime;
					Cars[i].CurrentSpeed = NewSpeeds[i];
					CurrentSpeed += NewSpeeds[i];
					Specs.CurrentAverageAcceleration += Cars[i].Specs.CurrentAcceleration;
				}
				double invcarlen = 1.0 / (double)Cars.Length;
				CurrentSpeed *= invcarlen;
				Specs.CurrentAverageAcceleration *= invcarlen;
			}

			internal void UpdateAtmosphericConstants()
			{
				double h = 0.0;
				for (int i = 0; i < Cars.Length; i++)
				{
					h += Cars[i].FrontAxle.Follower.WorldPosition.Y + Cars[i].RearAxle.Follower.WorldPosition.Y;
				}
				Specs.CurrentElevation = Program.CurrentRoute.Atmosphere.InitialElevation + h / (2.0 * (double)Cars.Length);
				Specs.CurrentAirTemperature = Program.CurrentRoute.Atmosphere.GetAirTemperature(Specs.CurrentElevation);
				Specs.CurrentAirPressure = Program.CurrentRoute.Atmosphere.GetAirPressure(Specs.CurrentElevation, Specs.CurrentAirTemperature);
				Specs.CurrentAirDensity = Program.CurrentRoute.Atmosphere.GetAirDensity(Specs.CurrentAirPressure, Specs.CurrentAirTemperature);
			}

			/// <summary>Updates the safety system for this train</summary>
			internal void UpdateSafetySystem()
			{
				Game.UpdatePluginSections(this);
				if (Plugin != null)
				{
					Plugin.LastSection = CurrentSectionIndex;
					Plugin.UpdatePlugin();
				}
			}

			/// <summary>Updates the objects for all cars in this train</summary>
			/// <param name="TimeElapsed">The time elapsed</param>
			/// <param name="ForceUpdate">Whether this is a forced update</param>
			internal void UpdateObjects(double TimeElapsed, bool ForceUpdate)
			{
				if (!Game.MinimalisticSimulation)
				{
					for (int i = 0; i < Cars.Length; i++)
					{
						Cars[i].UpdateObjects(TimeElapsed, ForceUpdate, true);
						Cars[i].FrontBogie.UpdateObjects(TimeElapsed, ForceUpdate);
						Cars[i].RearBogie.UpdateObjects(TimeElapsed, ForceUpdate);
						Cars[i].Coupler.UpdateObjects(TimeElapsed, ForceUpdate);
					}
				}
			}

			/// <summary>Performs a forced update on all objects attached to the driver car</summary>
			internal void UpdateCabObjects()
			{
				Cars[this.DriverCar].UpdateObjects(0.0, true, false);
			}

			/// <summary>Synchronizes the entire train after a period of infrequent updates</summary>
			internal void Synchronize()
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Syncronize();
				}
			}

			/// <summary>Call this method to derail a car</summary>
			/// <param name="CarIndex">The car index to derail</param>
			/// <param name="ElapsedTime">The elapsed time for this frame (Used for logging)</param>
			public override void Derail(int CarIndex, double ElapsedTime)
			{
				this.Cars[CarIndex].Derailed = true;
				this.Derailed = true;
				if (Program.GenerateDebugLogging)
				{
					Program.FileSystem.AppendToLogFile("Train " + Array.IndexOf(TrainManager.Trains, this) + ", Car " + CarIndex + " derailed. Current simulation time: " + Program.CurrentRoute.SecondsSinceMidnight + " Current frame time: " + ElapsedTime);
				}
			}

			/// <inheritdoc/>
			public override void Derail(AbstractCar Car, double ElapsedTime)
			{
				if (this.Cars.Contains(Car))
				{
					var c = Car as TrainManager.Car;
					c.Derailed = true;
					this.Derailed = true;
					if (Program.GenerateDebugLogging)
					{
						Program.FileSystem.AppendToLogFile("Train " + Array.IndexOf(TrainManager.Trains, this) + ", Car " + c.Index + " derailed. Current simulation time: " + Program.CurrentRoute.SecondsSinceMidnight + " Current frame time: " + ElapsedTime);
					}
				}
			}

			/// <summary>Call this method to topple a car</summary>
			/// <param name="CarIndex">The car index to derail</param>
			/// <param name="ElapsedTime">The elapsed time for this frame (Used for logging)</param>
			internal void Topple(int CarIndex, double ElapsedTime)
			{
				this.Cars[CarIndex].Topples = true;
				if (Program.GenerateDebugLogging)
				{
					Program.FileSystem.AppendToLogFile("Train " + Array.IndexOf(TrainManager.Trains, this) + ", Car " + CarIndex + " toppled. Current simulation time: " + Program.CurrentRoute.SecondsSinceMidnight + " Current frame time: " + ElapsedTime);
				}
			}

			/// <summary>Initializes a train with the default (empty) set of car sounds</summary>
			internal void InitializeCarSounds()
			{
				// initialize
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Sounds.Run = new CarSound[] { };
					Cars[i].Sounds.Flange = new CarSound[] { };
					Cars[i].CarBrake.Air = new CarSound();
					Cars[i].CarBrake.AirHigh = new CarSound();
					Cars[i].CarBrake.AirZero = new CarSound();
					Cars[i].Sounds.Brake = new CarSound();
					Cars[i].Sounds.BreakerResume = new CarSound();
					Cars[i].Sounds.BreakerResumeOrInterrupt = new CarSound();
					Cars[i].Doors[0].CloseSound = new CarSound();
					Cars[i].Doors[1].CloseSound = new CarSound();
					Cars[i].Doors[0].OpenSound = new CarSound();
					Cars[i].Doors[1].OpenSound = new CarSound();
					Cars[i].Sounds.Flange = new CarSound[] { };
					Cars[i].Sounds.FlangeVolume = new double[] { };
					Cars[i].Horns = new TrainManager.Horn[]
					{
						new TrainManager.Horn(),
						new TrainManager.Horn(),
						new TrainManager.Horn()
					};
					Cars[i].Sounds.RequestStop = new CarSound[]
					{
						//Stop
						new CarSound(),
						//Pass
						new CarSound(),
						//Ignored
						new CarSound()
					};
					Cars[i].Sounds.Loop = new CarSound();
					Cars[i].FrontAxle.PointSounds = new CarSound[] { };
					Cars[i].RearAxle.PointSounds = new CarSound[] { };
					Cars[i].CarBrake.Rub = new CarSound();
					Cars[i].Sounds.Run = new CarSound[] { };
					Cars[i].Sounds.RunVolume = new double[] { };
					Cars[i].Sounds.SpringL = new CarSound();
					Cars[i].Sounds.SpringR = new CarSound();
					Cars[i].Sounds.Plugin = new CarSound[] { };
					Cars[i].Sounds.Touch = new CarSound[] { };
				}
			}

			/// <summary>Places the cars</summary>
			/// <param name="TrackPosition">The track position of the front car</param>
			internal void PlaceCars(double TrackPosition)
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					//Front axle track position
					Cars[i].FrontAxle.Follower.TrackPosition = TrackPosition - 0.5 * Cars[i].Length + Cars[i].FrontAxle.Position;
					//Bogie for front axle
					Cars[i].FrontBogie.FrontAxle.Follower.TrackPosition = Cars[i].FrontAxle.Follower.TrackPosition - 0.5 * Cars[i].FrontBogie.Length + Cars[i].FrontBogie.FrontAxle.Position;
					Cars[i].FrontBogie.RearAxle.Follower.TrackPosition = Cars[i].FrontAxle.Follower.TrackPosition - 0.5 * Cars[i].FrontBogie.Length + Cars[i].FrontBogie.RearAxle.Position;
					//Rear axle track position
					Cars[i].RearAxle.Follower.TrackPosition = TrackPosition - 0.5 * Cars[i].Length + Cars[i].RearAxle.Position;
					//Bogie for rear axle
					Cars[i].RearBogie.FrontAxle.Follower.TrackPosition = Cars[i].RearAxle.Follower.TrackPosition - 0.5 * Cars[i].RearBogie.Length + Cars[i].RearBogie.FrontAxle.Position;
					Cars[i].RearBogie.RearAxle.Follower.TrackPosition = Cars[i].RearAxle.Follower.TrackPosition - 0.5 * Cars[i].RearBogie.Length + Cars[i].RearBogie.RearAxle.Position;
					//Beacon reciever (AWS, ATC etc.)
					Cars[i].BeaconReceiver.TrackPosition = TrackPosition - 0.5 * Cars[i].Length + Cars[i].BeaconReceiverPosition;
					TrackPosition -= Cars[i].Length;
					if (i < Cars.Length - 1)
					{
						TrackPosition -= 0.5 * (Cars[i].Coupler.MinimumDistanceBetweenCars + Cars[i].Coupler.MaximumDistanceBetweenCars);
					}
				}
			}

			/// <inheritdoc/>
			public override double FrontCarTrackPosition()
			{
				return Cars[0].FrontAxle.Follower.TrackPosition - Cars[0].FrontAxle.Position + 0.5 * Cars[0].Length;
			}

			/// <inheritdoc/>
			public override double RearCarTrackPosition()
			{
				return Cars[Cars.Length - 1].RearAxle.Follower.TrackPosition - Cars[Cars.Length - 1].RearAxle.Position - 0.5 * Cars[Cars.Length - 1].Length;
			}
		}
	}
}
