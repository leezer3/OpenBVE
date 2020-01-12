using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Others;

namespace TrainEditor2.ViewModels.Others
{
	internal class ListViewSubItemViewModel : BaseViewModel
	{
		internal ReadOnlyReactivePropertySlim<string> Text
		{
			get;
		}

		internal ListViewSubItemViewModel(ListViewSubItemModel subItem)
		{
			Text = subItem
				.ObserveProperty(x => x.Text)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);
		}
	}
}
