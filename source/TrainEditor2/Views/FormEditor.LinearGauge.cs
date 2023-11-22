using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Panels;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToLinearGauge(LinearGaugeElementViewModel linearGauge)
		{
			CompositeDisposable linearGaugeDisposable = new CompositeDisposable();

			linearGauge.LocationX
				.BindTo(
					textBoxLinearGaugeLocationX,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxLinearGaugeLocationX.TextChanged += h,
							h => textBoxLinearGaugeLocationX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(linearGaugeDisposable);

			linearGauge.LocationX
				.BindToErrorProvider(errorProvider, textBoxLinearGaugeLocationX)
				.AddTo(linearGaugeDisposable);

			linearGauge.LocationY
				.BindTo(
					textBoxLinearGaugeLocationY,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxLinearGaugeLocationY.TextChanged += h,
							h => textBoxLinearGaugeLocationY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(linearGaugeDisposable);

			linearGauge.LocationY
				.BindToErrorProvider(errorProvider, textBoxLinearGaugeLocationY)
				.AddTo(linearGaugeDisposable);

			linearGauge.Layer
				.BindTo(
					numericUpDownLinearGaugeLayer,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownLinearGaugeLayer.ValueChanged += h,
							h => numericUpDownLinearGaugeLayer.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(linearGaugeDisposable);

			linearGauge.DaytimeImage
				.BindTo(
					textBoxLinearGaugeDaytimeImage,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxLinearGaugeDaytimeImage.TextChanged += h,
							h => textBoxLinearGaugeDaytimeImage.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(linearGaugeDisposable);

			linearGauge.NighttimeImage
				.BindTo(
					textBoxLinearGaugeNighttimeImage,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxLinearGaugeNighttimeImage.TextChanged += h,
							h => textBoxLinearGaugeNighttimeImage.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(linearGaugeDisposable);

			linearGauge.TransparentColor
				.BindTo(
					textBoxLinearGaugeTransparentColor,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxLinearGaugeTransparentColor.TextChanged += h,
							h => textBoxLinearGaugeTransparentColor.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(linearGaugeDisposable);

			linearGauge.TransparentColor
				.BindToErrorProvider(errorProvider, textBoxLinearGaugeTransparentColor)
				.AddTo(linearGaugeDisposable);

			linearGauge.Minimum
				.BindTo(
					textBoxLinearGaugeMinimum,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxLinearGaugeMinimum.TextChanged += h,
							h => textBoxLinearGaugeMinimum.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(linearGaugeDisposable);

			linearGauge.Minimum
				.BindToErrorProvider(errorProvider, textBoxLinearGaugeMinimum)
				.AddTo(linearGaugeDisposable);

			linearGauge.Maximum
				.BindTo(
					textBoxLinearGaugeMaximum,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxLinearGaugeMaximum.TextChanged += h,
							h => textBoxLinearGaugeMaximum.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(linearGaugeDisposable);

			linearGauge.Maximum
				.BindToErrorProvider(errorProvider, textBoxLinearGaugeMaximum)
				.AddTo(linearGaugeDisposable);

			linearGauge.DirectionX
				.BindTo(
					numericUpDownLinearGaugeDirectionX,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownLinearGaugeDirectionX.ValueChanged += h,
							h => numericUpDownLinearGaugeDirectionX.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(linearGaugeDisposable);

			linearGauge.DirectionY
				.BindTo(
					numericUpDownLinearGaugeDirectionY,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownLinearGaugeDirectionY.ValueChanged += h,
							h => numericUpDownLinearGaugeDirectionY.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(linearGaugeDisposable);

			linearGauge.Width
				.BindTo(
					numericUpDownLinearGaugeWidth,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownLinearGaugeWidth.ValueChanged += h,
							h => numericUpDownLinearGaugeWidth.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(linearGaugeDisposable);

			return linearGaugeDisposable;
		}
	}
}
