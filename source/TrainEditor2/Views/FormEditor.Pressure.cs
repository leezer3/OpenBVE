using System;
using System.Reactive.Disposables;
using Reactive.Bindings.Extensions;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToPressure(PressureViewModel pressure)
		{
			CompositeDisposable pressureDisposable = new CompositeDisposable();
			CompositeDisposable compressorDisposable = new CompositeDisposable().AddTo(pressureDisposable);
			CompositeDisposable mainReservoirDisposable = new CompositeDisposable().AddTo(pressureDisposable);
			CompositeDisposable auxiliaryReservoirDisposable = new CompositeDisposable().AddTo(pressureDisposable);
			CompositeDisposable equalizingReservoirDisposable = new CompositeDisposable().AddTo(pressureDisposable);
			CompositeDisposable brakePipeDisposable = new CompositeDisposable().AddTo(pressureDisposable);
			CompositeDisposable straightAirPipeDisposable = new CompositeDisposable().AddTo(pressureDisposable);
			CompositeDisposable brakeCylinderDisposable = new CompositeDisposable().AddTo(pressureDisposable);

			pressure.Compressor
				.Subscribe(x =>
				{
					compressorDisposable.Dispose();
					compressorDisposable = new CompositeDisposable().AddTo(pressureDisposable);

					BindToCompressor(x).AddTo(compressorDisposable);
				})
				.AddTo(pressureDisposable);

			pressure.MainReservoir
				.Subscribe(x =>
				{
					mainReservoirDisposable.Dispose();
					mainReservoirDisposable = new CompositeDisposable().AddTo(pressureDisposable);

					BindToMainReservoir(x).AddTo(mainReservoirDisposable);
				})
				.AddTo(pressureDisposable);

			pressure.AuxiliaryReservoir
				.Subscribe(x =>
				{
					auxiliaryReservoirDisposable.Dispose();
					auxiliaryReservoirDisposable = new CompositeDisposable().AddTo(pressureDisposable);

					BindToAuxiliaryReservoir(x).AddTo(auxiliaryReservoirDisposable);
				})
				.AddTo(pressureDisposable);

			pressure.EqualizingReservoir
				.Subscribe(x =>
				{
					equalizingReservoirDisposable.Dispose();
					equalizingReservoirDisposable = new CompositeDisposable().AddTo(pressureDisposable);

					BindToEqualizingReservoir(x).AddTo(equalizingReservoirDisposable);
				})
				.AddTo(pressureDisposable);

			pressure.BrakePipe
				.Subscribe(x =>
				{
					brakePipeDisposable.Dispose();
					brakePipeDisposable = new CompositeDisposable().AddTo(pressureDisposable);

					BindToBrakePipe(x).AddTo(brakePipeDisposable);
				})
				.AddTo(pressureDisposable);

			pressure.StraightAirPipe
				.Subscribe(x =>
				{
					straightAirPipeDisposable.Dispose();
					straightAirPipeDisposable = new CompositeDisposable().AddTo(pressureDisposable);

					BindToStraightAirPipe(x).AddTo(straightAirPipeDisposable);
				})
				.AddTo(pressureDisposable);

			pressure.BrakeCylinder
				.Subscribe(x =>
				{
					brakeCylinderDisposable.Dispose();
					brakeCylinderDisposable = new CompositeDisposable().AddTo(pressureDisposable);

					BindToBrakeCylinder(x).AddTo(brakeCylinderDisposable);
				})
				.AddTo(pressureDisposable);

			return pressureDisposable;
		}
	}
}
