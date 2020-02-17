using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Dialogs;

namespace TrainEditor2.ViewModels.Dialogs
{
	internal class OpenFolderDialogViewModel : BaseDialogViewModel
	{
		internal ReactiveProperty<string> Folder
		{
			get;
		}

		internal OpenFolderDialogViewModel(OpenFolderDialog dialog) : base(dialog)
		{
			Folder = dialog
				.ToReactivePropertyAsSynchronized(x => x.Folder)
				.AddTo(disposable);
		}
	}
}
