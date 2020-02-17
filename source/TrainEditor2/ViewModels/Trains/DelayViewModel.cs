using System.Globalization;
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

			internal ReactiveProperty<string> Down
			{
				get;
			}

			internal EntryViewModel(Delay.Entry entry)
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
