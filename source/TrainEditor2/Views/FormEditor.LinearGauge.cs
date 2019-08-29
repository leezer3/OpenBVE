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
		private IDisposable BindToLinearGauge(LinearGaugeElementViewModel y)
		{
			CompositeDisposable linearGaugeDisposable = new CompositeDisposable();

			y.LocationX
				.BindTo(
					textBoxLinearGaugeLocationX,
					z => z.Text,
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

			y.LocationX
				.BindToErrorProvider(errorProvider, textBoxLinearGaugeLocationX)
				.AddTo(linearGaugeDisposable);

			y.LocationY
				.BindTo(
					textBoxLinearGaugeLocationY,
					z => z.Text,
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

			y.LocationY
				.BindToErrorProvider(errorProvider, textBoxLinearGaugeLocationY)
				.AddTo(linearGaugeDisposable);

			y.Layer
				.BindTo(
					numericUpDownLinearGaugeLayer,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownLinearGaugeLayer.ValueChanged += h,
							h => numericUpDownLinearGaugeLayer.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(linearGaugeDisposable);

			y.DaytimeImage
				.BindTo(
					textBoxLinearGaugeDaytimeImage,
					z => z.Text,
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

			y.NighttimeImage
				.BindTo(
					textBoxLinearGaugeNighttimeImage,
					z => z.Text,
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

			y.TransparentColor
				.BindTo(
					textBoxLinearGaugeTransparentColor,
					z => z.Text,
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

			y.TransparentColor
				.BindToErrorProvider(errorProvider, textBoxLinearGaugeTransparentColor)
				.AddTo(linearGaugeDisposable);

			y.Minimum
				.BindTo(
					textBoxLinearGaugeMinimum,
					z => z.Text,
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

			y.Minimum
				.BindToErrorProvider(errorProvider, textBoxLinearGaugeMinimum)
				.AddTo(linearGaugeDisposable);

			y.Maximum
				.BindTo(
					textBoxLinearGaugeMaximum,
					z => z.Text,
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

			y.Maximum
				.BindToErrorProvider(errorProvider, textBoxLinearGaugeMaximum)
				.AddTo(linearGaugeDisposable);

			y.DirectionX
				.BindTo(
					numericUpDownLinearGaugeDirectionX,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownLinearGaugeDirectionX.ValueChanged += h,
							h => numericUpDownLinearGaugeDirectionX.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(linearGaugeDisposable);

			y.DirectionY
				.BindTo(
					numericUpDownLinearGaugeDirectionY,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownLinearGaugeDirectionY.ValueChanged += h,
							h => numericUpDownLinearGaugeDirectionY.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(linearGaugeDisposable);

			y.Width
				.BindTo(
					numericUpDownLinearGaugeWidth,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
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
