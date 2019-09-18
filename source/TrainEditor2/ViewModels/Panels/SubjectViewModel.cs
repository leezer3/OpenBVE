using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Panels;

namespace TrainEditor2.ViewModels.Panels
{
	internal class SubjectViewModel : BaseViewModel
	{
		internal ReactiveProperty<SubjectBase> Base
		{
			get;
		}

		internal ReactiveProperty<int> BaseOption
		{
			get;
		}

		internal ReactiveProperty<SubjectSuffix> Suffix
		{
			get;
		}

		internal ReactiveProperty<int> SuffixOption
		{
			get;
		}

		internal SubjectViewModel(Subject subject)
		{
			Base = subject
				.ToReactivePropertyAsSynchronized(x => x.Base)
				.AddTo(disposable);

			BaseOption = subject
				.ToReactivePropertyAsSynchronized(x => x.BaseOption)
				.AddTo(disposable);

			Suffix = subject
				.ToReactivePropertyAsSynchronized(x => x.Suffix)
				.AddTo(disposable);

			SuffixOption = subject
				.ToReactivePropertyAsSynchronized(x => x.SuffixOption)
				.AddTo(disposable);
		}
	}
}
