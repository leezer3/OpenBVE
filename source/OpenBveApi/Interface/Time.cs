﻿using System.Globalization;

namespace OpenBveApi
{
	/// <summary>Provides time related functionality</summary>
	public class Time
	{
		/// <summary>Parses a string into OpenBVE's internal time representation (Seconds since midnight on the first day)</summary>
		/// <param name="Expression">The time in string format</param>
		/// <param name="Value">The number of seconds since midnight on the first day this represents, updated via 'out'</param>
		/// <returns>True if the parse succeeds, false if it does not</returns>
		public static bool TryParseTime(string Expression, out double Value)
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
								n = 4;
							}
							if (uint.TryParse(Expression.Substring(i + 1, 2), NumberStyles.None, Culture, out uint m)) {
								string ss = Expression.Substring(i + 3, n - 2);
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
