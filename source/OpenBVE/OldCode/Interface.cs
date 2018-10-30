using System;
using System.Globalization;
using OpenBveApi;
using OpenBveApi.Interface;

namespace OpenBve {
	internal static partial class Interface {
		internal static LogMessage[] LogMessages = new LogMessage[] { };
		internal static int MessageCount = 0;
		internal static void AddMessage(MessageType Type, bool FileNotFound, string Text) {
			if (Type == MessageType.Warning & !CurrentOptions.ShowWarningMessages) return;
			if (Type == MessageType.Error & !CurrentOptions.ShowErrorMessages) return;
			if (MessageCount == 0) {
				LogMessages = new LogMessage[16];
			} else if (MessageCount >= LogMessages.Length) {
				Array.Resize<LogMessage>(ref LogMessages, LogMessages.Length << 1);
			}
			LogMessages[MessageCount] = new LogMessage(Type, FileNotFound, Text);
			MessageCount++;
			
			Program.FileSystem.AppendToLogFile(Text);
			
		}
		internal static void ClearMessages() {
			LogMessages = new LogMessage[] { };
			MessageCount = 0;
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
					int h; if (int.TryParse(Expression.Substring(0, i), NumberStyles.Integer, Culture, out h)) {
						int n = Expression.Length - i - 1;
						if (n == 1 | n == 2) {
							uint m; if (uint.TryParse(Expression.Substring(i + 1, n), NumberStyles.None, Culture, out m)) {
								Value = 3600.0 * (double)h + 60.0 * (double)m;
								return true;
							}
						} else if (n >= 3) {
							if (n > 4)
							{
								Interface.AddMessage(MessageType.Warning, false, "A maximum of 4 digits of precision are supported in TIME values");
								n = 4;
							}
							uint m; if (uint.TryParse(Expression.Substring(i + 1, 2), NumberStyles.None, Culture, out m)) {
								uint s;
								string ss = Expression.Substring(i + 3, n - 2);
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
								if (uint.TryParse(ss, NumberStyles.None, Culture, out s)) {
									Value = 3600.0 * (double)h + 60.0 * (double)m + (double)s;
									return true;
								}
							}
						}
					}
				} else if (i == -1) {
					int h; if (int.TryParse(Expression, NumberStyles.Integer, Culture, out h)) {
						Value = 3600.0 * (double)h;
						return true;
					}
				}
			}
			Value = 0.0;
			return false;
		}
	}
}
