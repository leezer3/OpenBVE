using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using OpenBveApi.World;
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

			equalizingReservoir.ChargeRateUnit
				.BindTo(
					comboBoxEqualizingReservoirChargeRateUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.PressureRate)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxEqualizingReservoirChargeRateUnit.SelectedIndexChanged += h,
							h => comboBoxEqualizingReservoirChargeRateUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
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

			equalizingReservoir.ServiceRateUnit
				.BindTo(
					comboBoxEqualizingReservoirServiceRateUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.PressureRate)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxEqualizingReservoirServiceRateUnit.SelectedIndexChanged += h,
							h => comboBoxEqualizingReservoirServiceRateUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
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

			equalizingReservoir.EmergencyRateUnit
				.BindTo(
					comboBoxEqualizingReservoirEmergencyRateUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.PressureRate)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxEqualizingReservoirEmergencyRateUnit.SelectedIndexChanged += h,
							h => comboBoxEqualizingReservoirEmergencyRateUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(equalizingReservoirDisposable);

			return equalizingReservoirDisposable;
		}
	}
}
