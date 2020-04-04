using System;
using System.Globalization;
using System.Reactive.Linq;
using OpenBveApi.Units;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class BrakeCylinderViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> ServiceMaximumPressure
		{
			get;
		}

		internal ReactiveProperty<Unit.Pressure> ServiceMaximumPressureUnit
		{
			get;
		}

		internal ReactiveProperty<string> EmergencyMaximumPressure
		{
			get;
		}

		internal ReactiveProperty<Unit.Pressure> EmergencyMaximumPressureUnit
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

		internal BrakeCylinderViewModel(BrakeCylinder brakeCylinder)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			ServiceMaximumPressure = brakeCylinder
				.ToReactivePropertyAsSynchronized(
					x => x.ServiceMaximumPressure,
					x => x.Value.ToString(culture),
					x => new Quantity.Pressure(double.Parse(x, NumberStyles.Float, culture), brakeCylinder.ServiceMaximumPressure.UnitValue),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			ServiceMaximumPressureUnit = brakeCylinder
				.ToReactivePropertyAsSynchronized(
					x => x.ServiceMaximumPressure,
					x => x.UnitValue,
					x => brakeCylinder.ServiceMaximumPressure.ToNewUnit(x)
				)
				.AddTo(disposable);

			EmergencyMaximumPressure = brakeCylinder
				.ToReactivePropertyAsSynchronized(
					x => x.EmergencyMaximumPressure,
					x => x.Value.ToString(culture),
					x => new Quantity.Pressure(double.Parse(x, NumberStyles.Float, culture), brakeCylinder.EmergencyMaximumPressure.UnitValue),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			EmergencyMaximumPressureUnit = brakeCylinder
				.ToReactivePropertyAsSynchronized(
					x => x.EmergencyMaximumPressure,
					x => x.UnitValue,
					x => brakeCylinder.EmergencyMaximumPressure.ToNewUnit(x)
				)
				.AddTo(disposable);

			EmergencyRate = brakeCylinder
				.ToReactivePropertyAsSynchronized(
					x => x.EmergencyRate,
					x => x.Value.ToString(culture),
					x => new Quantity.PressureRate(double.Parse(x, NumberStyles.Float, culture), brakeCylinder.EmergencyRate.UnitValue),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.NonNegative, out result, out message);

					return message;
				})
				.AddTo(disposable);

			EmergencyRateUnit = brakeCylinder
				.ToReactivePropertyAsSynchronized(
					x => x.EmergencyRate,
					x => x.UnitValue,
					x => brakeCylinder.EmergencyRate.ToNewUnit(x)
				)
				.AddTo(disposable);

			ReleaseRate = brakeCylinder
				.ToReactivePropertyAsSynchronized(
					x => x.ReleaseRate,
					x => x.Value.ToString(culture),
					x => new Quantity.PressureRate(double.Parse(x, NumberStyles.Float, culture), brakeCylinder.ReleaseRate.UnitValue),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.NonNegative, out result, out message);

					return message;
				})
				.AddTo(disposable);

			ReleaseRateUnit = brakeCylinder
				.ToReactivePropertyAsSynchronized(
					x => x.ReleaseRate,
					x => x.UnitValue,
					x => brakeCylinder.ReleaseRate.ToNewUnit(x)
				)
				.AddTo(disposable);

			ServiceMaximumPressure
				.SetValidateNotifyError(x =>
				{
					double service;
					string message;

					if (Utilities.TryParse(x, NumberRange.Positive, out service, out message))
					{
						double emergency;

						if (Utilities.TryParse(EmergencyMaximumPressure.Value, NumberRange.Positive, out emergency) && new Quantity.Pressure(service, brakeCylinder.ServiceMaximumPressure.UnitValue) > new Quantity.Pressure(emergency, brakeCylinder.EmergencyMaximumPressure.UnitValue))
						{
							return "The EmergencyMaximumPressure is required to be greater than or equal to ServiceMaximumPressure.";
						}
					}

					return message;
				})
				.Subscribe(_ => EmergencyMaximumPressure.ForceValidate())
				.AddTo(disposable);

			ServiceMaximumPressure.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => ServiceMaximumPressure.ForceNotify())
				.AddTo(disposable);

			EmergencyMaximumPressure
				.SetValidateNotifyError(x =>
				{
					double emergency;
					string message;

					if (Utilities.TryParse(x, NumberRange.Positive, out emergency, out message))
					{
						double service;

						if (Utilities.TryParse(ServiceMaximumPressure.Value, NumberRange.Positive, out service) && new Quantity.Pressure(emergency, brakeCylinder.EmergencyMaximumPressure.UnitValue) < new Quantity.Pressure(service, brakeCylinder.ServiceMaximumPressure.UnitValue))
						{
							return "The EmergencyMaximumPressure is required to be greater than or equal to ServiceMaximumPressure.";
						}
					}

					return message;
				})
				.Subscribe(_ => ServiceMaximumPressure.ForceValidate())
				.AddTo(disposable);

			EmergencyMaximumPressure.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => EmergencyMaximumPressure.ForceNotify())
				.AddTo(disposable);
		}
	}
}
