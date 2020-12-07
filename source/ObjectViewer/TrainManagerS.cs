// ╔═════════════════════════════════════════════════════════════╗
// ║ TrainManager.cs for the Object Viewer                       ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using OpenBveApi.Trains;
using TrainManager;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Power;
using TrainManager.SafetySystems;

namespace OpenBve {
	internal class TrainManager : TrainManagerBase {

// Silence the absurd amount of unused variable warnings
#pragma warning disable 0649
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
			internal CarBrake CarBrake;

			internal Car(Train train)
			{
				FrontAxle = new Axle(Program.CurrentHost, train, this);
				RearAxle = new Axle(Program.CurrentHost, train, this);
				CarBrake = new ElectromagneticStraightAirBrake(EletropneumaticBrakeType.None, train.Specs.CurrentEmergencyBrake, train.Specs.CurrentReverser, true, 0.0, 0.0, new AccelerationCurve[] {});
				CarBrake.mainReservoir = new MainReservoir(690000.0);
				CarBrake.brakePipe = new BrakePipe(690000.0);
				CarBrake.brakeCylinder = new BrakeCylinder(0.0);
				CarBrake.straightAirPipe = new StraightAirPipe(690000.0);
			}
		}
		// train security
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
			internal PowerHandle CurrentPowerNotch;
			internal BrakeHandle CurrentBrakeNotch;
			internal EmergencyHandle CurrentEmergencyBrake;
			internal bool HasHoldBrake;
			internal HoldBrakeHandle CurrentHoldBrake;
			internal bool HasConstSpeed;
			internal bool CurrentConstSpeed;
			internal TrainSafety Safety;
			internal AirBrakeHandle AirBrake;
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

			internal Train()
			{
				Specs.CurrentReverser = new ReverserHandle();
				Specs.CurrentPowerNotch = new PowerHandle(8, 8, new double[] {}, new double[] {});
				Specs.CurrentBrakeNotch = new BrakeHandle(8, 8, null, new double[] {}, new double[] {});
				Specs.CurrentEmergencyBrake = new EmergencyHandle();
				Specs.AirBrake = new AirBrakeHandle();
				Specs.CurrentHoldBrake = new HoldBrakeHandle();
			}
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
