// ╔═════════════════════════════════════════════════════════════╗
// ║ Interface.cs for the Route Viewer                           ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System.Collections.Generic;
using OpenBveApi.Interface;

namespace RouteViewer {
	// --- Interface.cs ---
	internal static class Interface {
		
		/// <summary>The current options in use</summary>
		internal static Options CurrentOptions;

		internal static readonly List<LogMessage> LogMessages = new List<LogMessage>();
		internal static readonly object LogLock = new object();
		internal static void AddMessage(MessageType type, bool fileNotFound, string text) {
			lock (LogLock)
			{
				LogMessages.Add(new LogMessage(type, fileNotFound, text));
			}
		}

		internal static List<LogMessage> GetLogSnapshot()
		{
			lock (LogLock)
			{
				return new List<LogMessage>(LogMessages);
			}
		}

		internal static void ClearLog()
		{
			lock (LogLock)
			{
				LogMessages.Clear();
			}
		}
	}
}
