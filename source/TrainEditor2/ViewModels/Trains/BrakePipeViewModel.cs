using System.Globalization;
using OpenBveApi.Units;
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

		internal ReactiveProperty<Unit.Pressure> NormalPressureUnit
		{
			get;
		}

		internal ReactiveProperty<string> ChargeRate
		{
			get;
		}

		internal ReactiveProperty<Unit.PressureRate> ChargeRateUnit
		{
			get;
		}

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

		internal BrakePipeViewModel(BrakePipe brakePipe)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			NormalPressure = brakePipe
				.ToReactivePropertyAsSynchronized(
					x => x.NormalPressure,
					x => x.Value.ToString(culture),
					x => new Quantity.Pressure(double.Parse(x, NumberStyles.Float, culture), brakePipe.NormalPressure.UnitValue),
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

			NormalPressureUnit = brakePipe
				.ToReactivePropertyAsSynchronized(
					x => x.NormalPressure,
					x => x.UnitValue,
					x => brakePipe.NormalPressure.ToNewUnit(x)
				)
				.AddTo(disposable);

			ChargeRate = brakePipe
				.ToReactivePropertyAsSynchronized(
					x => x.ChargeRate,
					x => x.Value.ToString(culture),
					x => new Quantity.PressureRate(double.Parse(x, NumberStyles.Float, culture), brakePipe.ChargeRate.UnitValue),
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

			ChargeRateUnit = brakePipe
				.ToReactivePropertyAsSynchronized(
					x => x.ChargeRate,
					x => x.UnitValue,
					x => brakePipe.ChargeRate.ToNewUnit(x)
				)
				.AddTo(disposable);

			ServiceRate = brakePipe
				.ToReactivePropertyAsSynchronized(
					x => x.ServiceRate,
					x => x.Value.ToString(culture),
					x => new Quantity.PressureRate(double.Parse(x, NumberStyles.Float, culture), brakePipe.ServiceRate.UnitValue),
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

			ServiceRateUnit = brakePipe
				.ToReactivePropertyAsSynchronized(
					x => x.ServiceRate,
					x => x.UnitValue,
					x => brakePipe.ServiceRate.ToNewUnit(x)
				)
				.AddTo(disposable);

			EmergencyRate = brakePipe
				.ToReactivePropertyAsSynchronized(
					x => x.EmergencyRate,
					x => x.Value.ToString(culture),
					x => new Quantity.PressureRate(double.Parse(x, NumberStyles.Float, culture), brakePipe.EmergencyRate.UnitValue),
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

			EmergencyRateUnit = brakePipe
				.ToReactivePropertyAsSynchronized(
					x => x.EmergencyRate,
					x => x.UnitValue,
					x => brakePipe.EmergencyRate.ToNewUnit(x)
				)
				.AddTo(disposable);
		}
	}
}
