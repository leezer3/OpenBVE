// ╔═════════════════════════════════════════════════════════════╗
// ║ TrainManager.cs for the Route Viewer                        ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using TrainManager;
using TrainManager.Handles;
using TrainManager.Trains;

namespace OpenBve {
	using System;

	internal class TrainManager : TrainManagerBase {

		// train
		internal class Train : TrainBase {
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
		
		// trains
		internal static Train[] Trains = new Train[] { };
		internal static Train PlayerTrain = new Train();
	}
}
