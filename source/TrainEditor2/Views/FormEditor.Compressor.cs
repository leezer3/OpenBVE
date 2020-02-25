using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using OpenBveApi.Units;
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

			compressor.RateUnit
				.BindTo(
					comboBoxCompressorRateUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.PressureRate)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxCompressorRateUnit.SelectedIndexChanged += h,
							h => comboBoxCompressorRateUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(compressorDisposable);

			return compressorDisposable;
		}
	}
}
