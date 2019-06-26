using System.Linq;

namespace MotorSoundEditor.Parsers.Train
{
	internal static partial class TrainDat
	{
		/// <summary>
		/// The Acceleration section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
		/// </summary>
		internal class Acceleration
		{
			internal struct Entry
			{
				internal double a0;
				internal double a1;
				internal double v1;
				internal double v2;
				internal double e;
			}

			internal Entry[] Entries;

			internal Acceleration()
			{
				const int n = 8;
				Entries = Enumerable.Repeat(new Entry { a0 = 1.0, a1 = 1.0, v1 = 25.0, v2 = 25.0, e = 1.0 }, n).ToArray();
			}
		}

		/// <summary>
		/// The Performance section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
		/// </summary>
		internal class Performance
		{
			internal double Deceleration;
			internal double CoefficientOfStaticFriction;
			internal double CoefficientOfRollingResistance;
			internal double AerodynamicDragCoefficient;

			internal Performance()
			{
				Deceleration = 1.0;
				CoefficientOfStaticFriction = 0.35;
				CoefficientOfRollingResistance = 0.0025;
				AerodynamicDragCoefficient = 1.2;
			}
		}

		/// <summary>
		/// The Delay section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
		/// </summary>
		internal class Delay
		{
			internal double[] DelayPowerUp;
			internal double[] DelayPowerDown;
			internal double[] DelayBrakeUp;
			internal double[] DelayBrakeDown;
			internal double[] DelayLocoBrakeUp;
			internal double[] DelayLocoBrakeDown;

			internal Delay()
			{
				DelayPowerUp = Enumerable.Repeat(0.0, 8).ToArray();
				DelayPowerDown = Enumerable.Repeat(0.0, 8).ToArray();
				DelayBrakeUp = Enumerable.Repeat(0.0, 8).ToArray();
				DelayBrakeDown = Enumerable.Repeat(0.0, 8).ToArray();
				DelayLocoBrakeUp = Enumerable.Repeat(0.0, 8).ToArray();
				DelayLocoBrakeDown = Enumerable.Repeat(0.0, 8).ToArray();
			}
		}

		/// <summary>
		/// The Move section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
		/// </summary>
		internal class Move
		{
			internal double JerkPowerUp;
			internal double JerkPowerDown;
			internal double JerkBrakeUp;
			internal double JerkBrakeDown;
			internal double BrakeCylinderUp;
			internal double BrakeCylinderDown;

			internal Move()
			{
				JerkPowerUp = 1000.0;
				JerkPowerDown = 1000.0;
				JerkBrakeUp = 1000.0;
				JerkBrakeDown = 1000.0;
				BrakeCylinderUp = 300.0;
				BrakeCylinderDown = 200.0;
			}
		}

		/// <summary>
		/// The Brake section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
		/// </summary>
		internal class Brake
		{
			internal enum BrakeTypes
			{
				ElectromagneticStraightAirBrake = 0,
				ElectricCommandBrake = 1,
				AutomaticAirBrake = 2
			}

			internal enum LocoBrakeTypes
			{
				NotFitted = 0,
				NotchedAirBrake = 1,
				AutomaticAirBrake = 2
			}

			internal enum BrakeControlSystems
			{
				None = 0,
				ClosingElectromagneticValve = 1,
				DelayIncludingSystem = 2
			}

			internal BrakeTypes BrakeType;
			internal LocoBrakeTypes LocoBrakeType;
			internal BrakeControlSystems BrakeControlSystem;
			internal double BrakeControlSpeed;

			internal Brake()
			{
				BrakeType = BrakeTypes.ElectromagneticStraightAirBrake;
				LocoBrakeType = LocoBrakeTypes.NotFitted;
				BrakeControlSystem = BrakeControlSystems.None;
				BrakeControlSpeed = 0.0;
			}
		}

		/// <summary>
		/// The Pressure section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
		/// </summary>
		internal class Pressure
		{
			internal double BrakeCylinderServiceMaximumPressure;
			internal double BrakeCylinderEmergencyMaximumPressure;
			internal double MainReservoirMinimumPressure;
			internal double MainReservoirMaximumPressure;
			internal double BrakePipeNormalPressure;

			internal Pressure()
			{
				BrakeCylinderServiceMaximumPressure = 480.0;
				BrakeCylinderEmergencyMaximumPressure = 480.0;
				MainReservoirMinimumPressure = 690.0;
				MainReservoirMaximumPressure = 780.0;
				BrakePipeNormalPressure = 490.0;
			}
		}

		/// <summary>
		/// The Handle section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
		/// </summary>
		internal class Handle
		{
			internal enum HandleTypes
			{
				Separate = 0,
				Combined = 1
			}

			internal enum EbHandleBehaviour
			{
				NoAction = 0,
				PowerNeutral = 1,
				ReverserNeutral = 2,
				PowerReverserNeutral = 3
			}

			internal enum LocoBrakeType
			{
				Combined = 0,
				Independent = 1,
				Blocking = 2
			}

			internal HandleTypes HandleType;
			internal int PowerNotches;
			internal int BrakeNotches;
			internal int PowerNotchReduceSteps;
			internal EbHandleBehaviour HandleBehaviour;
			internal LocoBrakeType LocoBrake;
			internal int LocoBrakeNotches;
			internal int DriverPowerNotches;
			internal int DriverBrakeNotches;

			internal Handle()
			{
				HandleType = HandleTypes.Separate;
				PowerNotches = 8;
				BrakeNotches = 8;
				PowerNotchReduceSteps = 0;
				LocoBrakeNotches = 0;
				HandleBehaviour = EbHandleBehaviour.NoAction;
				LocoBrake = LocoBrakeType.Combined;
				DriverPowerNotches = 8;
				DriverBrakeNotches = 8;
			}
		}

		/// <summary>
		/// The Cab section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
		/// </summary>
		internal class Cab
		{
			internal double X;
			internal double Y;
			internal double Z;
			internal double DriverCar;

			internal Cab()
			{
				X = 0.0;
				Y = 0.0;
				Z = 0.0;
				DriverCar = 0;
			}
		}

		/// <summary>
		/// The Car section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
		/// </summary>
		internal class Car
		{
			internal double MotorCarMass;
			internal int NumberOfMotorCars;
			internal double TrailerCarMass;
			internal int NumberOfTrailerCars;
			internal double LengthOfACar;
			internal bool FrontCarIsAMotorCar;
			internal double WidthOfACar;
			internal double HeightOfACar;
			internal double CenterOfGravityHeight;
			internal double ExposedFrontalArea;
			internal double UnexposedFrontalArea;

			internal Car()
			{
				MotorCarMass = 40.0;
				NumberOfMotorCars = 1;
				TrailerCarMass = 40.0;
				NumberOfTrailerCars = 1;
				LengthOfACar = 20.0;
				FrontCarIsAMotorCar = false;
				WidthOfACar = 2.6;
				HeightOfACar = 3.2;
				CenterOfGravityHeight = 1.5;
				ExposedFrontalArea = 5.0;
				UnexposedFrontalArea = 1.6;
			}
		}

		/// <summary>
		/// The Device section of the train.dat. All members are stored in the unit as specified by the train.dat documentation.
		/// </summary>
		internal class Device
		{
			internal enum AtsModes
			{
				None = -1,
				AtsSn = 0,
				AtsSnP = 1
			}

			internal enum AtcModes
			{
				None = 0,
				Manual = 1,
				Automatic = 2
			}

			internal enum ReAdhesionDevices
			{
				None = -1,
				TypeA = 0,
				TypeB = 1,
				TypeC = 2,
				TypeD = 3
			}

			internal enum PassAlarmModes
			{
				None = 0,
				Single = 1,
				Looping = 2
			}

			internal enum DoorModes
			{
				SemiAutomatic = 0,
				Automatic = 1,
				Manual = 2
			}

			internal AtsModes Ats;
			internal AtcModes Atc;
			internal bool Eb;
			internal bool ConstSpeed;
			internal bool HoldBrake;
			internal ReAdhesionDevices ReAdhesionDevice;
			internal double LoadCompensatingDevice;
			internal PassAlarmModes PassAlarm;
			internal DoorModes DoorOpenMode;
			internal DoorModes DoorCloseMode;
			internal double DoorWidth;
			internal double DoorMaxTolerance;

			internal Device()
			{
				Ats = AtsModes.AtsSn;
				Atc = AtcModes.None;
				Eb = false;
				ConstSpeed = false;
				HoldBrake = false;
				ReAdhesionDevice = ReAdhesionDevices.TypeA;
				LoadCompensatingDevice = 0.0;
				PassAlarm = PassAlarmModes.None;
				DoorOpenMode = DoorModes.SemiAutomatic;
				DoorCloseMode = DoorModes.SemiAutomatic;
				DoorWidth = 1000.0;
				DoorMaxTolerance = 0.0;
			}
		}

		internal class Motor
		{
			internal struct Entry
			{
				internal int SoundIndex;
				internal double Pitch;
				internal double Volume;
			}

			internal Entry[] Entries;

			internal Motor()
			{
				const int n = 800;
				Entries = Enumerable.Repeat(new Entry { SoundIndex = -1, Pitch = 100.0, Volume = 128.0 }, n).ToArray();
			}
		}

		/// <summary>
		/// The representation of the train.dat.
		/// </summary>
		internal class Train
		{
			internal Acceleration Acceleration;
			internal Performance Performance;
			internal Delay Delay;
			internal Move Move;
			internal Brake Brake;
			internal Pressure Pressure;
			internal Handle Handle;
			internal Cab Cab;
			internal Car Car;
			internal Device Device;
			internal Motor MotorP1;
			internal Motor MotorP2;
			internal Motor MotorB1;
			internal Motor MotorB2;

			internal Train()
			{
				Acceleration = new Acceleration();
				Performance = new Performance();
				Delay = new Delay();
				Move = new Move();
				Brake = new Brake();
				Pressure = new Pressure();
				Handle = new Handle();
				Cab = new Cab();
				Car = new Car();
				Device = new Device();
				MotorP1 = new Motor();
				MotorP2 = new Motor();
				MotorB1 = new Motor();
				MotorB2 = new Motor();
			}
		}
	}
}
