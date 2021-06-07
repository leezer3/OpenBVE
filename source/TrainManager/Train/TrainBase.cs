using System;
using System.Globalization;
using System.Linq;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using RouteManager2.MessageManager;
using RouteManager2.SignalManager;
using RouteManager2.Stations;
using SoundManager;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.SafetySystems;

namespace TrainManager.Trains
{
	/*
	 * TEMPORARY NAME AND CLASS TO ALLOW FOR MOVE IN PARTS
	 */
	public partial class TrainBase : AbstractTrain
	{
		/// <summary>Contains information on the specifications of the train</summary>
		public TrainSpecs Specs;
		/// <summary>The cab handles</summary>
		public CabHandles Handles;
		/// <summary>Holds the passengers</summary>
		public TrainPassengers Passengers;
		/// <summary>Holds the safety systems for the train</summary>
		public TrainSafetySystems SafetySystems;
		/// <summary>Holds the cars</summary>
		public CarBase[] Cars;
		/// <summary>The index of the car which the camera is currently anchored to</summary>
		public int CameraCar;
		/// <summary>Coefficient of friction used for braking</summary>
		public const double CoefficientOfGroundFriction = 0.5;
		/// <summary>The speed difference in m/s above which derailments etc. will occur</summary>
		public double CriticalCollisionSpeedDifference = 8.0;
		/// <summary>The time of the last station arrival in seconds since midnight</summary>
		public double StationArrivalTime;
		/// <summary>The time of the last station departure in seconds since midnight</summary>
		public double StationDepartureTime;
		/// <summary>Whether the station departure sound has been triggered</summary>
		public bool StationDepartureSoundPlayed;
		/// <summary>The adjust distance to the station stop point</summary>
		public double StationDistanceToStopPoint;
		/// <summary>The plugin used by this train.</summary>
		public Plugin Plugin;
		/// <summary>The driver body</summary>
		public DriverBody DriverBody;
		/// <summary>Whether the train has currently derailed</summary>
		public bool Derailed;
		/// <summary>Stores the previous route speed limit</summary>
		private double previousRouteLimit;
		/// <summary>Internal timer used for updates</summary>
		private double InternalTimerTimeElapsed;
		/// <inheritdoc/>
		public override bool IsPlayerTrain => this == TrainManagerBase.PlayerTrain;

		/// <inheritdoc/>
		public override int NumberOfCars => this.Cars.Length;

		public TrainBase(TrainState state)
		{
			State = state;
			Destination = TrainManagerBase.CurrentOptions.InitialDestination;
			Station = -1;
			RouteLimits = new[] { double.PositiveInfinity };
			CurrentRouteLimit = double.PositiveInfinity;
			CurrentSectionLimit = double.PositiveInfinity;
			Cars = new CarBase[] { };
				
			Specs.DoorOpenMode = DoorMode.AutomaticManualOverride;
			Specs.DoorCloseMode = DoorMode.AutomaticManualOverride;
		}

		/// <summary>Called once when the simulation loads to initalize the train</summary>
		public virtual void Initialize()
		{
			for (int i = 0; i < Cars.Length; i++)
			{
				Cars[i].Initialize();
			}

			Update(0.0);
		}

		/// <summary>Synchronizes the entire train after a period of infrequent updates</summary>
		public void Synchronize()
		{
			for (int i = 0; i < Cars.Length; i++)
			{
				Cars[i].Syncronize();
			}
		}

		/// <summary>Updates the objects for all cars in this train</summary>
		/// <param name="TimeElapsed">The time elapsed</param>
		/// <param name="ForceUpdate">Whether this is a forced update</param>
		public void UpdateObjects(double TimeElapsed, bool ForceUpdate)
		{
			if (TrainManagerBase.currentHost.SimulationState == SimulationState.Running)
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].UpdateObjects(TimeElapsed, ForceUpdate, true);
					Cars[i].FrontBogie.UpdateObjects(TimeElapsed, ForceUpdate);
					Cars[i].RearBogie.UpdateObjects(TimeElapsed, ForceUpdate);
					if (i == DriverCar && Cars[i].Windscreen != null)
					{
						Cars[i].Windscreen.Update(TimeElapsed);
					}

					Cars[i].Coupler.UpdateObjects(TimeElapsed, ForceUpdate);
				}
			}
		}

		/// <summary>Performs a forced update on all objects attached to the driver car</summary>
		/// <remarks>This function ignores damping of needles etc.</remarks>
		public void UpdateCabObjects()
		{
			Cars[this.DriverCar].UpdateObjects(0.0, true, false);
		}

		/// <summary>Places the cars</summary>
		/// <param name="TrackPosition">The track position of the front car</param>
		public void PlaceCars(double TrackPosition)
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

			TrainManagerBase.currentHost.StopAllSounds(this);

			for (int i = 0; i < TrainManagerBase.CurrentRoute.Sections.Length; i++)
			{
				TrainManagerBase.CurrentRoute.Sections[i].Leave(this);
			}

			if (TrainManagerBase.CurrentRoute.Sections.Length != 0)
			{
				TrainManagerBase.CurrentRoute.UpdateAllSections();
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

		/// <inheritdoc/>
		public override void Update(double TimeElapsed)
		{
			if (State == TrainState.Pending)
			{
				// pending train
				bool forceIntroduction = !IsPlayerTrain && TrainManagerBase.currentHost.SimulationState != SimulationState.MinimalisticSimulation;
				double time = 0.0;
				if (!forceIntroduction)
				{
					for (int i = 0; i < TrainManagerBase.CurrentRoute.Stations.Length; i++)
					{
						if (TrainManagerBase.CurrentRoute.Stations[i].StopMode == StationStopMode.AllStop | TrainManagerBase.CurrentRoute.Stations[i].StopMode == StationStopMode.PlayerPass)
						{
							if (TrainManagerBase.CurrentRoute.Stations[i].ArrivalTime >= 0.0)
							{
								time = TrainManagerBase.CurrentRoute.Stations[i].ArrivalTime;
							}
							else if (TrainManagerBase.CurrentRoute.Stations[i].DepartureTime >= 0.0)
							{
								time = TrainManagerBase.CurrentRoute.Stations[i].DepartureTime - TrainManagerBase.CurrentRoute.Stations[i].StopTime;
							}

							break;
						}
					}

					time -= TimetableDelta;
				}

				if (TrainManagerBase.CurrentRoute.SecondsSinceMidnight >= time | forceIntroduction)
				{
					bool introduce = true;
					if (!forceIntroduction)
					{
						if (CurrentSectionIndex >= 0  && TrainManagerBase.CurrentRoute.Sections.Length > CurrentSectionIndex)
						{
							if (!TrainManagerBase.CurrentRoute.Sections[CurrentSectionIndex].IsFree())
							{
								introduce = false;
							}
						}
					}

					if (this == TrainManagerBase.PlayerTrain && TrainManagerBase.currentHost.SimulationState != SimulationState.Loading)
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
								if (j == DriverCar && IsPlayerTrain && TrainManagerBase.CurrentOptions.InitialViewpoint == 0)
								{
									Cars[j].ChangeCarSection(CarSectionType.Interior);
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
									Cars[j].ChangeCarSection(CarSectionType.Exterior);
									if (IsPlayerTrain && TrainManagerBase.CurrentOptions.InitialViewpoint == 0)
									{
										Cars[j].ChangeCarSection(CarSectionType.NotVisible, true);
									}
								}

							}

							Cars[j].FrontBogie.ChangeSection(!IsPlayerTrain ? 0 : -1);
							Cars[j].RearBogie.ChangeSection(!IsPlayerTrain ? 0 : -1);
							Cars[j].Coupler.ChangeSection(!IsPlayerTrain ? 0 : -1);

							if (Cars[j].Specs.IsMotorCar && Cars[j].Sounds.Loop != null)
							{
								Cars[j].Sounds.Loop.Play(Cars[j], true);
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
					if (previousRouteLimit != CurrentRouteLimit || TrainManagerBase.CurrentOptions.GameMode == GameMode.Arcade)
					{
						/*
						 * HACK: If the limit has changed, or we are in arcade mode, notify the player
						 *       This conforms to the original behaviour, but doesn't need to raise the message from the event.
						 */
						TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString("message_route_overspeed"), MessageDependency.RouteLimit, GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
					}
				}

				if (TrainManagerBase.CurrentOptions.Accessibility)
				{
					if (previousRouteLimit != CurrentRouteLimit)
					{
						//Show for 10s and announce the current speed limit if screen reader present
						TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString("message_route_newlimit"), MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, TrainManagerBase.currentHost.InGameTime + 10.0, null);
					}

					Section nextSection = TrainManagerBase.CurrentRoute.NextSection(FrontCarTrackPosition());
					if (nextSection != null)
					{
						//If we find an appropriate signal, and the distance to it is less than 500m, announce if screen reader is present
						//Aspect announce to be triggered via a separate keybind
						double tPos = nextSection.TrackPosition - FrontCarTrackPosition();
						if (!nextSection.AccessibilityAnnounced && tPos < 500)
						{
							string s = Translations.GetInterfaceString("message_route_nextsection").Replace("[distance]", $"{tPos:0.0}") + "m";
							TrainManagerBase.currentHost.AddMessage(s, MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, TrainManagerBase.currentHost.InGameTime + 10.0, null);
							nextSection.AccessibilityAnnounced = true;
						}
					}
					RouteStation nextStation = TrainManagerBase.CurrentRoute.NextStation(FrontCarTrackPosition());
					if (nextStation != null)
					{
						//If we find an appropriate signal, and the distance to it is less than 500m, announce if screen reader is present
						//Aspect announce to be triggered via a separate keybind
						double tPos = nextStation.DefaultTrackPosition - FrontCarTrackPosition();
						if (!nextStation.AccessibilityAnnounced && tPos < 500)
						{
							string s = Translations.GetInterfaceString("message_route_nextstation").Replace("[distance]", $"{tPos:0.0}") + "m".Replace("[name]", nextStation.Name);
							TrainManagerBase.currentHost.AddMessage(s, MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, TrainManagerBase.currentHost.InGameTime + 10.0, null);
							nextStation.AccessibilityAnnounced = true;
						}
					}
				}
				previousRouteLimit = CurrentRouteLimit;
				if (TrainManagerBase.CurrentOptions.GameMode == GameMode.Arcade)
				{
					if (CurrentSectionLimit == 0.0)
					{
						TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString("message_signal_stop"), MessageDependency.PassedRedSignal, GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
					}
					else if (CurrentSpeed > CurrentSectionLimit)
					{
						TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString("message_signal_overspeed"), MessageDependency.SectionLimit, GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
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
				CarSound c = null;
				if (Cars[i].FrontAxle.PointSoundTriggered)
				{
					Cars[i].FrontAxle.PointSoundTriggered = false;
					int bufferIndex = Cars[i].FrontAxle.RunIndex;
					if (bufferIndex > Cars[i].FrontAxle.PointSounds.Length - 1)
					{
						//If the switch sound does not exist, return zero
						//Required to handle legacy trains which don't have idx specific run sounds defined
						bufferIndex = 0;
					}

					if (Cars[i].FrontAxle.PointSounds == null || Cars[i].FrontAxle.PointSounds.Length == 0)
					{
						//No point sounds defined at all
						continue;
					}

					c = (CarSound) Cars[i].FrontAxle.PointSounds[bufferIndex];
					if (c.Buffer == null)
					{
						c = (CarSound) Cars[i].FrontAxle.PointSounds[0];
					}
				}

				if (c != null)
				{
					double spd = Math.Abs(CurrentSpeed);
					double pitch = spd / 12.5;
					double gain = pitch < 0.5 ? 2.0 * pitch : 1.0;
					if (pitch > 0.2 && gain > 0.2)
					{
						c.Play(pitch, gain, Cars[i], false);
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
			UpdateStation(TimeElapsed);
			UpdateDoors(TimeElapsed);
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
			if (TrainManagerBase.currentHost.SimulationState != SimulationState.MinimalisticSimulation | !IsPlayerTrain)
			{
				UpdateSafetySystem();
			}

			{
				// breaker sound
				bool breaker;
				if (Cars[DriverCar].CarBrake is AutomaticAirBrake)
				{
					breaker = Handles.Reverser.Actual != 0 & Handles.Power.Safety >= 1 & Handles.Brake.Safety == (int) AirBrakeHandleState.Release & !Handles.EmergencyBrake.Safety & !Handles.HoldBrake.Actual;
				}
				else
				{
					breaker = Handles.Reverser.Actual != 0 & Handles.Power.Safety >= 1 & Handles.Brake.Safety == 0 & !Handles.EmergencyBrake.Safety & !Handles.HoldBrake.Actual;
				}

				Cars[DriverCar].Breaker.Update(breaker);
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
						double a = (3.6 * CurrentSectionLimit) * TrainManagerBase.CurrentOptions.SpeedConversionFactor;
						s = s.Replace("[speed]", a.ToString("0", CultureInfo.InvariantCulture));
						s = s.Replace("[unit]", TrainManagerBase.CurrentOptions.UnitOfSpeed);
						TrainManagerBase.currentHost.AddMessage(s, MessageDependency.None, GameMode.Normal, MessageColor.Red, TrainManagerBase.currentHost.InGameTime + 5.0, null);
					}
				}
			}

			// infrequent updates
			InternalTimerTimeElapsed += TimeElapsed;
			if (InternalTimerTimeElapsed > 10.0)
			{
				InternalTimerTimeElapsed -= 10.0;
				Synchronize();
			}
		}

		private void UpdateSpeeds(double TimeElapsed)
		{
			if (TrainManagerBase.currentHost.SimulationState == SimulationState.MinimalisticSimulation & IsPlayerTrain)
			{
				// hold the position of the player's train during startup
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].CurrentSpeed = 0.0;
					Cars[i].Specs.MotorAcceleration = 0.0;
				}

				return;
			}

			// update brake system
			UpdateBrakeSystem(TimeElapsed, out var DecelerationDueToBrake, out var DecelerationDueToMotor);
			// calculate new car speeds
			double[] NewSpeeds = new double[Cars.Length];
			for (int i = 0; i < Cars.Length; i++)
			{
				Cars[i].UpdateSpeed(TimeElapsed, DecelerationDueToMotor[i], DecelerationDueToBrake[i], out NewSpeeds[i]);
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
				CenterOfMassPosition += CenterOfCarPositions[i] * Cars[i].CurrentMass;
				TrainMass += Cars[i].CurrentMass;
			}

			if (TrainMass != 0.0)
			{
				CenterOfMassPosition /= TrainMass;
			}

			{
				// coupler
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
						int t = p;
						p = s;
						s = t;
					}

					double min = Cars[p].Coupler.MinimumDistanceBetweenCars;
					double max = Cars[p].Coupler.MaximumDistanceBetweenCars;
					double d = CenterOfCarPositions[p] - CenterOfCarPositions[s] - 0.5 * (Cars[p].Length + Cars[s].Length);
					if (d < min)
					{
						double t = (min - d) / (Cars[p].CurrentMass + Cars[s].CurrentMass);
						double tp = t * Cars[s].CurrentMass;
						double ts = t * Cars[p].CurrentMass;
						Cars[p].UpdateTrackFollowers(tp, false, false);
						Cars[s].UpdateTrackFollowers(-ts, false, false);
						CenterOfCarPositions[p] += tp;
						CenterOfCarPositions[s] -= ts;
						CouplerCollision[p] = true;
					}
					else if (d > max & !Cars[p].Derailed & !Cars[s].Derailed)
					{
						double t = (d - max) / (Cars[p].CurrentMass + Cars[s].CurrentMass);
						double tp = t * Cars[s].CurrentMass;
						double ts = t * Cars[p].CurrentMass;

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
							v += NewSpeeds[k] * Cars[k].CurrentMass;
							m += Cars[k].CurrentMass;
						}

						if (m != 0.0)
						{
							v /= m;
						}

						for (int k = i; k <= j; k++)
						{
							if (TrainManagerBase.CurrentOptions.Derailments && Math.Abs(v - NewSpeeds[k]) > 0.5 * CriticalCollisionSpeedDifference)
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
				Cars[i].Specs.Acceleration = (NewSpeeds[i] - Cars[i].CurrentSpeed) * invtime;
				Cars[i].CurrentSpeed = NewSpeeds[i];
				CurrentSpeed += NewSpeeds[i];
				Specs.CurrentAverageAcceleration += Cars[i].Specs.Acceleration;
			}

			double invcarlen = 1.0 / Cars.Length;
			CurrentSpeed *= invcarlen;
			Specs.CurrentAverageAcceleration *= invcarlen;
		}

		/// <summary>Updates the safety system plugin for this train</summary>
		internal void UpdateSafetySystem()
		{
			if (Plugin != null)
			{
				SignalData[] data = new SignalData[16];
				int count = 0;
				int start = CurrentSectionIndex >= 0 ? CurrentSectionIndex : 0;
				for (int i = start; i < TrainManagerBase.CurrentRoute.Sections.Length; i++)
				{
					SignalData signal = TrainManagerBase.CurrentRoute.Sections[i].GetPluginSignal(this);
					if (data.Length == count)
					{
						Array.Resize(ref data, data.Length << 1);
					}

					data[count] = signal;
					count++;
					if (signal.Aspect == 0 | count == 16)
					{
						break;
					}
				}

				Array.Resize(ref data, count);
				Plugin.UpdateSignals(data);
				Plugin.LastSection = CurrentSectionIndex;
				Plugin.UpdatePlugin();
			}
		}

		
		/// <summary>Call this method to derail a car</summary>
		/// <param name="CarIndex">The car index to derail</param>
		/// <param name="ElapsedTime">The elapsed time for this frame (Used for logging)</param>
		public override void Derail(int CarIndex, double ElapsedTime)
		{
			this.Cars[CarIndex].Derailed = true;
			this.Derailed = true;
			if (TrainManagerBase.CurrentOptions.GenerateDebugLogging)
			{
				TrainManagerBase.currentHost.AddMessage(MessageType.Information, false, "Car " + CarIndex + " derailed. Current simulation time: " + TrainManagerBase.CurrentRoute.SecondsSinceMidnight + " Current frame time: " + ElapsedTime);
			}
		}

		/// <inheritdoc/>
		public override void Derail(AbstractCar Car, double ElapsedTime)
		{
			if (this.Cars.Contains(Car))
			{
				var c = Car as CarBase;
				// ReSharper disable once PossibleNullReferenceException
				c.Derailed = true;
				this.Derailed = true;
				if (TrainManagerBase.CurrentOptions.GenerateDebugLogging)
				{
					TrainManagerBase.currentHost.AddMessage(MessageType.Information, false, "Car " + c.Index + " derailed. Current simulation time: " + TrainManagerBase.CurrentRoute.SecondsSinceMidnight + " Current frame time: " + ElapsedTime);
				}
			}
		}

		/// <inheritdoc/>
		public override void Reverse()
		{
			double trackPosition = Cars[0].TrackPosition;
			Cars = Cars.Reverse().ToArray();
			for (int i = 0; i < Cars.Length; i++)
			{
				Cars[i].Reverse();
			}
			PlaceCars(trackPosition);
			DriverCar = Cars.Length - 1 - DriverCar;
			UpdateCabObjects();
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

		/// <inheritdoc/>
		public override void SectionChange()
		{
			if (CurrentSectionLimit == 0.0 && TrainManagerBase.currentHost.SimulationState != SimulationState.MinimalisticSimulation)
			{
				TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString("message_signal_stop"), MessageDependency.PassedRedSignal, GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
			}
			else if (CurrentSpeed > CurrentSectionLimit)
			{
				TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString("message_signal_overspeed"), MessageDependency.SectionLimit, GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
			}
		}


		public override void Jump(int stationIndex)
		{
			if (IsPlayerTrain)
			{
				for (int i = 0; i < TrainManagerBase.CurrentRoute.Sections.Length; i++)
				{
					TrainManagerBase.CurrentRoute.Sections[i].AccessibilityAnnounced = false;
				}
			}
			SafetySystems.PassAlarm.Halt();
			int currentTrackElement = Cars[0].FrontAxle.Follower.LastTrackElement;
			StationState = TrainStopState.Jumping;
			int stopIndex = TrainManagerBase.CurrentRoute.Stations[stationIndex].GetStopIndex(NumberOfCars);
			if (stopIndex >= 0)
			{
				if (IsPlayerTrain)
				{
					if (Plugin != null)
					{
						Plugin.BeginJump((InitializationModes) TrainManagerBase.CurrentOptions.TrainStart);
					}
				}

				for (int h = 0; h < Cars.Length; h++)
				{
					Cars[h].CurrentSpeed = 0.0;
				}

				double d = TrainManagerBase.CurrentRoute.Stations[stationIndex].Stops[stopIndex].TrackPosition - Cars[0].FrontAxle.Follower.TrackPosition + Cars[0].FrontAxle.Position - 0.5 * Cars[0].Length;
				if (IsPlayerTrain)
				{
					SoundsBase.SuppressSoundEvents = true;
				}

				while (d != 0.0)
				{
					double x;
					if (Math.Abs(d) > 1.0)
					{
						x = Math.Sign(d);
					}
					else
					{
						x = d;
					}

					for (int h = 0; h < Cars.Length; h++)
					{
						Cars[h].Move(x);
					}

					if (Math.Abs(d) >= 1.0)
					{
						d -= x;
					}
					else
					{
						break;
					}
				}

				if (IsPlayerTrain)
				{
					SoundsBase.SuppressSoundEvents = false;
				}

				if (Handles.EmergencyBrake.Driver)
				{
					Handles.Power.ApplyState(0, false);
				}
				else
				{
					Handles.Brake.ApplyState(Handles.Brake.MaximumNotch, false);
					Handles.Power.ApplyState(0, false);
					if (Handles.Brake is AirBrakeHandle)
					{
						Handles.Brake.ApplyState(AirBrakeHandleState.Service);
					}
					
				}

				if (TrainManagerBase.CurrentRoute.Sections.Length > 0)
				{
					TrainManagerBase.CurrentRoute.UpdateAllSections();
				}

				if (IsPlayerTrain)
				{
					if (TrainManagerBase.CurrentRoute.Stations[stationIndex].ArrivalTime >= 0.0)
					{
						TrainManagerBase.CurrentRoute.SecondsSinceMidnight = TrainManagerBase.CurrentRoute.Stations[stationIndex].ArrivalTime;
					}
					else if (TrainManagerBase.CurrentRoute.Stations[stationIndex].DepartureTime >= 0.0)
					{
						TrainManagerBase.CurrentRoute.SecondsSinceMidnight = TrainManagerBase.CurrentRoute.Stations[stationIndex].DepartureTime - TrainManagerBase.CurrentRoute.Stations[stationIndex].StopTime;
					}
				}

				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Doors[0].AnticipatedOpen = TrainManagerBase.CurrentRoute.Stations[stationIndex].OpenLeftDoors;
					Cars[i].Doors[1].AnticipatedOpen = TrainManagerBase.CurrentRoute.Stations[stationIndex].OpenRightDoors;
				}
				if (IsPlayerTrain)
				{
					if (Plugin != null)
					{
						Plugin.EndJump();
					}
				}

				StationState = TrainStopState.Pending;
				if (IsPlayerTrain)
				{
					LastStation = stationIndex;
				}

				int newTrackElement = Cars[0].FrontAxle.Follower.LastTrackElement;
				if (newTrackElement < currentTrackElement)
				{
					for (int i = newTrackElement; i < currentTrackElement; i++)
					{
						for (int j = 0; j < TrainManagerBase.currentHost.Tracks[0].Elements[i].Events.Length; j++)
						{
							TrainManagerBase.currentHost.Tracks[0].Elements[i].Events[j].Reset();
						}

					}
				}
				TrainManagerBase.currentHost.ProcessJump(this, stationIndex);
			}
		}
	}
}
