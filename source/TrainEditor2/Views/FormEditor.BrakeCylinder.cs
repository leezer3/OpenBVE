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

			return brakeCylinderDisposable;
		}
	}
}
