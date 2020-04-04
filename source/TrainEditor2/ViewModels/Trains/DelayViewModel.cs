using System.Globalization;
using OpenBveApi.Units;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class DelayViewModel : BaseViewModel
	{
		internal class EntryViewModel : BaseViewModel
		{
			internal ReactiveProperty<string> Up
			{
				get;
			}

			internal ReactiveProperty<Unit.Time> UpUnit
			{
				get;
			}

			internal ReactiveProperty<string> Down
			{
				get;
			}

			internal ReactiveProperty<Unit.Time> DownUnit
			{
				get;
			}

			internal EntryViewModel(Delay.Entry entry)
			{
				CultureInfo culture = CultureInfo.InvariantCulture;

				Up = entry
					.ToReactivePropertyAsSynchronized(
						x => x.Up,
						x => x.Value.ToString(culture),
						x => new Quantity.Time(double.Parse(x, NumberStyles.Float, culture), entry.Up.UnitValue),
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

				UpUnit = entry
					.ToReactivePropertyAsSynchronized(
						x => x.Up,
						x => x.UnitValue,
						x => entry.Up.ToNewUnit(x)
					)
					.AddTo(disposable);

				Down = entry
					.ToReactivePropertyAsSynchronized(
						x => x.Down,
						x => x.Value.ToString(culture),
						x => new Quantity.Time(double.Parse(x, NumberStyles.Float, culture), entry.Down.UnitValue),
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

				DownUnit = entry
					.ToReactivePropertyAsSynchronized(
						x => x.Down,
						x => x.UnitValue,
						x => entry.Down.ToNewUnit(x)
					)
					.AddTo(disposable);
			}
		}

		internal ReadOnlyReactiveCollection<EntryViewModel> Power
		{
			get;
		}

		internal ReadOnlyReactiveCollection<EntryViewModel> Brake
		{
			get;
		}

		internal ReadOnlyReactiveCollection<EntryViewModel> LocoBrake
		{
			get;
		}

		internal DelayViewModel(Delay delay)
		{
			Power = delay.Power.ToReadOnlyReactiveCollection(x => new EntryViewModel(x)).AddTo(disposable);

			Brake = delay.Brake.ToReadOnlyReactiveCollection(x => new EntryViewModel(x)).AddTo(disposable);

			LocoBrake = delay.LocoBrake.ToReadOnlyReactiveCollection(x => new EntryViewModel(x)).AddTo(disposable);
		}
	}
}
