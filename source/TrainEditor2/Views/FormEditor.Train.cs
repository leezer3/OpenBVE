using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToTrain(TrainViewModel x)
		{
			CompositeDisposable trainDisposable = new CompositeDisposable();

			CompositeDisposable handleDisposable = new CompositeDisposable().AddTo(trainDisposable);
			CompositeDisposable deviceDisposable = new CompositeDisposable().AddTo(trainDisposable);
			CompositeDisposable carDisposable = new CompositeDisposable().AddTo(trainDisposable);
			CompositeDisposable couplerDisposable = new CompositeDisposable().AddTo(trainDisposable);

			x.Cars
				.CollectionChangedAsObservable()
				.ToReadOnlyReactivePropertySlim()
				.Subscribe(_ =>
				{
					int index = comboBoxInitialDriverCar.SelectedIndex;

					comboBoxInitialDriverCar.Items.Clear();
					comboBoxInitialDriverCar.Items.AddRange(Enumerable.Range(0, x.Cars.Count).OfType<object>().ToArray());

					if (index < comboBoxInitialDriverCar.Items.Count)
					{
						comboBoxInitialDriverCar.SelectedIndex = index;
					}
					else
					{
						comboBoxInitialDriverCar.SelectedIndex = comboBoxInitialDriverCar.Items.Count - 1;
					}
				})
				.AddTo(trainDisposable);

			x.Handle
				.Subscribe(y =>
				{
					handleDisposable.Dispose();
					handleDisposable = new CompositeDisposable().AddTo(trainDisposable);

					BindToHandle(y).AddTo(handleDisposable);
				})
				.AddTo(trainDisposable);

			x.Device
				.Subscribe(y =>
				{
					deviceDisposable.Dispose();
					deviceDisposable = new CompositeDisposable().AddTo(trainDisposable);

					BindToDevice(y).AddTo(deviceDisposable);
				})
				.AddTo(trainDisposable);

			x.InitialDriverCar
				.BindTo(
					comboBoxInitialDriverCar,
					z => z.SelectedIndex,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxInitialDriverCar.SelectedIndexChanged += h,
							h => comboBoxInitialDriverCar.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(trainDisposable);

			x.InitialDriverCar
				.BindToErrorProvider(errorProvider, comboBoxInitialDriverCar)
				.AddTo(trainDisposable);

			x.SelectedCar
				.Subscribe(y =>
				{
					carDisposable.Dispose();
					carDisposable = new CompositeDisposable().AddTo(trainDisposable);

					if (y != null)
					{
						BindToCar(y).AddTo(carDisposable);
					}
				})
				.AddTo(trainDisposable);

			x.SelectedCar
				.BindTo(
					checkBoxIsMotorCar,
					y => y.Checked,
					y => y is MotorCarViewModel
				)
				.AddTo(trainDisposable);

			x.SelectedCar
				.BindTo(
					checkBoxIsControlledCar,
					y => y.Checked,
					y => y is ControlledMotorCarViewModel || y is ControlledTrailerCarViewModel
				)
				.AddTo(trainDisposable);

			x.SelectedCar
				.BindTo(
					groupBoxCab,
					y => y.Enabled,
					y => y is ControlledMotorCarViewModel || y is ControlledTrailerCarViewModel
				)
				.AddTo(trainDisposable);

			x.SelectedCoupler
				.Subscribe(y =>
				{
					couplerDisposable.Dispose();
					couplerDisposable = new CompositeDisposable().AddTo(trainDisposable);

					if (y != null)
					{
						BindToCoupler(y).AddTo(couplerDisposable);
					}
				})
				.AddTo(trainDisposable);

			return trainDisposable;
		}
	}
}
