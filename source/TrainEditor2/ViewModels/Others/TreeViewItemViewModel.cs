using System.Linq;
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

		internal TreeViewItemViewModel Parent
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<string> Title
		{
			get;
		}

		internal ReactiveProperty<bool> Checked
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

		internal TreeViewItemViewModel(TreeViewItemModel item, TreeViewItemViewModel parent)
		{
			Model = item;

			Parent = parent;

			Title = item
				.ObserveProperty(x => x.Title)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Checked = item
				.ToReactivePropertyAsSynchronized(x => x.Checked)
				.AddTo(disposable);

			Tag = item
				.ObserveProperty(x => x.Tag)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Children = item.Children
				.ToReadOnlyReactiveCollection(x => new TreeViewItemViewModel(x, this))
				.AddTo(disposable);
		}

		internal TreeViewItemViewModel SearchViewModel(TreeViewItemModel model)
		{
			return Model == model ? this : Children.Select(x => x.SearchViewModel(model)).FirstOrDefault(x => x != null);
		}
	}
}
