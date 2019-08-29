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
		private IDisposable BindToScreen(ScreenViewModel y)
		{
			CompositeDisposable screenDisposable = new CompositeDisposable();

			y.Number
				.BindTo(
					numericUpDownScreenNumber,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownScreenNumber.ValueChanged += h,
							h => numericUpDownScreenNumber.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(screenDisposable);

			y.Number
				.BindToErrorProvider(errorProvider, numericUpDownScreenNumber)
				.AddTo(screenDisposable);

			y.Layer
				.BindTo(
					numericUpDownScreenLayer,
					z => z.Value,
					BindingMode.TwoWay,
					null,
					z => (int)z,
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
