using System.Drawing;
using System.Text;
using LibRender2;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Trains;
using TrainManager.Trains;

namespace Train.MsTs
{
	public class Plugin : TrainInterface
	{
		internal static ConsistParser ConsistParser;

		internal static WagonParser WagonParser;

		internal static HostInterface currentHost;

		internal static BaseRenderer Renderer;

		internal static bool PreviewOnly;
		public Plugin()
		{
			ConsistParser = new ConsistParser(this);
			WagonParser = new WagonParser(this);
		}

		public override void Load(HostInterface host, FileSystem fileSystem, BaseOptions Options, object rendererReference)
		{
			currentHost = host;
			Renderer = (BaseRenderer) rendererReference;
		}

		public override bool CanLoadTrain(string path)
		{
			if (path.ToLowerInvariant().EndsWith(".con"))
			{
				return true;
			}
			return false;
		}

		public override bool LoadTrain(Encoding Encoding, string trainPath, ref AbstractTrain train, ref Control[] currentControls)
		{
			PreviewOnly = false;
			try
			{
				ConsistParser.ReadConsist(trainPath, ref train);
			}
			catch
			{
				return false;
			}
			
			return true;
		}

		public override string GetDescription(string trainPath, Encoding userSelectedEncoding = null)
		{
			PreviewOnly = true;
			AbstractTrain train = new TrainBase(TrainState.Pending);
			ConsistParser.ReadConsist(trainPath, ref train);
			TrainBase trainBase = train as TrainBase;
			if(trainBase != null && trainBase.Cars.Length != 0)
			{
				return trainBase.Cars[train.DriverCar].Description;
			}
			return string.Empty;
		}

		public override Image GetImage(string trainPath)
		{
			PreviewOnly = true;
			return new Bitmap(1, 1);
		}
	}
}
