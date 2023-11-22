using System.Globalization;
using OpenBveApi.World;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class AuxiliaryReservoirViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> ChargeRate
		{
			get;
		}

		internal ReactiveProperty<Unit.PressureRate> ChargeRateUnit
		{
			get;
		}

		internal AuxiliaryReservoirViewModel(AuxiliaryReservoir auxiliaryReservoir)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			ChargeRate = auxiliaryReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.ChargeRate,
					x => x.Value.ToString(culture),
					x => new Quantity.PressureRate(double.Parse(x, NumberStyles.Float, culture), auxiliaryReservoir.ChargeRate.UnitValue),
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

			ChargeRateUnit = auxiliaryReservoir
				.ToReactivePropertyAsSynchronized(
					x => x.ChargeRate,
					x => x.UnitValue,
					x => auxiliaryReservoir.ChargeRate.ToNewUnit(x)
				)
				.AddTo(disposable);
		}
	}
}
