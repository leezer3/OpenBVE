using System.Linq;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class TrainViewModel : BaseViewModel
	{
		internal ReadOnlyReactivePropertySlim<HandleViewModel> Handle
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<DeviceViewModel> Device
		{
			get;
		}

		internal ReactiveProperty<int> InitialDriverCar
		{
			get;
		}

		internal ReadOnlyReactiveCollection<CarViewModel> Cars
		{
			get;
		}

		internal ReadOnlyReactiveCollection<CouplerViewModel> Couplers
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<CarViewModel> SelectedCar
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<CouplerViewModel> SelectedCoupler
		{
			get;
		}

		internal TrainViewModel(Train train, App app)
		{
			Handle = train
				.ObserveProperty(x => x.Handle)
				.Do(_ => Handle?.Value.Dispose())
				.Select(x => new HandleViewModel(x, train))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Device = train
				.ObserveProperty(x => x.Device)
				.Do(_ => Device?.Value.Dispose())
				.Select(x => new DeviceViewModel(x, train.Handle))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			InitialDriverCar = train
				.ToReactivePropertyAsSynchronized(
					x => x.InitialDriverCar,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			Cars = train.Cars
				.ToReadOnlyReactiveCollection(x =>
				{
					CarViewModel viewModel;

					MotorCar motorCar = x as MotorCar;

					if (motorCar != null)
					{
						ControlledMotorCar controlledMotorCar = motorCar as ControlledMotorCar;

						if (controlledMotorCar != null)
						{
							viewModel = new ControlledMotorCarViewModel(controlledMotorCar, train);
						}
						else
						{
							viewModel = new UncontrolledMotorCarViewModel(motorCar, train);
						}
					}
					else
					{
						ControlledTrailerCar controlledTrailerCar = x as ControlledTrailerCar;

						if (controlledTrailerCar != null)
						{
							viewModel = new ControlledTrailerCarViewModel(controlledTrailerCar);
						}
						else
						{
							viewModel = new UncontrolledTrailerCarViewModel(x);
						}
					}

					return viewModel;
				})
				.AddTo(disposable);

			Couplers = train.Couplers
				.ToReadOnlyReactiveCollection(x => new CouplerViewModel(x))
				.AddTo(disposable);

			SelectedCar = app
				.ObserveProperty(x => x.SelectedTreeItem)
				.Select(x => Cars.FirstOrDefault(y => y.Model == x?.Tag))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedCoupler = app
				.ObserveProperty(x => x.SelectedTreeItem)
				.Select(x => Couplers.FirstOrDefault(y => y.Model == x?.Tag))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			InitialDriverCar
				.SetValidateNotifyError(x => x < 0 || x >= Cars.Count || Cars[x] is UncontrolledMotorCarViewModel || Cars[x] is UncontrolledTrailerCarViewModel ? @"The specified car does not have a cab." : null)
				.AddTo(disposable);
		}
	}
}
