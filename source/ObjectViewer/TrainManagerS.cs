// ╔═════════════════════════════════════════════════════════════╗
// ║ TrainManager.cs for the Object Viewer                       ║
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

namespace OpenBve {
	internal class TrainManager : TrainManagerBase {
		
		public TrainManager(HostInterface host, BaseRenderer renderer, BaseOptions options, FileSystem fileSystem) : base(host, renderer, options, fileSystem)
		{
		}

		internal class Train : TrainBase
		{

			internal bool SafetySystemPlugin;

			internal Train() : base(TrainState.Available)
			{
				Handles.Reverser = new ReverserHandle();
				Handles.Power = new PowerHandle(8, 8, new double[] {}, new double[] {});
				Handles.Brake = new BrakeHandle(8, 8, null, new double[] {}, new double[] {});
				Handles.EmergencyBrake = new EmergencyHandle();
				Handles.HoldBrake = new HoldBrakeHandle();
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
	}
}
