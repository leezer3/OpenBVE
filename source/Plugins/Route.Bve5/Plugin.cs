using System;
using System.IO;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using RouteManager2;

namespace Bve5RouteParser
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

	    /// <summary>Checks whether the plugin can load the specified route.</summary>
	    /// <param name="path">The path to the file or folder that contains the route.</param>
	    /// <returns>Whether the plugin can load the specified sound.</returns>
	    public override bool CanLoadRoute(string path)
	    {
		    using (StreamReader reader = new StreamReader(path))
		    {
			    var firstLine = reader.ReadLine() ?? "";
			    string b = String.Empty;
			    if (!firstLine.ToLowerInvariant().StartsWith("bvets scenario"))
			    {
				    return false;
			    }

			    for (int i = 15; i < firstLine.Length; i++)
			    {
				    if (Char.IsDigit(firstLine[i]) || firstLine[i] == '.')
				    {
					    b += firstLine[i];
				    }
				    else
				    {
					    break;
				    }
			    }

			    if (b.Length > 0)
			    {
				    double version = 0;
				    NumberFormats.TryParseDoubleVb6(b, out version);
				    if (version > 2.0)
				    {
					    throw new Exception(version + " is not a supported BVE5 scenario version");
				    }
			    }
			    else
			    {
				    return false;
			    }
		    }
			return true;
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
		    Cancel = false;
		    CurrentProgress = 0.0;
		    IsLoading = true;
		    FileSystem.AppendToLogFile("Loading BVE5 route file: " + path);
		    CurrentRoute = (CurrentRoute)route;
		    try
		    {
				IsLoading = false;
				Parser parser = new Parser();
				parser.ParseRoute(path, Encoding, trainPath, objectPath, soundPath, PreviewOnly, this);
			    return true;
		    }
		    catch
		    {
			    route = null;
			    CurrentHost.AddMessage(MessageType.Error, false, "An unexpected error occured whilst attempting to load the following routefile: " + path);
			    IsLoading = false;
			    return false;
		    }
	    }

    }
}
