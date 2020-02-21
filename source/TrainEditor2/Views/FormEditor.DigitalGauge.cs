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
		private IDisposable BindToDigitalGauge(DigitalGaugeElementViewModel digitalGauge)
		{
			CompositeDisposable digitalGaugeDisposable = new CompositeDisposable();

			digitalGauge.LocationX
				.BindTo(
					textBoxDigitalGaugeLocationX,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDigitalGaugeLocationX.TextChanged += h,
							h => textBoxDigitalGaugeLocationX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.LocationX
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeLocationX)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.LocationY
				.BindTo(
					textBoxDigitalGaugeLocationY,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDigitalGaugeLocationY.TextChanged += h,
							h => textBoxDigitalGaugeLocationY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.LocationY
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeLocationY)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.Layer
				.BindTo(
					numericUpDownDigitalGaugeLayer,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownDigitalGaugeLayer.ValueChanged += h,
							h => numericUpDownDigitalGaugeLayer.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.Radius
				.BindTo(
					textBoxDigitalGaugeRadius,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDigitalGaugeRadius.TextChanged += h,
							h => textBoxDigitalGaugeRadius.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.Radius
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeRadius)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.Color
				.BindTo(
					textBoxDigitalGaugeColor,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDigitalGaugeColor.TextChanged += h,
							h => textBoxDigitalGaugeColor.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.Color
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeColor)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.InitialAngle
				.BindTo(
					textBoxDigitalGaugeInitialAngle,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDigitalGaugeInitialAngle.TextChanged += h,
							h => textBoxDigitalGaugeInitialAngle.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.InitialAngle
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeInitialAngle)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.LastAngle
				.BindTo(
					textBoxDigitalGaugeLastAngle,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDigitalGaugeLastAngle.TextChanged += h,
							h => textBoxDigitalGaugeLastAngle.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.LastAngle
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeLastAngle)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.Minimum
				.BindTo(
					textBoxDigitalGaugeMinimum,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDigitalGaugeMinimum.TextChanged += h,
							h => textBoxDigitalGaugeMinimum.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.Minimum
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeMinimum)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.Maximum
				.BindTo(
					textBoxDigitalGaugeMaximum,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDigitalGaugeMaximum.TextChanged += h,
							h => textBoxDigitalGaugeMaximum.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.Maximum
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeMaximum)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.Step
				.BindTo(
					textBoxDigitalGaugeStep,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDigitalGaugeStep.TextChanged += h,
							h => textBoxDigitalGaugeStep.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalGaugeDisposable);

			digitalGauge.Step
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeStep)
				.AddTo(digitalGaugeDisposable);

			return digitalGaugeDisposable;
		}
	}
}
