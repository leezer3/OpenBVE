using System.Globalization;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class StraightAirPipeViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> ServiceRate
		{
			get;
		}

		internal ReactiveProperty<string> EmergencyRate
		{
			get;
		}

		internal ReactiveProperty<string> ReleaseRate
		{
			get;
		}

		internal StraightAirPipeViewModel(StraightAirPipe straightAirPipe)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			ServiceRate = straightAirPipe
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

			EmergencyRate = straightAirPipe
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

			ReleaseRate = straightAirPipe
				.ToReactivePropertyAsSynchronized(
					x => x.ReleaseRate,
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
