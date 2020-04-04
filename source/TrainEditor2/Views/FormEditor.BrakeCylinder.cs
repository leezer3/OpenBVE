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
		private IDisposable BindToBrakeCylinder(BrakeCylinderViewModel brakeCylinder)
		{
			CompositeDisposable brakeCylinderDisposable = new CompositeDisposable();

			brakeCylinder.ServiceMaximumPressure
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
				.AddTo(brakeCylinderDisposable);

			brakeCylinder.ServiceMaximumPressure
				.BindToErrorProvider(errorProvider, textBoxBrakeCylinderServiceMaximumPressure)
				.AddTo(brakeCylinderDisposable);

			brakeCylinder.ServiceMaximumPressureUnit
				.BindTo(
					comboBoxBrakeCylinderServiceMaximumPressureUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Pressure)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxBrakeCylinderServiceMaximumPressureUnit.SelectedIndexChanged += h,
							h => comboBoxBrakeCylinderServiceMaximumPressureUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakeCylinderDisposable);

			brakeCylinder.EmergencyMaximumPressure
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
				.AddTo(brakeCylinderDisposable);

			brakeCylinder.EmergencyMaximumPressure
				.BindToErrorProvider(errorProvider, textBoxBrakeCylinderEmergencyMaximumPressure)
				.AddTo(brakeCylinderDisposable);

			brakeCylinder.EmergencyMaximumPressureUnit
				.BindTo(
					comboBoxBrakeCylinderEmergencyMaximumPressureUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.Pressure)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxBrakeCylinderEmergencyMaximumPressureUnit.SelectedIndexChanged += h,
							h => comboBoxBrakeCylinderEmergencyMaximumPressureUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakeCylinderDisposable);

			brakeCylinder.EmergencyRate
				.BindTo(
					textBoxBrakeCylinderEmergencyRate,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxBrakeCylinderEmergencyRate.TextChanged += h,
							h => textBoxBrakeCylinderEmergencyRate.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakeCylinderDisposable);

			brakeCylinder.EmergencyRate
				.BindToErrorProvider(errorProvider, textBoxBrakeCylinderEmergencyRate)
				.AddTo(brakeCylinderDisposable);

			brakeCylinder.EmergencyRateUnit
				.BindTo(
					comboBoxBrakeCylinderEmergencyRateUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.PressureRate)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxBrakeCylinderEmergencyRateUnit.SelectedIndexChanged += h,
							h => comboBoxBrakeCylinderEmergencyRateUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakeCylinderDisposable);

			brakeCylinder.ReleaseRate
				.BindTo(
					textBoxBrakeCylinderReleaseRate,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxBrakeCylinderReleaseRate.TextChanged += h,
							h => textBoxBrakeCylinderReleaseRate.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakeCylinderDisposable);

			brakeCylinder.ReleaseRate
				.BindToErrorProvider(errorProvider, textBoxBrakeCylinderReleaseRate)
				.AddTo(brakeCylinderDisposable);

			brakeCylinder.ReleaseRateUnit
				.BindTo(
					comboBoxBrakeCylinderReleaseRateUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.PressureRate)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxBrakeCylinderReleaseRateUnit.SelectedIndexChanged += h,
							h => comboBoxBrakeCylinderReleaseRateUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(brakeCylinderDisposable);

			return brakeCylinderDisposable;
		}
	}
}
