using System.Globalization;
using Formats.OpenBve;
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
						x => x.Parse(),
						ignoreValidationErrorValue: true
					)
					.SetValidateNotifyError(x =>
					{
						Utilities.TryValidate(x, NumberRange.NonNegative, out string message);
						return message;
					})
					.AddTo(disposable);

				Down = entry
					.ToReactivePropertyAsSynchronized(
						x => x.Down,
						x => x.ToString(culture),
						x => x.Parse(),
						ignoreValidationErrorValue: true
					)
					.SetValidateNotifyError(x =>
					{
						Utilities.TryValidate(x, NumberRange.NonNegative, out string message);
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

		internal ReadOnlyReactiveCollection<EntryViewModel> DelayElectricBrake
		{
			get;
		}

		internal DelayViewModel(Delay delay)
		{
			DelayPower = delay.DelayPower.ToReadOnlyReactiveCollection(x => new EntryViewModel(x)).AddTo(disposable);

			DelayBrake = delay.DelayBrake.ToReadOnlyReactiveCollection(x => new EntryViewModel(x)).AddTo(disposable);

			DelayLocoBrake = delay.DelayLocoBrake.ToReadOnlyReactiveCollection(x => new EntryViewModel(x)).AddTo(disposable);

			DelayElectricBrake = delay.DelayElectricBrake.ToReadOnlyReactiveCollection(x => new EntryViewModel(x)).AddTo(disposable);
		}
	}
}
