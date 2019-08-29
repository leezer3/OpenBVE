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
		private IDisposable BindToDigitalNumber(DigitalNumberElementViewModel y)
		{
			CompositeDisposable digitalNumberDisposable = new CompositeDisposable();

			y.LocationX
				.BindTo(
					textBoxDigitalNumberLocationX,
					z => z.Text,
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

			y.LocationX
				.BindToErrorProvider(errorProvider, textBoxDigitalNumberLocationX)
				.AddTo(digitalNumberDisposable);

			y.LocationY
				.BindTo(
					textBoxDigitalNumberLocationY,
					z => z.Text,
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

			y.LocationY
				.BindToErrorProvider(errorProvider, textBoxDigitalNumberLocationY)
				.AddTo(digitalNumberDisposable);

			y.Layer
				.BindTo(
					numericUpDownDigitalNumberLayer,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownDigitalNumberLayer.ValueChanged += h,
							h => numericUpDownDigitalNumberLayer.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(digitalNumberDisposable);

			y.DaytimeImage
				.BindTo(
					textBoxDigitalNumberDaytimeImage,
					z => z.Text,
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

			y.NighttimeImage
				.BindTo(
					textBoxDigitalNumberNighttimeImage,
					z => z.Text,
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

			y.TransparentColor
				.BindTo(
					textBoxDigitalNumberTransparentColor,
					z => z.Text,
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

			y.TransparentColor
				.BindToErrorProvider(errorProvider, textBoxDigitalNumberTransparentColor)
				.AddTo(digitalNumberDisposable);

			y.Interval
				.BindTo(
					numericUpDownDigitalNumberInterval,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
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
