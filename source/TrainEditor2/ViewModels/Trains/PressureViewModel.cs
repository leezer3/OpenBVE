using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class PressureViewModel : BaseViewModel
	{
		internal ReadOnlyReactivePropertySlim<CompressorViewModel> Compressor
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<MainReservoirViewModel> MainReservoir
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<AuxiliaryReservoirViewModel> AuxiliaryReservoir
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<EqualizingReservoirViewModel> EqualizingReservoir
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<BrakePipeViewModel> BrakePipe
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<StraightAirPipeViewModel> StraightAirPipe
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<BrakeCylinderViewModel> BrakeCylinder
		{
			get;
		}

		internal PressureViewModel(Pressure pressure)
		{
			Compressor = pressure
				.ObserveProperty(x => x.Compressor)
				.Do(_ => Compressor?.Value.Dispose())
				.Select(x => new CompressorViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			MainReservoir = pressure
				.ObserveProperty(x => x.MainReservoir)
				.Do(_ => MainReservoir?.Value.Dispose())
				.Select(x => new MainReservoirViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			AuxiliaryReservoir = pressure
				.ObserveProperty(x => x.AuxiliaryReservoir)
				.Do(_ => AuxiliaryReservoir?.Value.Dispose())
				.Select(x => new AuxiliaryReservoirViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			EqualizingReservoir = pressure
				.ObserveProperty(x => x.EqualizingReservoir)
				.Do(_ => EqualizingReservoir?.Value.Dispose())
				.Select(x => new EqualizingReservoirViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			BrakePipe = pressure
				.ObserveProperty(x => x.BrakePipe)
				.Do(_ => BrakePipe?.Value.Dispose())
				.Select(x => new BrakePipeViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			StraightAirPipe = pressure
				.ObserveProperty(x => x.StraightAirPipe)
				.Do(_ => StraightAirPipe?.Value.Dispose())
				.Select(x => new StraightAirPipeViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			BrakeCylinder = pressure
				.ObserveProperty(x => x.BrakeCylinder)
				.Do(_ => BrakeCylinder?.Value.Dispose())
				.Select(x => new BrakeCylinderViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);
		}
	}
}
