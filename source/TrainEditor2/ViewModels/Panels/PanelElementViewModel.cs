using System.Globalization;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Panels;

namespace TrainEditor2.ViewModels.Panels
{
	internal abstract class PanelElementViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> LocationX
		{
			get;
		}

		internal ReactiveProperty<string> LocationY
		{
			get;
		}

		internal ReactiveProperty<int> Layer
		{
			get;
		}

		internal PanelElementViewModel(PanelElement element)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			LocationX = element
				.ToReactivePropertyAsSynchronized(
					x => x.LocationX,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Any, out result, out message);

					return message;
				})
				.AddTo(disposable);

			LocationY = element
				.ToReactivePropertyAsSynchronized(
					x => x.LocationY,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Any, out result, out message);

					return message;
				})
				.AddTo(disposable);

			Layer = element
				.ToReactivePropertyAsSynchronized(x => x.Layer)
				.AddTo(disposable);
		}
	}
}
