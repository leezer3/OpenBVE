using System;
using System.Globalization;
using System.Reactive.Linq;
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

		internal ReactiveProperty<string> EmergencyMaximumPressure
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

		internal BrakeCylinderViewModel(BrakeCylinder brakeCylinder)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			ServiceMaximumPressure = brakeCylinder
				.ToReactivePropertyAsSynchronized(
					x => x.ServiceMaximumPressure,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			EmergencyMaximumPressure = brakeCylinder
				.ToReactivePropertyAsSynchronized(
					x => x.EmergencyMaximumPressure,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			EmergencyRate = brakeCylinder
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

					Utilities.TryParse(x, NumberRange.NonNegative, out result, out message);

					return message;
				})
				.AddTo(disposable);

			ReleaseRate = brakeCylinder
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

					Utilities.TryParse(x, NumberRange.NonNegative, out result, out message);

					return message;
				})
				.AddTo(disposable);

			ServiceMaximumPressure
				.SetValidateNotifyError(x =>
				{
					double service;
					string message;

					if (Utilities.TryParse(x, NumberRange.Positive, out service, out message))
					{
						double emergency;

						if (Utilities.TryParse(EmergencyMaximumPressure.Value, NumberRange.Positive, out emergency) && service > emergency)
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

						if (Utilities.TryParse(ServiceMaximumPressure.Value, NumberRange.Positive, out service) && emergency < service)
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
