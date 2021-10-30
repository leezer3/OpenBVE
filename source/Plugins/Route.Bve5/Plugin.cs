using System;
using System.Text;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Routes;
using RouteManager2;
using TrainManager;

namespace Route.Bve5
{
    public class Plugin : RouteInterface
    {
	    internal static HostInterface CurrentHost;

	    internal static CurrentRoute CurrentRoute;

	    internal static Random RandomNumberGenerator = new Random();

	    internal static FileSystem FileSystem;

	    internal static BaseOptions CurrentOptions;

	    internal static TrainManagerBase TrainManager;

	    /// <summary>Called when the plugin is loaded.</summary>
	    /// <param name="host">The host that loaded the plugin.</param>
	    /// <param name="fileSystem"></param>
	    /// <param name="Options"></param>
	    /// <param name="trainManagerReference"></param>
	    public override void Load(HostInterface host, FileSystem fileSystem, BaseOptions Options, object trainManagerReference)
	    {
		    CurrentHost = host;
		    FileSystem = fileSystem;
		    CurrentOptions = Options;
		    if (trainManagerReference is TrainManagerBase)
		    {
			    TrainManager = (TrainManagerBase)trainManagerReference;
		    }

		    Bve5ScenarioParser.plugin = this;
	    }

	    public override bool CanLoadRoute(string path)
	    {
		    return Bve5ScenarioParser.IsBve5(path);
	    }
		
	    public override bool LoadRoute(string path, Encoding Encoding, string trainPath, string objectPath, string soundPath, bool PreviewOnly, ref object route)
	    {
		    if (Encoding == null)
		    {
			    Encoding = Encoding.UTF8;
		    }
		    LastException = null;
		    Cancel = false;
		    CurrentProgress = 0.0;
		    IsLoading = true;
		    FileSystem.AppendToLogFile("Loading route file: " + path);
		    FileSystem.AppendToLogFile("INFO: Route file hash " + Path.GetChecksum(path));
		    CurrentRoute = (CurrentRoute)route;
		    try
		    {
			    Bve5ScenarioParser.ParseScenario(path, PreviewOnly);
			    route = CurrentRoute;
		    }
		    catch
		    {
			    return false;
		    }

		    return true;

	    }
    }
}
