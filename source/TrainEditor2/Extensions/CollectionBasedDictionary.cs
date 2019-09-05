// https://gist.github.com/gayaK/4475686

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TrainEditor2.Extensions
{
	internal class CollectionBasedDictionary<TKey, TValue> : KeyedCollection<TKey, KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
	{
		public CollectionBasedDictionary()
		{
		}

		public CollectionBasedDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }

		public CollectionBasedDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

		public CollectionBasedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(comparer)
		{
			if (dictionary == null)
			{
				throw new ArgumentNullException(nameof(dictionary));
			}

			foreach (KeyValuePair<TKey, TValue> pair in dictionary)
			{
				Add(pair.Key, pair.Value);
			}
		}

		protected override TKey GetKeyForItem(KeyValuePair<TKey, TValue> item)
		{
			return item.Key;
		}

		public ICollection<TKey> Keys => Dictionary == null ? Enumerable.Empty<TKey>().ToArray() : Dictionary.Keys;

		public ICollection<TValue> Values => Dictionary == null ? Enumerable.Empty<TValue>().ToArray() : Dictionary.Values.Select(x => x.Value).ToArray();

		public new TValue this[TKey key]
		{
			get
			{
				if (Dictionary == null)
				{
					throw new KeyNotFoundException();
				}

				return Dictionary[key].Value;
			}
			set
			{
				KeyValuePair<TKey, TValue> newValue = new KeyValuePair<TKey, TValue>(key, value);
				KeyValuePair<TKey, TValue> oldValue;

				if (Dictionary.TryGetValue(key, out oldValue))
				{
					SetItem(Items.IndexOf(oldValue), newValue);
				}
				else
				{
					InsertItem(Count, newValue);
				}
			}
		}

		public void Add(TKey key, TValue value)
		{
			Add(new KeyValuePair<TKey, TValue>(key, value));
		}

		public bool ContainsKey(TKey key)
		{
			return Dictionary.ContainsKey(key);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			KeyValuePair<TKey, TValue> pair;
			bool result = Dictionary.TryGetValue(key, out pair);
			value = pair.Value;
			return result;
		}
	}
}
