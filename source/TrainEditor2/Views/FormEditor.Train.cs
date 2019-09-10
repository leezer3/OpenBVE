using System;
using System.Linq;
using System.Reactive.Disposables;
using Reactive.Bindings;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
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
			CompositeDisposable cabDisposable = new CompositeDisposable().AddTo(trainDisposable);
			CompositeDisposable carDisposable = new CompositeDisposable().AddTo(trainDisposable);
			CompositeDisposable couplerDisposable = new CompositeDisposable().AddTo(trainDisposable);

			x.Cars
				.CollectionChangedAsObservable()
				.ToReadOnlyReactivePropertySlim()
				.Subscribe(_ =>
				{
					int index = comboBoxDriverCar.SelectedIndex;

					comboBoxDriverCar.Items.Clear();
					comboBoxDriverCar.Items.AddRange(Enumerable.Range(0, x.Cars.Count).OfType<object>().ToArray());

					if (index < comboBoxDriverCar.Items.Count)
					{
						comboBoxDriverCar.SelectedIndex = index;
					}
					else
					{
						comboBoxDriverCar.SelectedIndex = comboBoxDriverCar.Items.Count - 1;
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

			x.Cab
				.Subscribe(y =>
				{
					cabDisposable.Dispose();
					cabDisposable = new CompositeDisposable().AddTo(trainDisposable);

					BindToCab(y).AddTo(cabDisposable);
				})
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
