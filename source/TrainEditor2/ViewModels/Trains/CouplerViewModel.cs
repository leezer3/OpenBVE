using System;
using System.Globalization;
using System.Reactive.Linq;
using OpenBveApi.World;
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

		internal ReactiveProperty<UnitOfLength> MinUnit
		{
			get;
		}

		internal ReactiveProperty<string> Max
		{
			get;
		}

		internal ReactiveProperty<UnitOfLength> MaxUnit
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
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), coupler.Min.UnitValue),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MinUnit = coupler
				.ToReactivePropertyAsSynchronized(
					x => x.Min,
					x => x.UnitValue,
					x => coupler.Min.ToNewUnit(x)
				)
				.AddTo(disposable);

			Max = coupler
				.ToReactivePropertyAsSynchronized(
					x => x.Max,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), coupler.Max.UnitValue),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			MaxUnit = coupler
				.ToReactivePropertyAsSynchronized(
					x => x.Max,
					x => x.UnitValue,
					x => coupler.Max.ToNewUnit(x)
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

						if (Utilities.TryParse(Max.Value, NumberRange.Any, out max) && new Quantity.Length(min, coupler.Min.UnitValue) > new Quantity.Length(max, coupler.Max.UnitValue))
						{
							message = "Max must be greater than or equal to Min.";
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

						if (Utilities.TryParse(Min.Value, NumberRange.Any, out min) && new Quantity.Length(max, coupler.Max.UnitValue) < new Quantity.Length(min, coupler.Min.UnitValue))
						{
							message = "Max must be greater than or equal to Min.";
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
