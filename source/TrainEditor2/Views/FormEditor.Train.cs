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
		private IDisposable BindToTrain(TrainViewModel train)
		{
			CompositeDisposable trainDisposable = new CompositeDisposable();
			CompositeDisposable handleDisposable = new CompositeDisposable().AddTo(trainDisposable);
			CompositeDisposable deviceDisposable = new CompositeDisposable().AddTo(trainDisposable);
			CompositeDisposable carDisposable = new CompositeDisposable().AddTo(trainDisposable);
			CompositeDisposable couplerDisposable = new CompositeDisposable().AddTo(trainDisposable);

			train.Cars
				.CollectionChangedAsObservable()
				.ToReadOnlyReactivePropertySlim()
				.Subscribe(_ =>
				{
					int index = comboBoxInitialDriverCar.SelectedIndex;

					comboBoxInitialDriverCar.Items.Clear();
					comboBoxInitialDriverCar.Items.AddRange(Enumerable.Range(0, train.Cars.Count).OfType<object>().ToArray());

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

			train.Handle
				.Subscribe(x =>
				{
					handleDisposable.Dispose();
					handleDisposable = new CompositeDisposable().AddTo(trainDisposable);

					BindToHandle(x).AddTo(handleDisposable);
				})
				.AddTo(trainDisposable);

			train.Device
				.Subscribe(x =>
				{
					deviceDisposable.Dispose();
					deviceDisposable = new CompositeDisposable().AddTo(trainDisposable);

					BindToDevice(x).AddTo(deviceDisposable);
				})
				.AddTo(trainDisposable);

			train.InitialDriverCar
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

			train.InitialDriverCar
				.BindToErrorProvider(errorProvider, comboBoxInitialDriverCar)
				.AddTo(trainDisposable);

			train.SelectedCar
				.Subscribe(x =>
				{
					carDisposable.Dispose();
					carDisposable = new CompositeDisposable().AddTo(trainDisposable);

					if (x != null)
					{
						BindToCar(x).AddTo(carDisposable);
					}
				})
				.AddTo(trainDisposable);

			train.SelectedCar
				.BindTo(
					checkBoxIsMotorCar,
					x => x.Checked,
					x => x is MotorCarViewModel
				)
				.AddTo(trainDisposable);

			train.SelectedCar
				.BindTo(
					checkBoxIsControlledCar,
					x => x.Checked,
					x => x is ControlledMotorCarViewModel || x is ControlledTrailerCarViewModel
				)
				.AddTo(trainDisposable);

			train.SelectedCar
				.BindTo(
					groupBoxCab,
					x => x.Enabled,
					x => x is ControlledMotorCarViewModel || x is ControlledTrailerCarViewModel
				)
				.AddTo(trainDisposable);

			train.SelectedCoupler
				.Subscribe(x =>
				{
					couplerDisposable.Dispose();
					couplerDisposable = new CompositeDisposable().AddTo(trainDisposable);

					if (x != null)
					{
						BindToCoupler(x).AddTo(couplerDisposable);
					}
				})
				.AddTo(trainDisposable);

			return trainDisposable;
		}
	}
}
