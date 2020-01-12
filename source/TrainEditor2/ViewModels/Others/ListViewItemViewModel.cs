using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Others;

namespace TrainEditor2.ViewModels.Others
{
	internal class ListViewItemViewModel : BaseViewModel
	{
		internal ListViewItemModel Model
		{
			get;
		}

		internal ReadOnlyReactiveCollection<ListViewSubItemViewModel> SubItems
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<object> Tag
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<int> ImageIndex
		{
			get;
		}

		internal ListViewItemViewModel(ListViewItemModel item)
		{
			Model = item;

			SubItems = item.SubItems.ToReadOnlyReactiveCollection(x => new ListViewSubItemViewModel(x)).AddTo(disposable);

			Tag = item
				.ObserveProperty(x => x.Tag)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			ImageIndex = item
				.ObserveProperty(x => x.ImageIndex)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);
		}
	}
}
