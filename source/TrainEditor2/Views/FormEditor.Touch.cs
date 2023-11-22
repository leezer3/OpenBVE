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
		private IDisposable BindToTouch(TouchElementViewModel touch)
		{
			CompositeDisposable touchDisposable = new CompositeDisposable();

			touch.LocationX
				.BindTo(
					textBoxTouchLocationX,
					x => x.Text,
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

			touch.LocationX
				.BindToErrorProvider(errorProvider, textBoxTouchLocationX)
				.AddTo(touchDisposable);

			touch.LocationY
				.BindTo(
					textBoxTouchLocationY,
					x => x.Text,
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

			touch.LocationY
				.BindToErrorProvider(errorProvider, textBoxTouchLocationY)
				.AddTo(touchDisposable);

			touch.Layer
				.BindTo(
					numericUpDownTouchLayer,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownTouchLayer.ValueChanged += h,
							h => numericUpDownTouchLayer.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(touchDisposable);

			touch.SizeX
				.BindTo(
					textBoxTouchSizeX,
					x => x.Text,
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

			touch.SizeX
				.BindToErrorProvider(errorProvider, textBoxTouchSizeX)
				.AddTo(touchDisposable);

			touch.SizeY
				.BindTo(
					textBoxTouchSizeY,
					x => x.Text,
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

			touch.SizeY
				.BindToErrorProvider(errorProvider, textBoxTouchSizeY)
				.AddTo(touchDisposable);

			touch.JumpScreen
				.BindTo(
					numericUpDownTouchJumpScreen,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
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
