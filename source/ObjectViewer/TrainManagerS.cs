// ╔═════════════════════════════════════════════════════════════╗
// ║ TrainManager.cs for the Object Viewer                       ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using LibRender2;
using OpenBveApi;
using OpenBveApi.Hosts;
using TrainManager;

namespace ObjectViewer
{
	internal class TrainManager : TrainManagerBase
	{
		public TrainManager(HostInterface host, BaseRenderer renderer, BaseOptions options) : base(host, renderer, options)
		{
		}
	}
}
