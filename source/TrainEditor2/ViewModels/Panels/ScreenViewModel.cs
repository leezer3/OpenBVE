using System.Collections.Generic;
using System.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Panels;

namespace TrainEditor2.ViewModels.Panels
{
	internal class ScreenViewModel : BaseViewModel
	{
		internal ReactiveProperty<int> Number
		{
			get;
		}

		internal ReactiveProperty<int> Layer
		{
			get;
		}

		internal ScreenViewModel(Screen screen, IEnumerable<Screen> otherScreens)
		{
			Number = screen
				.ToReactivePropertyAsSynchronized(x => x.Number, ignoreValidationErrorValue: true)
				.SetValidateNotifyError(x =>
				{
					string message = null;

					if (otherScreens.Any(y => y.Number == x))
					{
						message = "指定されたNumberは既に追加されています。";
					}

					return message;
				})
				.AddTo(disposable);

			Layer = screen
				.ToReactivePropertyAsSynchronized(x => x.Layer)
				.AddTo(disposable);
		}
	}
}
