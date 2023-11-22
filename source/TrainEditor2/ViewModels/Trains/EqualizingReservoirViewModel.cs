using System.Globalization;
using OpenBveApi.World;
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

		internal EqualizingReservoirViewModel(EqualizingReservoir equalizingReservoir)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			ChargeRate = equalizingReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.ChargeRate,
					x => x.Value.ToString(culture),
					x => new Quantity.PressureRate(double.Parse(x, NumberStyles.Float, culture), equalizingReservoir.ChargeRate.UnitValue),
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

			ChargeRateUnit = equalizingReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.ChargeRate,
					x => x.UnitValue,
					x => equalizingReservoir.ChargeRate.ToNewUnit(x)
				)
				.AddTo(disposable);

			ServiceRate = equalizingReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.ServiceRate,
					x => x.Value.ToString(culture),
					x => new Quantity.PressureRate(double.Parse(x, NumberStyles.Float, culture), equalizingReservoir.ServiceRate.UnitValue),
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

			ServiceRateUnit = equalizingReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.ServiceRate,
					x => x.UnitValue,
					x => equalizingReservoir.ServiceRate.ToNewUnit(x)
				)
				.AddTo(disposable);

			EmergencyRate = equalizingReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.EmergencyRate,
					x => x.Value.ToString(culture),
					x => new Quantity.PressureRate(double.Parse(x, NumberStyles.Float, culture), equalizingReservoir.EmergencyRate.UnitValue),
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

			EmergencyRateUnit = equalizingReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.EmergencyRate,
					x => x.UnitValue,
					x => equalizingReservoir.EmergencyRate.ToNewUnit(x)
				)
				.AddTo(disposable);
		}
	}
}
