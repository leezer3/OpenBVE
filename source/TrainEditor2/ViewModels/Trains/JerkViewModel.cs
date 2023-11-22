using System.Globalization;
using System.Reactive.Linq;
using OpenBveApi.World;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class JerkViewModel : BaseViewModel
	{
		internal class EntryViewModel : BaseViewModel
		{
			internal ReactiveProperty<string> Up
			{
				get;
			}

			internal ReactiveProperty<Unit.Jerk> UpUnit
			{
				get;
			}

			internal ReactiveProperty<string> Down
			{
				get;
			}

			internal ReactiveProperty<Unit.Jerk> DownUnit
			{
				get;
			}

			internal EntryViewModel(Jerk.Entry entry)
			{
				CultureInfo culture = CultureInfo.InvariantCulture;

				Up = entry
					.ToReactivePropertyAsSynchronized(
						x => x.Up,
						x => x.Value.ToString(culture),
						x => new Quantity.Jerk(double.Parse(x, NumberStyles.Float, culture), entry.Up.UnitValue),
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
						x => new Quantity.Jerk(double.Parse(x, NumberStyles.Float, culture), entry.Down.UnitValue),
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

		internal ReadOnlyReactivePropertySlim<EntryViewModel> Power
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<EntryViewModel> Brake
		{
			get;
		}

		internal JerkViewModel(Jerk jerk)
		{
			Power = jerk
				.ObserveProperty(x => x.Power)
				.Do(_ => Power?.Value.Dispose())
				.Select(x => new EntryViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Brake = jerk
				.ObserveProperty(x => x.Brake)
				.Do(_ => Brake?.Value.Dispose())
				.Select(x => new EntryViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);
		}
	}
}
