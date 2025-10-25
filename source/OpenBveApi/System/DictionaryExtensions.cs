using System.Collections.Generic;

namespace OpenBveApi
{
	/// <summary>Contains extension methods for working with dictionaries</summary>
    public static class DictionaryExtensions
    {
		/// <summary>Gets a typed value from a generic dictionary</summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <typeparam name="TActual"></typeparam>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool TryGetTypedValue<TKey, TValue, TActual>(this IDictionary<TKey, TValue> data, TKey key, out TActual value) where TActual : TValue
		{
			if (data.TryGetValue(key, out TValue tmp))
			{
				value = (TActual)tmp;
				return true;
			}
			value = default(TActual);
			return false;
		}
	}
}
