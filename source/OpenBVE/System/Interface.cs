using System.Collections.Generic;
using System.Globalization;
using OpenBveApi;
using OpenBveApi.Interface;

namespace OpenBve {
	internal static partial class Interface {
		internal static List<LogMessage> LogMessages = new List<LogMessage>();
		internal static void AddMessage(MessageType messageType, bool fileNotFound, string messageText) {
			if (messageType == MessageType.Warning && !CurrentOptions.ShowWarningMessages) return;
			if (messageType == MessageType.Error && !CurrentOptions.ShowErrorMessages) return;
			LogMessages.Add(new LogMessage(messageType, fileNotFound, messageText));
			Program.FileSystem.AppendToLogFile(messageText);
			
		}

		/// <summary>Parses a string into OpenBVE's internal time representation (Seconds since midnight on the first day)</summary>
		/// <param name="Expression">The time in string format</param>
		/// <param name="Value">The number of seconds since midnight on the first day this represents, updated via 'out'</param>
		/// <returns>True if the parse succeeds, false if it does not</returns>
		internal static bool TryParseTime(string Expression, out double Value)
		{
			Expression = Expression.TrimInside();
			if (Expression.Length != 0) {
				CultureInfo Culture = CultureInfo.InvariantCulture;
				int i = Expression.IndexOf('.');
				if (i == -1)
				{
					i = Expression.IndexOf(':');
				}
				if (i >= 1) {
					if (int.TryParse(Expression.Substring(0, i), NumberStyles.Integer, Culture, out int h)) {
						int n = Expression.Length - i - 1;
						if (n == 1 | n == 2) {
							if (uint.TryParse(Expression.Substring(i + 1, n), NumberStyles.None, Culture, out uint m)) {
								Value = 3600.0 * h + 60.0 * m;
								return true;
							}
						} else if (n >= 3) {
							if (n > 4)
							{
								Program.CurrentHost.AddMessage(MessageType.Warning, false, "A maximum of 4 digits of precision are supported in TIME values");
								n = 4;
							}
							if (uint.TryParse(Expression.Substring(i + 1, 2), NumberStyles.None, Culture, out uint m)) {
								string ss = Expression.Substring(i + 3, n - 2);
								if (CurrentOptions.EnableBveTsHacks)
								{
									/*
									 * Handles values in the following format:
									 * HH.MM.SS
									 */
									if (ss.StartsWith("."))
									{
										ss = ss.Substring(1, ss.Length - 1);
									}
								}
								if (uint.TryParse(ss, NumberStyles.None, Culture, out uint s)) {
									Value = 3600.0 * h + 60.0 * m + s;
									return true;
								}
							}
						}
					}
				} else if (i == -1) {
					if (int.TryParse(Expression, NumberStyles.Integer, Culture, out int h)) {
						Value = 3600.0 * h;
						return true;
					}
				}
			}
			Value = 0.0;
			return false;
		}
	}
}
