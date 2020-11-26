// ╔═════════════════════════════════════════════════════════════╗
// ║ TrainManager.cs for the Object Viewer                       ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using OpenBveApi.Trains;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.SafetySystems;

namespace OpenBve {
	internal static class TrainManager {

// Silence the absurd amount of unused variable warnings
#pragma warning disable 0649

		// cars
		internal struct AirBrakeHandle {
			internal AirBrakeHandleState Driver;
			internal AirBrakeHandleState Safety;
			internal AirBrakeHandleState Actual;
			internal AirBrakeHandleState DelayedValue;
			internal double DelayedTime;
		}
		internal struct CarAirBrake {
			internal BrakeType Type;
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
		internal struct CarSpecs {
			internal bool IsMotorCar;
			internal double AccelerationCurvesMultiplier;
			internal double BrakeDecelerationAtServiceMaximumPressure;
			internal double BrakeControlSpeed;
			internal double MotorDeceleration;
			internal double ExposedFrontalArea;
			internal double UnexposedFrontalArea;
			internal double CenterOfGravityHeight;
			internal double CriticalTopplingAngle;
			internal double CurrentPerceivedSpeed;
			internal double CurrentAcceleration;
			internal double CurrentAccelerationOutput;
			internal bool CurrentMotorPower;
			internal bool CurrentMotorBrake;
			internal CarHoldBrake HoldBrake;
			internal CarConstSpeed ConstSpeed;
			internal BrakeSystemType BrakeType;
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

		internal class Car : AbstractCar {
			internal int CurrentSection;
			internal CarSpecs Specs;
			internal bool CurrentlyVisible;
			internal bool Derailed;
			internal bool Topples;

			internal Car(Train train)
			{
				FrontAxle = new Axle(Program.CurrentHost, train, this);
				RearAxle = new Axle(Program.CurrentHost, train, this);
			}
		}

		// train
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

		internal struct TrainSafety {
			internal SafetySystem Mode;
		}
		// train specs
		internal struct TrainAirBrake {
			internal AirBrakeHandle Handle;
		}
		internal struct TrainSpecs {
			internal double TotalMass;
			internal ReverserHandle CurrentReverser;
			internal double CurrentAverageAcceleration;
			internal double CurrentAverageJerk;
			internal double CurrentAirPressure;
			internal double CurrentAirDensity;
			internal double CurrentAirTemperature;
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
		internal class Train : AbstractTrain {
			internal Car[] Cars;
			internal TrainSpecs Specs;
			public override double FrontCarTrackPosition()
			{
				return Cars[0].FrontAxle.Follower.TrackPosition - Cars[0].FrontAxle.Position + 0.5 * Cars[0].Length;
			}

			public override double RearCarTrackPosition()
			{
				return Cars[Cars.Length - 1].RearAxle.Follower.TrackPosition - Cars[Cars.Length - 1].RearAxle.Position - 0.5 * Cars[Cars.Length - 1].Length;
			}
		}

#pragma warning restore 0649

		// trains
		internal static Train[] Trains = new Train[] { };
		internal static Train PlayerTrain = new Train();

	}
}
