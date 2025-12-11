using System.Globalization;
using Formats.OpenBve;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class CabViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> PositionX
		{
			get;
		}

		internal ReactiveProperty<string> PositionY
		{
			get;
		}

		internal ReactiveProperty<string> PositionZ
		{
			get;
		}

		internal ReactiveProperty<int> DriverCar
		{
			get;
		}

		internal CabViewModel(Cab cab)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			PositionX = cab
				.ToReactivePropertyAsSynchronized(
					x => x.PositionX,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.Any, out string message);
					return message;
				})
				.AddTo(disposable);

			PositionY = cab
				.ToReactivePropertyAsSynchronized(
					x => x.PositionY,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.Any, out string message);
					return message;
				})
				.AddTo(disposable);

			PositionZ = cab
				.ToReactivePropertyAsSynchronized(
					x => x.PositionZ,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.Any, out string message);
					return message;
				})
				.AddTo(disposable);

			DriverCar = cab
				.ToReactivePropertyAsSynchronized(x => x.DriverCar)
				.AddTo(disposable);
		}
	}
}
