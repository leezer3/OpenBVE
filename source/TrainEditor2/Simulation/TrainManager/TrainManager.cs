using System.Collections.Generic;
using LibRender2;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using TrainEditor2.Models.Sounds;
using TrainManager;

namespace TrainEditor2.Simulation.TrainManager
{
	public partial class TrainManager : TrainManagerBase
	{
		/// <summary>A reference to the train of the Trains element that corresponds to the player's train.</summary>
		internal static Train PlayerTrain = null;

		internal static List<RunElement> RunSounds = new List<RunElement>();
		internal static List<MotorElement> MotorSounds = new List<MotorElement>();

		public TrainManager(HostInterface host, BaseRenderer renderer, BaseOptions Options, FileSystem fileSystem) : base(host, renderer, Options, fileSystem)
		{
		}
	}
}
