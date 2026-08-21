using System;
using System.Collections.Generic;

namespace Formats.OpenBve
{
	/// <summary>Provides fast case-insensitive enum name parsing via a cached dictionary lookup.</summary>
	/// <remarks>Falls back to Enum.TryParse on a dictionary miss so that numeric strings and surrounding whitespace keep their original semantics.</remarks>
	internal static class EnumCache<T> where T : struct, Enum
	{
		private static readonly Dictionary<string, T> Names = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

		static EnumCache()
		{
			foreach (string name in Enum.GetNames(typeof(T)))
			{
				Names[name] = (T)Enum.Parse(typeof(T), name);
			}
		}

		internal static bool TryParse(string value, out T result)
		{
			if (value != null && Names.TryGetValue(value, out result))
			{
				return true;
			}
			return Enum.TryParse(value, true, out result);
		}
	}
}
