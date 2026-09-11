//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Christopher Lees, The OpenBVE Project
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
using System.Text;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using RouteManager2;
using RouteManager2.Stations;

namespace LokSimRouteParser
{
	public class Plugin : RouteInterface
	{
		internal static HostInterface CurrentHost;

		internal static CurrentRoute CurrentRoute;

		internal static FileSystem FileSystem;

		internal static Dictionary<string, UnifiedObject> ObjectCache;

		internal static BaseOptions BaseOptions;

		internal static bool previewOnly;

		internal static Dictionary<Guid, int> TrackKeys;

		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		/// <param name="fileSystem"></param>
		/// <param name="Options"></param>
		/// <param name="rendererReference"></param>
		public override void Load(HostInterface host, FileSystem fileSystem, BaseOptions Options, object rendererReference)
		{
			CurrentHost = host;
			FileSystem = fileSystem;
			ObjectCache = new Dictionary<string, UnifiedObject>();
			BaseOptions = Options;
		}

		public override bool CanLoadRoute(string path)
		{
			return path.ToLowerInvariant().EndsWith(".l3dfpl");
		}

		public override bool LoadRoute(string path, Encoding Encoding, string trainPath, string objectPath, string soundPath, bool PreviewOnly, ref object route)
		{
			LastException = null;
			Cancel = false;
			CurrentProgress = 0.0;
			IsLoading = true;
			FileSystem.AppendToLogFile("Loading route file: " + path);
			CurrentRoute = (CurrentRoute)route;
			CurrentRoute.Stations = new RouteStation[] { };
			FileSystem.AppendToLogFile("Route file format is: LokSim3D");
			previewOnly = PreviewOnly;
			BaseOptions.ObjectDisposalMode = ObjectDisposalMode.QuadTree;
			BaseOptions.ViewingDistance = 10000;
			TrackKeys = new Dictionary<Guid, int>();
			try
			{
				Fahrplan Fahrplan = new Fahrplan(path);
				IsLoading = false;
				return true;
			}
			catch(Exception ex)
			{
				route = null;
				CurrentHost.AddMessage(MessageType.Error, false, "An unexpected error occured whilst attempting to load the following LokSim3D routefile: " + path);
				IsLoading = false;
				LastException = ex;
				return false;
			}
		}
	}
}
