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
		public CabHandles Handles => Cars[DriverCar].Handles;
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
		}

		/// <summary>Called once when the simulation loads to initialize the train</summary>
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
				Cars[i].Synchronize();
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
				if (d < min && min < max)
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
				else if (d < min)
				{
					/*
					 * References:
					 * https://github.com/leezer3/OpenBVE/issues/1258
					 * https://github.com/leezer3/OpenBVE/issues/1298
					 * If min == max we don't want to move our cars here
					 * (as the following code may then move them again in the opposite direction, causing a 'vibrating' effect)
					 *
					 * However what the original fix overlooked, is that if we don't set the collision flag, and collision
					 * is not detected in the following code, when the speed is updated the car carries on 'into' the car in front,
					 * as the updates speeds loop relies on the CouplerCollisions array 
					 * 
					 */
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

		/// <summary>Change the camera car</summary>
		/// <param name="shouldIncrement">Whether to increase or decrease the camera car index</param>
		public void ChangeCameraCar(bool shouldIncrement)
		{
			if (CurrentDirection != TrackDirection.Reverse)
			{
				// If in the reverse direction, the last car is Car0 and the direction of increase is reversed
				shouldIncrement = !shouldIncrement;
			}
			
			int currentTarget = TrainManagerBase.Renderer.Camera.TargetCameraCar != -1 ? TrainManagerBase.Renderer.Camera.TargetCameraCar : CameraCar;
			int nextCar = shouldIncrement ? currentTarget + 1 : currentTarget - 1;
			
			if (nextCar >= 0 && nextCar < Cars.Length)
			{
				TrainManagerBase.Renderer.Camera.TargetCameraCar = nextCar;
				
				if (!TrainManagerBase.Renderer.Camera.IsTransitioning)
				{
					TrainManagerBase.Renderer.Camera.PreviousCameraCar = CameraCar;
					CameraCar = nextCar;
					TrainManagerBase.Renderer.Camera.TargetCameraCar = -1;
					TrainManagerBase.Renderer.Camera.IsTransitioning = true;
					TrainManagerBase.Renderer.Camera.CameraCarTransitionTimer = 0.0;
				}
				
				TrainManagerBase.currentHost.AddMessage(
					Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "notification", "exterior" }) + " " + (CurrentDirection == TrackDirection.Reverse ? Cars.Length - nextCar : nextCar + 1),
					MessageDependency.CameraView,
					GameMode.Expert,
					MessageColor.White,
					2.0,
					null);
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
