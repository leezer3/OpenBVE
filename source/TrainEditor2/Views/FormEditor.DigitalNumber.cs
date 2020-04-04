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
		private IDisposable BindToDigitalNumber(DigitalNumberElementViewModel digitalNumber)
		{
			CompositeDisposable digitalNumberDisposable = new CompositeDisposable();

			digitalNumber.LocationX
				.BindTo(
					textBoxDigitalNumberLocationX,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDigitalNumberLocationX.TextChanged += h,
							h => textBoxDigitalNumberLocationX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalNumberDisposable);

			digitalNumber.LocationX
				.BindToErrorProvider(errorProvider, textBoxDigitalNumberLocationX)
				.AddTo(digitalNumberDisposable);

			digitalNumber.LocationY
				.BindTo(
					textBoxDigitalNumberLocationY,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDigitalNumberLocationY.TextChanged += h,
							h => textBoxDigitalNumberLocationY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalNumberDisposable);

			digitalNumber.LocationY
				.BindToErrorProvider(errorProvider, textBoxDigitalNumberLocationY)
				.AddTo(digitalNumberDisposable);

			digitalNumber.Layer
				.BindTo(
					numericUpDownDigitalNumberLayer,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownDigitalNumberLayer.ValueChanged += h,
							h => numericUpDownDigitalNumberLayer.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalNumberDisposable);

			digitalNumber.DaytimeImage
				.BindTo(
					textBoxDigitalNumberDaytimeImage,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDigitalNumberDaytimeImage.TextChanged += h,
							h => textBoxDigitalNumberDaytimeImage.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalNumberDisposable);

			digitalNumber.NighttimeImage
				.BindTo(
					textBoxDigitalNumberNighttimeImage,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDigitalNumberNighttimeImage.TextChanged += h,
							h => textBoxDigitalNumberNighttimeImage.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalNumberDisposable);

			digitalNumber.TransparentColor
				.BindTo(
					textBoxDigitalNumberTransparentColor,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDigitalNumberTransparentColor.TextChanged += h,
							h => textBoxDigitalNumberTransparentColor.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalNumberDisposable);

			digitalNumber.TransparentColor
				.BindToErrorProvider(errorProvider, textBoxDigitalNumberTransparentColor)
				.AddTo(digitalNumberDisposable);

			digitalNumber.Interval
				.BindTo(
					numericUpDownDigitalNumberInterval,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownDigitalNumberInterval.ValueChanged += h,
							h => numericUpDownDigitalNumberInterval.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalNumberDisposable);

			return digitalNumberDisposable;
		}
	}
}
