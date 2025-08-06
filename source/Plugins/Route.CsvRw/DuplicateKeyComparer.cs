using System;
using System.Collections.Generic;

namespace Route.CsvRw
{
	/// <summary>Comparer for comparing two keys, treating equality as being less than</summary>
	/// <typeparam name="TKey"></typeparam>
	public class DuplicateLessThanKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
	{
		public int Compare(TKey x, TKey y)
		{
			int result = x.CompareTo(y);
			if (result == 0)
				return -1;
			
			return result;
		}
	}
}
