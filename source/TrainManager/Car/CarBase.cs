using System;
using System.Linq;
using LibRender2;
using LibRender2.Camera;
using LibRender2.Cameras;
using LibRender2.Trains;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using OpenBveApi.World;
using TrainManager.Brake;
using TrainManager.BrakeSystems;
using TrainManager.Car.Systems;
using TrainManager.Cargo;
using TrainManager.Handles;
using TrainManager.Power;
using TrainManager.SafetySystems;
using TrainManager.Trains;

namespace TrainManager.Car
{
	/*
	 * TEMPORARY NAME AND CLASS TO ALLOW FOR MOVE IN PARTS
	 */
	public class CarBase : AbstractCar
	{
		/// <summary>A reference to the base train</summary>
		public TrainBase baseTrain;
		/// <summary>The front bogie</summary>
		public Bogie FrontBogie;
		/// <summary>The rear bogie</summary>
		public Bogie RearBogie;
		/// <summary>The doors for this car</summary>
		public Door[] Doors;
		/// <summary>The horns attached to this car</summary>
		public Horn[] Horns;
		/// <summary>Contains the physics properties for the car</summary>
		public readonly CarPhysics Specs;
		/// <summary>The car brake for this car</summary>
		public CarBrake CarBrake;
		/// <summary>The car sections (objects) attached to the car</summary>
		public CarSection[] CarSections;
		/// <summary>The index of the current car section</summary>
		public int CurrentCarSection;
		/// <summary>The driver's eye position within the car</summary>
		public Vector3 Driver;
		/// <summary>The current yaw of the driver's eyes</summary>
		public double DriverYaw;
		/// <summary>The current pitch of the driver's eyes</summary>
		public double DriverPitch;
		/// <summary>Whether currently visible from the in-game camera location</summary>
		public bool CurrentlyVisible;
		/// <summary>Whether currently derailed</summary>
		public bool Derailed;
		/// <summary>Whether currently toppled over</summary>
		public bool Topples;
		/// <summary>The coupler between cars</summary>
		public Coupler Coupler;
		/// <summary>The breaker</summary>
		public Breaker Breaker;
		/// <summary>The windscreen</summary>
		public Windscreen Windscreen;
		/// <summary>The hold brake for this car</summary>
		public CarHoldBrake HoldBrake;
		/// <summary>The constant speed device for this car</summary>
		public CarConstSpeed ConstSpeed;
		/// <summary>The readhesion device for this car</summary>
		public AbstractReAdhesionDevice ReAdhesionDevice;
		public DriverSupervisionDevice DSD;
		/// <summary>The DriverSupervisionDevice for this car</summary>
		/// <summary>The position of the beacon reciever within the car</summary>
		public double BeaconReceiverPosition;
		/// <summary>The beacon reciever</summary>
		public TrackFollower BeaconReceiver;
		/// <summary>Stores the camera restriction mode for the interior view of this car</summary>
		public CameraRestrictionMode CameraRestrictionMode = CameraRestrictionMode.NotSpecified;
		/// <summary>The current camera restriction mode for this car</summary>
		public CameraRestriction CameraRestriction;
		/// <summary>Stores the camera interior camera alignment for this car</summary>
		public CameraAlignment InteriorCamera;
		/// <summary>Whether loading sway is enabled for this car</summary>
		public bool EnableLoadingSway = true;
		/// <summary>Whether this car has an interior view</summary>
		public bool HasInteriorView = false;
		/// <summary>Contains the generic sounds attached to the car</summary>
		public CarSounds Sounds;
		/// <summary>The cargo carried by the car</summary>
		public CargoBase Cargo;
		/// <summary>The car suspension</summary>
		public Suspension Suspension;
		/// <summary>The flange sounds</summary>
		public Flange Flange;
		/// <summary>The run sounds</summary>
		public RunSounds Run;

		private int trainCarIndex;

		public CarBase(TrainBase train, int index, double CoefficientOfFriction, double CoefficientOfRollingResistance, double AerodynamicDragCoefficient)
		{
			Specs = new CarPhysics();
			Brightness = new Brightness(this);
			baseTrain = train;
			trainCarIndex = index;
			CarSections = new CarSection[] { };
			FrontAxle = new Axle(TrainManagerBase.currentHost, train, this, CoefficientOfFriction, CoefficientOfRollingResistance, AerodynamicDragCoefficient);
			FrontAxle.Follower.TriggerType = index == 0 ? EventTriggerType.FrontCarFrontAxle : EventTriggerType.OtherCarFrontAxle;
			RearAxle = new Axle(TrainManagerBase.currentHost, train, this, CoefficientOfFriction, CoefficientOfRollingResistance, AerodynamicDragCoefficient);
			RearAxle.Follower.TriggerType = index == baseTrain.Cars.Length - 1 ? EventTriggerType.RearCarRearAxle : EventTriggerType.OtherCarRearAxle;
			BeaconReceiver = new TrackFollower(TrainManagerBase.currentHost, train);
			FrontBogie = new Bogie(this, false);
			RearBogie = new Bogie(this, true);
			Doors = new Door[2];
			Horns = new[]
			{
				new Horn(this),
				new Horn(this),
				new Horn(this)
			};
			Sounds = new CarSounds();
			CurrentCarSection = -1;
			ChangeCarSection(CarSectionType.NotVisible);
			FrontBogie.ChangeSection(-1);
			RearBogie.ChangeSection(-1);
			Cargo = new Passengers(this);
			Suspension = new Suspension(this);
			Flange = new Flange(this);
			Run = new RunSounds(this);
		}

		public CarBase(TrainBase train, int index)
		{
			baseTrain = train;
			trainCarIndex = index;
			CarSections = new CarSection[] { };
			FrontAxle = new Axle(TrainManagerBase.currentHost, train, this);
			RearAxle = new Axle(TrainManagerBase.currentHost, train, this);
			BeaconReceiver = new TrackFollower(TrainManagerBase.currentHost, train);
			FrontBogie = new Bogie(this, false);
			RearBogie = new Bogie(this, true);
			Doors = new Door[2];
			Horns = new[]
			{
				new Horn(this),
				new Horn(this),
				new Horn(this)
			};
			Brightness = new Brightness(this);
			Cargo = new Passengers(this);
			Specs = new CarPhysics();
			Suspension = new Suspension(this);
			Flange = new Flange(this);
			Run = new RunSounds(this);
			Sounds = new CarSounds();
		}

		/// <summary>Moves the car</summary>
		/// <param name="Delta">The delta to move</param>
		public void Move(double Delta)
		{
			if (baseTrain.State != TrainState.Disposed)
			{
				FrontAxle.Follower.UpdateRelative(Delta, true, true);
				FrontBogie.FrontAxle.Follower.UpdateRelative(Delta, true, true);
				FrontBogie.RearAxle.Follower.UpdateRelative(Delta, true, true);
				if (baseTrain.State != TrainState.Disposed)
				{
					RearAxle.Follower.UpdateRelative(Delta, true, true);
					RearBogie.FrontAxle.Follower.UpdateRelative(Delta, true, true);
					RearBogie.RearAxle.Follower.UpdateRelative(Delta, true, true);
					if (baseTrain.State != TrainState.Disposed)
					{
						BeaconReceiver.UpdateRelative(Delta, true, false);
					}
				}
			}
		}

		/// <summary>Call this method to update all track followers attached to the car</summary>
		/// <param name="NewTrackPosition">The track position change</param>
		/// <param name="UpdateWorldCoordinates">Whether to update the world co-ordinates</param>
		/// <param name="AddTrackInaccurary">Whether to add track innaccuarcy</param>
		public void UpdateTrackFollowers(double NewTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccurary)
		{
			//Car axles
			FrontAxle.Follower.UpdateRelative(NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
			RearAxle.Follower.UpdateRelative(NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
			//Front bogie axles
			FrontBogie.FrontAxle.Follower.UpdateRelative(NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
			FrontBogie.RearAxle.Follower.UpdateRelative(NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
			//Rear bogie axles

			RearBogie.FrontAxle.Follower.UpdateRelative(NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
			RearBogie.RearAxle.Follower.UpdateRelative(NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
		}

		/// <summary>Initializes the car</summary>
		public void Initialize()
		{
			for (int i = 0; i < CarSections.Length; i++)
			{
				CarSections[i].Initialize(false);
			}

			for (int i = 0; i < FrontBogie.CarSections.Length; i++)
			{
				FrontBogie.CarSections[i].Initialize(false);
			}

			for (int i = 0; i < RearBogie.CarSections.Length; i++)
			{
				RearBogie.CarSections[i].Initialize(false);
			}

			Brightness.PreviousBrightness = 1.0f;
			Brightness.NextBrightness = 1.0f;
		}

		/// <summary>Synchronizes the car after a period of infrequent updates</summary>
		public void Syncronize()
		{
			double s = 0.5 * (FrontAxle.Follower.TrackPosition + RearAxle.Follower.TrackPosition);
			double d = 0.5 * (FrontAxle.Follower.TrackPosition - RearAxle.Follower.TrackPosition);
			FrontAxle.Follower.UpdateAbsolute(s + d, false, false);
			RearAxle.Follower.UpdateAbsolute(s - d, false, false);
			double b = FrontAxle.Follower.TrackPosition - FrontAxle.Position + BeaconReceiverPosition;
			BeaconReceiver.UpdateAbsolute(b, false, false);
		}

		public override void CreateWorldCoordinates(Vector3 Car, out Vector3 Position, out Vector3 Direction)
		{
			Direction = FrontAxle.Follower.WorldPosition - RearAxle.Follower.WorldPosition;
			double t = Direction.NormSquared();
			if (t != 0.0)
			{
				t = 1.0 / Math.Sqrt(t);
				Direction *= t;
				double sx = Direction.Z * Up.Y - Direction.Y * Up.Z;
				double sy = Direction.X * Up.Z - Direction.Z * Up.X;
				double sz = Direction.Y * Up.X - Direction.X * Up.Y;
				double rx = 0.5 * (FrontAxle.Follower.WorldPosition.X + RearAxle.Follower.WorldPosition.X);
				double ry = 0.5 * (FrontAxle.Follower.WorldPosition.Y + RearAxle.Follower.WorldPosition.Y);
				double rz = 0.5 * (FrontAxle.Follower.WorldPosition.Z + RearAxle.Follower.WorldPosition.Z);
				Position.X = rx + sx * Car.X + Up.X * Car.Y + Direction.X * Car.Z;
				Position.Y = ry + sy * Car.X + Up.Y * Car.Y + Direction.Y * Car.Z;
				Position.Z = rz + sz * Car.X + Up.Z * Car.Y + Direction.Z * Car.Z;
			}
			else
			{
				Position = FrontAxle.Follower.WorldPosition;
				Direction = Vector3.Down;
			}
		}

		public override double TrackPosition => FrontAxle.Follower.TrackPosition;

		/// <summary>Backing property for the index of the car within the train</summary>
		public override int Index
		{
			get => trainCarIndex;
			set => trainCarIndex = value;
		}

		public override void Reverse(bool flipInterior = false)
		{
			// reverse axle positions
			double temp = FrontAxle.Position;
			FrontAxle.Position = -RearAxle.Position;
			RearAxle.Position = -temp;
			if (flipInterior)
			{
				if (CarSections != null && CarSections.Length > 0)
				{
					for (int i = 0; i < CarSections.Length; i++)
					{
						if (CarSections[i].Type == ObjectType.Overlay)
						{
							for (int j = 0; j < CarSections[i].Groups.Length; j++)
							{
								CarSections[i].Groups[j].Reverse(Driver, TrainManagerBase.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable); // restriction not available must equal 3D cab
							}
						}
						else
						{
							foreach (AnimatedObject animatedObject in CarSections[i].Groups[0].Elements)
							{
								animatedObject.Reverse();
							}	
						}
						
					}
				}
				Driver = new Vector3(-Driver.X, Driver.Y, -Driver.Z);
				CameraRestriction.Reverse();
			}
			else
			{
				int idxToReverse = HasInteriorView ? 1 : 0;
				if (CarSections != null && CarSections.Length > 0)
				{
					foreach (AnimatedObject animatedObject in CarSections[idxToReverse].Groups[0].Elements)
					{
						animatedObject.Reverse();
					}
				}	
			}
			

			Bogie b = RearBogie;
			RearBogie = FrontBogie;
			FrontBogie = b;
			FrontBogie.Reverse();
			RearBogie.Reverse();
			FrontBogie.FrontAxle.Follower.UpdateAbsolute(FrontAxle.Position + FrontBogie.FrontAxle.Position, true, false);
			FrontBogie.RearAxle.Follower.UpdateAbsolute(FrontAxle.Position + FrontBogie.RearAxle.Position, true, false);
			RearBogie.FrontAxle.Follower.UpdateAbsolute(RearAxle.Position + RearBogie.FrontAxle.Position, true, false);
			RearBogie.RearAxle.Follower.UpdateAbsolute(RearAxle.Position + RearBogie.RearAxle.Position, true, false);
		}

		public override void OpenDoors(bool Left, bool Right)
		{
			bool sl = false, sr = false;
			if (Left & !Doors[0].AnticipatedOpen & (baseTrain.SafetySystems.DoorInterlockState == DoorInterlockStates.Left | baseTrain.SafetySystems.DoorInterlockState == DoorInterlockStates.Unlocked))
			{
				Doors[0].AnticipatedOpen = true;
				sl = true;
			}

			if (Right & !Doors[1].AnticipatedOpen & (baseTrain.SafetySystems.DoorInterlockState == DoorInterlockStates.Right | baseTrain.SafetySystems.DoorInterlockState == DoorInterlockStates.Unlocked))
			{
				Doors[1].AnticipatedOpen = true;
				sr = true;
			}

			if (sl)
			{
				Doors[0].OpenSound.Play(Specs.DoorOpenPitch, 1.0, this, false);
				for (int i = 0; i < Doors.Length; i++)
				{
					if (Doors[i].Direction == -1)
					{
						Doors[i].DoorLockDuration = 0.0;
					}
				}
			}

			if (sr)
			{
				Doors[1].OpenSound.Play(Specs.DoorOpenPitch, 1.0, this, false);
				for (int i = 0; i < Doors.Length; i++)
				{
					if (Doors[i].Direction == 1)
					{
						Doors[i].DoorLockDuration = 0.0;
					}
				}
			}

			for (int i = 0; i < Doors.Length; i++)
			{
				if (Doors[i].AnticipatedOpen)
				{
					Doors[i].NextReopenTime = 0.0;
					Doors[i].ReopenCounter++;
				}
			}
		}

		public override void Uncouple(bool Front, bool Rear)
		{
			lock (baseTrain.updateLock)
			{
				if (!Front && !Rear)
				{
					return;
				}

				// Create new train
				TrainBase newTrain = new TrainBase(TrainState.Available);
				UncouplingBehaviour uncouplingBehaviour = UncouplingBehaviour.Emergency;
				newTrain.Handles.Power = new PowerHandle(0, 0, new double[0], new double[0], newTrain);
				newTrain.Handles.Brake = new BrakeHandle(0, 0, newTrain.Handles.EmergencyBrake, new double[0], new double[0], newTrain);
				newTrain.Handles.HoldBrake = new HoldBrakeHandle(newTrain);
				if (Front)
				{
					int totalPreceedingCars = trainCarIndex;
					newTrain.Cars = new CarBase[trainCarIndex];
					for (int i = 0; i < totalPreceedingCars; i++)
					{
						newTrain.Cars[i] = baseTrain.Cars[i];
						newTrain.Cars[i].baseTrain = newTrain;
					}

					for (int i = totalPreceedingCars; i < baseTrain.Cars.Length; i++)
					{
						baseTrain.Cars[i - totalPreceedingCars] = baseTrain.Cars[i];
						baseTrain.Cars[i].Index = i - totalPreceedingCars;
					}

					Array.Resize(ref baseTrain.Cars, baseTrain.Cars.Length - totalPreceedingCars);
					baseTrain.Cars[baseTrain.Cars.Length - 1].Coupler.UncoupleSound.Play(baseTrain.Cars[baseTrain.Cars.Length - 1], false);
					uncouplingBehaviour = baseTrain.Cars[baseTrain.Cars.Length - 1].Coupler.UncouplingBehaviour;
					TrainManagerBase.currentHost.AddTrain(baseTrain, newTrain, false);

					if (baseTrain.DriverCar - totalPreceedingCars >= 0)
					{
						baseTrain.DriverCar -= totalPreceedingCars;
					}
				}

				if (Rear)
				{
					int totalFollowingCars = baseTrain.Cars.Length - (Index + 1);
					if (totalFollowingCars > 0)
					{
						newTrain.Cars = new CarBase[totalFollowingCars];
						// Move following cars to new train
						for (int i = 0; i < totalFollowingCars; i++)
						{
							newTrain.Cars[i] = baseTrain.Cars[Index + i + 1];
							newTrain.Cars[i].baseTrain = newTrain;
							newTrain.Cars[i].Index = i;
						}

						Array.Resize(ref baseTrain.Cars, baseTrain.Cars.Length - totalFollowingCars);
						baseTrain.Cars[baseTrain.Cars.Length - 1].Coupler.connectedCar = baseTrain.Cars[baseTrain.Cars.Length - 1];
					}
					else
					{
						return;
					}

					Coupler.UncoupleSound.Play(this, false);
					uncouplingBehaviour = Coupler.UncouplingBehaviour;
					TrainManagerBase.currentHost.AddTrain(baseTrain, newTrain, true);
				}

				for (int i = 0; i < newTrain.Cars.Length; i++)
				{
					/*
					 * Make visible if not part of player train
					 * Otherwise uncoupling from cab then changing to exterior, they will still be hidden
					 *
					 * Need to do this after everything has been done in case objects refer to other bits
					 */
					newTrain.Cars[i].ChangeCarSection(CarSectionType.Exterior);
					newTrain.Cars[i].FrontBogie.ChangeSection(0);
					newTrain.Cars[i].RearBogie.ChangeSection(0);
					newTrain.Cars[i].Coupler.ChangeSection(0);
				}

				if (baseTrain.DriverCar >= baseTrain.Cars.Length)
				{
					/*
					 * The driver car is no longer in the train
					 *
					 * Look for a car with an interior view to substitute
					 * If not found, this will stop at Car 0
					 */

					for (int i = baseTrain.Cars.Length; i > 0; i--)
					{
						baseTrain.DriverCar = i - 1;
						if (!baseTrain.Cars[i - 1].HasInteriorView)
						{
							/*
							 * Set the eye position to something vaguely sensible, rather than leaving it on the rails
							 * Whilst there will be no cab, at least it's a bit more usable like this
							 */
							baseTrain.Cars[i - 1].InteriorCamera = new CameraAlignment()
							{
								Position = new Vector3(0, 2, 0.5 * Length)
							};
						}
						else
						{
							break;
						}
					}
				}

				if (Front)
				{
					// Uncoupling the front will always make the car our first
					baseTrain.CameraCar = 0;
				}
				else
				{
					if (baseTrain.CameraCar >= baseTrain.Cars.Length)
					{
						baseTrain.CameraCar = baseTrain.DriverCar;
					}
				}

				switch (uncouplingBehaviour)
				{
					case UncouplingBehaviour.Emergency:
						baseTrain.Handles.EmergencyBrake.Apply();
						newTrain.Handles.EmergencyBrake.Apply();
						break;
					case UncouplingBehaviour.EmergencyUncoupledConsist:
						newTrain.Handles.EmergencyBrake.Apply();
						break;
					case UncouplingBehaviour.EmergencyPlayer:
						baseTrain.Handles.EmergencyBrake.Apply();
						break;
					case UncouplingBehaviour.Released:
						baseTrain.Handles.Brake.ApplyState(0, false);
						baseTrain.Handles.EmergencyBrake.Release();
						newTrain.Handles.Brake.ApplyState(0, false);
						newTrain.Handles.EmergencyBrake.Release();
						break;
					case UncouplingBehaviour.ReleasedUncoupledConsist:
						newTrain.Handles.Brake.ApplyState(0, false);
						newTrain.Handles.EmergencyBrake.Release();
						break;

				}
			}
		}

		/// <summary>Returns the combination of door states what encountered at the specified car in a train.</summary>
		/// <param name="Left">Whether to include left doors.</param>
		/// <param name="Right">Whether to include right doors.</param>
		/// <returns>A bit mask combining encountered door states.</returns>
		public TrainDoorState GetDoorsState(bool Left, bool Right)
		{
			bool opened = false, closed = false, mixed = false;
			for (int i = 0; i < Doors.Length; i++)
			{
				if (Left & Doors[i].Direction == -1 | Right & Doors[i].Direction == 1)
				{
					if (Doors[i].State == 0.0)
					{
						closed = true;
					}
					else if (Doors[i].State == 1.0)
					{
						opened = true;
					}
					else
					{
						mixed = true;
					}
				}
			}

			TrainDoorState Result = TrainDoorState.None;
			if (opened) Result |= TrainDoorState.Opened;
			if (closed) Result |= TrainDoorState.Closed;
			if (mixed) Result |= TrainDoorState.Mixed;
			if (opened & !closed & !mixed) Result |= TrainDoorState.AllOpened;
			if (!opened & closed & !mixed) Result |= TrainDoorState.AllClosed;
			if (!opened & !closed & mixed) Result |= TrainDoorState.AllMixed;
			return Result;
		}
		
		/// <summary>Loads Car Sections (Exterior objects etc.) for this car</summary>
		/// <param name="currentObject">The object to add to the car sections array</param>
		/// <param name="visibleFromInterior">Wether this is visible from the interior of other cars</param>
		public void LoadCarSections(UnifiedObject currentObject, bool visibleFromInterior)
		{
			int j = CarSections.Length;
			Array.Resize(ref CarSections, j + 1);
			CarSections[j] = new CarSection(TrainManagerBase.currentHost, ObjectType.Dynamic, visibleFromInterior, currentObject);
		}

		/// <summary>Changes the currently visible car section</summary>
		/// <param name="newCarSection">The type of new car section to display</param>
		/// <param name="trainVisible">Whether the train is visible</param>
		/// <param name="forceChange">Whether to force a show / rehide</param>
		public void ChangeCarSection(CarSectionType newCarSection, bool trainVisible = false, bool forceChange = false)
		{
			if(CurrentCarSection == (int)newCarSection && !forceChange)
			{
				return;
			}
			if (trainVisible)
			{
				if (CurrentCarSection != -1 && CarSections[CurrentCarSection].VisibleFromInterior)
				{
					return;
				}
			}

			for (int i = 0; i < CarSections.Length; i++)
			{
				for (int j = 0; j < CarSections[i].Groups.Length; j++)
				{
					for (int k = 0; k < CarSections[i].Groups[j].Elements.Length; k++)
					{
						TrainManagerBase.currentHost.HideObject(CarSections[i].Groups[j].Elements[k].internalObject);
					}
				}
			}

			switch (newCarSection)
			{
				case CarSectionType.NotVisible:
					this.CurrentCarSection = -1;
					break;
				case CarSectionType.Interior:
					if (this.HasInteriorView && this.CarSections.Length > 0)
					{
						this.CurrentCarSection = 0;
						this.CarSections[0].Initialize(false);
						CarSections[0].Show();
						break;
					}

					this.CurrentCarSection = -1;
					break;
				case CarSectionType.Exterior:
					if (this.HasInteriorView && this.CarSections.Length > 1)
					{
						this.CurrentCarSection = 1;
						this.CarSections[1].Initialize(false);
						CarSections[1].Show();
						break;
					}
					else if (!this.HasInteriorView && this.CarSections.Length > 0)
					{
						this.CurrentCarSection = 0;
						this.CarSections[0].Initialize(false);
						CarSections[0].Show();
						break;
					}

					this.CurrentCarSection = -1;
					break;
			}

			//When changing car section, do not apply damping
			//This stops objects from spinning if the last position before they were hidden is different
			UpdateObjects(0.0, true, false);
		}

		/// <summary>Updates the currently displayed objects for this car</summary>
		/// <param name="TimeElapsed">The time elapsed</param>
		/// <param name="ForceUpdate">Whether this is a forced update</param>
		/// <param name="EnableDamping">Whether damping is applied during this update (Skipped on transitions between camera views etc.)</param>
		public void UpdateObjects(double TimeElapsed, bool ForceUpdate, bool EnableDamping)
		{
			// calculate positions and directions for section element update

			Vector3 d = new Vector3(FrontAxle.Follower.WorldPosition - RearAxle.Follower.WorldPosition);
			Vector3 s;
			double t = d.NormSquared();
			if (t != 0.0)
			{
				t = 1.0 / Math.Sqrt(t);
				d *= t;
				s.X = d.Z * Up.Y - d.Y * Up.Z;
				s.Y = d.X * Up.Z - d.Z * Up.X;
				s.Z = d.Y * Up.X - d.X * Up.Y;
			}
			else
			{
				s = Vector3.Right;
			}

			Vector3 p = new Vector3(0.5 * (FrontAxle.Follower.WorldPosition + RearAxle.Follower.WorldPosition));
			p -= d * (0.5 * (FrontAxle.Position + RearAxle.Position));
			// determine visibility
			Vector3 cd = new Vector3(p - TrainManagerBase.Renderer.Camera.AbsolutePosition);
			double dist = cd.NormSquared();
			double bid = TrainManagerBase.Renderer.Camera.ViewingDistance + Length;
			CurrentlyVisible = dist < bid * bid;
			// Updates the brightness value
			byte dnb = (byte)Brightness.CurrentBrightness(TrainManagerBase.Renderer.Lighting.DynamicCabBrightness, 0.0);
			// update current section
			int cs = CurrentCarSection;
			if (cs >= 0 && cs < CarSections.Length)
			{
				if (CarSections[cs].Groups.Length > 0)
				{
					for (int i = 0; i < CarSections[cs].Groups[0].Elements.Length; i++)
					{
						UpdateCarSectionElement(cs, 0, i, p, d, s, CurrentlyVisible, TimeElapsed, ForceUpdate, EnableDamping);

						// brightness change
						if (CarSections[cs].Groups[0].Elements[i].internalObject != null)
						{
							CarSections[cs].Groups[0].Elements[i].internalObject.DaytimeNighttimeBlend = dnb;
						}
					}
				}

				int add = CarSections[cs].CurrentAdditionalGroup + 1;
				if (add < CarSections[cs].Groups.Length)
				{
					for (int i = 0; i < CarSections[cs].Groups[add].Elements.Length; i++)
					{
						UpdateCarSectionElement(cs, add, i, p, d, s, CurrentlyVisible, TimeElapsed, ForceUpdate, EnableDamping);

						// brightness change
						if (CarSections[cs].Groups[add].Elements[i].internalObject != null)
						{
							CarSections[cs].Groups[add].Elements[i].internalObject.DaytimeNighttimeBlend = dnb;
						}
					}

					if (CarSections[cs].Groups[add].TouchElements != null)
					{
						for (int i = 0; i < CarSections[cs].Groups[add].TouchElements.Length; i++)
						{
							UpdateCarSectionTouchElement(cs, add, i, p, d, s, false, TimeElapsed, ForceUpdate, EnableDamping);
						}
					}
				}
			}
			//Update camera restriction

			CameraRestriction.AbsoluteBottomLeft = new Vector3(CameraRestriction.BottomLeft);
			CameraRestriction.AbsoluteBottomLeft += Driver;
			CameraRestriction.AbsoluteBottomLeft.Rotate(new Transformation(d, Up, s));
			CameraRestriction.AbsoluteBottomLeft.Translate(p);

			CameraRestriction.AbsoluteTopRight = new Vector3(CameraRestriction.TopRight);
			CameraRestriction.AbsoluteTopRight += Driver;
			CameraRestriction.AbsoluteTopRight.Rotate(new Transformation(d, Up, s));
			CameraRestriction.AbsoluteTopRight.Translate(p);
		}

		/// <summary>Updates the given car section element</summary>
		/// <param name="SectionIndex">The car section</param>
		/// <param name="GroupIndex">The group within the car section</param>
		/// <param name="ElementIndex">The element within the group</param>
		/// <param name="Position"></param>
		/// <param name="Direction"></param>
		/// <param name="Side"></param>
		/// <param name="Show"></param>
		/// <param name="TimeElapsed"></param>
		/// <param name="ForceUpdate"></param>
		/// <param name="EnableDamping"></param>
		private void UpdateCarSectionElement(int SectionIndex, int GroupIndex, int ElementIndex, Vector3 Position, Vector3 Direction, Vector3 Side, bool Show, double TimeElapsed, bool ForceUpdate, bool EnableDamping)
		{
			Vector3 p;
			if (CarSections[SectionIndex].Type == ObjectType.Overlay & (TrainManagerBase.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable && TrainManagerBase.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.Restricted3D))
			{
				p = new Vector3(Driver.X, Driver.Y, Driver.Z);
			}
			else
			{
				p = Position;
			}

			double timeDelta;
			bool updatefunctions;
			if (CarSections[SectionIndex].Groups[GroupIndex].Elements[ElementIndex].RefreshRate != 0.0)
			{
				if (CarSections[SectionIndex].Groups[GroupIndex].Elements[ElementIndex].SecondsSinceLastUpdate >= CarSections[SectionIndex].Groups[GroupIndex].Elements[ElementIndex].RefreshRate)
				{
					timeDelta = CarSections[SectionIndex].Groups[GroupIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
					CarSections[SectionIndex].Groups[GroupIndex].Elements[ElementIndex].SecondsSinceLastUpdate = TimeElapsed;
					updatefunctions = true;
				}
				else
				{
					timeDelta = TimeElapsed;
					CarSections[SectionIndex].Groups[GroupIndex].Elements[ElementIndex].SecondsSinceLastUpdate += TimeElapsed;
					updatefunctions = false;
				}
			}
			else
			{
				timeDelta = CarSections[SectionIndex].Groups[GroupIndex].Elements[ElementIndex].SecondsSinceLastUpdate;
				CarSections[SectionIndex].Groups[GroupIndex].Elements[ElementIndex].SecondsSinceLastUpdate = TimeElapsed;
				updatefunctions = true;
			}

			if (ForceUpdate)
			{
				updatefunctions = true;
			}

			CarSections[SectionIndex].Groups[GroupIndex].Elements[ElementIndex].Update(baseTrain, Index, FrontAxle.Follower.TrackPosition - FrontAxle.Position, p, Direction, Up, Side, updatefunctions, Show, timeDelta, EnableDamping, false, CarSections[SectionIndex].Type == ObjectType.Overlay ? TrainManagerBase.Renderer.Camera : null);
			if (!TrainManagerBase.Renderer.ForceLegacyOpenGL && CarSections[SectionIndex].Groups[GroupIndex].Elements[ElementIndex].UpdateVAO)
			{
				VAOExtensions.CreateVAO(ref CarSections[SectionIndex].Groups[GroupIndex].Elements[ElementIndex].internalObject.Prototype.Mesh, true, TrainManagerBase.Renderer.DefaultShader.VertexLayout, TrainManagerBase.Renderer);
			}
		}

		private void UpdateCarSectionTouchElement(int SectionIndex, int GroupIndex, int ElementIndex, Vector3 Position, Vector3 Direction, Vector3 Side, bool Show, double TimeElapsed, bool ForceUpdate, bool EnableDamping)
		{
			Vector3 p;
			if (CarSections[SectionIndex].Type == ObjectType.Overlay & (TrainManagerBase.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable && TrainManagerBase.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.Restricted3D))
			{
				p = new Vector3(Driver.X, Driver.Y, Driver.Z);
			}
			else
			{
				p = Position;
			}

			double timeDelta;
			bool updatefunctions;
			if (CarSections[SectionIndex].Groups[GroupIndex].TouchElements[ElementIndex].Element.RefreshRate != 0.0)
			{
				if (CarSections[SectionIndex].Groups[GroupIndex].TouchElements[ElementIndex].Element.SecondsSinceLastUpdate >= CarSections[SectionIndex].Groups[GroupIndex].TouchElements[ElementIndex].Element.RefreshRate)
				{
					timeDelta = CarSections[SectionIndex].Groups[GroupIndex].TouchElements[ElementIndex].Element.SecondsSinceLastUpdate;
					CarSections[SectionIndex].Groups[GroupIndex].TouchElements[ElementIndex].Element.SecondsSinceLastUpdate = TimeElapsed;
					updatefunctions = true;
				}
				else
				{
					timeDelta = TimeElapsed;
					CarSections[SectionIndex].Groups[GroupIndex].TouchElements[ElementIndex].Element.SecondsSinceLastUpdate += TimeElapsed;
					updatefunctions = false;
				}
			}
			else
			{
				timeDelta = CarSections[SectionIndex].Groups[GroupIndex].TouchElements[ElementIndex].Element.SecondsSinceLastUpdate;
				CarSections[SectionIndex].Groups[GroupIndex].TouchElements[ElementIndex].Element.SecondsSinceLastUpdate = TimeElapsed;
				updatefunctions = true;
			}

			if (ForceUpdate)
			{
				updatefunctions = true;
			}

			CarSections[SectionIndex].Groups[GroupIndex].TouchElements[ElementIndex].Element.Update(baseTrain, Index, FrontAxle.Follower.TrackPosition - FrontAxle.Position, p, Direction, Up, Side, updatefunctions, Show, timeDelta, EnableDamping, true, CarSections[SectionIndex].Type == ObjectType.Overlay ? TrainManagerBase.Renderer.Camera : null);
			if (!TrainManagerBase.Renderer.ForceLegacyOpenGL && CarSections[SectionIndex].Groups[GroupIndex].TouchElements[ElementIndex].Element.UpdateVAO)
			{
				VAOExtensions.CreateVAO(ref CarSections[SectionIndex].Groups[GroupIndex].TouchElements[ElementIndex].Element.internalObject.Prototype.Mesh, true, TrainManagerBase.Renderer.DefaultShader.VertexLayout, TrainManagerBase.Renderer);
			}
		}

		public void UpdateTopplingCantAndSpring(double TimeElapsed)
		{
			// get direction, up and side vectors
			Vector3 d = new Vector3(FrontAxle.Follower.WorldPosition - RearAxle.Follower.WorldPosition);
			Vector3 s;
			{
				double t = 1.0 / d.Norm();
				d *= t;
				t = 1.0 / Math.Sqrt(d.X * d.X + d.Z * d.Z);
				double ex = d.X * t;
				double ez = d.Z * t;
				s = new Vector3(ez, 0.0, -ex);
				Up = Vector3.Cross(d, s);
			}

			double r = 0.0, rs = 0.0;
			if (FrontAxle.Follower.CurveRadius != 0.0 & RearAxle.Follower.CurveRadius != 0.0)
			{
				r = Math.Sqrt(Math.Abs(FrontAxle.Follower.CurveRadius * RearAxle.Follower.CurveRadius));
				rs = Math.Sign(FrontAxle.Follower.CurveRadius + RearAxle.Follower.CurveRadius);
			}
			else if (FrontAxle.Follower.CurveRadius != 0.0)
			{
				r = Math.Abs(FrontAxle.Follower.CurveRadius);
				rs = Math.Sign(FrontAxle.Follower.CurveRadius);
			}
			else if (RearAxle.Follower.CurveRadius != 0.0)
			{
				r = Math.Abs(RearAxle.Follower.CurveRadius);
				rs = Math.Sign(RearAxle.Follower.CurveRadius);
			}

			// roll due to shaking
			{

				double a0 = Specs.RollDueToShakingAngle;
				double a1 = 0.0;
				if (Specs.RollShakeDirection != 0.0)
				{
					const double c0 = 0.03;
					const double c1 = 0.15;
					a1 = c1 * Math.Atan(c0 * Specs.RollShakeDirection);
					double dr = 0.5 + Specs.RollShakeDirection * Specs.RollShakeDirection;
					if (Specs.RollShakeDirection < 0.0)
					{
						Specs.RollShakeDirection += dr * TimeElapsed;
						if (Specs.RollShakeDirection > 0.0) Specs.RollShakeDirection = 0.0;
					}
					else
					{
						Specs.RollShakeDirection -= dr * TimeElapsed;
						if (Specs.RollShakeDirection < 0.0) Specs.RollShakeDirection = 0.0;
					}
				}

				double SpringAcceleration;
				if (!Derailed)
				{
					SpringAcceleration = 15.0 * Math.Abs(a1 - a0);
				}
				else
				{
					SpringAcceleration = 1.5 * Math.Abs(a1 - a0);
				}

				double SpringDeceleration = 0.25 * SpringAcceleration;
				Specs.RollDueToShakingAngularSpeed += Math.Sign(a1 - a0) * SpringAcceleration * TimeElapsed;
				double x = Math.Sign(Specs.RollDueToShakingAngularSpeed) * SpringDeceleration * TimeElapsed;
				if (Math.Abs(x) < Math.Abs(Specs.RollDueToShakingAngularSpeed))
				{
					Specs.RollDueToShakingAngularSpeed -= x;
				}
				else
				{
					Specs.RollDueToShakingAngularSpeed = 0.0;
				}

				a0 += Specs.RollDueToShakingAngularSpeed * TimeElapsed;
				Specs.RollDueToShakingAngle = a0;
			}
			// roll due to cant (incorporates shaking)
			{
				double cantAngle = Math.Atan(Math.Tan(0.5 * (Math.Atan(FrontAxle.Follower.CurveCant) + Math.Atan(RearAxle.Follower.CurveCant))) / TrainManagerBase.currentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge);
				Specs.RollDueToCantAngle = cantAngle + Specs.RollDueToShakingAngle;
			}
			// pitch due to acceleration
			{
				for (int i = 0; i < 3; i++)
				{
					double a, v, j;
					switch (i)
					{
						case 0:
							a = Specs.Acceleration;
							v = Specs.PitchDueToAccelerationFastValue;
							j = 1.8;
							break;
						case 1:
							a = Specs.PitchDueToAccelerationFastValue;
							v = Specs.PitchDueToAccelerationMediumValue;
							j = 1.2;
							break;
						default:
							a = Specs.PitchDueToAccelerationFastValue;
							v = Specs.PitchDueToAccelerationSlowValue;
							j = 1.0;
							break;
					}

					double da = a - v;
					if (da < 0.0)
					{
						v -= j * TimeElapsed;
						if (v < a) v = a;
					}
					else
					{
						v += j * TimeElapsed;
						if (v > a) v = a;
					}

					switch (i)
					{
						case 0:
							Specs.PitchDueToAccelerationFastValue = v;
							break;
						case 1:
							Specs.PitchDueToAccelerationMediumValue = v;
							break;
						default:
							Specs.PitchDueToAccelerationSlowValue = v;
							break;
					}
				}

				{
					double da = Specs.PitchDueToAccelerationSlowValue - Specs.PitchDueToAccelerationFastValue;
					Specs.PitchDueToAccelerationTargetAngle = 0.03 * Math.Atan(da);
				}
				{
					double a = 3.0 * Math.Sign(Specs.PitchDueToAccelerationTargetAngle - Specs.PitchDueToAccelerationAngle);
					Specs.PitchDueToAccelerationAngularSpeed += a * TimeElapsed;
					double ds = Math.Abs(Specs.PitchDueToAccelerationTargetAngle - Specs.PitchDueToAccelerationAngle);
					if (Math.Abs(Specs.PitchDueToAccelerationAngularSpeed) > ds)
					{
						Specs.PitchDueToAccelerationAngularSpeed = ds * Math.Sign(Specs.PitchDueToAccelerationAngularSpeed);
					}

					Specs.PitchDueToAccelerationAngle += Specs.PitchDueToAccelerationAngularSpeed * TimeElapsed;
				}
			}
			// derailment
			if (TrainManagerBase.Derailments & !Derailed)
			{
				double a = Specs.RollDueToTopplingAngle + Specs.RollDueToCantAngle;
				double sa = Math.Sign(a);
				if (a * sa > Specs.CriticalTopplingAngle)
				{
					baseTrain.Derail(Index, TimeElapsed);
				}
			}

			// toppling roll
			if (TrainManagerBase.Toppling | Derailed)
			{
				double a = Specs.RollDueToTopplingAngle;
				double ab = Specs.RollDueToTopplingAngle + Specs.RollDueToCantAngle;
				double h = Specs.CenterOfGravityHeight;
				double sr = Math.Abs(CurrentSpeed);
				double rmax = 2.0 * h * sr * sr / (TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity * TrainManagerBase.currentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge);
				double ta;
				Topples = false;
				if (Derailed)
				{
					double sab = Math.Sign(ab);
					ta = 0.5 * Math.PI * (sab == 0.0 ? TrainManagerBase.RandomNumberGenerator.NextDouble() < 0.5 ? -1.0 : 1.0 : sab);
				}
				else
				{
					if (r != 0.0)
					{
						if (r < rmax)
						{
							double s0 = Math.Sqrt(r * TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity * TrainManagerBase.currentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge / (2.0 * h));
							const double fac = 0.25; // arbitrary coefficient
							ta = -fac * (sr - s0) * rs;
							Topples = true;
							//FIXME: DEBUG MESSAGE
							//baseTrain.Topple(Index, TimeElapsed);
						}
						else
						{
							ta = 0.0;
						}
					}
					else
					{
						ta = 0.0;
					}
				}

				double td = 1.0;
				if (Derailed)
				{
					td = Math.Abs(ab);
					if (td < 0.1) td = 0.1;
				}

				if (a > ta)
				{
					double da = a - ta;
					if (td > da) td = da;
					a -= td * TimeElapsed;
				}
				else if (a < ta)
				{
					double da = ta - a;
					if (td > da) td = da;
					a += td * TimeElapsed;
				}

				Specs.RollDueToTopplingAngle = a;
			}
			else
			{
				Specs.RollDueToTopplingAngle = 0.0;
			}

			// apply position due to cant/toppling
			{
				double a = Specs.RollDueToTopplingAngle + Specs.RollDueToCantAngle;
				double x = Math.Sign(a) * 0.5 * TrainManagerBase.currentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge * (1.0 - Math.Cos(a));
				double y = Math.Abs(0.5 * TrainManagerBase.currentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge * Math.Sin(a));
				Vector3 cc = new Vector3(s.X * x + Up.X * y, s.Y * x + Up.Y * y, s.Z * x + Up.Z * y);
				FrontAxle.Follower.WorldPosition += cc;
				RearAxle.Follower.WorldPosition += cc;
			}
			// apply rolling
			{
				s.Rotate(d, -Specs.RollDueToTopplingAngle - Specs.RollDueToCantAngle);
				Up.Rotate(d, -Specs.RollDueToTopplingAngle - Specs.RollDueToCantAngle);
			}
			// apply pitching
			if (CurrentCarSection >= 0 && CarSections[CurrentCarSection].Type == ObjectType.Overlay)
			{
				d.Rotate(s, Specs.PitchDueToAccelerationAngle);
				Up.Rotate(s, Specs.PitchDueToAccelerationAngle);
				Vector3 cc = new Vector3(0.5 * (FrontAxle.Follower.WorldPosition + RearAxle.Follower.WorldPosition));
				FrontAxle.Follower.WorldPosition -= cc;
				RearAxle.Follower.WorldPosition -= cc;
				FrontAxle.Follower.WorldPosition.Rotate(s, Specs.PitchDueToAccelerationAngle);
				RearAxle.Follower.WorldPosition.Rotate(s, Specs.PitchDueToAccelerationAngle);
				FrontAxle.Follower.WorldPosition += cc;
				RearAxle.Follower.WorldPosition += cc;
			}

			Suspension.Update(TimeElapsed);
			Flange.Update(TimeElapsed);
		}

		/// <summary>Updates the position of the camera relative to this car</summary>
		public void UpdateCamera()
		{
			Vector3 direction = new Vector3(FrontAxle.Follower.WorldPosition - RearAxle.Follower.WorldPosition);
			direction *= 1.0 / direction.Norm();
			double sx = direction.Z * Up.Y - direction.Y * Up.Z;
			double sy = direction.X * Up.Z - direction.Z * Up.X;
			double sz = direction.Y * Up.X - direction.X * Up.Y;
			double rx = 0.5 * (FrontAxle.Follower.WorldPosition.X + RearAxle.Follower.WorldPosition.X);
			double ry = 0.5 * (FrontAxle.Follower.WorldPosition.Y + RearAxle.Follower.WorldPosition.Y);
			double rz = 0.5 * (FrontAxle.Follower.WorldPosition.Z + RearAxle.Follower.WorldPosition.Z);
			Vector3 cameraPosition;
			Vector3 driverPosition = this.HasInteriorView ? Driver : this.baseTrain.Cars[this.baseTrain.DriverCar].Driver;
			cameraPosition.X = rx + sx * driverPosition.X + Up.X * driverPosition.Y + direction.X * driverPosition.Z;
			cameraPosition.Y = ry + sy * driverPosition.X + Up.Y * driverPosition.Y + direction.Y * driverPosition.Z;
			cameraPosition.Z = rz + sz * driverPosition.X + Up.Z * driverPosition.Y + direction.Z * driverPosition.Z;

			TrainManagerBase.Renderer.CameraTrackFollower.WorldPosition = cameraPosition;
			TrainManagerBase.Renderer.CameraTrackFollower.WorldDirection = direction;
			TrainManagerBase.Renderer.CameraTrackFollower.WorldUp = new Vector3(Up);
			TrainManagerBase.Renderer.CameraTrackFollower.WorldSide = new Vector3(sx, sy, sz);
			double f = (Driver.Z - RearAxle.Position) / (FrontAxle.Position - RearAxle.Position);
			double tp = (1.0 - f) * RearAxle.Follower.TrackPosition + f * FrontAxle.Follower.TrackPosition;
			TrainManagerBase.Renderer.CameraTrackFollower.UpdateAbsolute(tp, false, false);
		}

		public void UpdateSpeed(double TimeElapsed, double DecelerationDueToMotor, double DecelerationDueToBrake, out double Speed)
		{

			double PowerRollingCouplerAcceleration;
			// rolling on an incline
			{
				double a = FrontAxle.Follower.WorldDirection.Y;
				double b = RearAxle.Follower.WorldDirection.Y;
				PowerRollingCouplerAcceleration =
					-0.5 * (a + b) * TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity;
			}
			// friction
			double FrictionBrakeAcceleration;
			{
				double v = Math.Abs(CurrentSpeed);
				double t = Index == 0 & CurrentSpeed >= 0.0 || Index == baseTrain.NumberOfCars - 1 & CurrentSpeed <= 0.0 ? Specs.ExposedFrontalArea : Specs.UnexposedFrontalArea;
				double a = FrontAxle.GetResistance(v, t, TrainManagerBase.CurrentRoute.Atmosphere.GetAirDensity(FrontAxle.Follower.WorldPosition.Y), TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				double b = RearAxle.GetResistance(v, t, TrainManagerBase.CurrentRoute.Atmosphere.GetAirDensity(FrontAxle.Follower.WorldPosition.Y), TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				FrictionBrakeAcceleration = 0.5 * (a + b);
			}
			// power
			double wheelspin = 0.0;
			double wheelSlipAccelerationMotorFront = 0.0;
			double wheelSlipAccelerationMotorRear = 0.0;
			double wheelSlipAccelerationBrakeFront = 0.0;
			double wheelSlipAccelerationBrakeRear = 0.0;
			if (!Derailed)
			{
				wheelSlipAccelerationMotorFront = FrontAxle.CriticalWheelSlipAccelerationForElectricMotor(TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				wheelSlipAccelerationMotorRear = RearAxle.CriticalWheelSlipAccelerationForElectricMotor(TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				wheelSlipAccelerationBrakeFront = FrontAxle.CriticalWheelSlipAccelerationForFrictionBrake(TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				wheelSlipAccelerationBrakeRear = RearAxle.CriticalWheelSlipAccelerationForFrictionBrake(TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
			}

			if (DecelerationDueToMotor == 0.0)
			{
				double a;
				if (DecelerationDueToMotor == 0.0 || !Specs.IsMotorCar)
				{
					if (baseTrain.Handles.Reverser.Actual != 0 & baseTrain.Handles.Power.Actual > 0 &
					    !baseTrain.Handles.HoldBrake.Actual &
					    !baseTrain.Handles.EmergencyBrake.Actual)
					{
						// target acceleration
						if (baseTrain.Handles.Power.Actual - 1 < Specs.AccelerationCurves.Length)
						{
							// Load factor is a constant 1.0 for anything prior to BVE5
							// This will need to be changed when the relevant branch is merged in
							a = Specs.AccelerationCurves[baseTrain.Handles.Power.Actual - 1].GetAccelerationOutput((double)baseTrain.Handles.Reverser.Actual * CurrentSpeed, 1.0);
						}
						else
						{
							a = 0.0;
						}

						// readhesion device
						if (ReAdhesionDevice is BveReAdhesionDevice device)
						{
							if (a > device.MaximumAccelerationOutput)
							{
								a = device.MaximumAccelerationOutput;
							}
						}
						else if (ReAdhesionDevice is Sanders)
						{
							wheelSlipAccelerationMotorFront *= 2.0;
							wheelSlipAccelerationMotorRear *= 2.0;
							wheelSlipAccelerationBrakeFront *= 2.0;
							wheelSlipAccelerationBrakeRear *= 2.0;
						}


						// wheel slip
						if (a < wheelSlipAccelerationMotorFront)
						{
							FrontAxle.CurrentWheelSlip = false;
						}
						else
						{
							FrontAxle.CurrentWheelSlip = true;
							wheelspin += (double)baseTrain.Handles.Reverser.Actual * a * CurrentMass;
						}

						if (a < wheelSlipAccelerationMotorRear)
						{
							RearAxle.CurrentWheelSlip = false;
						}
						else
						{
							RearAxle.CurrentWheelSlip = true;
							wheelspin += (double)baseTrain.Handles.Reverser.Actual * a * CurrentMass;
						}

						Specs.MaxMotorAcceleration = a;
						// Update constant speed device
						this.ConstSpeed.Update(ref a, baseTrain.Specs.CurrentConstSpeed, baseTrain.Handles.Reverser.Actual);

						// finalize
						if (wheelspin != 0.0) a = 0.0;
					}
					else
					{
						a = 0.0;
						FrontAxle.CurrentWheelSlip = false;
						RearAxle.CurrentWheelSlip = false;
					}
				}
				else
				{
					// HACK: Use special value here to inform the BVE readhesion device it shouldn't update this frame
					Specs.MaxMotorAcceleration = -1;
					a = 0.0;
					FrontAxle.CurrentWheelSlip = false;
					RearAxle.CurrentWheelSlip = false;
				}


				if (!Derailed)
				{
					if (Specs.MotorAcceleration < a)
					{
						if (Specs.MotorAcceleration < 0.0)
						{
							Specs.MotorAcceleration += CarBrake.JerkDown * TimeElapsed;
						}
						else
						{
							Specs.MotorAcceleration += Specs.JerkPowerUp * TimeElapsed;
						}

						if (Specs.MotorAcceleration > a)
						{
							Specs.MotorAcceleration = a;
						}
					}
					else
					{
						Specs.MotorAcceleration -= Specs.JerkPowerDown * TimeElapsed;
						if (Specs.MotorAcceleration < a)
						{
							Specs.MotorAcceleration = a;
						}
					}
				}
				else
				{
					Specs.MotorAcceleration = 0.0;
				}
			}

			ReAdhesionDevice.Update(TimeElapsed);
			// brake
			bool wheellock = wheelspin == 0.0 & Derailed;
			if (!Derailed & wheelspin == 0.0)
			{
				double a;
				// motor
				if (Specs.IsMotorCar & DecelerationDueToMotor != 0.0)
				{
					a = -DecelerationDueToMotor;
					if (Specs.MotorAcceleration > a)
					{
						if (Specs.MotorAcceleration > 0.0)
						{
							Specs.MotorAcceleration -= Specs.JerkPowerDown * TimeElapsed;
						}
						else
						{
							Specs.MotorAcceleration -= CarBrake.JerkUp * TimeElapsed;
						}

						if (Specs.MotorAcceleration < a)
						{
							Specs.MotorAcceleration = a;
						}
					}
					else
					{
						Specs.MotorAcceleration += CarBrake.JerkDown * TimeElapsed;
						if (Specs.MotorAcceleration > a)
						{
							Specs.MotorAcceleration = a;
						}
					}
				}

				// brake
				a = DecelerationDueToBrake;
				if (CurrentSpeed >= -0.01 & CurrentSpeed <= 0.01)
				{
					double rf = FrontAxle.Follower.WorldDirection.Y;
					double rr = RearAxle.Follower.WorldDirection.Y;
					double ra = Math.Abs(0.5 * (rf + rr) *
					                     TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
					if (a > ra) a = ra;
				}

				double factor = EmptyMass / CurrentMass;
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
			else if (Derailed)
			{
				FrictionBrakeAcceleration += TrainBase.CoefficientOfGroundFriction *
				                             TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity;
			}

			// motor
			if (baseTrain.Handles.Reverser.Actual != 0)
			{
				double factor = EmptyMass / CurrentMass;
				if (Specs.MotorAcceleration > 0.0)
				{
					PowerRollingCouplerAcceleration +=
						(double) baseTrain.Handles.Reverser.Actual * Specs.MotorAcceleration * factor;
				}
				else
				{
					double a = -Specs.MotorAcceleration;
					if (a >= wheelSlipAccelerationMotorFront)
					{
						FrontAxle.CurrentWheelSlip = true;
					}
					else if (!Derailed)
					{
						FrictionBrakeAcceleration += 0.5 * a * factor;
					}

					if (a >= wheelSlipAccelerationMotorRear)
					{
						RearAxle.CurrentWheelSlip = true;
					}
					else
					{
						FrictionBrakeAcceleration += 0.5 * a * factor;
					}
				}
			}
			else
			{
				Specs.MotorAcceleration = 0.0;
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
					target = CurrentSpeed;
				}
				else
				{
					target = CurrentSpeed + wheelspin / 2500.0;
				}

				double diff = target - Specs.PerceivedSpeed;
				double rate = (diff < 0.0 ? 5.0 : 1.0) * TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity *
				              TimeElapsed;
				rate *= 1.0 - 0.7 / (diff * diff + 1.0);
				double factor = rate * rate;
				factor = 1.0 - factor / (factor + 1000.0);
				rate *= factor;
				if (diff >= -rate & diff <= rate)
				{
					Specs.PerceivedSpeed = target;
				}
				else
				{
					Specs.PerceivedSpeed += rate * Math.Sign(diff);
				}
			}
			// calculate new speed
			{
				int d = Math.Sign(CurrentSpeed);
				double a = PowerRollingCouplerAcceleration;
				double b = FrictionBrakeAcceleration;
				if (Math.Abs(a) < b)
				{
					if (Math.Sign(a) == d)
					{
						if (d == 0)
						{
							Speed = 0.0;
						}
						else
						{
							double c = (b - Math.Abs(a)) * TimeElapsed;
							if (Math.Abs(CurrentSpeed) > c)
							{
								Speed = CurrentSpeed - d * c;
							}
							else
							{
								Speed = 0.0;
							}
						}
					}
					else
					{
						double c = (Math.Abs(a) + b) * TimeElapsed;
						if (Math.Abs(CurrentSpeed) > c)
						{
							Speed = CurrentSpeed - d * c;
						}
						else
						{
							Speed = 0.0;
						}
					}
				}
				else
				{
					Speed = CurrentSpeed + (a - b * d) * TimeElapsed;
				}
			}
		}

		public void DetermineDoorClosingSpeed()
		{
			if (Specs.DoorOpenFrequency <= 0.0)
			{
				if (Doors[0].OpenSound.Buffer != null & Doors[1].OpenSound.Buffer != null)
				{
					double a = Doors[0].OpenSound.Buffer.Duration;
					double b = Doors[1].OpenSound.Buffer.Duration;
					Specs.DoorOpenFrequency = a + b > 0.0 ? 2.0 / (a + b) : 0.8;
				}
				else if (Doors[0].OpenSound.Buffer != null)
				{
					double a = Doors[0].OpenSound.Buffer.Duration;
					Specs.DoorOpenFrequency = a > 0.0 ? 1.0 / a : 0.8;
				}
				else if (Doors[1].OpenSound.Buffer != null)
				{
					double b = Doors[1].OpenSound.Buffer.Duration;
					Specs.DoorOpenFrequency = b > 0.0 ? 1.0 / b : 0.8;
				}
				else
				{
					Specs.DoorOpenFrequency = 0.8;
				}
			}

			if (Specs.DoorCloseFrequency <= 0.0)
			{
				if (Doors[0].CloseSound.Buffer != null & Doors[1].CloseSound.Buffer != null)
				{
					double a = Doors[0].CloseSound.Buffer.Duration;
					double b = Doors[1].CloseSound.Buffer.Duration;
					Specs.DoorCloseFrequency = a + b > 0.0 ? 2.0 / (a + b) : 0.8;
				}
				else if (Doors[0].CloseSound.Buffer != null)
				{
					double a = Doors[0].CloseSound.Buffer.Duration;
					Specs.DoorCloseFrequency = a > 0.0 ? 1.0 / a : 0.8;
				}
				else if (Doors[1].CloseSound.Buffer != null)
				{
					double b = Doors[1].CloseSound.Buffer.Duration;
					Specs.DoorCloseFrequency = b > 0.0 ? 1.0 / b : 0.8;
				}
				else
				{
					Specs.DoorCloseFrequency = 0.8;
				}
			}

			const double f = 0.015;
			const double g = 2.75;
			Specs.DoorOpenPitch = Math.Exp(f * Math.Tan(g * (TrainManagerBase.RandomNumberGenerator.NextDouble() - 0.5)));
			Specs.DoorClosePitch = Math.Exp(f * Math.Tan(g * (TrainManagerBase.RandomNumberGenerator.NextDouble() - 0.5)));
			Specs.DoorOpenFrequency /= Specs.DoorOpenPitch;
			Specs.DoorCloseFrequency /= Specs.DoorClosePitch;
			/* 
			 * Remove the following two lines, then the pitch at which doors play
			 * takes their randomized opening and closing times into account.
			 * */
			Specs.DoorOpenPitch = 1.0;
			Specs.DoorClosePitch = 1.0;
		}

		public override void Derail()
		{
			baseTrain.Derail(this, 0.0);
		}
	}
}
