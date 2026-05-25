// ╔═════════════════════════════════════════════════════════════╗
// ║ TrainManager.cs for the Route Viewer                        ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using LibRender2;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Trains;
using TrainManager;
using TrainManager.Handles;
using TrainManager.Trains;

namespace RouteViewer {

	internal class TrainManager : TrainManagerBase {

		public TrainManager(HostInterface host, BaseRenderer renderer, BaseOptions options, FileSystem fileSystem) : base(host, renderer, options, fileSystem)
		{
		}

		// train
		internal class Train : TrainBase {
			internal Train() : base(TrainState.Pending, TrainType.LocalPlayerTrain)
			{
				Handles.Power = new PowerHandle(8, this);
				Handles.Brake = new BrakeHandle(8, null, this);
				Handles.HoldBrake = new HoldBrakeHandle(this);
			}
			public override int NumberOfCars => this.Cars.Length;

			public override double FrontCarTrackPosition => Cars[0].FrontAxle.Follower.TrackPosition - Cars[0].FrontAxle.Position + 0.5 * Cars[0].Length;

			public override double RearCarTrackPosition => Cars[Cars.Length - 1].RearAxle.Follower.TrackPosition - Cars[Cars.Length - 1].RearAxle.Position - 0.5 * Cars[Cars.Length - 1].Length;

			public override bool IsPlayerTrain => true;
		}
	}
}
