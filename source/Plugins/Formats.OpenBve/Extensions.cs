using System;

namespace Formats.OpenBve
{
	internal class Extensions
	{
		public static bool EnumTryParse<T>(string input, out T theEnum)
		{
			foreach (string en in Enum.GetNames(typeof(T)))
			{
				if (en.Equals(input, StringComparison.CurrentCultureIgnoreCase))
				{
					theEnum = (T)Enum.Parse(typeof(T), input, true);
					return true;
				}
			}

			theEnum = default(T);
			return false;
		}
	}
}
