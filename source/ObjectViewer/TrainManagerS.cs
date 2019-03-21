// ╔═════════════════════════════════════════════════════════════╗
// ║ TrainManager.cs for the Object Viewer                       ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace OpenBve {
	internal static class TrainManager {

// Silence the absurd amount of unused variable warnings
#pragma warning disable 0649

		// structures
		internal struct Axle {
			internal TrackManager.TrackFollower Follower;
		}

		// cars
		internal struct Door {
			internal int Direction;
			internal double State;
		}
		
		internal enum CarBrakeType {
			ElectromagneticStraightAirBrake = 0, //電磁直通
			ElectricCommandBrake = 1, //電気指令式
			AutomaticAirBrake = 2 //自動空気
		}
		internal enum EletropneumaticBrakeType {
			None = 0,
			ClosingElectromagneticValve = 1, //締切電磁弁
			DelayFillingControl = 2 //遅れ込め制御
		}
		internal enum AirBrakeHandleState {
			Invalid = -1,
			Release = 0,
			Lap = 1,
			Service = 2,
		}
		internal struct AirBrakeHandle {
			internal AirBrakeHandleState Driver;
			internal AirBrakeHandleState Safety;
			internal AirBrakeHandleState Actual;
			internal AirBrakeHandleState DelayedValue;
			internal double DelayedTime;
		}
		internal enum AirBrakeType { Main, Auxillary }
		internal struct CarAirBrake {
			internal AirBrakeType Type;
			internal bool AirCompressorEnabled;
			internal double AirCompressorMinimumPressure;
			internal double AirCompressorMaximumPressure;
			internal double AirCompressorRate;
			internal double MainReservoirCurrentPressure;
			internal double MainReservoirEqualizingReservoirCoefficient;
			internal double MainReservoirBrakePipeCoefficient;
			internal double EqualizingReservoirCurrentPressure;
			internal double EqualizingReservoirNormalPressure;
			internal double EqualizingReservoirServiceRate;
			internal double EqualizingReservoirEmergencyRate;
			internal double EqualizingReservoirChargeRate;
			internal double BrakePipeCurrentPressure;
			internal double BrakePipeNormalPressure;
			internal double BrakePipeChargeRate;
			internal double BrakePipeServiceRate;
			internal double BrakePipeEmergencyRate;
			internal double AuxillaryReservoirCurrentPressure;
			internal double AuxillaryReservoirMaximumPressure;
			internal double AuxillaryReservoirChargeRate;
			internal double AuxillaryReservoirBrakePipeCoefficient;
			internal double AuxillaryReservoirBrakeCylinderCoefficient;
			internal double BrakeCylinderCurrentPressure;
			internal double BrakeCylinderEmergencyMaximumPressure;
			internal double BrakeCylinderServiceMaximumPressure;
			internal double BrakeCylinderEmergencyChargeRate;
			internal double BrakeCylinderServiceChargeRate;
			internal double BrakeCylinderReleaseRate;
			internal double BrakeCylinderSoundPlayedForPressure;
			internal double StraightAirPipeCurrentPressure;
			internal double StraightAirPipeReleaseRate;
			internal double StraightAirPipeServiceRate;
			internal double StraightAirPipeEmergencyRate;
		}
		internal struct CarHoldBrake {
			internal double CurrentAccelerationOutput;
			internal double NextUpdateTime;
			internal double UpdateInterval;
		}
		internal struct CarConstSpeed {
			internal double CurrentAccelerationOutput;
			internal double NextUpdateTime;
			internal double UpdateInterval;
		}
		internal struct CarReAdhesionDevice {
			internal double UpdateInterval;
			internal double ApplicationFactor;
			internal double ReleaseInterval;
			internal double ReleaseFactor;
			internal double CurrentAccelerationOutput;
			internal double NextUpdateTime;
			internal double TimeStable;
		}
		internal struct CarSpecs {
			internal bool IsMotorCar;
			internal double AccelerationCurvesMultiplier;
			internal double BrakeDecelerationAtServiceMaximumPressure;
			internal double BrakeControlSpeed;
			internal double MotorDeceleration;
			internal double Mass;
			internal double ExposedFrontalArea;
			internal double UnexposedFrontalArea;
			internal double CoefficientOfStaticFriction;
			internal double CoefficientOfRollingResistance;
			internal double AerodynamicDragCoefficient;
			internal double CenterOfGravityHeight;
			internal double CriticalTopplingAngle;
			internal double CurrentSpeed;
			internal double CurrentPerceivedSpeed;
			internal double CurrentAcceleration;
			internal double CurrentAccelerationOutput;
			internal bool CurrentMotorPower;
			internal bool CurrentMotorBrake;
			internal CarHoldBrake HoldBrake;
			internal CarConstSpeed ConstSpeed;
			internal CarReAdhesionDevice ReAdhesionDevice;
			internal CarBrakeType BrakeType;
			internal EletropneumaticBrakeType ElectropneumaticType;
			internal CarAirBrake AirBrake;
			internal Door[] Doors;
			internal double DoorOpenSpeed;
			internal double DoorCloseSpeed;
			internal bool AnticipatedLeftDoorsOpened;
			internal bool AnticipatedRightDoorsOpened;
			internal double CurrentRollDueToTopplingAngle;
			internal double CurrentRollDueToCantAngle;
			internal double CurrentRollDueToCantAngularSpeed;
			internal double CurrentRollShakeDirection;
			internal double CurrentPitchDueToAccelerationAngle;
			internal double CurrentPitchDueToAccelerationTrackPosition;
			internal double CurrentPitchDueToAccelerationSpeed;
		}
		internal struct CarBrightness {
			internal float PreviousBrightness;
			internal double PreviousTrackPosition;
			internal float NextBrightness;
			internal double NextTrackPosition;
		}
		

		internal class Car : AbstractCar {
			internal Axle FrontAxle;
			internal Axle RearAxle;
			internal double FrontAxlePosition;
			internal double RearAxlePosition;
			internal int CurrentSection;
			internal CarSpecs Specs;
			internal bool CurrentlyVisible;
			internal bool Derailed;
			internal bool Topples;
			internal CarBrightness Brightness;
		}

		// train
		internal struct HandleChange {
			internal int Value;
			internal double Time;
		}
		internal struct PowerHandle {
			internal int Driver;
			internal int Safety;
			internal int Actual;
			internal HandleChange[] DelayedChanges;
		}
		internal struct BrakeHandle {
			internal int Driver;
			internal int Safety;
			internal int Actual;
			internal HandleChange[] DelayedChanges;
		}
		internal struct EmergencyHandle {
			internal bool Driver;
			internal bool Safety;
			internal bool Actual;
			internal double ApplicationTime;
		}
		internal struct ReverserHandle {
			internal int Driver;
			internal int Actual;
		}
		internal struct HoldBrakeHandle {
			internal bool Driver;
			internal bool Actual;
		}
		// train security
		internal enum SafetyState {
			Normal = 0,
			Initialization = 1,
			Ringing = 2,
			Emergency = 3,
			Pattern = 4,
			Service = 5
		}
		internal enum SafetySystem {
			Plugin = -1,
			None = 0,
			AtsSn = 1,
			AtsP = 2,
			Atc = 3
		}
		internal struct Ats {
			internal double Time;
			internal bool AtsPAvailable;
			internal double AtsPDistance;
			internal double AtsPTemporarySpeed;
			internal double AtsPPermanentSpeed;
			internal bool AtsPOverride;
			internal double AtsPOverrideTime;
		}
		internal struct Atc {
			internal bool Available;
			internal bool Transmitting;
			internal bool AutomaticSwitch;
			internal double SpeedRestriction;
		}
		internal struct Eb {
			internal bool Available;
			internal SafetyState BellState;
			internal double Time;
			internal bool Reset;
		}
		internal struct TrainPendingTransponder {
			//internal TrackManager.TransponderType Type;
			internal bool SwitchSubsystem;
			internal int OptionalInteger;
			internal double OptionalFloat;
			internal int SectionIndex;
		}
		internal struct TrainSafety {
			internal SafetySystem Mode;
			internal SafetySystem ModeChange;
			internal SafetyState State;
			internal TrainPendingTransponder[] PendingTransponders;
			internal Ats Ats;
			internal Atc Atc;
			internal Eb Eb;
		}
		// train specs
		internal enum PassAlarmType {
			None = 0,
			Single = 1,
			Loop = 2
		}
		internal struct TrainAirBrake {
			internal AirBrakeHandle Handle;
		}
		internal struct TrainSpecs {
			internal double TotalMass;
			internal ReverserHandle CurrentReverser;
			internal double CurrentAverageSpeed;
			internal double CurrentAverageAcceleration;
			internal double CurrentAverageJerk;
			internal double CurrentAirPressure;
			internal double CurrentAirDensity;
			internal double CurrentAirTemperature;
			internal double CurrentElevation;
			internal bool SingleHandle;
			internal int PowerNotchReduceSteps;
			internal int MaximumPowerNotch;
			internal PowerHandle CurrentPowerNotch;
			internal int MaximumBrakeNotch;
			internal BrakeHandle CurrentBrakeNotch;
			internal EmergencyHandle CurrentEmergencyBrake;
			internal bool HasHoldBrake;
			internal HoldBrakeHandle CurrentHoldBrake;
			internal bool HasConstSpeed;
			internal bool CurrentConstSpeed;
			internal TrainSafety Safety;
			internal TrainAirBrake AirBrake;
			internal double DelayPowerStart;
			internal double DelayPowerStop;
			internal double DelayBrakeStart;
			internal double DelayBrakeEnd;
			internal double DelayServiceBrake;
			internal double DelayEmergencyBrake;
			internal PassAlarmType PassAlarm;
		}
		// train
		internal enum TrainStopState {
			Pending = 0, Boarding = 1, Completed = 2
		}
		internal class Train : AbstractTrain {
			internal Car[] Cars;
			internal TrainSpecs Specs;
			internal int CurrentSectionIndex;
		}

#pragma warning restore 0649

		// trains
		internal static Train[] Trains = new Train[] { };
		internal static Train PlayerTrain = new Train();

	}
}
