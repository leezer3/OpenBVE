// ╔═════════════════════════════════════════════════════════════╗
// ║ TrainManager.cs for the Route Viewer                        ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using SoundManager;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Motor;
using TrainManager.Power;

namespace OpenBve {
	using System;

	internal static class TrainManager {

// Silence the absurd amount of unused variable warnings
#pragma warning disable 0649
		
		// cars
		internal struct AirBrakeHandle {
			internal AirBrakeHandleState Driver;
			internal AirBrakeHandleState Security;
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
			internal AccelerationCurve[] AccelerationCurves;
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
		internal struct Horn {
			internal CarSound Sound;
			internal bool Loop;
		}
		
		internal struct CarSounds {
			internal BVEMotorSound Motor;
			internal CarSound Adjust;
			internal CarSound Air;
			internal CarSound AirHigh;
			internal CarSound AirZero;
			internal CarSound Ats;
			internal CarSound AtsCnt;
			internal CarSound Brake;
			internal CarSound BrakeHandleApply;
			internal CarSound BrakeHandleRelease;
			internal CarSound BrakeHandleMin;
			internal CarSound BrakeHandleMax;
			internal CarSound CpEnd;
			internal CarSound CpLoop;
			internal bool CpLoopStarted;
			internal CarSound CpStart;
			internal double CpStartTimeStarted;
			internal CarSound Ding;
			internal CarSound DoorCloseL;
			internal CarSound DoorCloseR;
			internal CarSound DoorOpenL;
			internal CarSound DoorOpenR;
			internal CarSound Eb;
			internal CarSound EmrBrake;
			internal CarSound[] Flange;
			internal double[] FlangeVolume;
			internal CarSound Halt;
			internal Horn[] Horns;
			internal CarSound Loop;
			internal CarSound MasterControllerUp;
			internal CarSound MasterControllerDown;
			internal CarSound MasterControllerMin;
			internal CarSound MasterControllerMax;
			internal CarSound PilotLampOn;
			internal CarSound PilotLampOff;
			internal CarSound PointFrontAxle;
			internal CarSound PointRearAxle;
			internal CarSound Rub;
			internal CarSound ReverserOn;
			internal CarSound ReverserOff;
			internal CarSound[] Run;
			internal double[] RunVolume;
			internal CarSound SpringL;
			internal CarSound SpringR;
			internal CarSound ToAtc;
			internal CarSound ToAts;
			internal CarSound[] Plugin;
			internal double FlangePitch;
			internal double SpringPlayedAngle;
		}
		internal class Car : AbstractCar {
			internal int CurrentSection;
			internal CarSpecs Specs;
			internal CarSounds Sounds;
			internal bool Derailed;
			internal bool Topples;

			internal Car(Train train)
			{
				FrontAxle = new Axle(Program.CurrentHost, train, this);
				RearAxle = new Axle(Program.CurrentHost, train, this);
			}

			public override void CreateWorldCoordinates(Vector3 Car, out Vector3 Position, out Vector3 Direction)
			{
				Direction = FrontAxle.Follower.WorldPosition - RearAxle.Follower.WorldPosition;
				double t = Direction.Norm();
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
					Position.X = FrontAxle.Follower.WorldPosition.X;
					Position.Y = FrontAxle.Follower.WorldPosition.Y;
					Position.Z = FrontAxle.Follower.WorldPosition.Z;
					Direction.X = 0.0;
					Direction.Y = 1.0;
					Direction.Z = 0.0;
				}
			}
		}
		// train specs
		internal struct TrainAirBrake {
			internal AirBrakeHandle Handle;
		}
		internal struct TrainSpecs {
			internal ReverserHandle CurrentReverser;
			internal int MaximumPowerNotch;
			internal PowerHandle CurrentPowerNotch;
			internal int MaximumBrakeNotch;
			internal BrakeHandle CurrentBrakeNotch;
			internal EmergencyHandle CurrentEmergencyBrake;
			internal bool HasHoldBrake;
			internal HoldBrakeHandle CurrentHoldBrake;
			internal bool HasConstSpeed;
			internal bool CurrentConstSpeed;
			internal TrainAirBrake AirBrake;
		}
		// train
		internal class Train : AbstractTrain {
			internal Car[] Cars;
			internal TrainSpecs Specs;

			public override int NumberOfCars
			{
				get
				{
					return this.Cars.Length;
				}
			}

			public override double FrontCarTrackPosition()
			{
				return Cars[0].FrontAxle.Follower.TrackPosition - Cars[0].FrontAxle.Position + 0.5 * Cars[0].Length;
			}

			public override double RearCarTrackPosition()
			{
				return Cars[Cars.Length - 1].RearAxle.Follower.TrackPosition - Cars[Cars.Length - 1].RearAxle.Position - 0.5 * Cars[Cars.Length - 1].Length;
			}

			public override bool IsPlayerTrain
			{
				get
				{
					return true;
				}
			}
		}

#pragma warning restore 0649

		// trains
		internal static Train[] Trains = new Train[] { };
		internal static Train PlayerTrain = new Train();
	}
}
