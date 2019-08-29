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
		private IDisposable BindToDigitalGauge(DigitalGaugeElementViewModel y)
		{
			CompositeDisposable digitalGaugeDisposable = new CompositeDisposable();

			y.LocationX
				.BindTo(
					textBoxDigitalGaugeLocationX,
					z => z.Text,
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

			y.LocationX
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeLocationX)
				.AddTo(digitalGaugeDisposable);

			y.LocationY
				.BindTo(
					textBoxDigitalGaugeLocationY,
					z => z.Text,
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

			y.LocationY
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeLocationY)
				.AddTo(digitalGaugeDisposable);

			y.Layer
				.BindTo(
					numericUpDownDigitalGaugeLayer,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownDigitalGaugeLayer.ValueChanged += h,
							h => numericUpDownDigitalGaugeLayer.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalGaugeDisposable);

			y.Radius
				.BindTo(
					textBoxDigitalGaugeRadius,
					z => z.Text,
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

			y.Radius
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeRadius)
				.AddTo(digitalGaugeDisposable);

			y.Color
				.BindTo(
					textBoxDigitalGaugeColor,
					z => z.Text,
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

			y.Color
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeColor)
				.AddTo(digitalGaugeDisposable);

			y.InitialAngle
				.BindTo(
					textBoxDigitalGaugeInitialAngle,
					z => z.Text,
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

			y.InitialAngle
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeInitialAngle)
				.AddTo(digitalGaugeDisposable);

			y.LastAngle
				.BindTo(
					textBoxDigitalGaugeLastAngle,
					z => z.Text,
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

			y.LastAngle
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeLastAngle)
				.AddTo(digitalGaugeDisposable);

			y.Minimum
				.BindTo(
					textBoxDigitalGaugeMinimum,
					z => z.Text,
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

			y.Minimum
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeMinimum)
				.AddTo(digitalGaugeDisposable);

			y.Maximum
				.BindTo(
					textBoxDigitalGaugeMaximum,
					z => z.Text,
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

			y.Maximum
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeMaximum)
				.AddTo(digitalGaugeDisposable);

			y.Step
				.BindTo(
					textBoxDigitalGaugeStep,
					z => z.Text,
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

			y.Step
				.BindToErrorProvider(errorProvider, textBoxDigitalGaugeStep)
				.AddTo(digitalGaugeDisposable);

			return digitalGaugeDisposable;
		}
	}
}
