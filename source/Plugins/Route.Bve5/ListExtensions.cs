using System;
using System.Collections.Generic;

namespace Route.Bve5
{
	internal static class ListExtensions
	{
		public static int FindIndex<T>(this IList<T> source, Predicate<T> match)
		{
			for (int i = 0; i < source.Count; i++)
			{
				if (match(source[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public static int FindIndex<T>(this IList<T> source, int startIndex, Predicate<T> match)
		{
			for (int i = startIndex; i < source.Count; i++)
			{
				if (match(source[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public static int FindBlockIndex<T>(this SortedList<double, T> source, double distance)
		{
			if (source.ContainsKey(distance))
			{
				return source.IndexOfKey(distance);
			}
			for (int i = source.Count - 1; i > 0; i--)
			{
				if (source.Keys[i] <= distance)
				{
					return i;
				}
			}
			return 0;
		}

		public static int FindLastIndex<T>(this IList<T> source, int startIndex, Predicate<T> match)
		{
			for (int i = startIndex; i > 0; i--)
			{
				if (match(source[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public static int FindLastIndex<T>(this IList<T> source, int startIndex, int count, Predicate<T> match)
		{
			for (int i = startIndex; i > Math.Max(0, startIndex - count); i--)
			{
				if (match(source[i]))
				{
					return i;
				}
			}
			return 0;
		}
	}
}
