using System;
using OpenBveShared;

namespace OpenBve
{
	internal static partial class Renderer
	{
		// output mode
		internal enum OutputMode
		{
			/// <summary>Overlays are shown if active</summary>
			Default = 0,
			/// <summary>The debug overlay is shown (F10)</summary>
			Debug = 1,
			/// <summary>The ATS debug overlay is shown (F10)</summary>
			DebugATS = 2,
			/// <summary>No overlays are shown</summary>
			None = 3
		}	
	}
}
