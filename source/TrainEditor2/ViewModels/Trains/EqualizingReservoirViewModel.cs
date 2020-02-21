using System.Globalization;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class EqualizingReservoirViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> ChargeRate
		{
			get;
		}

		internal ReactiveProperty<string> ServiceRate
		{
			get;
		}

		internal ReactiveProperty<string> EmergencyRate
		{
			get;
		}

		internal EqualizingReservoirViewModel(EqualizingReservoir equalizingReservoir)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			ChargeRate = equalizingReservoir
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

			ServiceRate = equalizingReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.ServiceRate,
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

			EmergencyRate = equalizingReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.EmergencyRate,
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
