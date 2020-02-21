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

			return auxiliaryReservoirDisposable;
		}
	}
}
