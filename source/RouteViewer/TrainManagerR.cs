// ╔═════════════════════════════════════════════════════════════╗
// ║ TrainManager.cs for the Route Viewer                        ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using OpenBveApi.Math;
using OpenBveApi.Trains;
using SoundManager;
using TrainManager;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.Power;

namespace OpenBve {
	using System;

	internal class TrainManager : TrainManagerBase {

// Silence the absurd amount of unused variable warnings
#pragma warning disable 0649
		
		// cars
		internal struct CarSpecs {
			internal bool IsMotorCar;
			internal double CurrentPerceivedSpeed;
			internal double CurrentAcceleration;
			internal double CurrentAccelerationOutput;
		}
		
		internal class Car : AbstractCar {
			internal CarSpecs Specs;
			internal CarBrake CarBrake;
			internal readonly Door[] Doors;
			internal Car(Train train)
			{
				FrontAxle = new Axle(Program.CurrentHost, train, this);
				RearAxle = new Axle(Program.CurrentHost, train, this);
				CarBrake = new ElectromagneticStraightAirBrake(EletropneumaticBrakeType.None, train.Handles.EmergencyBrake, train.Handles.Reverser, true, 0.0, 0.0, new AccelerationCurve[] {});
				CarBrake.mainReservoir = new MainReservoir(690000.0);
				CarBrake.brakePipe = new BrakePipe(690000.0);
				CarBrake.brakeCylinder = new BrakeCylinder(0.0);
				CarBrake.straightAirPipe = new StraightAirPipe(690000.0);
				Doors = new Door[2];
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

		internal struct TrainSpecs {
			internal bool HasConstSpeed;
			internal bool CurrentConstSpeed;
		}
		// train
		internal class Train : AbstractTrain {
			internal Car[] Cars;
			internal CabHandles Handles;
			internal TrainSpecs Specs;
			internal Train()
			{
				Handles.Reverser = new ReverserHandle();
				Handles.EmergencyBrake = new EmergencyHandle();
				Handles.Power = new PowerHandle(8, 8, new double[] {}, new double[] {});
				Handles.Brake = new BrakeHandle(8, 8, null, new double[] {}, new double[] {});
			}
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
