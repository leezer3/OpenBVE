using System.Globalization;
using Formats.OpenBve;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;
using TrainManager.BrakeSystems;

namespace TrainEditor2.ViewModels.Trains
{
	internal class BrakeViewModel : BaseViewModel
	{
		internal ReactiveProperty<BrakeSystemType> BrakeType
		{
			get;
		}

		internal ReactiveProperty<Brake.LocoBrakeTypes> LocoBrakeType
		{
			get;
		}

		internal ReactiveProperty<EletropneumaticBrakeType> BrakeControlSystem
		{
			get;
		}

		internal ReactiveProperty<string> BrakeControlSpeed
		{
			get;
		}

		internal BrakeViewModel(Brake brake)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			BrakeType = brake
				.ToReactivePropertyAsSynchronized(x => x.BrakeType)
				.AddTo(disposable);

			LocoBrakeType = brake
				.ToReactivePropertyAsSynchronized(x => x.LocoBrakeType)
				.AddTo(disposable);

			BrakeControlSystem = brake
				.ToReactivePropertyAsSynchronized(x => x.BrakeControlSystem)
				.AddTo(disposable);

			BrakeControlSpeed = brake
				.ToReactivePropertyAsSynchronized(
					x => x.BrakeControlSpeed,
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
}
