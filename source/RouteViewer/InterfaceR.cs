// ╔═════════════════════════════════════════════════════════════╗
// ║ Interface.cs for the Route Viewer                           ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using OpenBveApi;
using OpenBveApi.Interface;

namespace OpenBve {
	// --- Interface.cs ---
	internal static class Interface {

		/// <summary>Holds the program specific options</summary>
		internal class Options : BaseOptions
		{
			internal bool LoadingProgressBar;
			internal bool LoadingLogo;
			internal bool LoadingBackground;
		}

		/// <summary>The current options in use</summary>
		internal static Options CurrentOptions;

		internal static LogMessage[] LogMessages = new LogMessage[] { };
		internal static int MessageCount = 0;
		internal static void AddMessage(MessageType Type, bool FileNotFound, string Text) {
			if (MessageCount == 0) {
				LogMessages = new LogMessage[16];
			} else if (MessageCount >= LogMessages.Length) {
				Array.Resize<LogMessage>(ref LogMessages, LogMessages.Length << 1);
			}
			LogMessages[MessageCount] = new LogMessage(Type, FileNotFound, Text);
			MessageCount++;
		}
		internal static void ClearMessages() {
			LogMessages = new LogMessage[] { };
			MessageCount = 0;
		}
	}
}
