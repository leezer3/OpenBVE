using System.Drawing;
using System.Text;
using LibRender2;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Trains;

namespace Train.MsTs
{
	public class Plugin : TrainInterface
	{
		internal static ConsistParser ConsistParser;

		internal static WagonParser WagonParser;

		internal static HostInterface currentHost;

		internal static BaseRenderer Renderer;
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
			return string.Empty;
		}

		public override Image GetImage(string trainPath)
		{
			return new Bitmap(0, 0);
		}
	}
}
