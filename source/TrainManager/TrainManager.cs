using System;
using LibRender2;
using OpenBveApi.Hosts;
using RouteManager2.Climate;

namespace TrainManager
{
	/// <summary>The base train manager class</summary>
	public abstract class TrainManagerBase
	{
		public static HostInterface currentHost;
		public static BaseRenderer Renderer;
		public static Atmosphere Atmosphere;
		public static bool Toppling;
		public static bool Derailments;
		internal static Random RandomNumberGenerator = new Random();
	}
}
