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
		private IDisposable BindToCab(CabViewModel y)
		{
			CompositeDisposable cabDisposable = new CompositeDisposable();

			y.PositionX
				.BindTo(
					textBoxCabX,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCabX.TextChanged += h,
							h => textBoxCabX.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cabDisposable);

			y.PositionX
				.BindToErrorProvider(errorProvider, textBoxCabX)
				.AddTo(cabDisposable);

			y.PositionY
				.BindTo(
					textBoxCabY,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCabY.TextChanged += h,
							h => textBoxCabY.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cabDisposable);

			y.PositionY
				.BindToErrorProvider(errorProvider, textBoxCabY)
				.AddTo(cabDisposable);

			y.PositionZ
				.BindTo(
					textBoxCabZ,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCabZ.TextChanged += h,
							h => textBoxCabZ.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cabDisposable);

			y.PositionZ
				.BindToErrorProvider(errorProvider, textBoxCabZ)
				.AddTo(cabDisposable);

			y.DriverCar
				.BindTo(
					comboBoxDriverCar,
					z => z.SelectedIndex,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxDriverCar.SelectedIndexChanged += h,
							h => comboBoxDriverCar.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(cabDisposable);

			return cabDisposable;
		}
	}
}
