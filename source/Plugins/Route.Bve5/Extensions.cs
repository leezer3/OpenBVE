using System;

namespace Bve5RouteParser
{
	/// <summary>Provides extension methods for working with text</summary>
	public static class TextExtensions
	{
		/// <summary>Trims BVE5 comments from a string.</summary>
		/// <param name="String">The string to trim.</param>
		public static string TrimBVE5Comments(this string String)
		{
			int j = String.IndexOf('#');
			if (j >= 0)
			{
				String = String.Substring(0, j);
			}
			int k = String.IndexOf("//", StringComparison.InvariantCultureIgnoreCase);
			if (k >= 0)
			{
				String = String.Substring(0, k);
			}
			return String;
		}

		/// <summary>Removes single or double quotes enclosing a string.</summary>
		/// <param name="String">The string to trim.</param>
		public static string RemoveEnclosingQuotes(this string String)
		{
			if (String.StartsWith("'") && String.EndsWith("'"))
			{
				String = String.Substring(1, String.Length - 2);
			}

			if (String.StartsWith("\"") && String.EndsWith("\""))
			{
				String = String.Substring(1, String.Length - 2);
			}
			return String;
		}
	}
}
