using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Dialogs;

namespace TrainEditor2.ViewModels.Dialogs
{
	internal class MessageBoxViewModel : BaseDialogViewModel
	{
		internal ReadOnlyReactivePropertySlim<BaseDialog.DialogIcon> Icon
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<BaseDialog.DialogButton> Button
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<string> Text
		{
			get;
		}

		internal MessageBoxViewModel(MessageBox messageBox) : base(messageBox)
		{
			Icon = messageBox
				.ObserveProperty(x => x.Icon)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Button = messageBox
				.ObserveProperty(x => x.Button)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Text = messageBox
				.ObserveProperty(x => x.Text)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);
		}
	}
}
