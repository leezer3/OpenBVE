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
		private IDisposable BindToScreen(ScreenViewModel screen)
		{
			CompositeDisposable screenDisposable = new CompositeDisposable();

			screen.Number
				.BindTo(
					numericUpDownScreenNumber,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownScreenNumber.ValueChanged += h,
							h => numericUpDownScreenNumber.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(screenDisposable);

			screen.Number
				.BindToErrorProvider(errorProvider, numericUpDownScreenNumber)
				.AddTo(screenDisposable);

			screen.Layer
				.BindTo(
					numericUpDownScreenLayer,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownScreenLayer.ValueChanged += h,
							h => numericUpDownScreenLayer.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(screenDisposable);

			return screenDisposable;
		}
	}
}
