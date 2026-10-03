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

		/// <summary>Finds the index of the last block starting at or before the given distance</summary>
		public static int FindBlockIndex<T>(this SortedList<double, T> source, double distance)
		{
			int i = source.IndexOfKey(distance);
			if (i >= 0)
			{
				return i;
			}
			for (i = source.Count - 1; i > 0; i--)
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
			for (int i = Math.Min(startIndex, source.Count - 1); i >= 0; i--)
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
			int stop = Math.Max(0, startIndex - count + 1);
			for (int i = Math.Min(startIndex, source.Count - 1); i >= stop; i--)
			{
				if (match(source[i]))
				{
					return i;
				}
			}
			return -1;
		}
	}
}
