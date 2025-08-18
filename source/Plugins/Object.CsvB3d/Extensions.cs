using System;

namespace Object.CsvB3d
{
	internal static class Extensions
	{
		/// <summary>Provides a case and directory separator insensitive version of string.EndsWith</summary>
		internal static bool FileNameEndsWith(this string s, string[] testStrings)
		{
			for (int i = 0; i < testStrings.Length; i++)
			{
				if (s.EndsWith(testStrings[i], StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}

				if (testStrings[i].IndexOf('\\') != -1)
				{
					testStrings[i] = testStrings[i].Replace('\\', '/');
				}
				else
				{
					testStrings[i] = testStrings[i].Replace('/', '\\');
				}


				if (s.EndsWith(testStrings[i], StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>Provides a case and directory separator insensitive version of string.Contains</summary>
		internal static bool FileNameContains(this string s, string[] testStrings)
		{
			for (int i = 0; i < testStrings.Length; i++)
			{
				if (s.IndexOf(testStrings[i], StringComparison.InvariantCultureIgnoreCase) != -1)
				{
					return true;
				}

				if (testStrings[i].IndexOf('\\') != -1)
				{
					testStrings[i] = testStrings[i].Replace('\\', '/');
				}
				else
				{
					testStrings[i] = testStrings[i].Replace('/', '\\');
				}

				if (s.IndexOf(testStrings[i], StringComparison.InvariantCultureIgnoreCase) != -1)
				{
					return true;
				}
			}

			return false;
		}
	}
}
