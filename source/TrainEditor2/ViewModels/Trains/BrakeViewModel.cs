using System.Globalization;
using OpenBveApi.Units;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class BrakeViewModel : BaseViewModel
	{
		internal ReactiveProperty<Brake.BrakeTypes> BrakeType
		{
			get;
		}

		internal ReactiveProperty<Brake.LocoBrakeTypes> LocoBrakeType
		{
			get;
		}

		internal ReactiveProperty<Brake.BrakeControlSystems> BrakeControlSystem
		{
			get;
		}

		internal ReactiveProperty<string> BrakeControlSpeed
		{
			get;
		}

		internal ReactiveProperty<Unit.Velocity> BrakeControlSpeedUnit
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
					x => x.Value.ToString(culture),
					x => new Quantity.Velocity(double.Parse(x, NumberStyles.Float, culture), brake.BrakeControlSpeed.UnitValue),
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

			BrakeControlSpeedUnit = brake
				.ToReactivePropertyAsSynchronized(
					x => x.BrakeControlSpeed,
					x => x.UnitValue,
					x => brake.BrakeControlSpeed.ToNewUnit(x)
				)
				.AddTo(disposable);
		}
	}
}
