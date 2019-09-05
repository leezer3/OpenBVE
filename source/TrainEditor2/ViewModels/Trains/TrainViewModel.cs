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

		internal ReactivePropertySlim<CarViewModel> SelectedCar
		{
			get;
		}

		internal ReadOnlyReactiveCollection<CouplerViewModel> Couplers
		{
			get;
		}

		internal ReactivePropertySlim<CouplerViewModel> SelectedCoupler
		{
			get;
		}

		internal TrainViewModel(Train train, App app)
		{
			CompositeDisposable selectedItemDisposable = new CompositeDisposable();

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

			SelectedCar = new ReactivePropertySlim<CarViewModel>();

			Couplers = train.Couplers
				.ToReadOnlyReactiveCollection(x => new CouplerViewModel(x))
				.AddTo(disposable);

			SelectedCoupler = new ReactivePropertySlim<CouplerViewModel>();

			app.ObserveProperty(x => x.SelectedItem)
				.Subscribe(x =>
				{
					selectedItemDisposable.Dispose();
					selectedItemDisposable = new CompositeDisposable();

					x?.PropertyChangedAsObservable()
						.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe)
						.Subscribe(_ =>
						{
							SelectedCar.Value = Cars.FirstOrDefault(y => y.Model == x.Tag);
							SelectedCoupler.Value = Couplers.FirstOrDefault(y => y.Model == x.Tag);
						})
						.AddTo(selectedItemDisposable);
				})
				.AddTo(disposable);

			selectedItemDisposable.AddTo(disposable);
		}
	}
}
