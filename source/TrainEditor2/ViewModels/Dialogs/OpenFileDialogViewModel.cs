using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Dialogs;

namespace TrainEditor2.ViewModels.Dialogs
{
	internal class OpenFileDialogViewModel : BaseDialogViewModel
	{
		internal ReadOnlyReactivePropertySlim<string> Filter
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<bool> CheckFileExists
		{
			get;
		}

		internal ReactiveProperty<string> FileName
		{
			get;
		}

		internal OpenFileDialogViewModel(OpenFileDialog dialog) : base(dialog)
		{
			Filter = dialog
				.ObserveProperty(x => x.Filter)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			CheckFileExists = dialog
				.ObserveProperty(x => x.CheckFileExists)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			FileName = dialog
				.ToReactivePropertyAsSynchronized(x => x.FileName)
				.AddTo(disposable);
		}
	}
}
