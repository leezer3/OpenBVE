//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using RouteManager2;
using RouteManager2.Stations;
using Path = OpenBveApi.Path;

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
		    Parser.knownRoutes = new Dictionary<string, RouteProperties>();
		    Parser.knownModules = new List<string>();
			RoutePropertiesDatabaseParser.LoadRoutePropertyDatabase(ref Parser.knownRoutes, ref Parser.knownModules);
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
		    if (!File.Exists(path))
		    {
			    return false;
		    }
			if (path.EndsWith(".dat", StringComparison.InvariantCultureIgnoreCase))
		    {
			    try
			    {
					if (Parser.knownModules.Contains(Path.GetChecksum(path)) || File.ReadLines(path).Count() < 800)
				    {
					    /*
					     * Slightly hacky check if not found in the known modules list:
					     * The original Mechanik download contained a route generator.
					     * All this did was to append various module files to generate a
					     * final route.
					     *
					     * If we have less than ~800 lines, it's not a complete route, but
					     * a module instead.
					     */
					    return false;
				    }
			    }
			    catch
			    {
					//Innacessable file?!
				    return false;
			    }
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
			CurrentRoute.Stations = new RouteStation[] { };
		    FileSystem.AppendToLogFile("Route file format is: Mechanik");
		    try
		    {
				Parser parser = new Parser();
				parser.ParseRoute(path, PreviewOnly);
				if (PreviewOnly && CurrentRoute.Stations.Length == 0)
				{
					route = null;
					CurrentHost.AddMessage(MessageType.Error, false, "No stations were found in the following Mechanik routefile: " + path + Environment.NewLine + Environment.NewLine + "This is most likely a module file.");
					IsLoading = false;
					return false;
				}
				IsLoading = false;
			    return true;
		    }
		    catch(Exception ex)
		    {
			    route = null;
			    CurrentHost.AddMessage(MessageType.Error, false, "An unexpected error occured whilst attempting to load the following Mechanik routefile: " + path);
			    IsLoading = false;
			    LastException = ex;
			    return false;
		    }
	    }

    }
}
