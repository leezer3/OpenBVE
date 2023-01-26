// ╔═════════════════════════════════════════════════════════════╗
// ║ Interface.cs for the Route Viewer                           ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System.Collections.Generic;
using OpenBveApi;
using OpenBveApi.Interface;

namespace RouteViewer {
	// --- Interface.cs ---
	internal static class Interface {

		/// <summary>Holds the program specific options</summary>
		internal class Options : BaseOptions
		{
			internal bool LoadingProgressBar;
			internal bool LoadingLogo;
			internal bool LoadingBackground;

			internal Options()
			{
				ViewingDistance = 600;
			}
		}

		/// <summary>The current options in use</summary>
		internal static Options CurrentOptions;

		internal static readonly List<LogMessage> LogMessages = new List<LogMessage>();
		internal static void AddMessage(MessageType Type, bool FileNotFound, string Text) {
			LogMessages.Add(new LogMessage(Type, FileNotFound, Text));
		}
	}
}
