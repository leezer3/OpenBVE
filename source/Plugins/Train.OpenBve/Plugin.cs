using System;
using System.IO;
using System.Text;
using LibRender2;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Trains;
using Path = OpenBveApi.Path;

namespace Train.OpenBve
{
    public class Plugin : TrainInterface
    {
	    internal static HostInterface currentHost;

	    internal static FileSystem FileSystem;

	    internal static BaseOptions CurrentOptions;

	    internal static Random RandomNumberGenerator = new Random();

	    internal static BaseRenderer Renderer;

	    public override bool CanLoadTrain(string path)
	    {
		    string vehicleTxt = Path.CombineFile(path, "vehicle.txt");
		    if (File.Exists(vehicleTxt))
		    {
			    string[] lines = File.ReadAllLines(vehicleTxt);
			    for (int i = 10; i < lines.Length; i++)
			    {
				    if (lines[i].StartsWith(@"bvets vehicle ", StringComparison.InvariantCultureIgnoreCase))
				    {
						/*
						 * BVE5 format train
						 * When the BVE5 plugin is implemented, this should return false, as BVE5 trains
						 * often seem to keep the train.dat lying around and we need to use the right plugin
						 *
						 * For the moment however, this is ignored....
						 */
				    }
			    }
		    }
			string trainDat = Path.CombineFile(path, "train.dat");
			if (File.Exists(trainDat))
			{
				return true;
			}
			string trainXML = Path.CombineFile(path, "train.xml");
			if (File.Exists(trainXML))
			{
				/*
				 * XML format train
				 * At present, XML is used only as an extension, but acceleration etc. will be implemented
				 * When this is done, return true here
				 */
			}
		    return false;
	    }

	    public override bool LoadTrain(string path, Encoding Encoding, string trainPath, ref object train)
	    {
		    throw new NotImplementedException();
	    }
    }
}
