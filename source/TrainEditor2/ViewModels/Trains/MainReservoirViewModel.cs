using System.Globalization;
using OpenBveApi.World;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class MainReservoirViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> MinimumPressure
		{
			get;
		}

		internal ReactiveProperty<Unit.Pressure> MinimumPressureUnit
		{
			get;
		}

		internal ReactiveProperty<string> MaximumPressure
		{
			get;
		}

		internal ReactiveProperty<Unit.Pressure> MaximumPressureUnit
		{
			get;
		}

		internal MainReservoirViewModel(MainReservoir mainReservoir)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			MinimumPressure = mainReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.MinimumPressure,
					x => x.Value.ToString(culture),
					x => new Quantity.Pressure(double.Parse(x, NumberStyles.Float, culture), mainReservoir.MinimumPressure.UnitValue),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Positive, out result, out message);

					return message;
				})
				.AddTo(disposable);

			MinimumPressureUnit = mainReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.MinimumPressure,
					x => x.UnitValue,
					x => mainReservoir.MinimumPressure.ToNewUnit(x)
				)
				.AddTo(disposable);

			MaximumPressure = mainReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.MaximumPressure,
					x => x.Value.ToString(culture),
					x => new Quantity.Pressure(double.Parse(x, NumberStyles.Float, culture), mainReservoir.MaximumPressure.UnitValue),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Positive, out result, out message);

					return message;
				})
				.AddTo(disposable);

			MaximumPressureUnit = mainReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.MaximumPressure,
					x => x.UnitValue,
					x => mainReservoir.MaximumPressure.ToNewUnit(x)
				)
				.AddTo(disposable);
		}
	}
}
