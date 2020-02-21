using System.Globalization;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class AuxiliaryReservoirViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> ChargeRate
		{
			get;
		}

		internal AuxiliaryReservoirViewModel(AuxiliaryReservoir auxiliaryReservoir)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			ChargeRate = auxiliaryReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.ChargeRate,
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
