using System;
using System.IO;
using System.Text;
using System.Threading;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using RouteManager2;
using TrainManager;
using Path = OpenBveApi.Path;

namespace CsvRwRouteParser
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
		    if (trainManagerReference is TrainManagerBase _base)
		    {
			    TrainManager = _base;
		    }
			CurrentOptions.TrainDownloadLocation = string.Empty;
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
		    if (string.IsNullOrEmpty(path) || !File.Exists(path))
		    {
			    return false;
		    }
		    if (path.EndsWith(".rw", StringComparison.InvariantCultureIgnoreCase))
		    {
			    return true;
		    }
			if (path.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase))
		    {
			    using (StreamReader fileReader = new StreamReader(path))
			    {
				    for (int i = 0; i < 30; i++)
				    {
					    try
					    {
						    string readLine = fileReader.ReadLine();
						    if (!string.IsNullOrEmpty(readLine) && readLine.IndexOf("meshbuilder", StringComparison.InvariantCultureIgnoreCase) != -1)
						    {
								//We have found the MeshBuilder statement within the first 30 lines
								//This must be an object (we hope)
							    return false;
						    }
					    }
					    catch
					    {
						    //ignored
					    }
						
				    }
			    }
			    return true;
		    }

		    return false;
	    }

	    /// <summary>Loads the specified route.</summary>
	    /// <param name="path">The path to the file or folder that contains the route.</param>
	    /// <param name="textEncoding">The user-selected encoding (if appropriate)</param>
	    /// <param name="trainPath">The path to the selected train</param>
	    /// <param name="objectPath">The base object folder path</param>
	    /// <param name="soundPath">The base sound folder path</param>
	    /// <param name="PreviewOnly">Whether this is a preview</param>
	    /// <param name="route">Receives the route.</param>
	    /// <returns>Whether loading the sound was successful.</returns>
	    public override bool LoadRoute(string path, Encoding textEncoding, string trainPath, string objectPath, string soundPath, bool PreviewOnly, ref object route)
	    {
		    if (textEncoding == null)
		    {
				textEncoding = Encoding.UTF8;
		    }
			CurrentOptions.TrainDownloadLocation = string.Empty;
			LastException = null;
		    Cancel = false;
		    CurrentProgress = 0.0;
		    IsLoading = true;
		    FileSystem.AppendToLogFile("Loading route file: " + path);
		    FileSystem.AppendToLogFile("INFO: Route file hash " + Path.GetChecksum(path));
		    CurrentRoute = (CurrentRoute)route;
		    //First, check the format of the route file
		    //RW routes were written for BVE1 / 2, and have a different command syntax
		    bool isRw = path.ToLowerInvariant().EndsWith(".rw");
		    FileSystem.AppendToLogFile("Route file format is: " + (isRw ? "RW" : "CSV"));
		    try
		    {
				Parser parser = new Parser();
				parser.ParseRoute(path, isRw, textEncoding, trainPath, objectPath, soundPath, PreviewOnly, this);
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
