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
			// Binary search: last block starting at or before distance (clamped to 0, as before).
			int lo = 0, hi = source.Count - 1, result = 0;
			while (lo <= hi)
			{
				int mid = lo + ((hi - lo) >> 1);
				if (source.Keys[mid] <= distance)
				{
					result = mid;
					lo = mid + 1;
				}
				else
				{
					hi = mid - 1;
				}
			}
			return result;
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
