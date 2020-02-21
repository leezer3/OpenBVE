using System.Globalization;
using System.Reactive.Linq;
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

			internal ReactiveProperty<string> Down
			{
				get;
			}

			internal EntryViewModel(Jerk.Entry entry)
			{
				CultureInfo culture = CultureInfo.InvariantCulture;

				Up = entry
					.ToReactivePropertyAsSynchronized(
						x => x.Up,
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

				Down = entry
					.ToReactivePropertyAsSynchronized(
						x => x.Down,
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
