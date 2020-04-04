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
		private IDisposable BindToNeedle(NeedleElementViewModel needle)
		{
			CompositeDisposable needleDisposable = new CompositeDisposable();

			needle.LocationX
				.BindTo(
					textBoxNeedleLocationX,
					x => x.Text,
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

			needle.LocationX
				.BindToErrorProvider(errorProvider, textBoxNeedleLocationX)
				.AddTo(needleDisposable);

			needle.LocationY
				.BindTo(
					textBoxNeedleLocationY,
					x => x.Text,
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

			needle.LocationY
				.BindToErrorProvider(errorProvider, textBoxNeedleLocationY)
				.AddTo(needleDisposable);

			needle.Layer
				.BindTo(
					numericUpDownNeedleLayer,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownNeedleLayer.ValueChanged += h,
							h => numericUpDownNeedleLayer.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(needleDisposable);

			needle.DaytimeImage
				.BindTo(
					textBoxNeedleDaytimeImage,
					x => x.Text,
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

			needle.NighttimeImage
				.BindTo(
					textBoxNeedleNighttimeImage,
					x => x.Text,
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

			needle.TransparentColor
				.BindTo(
					textBoxNeedleTransparentColor,
					x => x.Text,
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

			needle.TransparentColor
				.BindToErrorProvider(errorProvider, textBoxNeedleTransparentColor)
				.AddTo(needleDisposable);

			needle.DefinedRadius
				.BindTo(
					checkBoxNeedleDefinedRadius,
					x => x.Checked,
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

			needle.DefinedRadius
				.BindTo(
					labelNeedleRadius,
					x => x.Enabled
				)
				.AddTo(needleDisposable);

			needle.DefinedRadius
				.BindTo(
					textBoxNeedleRadius,
					x => x.Enabled
				)
				.AddTo(needleDisposable);

			needle.Radius
				.BindTo(
					textBoxNeedleRadius,
					x => x.Text,
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

			needle.Radius
				.BindToErrorProvider(errorProvider, textBoxNeedleRadius)
				.AddTo(needleDisposable);

			needle.Color
				.BindTo(
					textBoxNeedleColor,
					x => x.Text,
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

			needle.Color
				.BindToErrorProvider(errorProvider, textBoxNeedleColor)
				.AddTo(needleDisposable);

			needle.DefinedOrigin
				.BindTo(
					checkBoxNeedleDefinedOrigin,
					x => x.Checked,
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

			needle.DefinedOrigin
				.BindTo(
					groupBoxNeedleOrigin,
					x => x.Enabled
				)
				.AddTo(needleDisposable);

			needle.OriginX
				.BindTo(
					textBoxNeedleOriginX,
					x => x.Text,
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

			needle.OriginX
				.BindToErrorProvider(errorProvider, textBoxNeedleOriginX)
				.AddTo(needleDisposable);

			needle.OriginY
				.BindTo(
					textBoxNeedleOriginY,
					x => x.Text,
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

			needle.OriginY
				.BindToErrorProvider(errorProvider, textBoxNeedleOriginY)
				.AddTo(needleDisposable);

			needle.InitialAngle
				.BindTo(
					textBoxNeedleInitialAngle,
					x => x.Text,
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

			needle.InitialAngle
				.BindToErrorProvider(errorProvider, textBoxNeedleInitialAngle)
				.AddTo(needleDisposable);

			needle.LastAngle
				.BindTo(
					textBoxNeedleLastAngle,
					x => x.Text,
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

			needle.LastAngle
				.BindToErrorProvider(errorProvider, textBoxNeedleLastAngle)
				.AddTo(needleDisposable);

			needle.Minimum
				.BindTo(
					textBoxNeedleMinimum,
					x => x.Text,
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

			needle.Minimum
				.BindToErrorProvider(errorProvider, textBoxNeedleMinimum)
				.AddTo(needleDisposable);

			needle.Maximum
				.BindTo(
					textBoxNeedleMaximum,
					x => x.Text,
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

			needle.Maximum
				.BindToErrorProvider(errorProvider, textBoxNeedleMaximum)
				.AddTo(needleDisposable);

			needle.DefinedNaturalFreq
				.BindTo(
					checkBoxNeedleDefinedNaturalFreq,
					x => x.Checked,
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

			needle.DefinedNaturalFreq
				.BindTo(
					labelNeedleNaturalFreq,
					x => x.Enabled
				)
				.AddTo(needleDisposable);

			needle.DefinedNaturalFreq
				.BindTo(
					textBoxNeedleNaturalFreq,
					x => x.Enabled
				)
				.AddTo(needleDisposable);

			needle.NaturalFreq
				.BindTo(
					textBoxNeedleNaturalFreq,
					x => x.Text,
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

			needle.NaturalFreq
				.BindToErrorProvider(errorProvider, textBoxNeedleNaturalFreq)
				.AddTo(needleDisposable);

			needle.DefinedDampingRatio
				.BindTo(
					checkBoxNeedleDefinedDampingRatio,
					x => x.Checked,
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

			needle.DefinedDampingRatio
				.BindTo(
					labelNeedleDampingRatio,
					x => x.Enabled
				)
				.AddTo(needleDisposable);

			needle.DefinedDampingRatio
				.BindTo(
					textBoxNeedleDampingRatio,
					x => x.Enabled
				)
				.AddTo(needleDisposable);

			needle.DampingRatio
				.BindTo(
					textBoxNeedleDampingRatio,
					x => x.Text,
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

			needle.DampingRatio
				.BindToErrorProvider(errorProvider, textBoxNeedleDampingRatio)
				.AddTo(needleDisposable);

			needle.Backstop
				.BindTo(
					checkBoxNeedleBackstop,
					x => x.Checked,
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

			needle.Smoothed
				.BindTo(
					checkBoxNeedleSmoothed,
					x => x.Checked,
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
