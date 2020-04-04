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
		private IDisposable BindToMainReservoir(MainReservoirViewModel mainReservoir)
		{
			CompositeDisposable mainReservoirDisposable = new CompositeDisposable();

			mainReservoir.MinimumPressure
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
				.AddTo(mainReservoirDisposable);

			mainReservoir.MinimumPressure
				.BindToErrorProvider(errorProvider, textBoxMainReservoirMinimumPressure)
				.AddTo(mainReservoirDisposable);

			mainReservoir.MinimumPressureUnit
				.BindTo(
					comboBoxMainReservoirMinimumPressureUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Pressure)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxMainReservoirMinimumPressureUnit.SelectedIndexChanged += h,
							h => comboBoxMainReservoirMinimumPressureUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(mainReservoirDisposable);

			mainReservoir.MaximumPressure
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
				.AddTo(mainReservoirDisposable);

			mainReservoir.MaximumPressure
				.BindToErrorProvider(errorProvider, textBoxMainReservoirMaximumPressure)
				.AddTo(mainReservoirDisposable);

			mainReservoir.MaximumPressureUnit
				.BindTo(
					comboBoxMainReservoirMaximumPressureUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Pressure)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxMainReservoirMaximumPressureUnit.SelectedIndexChanged += h,
							h => comboBoxMainReservoirMaximumPressureUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(mainReservoirDisposable);

			return mainReservoirDisposable;
		}
	}
}
