using System;
using System.Linq;
using System.Reactive.Disposables;
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

		internal ReadOnlyReactivePropertySlim<CabViewModel> Cab
		{
			get;
		}

		internal ReadOnlyReactiveCollection<CarViewModel> Cars
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<CarViewModel> SelectedCar
		{
			get;
		}

		internal ReadOnlyReactiveCollection<CouplerViewModel> Couplers
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

			Cab = train
				.ObserveProperty(x => x.Cab)
				.Do(_ => Cab?.Value.Dispose())
				.Select(x => new CabViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Cars = train.Cars
				.ToReadOnlyReactiveCollection(x =>
				{
					MotorCar motorCar = x as MotorCar;
					TrailerCar trailerCar = x as TrailerCar;

					CarViewModel viewModel = null;

					if (motorCar != null)
					{
						viewModel = new MotorCarViewModel(motorCar, train);
					}

					if (trailerCar != null)
					{
						viewModel = new TrailerCarViewModel(trailerCar);
					}

					return viewModel;
				})
				.AddTo(disposable);

			Couplers = train.Couplers
				.ToReadOnlyReactiveCollection(x => new CouplerViewModel(x))
				.AddTo(disposable);

			SelectedCar = app
				.ObserveProperty(x => x.SelectedItem)
				.Where(x => x != null)
				.Select(x => Cars.FirstOrDefault(y => y.Model == x.Tag))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedCoupler = app
				.ObserveProperty(x => x.SelectedItem)
				.Where(x => x != null)
				.Select(x => Couplers.FirstOrDefault(y => y.Model == x.Tag))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);
		}
	}
}
