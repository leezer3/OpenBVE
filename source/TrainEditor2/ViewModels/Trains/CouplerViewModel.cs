﻿using System;
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
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			Max = coupler
				.ToReactivePropertyAsSynchronized(
					x => x.Max,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			Object = coupler
				.ToReactivePropertyAsSynchronized(x => x.Object)
				.AddTo(disposable);

			Min.SetValidateNotifyError(x =>
				{
					if (Utilities.TryParse(x, NumberRange.Any, out double min, out string message))
					{
						if (Utilities.TryParse(Max.Value, NumberRange.Any, out double max) && min > max)
						{
							message = Utilities.GetInterfaceString("message", "mustbe_greater_than");
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
					if (Utilities.TryParse(x, NumberRange.Any, out double max, out string message))
					{
						if (Utilities.TryParse(Min.Value, NumberRange.Any, out double min) && max < min)
						{
							message = Utilities.GetInterfaceString("message", "mustbe_greater_than");
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
