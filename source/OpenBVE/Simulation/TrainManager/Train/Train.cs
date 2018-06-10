using System.Globalization;
using System.Reflection;
using OpenBveApi.Colors;
using OpenBveApi.Runtime;
using OpenBveApi.Math;

namespace OpenBve
{
	using System;

	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>The root class for a train within the simulation</summary>
		public class Train
		{
			/// <summary>The plugin used by this train.</summary>
			internal PluginManager.Plugin Plugin;

			internal Handles Handles;
			internal int TrainIndex;
			internal TrainState State;
			internal Car[] Cars;
			internal Coupler[] Couplers;
			internal int DriverCar;
			internal TrainSpecs Specs;
			internal TrainPassengers Passengers;
			internal int LastStation;
			internal int Station;

			internal int Destination = -1;

			internal bool StationFrontCar;
			internal bool StationRearCar;
			internal TrainStopState StationState;
			internal double StationArrivalTime;
			internal double StationDepartureTime;
			internal bool StationDepartureSoundPlayed;
			internal bool StationAdjust;
			internal double StationDistanceToStopPoint;
			internal double[] RouteLimits;
			internal double CurrentRouteLimit;
			internal double CurrentSectionLimit;
			internal int CurrentSectionIndex;
			internal double TimetableDelta;
			internal Game.GeneralAI AI;
			private double InternalTimerTimeElapsed;
			internal bool Derailed;
			internal StopSkipMode NextStopSkipped = StopSkipMode.None;
			internal string[] PowerNotchDescriptions;
			internal string[] LocoBrakeNotchDescriptions;
			internal string[] BrakeNotchDescriptions;
			internal string[] ReverserDescriptions;
			/// <summary>The max width used in px for the power notch HUD string</summary>
			internal int MaxPowerNotchWidth = 48;
			/// <summary>The max width used in px for the brake notch HUD string</summary>
			internal int MaxBrakeNotchWidth = 48;
			/// <summary>The max width used in px for the reverser HUD string</summary>
			internal int MaxReverserWidth = 48;
			/// <summary>The absolute on-disk path to the train's folder</summary>
			internal string TrainFolder;

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
			internal void Dispose()
			{
				State = TrainState.Disposed;
				for (int i = 0; i < Cars.Length; i++)
				{
					int s = Cars[i].CurrentCarSection;
					
					Cars[i].ChangeCarSection(CarSectionType.NotVisible);
					Cars[i].FrontBogie.ChangeSection(-1);
					Cars[i].RearBogie.ChangeSection(-1);
				}
				Sounds.StopAllSounds(this);

				for (int i = 0; i < Game.Sections.Length; i++)
				{
					Game.Sections[i].Leave(this);
				}
				if (Game.Sections.Length != 0)
				{
					Game.UpdateSection(Game.Sections.Length - 1);
				}
			}

			/// <summary>Applies a power and / or brake notch to this train</summary>
			/// <param name="PowerValue">The power notch value</param>
			/// <param name="PowerRelative">Whether this is relative to the current notch</param>
			/// <param name="BrakeValue">The brake notch value</param>
			/// <param name="BrakeRelative">Whether this is relative to the current notch</param>
			internal void ApplyNotch(int PowerValue, bool PowerRelative, int BrakeValue, bool BrakeRelative)
			{
				// determine notch
				int p = PowerRelative ? PowerValue + Handles.Power.Driver : PowerValue;
				if (p < 0)
				{
					p = 0;
				}
				else if (p > Handles.Power.MaximumNotch)
				{
					p = Handles.Power.MaximumNotch;
				}

				int b = BrakeRelative ? BrakeValue + Handles.Brake.Driver : BrakeValue;
				if (b < 0)
				{
					b = 0;
				}
				else if (b > Handles.Brake.MaximumNotch)
				{
					b = Handles.Brake.MaximumNotch;
				}

				// power sound
				if (p < Handles.Power.Driver)
				{
					if (p > 0)
					{
						// down (not min)
						Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.MasterControllerDown.Buffer;
						if (buffer != null)
						{
							Vector3 pos = Cars[DriverCar].Sounds.MasterControllerDown.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
					else
					{
						// min
						Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.MasterControllerMin.Buffer;
						if (buffer != null)
						{
							Vector3 pos = Cars[DriverCar].Sounds.MasterControllerMin.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
				}
				else if (p > Handles.Power.Driver)
				{
					if (p < Handles.Power.MaximumNotch)
					{
						// up (not max)
						Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.MasterControllerUp.Buffer;
						if (buffer != null)
						{
							OpenBveApi.Math.Vector3 pos = Cars[DriverCar].Sounds.MasterControllerUp.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
					else
					{
						// max
						Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.MasterControllerMax.Buffer;
						if (buffer != null)
						{
							OpenBveApi.Math.Vector3 pos = Cars[DriverCar].Sounds.MasterControllerMax.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
				}

				// brake sound
				if (b < Handles.Brake.Driver)
				{
					// brake release
					Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.Brake.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Cars[DriverCar].Sounds.Brake.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
					}

					if (b > 0)
					{
						// brake release (not min)
						buffer = Cars[DriverCar].Sounds.BrakeHandleRelease.Buffer;
						if (buffer != null)
						{
							OpenBveApi.Math.Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleRelease.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
					else
					{
						// brake min
						buffer = Cars[DriverCar].Sounds.BrakeHandleMin.Buffer;
						if (buffer != null)
						{
							OpenBveApi.Math.Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleMin.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
				}
				else if (b > Handles.Brake.Driver)
				{
					// brake
					Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.BrakeHandleApply.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleApply.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
					}
				}

				// apply notch
				if (Handles.SingleHandle)
				{
					if (b != 0) p = 0;
				}

				Handles.Power.Driver = p;
				Handles.Brake.Driver = b;
				Game.AddBlackBoxEntry(Game.BlackBoxEventToken.None);
				// plugin
				if (Plugin != null)
				{
					Plugin.UpdatePower();
					Plugin.UpdateBrake();
				}
			}

			/// <summary>Applies a loco brake notch to this train</summary>
			/// <param name="NotchValue">The loco brake notch value</param>
			/// <param name="Relative">Whether this is relative to the current notch</param>
			internal void ApplyLocoBrakeNotch(int NotchValue, bool Relative)
			{
				int b = Relative ? NotchValue + Handles.LocoBrake.Driver : NotchValue;
				if (b < 0)
				{
					b = 0;
				}
				else if (b > Handles.LocoBrake.MaximumNotch)
				{
					b = Handles.LocoBrake.MaximumNotch;
				}

				// brake sound 
				if (b < Handles.LocoBrake.Driver)
				{
					// brake release 
					Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.Brake.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Cars[DriverCar].Sounds.Brake.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
					}

					if (b > 0)
					{
						// brake release (not min) 
						buffer = Cars[DriverCar].Sounds.BrakeHandleRelease.Buffer;
						if (buffer != null)
						{
							OpenBveApi.Math.Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleRelease.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
					else
					{
						// brake min 
						buffer = Cars[DriverCar].Sounds.BrakeHandleMin.Buffer;
						if (buffer != null)
						{
							OpenBveApi.Math.Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleMin.Position;
							Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
						}
					}
				}
				else if (b > Handles.LocoBrake.Driver)
				{
					// brake 
					Sounds.SoundBuffer buffer = Cars[DriverCar].Sounds.BrakeHandleApply.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Cars[DriverCar].Sounds.BrakeHandleApply.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, this, DriverCar, false);
					}
				}

				Handles.LocoBrake.Driver = b;
				Handles.LocoBrake.Actual = b;
			}


			/// <summary>Call this method to update the train</summary>
			/// <param name="TimeElapsed">The elapsed time this frame</param>
			internal void Update(double TimeElapsed)
			{
				if (State == TrainState.Pending)
				{
					// pending train
					bool forceIntroduction = this == PlayerTrain && !Game.MinimalisticSimulation;
					double time = 0.0;
					if (!forceIntroduction)
					{
						for (int i = 0; i < Game.Stations.Length; i++)
						{
							if (Game.Stations[i].StopMode == StationStopMode.AllStop | Game.Stations[i].StopMode == StationStopMode.PlayerPass)
							{
								if (Game.Stations[i].ArrivalTime >= 0.0)
								{
									time = Game.Stations[i].ArrivalTime;
								}
								else if (Game.Stations[i].DepartureTime >= 0.0)
								{
									time = Game.Stations[i].DepartureTime - Game.Stations[i].StopTime;
								}
								break;
							}
						}
						time -= TimetableDelta;
					}
					if (Game.SecondsSinceMidnight >= time | forceIntroduction)
					{
						bool introduce = true;
						if (!forceIntroduction)
						{
							if (CurrentSectionIndex >= 0)
							{
								if (!Game.Sections[CurrentSectionIndex].IsFree())
								{
									introduce = false;
								}
							}
						}
						if (introduce)
						{
							// train is introduced
							State = TrainState.Available;
							for (int j = 0; j < Cars.Length; j++)
							{
								if (Cars[j].CarSections.Length != 0)
								{
									if (j == this.DriverCar && this == PlayerTrain)
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
										if (this == PlayerTrain)
										{
											this.Cars[j].ChangeCarSection(CarSectionType.NotVisible);

										}
									}

								}
								Cars[j].FrontBogie.ChangeSection(this != PlayerTrain ? 0 : -1);
								Cars[j].RearBogie.ChangeSection(this != PlayerTrain ? 0 : -1);
								if (Cars[j].Specs.IsMotorCar)
								{
									if (Cars[j].Sounds.Loop.Buffer != null)
									{
										Vector3 pos = Cars[j].Sounds.Loop.Position;
										Cars[j].Sounds.Loop.Source = Sounds.PlaySound(Cars[j].Sounds.Loop.Buffer, 1.0, 1.0, pos, this, j, true);
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
					if (Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade)
					{
						if (Specs.CurrentAverageSpeed > CurrentRouteLimit)
						{
							Game.AddMessage(Interface.GetInterfaceString("message_route_overspeed"), MessageManager.MessageDependency.RouteLimit, Interface.GameMode.Arcade, MessageColor.Orange, Double.PositiveInfinity, null);
						}
						if (CurrentSectionLimit == 0.0)
						{
							Game.AddMessage(Interface.GetInterfaceString("message_signal_stop"), MessageManager.MessageDependency.PassedRedSignal, Interface.GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
						}
						else if (Specs.CurrentAverageSpeed > CurrentSectionLimit)
						{
							Game.AddMessage(Interface.GetInterfaceString("message_signal_overspeed"), MessageManager.MessageDependency.SectionLimit, Interface.GameMode.Normal, MessageColor.Orange, Double.PositiveInfinity, null);
						}
					}
					if (AI != null)
					{
						AI.Trigger(this, TimeElapsed);
					}
				}
				else if (State == TrainState.Bogus)
				{
					// bogus train
					if (AI != null)
					{
						AI.Trigger(this, TimeElapsed);
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
					Cars[i].Move(Cars[i].Specs.CurrentSpeed * TimeElapsed, TimeElapsed);
					if (State == TrainState.Disposed)
					{
						return;
					}
				}
				// update station and doors
				UpdateTrainStation(this, TimeElapsed);
				UpdateTrainDoors(this, TimeElapsed);
				// delayed handles
				Handles.Power.Update();
				Handles.Brake.Update();
				Handles.AirBrake.Handle.Update();
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
				if (!Game.MinimalisticSimulation | this != PlayerTrain)
				{
					UpdateSafetySystem();
				}
				{
					// breaker sound
					bool breaker;
					if (Cars[DriverCar].Specs.BrakeType == CarBrakeType.AutomaticAirBrake)
					{
						breaker = Handles.Reverser.Actual != 0 & Handles.Power.Safety >= 1 & Handles.AirBrake.Handle.Safety == AirBrakeHandleState.Release & !Handles.EmergencyBrake.Safety & !Handles.HoldBrake.Actual;
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
							Sounds.PlaySound(Cars[DriverCar].Sounds.BreakerResume.Buffer, 1.0, 1.0, Cars[DriverCar].Sounds.BreakerResume.Position, this, DriverCar, false);
						}
						if (Cars[DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer != null)
						{
							Sounds.PlaySound(Cars[DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer, 1.0, 1.0, Cars[DriverCar].Sounds.BreakerResumeOrInterrupt.Position, this, DriverCar, false);
						}
						Cars[DriverCar].Sounds.BreakerResumed = true;
					}
					else if (!breaker & Cars[DriverCar].Sounds.BreakerResumed)
					{
						// interrupt
						if (Cars[DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer != null)
						{
							Sounds.PlaySound(Cars[DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer, 1.0, 1.0, Cars[DriverCar].Sounds.BreakerResumeOrInterrupt.Position, this, DriverCar, false);
						}
						Cars[DriverCar].Sounds.BreakerResumed = false;
					}
				}
				// passengers
				UpdateTrainPassengers(this, TimeElapsed);
				// signals
				if (CurrentSectionLimit == 0.0)
				{
					if (Handles.EmergencyBrake.Driver & Specs.CurrentAverageSpeed > -0.03 & Specs.CurrentAverageSpeed < 0.03)
					{
						CurrentSectionLimit = 6.94444444444444;
						if (this == PlayerTrain)
						{
							string s = Interface.GetInterfaceString("message_signal_proceed");
							double a = (3.6 * CurrentSectionLimit) * Game.SpeedConversionFactor;
							s = s.Replace("[speed]", a.ToString("0", CultureInfo.InvariantCulture));
							s = s.Replace("[unit]", Game.UnitOfSpeed);
							Game.AddMessage(s, MessageManager.MessageDependency.None, Interface.GameMode.Normal, MessageColor.Red, Game.SecondsSinceMidnight + 5.0, null);
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
				if (Game.MinimalisticSimulation & this == PlayerTrain)
				{
					// hold the position of the player's train during startup
					for (int i = 0; i < Cars.Length; i++)
					{
						Cars[i].Specs.CurrentSpeed = 0.0;
						Cars[i].Specs.CurrentAccelerationOutput = 0.0;
					}
					return;
				}
				// update brake system
				double[] DecelerationDueToBrake, DecelerationDueToMotor;
				UpdateBrakeSystem(this, TimeElapsed, out DecelerationDueToBrake, out DecelerationDueToMotor);
				// calculate new car speeds
				double[] NewSpeeds = new double[Cars.Length];
				for (int i = 0; i < Cars.Length; i++)
				{
					double PowerRollingCouplerAcceleration;
					// rolling on an incline
					{
						double a = Cars[i].FrontAxle.Follower.WorldDirection.Y;
						double b = Cars[i].RearAxle.Follower.WorldDirection.Y;
						PowerRollingCouplerAcceleration = -0.5 * (a + b) * Game.RouteAccelerationDueToGravity;
					}
					// friction
					double FrictionBrakeAcceleration;
					{
						double v = Math.Abs(Cars[i].Specs.CurrentSpeed);
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
						wheelSlipAccelerationMotorFront = GetCriticalWheelSlipAccelerationForElectricMotor(this, i, Cars[i].FrontAxle.Follower.AdhesionMultiplier, Cars[i].FrontAxle.Follower.WorldUp.Y, Cars[i].Specs.CurrentSpeed);
						wheelSlipAccelerationMotorRear = GetCriticalWheelSlipAccelerationForElectricMotor(this, i, Cars[i].RearAxle.Follower.AdhesionMultiplier, Cars[i].RearAxle.Follower.WorldUp.Y, Cars[i].Specs.CurrentSpeed);
						wheelSlipAccelerationBrakeFront = GetCriticalWheelSlipAccelerationForFrictionBrake(this, i, Cars[i].FrontAxle.Follower.AdhesionMultiplier, Cars[i].FrontAxle.Follower.WorldUp.Y, Cars[i].Specs.CurrentSpeed);
						wheelSlipAccelerationBrakeRear = GetCriticalWheelSlipAccelerationForFrictionBrake(this, i, Cars[i].RearAxle.Follower.AdhesionMultiplier, Cars[i].RearAxle.Follower.WorldUp.Y, Cars[i].Specs.CurrentSpeed);
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
										a = Cars[i].Specs.AccelerationCurves[Handles.Power.Actual - 1].GetAccelerationOutput((double)Handles.Reverser.Actual * Cars[i].Specs.CurrentSpeed, 1.0);
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
						if (Cars[i].Specs.CurrentSpeed >= -0.01 & Cars[i].Specs.CurrentSpeed <= 0.01)
						{
							double rf = Cars[i].FrontAxle.Follower.WorldDirection.Y;
							double rr = Cars[i].RearAxle.Follower.WorldDirection.Y;
							double ra = Math.Abs(0.5 * (rf + rr) * Game.RouteAccelerationDueToGravity);
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
						FrictionBrakeAcceleration += Game.CoefficientOfGroundFriction * Game.RouteAccelerationDueToGravity;
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
							target = Cars[i].Specs.CurrentSpeed;
						}
						else
						{
							target = Cars[i].Specs.CurrentSpeed + wheelspin / 2500.0;
						}
						double diff = target - Cars[i].Specs.CurrentPerceivedSpeed;
						double rate = (diff < 0.0 ? 5.0 : 1.0) * Game.RouteAccelerationDueToGravity * TimeElapsed;
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
						int d = Math.Sign(Cars[i].Specs.CurrentSpeed);
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
									if (Math.Abs(Cars[i].Specs.CurrentSpeed) > c)
									{
										NewSpeeds[i] = Cars[i].Specs.CurrentSpeed - (double)d * c;
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
								if (Math.Abs(Cars[i].Specs.CurrentSpeed) > c)
								{
									NewSpeeds[i] = Cars[i].Specs.CurrentSpeed - (double)d * c;
								}
								else
								{
									NewSpeeds[i] = 0.0;
								}
							}
						}
						else
						{
							NewSpeeds[i] = Cars[i].Specs.CurrentSpeed + (a - b * (double)d) * TimeElapsed;
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
					bool[] CouplerCollision = new bool[Couplers.Length];
					int cf, cr;
					if (s >= 0)
					{
						// use two cars as center of mass
						if (p > s)
						{
							int t = p; p = s; s = t;
						}
						double min = Couplers[p].MinimumDistanceBetweenCars;
						double max = Couplers[p].MaximumDistanceBetweenCars;
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
						double min = Couplers[i].MinimumDistanceBetweenCars;
						double max = Couplers[i].MaximumDistanceBetweenCars;
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
						double min = Couplers[i - 1].MinimumDistanceBetweenCars;
						double max = Couplers[i - 1].MaximumDistanceBetweenCars;
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
					for (int i = 0; i < Couplers.Length; i++)
					{
						if (CouplerCollision[i])
						{
							int j;
							for (j = i + 1; j < Couplers.Length; j++)
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
				Specs.CurrentAverageSpeed = 0.0;
				Specs.CurrentAverageAcceleration = 0.0;
				double invtime = TimeElapsed != 0.0 ? 1.0 / TimeElapsed : 1.0;
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Specs.CurrentAcceleration = (NewSpeeds[i] - Cars[i].Specs.CurrentSpeed) * invtime;
					Cars[i].Specs.CurrentSpeed = NewSpeeds[i];
					Specs.CurrentAverageSpeed += NewSpeeds[i];
					Specs.CurrentAverageAcceleration += Cars[i].Specs.CurrentAcceleration;
				}
				double invcarlen = 1.0 / (double)Cars.Length;
				Specs.CurrentAverageSpeed *= invcarlen;
				Specs.CurrentAverageAcceleration *= invcarlen;
			}

			internal void UpdateAtmosphericConstants()
			{
				double h = 0.0;
				for (int i = 0; i < Cars.Length; i++)
				{
					h += Cars[i].FrontAxle.Follower.WorldPosition.Y + Cars[i].RearAxle.Follower.WorldPosition.Y;
				}
				Specs.CurrentElevation = Game.RouteInitialElevation + h / (2.0 * (double)Cars.Length);
				Specs.CurrentAirTemperature = Game.GetAirTemperature(Specs.CurrentElevation);
				Specs.CurrentAirPressure = Game.GetAirPressure(Specs.CurrentElevation, Specs.CurrentAirTemperature);
				Specs.CurrentAirDensity = Game.GetAirDensity(Specs.CurrentAirPressure, Specs.CurrentAirTemperature);
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
			internal void Derail(int CarIndex, double ElapsedTime)
			{
				this.Cars[CarIndex].Derailed = true;
				this.Derailed = true;
				if (Program.GenerateDebugLogging)
				{
					Program.AppendToLogFile("Train " + TrainIndex + ", Car " + CarIndex + " derailed. Current simulation time: " + Game.SecondsSinceMidnight + " Current frame time: " + ElapsedTime);
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
					Program.AppendToLogFile("Train " + TrainIndex + ", Car " + CarIndex + " toppled. Current simulation time: " + Game.SecondsSinceMidnight + " Current frame time: " + ElapsedTime);
				}
			}

			/// <summary>Initializes a train with the default (empty) set of car sounds</summary>
			internal void InitializeCarSounds()
			{
				// initialize
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Sounds.Run = new TrainManager.CarSound[] { };
					Cars[i].Sounds.Flange = new TrainManager.CarSound[] { };
					Cars[i].Sounds.Adjust = TrainManager.CarSound.Empty;
					Cars[i].Sounds.Air = TrainManager.CarSound.Empty;
					Cars[i].Sounds.AirHigh = TrainManager.CarSound.Empty;
					Cars[i].Sounds.AirZero = TrainManager.CarSound.Empty;
					Cars[i].Sounds.Brake = TrainManager.CarSound.Empty;
					Cars[i].Sounds.BrakeHandleApply = TrainManager.CarSound.Empty;
					Cars[i].Sounds.BrakeHandleMin = TrainManager.CarSound.Empty;
					Cars[i].Sounds.BrakeHandleMax = TrainManager.CarSound.Empty;
					Cars[i].Sounds.BrakeHandleRelease = TrainManager.CarSound.Empty;
					Cars[i].Sounds.BreakerResume = TrainManager.CarSound.Empty;
					Cars[i].Sounds.BreakerResumeOrInterrupt = TrainManager.CarSound.Empty;
					Cars[i].Sounds.CpEnd = TrainManager.CarSound.Empty;
					Cars[i].Sounds.CpLoop = TrainManager.CarSound.Empty;
					Cars[i].Sounds.CpStart = TrainManager.CarSound.Empty;
					Cars[i].Doors[0].CloseSound = TrainManager.CarSound.Empty;
					Cars[i].Doors[1].CloseSound = TrainManager.CarSound.Empty;
					Cars[i].Doors[0].OpenSound = TrainManager.CarSound.Empty;
					Cars[i].Doors[1].OpenSound = TrainManager.CarSound.Empty;
					Cars[i].Sounds.EmrBrake = TrainManager.CarSound.Empty;
					Cars[i].Sounds.Flange = new TrainManager.CarSound[] { };
					Cars[i].Sounds.FlangeVolume = new double[] { };
					Cars[i].Sounds.Halt = TrainManager.CarSound.Empty;
					Cars[i].Horns = new TrainManager.Horn[]
					{
						new TrainManager.Horn(),
						new TrainManager.Horn(),
						new TrainManager.Horn()
					};
					Cars[i].Sounds.RequestStop = new CarSound[]
					{
						//Stop
						CarSound.Empty,
						//Pass
						CarSound.Empty,
						//Ignored
						CarSound.Empty
					};
					Cars[i].Sounds.Loop = TrainManager.CarSound.Empty;
					Cars[i].Sounds.MasterControllerUp = TrainManager.CarSound.Empty;
					Cars[i].Sounds.MasterControllerDown = TrainManager.CarSound.Empty;
					Cars[i].Sounds.MasterControllerMin = TrainManager.CarSound.Empty;
					Cars[i].Sounds.MasterControllerMax = TrainManager.CarSound.Empty;
					Cars[i].Sounds.PilotLampOn = TrainManager.CarSound.Empty;
					Cars[i].Sounds.PilotLampOff = TrainManager.CarSound.Empty;
					Cars[i].FrontAxle.PointSounds = new TrainManager.CarSound[] { };
					Cars[i].RearAxle.PointSounds = new TrainManager.CarSound[] { };
					Cars[i].Sounds.ReverserOn = TrainManager.CarSound.Empty;
					Cars[i].Sounds.ReverserOff = TrainManager.CarSound.Empty;
					Cars[i].Sounds.Rub = TrainManager.CarSound.Empty;
					Cars[i].Sounds.Run = new TrainManager.CarSound[] { };
					Cars[i].Sounds.RunVolume = new double[] { };
					Cars[i].Sounds.SpringL = TrainManager.CarSound.Empty;
					Cars[i].Sounds.SpringR = TrainManager.CarSound.Empty;
					Cars[i].Sounds.Plugin = new TrainManager.CarSound[] { };
				}
			}
		}
	}
}
