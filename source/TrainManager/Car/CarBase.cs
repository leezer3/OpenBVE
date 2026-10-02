using System;
using System.Collections.Generic;
using System.Linq;
using LibRender2;
using LibRender2.Camera;
using LibRender2.Cameras;
using LibRender2.Smoke;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Motor;
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
using TrainManager.Motor;
using TrainManager.Power;
using TrainManager.SafetySystems;
using TrainManager.Trains;

namespace TrainManager.Car
{
	/*
	 * TEMPORARY NAME AND CLASS TO ALLOW FOR MOVE IN PARTS
	 */
	public partial class CarBase : AbstractCar
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
		public Dictionary<CarSectionType, CarSection> CarSections;
		/// <summary>The index of the current car section</summary>
		public CarSectionType CurrentCarSection;
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
		/// <summary>The readhesion device for this car</summary>
		public AbstractReAdhesionDevice ReAdhesionDevice;
		/// <summary>The position of the beacon receiver within the car</summary>
		public double BeaconReceiverPosition;
		/// <summary>The beacon receiver</summary>
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
		/// <summary>The traction model</summary>
		public TractionModel TractionModel;

		public List<ParticleSource> ParticleSources;

		public Dictionary<SafetySystem, AbstractSafetySystem> SafetySystems;

		public override Dictionary<PowerSupplyTypes, PowerSupply> AvailablePowerSupplies
		{
			get
			{
				if (TractionModel.Components.TryGetTypedValue(EngineComponent.Pantograph, out Pantograph pantograph))
				{
					return pantograph.AvailablePowerSupplies;
				}
				return base.AvailablePowerSupplies;
			}
		}

		private int trainCarIndex;

		public CarBase(TrainBase train, int index, double coefficientOfFriction, double coefficientOfRollingResistance, double aerodynamicDragCoefficient)
		{
			Specs = new CarPhysics();
			Brightness = new Brightness(this);
			baseTrain = train;
			trainCarIndex = index;
			CarSections = new Dictionary<CarSectionType, CarSection>();
			FrontAxle = new BVEAxle(TrainManagerBase.currentHost, train, this, coefficientOfFriction, coefficientOfRollingResistance, aerodynamicDragCoefficient);
			FrontAxle.Follower.TriggerType = index == 0 ? EventTriggerType.FrontCarFrontAxle : EventTriggerType.OtherCarFrontAxle;
			RearAxle = new BVEAxle(TrainManagerBase.currentHost, train, this, coefficientOfFriction, coefficientOfRollingResistance, aerodynamicDragCoefficient);
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
			ChangeCarSection(CarSectionType.NotVisible);
			Cargo = new Passengers(this);
			Suspension = new Suspension(this);
			Flange = new Flange(this);
			Run = new RunSounds(this);
			ParticleSources = new List<ParticleSource>();
			SafetySystems = new Dictionary<SafetySystem, AbstractSafetySystem>();
		}

		public CarBase(TrainBase train, int index)
		{
			baseTrain = train;
			trainCarIndex = index;
			CarSections = new Dictionary<CarSectionType, CarSection>();
			CurrentCarSection = CarSectionType.NotVisible;
			FrontAxle = new BVEAxle(TrainManagerBase.currentHost, train, this);
			RearAxle = new BVEAxle(TrainManagerBase.currentHost, train, this);
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
			Coupler = new Coupler(0, 0, this, null);
			ParticleSources = new List<ParticleSource>();
			SafetySystems = new Dictionary<SafetySystem, AbstractSafetySystem>();
		}

		/// <summary>Moves the car</summary>
		/// <param name="Delta">The delta to move</param>
		public void Move(double Delta)
		{
			if (baseTrain.State < TrainState.DisposePending)
			{
				FrontAxle.Follower.UpdateRelative(Delta, true, true);
				FrontBogie.FrontAxle.Follower.UpdateRelative(Delta, true, true);
				FrontBogie.RearAxle.Follower.UpdateRelative(Delta, true, true);
				if (baseTrain.State < TrainState.DisposePending)
				{
					RearAxle.Follower.UpdateRelative(Delta, true, true);
					RearBogie.FrontAxle.Follower.UpdateRelative(Delta, true, true);
					RearBogie.RearAxle.Follower.UpdateRelative(Delta, true, true);
					if (baseTrain.State < TrainState.DisposePending)
					{
						BeaconReceiver.UpdateRelative(Delta, true, false);
					}
				}
			}
		}

		/// <summary>Moves the car as a result of a collision</summary>
		/// <param name="Delta">The delta to move</param>
		public void MoveDueToCollision(double Delta)
		{
			FrontAxle.Follower.UpdateRelative(Delta, false, false);
			RearAxle.Follower.UpdateRelative(Delta, false, false);
			FrontBogie.FrontAxle.Follower.UpdateRelative(Delta, false, false);
			FrontBogie.RearAxle.Follower.UpdateRelative(Delta, false, false);
			RearBogie.FrontAxle.Follower.UpdateRelative(Delta, false, false);
			RearBogie.RearAxle.Follower.UpdateRelative(Delta, false, false);
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
			for (int i = 0; i < CarSections.Count; i++)
			{
				CarSectionType k = CarSections.ElementAt(i).Key;
				CarSections[k].Initialize(false);
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
		public void Synchronize()
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
			set
			{
				if (CarSections.TryGetTypedValue(CarSectionType.Interior, out CarSection interiorSection))
				{
					interiorSection.CorrectCarIndices(value - trainCarIndex);
				}
				if (CarSections.TryGetTypedValue(CarSectionType.Exterior, out CarSection exteriorSection))
				{
					exteriorSection.CorrectCarIndices(value - trainCarIndex);
				}

				for (int i = 0; i < ParticleSources.Count; i++)
				{
					ParticleSources[i].Controller.CorrectCarIndices(value - trainCarIndex);
				}
				trainCarIndex = value;
			}
		}

		public override void Reverse(bool flipInterior = false)
		{
			// reverse axle positions
			double temp = FrontAxle.Position;
			FrontAxle.Position = -RearAxle.Position;
			RearAxle.Position = -temp;
			if (flipInterior)
			{
				if (CarSections != null && CarSections.Count > 0)
				{
					for (int i = 0; i < CarSections.Count; i++)
					{
						CarSectionType k = CarSections.ElementAt(i).Key;
						if (CarSections[k].Type == ObjectType.Overlay)
						{
							for (int j = 0; j < CarSections[k].Groups.Length; j++)
							{
								CarSections[k].Groups[j].Reverse(Driver, TrainManagerBase.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable); // restriction not available must equal 3D cab
							}
						}
						else
						{
							foreach (AnimatedObject animatedObject in CarSections[k].Groups[0].Elements)
							{
								animatedObject.Reverse();
							}

							CarSections[k].Groups[0].Keyframes?.Reverse();
						}
						
					}
				}
				Driver = new Vector3(-Driver.X, Driver.Y, -Driver.Z);
				CameraRestriction.Reverse();
			}
			else
			{
				if (CarSections.TryGetValue(CarSectionType.Exterior, out CarSection sectionToReverse))
				{
					foreach (AnimatedObject animatedObject in sectionToReverse.Groups[0].Elements)
					{
						animatedObject.Reverse();
					}

					for (int i = 0; i < sectionToReverse.Groups.Length; i++)
					{
						sectionToReverse.Groups[i].Keyframes?.Reverse();
					}
				}	
			}

			for (int i = 0; i < ParticleSources.Count; i++)
			{
				ParticleSources[i].Offset.Z = -ParticleSources[i].Offset.Z;
			}

			(FrontBogie, RearBogie) = (RearBogie, FrontBogie);
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
			if (Left && !Doors[0].AnticipatedOpen && (baseTrain.SafetySystems.DoorInterlockState == DoorInterlockStates.Left || baseTrain.SafetySystems.DoorInterlockState == DoorInterlockStates.Unlocked))
			{
				Doors[0].AnticipatedOpen = true;
				sl = true;
			}

			if (Right && !Doors[1].AnticipatedOpen && (baseTrain.SafetySystems.DoorInterlockState == DoorInterlockStates.Right || baseTrain.SafetySystems.DoorInterlockState == DoorInterlockStates.Unlocked))
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
					switch (Doors[i].State)
					{
						case 0.0:
							closed = true;
							break;
						case 1.0:
							opened = true;
							break;
						default:
							mixed = true;
							break;
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
			if (CarSections.TryGetValue(CurrentCarSection, out CarSection currentCarSection))
			{
				if (currentCarSection.Groups.Length > 0)
				{
					for (int i = 0; i < currentCarSection.Groups[0].Elements.Length; i++)
					{
						UpdateCarSectionElement(currentCarSection, 0, i, p, d, s, CurrentlyVisible, TimeElapsed, ForceUpdate, EnableDamping);

						// brightness change
						if (currentCarSection.Groups[0].Elements[i].internalObject != null)
						{
							currentCarSection.Groups[0].Elements[i].internalObject.DaytimeNighttimeBlend = dnb;
						}
					}
				}

				int add = currentCarSection.CurrentAdditionalGroup + 1;
				if (add < currentCarSection.Groups.Length)
				{
					for (int i = 0; i < currentCarSection.Groups[add].Elements.Length; i++)
					{
						UpdateCarSectionElement(currentCarSection, add, i, p, d, s, CurrentlyVisible, TimeElapsed, ForceUpdate, EnableDamping);

						// brightness change
						if (currentCarSection.Groups[add].Elements[i].internalObject != null)
						{
							currentCarSection.Groups[add].Elements[i].internalObject.DaytimeNighttimeBlend = dnb;
						}
					}

					if (currentCarSection.Groups[add].TouchElements != null)
					{
						for (int i = 0; i < currentCarSection.Groups[add].TouchElements.Length; i++)
						{
							UpdateCarSectionTouchElement(currentCarSection, add, i, p, d, s, false, TimeElapsed, ForceUpdate, EnableDamping);
						}
					}
				}
				if (currentCarSection.Groups[0].Keyframes != null)
				{
					currentCarSection.Groups[0].Keyframes.Update(TrackPosition, p, d, Up, s, TimeElapsed);
				}
				if (currentCarSection.CurrentAdditionalGroup + 1 < currentCarSection.Groups.Length)
				{
					currentCarSection.Groups[currentCarSection.CurrentAdditionalGroup + 1].Keyframes?.Update(TrackPosition, p, d, Up, s, TimeElapsed);
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

		/// <summary>Updates the position of the camera relative to this car</summary>
		public void UpdateCamera(CarBase previousCar = null, double mu = 1.0)
		{
			var a2 = GetAnchor();
			var trackFollower = TrainManagerBase.Renderer.CameraTrackFollower;
			if (previousCar != null && mu < 1.0)
			{
				var a1 = previousCar.GetAnchor();
				trackFollower.WorldPosition = Vector3.CosineInterpolate(a1.worldPosition, a2.worldPosition, mu);
				trackFollower.WorldDirection = Vector3.CosineInterpolate(a1.worldDirection, a2.worldDirection, mu);
				trackFollower.WorldUp = Vector3.CosineInterpolate(a1.worldUp, a2.worldUp, mu);
				trackFollower.WorldSide = Vector3.CosineInterpolate(a1.worldSide, a2.worldSide, mu);
				double mu2 = (1.0 - Math.Cos(mu * Math.PI)) / 2.0;
				trackFollower.UpdateAbsolute(a1.trackPosition + (a2.trackPosition - a1.trackPosition) * mu2, false, false);
			}
			else
			{
				trackFollower.WorldPosition = a2.worldPosition; 
				trackFollower.WorldDirection = a2.worldDirection; 
				trackFollower.WorldUp = a2.worldUp; 
				trackFollower.WorldSide = a2.worldSide;
				trackFollower.UpdateAbsolute(a2.trackPosition, false, false);
			}
		}

		public (Vector3 worldPosition, Vector3 worldDirection, Vector3 worldUp, Vector3 worldSide, double trackPosition) GetAnchor()
		{
			Vector3 worldDirection = FrontAxle.Follower.WorldPosition - RearAxle.Follower.WorldPosition;
			worldDirection.Normalize();
			Vector3 worldSide = Vector3.Cross(Up, worldDirection);
			Vector3 worldPosition = 0.5 * (FrontAxle.Follower.WorldPosition + RearAxle.Follower.WorldPosition);
			Vector3 driverPosition = HasInteriorView ? Driver : baseTrain.Cars[baseTrain.DriverCar].Driver;
			worldPosition += worldSide * driverPosition.X + Up * driverPosition.Y + worldDirection * driverPosition.Z;
			double interpolationRatio = (Driver.Z - RearAxle.Position) / (FrontAxle.Position - RearAxle.Position);
			if (double.IsNaN(interpolationRatio))
			{
				// car with both axles at zero and a driver position of zero creates NaN (guarded against in original BVE parser)
				interpolationRatio = 0;
			}
			double trackPosition = (1.0 - interpolationRatio) * RearAxle.Follower.TrackPosition + interpolationRatio * FrontAxle.Follower.TrackPosition;
			return (worldPosition, worldDirection, new Vector3(Up), worldSide, trackPosition);
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
			Specs.DoorOpenPitch = Math.Exp(f * Math.Tan(g * (TrainManagerBase.currentHost.Random.NextDouble() - 0.5)));
			Specs.DoorClosePitch = Math.Exp(f * Math.Tan(g * (TrainManagerBase.currentHost.Random.NextDouble() - 0.5)));
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
