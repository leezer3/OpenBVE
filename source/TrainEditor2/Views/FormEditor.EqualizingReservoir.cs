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
		private IDisposable BindToEqualizingReservoir(EqualizingReservoirViewModel equalizingReservoir)
		{
			CompositeDisposable equalizingReservoirDisposable = new CompositeDisposable();

			equalizingReservoir.ChargeRate
				.BindTo(
					textBoxEqualizingReservoirChargeRate,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxEqualizingReservoirChargeRate.TextChanged += h,
							h => textBoxEqualizingReservoirChargeRate.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(equalizingReservoirDisposable);

			equalizingReservoir.ChargeRate
				.BindToErrorProvider(errorProvider, textBoxEqualizingReservoirChargeRate)
				.AddTo(equalizingReservoirDisposable);

			equalizingReservoir.ServiceRate
				.BindTo(
					textBoxEqualizingReservoirServiceRate,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxEqualizingReservoirServiceRate.TextChanged += h,
							h => textBoxEqualizingReservoirServiceRate.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(equalizingReservoirDisposable);

			equalizingReservoir.ServiceRate
				.BindToErrorProvider(errorProvider, textBoxEqualizingReservoirServiceRate)
				.AddTo(equalizingReservoirDisposable);

			equalizingReservoir.EmergencyRate
				.BindTo(
					textBoxEqualizingReservoirEmergencyRate,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxEqualizingReservoirEmergencyRate.TextChanged += h,
							h => textBoxEqualizingReservoirEmergencyRate.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(equalizingReservoirDisposable);

			equalizingReservoir.EmergencyRate
				.BindToErrorProvider(errorProvider, textBoxEqualizingReservoirEmergencyRate)
				.AddTo(equalizingReservoirDisposable);

			return equalizingReservoirDisposable;
		}
	}
}
