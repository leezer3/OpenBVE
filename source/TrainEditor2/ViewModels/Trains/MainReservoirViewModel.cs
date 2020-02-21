using System.Globalization;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class MainReservoirViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> MinimumPressure
		{
			get;
		}

		internal ReactiveProperty<string> MaximumPressure
		{
			get;
		}

		internal MainReservoirViewModel(MainReservoir mainReservoir)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			MinimumPressure = mainReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.MinimumPressure,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Positive, out result, out message);

					return message;
				})
				.AddTo(disposable);

			MaximumPressure = mainReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.MaximumPressure,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Positive, out result, out message);

					return message;
				})
				.AddTo(disposable);
		}
	}
}
