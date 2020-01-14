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
		private IDisposable BindToTouch(TouchElementViewModel y)
		{
			CompositeDisposable touchDisposable = new CompositeDisposable();

			y.LocationX
				.BindTo(
					textBoxTouchLocationX,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxTouchLocationX.TextChanged += h,
							h => textBoxTouchLocationX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(touchDisposable);

			y.LocationX
				.BindToErrorProvider(errorProvider, textBoxTouchLocationX)
				.AddTo(touchDisposable);

			y.LocationY
				.BindTo(
					textBoxTouchLocationY,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxTouchLocationY.TextChanged += h,
							h => textBoxTouchLocationY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(touchDisposable);

			y.LocationY
				.BindToErrorProvider(errorProvider, textBoxTouchLocationY)
				.AddTo(touchDisposable);

			y.Layer
				.BindTo(
					numericUpDownTouchLayer,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownTouchLayer.ValueChanged += h,
							h => numericUpDownTouchLayer.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(touchDisposable);

			y.SizeX
				.BindTo(
					textBoxTouchSizeX,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxTouchSizeX.TextChanged += h,
							h => textBoxTouchSizeX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(touchDisposable);

			y.SizeX
				.BindToErrorProvider(errorProvider, textBoxTouchSizeX)
				.AddTo(touchDisposable);

			y.SizeY
				.BindTo(
					textBoxTouchSizeY,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxTouchSizeY.TextChanged += h,
							h => textBoxTouchSizeY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(touchDisposable);

			y.SizeY
				.BindToErrorProvider(errorProvider, textBoxTouchSizeY)
				.AddTo(touchDisposable);

			y.JumpScreen
				.BindTo(
					numericUpDownTouchJumpScreen,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownTouchJumpScreen.ValueChanged += h,
							h => numericUpDownTouchJumpScreen.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(touchDisposable);

			return touchDisposable;
		}
	}
}
