using System;
using System.IO;
using System.Threading;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using RouteManager2;

namespace MechanikRouteParser
{
    public class Plugin : RouteInterface
    {
	    internal static HostInterface CurrentHost;

	    internal static CurrentRoute CurrentRoute;

	    internal static Random RandomNumberGenerator = new Random();

	    internal static FileSystem FileSystem;

	    internal static BaseOptions CurrentOptions;

	    /// <summary>Called when the plugin is loaded.</summary>
	    /// <param name="host">The host that loaded the plugin.</param>
	    /// <param name="fileSystem"></param>
	    /// <param name="Options"></param>
	    /// <param name="rendererReference"></param>
	    public override void Load(HostInterface host, FileSystem fileSystem, BaseOptions Options, object rendererReference)
	    {
		    CurrentHost = host;
		    FileSystem = fileSystem;
		    CurrentOptions = Options;
	    }

	    public override void Unload()
	    {
		    Cancel = true;
		    while (IsLoading)
		    {
			    Thread.Sleep(100);
		    }
	    }

	    /// <summary>Checks whether the plugin can load the specified route.</summary>
	    /// <param name="path">The path to the file or folder that contains the route.</param>
	    /// <returns>Whether the plugin can load the specified route.</returns>
	    public override bool CanLoadRoute(string path)
	    {
		    if (path.EndsWith(".dat", StringComparison.InvariantCultureIgnoreCase))
		    {
			    return true;
		    }
			return false;
	    }

	    /// <summary>Loads the specified route.</summary>
	    /// <param name="path">The path to the file or folder that contains the route.</param>
	    /// <param name="Encoding">The user-selected encoding (if appropriate)</param>
	    /// <param name="trainPath">The path to the selected train</param>
	    /// <param name="objectPath">The base object folder path</param>
	    /// <param name="soundPath">The base sound folder path</param>
	    /// <param name="PreviewOnly">Whether this is a preview</param>
	    /// <param name="route">Receives the route.</param>
	    /// <returns>Whether loading the sound was successful.</returns>
	    public override bool LoadRoute(string path, System.Text.Encoding Encoding, string trainPath, string objectPath, string soundPath, bool PreviewOnly, ref object route)
	    {
		    LastException = null;
		    Cancel = false;
		    CurrentProgress = 0.0;
		    IsLoading = true;
		    FileSystem.AppendToLogFile("Loading route file: " + path);
		    CurrentRoute = (CurrentRoute)route;
		    FileSystem.AppendToLogFile("Route file format is: Mechanik");
		    try
		    {
				Parser parser = new Parser();
				parser.ParseRoute(path);
				IsLoading = false;
			    return true;
		    }
		    catch(Exception ex)
		    {
			    route = null;
			    CurrentHost.AddMessage(MessageType.Error, false, "An unexpected error occured whilst attempting to load the following routefile: " + path);
			    IsLoading = false;
			    LastException = ex;
			    return false;
		    }
	    }

    }
}
