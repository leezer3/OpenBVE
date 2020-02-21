using System.Globalization;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class BrakePipeViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> NormalPressure
		{
			get;
		}

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

		internal BrakePipeViewModel(BrakePipe brakePipe)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			NormalPressure = brakePipe
				.ToReactivePropertyAsSynchronized(
					x => x.NormalPressure,
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

			ChargeRate = brakePipe
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

			ServiceRate = brakePipe
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

			EmergencyRate = brakePipe
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
