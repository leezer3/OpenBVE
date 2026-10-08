using System.Collections.Generic;
using OpenBveApi.Interface;

namespace OpenBve {
	internal static partial class Interface {
		internal static List<LogMessage> LogMessages = new List<LogMessage>();
		internal static readonly object LogLock = new object();
		internal static void AddMessage(MessageType messageType, bool fileNotFound, string messageText) {
			if (messageType == MessageType.Warning && !CurrentOptions.ShowWarningMessages) return;
			if (messageType == MessageType.Error && !CurrentOptions.ShowErrorMessages) return;
			lock (LogLock)
			{
				LogMessages.Add(new LogMessage(messageType, fileNotFound, messageText));
			}
			Program.FileSystem.AppendToLogFile(messageText);
			
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
