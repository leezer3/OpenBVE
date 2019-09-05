using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Dialogs;

namespace TrainEditor2.ViewModels.Dialogs
{
	internal class SaveFileDialogViewModel : BaseDialogViewModel
	{
		internal ReadOnlyReactivePropertySlim<string> Filter
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<bool> OverwritePrompt
		{
			get;
		}

		internal ReactiveProperty<string> FileName
		{
			get;
		}

		internal SaveFileDialogViewModel(SaveFileDialog dialog) : base(dialog)
		{
			Filter = dialog
				.ObserveProperty(x => x.Filter)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			OverwritePrompt = dialog
				.ObserveProperty(x => x.OverwritePrompt)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			FileName = dialog
				.ToReactivePropertyAsSynchronized(x => x.FileName)
				.AddTo(disposable);
		}
	}
}
