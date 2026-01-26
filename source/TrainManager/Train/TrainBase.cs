using LibRender2.Screens;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Motor;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using RouteManager2.MessageManager;
using RouteManager2.SignalManager;
using RouteManager2.Stations;
using SoundManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.SafetySystems;
using SafetySystem = TrainManager.SafetySystems.SafetySystem;

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
		/// <summary>Internal timer used for updates</summary>
		private double InternalTimerTimeElapsed;
		/// <inheritdoc/>
		public override bool IsPlayerTrain => ReferenceEquals(this, TrainManagerBase.PlayerTrain);

		/// <summary>The lock to be held whilst operations potentially affecting the makeup of the train are performed</summary>
		internal object updateLock = new object();

		/// <inheritdoc/>
		public override int NumberOfCars => this.Cars.Length;

		public override Dictionary<PowerSupplyTypes, PowerSupply> AvailablePowerSupplies
		{
			get
			{
				Dictionary<PowerSupplyTypes, PowerSupply> supplies = new Dictionary<PowerSupplyTypes, PowerSupply>();
				for (int i = 0; i < Cars.Length; i++)
				{
					if (Cars[i].AvailablePowerSupplies.Count == 0) continue;
					for (int j = 0; j < Cars[i].AvailablePowerSupplies.Count; j++)
					{
						PowerSupplyTypes type = Cars[i].AvailablePowerSupplies.ElementAt(j).Key;
						if (!supplies.ContainsKey(type))
						{
							supplies.Add(type, Cars[i].AvailablePowerSupplies.ElementAt(j).Value);
						}
					}
				}
				return supplies;
			}
		}

		/// <summary>Gets the average cargo loading ratio for this train</summary>
		public double CargoRatio
		{
			get
			{
				double r = 0;
				for (int i = 0; i < Cars.Length; i++)
				{
					r += Cars[i].Cargo.Ratio;
				}

				r /= Cars.Length;
				return r;
			}
		}

		public override double Length
		{
			get
			{
				double myLength = 0;
				for (int i = 0; i < Cars.Length; i++)
				{
					myLength += Cars[i].Length;
				}

				return myLength;
			}
		}

		public override int CurrentSignalAspect
		{
			get
			{
				int nextSectionIndex = CurrentSectionIndex + 1;
				int a = 0;
				if (nextSectionIndex >= 0 & nextSectionIndex < TrainManagerBase.CurrentRoute.Sections.Length)
				{
					a = TrainManagerBase.CurrentRoute.Sections[nextSectionIndex].CurrentAspect;
				}
				return a;
			}
		}

		/// <summary>The direction of travel on the current track</summary>
		public TrackDirection CurrentDirection => TrainManagerBase.CurrentRoute.Tracks[Cars[DriverCar].FrontAxle.Follower.TrackIndex].Direction;

		public TrainBase(TrainState state, TrainType type)
		{
			State = state;
			Type = type;
			Destination = TrainManagerBase.CurrentOptions.InitialDestination;
			Station = -1;
			RouteLimits = new[] { double.PositiveInfinity };
			CurrentRouteLimit = double.PositiveInfinity;
			CurrentSectionLimit = double.PositiveInfinity;
			Cars = new CarBase[] { };
				
			Specs.DoorOpenMode = DoorMode.AutomaticManualOverride;
			Specs.DoorCloseMode = DoorMode.AutomaticManualOverride;
			Specs.PantographState = PantographState.Lowered;
			DriverBody = new DriverBody(this);
			Handles.Reverser = new ReverserHandle(this);
			Handles.EmergencyBrake = new EmergencyHandle(this);
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
		/// <param name="timeElapsed">The time elapsed</param>
		/// <param name="forceUpdate">Whether this is a forced update</param>
		public void UpdateObjects(double timeElapsed, bool forceUpdate)
		{
			if (TrainManagerBase.currentHost.SimulationState == SimulationState.Running)
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].UpdateObjects(timeElapsed, forceUpdate, true);
					Cars[i].FrontBogie.UpdateObjects(timeElapsed, forceUpdate);
					Cars[i].RearBogie.UpdateObjects(timeElapsed, forceUpdate);
					if (i == DriverCar && Cars[i].Windscreen != null)
					{
						Cars[i].Windscreen.Update(timeElapsed);
					}

					Cars[i].Coupler.UpdateObjects(timeElapsed, forceUpdate);
				}
			}
		}

		/// <summary>Performs a forced update on all objects attached to the driver car</summary>
		/// <remarks>This function ignores damping of needles etc.</remarks>
		public void UpdateCabObjects()
		{
			Cars[this.DriverCar].UpdateObjects(0.0, true, false);
		}

		public void PreloadTextures()
		{
			for (int i = 0; i < Cars.Length; i++)
			{
				for (int j = 0; j < Cars[i].CarSections.Count; j++)
				{
					CarSectionType key = Cars[i].CarSections.ElementAt(j).Key;
					for (int k = 0; k < Cars[i].CarSections[key].Groups.Length; k++)
					{
						Cars[i].CarSections[key].Groups[k].PreloadTextures();
					}
					
				}
			}
		}

		/// <summary>Places the cars</summary>
		/// <param name="trackPosition">The track position of the front car</param>
		public void PlaceCars(double trackPosition)
		{
			for (int i = 0; i < Cars.Length; i++)
			{
				//Front axle track position
				Cars[i].FrontAxle.Follower.TrackPosition = trackPosition - 0.5 * Cars[i].Length + Cars[i].FrontAxle.Position;
				//Bogie for front axle
				Cars[i].FrontBogie.FrontAxle.Follower.TrackPosition = Cars[i].FrontAxle.Follower.TrackPosition - 0.5 * Cars[i].FrontBogie.Length + Cars[i].FrontBogie.FrontAxle.Position;
				Cars[i].FrontBogie.RearAxle.Follower.TrackPosition = Cars[i].FrontAxle.Follower.TrackPosition - 0.5 * Cars[i].FrontBogie.Length + Cars[i].FrontBogie.RearAxle.Position;
				//Rear axle track position
				Cars[i].RearAxle.Follower.TrackPosition = trackPosition - 0.5 * Cars[i].Length + Cars[i].RearAxle.Position;
				//Bogie for rear axle
				Cars[i].RearBogie.FrontAxle.Follower.TrackPosition = Cars[i].RearAxle.Follower.TrackPosition - 0.5 * Cars[i].RearBogie.Length + Cars[i].RearBogie.FrontAxle.Position;
				Cars[i].RearBogie.RearAxle.Follower.TrackPosition = Cars[i].RearAxle.Follower.TrackPosition - 0.5 * Cars[i].RearBogie.Length + Cars[i].RearBogie.RearAxle.Position;
				//Beacon reciever (AWS, ATC etc.)
				Cars[i].BeaconReceiver.TrackPosition = trackPosition - 0.5 * Cars[i].Length + Cars[i].BeaconReceiverPosition;
				trackPosition -= Cars[i].Length;
				if (i < Cars.Length - 1)
				{
					trackPosition -= 0.5 * (Cars[i].Coupler.MinimumDistanceBetweenCars + Cars[i].Coupler.MaximumDistanceBetweenCars);
				}
			}
		}

		/// <summary>Disposes of the train</summary>
		public override void Dispose()
		{
			State = TrainState.DisposePending;
			for (int i = 0; i < Cars.Length; i++)
			{
				Cars[i].ChangeCarSection(CarSectionType.NotVisible);
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
			Plugin?.UpdateBeacon(transponderType, sectionIndex, optional);
		}

		/// <inheritdoc/>
		public override void Update(double timeElapsed)
		{
			lock (updateLock)
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
							if (CurrentSectionIndex >= 0 && TrainManagerBase.CurrentRoute.Sections.Length > CurrentSectionIndex)
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

								if (Cars[j].TractionModel !=  null && Cars[j].TractionModel.ProvidesPower && Cars[j].Sounds.Loop != null)
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
					UpdatePhysicsAndControls(timeElapsed);
					if (TrainManagerBase.CurrentOptions.Accessibility)
					{
						Section nextSection = TrainManagerBase.CurrentRoute.NextSection(FrontCarTrackPosition);
						if (nextSection != null)
						{
							//If we find an appropriate signal, and the distance to it is less than 500m, announce if screen reader is present
							//Aspect announce to be triggered via a separate keybind
							double tPos = nextSection.TrackPosition - FrontCarTrackPosition;
							if (!nextSection.AccessibilityAnnounced && tPos < 500)
							{
								string s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "message", "route_nextsection" }).Replace("[distance]", $"{tPos:0.0}") + "m";
								TrainManagerBase.currentHost.AddMessage(s, MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, 10.0, null);
								nextSection.AccessibilityAnnounced = true;
							}
						}

						RouteStation nextStation = TrainManagerBase.CurrentRoute.NextStation(FrontCarTrackPosition);
						if (nextStation != null)
						{
							//If we find an appropriate signal, and the distance to it is less than 500m, announce if screen reader is present
							//Aspect announce to be triggered via a separate keybind
							double tPos = nextStation.DefaultTrackPosition - FrontCarTrackPosition;
							if (!nextStation.AccessibilityAnnounced && tPos < 500)
							{
								string s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "message", "route_nextstation" }).Replace("[distance]", $"{tPos:0.0}") + "m".Replace("[name]", nextStation.Name);
								TrainManagerBase.currentHost.AddMessage(s, MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, 10.0, null);
								nextStation.AccessibilityAnnounced = true;
							}
						}
					}

					if (TrainManagerBase.CurrentOptions.GameMode == GameMode.Arcade)
					{
						if (CurrentSectionLimit == 0.0)
						{
							TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "message", "signal_stop" }), MessageDependency.PassedRedSignal, GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
						}
						else if (CurrentSpeed > CurrentSectionLimit)
						{
							TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "message", "signal_overspeed" }), MessageDependency.SectionLimit, GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
						}
					}

					AI?.Trigger(timeElapsed);
				}
				else if (State == TrainState.Bogus)
				{
					// bogus train
					AI?.Trigger(timeElapsed);
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

						c = (CarSound)Cars[i].FrontAxle.PointSounds[bufferIndex];
						if (c.Buffer == null)
						{
							c = (CarSound)Cars[i].FrontAxle.PointSounds[0];
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
		}

		/// <summary>Updates the physics and controls for this train</summary>
		/// <param name="timeElapsed">The time elapsed</param>
		private void UpdatePhysicsAndControls(double timeElapsed)
		{
			if (timeElapsed == 0.0 || timeElapsed > 1000)
			{
				//HACK: The physics engine really does not like update times above 1000ms
				//This works around a bug experienced when jumping to a station on a steep hill
				//causing exessive acceleration
				return;
			}

			// Car level initial processing
			for (int i = 0; i < Cars.Length; i++)
			{
				// move cars
				
				Cars[i].Move((double.IsNaN(Cars[i].CurrentSpeed) ? 0 : Cars[i].CurrentSpeed) * timeElapsed);
				if (State >= TrainState.DisposePending)
				{
					return;
				}
				// update cargo and related score
				Cars[i].Cargo.Update(Specs.CurrentAverageAcceleration, timeElapsed);
			}

			// update station and doors
			UpdateStation(timeElapsed);
			UpdateDoors(timeElapsed);
			// delayed handles
			if (Plugin == null)
			{
				Handles.Power.ApplySafetyState(Handles.Power.Driver);
				Handles.Brake.ApplySafetyState(Handles.Brake.Driver);
				Handles.EmergencyBrake.Safety = Handles.EmergencyBrake.Driver;
			}

			Handles.Power.Update(timeElapsed);
			Handles.Brake.Update(timeElapsed);
			if (Handles.HasLocoBrake)
			{
				Handles.LocoBrake.Update(timeElapsed);
			}
			Handles.EmergencyBrake.Update();
			Handles.HoldBrake.Actual = Handles.HoldBrake.Driver;
			for(int i = 0; i < Cars[DriverCar].SafetySystems.Count; i++)
			{
				SafetySystem system = Cars[DriverCar].SafetySystems.ElementAt(i).Key;
				Cars[DriverCar].SafetySystems[system].Update(timeElapsed);
			}
			// update speeds
			UpdateSpeeds(timeElapsed);
			// Update Run and Motor sounds
			for (int i = 0; i < Cars.Length; i++)
			{
				Cars[i].Run.Update(timeElapsed);
				for (int j = 0; j < Cars[i].Sounds.ControlledSounds.Count; j++)
				{
					Cars[i].Sounds.ControlledSounds[j].Update(timeElapsed);
				}
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
				Cars[DriverCar].Breaker?.Update(breaker);
			}
			// signals
			if (CurrentSectionLimit == 0.0)
			{
				if (Handles.EmergencyBrake.Driver & CurrentSpeed > -0.03 & CurrentSpeed < 0.03)
				{
					CurrentSectionLimit = 6.94444444444444;
					if (IsPlayerTrain)
					{
						string s = Translations.GetInterfaceString(HostApplication.OpenBve, new [] {"message","signal_proceed"});
						double a = (3.6 * CurrentSectionLimit) * TrainManagerBase.CurrentOptions.SpeedConversionFactor;
						s = s.Replace("[speed]", a.ToString("0", CultureInfo.InvariantCulture));
						s = s.Replace("[unit]", TrainManagerBase.CurrentOptions.UnitOfSpeed);
						TrainManagerBase.currentHost.AddMessage(s, MessageDependency.None, GameMode.Normal, MessageColor.Red, 5.0, null);
					}
				}
			}

			// infrequent updates
			InternalTimerTimeElapsed += timeElapsed;
			if (InternalTimerTimeElapsed > 10.0)
			{
				InternalTimerTimeElapsed -= 10.0;
				Synchronize();
			}
		}

		private void UpdateSpeeds(double timeElapsed)
		{
			if (TrainManagerBase.currentHost.SimulationState == SimulationState.MinimalisticSimulation & IsPlayerTrain)
			{
				// hold the position of the player's train during startup
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].CurrentSpeed = 0.0;
					Cars[i].TractionModel.CurrentAcceleration = 0.0;
				}

				return;
			}

			// update brake system
			UpdateBrakeSystem(timeElapsed, out var DecelerationDueToBrake, out var DecelerationDueToMotor);
			
			double[] NewSpeeds = new double[Cars.Length];
			double[] CenterOfCarPositions = new double[Cars.Length];
			double CenterOfMassPosition = 0.0;
			double TrainMass = 0.0;
			for (int i = 0; i < Cars.Length; i++)
			{
				// calculate new car speeds
				Cars[i].UpdateSpeed(timeElapsed, DecelerationDueToMotor[i], DecelerationDueToBrake[i], out NewSpeeds[i]);
				// calculate center of mass position
				double pr = Cars[i].RearAxle.Follower.TrackPosition - Cars[i].RearAxle.Position;
				double pf = Cars[i].FrontAxle.Follower.TrackPosition - Cars[i].FrontAxle.Position;
				CenterOfCarPositions[i] = 0.5 * (pr + pf);
				CenterOfMassPosition += CenterOfCarPositions[i] * Cars[i].CurrentMass;
				TrainMass += Cars[i].CurrentMass;
				// update engine etc.
				Cars[i].TractionModel?.Update(timeElapsed);
			}

			if (TrainMass != 0.0)
			{
				CenterOfMassPosition /= TrainMass;
			}

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

				double SecondDistance = double.MaxValue;
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
					(s, p) = (p, s);
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
							Derail(k, timeElapsed);
						}

						NewSpeeds[k] = v;
					}

					i = j - 1;
				}
			}
			// update average data
			CurrentSpeed = 0.0;
			Specs.CurrentAverageAcceleration = 0.0;
			double invtime = timeElapsed != 0.0 ? 1.0 / timeElapsed : 1.0;
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
			else
			{
				Handles.Reverser.Actual = Handles.Reverser.Driver;
			}
		}

		
		/// <summary>Call this method to derail a car</summary>
		/// <param name="carIndex">The car index to derail</param>
		/// <param name="elapsedTime">The elapsed time for this frame (Used for logging)</param>
		public override void Derail(int carIndex, double elapsedTime)
		{
			this.Cars[carIndex].Derailed = true;
			this.Derailed = true;
			if (Cars[carIndex].Sounds.Loop != null)
			{
				TrainManagerBase.currentHost.StopSound(Cars[carIndex].Sounds.Loop.Source);
			}
			Cars[carIndex].Run.Stop();

			if (TrainManagerBase.CurrentOptions.GenerateDebugLogging)
			{
				TrainManagerBase.currentHost.AddMessage(MessageType.Information, false, "Car " + carIndex + " derailed. Current simulation time: " + TrainManagerBase.CurrentRoute.SecondsSinceMidnight + " Current frame time: " + elapsedTime);
			}
		}

		/// <inheritdoc/>
		public override void Derail(AbstractCar car, double elapsedTime)
		{
			if (this.Cars.Contains(car))
			{
				var c = car as CarBase;
				// ReSharper disable once PossibleNullReferenceException
				if (c.Sounds.Loop != null)
				{
					TrainManagerBase.currentHost.StopSound(c.Sounds.Loop.Source);
				}
				c.Run.Stop();
				c.Derailed = true;
				this.Derailed = true;
				if (TrainManagerBase.CurrentOptions.GenerateDebugLogging)
				{
					TrainManagerBase.currentHost.AddMessage(MessageType.Information, false, "Car " + c.Index + " derailed. Current simulation time: " + TrainManagerBase.CurrentRoute.SecondsSinceMidnight + " Current frame time: " + elapsedTime);
				}
			}
		}

		/// <inheritdoc/>
		public override void Reverse(bool flipInterior = false, bool flipDriver = false)
		{
			lock (updateLock)
			{
				double trackPosition = Cars[0].TrackPosition;
				Cars = Cars.Reverse().ToArray();
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Index = i;
					Cars[i].Reverse(flipInterior);
					// Re-create the coupler with appropriate distances between the cars
					double minDistance = 0, maxDistance = 0;
					if (i < Cars.Length - 1)
					{
						minDistance = Cars[i + 1].Coupler.MinimumDistanceBetweenCars;
						maxDistance = Cars[i + 1].Coupler.MaximumDistanceBetweenCars;
					}

					Cars[i].Coupler = new Coupler(minDistance, maxDistance, Cars[i], i < Cars.Length - 1 ? Cars[i + 1] : null);
					for (int j = 0; j < Cars[i].ParticleSources.Count; j++)
					{
						Cars[i].ParticleSources[j].Offset.Z = -Cars[i].ParticleSources[j].Offset.Z;
					}
				}

				PlaceCars(trackPosition);
				DriverCar = Cars.Length - 1 - DriverCar;
				CameraCar = Cars.Length - 1 - CameraCar;
				UpdateCabObjects();
			}
		}



		/// <inheritdoc/>
		public override double FrontCarTrackPosition
		{
			get
			{
				if (CurrentDirection == TrackDirection.Reverse)
				{
					return Cars[Cars.Length - 1].FrontAxle.Follower.TrackPosition - Cars[Cars.Length - 1].FrontAxle.Position + 0.5 * Cars[Cars.Length -  1].Length;
				}
				return Cars[0].FrontAxle.Follower.TrackPosition - Cars[0].FrontAxle.Position + 0.5 * Cars[0].Length;
			}
		}
		
		/// <inheritdoc/>
		public override double RearCarTrackPosition
		{
			get
			{
				if (CurrentDirection == TrackDirection.Reverse)
				{
					return Cars[0].FrontAxle.Follower.TrackPosition - Cars[0].FrontAxle.Position + 0.5 * Cars[0].Length;	
				}
				return Cars[Cars.Length - 1].RearAxle.Follower.TrackPosition - Cars[Cars.Length - 1].RearAxle.Position - 0.5 * Cars[Cars.Length - 1].Length;
			}
		}

		/// <inheritdoc/>
		public override void SectionChange()
		{
			if (CurrentSectionLimit == 0.0 && TrainManagerBase.currentHost.SimulationState != SimulationState.MinimalisticSimulation)
			{
				TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new [] {"message","signal_stop"}), MessageDependency.PassedRedSignal, GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
			}
			else if (CurrentSpeed > CurrentSectionLimit)
			{
				TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new [] {"message","signal_overspeed"}), MessageDependency.SectionLimit, GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
			}
		}


		public override void Jump(int stationIndex, int trackKey)
		{
			if (IsPlayerTrain)
			{
				for (int i = 0; i < TrainManagerBase.CurrentRoute.Sections.Length; i++)
				{
					TrainManagerBase.CurrentRoute.Sections[i].AccessibilityAnnounced = false;
				}
			}

			SafetySystems.PassAlarm?.Halt();
			int currentTrackElement = Cars[0].FrontAxle.Follower.LastTrackElement;
			StationState = TrainStopState.Jumping;
			int stopIndex = TrainManagerBase.CurrentRoute.Stations[stationIndex].GetStopIndex(this);
			if (stopIndex >= 0)
			{
				if (IsPlayerTrain)
				{
					Plugin?.BeginJump((InitializationModes) TrainManagerBase.CurrentOptions.TrainStart);
				}

				for (int h = 0; h < Cars.Length; h++)
				{
					Cars[h].CurrentSpeed = 0.0;
					// Change the track followers to the appropriate track
					Cars[h].FrontAxle.Follower.TrackIndex = trackKey;
					Cars[h].RearAxle.Follower.TrackIndex = trackKey;
					Cars[h].FrontBogie.FrontAxle.Follower.TrackIndex = trackKey;
					Cars[h].FrontBogie.RearAxle.Follower.TrackIndex = trackKey;
					Cars[h].RearBogie.FrontAxle.Follower.TrackIndex = trackKey;
					Cars[h].RearBogie.RearAxle.Follower.TrackIndex = trackKey;
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
					if (TrainManagerBase.CurrentRoute.Stations[stationIndex].JumpTime > 0.0)
					{
						// jump time is set, so use that (BVE5)
						TrainManagerBase.CurrentRoute.SecondsSinceMidnight = TrainManagerBase.CurrentRoute.Stations[stationIndex].JumpTime;
					}
					else if (TrainManagerBase.CurrentRoute.Stations[stationIndex].ArrivalTime >= 0.0)
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
					Cars[i].ReAdhesionDevice?.Jump();
				}
				if (IsPlayerTrain)
				{
					Plugin?.EndJump();
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
						for (int j = 0; j < TrainManagerBase.currentHost.Tracks[0].Elements[i].Events.Count; j++)
						{
							TrainManagerBase.currentHost.Tracks[0].Elements[i].Events[j].Reset();
						}

					}
				}
				TrainManagerBase.currentHost.ProcessJump(this, stationIndex, 0);
			}
		}

		/// <summary>Change the camera car</summary>
		/// <param name="shouldIncrement">Whether to increase or decrease the camera car index</param>
		public void ChangeCameraCar(bool shouldIncrement)
		{
			if (CurrentDirection != TrackDirection.Reverse)
			{
				// If in the reverse direction, the last car is Car0 and the direction of increase is reversed
				shouldIncrement = !shouldIncrement;
			}

			if (shouldIncrement)
			{
				if (CameraCar < Cars.Length - 1)
				{
					CameraCar++;
					TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new [] {"notification","exterior"}) + " " + (CurrentDirection == TrackDirection.Reverse ? Cars.Length - CameraCar : CameraCar + 1), MessageDependency.CameraView, GameMode.Expert,
						MessageColor.White, 2.0, null);
				}
			}
			else
			{
				if (CameraCar > 0)
				{
					CameraCar--;
					TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new [] {"notification","exterior"}) + " " + (CurrentDirection == TrackDirection.Reverse ? Cars.Length - CameraCar : CameraCar + 1), MessageDependency.CameraView, GameMode.Expert, MessageColor.White, 2.0, null);
				}
			}

		}

		public override void Couple(AbstractTrain train, bool front)
		{
			TrainBase trainBase = train as TrainBase;
			if (trainBase == null)
			{
				throw new Exception("Attempted to couple to something that isn't a train");
			}

			int oldCars = Cars.Length;
			/*
			 * NOTE: Need to set the speeds to zero for *both* trains on coupling
			 *       The 'old' train will be disposed of immediately, but if not
			 *       set to zero, we can get glitched acceleration and the train enters orbit....
			 */

			if (front)
			{
				CarBase[] newCars = new CarBase[Cars.Length + trainBase.Cars.Length];
				Array.Copy(Cars, 0, newCars, trainBase.Cars.Length, Cars.Length);
				Array.Copy(trainBase.Cars, 0, newCars, 0, trainBase.Cars.Length);
				Cars = newCars;
				// camera / driver car is now down the train
				DriverCar += trainBase.Cars.Length;
				CameraCar += trainBase.Cars.Length;
			}
			else
			{
				Array.Resize(ref Cars, Cars.Length + trainBase.Cars.Length);
				// add new cars to end
				for (int i = 0; i < trainBase.Cars.Length; i++)
				{
					Cars[i + oldCars] = trainBase.Cars[i];
					Cars[i + oldCars].Index = i + oldCars;
					trainBase.Cars[i].CurrentSpeed = 0;
				}
			}

			// set properties
			for (int i = 0; i < Cars.Length; i++)
			{
				Cars[i].baseTrain = this;
				Cars[i].CurrentSpeed = 0;
				if ((int)TrainManagerBase.Renderer.Camera.CurrentMode > 1)
				{
					Cars[i].ChangeCarSection(CarSectionType.Exterior);
				}
				Cars[i].FrontAxle.Follower.Train = this;
				Cars[i].RearAxle.Follower.Train = this;
				Cars[i].FrontBogie.FrontAxle.Follower.Train = this;
				Cars[i].FrontBogie.RearAxle.Follower.Train = this;
				Cars[i].RearBogie.FrontAxle.Follower.Train = this;
				Cars[i].RearBogie.RearAxle.Follower.Train = this;
				if (i < Cars.Length - 1)
				{
					Cars[i].Coupler.ConnectedCar = Cars[i + 1];
				}
				Cars[i].Index = i;
			}

			Cars[oldCars].Coupler.CoupleSound.Play(1.0, 1.0, Cars[oldCars], false);

			Cars[0].BeaconReceiver.Train = this;

			/*
			 * Reset properties for 'old' train to empty cars
			 * 
			 * Due to multi-threading, we can't guarantee
			 * that something isn't trying to access the cars
			 * array until the *next* complete frame.
			 */
			for (int i = 0; i < trainBase.Cars.Length; i++)
			{
				trainBase.Cars[i] = new CarBase(trainBase, i);
			}

			Cars[DriverCar].Sounds.CoupleCab?.Play(1.0, 1.0, Cars[DriverCar], false);

			string message = Translations.GetInterfaceString(HostApplication.OpenBve, front ? new[] { "notification", "couple_front" } : new[] { "notification", "couple_rear" }).Replace("[number]", trainBase.Cars.Length.ToString());
			TrainManagerBase.currentHost.AddMessage(message, MessageDependency.None, GameMode.Normal, MessageColor.White, 5.0, null);
		}

		public void ContactSignaller()
		{
			Section sct = TrainManagerBase.CurrentRoute.Sections[CurrentSectionIndex].NextSection;
			if (sct.Type != SectionType.PermissiveValueBased && sct.Type != SectionType.PermissiveIndexBased)
			{
				// not a valid section to access
				string s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "message", "signal_access_invalid" });
				TrainManagerBase.currentHost.AddMessage(s, MessageDependency.None, GameMode.Expert, MessageColor.White, 10.0, null);
			}
			else
			{
				if (sct.IsFree())
				{
					// section is free of trains, so can be permissively accessed
					string s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "message", "signal_access_granted" });
					TrainManagerBase.currentHost.AddMessage(s, MessageDependency.None, GameMode.Expert, MessageColor.White, 10.0, null);
					sct.SignallerPermission = true;
				}
				else
				{
					// not free, access denied
					string s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "message", "signal_access_denied" });
					TrainManagerBase.currentHost.AddMessage(s, MessageDependency.None, GameMode.Expert, MessageColor.Red, 10.0, null);
					sct.SignallerPermission = false;
				}
			}
		}

		public void UpdateParticleSources(double timeElapsed)
		{
			for (int i = 0; i < Cars.Length; i++)
			{
				if (Cars[i].ParticleSources.Count == 0)
				{
					continue;
				}
				Cars[i].CreateWorldCoordinates(Vector3.Zero, out Vector3 p, out _);
				Vector3 cd = new Vector3(p - TrainManagerBase.Renderer.Camera.AbsolutePosition);
				double dist = cd.NormSquared();
				double bid = TrainManagerBase.Renderer.Camera.ViewingDistance + 30;
				bool currentlyVisible = dist < bid * bid;
				for (int j = 0; j < Cars[i].ParticleSources?.Count; j++)
				{
					Cars[i].ParticleSources[j]?.Update(TrainManagerBase.Renderer.CurrentInterface == InterfaceType.Normal ? timeElapsed : 0, currentlyVisible);
				}
			}
		}
	}
}
