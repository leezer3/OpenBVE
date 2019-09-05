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
		private IDisposable BindToPressure(PressureViewModel z)
		{
			CompositeDisposable pressureDisposable = new CompositeDisposable();

			z.BrakeCylinderServiceMaximumPressure
				.BindTo(
					textBoxBrakeCylinderServiceMaximumPressure,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxBrakeCylinderServiceMaximumPressure.TextChanged += h,
							h => textBoxBrakeCylinderServiceMaximumPressure.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(pressureDisposable);

			z.BrakeCylinderServiceMaximumPressure
				.BindToErrorProvider(errorProvider, textBoxBrakeCylinderServiceMaximumPressure)
				.AddTo(pressureDisposable);

			z.BrakeCylinderEmergencyMaximumPressure
				.BindTo(
					textBoxBrakeCylinderEmergencyMaximumPressure,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxBrakeCylinderEmergencyMaximumPressure.TextChanged += h,
							h => textBoxBrakeCylinderEmergencyMaximumPressure.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(pressureDisposable);

			z.BrakeCylinderEmergencyMaximumPressure
				.BindToErrorProvider(errorProvider, textBoxBrakeCylinderEmergencyMaximumPressure)
				.AddTo(pressureDisposable);

			z.MainReservoirMinimumPressure
				.BindTo(
					textBoxMainReservoirMinimumPressure,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxMainReservoirMinimumPressure.TextChanged += h,
							h => textBoxMainReservoirMinimumPressure.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(pressureDisposable);

			z.MainReservoirMinimumPressure
				.BindToErrorProvider(errorProvider, textBoxMainReservoirMinimumPressure)
				.AddTo(pressureDisposable);

			z.MainReservoirMaximumPressure
				.BindTo(
					textBoxMainReservoirMaximumPressure,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxMainReservoirMaximumPressure.TextChanged += h,
							h => textBoxMainReservoirMaximumPressure.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(pressureDisposable);

			z.MainReservoirMaximumPressure
				.BindToErrorProvider(errorProvider, textBoxMainReservoirMaximumPressure)
				.AddTo(pressureDisposable);

			z.BrakePipeNormalPressure
				.BindTo(
					textBoxBrakePipeNormalPressure,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxBrakePipeNormalPressure.TextChanged += h,
							h => textBoxBrakePipeNormalPressure.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(pressureDisposable);

			z.BrakePipeNormalPressure
				.BindToErrorProvider(errorProvider, textBoxBrakePipeNormalPressure)
				.AddTo(pressureDisposable);

			return pressureDisposable;
		}
	}
}
