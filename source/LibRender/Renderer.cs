﻿using OpenBveApi.Hosts;

namespace LibRender
{
    public static partial class Renderer
    {
		/// <summary>The callback to the host application</summary>
	    public static HostInterface currentHost;
		/// <summary>Whether texturing is currently enabled in openGL</summary>
		public static bool TexturingEnabled;
		/// <summary>Holds the lock for GDI Plus functions</summary>
		public static readonly object gdiPlusLock = new object();
    }
}
