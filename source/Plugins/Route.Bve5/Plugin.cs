//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, S520, The OpenBVE Project
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
		    if (trainManagerReference is TrainManagerBase trainManagerBase)
		    {
			    TrainManager = trainManagerBase;
		    }

		    Bve5ScenarioParser.plugin = this;
	    }

	    public override bool CanLoadRoute(string path)
	    {
		    return Bve5ScenarioParser.IsBve5(path);
	    }
		
	    public override bool LoadRoute(string path, Encoding textEncoding, string trainPath, string objectPath, string soundPath, bool previewOnly, ref object route)
	    {
		    LastException = null;
		    Cancel = false;
		    CurrentProgress = 0.0;
		    IsLoading = true;
		    FileSystem.AppendToLogFile("Loading route file: " + path);
		    FileSystem.AppendToLogFile("INFO: Route file hash " + Path.GetChecksum(path));
		    CurrentRoute = (CurrentRoute)route;
		    try
		    {
			    Bve5ScenarioParser.ParseScenario(path, previewOnly);
			    route = CurrentRoute;
		    }
		    catch(Exception ex)
		    {
			    LastException = ex;
			    return false;
		    }

		    return true;

	    }
    }
}
