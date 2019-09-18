using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Others;

namespace TrainEditor2.ViewModels.Others
{
	internal class ToolTipViewModel : BaseViewModel
	{
		internal ReadOnlyReactivePropertySlim<bool> IsOpen
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<string> Title
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<ToolTipModel.ToolTipIcon> Icon
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<string> Text
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<double> X
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<double> Y
		{
			get;
		}

		internal ToolTipViewModel(ToolTipModel toolTip)
		{
			IsOpen = toolTip
				.ObserveProperty(x => x.IsOpen)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Title = toolTip
				.ObserveProperty(x => x.Title)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Icon = toolTip
				.ObserveProperty(x => x.Icon)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Text = toolTip
				.ObserveProperty(x => x.Text)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			X = toolTip
				.ObserveProperty(x => x.X)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Y = toolTip
				.ObserveProperty(x => x.Y)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);
		}
	}
}
