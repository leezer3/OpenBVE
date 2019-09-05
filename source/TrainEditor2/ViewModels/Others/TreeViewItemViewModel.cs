using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Others;

namespace TrainEditor2.ViewModels.Others
{
	internal class TreeViewItemViewModel : BaseViewModel
	{
		internal TreeViewItemModel Model
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<string> Title
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<object> Tag
		{
			get;
		}

		internal ReadOnlyReactiveCollection<TreeViewItemViewModel> Children
		{
			get;
		}

		internal TreeViewItemViewModel(TreeViewItemModel item)
		{
			Model = item;

			Title = item
				.ObserveProperty(x => x.Title)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Tag = item
				.ObserveProperty(x => x.Tag)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Children = item.Children
				.ToReadOnlyReactiveCollection(x => new TreeViewItemViewModel(x))
				.AddTo(disposable);

			new[]
				{
					Children.CollectionChangedAsObservable().OfType<object>(),
					Children.ObserveElementObservableProperty(x => x.Title).OfType<object>(),
					Children.ObserveElementObservableProperty(x => x.Tag).OfType<object>(),
					Children.ObserveElementProperty(x => x.Children).OfType<object>()
				}
				.Merge()
				.Subscribe(_ => OnPropertyChanged(new PropertyChangedEventArgs(nameof(Children))))
				.AddTo(disposable);
		}

		internal TreeViewItemViewModel SearchViewModel(TreeViewItemModel model)
		{
			return Model == model ? this : Children.Select(x => x.SearchViewModel(model)).FirstOrDefault(x => x != null);
		}
	}
}
