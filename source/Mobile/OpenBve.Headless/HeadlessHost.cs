// Minimal headless host implementation of the OpenBVE API.
// Used to verify that the simulation core loads content on .NET 8 (the runtime
// used by the future net8.0-android / net8.0-ios builds) without any desktop UI,
// GDI or OpenGL context.

using System;
using System.Collections.Generic;
using LibRender2;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using RouteManager2;
using TrainManager;

namespace OpenBve.Headless
{
	/// <summary>A host which reports everything to the console and has no UI.</summary>
	internal class HeadlessHost : HostInterface
	{
		public int ErrorCount;

		/// <summary>The route currently loaded into the simulation.</summary>
		internal CurrentRoute CurrentRoute;

		public HeadlessHost() : base(HostApplication.OpenBve)
		{
			Plugins = new ContentLoadingPlugin[0];
		}

		/// <summary>Tracks are stored on the route; the desktop host bridges them the same way.</summary>
		public override Dictionary<int, Track> Tracks
		{
			get => CurrentRoute?.Tracks;
			set
			{
				if (CurrentRoute != null)
				{
					CurrentRoute.Tracks = value;
				}
			}
		}

		public override AbstractTrain ParseTrackFollowingObject(string objectPath, string tfoFile)
		{
			AddMessage(MessageType.Error, false, "Track following objects are not supported by the headless host.");
			return null;
		}

		public override void AddMessage(MessageType type, bool fileNotFound, string text)
		{
			if (type == MessageType.Error || type == MessageType.Critical)
			{
				ErrorCount++;
			}
			Console.WriteLine("[" + type + "] " + text);
		}

		public override void ReportProblem(ProblemType type, string text)
		{
			Console.WriteLine("[Problem:" + type + "] " + text);
		}
	}

	/// <summary>Options stub; nothing is persisted by the headless harness.</summary>
	internal class HeadlessOptions : BaseOptions
	{
		public override void Save(string fileName)
		{
		}
	}

	/// <summary>Renderer stub; the headless harness never touches the GPU.</summary>
	internal class HeadlessRenderer : BaseRenderer
	{
		public HeadlessRenderer(HostInterface host, BaseOptions options, FileSystem fileSystem) : base(host, options, fileSystem)
		{
		}
	}

	/// <summary>Train manager with no desktop extras.</summary>
	internal class HeadlessTrainManager : TrainManagerBase
	{
		public HeadlessTrainManager(HostInterface host, BaseRenderer renderer, BaseOptions options, FileSystem fileSystem) : base(host, renderer, options, fileSystem)
		{
		}
	}
}