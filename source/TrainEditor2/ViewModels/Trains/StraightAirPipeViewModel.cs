using System.Globalization;
using OpenBveApi.Units;
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

		internal ReactiveProperty<Unit.PressureRate> ServiceRateUnit
		{
			get;
		}

		internal ReactiveProperty<string> EmergencyRate
		{
			get;
		}

		internal ReactiveProperty<Unit.PressureRate> EmergencyRateUnit
		{
			get;
		}

		internal ReactiveProperty<string> ReleaseRate
		{
			get;
		}

		internal ReactiveProperty<Unit.PressureRate> ReleaseRateUnit
		{
			get;
		}

		internal StraightAirPipeViewModel(StraightAirPipe straightAirPipe)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			ServiceRate = straightAirPipe
				.ToReactivePropertyAsSynchronized(
					x => x.ServiceRate,
					x => x.Value.ToString(culture),
					x => new Quantity.PressureRate(double.Parse(x, NumberStyles.Float, culture), straightAirPipe.ServiceRate.UnitValue),
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

			ServiceRateUnit = straightAirPipe
				.ToReactivePropertyAsSynchronized(
					x => x.ServiceRate,
					x => x.UnitValue,
					x => straightAirPipe.ServiceRate.ToNewUnit(x)
				)
				.AddTo(disposable);

			EmergencyRate = straightAirPipe
				.ToReactivePropertyAsSynchronized(
					x => x.EmergencyRate,
					x => x.Value.ToString(culture),
					x => new Quantity.PressureRate(double.Parse(x, NumberStyles.Float, culture), straightAirPipe.EmergencyRate.UnitValue),
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

			EmergencyRateUnit = straightAirPipe
				.ToReactivePropertyAsSynchronized(
					x => x.EmergencyRate,
					x => x.UnitValue,
					x => straightAirPipe.EmergencyRate.ToNewUnit(x)
				)
				.AddTo(disposable);

			ReleaseRate = straightAirPipe
				.ToReactivePropertyAsSynchronized(
					x => x.ReleaseRate,
					x => x.Value.ToString(culture),
					x => new Quantity.PressureRate(double.Parse(x, NumberStyles.Float, culture), straightAirPipe.ReleaseRate.UnitValue),
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

			ReleaseRateUnit = straightAirPipe
				.ToReactivePropertyAsSynchronized(
					x => x.ReleaseRate,
					x => x.UnitValue,
					x => straightAirPipe.ReleaseRate.ToNewUnit(x)
				)
				.AddTo(disposable);
		}
	}
}
