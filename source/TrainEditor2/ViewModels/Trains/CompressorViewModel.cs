using System.Globalization;
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

		internal CompressorViewModel(Compressor compressor)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			Rate = compressor
				.ToReactivePropertyAsSynchronized(
					x => x.Rate,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
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
		}
	}
}
