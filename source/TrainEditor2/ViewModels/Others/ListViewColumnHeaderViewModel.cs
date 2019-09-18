using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Others;

namespace TrainEditor2.ViewModels.Others
{
	internal class ListViewColumnHeaderViewModel : BaseViewModel
	{
		internal ReadOnlyReactivePropertySlim<string> Text
		{
			get;
		}

		internal ListViewColumnHeaderViewModel(ListViewColumnHeaderModel column)
		{
			Text = column
				.ObserveProperty(x => x.Text)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);
		}
	}
}
