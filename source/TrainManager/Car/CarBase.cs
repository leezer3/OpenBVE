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
		public TrainBase BaseTrain;
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

		public CarBase(TrainBase train, int index, double coefficientOfFriction, double coefficientOfRollingResistance, double aerodynamicDragCoefficient)
		{
			Specs = new CarPhysics();
			Brightness = new Brightness(this);
			BaseTrain = train;
			trainCarIndex = index;
			CarSections = new CarSection[] { };
			FrontAxle = new Axle(TrainManagerBase.CurrentHost, train, this, coefficientOfFriction, coefficientOfRollingResistance, aerodynamicDragCoefficient);
			FrontAxle.Follower.TriggerType = index == 0 ? EventTriggerType.FrontCarFrontAxle : EventTriggerType.OtherCarFrontAxle;
			RearAxle = new Axle(TrainManagerBase.CurrentHost, train, this, coefficientOfFriction, coefficientOfRollingResistance, aerodynamicDragCoefficient);
			RearAxle.Follower.TriggerType = index == BaseTrain.Cars.Length - 1 ? EventTriggerType.RearCarRearAxle : EventTriggerType.OtherCarRearAxle;
			BeaconReceiver = new TrackFollower(TrainManagerBase.CurrentHost, train);
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
			BaseTrain = train;
			trainCarIndex = index;
			CarSections = new CarSection[] { };
			CurrentCarSection = -1;
			FrontAxle = new Axle(TrainManagerBase.CurrentHost, train, this);
			RearAxle = new Axle(TrainManagerBase.CurrentHost, train, this);
			BeaconReceiver = new TrackFollower(TrainManagerBase.CurrentHost, train);
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
		/// <param name="delta">The delta to move</param>
		public void Move(double delta)
		{
			if (BaseTrain.State != TrainState.Disposed)
			{
				FrontAxle.Follower.UpdateRelative(delta, true, true);
				FrontBogie.FrontAxle.Follower.UpdateRelative(delta, true, true);
				FrontBogie.RearAxle.Follower.UpdateRelative(delta, true, true);
				if (BaseTrain.State != TrainState.Disposed)
				{
					RearAxle.Follower.UpdateRelative(delta, true, true);
					RearBogie.FrontAxle.Follower.UpdateRelative(delta, true, true);
					RearBogie.RearAxle.Follower.UpdateRelative(delta, true, true);
					if (BaseTrain.State != TrainState.Disposed)
					{
						BeaconReceiver.UpdateRelative(delta, true, false);
					}
				}
			}
		}

		/// <summary>Call this method to update all track followers attached to the car</summary>
		/// <param name="newTrackPosition">The track position change</param>
		/// <param name="updateWorldCoordinates">Whether to update the world co-ordinates</param>
		/// <param name="addTrackInaccurary">Whether to add track innaccuarcy</param>
		public void UpdateTrackFollowers(double newTrackPosition, bool updateWorldCoordinates, bool addTrackInaccurary)
		{
			//Car axles
			FrontAxle.Follower.UpdateRelative(newTrackPosition, updateWorldCoordinates, addTrackInaccurary);
			RearAxle.Follower.UpdateRelative(newTrackPosition, updateWorldCoordinates, addTrackInaccurary);
			//Front bogie axles
			FrontBogie.FrontAxle.Follower.UpdateRelative(newTrackPosition, updateWorldCoordinates, addTrackInaccurary);
			FrontBogie.RearAxle.Follower.UpdateRelative(newTrackPosition, updateWorldCoordinates, addTrackInaccurary);
			//Rear bogie axles

			RearBogie.FrontAxle.Follower.UpdateRelative(newTrackPosition, updateWorldCoordinates, addTrackInaccurary);
			RearBogie.RearAxle.Follower.UpdateRelative(newTrackPosition, updateWorldCoordinates, addTrackInaccurary);
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

		public override void CreateWorldCoordinates(Vector3 car, out Vector3 position, out Vector3 direction)
		{
			direction = FrontAxle.Follower.WorldPosition - RearAxle.Follower.WorldPosition;
			double t = direction.NormSquared();
			if (t != 0.0)
			{
				t = 1.0 / Math.Sqrt(t);
				direction *= t;
				double sx = direction.Z * Up.Y - direction.Y * Up.Z;
				double sy = direction.X * Up.Z - direction.Z * Up.X;
				double sz = direction.Y * Up.X - direction.X * Up.Y;
				double rx = 0.5 * (FrontAxle.Follower.WorldPosition.X + RearAxle.Follower.WorldPosition.X);
				double ry = 0.5 * (FrontAxle.Follower.WorldPosition.Y + RearAxle.Follower.WorldPosition.Y);
				double rz = 0.5 * (FrontAxle.Follower.WorldPosition.Z + RearAxle.Follower.WorldPosition.Z);
				position.X = rx + sx * car.X + Up.X * car.Y + direction.X * car.Z;
				position.Y = ry + sy * car.X + Up.Y * car.Y + direction.Y * car.Z;
				position.Z = rz + sz * car.X + Up.Z * car.Y + direction.Z * car.Z;
			}
			else
			{
				position = FrontAxle.Follower.WorldPosition;
				direction = Vector3.Down;
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

		public override void OpenDoors(bool left, bool right)
		{
			bool sl = false, sr = false;
			if (left & !Doors[0].AnticipatedOpen & (BaseTrain.SafetySystems.DoorInterlockState == DoorInterlockStates.Left | BaseTrain.SafetySystems.DoorInterlockState == DoorInterlockStates.Unlocked))
			{
				Doors[0].AnticipatedOpen = true;
				sl = true;
			}

			if (right & !Doors[1].AnticipatedOpen & (BaseTrain.SafetySystems.DoorInterlockState == DoorInterlockStates.Right | BaseTrain.SafetySystems.DoorInterlockState == DoorInterlockStates.Unlocked))
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

		public override void Uncouple(bool front, bool rear)
		{
			lock (BaseTrain.UpdateLock)
			{
				if (!front && !rear)
				{
					return;
				}

				// Create new train
				TrainBase newTrain = new TrainBase(TrainState.Available);
				UncouplingBehaviour uncouplingBehaviour = UncouplingBehaviour.Emergency;
				newTrain.Handles.Power = new PowerHandle(0, 0, new double[0], new double[0], newTrain);
				newTrain.Handles.Brake = new BrakeHandle(0, 0, newTrain.Handles.EmergencyBrake, new double[0], new double[0], newTrain);
				newTrain.Handles.HoldBrake = new HoldBrakeHandle(newTrain);
				if (front)
				{
					int totalPreceedingCars = trainCarIndex;
					newTrain.Cars = new CarBase[trainCarIndex];
					for (int i = 0; i < totalPreceedingCars; i++)
					{
						newTrain.Cars[i] = BaseTrain.Cars[i];
						newTrain.Cars[i].BaseTrain = newTrain;
					}

					for (int i = totalPreceedingCars; i < BaseTrain.Cars.Length; i++)
					{
						BaseTrain.Cars[i - totalPreceedingCars] = BaseTrain.Cars[i];
						BaseTrain.Cars[i].Index = i - totalPreceedingCars;
					}

					Array.Resize(ref BaseTrain.Cars, BaseTrain.Cars.Length - totalPreceedingCars);
					BaseTrain.Cars[BaseTrain.Cars.Length - 1].Coupler.UncoupleSound.Play(BaseTrain.Cars[BaseTrain.Cars.Length - 1], false);
					uncouplingBehaviour = BaseTrain.Cars[BaseTrain.Cars.Length - 1].Coupler.UncouplingBehaviour;
					TrainManagerBase.CurrentHost.AddTrain(BaseTrain, newTrain, false);

					if (BaseTrain.DriverCar - totalPreceedingCars >= 0)
					{
						BaseTrain.DriverCar -= totalPreceedingCars;
					}
				}

				if (rear)
				{
					int totalFollowingCars = BaseTrain.Cars.Length - (Index + 1);
					if (totalFollowingCars > 0)
					{
						newTrain.Cars = new CarBase[totalFollowingCars];
						// Move following cars to new train
						for (int i = 0; i < totalFollowingCars; i++)
						{
							newTrain.Cars[i] = BaseTrain.Cars[Index + i + 1];
							newTrain.Cars[i].BaseTrain = newTrain;
							newTrain.Cars[i].Index = i;
						}

						Array.Resize(ref BaseTrain.Cars, BaseTrain.Cars.Length - totalFollowingCars);
						BaseTrain.Cars[BaseTrain.Cars.Length - 1].Coupler.ConnectedCar = BaseTrain.Cars[BaseTrain.Cars.Length - 1];
					}
					else
					{
						return;
					}

					Coupler.UncoupleSound.Play(this, false);
					uncouplingBehaviour = Coupler.UncouplingBehaviour;
					TrainManagerBase.CurrentHost.AddTrain(BaseTrain, newTrain, true);
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

				if (BaseTrain.DriverCar >= BaseTrain.Cars.Length)
				{
					/*
					 * The driver car is no longer in the train
					 *
					 * Look for a car with an interior view to substitute
					 * If not found, this will stop at Car 0
					 */

					for (int i = BaseTrain.Cars.Length; i > 0; i--)
					{
						BaseTrain.DriverCar = i - 1;
						if (!BaseTrain.Cars[i - 1].HasInteriorView)
						{
							/*
							 * Set the eye position to something vaguely sensible, rather than leaving it on the rails
							 * Whilst there will be no cab, at least it's a bit more usable like this
							 */
							BaseTrain.Cars[i - 1].InteriorCamera = new CameraAlignment()
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

				if (front)
				{
					// Uncoupling the front will always make the car our first
					BaseTrain.CameraCar = 0;
				}
				else
				{
					if (BaseTrain.CameraCar >= BaseTrain.Cars.Length)
					{
						BaseTrain.CameraCar = BaseTrain.DriverCar;
					}
				}

				switch (uncouplingBehaviour)
				{
					case UncouplingBehaviour.Emergency:
						BaseTrain.Handles.EmergencyBrake.Apply();
						newTrain.Handles.EmergencyBrake.Apply();
						break;
					case UncouplingBehaviour.EmergencyUncoupledConsist:
						newTrain.Handles.EmergencyBrake.Apply();
						break;
					case UncouplingBehaviour.EmergencyPlayer:
						BaseTrain.Handles.EmergencyBrake.Apply();
						break;
					case UncouplingBehaviour.Released:
						BaseTrain.Handles.Brake.ApplyState(0, false);
						BaseTrain.Handles.EmergencyBrake.Release();
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
		/// <param name="left">Whether to include left doors.</param>
		/// <param name="right">Whether to include right doors.</param>
		/// <returns>A bit mask combining encountered door states.</returns>
		public TrainDoorState GetDoorsState(bool left, bool right)
		{
			bool opened = false, closed = false, mixed = false;
			for (int i = 0; i < Doors.Length; i++)
			{
				if (left & Doors[i].Direction == -1 | right & Doors[i].Direction == 1)
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

			TrainDoorState result = TrainDoorState.None;
			if (opened) result |= TrainDoorState.Opened;
			if (closed) result |= TrainDoorState.Closed;
			if (mixed) result |= TrainDoorState.Mixed;
			if (opened & !closed & !mixed) result |= TrainDoorState.AllOpened;
			if (!opened & closed & !mixed) result |= TrainDoorState.AllClosed;
			if (!opened & !closed & mixed) result |= TrainDoorState.AllMixed;
			return result;
		}
		
		/// <summary>Loads Car Sections (Exterior objects etc.) for this car</summary>
		/// <param name="currentObject">The object to add to the car sections array</param>
		/// <param name="visibleFromInterior">Wether this is visible from the interior of other cars</param>
		public void LoadCarSections(UnifiedObject currentObject, bool visibleFromInterior)
		{
			int j = CarSections.Length;
			Array.Resize(ref CarSections, j + 1);
			CarSections[j] = new CarSection(TrainManagerBase.CurrentHost, ObjectType.Dynamic, visibleFromInterior, currentObject);
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
						TrainManagerBase.CurrentHost.HideObject(CarSections[i].Groups[j].Elements[k].internalObject);
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
		/// <param name="timeElapsed">The time elapsed</param>
		/// <param name="forceUpdate">Whether this is a forced update</param>
		/// <param name="enableDamping">Whether damping is applied during this update (Skipped on transitions between camera views etc.)</param>
		public void UpdateObjects(double timeElapsed, bool forceUpdate, bool enableDamping)
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
						UpdateCarSectionElement(cs, 0, i, p, d, s, CurrentlyVisible, timeElapsed, forceUpdate, enableDamping);

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
						UpdateCarSectionElement(cs, add, i, p, d, s, CurrentlyVisible, timeElapsed, forceUpdate, enableDamping);

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
							UpdateCarSectionTouchElement(cs, add, i, p, d, s, false, timeElapsed, forceUpdate, enableDamping);
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
		/// <param name="sectionIndex">The car section</param>
		/// <param name="groupIndex">The group within the car section</param>
		/// <param name="elementIndex">The element within the group</param>
		/// <param name="position"></param>
		/// <param name="direction"></param>
		/// <param name="side"></param>
		/// <param name="show"></param>
		/// <param name="timeElapsed"></param>
		/// <param name="forceUpdate"></param>
		/// <param name="enableDamping"></param>
		private void UpdateCarSectionElement(int sectionIndex, int groupIndex, int elementIndex, Vector3 position, Vector3 direction, Vector3 side, bool show, double timeElapsed, bool forceUpdate, bool enableDamping)
		{
			Vector3 p;
			if (CarSections[sectionIndex].Type == ObjectType.Overlay & (TrainManagerBase.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable && TrainManagerBase.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.Restricted3D))
			{
				p = new Vector3(Driver.X, Driver.Y, Driver.Z);
			}
			else
			{
				p = position;
			}

			double timeDelta;
			bool updatefunctions;
			if (CarSections[sectionIndex].Groups[groupIndex].Elements[elementIndex].RefreshRate != 0.0)
			{
				if (CarSections[sectionIndex].Groups[groupIndex].Elements[elementIndex].SecondsSinceLastUpdate >= CarSections[sectionIndex].Groups[groupIndex].Elements[elementIndex].RefreshRate)
				{
					timeDelta = CarSections[sectionIndex].Groups[groupIndex].Elements[elementIndex].SecondsSinceLastUpdate;
					CarSections[sectionIndex].Groups[groupIndex].Elements[elementIndex].SecondsSinceLastUpdate = timeElapsed;
					updatefunctions = true;
				}
				else
				{
					timeDelta = timeElapsed;
					CarSections[sectionIndex].Groups[groupIndex].Elements[elementIndex].SecondsSinceLastUpdate += timeElapsed;
					updatefunctions = false;
				}
			}
			else
			{
				timeDelta = CarSections[sectionIndex].Groups[groupIndex].Elements[elementIndex].SecondsSinceLastUpdate;
				CarSections[sectionIndex].Groups[groupIndex].Elements[elementIndex].SecondsSinceLastUpdate = timeElapsed;
				updatefunctions = true;
			}

			if (forceUpdate)
			{
				updatefunctions = true;
			}

			CarSections[sectionIndex].Groups[groupIndex].Elements[elementIndex].Update(BaseTrain, Index, FrontAxle.Follower.TrackPosition - FrontAxle.Position, p, direction, Up, side, updatefunctions, show, timeDelta, enableDamping, false, CarSections[sectionIndex].Type == ObjectType.Overlay ? TrainManagerBase.Renderer.Camera : null);
			if (!TrainManagerBase.Renderer.ForceLegacyOpenGL && CarSections[sectionIndex].Groups[groupIndex].Elements[elementIndex].UpdateVAO)
			{
				VAOExtensions.CreateVAO(ref CarSections[sectionIndex].Groups[groupIndex].Elements[elementIndex].internalObject.Prototype.Mesh, true, TrainManagerBase.Renderer.DefaultShader.VertexLayout, TrainManagerBase.Renderer);
			}
		}

		private void UpdateCarSectionTouchElement(int sectionIndex, int groupIndex, int elementIndex, Vector3 position, Vector3 direction, Vector3 side, bool show, double timeElapsed, bool forceUpdate, bool enableDamping)
		{
			Vector3 p;
			if (CarSections[sectionIndex].Type == ObjectType.Overlay & (TrainManagerBase.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable && TrainManagerBase.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.Restricted3D))
			{
				p = new Vector3(Driver.X, Driver.Y, Driver.Z);
			}
			else
			{
				p = position;
			}

			double timeDelta;
			bool updatefunctions;
			if (CarSections[sectionIndex].Groups[groupIndex].TouchElements[elementIndex].Element.RefreshRate != 0.0)
			{
				if (CarSections[sectionIndex].Groups[groupIndex].TouchElements[elementIndex].Element.SecondsSinceLastUpdate >= CarSections[sectionIndex].Groups[groupIndex].TouchElements[elementIndex].Element.RefreshRate)
				{
					timeDelta = CarSections[sectionIndex].Groups[groupIndex].TouchElements[elementIndex].Element.SecondsSinceLastUpdate;
					CarSections[sectionIndex].Groups[groupIndex].TouchElements[elementIndex].Element.SecondsSinceLastUpdate = timeElapsed;
					updatefunctions = true;
				}
				else
				{
					timeDelta = timeElapsed;
					CarSections[sectionIndex].Groups[groupIndex].TouchElements[elementIndex].Element.SecondsSinceLastUpdate += timeElapsed;
					updatefunctions = false;
				}
			}
			else
			{
				timeDelta = CarSections[sectionIndex].Groups[groupIndex].TouchElements[elementIndex].Element.SecondsSinceLastUpdate;
				CarSections[sectionIndex].Groups[groupIndex].TouchElements[elementIndex].Element.SecondsSinceLastUpdate = timeElapsed;
				updatefunctions = true;
			}

			if (forceUpdate)
			{
				updatefunctions = true;
			}

			CarSections[sectionIndex].Groups[groupIndex].TouchElements[elementIndex].Element.Update(BaseTrain, Index, FrontAxle.Follower.TrackPosition - FrontAxle.Position, p, direction, Up, side, updatefunctions, show, timeDelta, enableDamping, true, CarSections[sectionIndex].Type == ObjectType.Overlay ? TrainManagerBase.Renderer.Camera : null);
			if (!TrainManagerBase.Renderer.ForceLegacyOpenGL && CarSections[sectionIndex].Groups[groupIndex].TouchElements[elementIndex].Element.UpdateVAO)
			{
				VAOExtensions.CreateVAO(ref CarSections[sectionIndex].Groups[groupIndex].TouchElements[elementIndex].Element.internalObject.Prototype.Mesh, true, TrainManagerBase.Renderer.DefaultShader.VertexLayout, TrainManagerBase.Renderer);
			}
		}

		public void UpdateTopplingCantAndSpring(double timeElapsed)
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
						Specs.RollShakeDirection += dr * timeElapsed;
						if (Specs.RollShakeDirection > 0.0) Specs.RollShakeDirection = 0.0;
					}
					else
					{
						Specs.RollShakeDirection -= dr * timeElapsed;
						if (Specs.RollShakeDirection < 0.0) Specs.RollShakeDirection = 0.0;
					}
				}

				double springAcceleration;
				if (!Derailed)
				{
					springAcceleration = 15.0 * Math.Abs(a1 - a0);
				}
				else
				{
					springAcceleration = 1.5 * Math.Abs(a1 - a0);
				}

				double springDeceleration = 0.25 * springAcceleration;
				Specs.RollDueToShakingAngularSpeed += Math.Sign(a1 - a0) * springAcceleration * timeElapsed;
				double x = Math.Sign(Specs.RollDueToShakingAngularSpeed) * springDeceleration * timeElapsed;
				if (Math.Abs(x) < Math.Abs(Specs.RollDueToShakingAngularSpeed))
				{
					Specs.RollDueToShakingAngularSpeed -= x;
				}
				else
				{
					Specs.RollDueToShakingAngularSpeed = 0.0;
				}

				a0 += Specs.RollDueToShakingAngularSpeed * timeElapsed;
				Specs.RollDueToShakingAngle = a0;
			}
			// roll due to cant (incorporates shaking)
			{
				double cantAngle = Math.Atan(Math.Tan(0.5 * (Math.Atan(FrontAxle.Follower.CurveCant) + Math.Atan(RearAxle.Follower.CurveCant))) / TrainManagerBase.CurrentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge);
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
						v -= j * timeElapsed;
						if (v < a) v = a;
					}
					else
					{
						v += j * timeElapsed;
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
					Specs.PitchDueToAccelerationAngularSpeed += a * timeElapsed;
					double ds = Math.Abs(Specs.PitchDueToAccelerationTargetAngle - Specs.PitchDueToAccelerationAngle);
					if (Math.Abs(Specs.PitchDueToAccelerationAngularSpeed) > ds)
					{
						Specs.PitchDueToAccelerationAngularSpeed = ds * Math.Sign(Specs.PitchDueToAccelerationAngularSpeed);
					}

					Specs.PitchDueToAccelerationAngle += Specs.PitchDueToAccelerationAngularSpeed * timeElapsed;
				}
			}
			// derailment
			if (TrainManagerBase.Derailments & !Derailed)
			{
				double a = Specs.RollDueToTopplingAngle + Specs.RollDueToCantAngle;
				double sa = Math.Sign(a);
				if (a * sa > Specs.CriticalTopplingAngle)
				{
					BaseTrain.Derail(Index, timeElapsed);
				}
			}

			// toppling roll
			if (TrainManagerBase.Toppling | Derailed)
			{
				double a = Specs.RollDueToTopplingAngle;
				double ab = Specs.RollDueToTopplingAngle + Specs.RollDueToCantAngle;
				double h = Specs.CenterOfGravityHeight;
				double sr = Math.Abs(CurrentSpeed);
				double rmax = 2.0 * h * sr * sr / (TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity * TrainManagerBase.CurrentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge);
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
							double s0 = Math.Sqrt(r * TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity * TrainManagerBase.CurrentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge / (2.0 * h));
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
					a -= td * timeElapsed;
				}
				else if (a < ta)
				{
					double da = ta - a;
					if (td > da) td = da;
					a += td * timeElapsed;
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
				double x = Math.Sign(a) * 0.5 * TrainManagerBase.CurrentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge * (1.0 - Math.Cos(a));
				double y = Math.Abs(0.5 * TrainManagerBase.CurrentHost.Tracks[FrontAxle.Follower.TrackIndex].RailGauge * Math.Sin(a));
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

			Suspension.Update(timeElapsed);
			Flange.Update(timeElapsed);
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
			Vector3 driverPosition = this.HasInteriorView ? Driver : this.BaseTrain.Cars[this.BaseTrain.DriverCar].Driver;
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

		public void UpdateSpeed(double timeElapsed, double decelerationDueToMotor, double decelerationDueToBrake, out double speed)
		{

			double powerRollingCouplerAcceleration;
			// rolling on an incline
			{
				double a = FrontAxle.Follower.WorldDirection.Y;
				double b = RearAxle.Follower.WorldDirection.Y;
				powerRollingCouplerAcceleration =
					-0.5 * (a + b) * TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity;
			}
			// friction
			double frictionBrakeAcceleration;
			{
				double v = Math.Abs(CurrentSpeed);
				double t = Index == 0 & CurrentSpeed >= 0.0 || Index == BaseTrain.NumberOfCars - 1 & CurrentSpeed <= 0.0 ? Specs.ExposedFrontalArea : Specs.UnexposedFrontalArea;
				double a = FrontAxle.GetResistance(v, t, TrainManagerBase.CurrentRoute.Atmosphere.GetAirDensity(FrontAxle.Follower.WorldPosition.Y), TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				double b = RearAxle.GetResistance(v, t, TrainManagerBase.CurrentRoute.Atmosphere.GetAirDensity(FrontAxle.Follower.WorldPosition.Y), TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				frictionBrakeAcceleration = 0.5 * (a + b);
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

			if (decelerationDueToMotor == 0.0)
			{
				double a;
				if (decelerationDueToMotor == 0.0 || !Specs.IsMotorCar)
				{
					if (BaseTrain.Handles.Reverser.Actual != 0 & BaseTrain.Handles.Power.Actual > 0 &
					    !BaseTrain.Handles.HoldBrake.Actual &
					    !BaseTrain.Handles.EmergencyBrake.Actual)
					{
						// target acceleration
						if (BaseTrain.Handles.Power.Actual - 1 < Specs.AccelerationCurves.Length)
						{
							// Load factor is a constant 1.0 for anything prior to BVE5
							// This will need to be changed when the relevant branch is merged in
							a = Specs.AccelerationCurves[BaseTrain.Handles.Power.Actual - 1].GetAccelerationOutput((double)BaseTrain.Handles.Reverser.Actual * CurrentSpeed, 1.0);
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
							wheelspin += (double)BaseTrain.Handles.Reverser.Actual * a * CurrentMass;
						}

						if (a < wheelSlipAccelerationMotorRear)
						{
							RearAxle.CurrentWheelSlip = false;
						}
						else
						{
							RearAxle.CurrentWheelSlip = true;
							wheelspin += (double)BaseTrain.Handles.Reverser.Actual * a * CurrentMass;
						}

						Specs.MaxMotorAcceleration = a;
						// Update constant speed device
						this.ConstSpeed.Update(ref a, BaseTrain.Specs.CurrentConstSpeed, BaseTrain.Handles.Reverser.Actual);

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
							Specs.MotorAcceleration += CarBrake.JerkDown * timeElapsed;
						}
						else
						{
							Specs.MotorAcceleration += Specs.JerkPowerUp * timeElapsed;
						}

						if (Specs.MotorAcceleration > a)
						{
							Specs.MotorAcceleration = a;
						}
					}
					else
					{
						Specs.MotorAcceleration -= Specs.JerkPowerDown * timeElapsed;
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

			ReAdhesionDevice.Update(timeElapsed);
			// brake
			bool wheellock = wheelspin == 0.0 & Derailed;
			if (!Derailed & wheelspin == 0.0)
			{
				double a;
				// motor
				if (Specs.IsMotorCar & decelerationDueToMotor != 0.0)
				{
					a = -decelerationDueToMotor;
					if (Specs.MotorAcceleration > a)
					{
						if (Specs.MotorAcceleration > 0.0)
						{
							Specs.MotorAcceleration -= Specs.JerkPowerDown * timeElapsed;
						}
						else
						{
							Specs.MotorAcceleration -= CarBrake.JerkUp * timeElapsed;
						}

						if (Specs.MotorAcceleration < a)
						{
							Specs.MotorAcceleration = a;
						}
					}
					else
					{
						Specs.MotorAcceleration += CarBrake.JerkDown * timeElapsed;
						if (Specs.MotorAcceleration > a)
						{
							Specs.MotorAcceleration = a;
						}
					}
				}

				// brake
				a = decelerationDueToBrake;
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
					frictionBrakeAcceleration += 0.5 * a * factor;
				}

				if (a >= wheelSlipAccelerationBrakeRear)
				{
					wheellock = true;
				}
				else
				{
					frictionBrakeAcceleration += 0.5 * a * factor;
				}
			}
			else if (Derailed)
			{
				frictionBrakeAcceleration += TrainBase.CoefficientOfGroundFriction *
				                             TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity;
			}

			// motor
			if (BaseTrain.Handles.Reverser.Actual != 0)
			{
				double factor = EmptyMass / CurrentMass;
				if (Specs.MotorAcceleration > 0.0)
				{
					powerRollingCouplerAcceleration +=
						(double) BaseTrain.Handles.Reverser.Actual * Specs.MotorAcceleration * factor;
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
						frictionBrakeAcceleration += 0.5 * a * factor;
					}

					if (a >= wheelSlipAccelerationMotorRear)
					{
						RearAxle.CurrentWheelSlip = true;
					}
					else
					{
						frictionBrakeAcceleration += 0.5 * a * factor;
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
				              timeElapsed;
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
				double a = powerRollingCouplerAcceleration;
				double b = frictionBrakeAcceleration;
				if (Math.Abs(a) < b)
				{
					if (Math.Sign(a) == d)
					{
						if (d == 0)
						{
							speed = 0.0;
						}
						else
						{
							double c = (b - Math.Abs(a)) * timeElapsed;
							if (Math.Abs(CurrentSpeed) > c)
							{
								speed = CurrentSpeed - d * c;
							}
							else
							{
								speed = 0.0;
							}
						}
					}
					else
					{
						double c = (Math.Abs(a) + b) * timeElapsed;
						if (Math.Abs(CurrentSpeed) > c)
						{
							speed = CurrentSpeed - d * c;
						}
						else
						{
							speed = 0.0;
						}
					}
				}
				else
				{
					speed = CurrentSpeed + (a - b * d) * timeElapsed;
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
			BaseTrain.Derail(this, 0.0);
		}
	}
}
