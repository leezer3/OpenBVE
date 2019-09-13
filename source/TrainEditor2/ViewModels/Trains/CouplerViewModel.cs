using System;
using System.Globalization;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class CouplerViewModel : BaseViewModel
	{
		internal Coupler Model
		{
			get;
		}

		internal ReactiveProperty<string> Min
		{
			get;
		}

		internal ReactiveProperty<string> Max
		{
			get;
		}

		internal ReactiveProperty<string> Object
		{
			get;
		}

		internal CouplerViewModel(Coupler coupler)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			Model = coupler;

			Min = coupler
				.ToReactivePropertyAsSynchronized(
					x => x.Min,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			Max = coupler
				.ToReactivePropertyAsSynchronized(
					x => x.Max,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			Object = coupler
				.ToReactivePropertyAsSynchronized(x => x.Object)
				.AddTo(disposable);

			Min.SetValidateNotifyError(x =>
				{
					double min;
					string message;

					if (Utilities.TryParse(x, NumberRange.Any, out min, out message))
					{
						double max;

						if (Utilities.TryParse(Max.Value, NumberRange.Any, out max) && min > max)
						{
							message = "MaxはMin以上でなければなりません。";
						}
					}

					return message;
				})
				.Subscribe(_ => Max.ForceValidate())
				.AddTo(disposable);

			Min.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => Min.ForceNotify())
				.AddTo(disposable);

			Max.SetValidateNotifyError(x =>
				{
					double max;
					string message;

					if (Utilities.TryParse(x, NumberRange.Any, out max, out message))
					{
						double min;

						if (Utilities.TryParse(Min.Value, NumberRange.Any, out min) && max < min)
						{
							message = "MaxはMin以上でなければなりません。";
						}
					}

					return message;
				})
				.Subscribe(_ => Min.ForceValidate())
				.AddTo(disposable);

			Max.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => Max.ForceNotify())
				.AddTo(disposable);
		}
	}
}
