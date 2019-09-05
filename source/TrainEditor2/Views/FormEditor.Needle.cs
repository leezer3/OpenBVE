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
		private IDisposable BindToNeedle(NeedleElementViewModel y)
		{
			CompositeDisposable needleDisposable = new CompositeDisposable();

			y.LocationX
				.BindTo(
					textBoxNeedleLocationX,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNeedleLocationX.TextChanged += h,
							h => textBoxNeedleLocationX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.LocationX
				.BindToErrorProvider(errorProvider, textBoxNeedleLocationX)
				.AddTo(needleDisposable);

			y.LocationY
				.BindTo(
					textBoxNeedleLocationY,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNeedleLocationY.TextChanged += h,
							h => textBoxNeedleLocationY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.LocationY
				.BindToErrorProvider(errorProvider, textBoxNeedleLocationY)
				.AddTo(needleDisposable);

			y.Layer
				.BindTo(
					numericUpDownNeedleLayer,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownNeedleLayer.ValueChanged += h,
							h => numericUpDownNeedleLayer.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.DaytimeImage
				.BindTo(
					textBoxNeedleDaytimeImage,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNeedleDaytimeImage.TextChanged += h,
							h => textBoxNeedleDaytimeImage.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.NighttimeImage
				.BindTo(
					textBoxNeedleNighttimeImage,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNeedleNighttimeImage.TextChanged += h,
							h => textBoxNeedleNighttimeImage.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.TransparentColor
				.BindTo(
					textBoxNeedleTransparentColor,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNeedleTransparentColor.TextChanged += h,
							h => textBoxNeedleTransparentColor.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.TransparentColor
				.BindToErrorProvider(errorProvider, textBoxNeedleTransparentColor)
				.AddTo(needleDisposable);

			y.DefinedRadius
				.BindTo(
					checkBoxNeedleDefinedRadius,
					z => z.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxNeedleDefinedRadius.CheckedChanged += h,
							h => checkBoxNeedleDefinedRadius.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.DefinedRadius
				.BindTo(
					labelNeedleRadius,
					z => z.Enabled
				)
				.AddTo(needleDisposable);

			y.DefinedRadius
				.BindTo(
					textBoxNeedleRadius,
					z => z.Enabled
				)
				.AddTo(needleDisposable);

			y.Radius
				.BindTo(
					textBoxNeedleRadius,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNeedleRadius.TextChanged += h,
							h => textBoxNeedleRadius.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.Radius
				.BindToErrorProvider(errorProvider, textBoxNeedleRadius)
				.AddTo(needleDisposable);

			y.Color
				.BindTo(
					textBoxNeedleColor,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNeedleColor.TextChanged += h,
							h => textBoxNeedleColor.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.Color
				.BindToErrorProvider(errorProvider, textBoxNeedleColor)
				.AddTo(needleDisposable);

			y.DefinedOrigin
				.BindTo(
					checkBoxNeedleDefinedOrigin,
					z => z.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxNeedleDefinedOrigin.CheckedChanged += h,
							h => checkBoxNeedleDefinedOrigin.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.DefinedOrigin
				.BindTo(
					groupBoxNeedleOrigin,
					z => z.Enabled
				)
				.AddTo(needleDisposable);

			y.OriginX
				.BindTo(
					textBoxNeedleOriginX,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNeedleOriginX.TextChanged += h,
							h => textBoxNeedleOriginX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.OriginX
				.BindToErrorProvider(errorProvider, textBoxNeedleOriginX)
				.AddTo(needleDisposable);

			y.OriginY
				.BindTo(
					textBoxNeedleOriginY,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNeedleOriginY.TextChanged += h,
							h => textBoxNeedleOriginY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.OriginY
				.BindToErrorProvider(errorProvider, textBoxNeedleOriginY)
				.AddTo(needleDisposable);

			y.InitialAngle
				.BindTo(
					textBoxNeedleInitialAngle,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNeedleInitialAngle.TextChanged += h,
							h => textBoxNeedleInitialAngle.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.InitialAngle
				.BindToErrorProvider(errorProvider, textBoxNeedleInitialAngle)
				.AddTo(needleDisposable);

			y.LastAngle
				.BindTo(
					textBoxNeedleLastAngle,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNeedleLastAngle.TextChanged += h,
							h => textBoxNeedleLastAngle.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.LastAngle
				.BindToErrorProvider(errorProvider, textBoxNeedleLastAngle)
				.AddTo(needleDisposable);

			y.Minimum
				.BindTo(
					textBoxNeedleMinimum,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNeedleMinimum.TextChanged += h,
							h => textBoxNeedleMinimum.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.Minimum
				.BindToErrorProvider(errorProvider, textBoxNeedleMinimum)
				.AddTo(needleDisposable);

			y.Maximum
				.BindTo(
					textBoxNeedleMaximum,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNeedleMaximum.TextChanged += h,
							h => textBoxNeedleMaximum.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.Maximum
				.BindToErrorProvider(errorProvider, textBoxNeedleMaximum)
				.AddTo(needleDisposable);

			y.DefinedNaturalFreq
				.BindTo(
					checkBoxNeedleDefinedNaturalFreq,
					z => z.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxNeedleDefinedNaturalFreq.CheckedChanged += h,
							h => checkBoxNeedleDefinedNaturalFreq.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.DefinedNaturalFreq
				.BindTo(
					labelNeedleNaturalFreq,
					z => z.Enabled
				)
				.AddTo(needleDisposable);

			y.DefinedNaturalFreq
				.BindTo(
					textBoxNeedleNaturalFreq,
					z => z.Enabled
				)
				.AddTo(needleDisposable);

			y.NaturalFreq
				.BindTo(
					textBoxNeedleNaturalFreq,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNeedleNaturalFreq.TextChanged += h,
							h => textBoxNeedleNaturalFreq.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.NaturalFreq
				.BindToErrorProvider(errorProvider, textBoxNeedleNaturalFreq)
				.AddTo(needleDisposable);

			y.DefinedDampingRatio
				.BindTo(
					checkBoxNeedleDefinedDampingRatio,
					z => z.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxNeedleDefinedDampingRatio.CheckedChanged += h,
							h => checkBoxNeedleDefinedDampingRatio.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.DefinedDampingRatio
				.BindTo(
					labelNeedleDampingRatio,
					z => z.Enabled
				)
				.AddTo(needleDisposable);

			y.DefinedDampingRatio
				.BindTo(
					textBoxNeedleDampingRatio,
					z => z.Enabled
				)
				.AddTo(needleDisposable);

			y.DampingRatio
				.BindTo(
					textBoxNeedleDampingRatio,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNeedleDampingRatio.TextChanged += h,
							h => textBoxNeedleDampingRatio.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.DampingRatio
				.BindToErrorProvider(errorProvider, textBoxNeedleDampingRatio)
				.AddTo(needleDisposable);

			y.Backstop
				.BindTo(
					checkBoxNeedleBackstop,
					z => z.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxNeedleBackstop.CheckedChanged += h,
							h => checkBoxNeedleBackstop.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			y.Smoothed
				.BindTo(
					checkBoxNeedleSmoothed,
					z => z.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxNeedleSmoothed.CheckedChanged += h,
							h => checkBoxNeedleSmoothed.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			return needleDisposable;
		}
	}
}
