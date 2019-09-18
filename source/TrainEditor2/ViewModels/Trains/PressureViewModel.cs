using System;
using System.Globalization;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class PressureViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> BrakeCylinderServiceMaximumPressure
		{
			get;
		}

		internal ReactiveProperty<string> BrakeCylinderEmergencyMaximumPressure
		{
			get;
		}

		internal ReactiveProperty<string> MainReservoirMinimumPressure
		{
			get;
		}

		internal ReactiveProperty<string> MainReservoirMaximumPressure
		{
			get;
		}

		internal ReactiveProperty<string> BrakePipeNormalPressure
		{
			get;
		}

		internal PressureViewModel(Pressure pressure)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			BrakeCylinderServiceMaximumPressure = pressure
				.ToReactivePropertyAsSynchronized(
					x => x.BrakeCylinderServiceMaximumPressure,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			BrakeCylinderEmergencyMaximumPressure = pressure
				.ToReactivePropertyAsSynchronized(
					x => x.BrakeCylinderEmergencyMaximumPressure,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MainReservoirMinimumPressure = pressure
				.ToReactivePropertyAsSynchronized(
					x => x.MainReservoirMinimumPressure,
					x => x.ToString(culture),
					double.Parse,
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

			MainReservoirMaximumPressure = pressure
				.ToReactivePropertyAsSynchronized(
					x => x.MainReservoirMaximumPressure,
					x => x.ToString(culture),
					double.Parse,
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

			BrakePipeNormalPressure = pressure
				.ToReactivePropertyAsSynchronized(
					x => x.BrakePipeNormalPressure,
					x => x.ToString(culture),
					double.Parse,
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

			BrakeCylinderServiceMaximumPressure
				.SetValidateNotifyError(x =>
				{
					double service;
					string message;

					if (Utilities.TryParse(x, NumberRange.Positive, out service, out message))
					{
						double emergency;

						if (Utilities.TryParse(BrakeCylinderEmergencyMaximumPressure.Value, NumberRange.Positive, out emergency) && service > emergency)
						{
							return "The BrakeCylinderEmergencyMaximumPressure is required to be greater than or equal to BrakeCylinderServiceMaximumPressure.";
						}
					}

					return message;
				})
				.Subscribe(_ => BrakeCylinderEmergencyMaximumPressure.ForceValidate())
				.AddTo(disposable);

			BrakeCylinderServiceMaximumPressure.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => BrakeCylinderServiceMaximumPressure.ForceNotify())
				.AddTo(disposable);

			BrakeCylinderEmergencyMaximumPressure
				.SetValidateNotifyError(x =>
				{
					double emergency;
					string message;

					if (Utilities.TryParse(x, NumberRange.Positive, out emergency, out message))
					{
						double service;

						if (Utilities.TryParse(BrakeCylinderServiceMaximumPressure.Value, NumberRange.Positive, out service) && emergency < service)
						{
							return "The BrakeCylinderEmergencyMaximumPressure is required to be greater than or equal to BrakeCylinderServiceMaximumPressure.";
						}
					}

					return message;
				})
				.Subscribe(_ => BrakeCylinderServiceMaximumPressure.ForceValidate())
				.AddTo(disposable);

			BrakeCylinderEmergencyMaximumPressure.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => BrakeCylinderEmergencyMaximumPressure.ForceNotify())
				.AddTo(disposable);
		}
	}
}
