// https://gist.github.com/gayaK/4475686

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace TrainEditor2.Extensions
{
	internal class ObservableDictionary<TKey, TValue> : CollectionBasedDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
	{
		private const string DictionaryName = "Dictionary";
		private const string ItemsName = "Items[]";
		private const string KeysName = "Keys[]";
		private const string ValuesName = "Values[]";
		private const string CountName = "Count";

		protected virtual event PropertyChangedEventHandler PropertyChanged;
		public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

		public ObservableDictionary()
		{
		}

		public ObservableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }

		public ObservableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }

		public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add
			{
				PropertyChanged += value;
			}
			remove
			{
				PropertyChanged -= value;
			}
		}

		private void OnPropertyChanged(string propertyName)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			PropertyChanged?.Invoke(this, e);
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke(this, e);
		}

		private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
		}

		private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
		}

		private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
		}

		private void OnCollectionReset()
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			OnPropertyChanged(DictionaryName);
			OnPropertyChanged(ItemsName);
			OnPropertyChanged(KeysName);
			OnPropertyChanged(ValuesName);
			OnPropertyChanged(CountName);
			OnCollectionReset();
		}

		protected override void RemoveItem(int index)
		{
			KeyValuePair<TKey, TValue> removedItem = this[index];
			base.RemoveItem(index);
			OnPropertyChanged(DictionaryName);
			OnPropertyChanged(ItemsName);
			OnPropertyChanged(KeysName);
			OnPropertyChanged(ValuesName);
			OnPropertyChanged(CountName);
			OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
		}

		protected override void InsertItem(int index, KeyValuePair<TKey, TValue> item)
		{
			base.InsertItem(index, item);
			OnPropertyChanged(DictionaryName);
			OnPropertyChanged(ItemsName);
			OnPropertyChanged(KeysName);
			OnPropertyChanged(ValuesName);
			OnPropertyChanged(CountName);
			OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
		}

		protected override void SetItem(int index, KeyValuePair<TKey, TValue> item)
		{
			KeyValuePair<TKey, TValue> originalItem = this[index];
			base.SetItem(index, item);
			OnPropertyChanged(DictionaryName);
			OnPropertyChanged(ItemsName);
			OnPropertyChanged(KeysName);
			OnPropertyChanged(ValuesName);
			OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, item, index);
		}

		protected virtual void MoveItem(int oldIndex, int newIndex)
		{
			KeyValuePair<TKey, TValue> removedItem = this[oldIndex];
			base.RemoveItem(oldIndex);
			base.InsertItem(newIndex, removedItem);
			OnPropertyChanged(DictionaryName);
			OnPropertyChanged(ItemsName);
			OnPropertyChanged(KeysName);
			OnPropertyChanged(ValuesName);
			OnCollectionChanged(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex);
		}

		public void Move(int oldIndex, int newIndex)
		{
			MoveItem(oldIndex, newIndex);
		}
	}
}
