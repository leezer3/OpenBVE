using System.Globalization;
using OpenBveApi.World;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class CompressorViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> Rate
		{
			get;
		}

		internal ReactiveProperty<Unit.PressureRate> RateUnit
		{
			get;
		}

		internal CompressorViewModel(Compressor compressor)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			Rate = compressor
				.ToReactivePropertyAsSynchronized(
					x => x.Rate,
					x => x.Value.ToString(culture),
					x => new Quantity.PressureRate(double.Parse(x, NumberStyles.Float, culture), compressor.Rate.UnitValue),
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

			RateUnit = compressor
				.ToReactivePropertyAsSynchronized(
					x => x.Rate,
					x => x.UnitValue,
					x => compressor.Rate.ToNewUnit(x)
				)
				.AddTo(disposable);
		}
	}
}
