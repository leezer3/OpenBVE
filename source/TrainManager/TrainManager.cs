using System;
using LibRender2;
using OpenBveApi.Hosts;
using RouteManager2;
using TrainManager.Trains;

namespace TrainManager
{
	/// <summary>The base train manager class</summary>
	public abstract class TrainManagerBase
	{
		public static HostInterface currentHost;
		public static BaseRenderer Renderer;
		public static CurrentRoute CurrentRoute;
		public static bool Toppling;
		public static bool Derailments;
		internal static Random RandomNumberGenerator = new Random();

		public static TrainBase[] Trains;
	}
}
