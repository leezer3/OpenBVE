using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToCompressor(CompressorViewModel compressor)
		{
			CompositeDisposable compressorDisposable = new CompositeDisposable();

			compressor.Rate
				.BindTo(
					textBoxCompressorRate,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCompressorRate.TextChanged += h,
							h => textBoxCompressorRate.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(compressorDisposable);

			compressor.Rate
				.BindToErrorProvider(errorProvider, textBoxCompressorRate)
				.AddTo(compressorDisposable);

			return compressorDisposable;
		}
	}
}
