using System.Collections.Generic;
using System.Globalization;
using OpenBveApi;
using OpenBveApi.Interface;

namespace OpenBve {
	internal static partial class Interface {
		internal static List<LogMessage> LogMessages = new List<LogMessage>();
		internal static void AddMessage(MessageType messageType, bool fileNotFound, string messageText) {
			if (messageType == MessageType.Warning & !CurrentOptions.ShowWarningMessages) return;
			if (messageType == MessageType.Error & !CurrentOptions.ShowErrorMessages) return;
			LogMessages.Add(new LogMessage(messageType, fileNotFound, messageText));
			Program.FileSystem.AppendToLogFile(messageText);
			
		}

		/// <summary>Parses a string into OpenBVE's internal time representation (Seconds since midnight on the first day)</summary>
		/// <param name="expression">The time in string format</param>
		/// <param name="value">The number of seconds since midnight on the first day this represents, updated via 'out'</param>
		/// <returns>True if the parse succeeds, false if it does not</returns>
		internal static bool TryParseTime(string expression, out double value)
		{
			expression = expression.TrimInside();
			if (expression.Length != 0) {
				CultureInfo culture = CultureInfo.InvariantCulture;
				int i = expression.IndexOf('.');
				if (i == -1)
				{
					i = expression.IndexOf(':');
				}
				if (i >= 1) {
					if (int.TryParse(expression.Substring(0, i), NumberStyles.Integer, culture, out int h)) {
						int n = expression.Length - i - 1;
						if (n == 1 | n == 2) {
							if (uint.TryParse(expression.Substring(i + 1, n), NumberStyles.None, culture, out uint m)) {
								value = 3600.0 * h + 60.0 * m;
								return true;
							}
						} else if (n >= 3) {
							if (n > 4)
							{
								Program.CurrentHost.AddMessage(MessageType.Warning, false, "A maximum of 4 digits of precision are supported in TIME values");
								n = 4;
							}
							if (uint.TryParse(expression.Substring(i + 1, 2), NumberStyles.None, culture, out uint m)) {
								string ss = expression.Substring(i + 3, n - 2);
								if (Interface.CurrentOptions.EnableBveTsHacks)
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
								if (uint.TryParse(ss, NumberStyles.None, culture, out uint s)) {
									value = 3600.0 * h + 60.0 * m + s;
									return true;
								}
							}
						}
					}
				} else if (i == -1) {
					if (int.TryParse(expression, NumberStyles.Integer, culture, out int h)) {
						value = 3600.0 * h;
						return true;
					}
				}
			}
			value = 0.0;
			return false;
		}
	}
}
