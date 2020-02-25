using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using OpenBveApi.Units;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToAuxiliaryReservoir(AuxiliaryReservoirViewModel auxiliaryReservoir)
		{
			CompositeDisposable auxiliaryReservoirDisposable = new CompositeDisposable();

			auxiliaryReservoir.ChargeRate
				.BindTo(
					textBoxAuxiliaryReservoirChargeRate,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxAuxiliaryReservoirChargeRate.TextChanged += h,
							h => textBoxAuxiliaryReservoirChargeRate.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(auxiliaryReservoirDisposable);

			auxiliaryReservoir.ChargeRate
				.BindToErrorProvider(errorProvider, textBoxAuxiliaryReservoirChargeRate)
				.AddTo(auxiliaryReservoirDisposable);

			auxiliaryReservoir.ChargeRateUnit
				.BindTo(
					comboBoxAuxiliaryReservoirChargeRateUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.PressureRate)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxAuxiliaryReservoirChargeRateUnit.SelectedIndexChanged += h,
							h => comboBoxAuxiliaryReservoirChargeRateUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(auxiliaryReservoirDisposable);

			return auxiliaryReservoirDisposable;
		}
	}
}
