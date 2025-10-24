using System.IO;
using System.Text;
using LibRender2;
using Microsoft.Win32;
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

		internal static HostInterface CurrentHost;

		internal static BaseRenderer Renderer;

		internal static FileSystem FileSystem;

		internal static BaseOptions CurrentOptions;

		internal static bool PreviewOnly;
		public Plugin()
		{
			ConsistParser = new ConsistParser(this);
			WagonParser = new WagonParser();
		}

		public override void Load(HostInterface host, FileSystem fileSystem, BaseOptions options, object rendererReference)
		{
			CurrentHost = host;
			FileSystem = fileSystem;
			CurrentOptions = options;
			Renderer = (BaseRenderer) rendererReference;
			try
			{
				if (string.IsNullOrEmpty(FileSystem.MSTSDirectory))
				{
					FileSystem.MSTSDirectory = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft Games\\Train Simulator\\1.0", "Path", string.Empty);
					string OrTsPath = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\OpenRails\\ORTS\\Folders", "Train Simulator", string.Empty);
					if (!string.IsNullOrEmpty(OrTsPath))
					{
						FileSystem.MSTSDirectory = OrTsPath;
					}
				}
			}
			catch
			{
				// ignored
			}
		}

		public override bool CanLoadTrain(string path)
		{
			if (File.Exists(path) && path.ToLowerInvariant().EndsWith(".con"))
			{
				return true;
			}
			return false;
		}

		public override bool LoadTrain(Encoding encoding, string trainPath, ref AbstractTrain train, ref Control[] currentControls)
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
			AbstractTrain train = new TrainBase(TrainState.Pending, TrainType.LocalPlayerTrain);
			try
			{
				ConsistParser.ReadConsist(trainPath, ref train);
			}
			catch
			{
				return string.Empty;
			}
			
			if(train is TrainBase trainBase && trainBase.Cars.Length != 0)
			{
				return trainBase.Cars[train.DriverCar].Description;
			}
			return string.Empty;
		}

		public override string GetImage(string trainPath)
		{
			return string.Empty;
		}
	}
}
