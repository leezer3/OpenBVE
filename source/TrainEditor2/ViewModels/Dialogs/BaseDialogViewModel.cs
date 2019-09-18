using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Dialogs;

namespace TrainEditor2.ViewModels.Dialogs
{
	internal abstract class BaseDialogViewModel : BaseViewModel
	{
		internal ReadOnlyReactivePropertySlim<bool> IsOpen
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<string> Title
		{
			get;
		}

		internal ReactiveCommand YesCommand
		{
			get;
		}

		internal ReactiveCommand NoCommand
		{
			get;
		}

		internal ReactiveCommand CancelCommand
		{
			get;
		}

		internal BaseDialogViewModel(BaseDialog dialog)
		{
			IsOpen = dialog
				.ObserveProperty(x => x.IsOpen)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Title = dialog
				.ObserveProperty(x => x.Title)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			YesCommand = new ReactiveCommand();
			YesCommand.Subscribe(() =>
			{
				dialog.DialogResult = true;
				dialog.IsOpen = false;
			});

			NoCommand = new ReactiveCommand();
			NoCommand.Subscribe(() =>
			{
				dialog.DialogResult = false;
				dialog.IsOpen = false;
			});

			CancelCommand = new ReactiveCommand();
			CancelCommand.Subscribe(() =>
			{
				dialog.DialogResult = null;
				dialog.IsOpen = false;
			});
		}
	}
}
