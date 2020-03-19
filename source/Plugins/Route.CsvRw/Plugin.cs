using System;
using LibRender2;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using RouteManager2;

namespace OpenBve
{
    public class Program : RouteInterface
    {
	    public static HostInterface CurrentHost;

	    public static CurrentRoute CurrentRoute;

	    public static Random RandomNumberGenerator = new Random();

	    public static FileSystem FileSystem;

	    public static bool EnableBveTsHacks;

	    public static BaseOptions CurrentOptions;

	    public static BaseRenderer Renderer;

	    /// <summary>Called when the plugin is loaded.</summary>
	    /// <param name="host">The host that loaded the plugin.</param>
	    /// <param name="fileSystem"></param>
	    /// <param name="Options"></param>
	    public override void Load(HostInterface host, FileSystem fileSystem, BaseOptions Options)
	    {
		    CurrentHost = host;
		    FileSystem = fileSystem;
		    CurrentOptions = Options;
	    }

	    /// <summary>Checks whether the plugin can load the specified route.</summary>
	    /// <param name="path">The path to the file or folder that contains the route.</param>
	    /// <returns>Whether the plugin can load the specified sound.</returns>
	    public override bool CanLoadRoute(string path)
	    {
		    path = path.ToLowerInvariant();
		    if (path.EndsWith(".csv") || path.EndsWith(".rw"))
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
	    public override bool LoadRoute(string path, System.Text.Encoding Encoding, string trainPath, string objectPath, string soundPath, bool PreviewOnly, out object route)
	    {
		    bool isRw = path.ToLowerInvariant().EndsWith(".csv") || path.ToLowerInvariant().EndsWith(".rw");
		    try
		    {
			    CsvRwRouteParser.ParseRoute(path, isRw, Encoding, trainPath, objectPath, soundPath, PreviewOnly, out route);
			    return true;
		    }
		    catch
		    {
			    route = null;
			    CurrentHost.AddMessage(MessageType.Error, false, "An unexpected error occured whilst attempting to load the following routefile: " + path);
			    return false;
		    }
	    }

    }
}
