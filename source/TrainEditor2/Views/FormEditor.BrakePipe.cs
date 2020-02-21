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
		private IDisposable BindToBrakePipe(BrakePipeViewModel brakePipe)
		{
			CompositeDisposable brakePipeDisposable = new CompositeDisposable();

			brakePipe.NormalPressure
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
				.AddTo(brakePipeDisposable);

			brakePipe.NormalPressure
				.BindToErrorProvider(errorProvider, textBoxBrakePipeNormalPressure)
				.AddTo(brakePipeDisposable);

			brakePipe.ChargeRate
				.BindTo(
					textBoxBrakePipeChargeRate,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxBrakePipeChargeRate.TextChanged += h,
							h => textBoxBrakePipeChargeRate.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakePipeDisposable);

			brakePipe.ChargeRate
				.BindToErrorProvider(errorProvider, textBoxBrakePipeChargeRate)
				.AddTo(brakePipeDisposable);

			brakePipe.ServiceRate
				.BindTo(
					textBoxBrakePipeServiceRate,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxBrakePipeServiceRate.TextChanged += h,
							h => textBoxBrakePipeServiceRate.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakePipeDisposable);

			brakePipe.ServiceRate
				.BindToErrorProvider(errorProvider, textBoxBrakePipeServiceRate)
				.AddTo(brakePipeDisposable);

			brakePipe.EmergencyRate
				.BindTo(
					textBoxBrakePipeEmergencyRate,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxBrakePipeEmergencyRate.TextChanged += h,
							h => textBoxBrakePipeEmergencyRate.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakePipeDisposable);

			brakePipe.EmergencyRate
				.BindToErrorProvider(errorProvider, textBoxBrakePipeEmergencyRate)
				.AddTo(brakePipeDisposable);

			return brakePipeDisposable;
		}
	}
}
