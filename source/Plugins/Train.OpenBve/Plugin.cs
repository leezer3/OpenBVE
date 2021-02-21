using System;
using System.Text;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Trains;

namespace OpenBveTrainParser
{
    public class Plugin : TrainInterface
    {
	    internal static HostInterface currentHost;

	    internal static FileSystem FileSystem;

	    public override bool CanLoadTrain(string path)
	    {
		    return false;
	    }

	    public override bool LoadTrain(string path, Encoding Encoding, string trainPath, ref object train)
	    {
		    throw new NotImplementedException();
	    }
    }
}
