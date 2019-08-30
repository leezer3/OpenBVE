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
						double.Parse,
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
						double.Parse,
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

		internal ReadOnlyReactiveCollection<EntryViewModel> DelayPower
		{
			get;
		}

		internal ReadOnlyReactiveCollection<EntryViewModel> DelayBrake
		{
			get;
		}

		internal ReadOnlyReactiveCollection<EntryViewModel> DelayLocoBrake
		{
			get;
		}

		internal DelayViewModel(Delay delay)
		{
			DelayPower = delay.DelayPower.ToReadOnlyReactiveCollection(x => new EntryViewModel(x)).AddTo(disposable);

			DelayBrake = delay.DelayBrake.ToReadOnlyReactiveCollection(x => new EntryViewModel(x)).AddTo(disposable);

			DelayLocoBrake = delay.DelayLocoBrake.ToReadOnlyReactiveCollection(x => new EntryViewModel(x)).AddTo(disposable);
		}
	}
}
