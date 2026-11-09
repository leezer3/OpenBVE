// Minimal Android host implementation of the OpenBVE API.
//
// Messages go to logcat. The Tracks property bridges to the loaded route in the
// same way the desktop host does (see source/OpenBVE/System/Host.cs).

using System;
using System.Collections.Generic;
using Android.Util;
using LibRender2;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using OpenBveApi.Trains;
using RouteManager2;
using TrainManager;

namespace DroidBVE
{
	internal class AndroidHost : HostInterface
	{
		private const string Tag = "DroidBVE";

		/// <summary>The route currently loaded into the simulation.</summary>
		internal CurrentRoute CurrentRoute;

		/// <summary>Raised for every host message, for display in the UI.</summary>
		internal event Action<string> MessageReceived;

		public AndroidHost() : base(HostApplication.OpenBve)
		{
			Plugins = new ContentLoadingPlugin[0];
		}

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
			AddMessage(MessageType.Error, false, "Track following objects are not yet supported on Android.");
			return null;
		}

		public override void AddMessage(MessageType type, bool fileNotFound, string text)
		{
			Log.Warn(Tag, "[" + type + "] " + text);
			MessageReceived?.Invoke(text);
		}

		public override void ReportProblem(ProblemType type, string text)
		{
			Log.Warn(Tag, "[Problem:" + type + "] " + text);
		}
	}

	/// <summary>Options stub; settings persistence is TODO.</summary>
	internal class AndroidOptions : BaseOptions
	{
		public override void Save(string fileName)
		{
		}
	}

	/// <summary>
	/// Renderer stub. The OpenGL renderer (LibRender2 on top of a GLSurfaceView
	/// context) is the next major milestone, see ../../BuildingAndroid.md.
	/// </summary>
	internal class AndroidRenderer : BaseRenderer
	{
		public AndroidRenderer(HostInterface host, BaseOptions options, FileSystem fileSystem) : base(host, options, fileSystem)
		{
		}
	}

	/// <summary>Train manager with no desktop extras.</summary>
	internal class AndroidTrainManager : TrainManagerBase
	{
		public AndroidTrainManager(HostInterface host, BaseRenderer renderer, BaseOptions options, FileSystem fileSystem) : base(host, renderer, options, fileSystem)
		{
		}
	}
}