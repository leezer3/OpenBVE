using System.IO;
using System.Text;
using LibRender2;
using Microsoft.Win32;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Trains;
using TrainManager.Motor;
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
				if (!string.IsNullOrEmpty(FileSystem.MSTSDirectory))
				{
					return;
				}
				FileSystem.MSTSDirectory = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft Games\\Train Simulator\\1.0", "Path", string.Empty);
				string OrTsPath = (string)Registry.GetValue("HKEY_CURRENT_USER\\Software\\OpenRails\\ORTS\\Folders", "Train Simulator", string.Empty);
				if (!string.IsNullOrEmpty(OrTsPath))
				{
					FileSystem.MSTSDirectory = OrTsPath;
				}
			}
			catch
			{
				// ignored
			}
		}

		public override bool CanLoadTrain(string path)
		{
			return File.Exists(path) && path.ToLowerInvariant().EndsWith(".con");
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
				if (string.IsNullOrEmpty(trainBase.Cars[train.DriverCar].Description))
				{
					// attempt to find description of at least an engine
					for (int i = 0; i < trainBase.Cars.Length; i++)
					{
						if(!string.IsNullOrEmpty(trainBase.Cars[i].Description) && trainBase.Cars[i].TractionModel.ProvidesPower)
						{
							// return description of first engine which has one
							return trainBase.Cars[i].Description;
						}
					}
				}
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
