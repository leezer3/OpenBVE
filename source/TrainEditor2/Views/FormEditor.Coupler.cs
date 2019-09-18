using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToCoupler(CouplerViewModel y)
		{
			CompositeDisposable couplerDisposable = new CompositeDisposable();

			y.Min
				.BindTo(
					textBoxCouplerMin,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCouplerMin.TextChanged += h,
							h => textBoxCouplerMin.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(couplerDisposable);

			y.Min
				.BindToErrorProvider(errorProvider, textBoxCouplerMin)
				.AddTo(couplerDisposable);

			y.Max
				.BindTo(
					textBoxCouplerMax,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCouplerMax.TextChanged += h,
							h => textBoxCouplerMax.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(couplerDisposable);

			y.Max
				.BindToErrorProvider(errorProvider, textBoxCouplerMax)
				.AddTo(couplerDisposable);

			y.Object
				.BindTo(
					textBoxCouplerObject,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCouplerObject.TextChanged += h,
							h => textBoxCouplerObject.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(couplerDisposable);

			

			return couplerDisposable;
		}
	}
}
