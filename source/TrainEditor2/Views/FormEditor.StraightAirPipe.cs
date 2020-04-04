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
		private IDisposable BindToStraightAirPipe(StraightAirPipeViewModel straightAirPipe)
		{
			CompositeDisposable straightAirPipeDisposable = new CompositeDisposable();

			straightAirPipe.ServiceRate
				.BindTo(
					textBoxStraightAirPipeServiceRate,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxStraightAirPipeServiceRate.TextChanged += h,
							h => textBoxStraightAirPipeServiceRate.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(straightAirPipeDisposable);

			straightAirPipe.ServiceRate
				.BindToErrorProvider(errorProvider, textBoxStraightAirPipeServiceRate)
				.AddTo(straightAirPipeDisposable);

			straightAirPipe.ServiceRateUnit
				.BindTo(
					comboBoxStraightAirPipeServiceRateUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.PressureRate)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxStraightAirPipeServiceRateUnit.SelectedIndexChanged += h,
							h => comboBoxStraightAirPipeServiceRateUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(straightAirPipeDisposable);

			straightAirPipe.EmergencyRate
				.BindTo(
					textBoxStraightAirPipeEmergencyRate,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxStraightAirPipeEmergencyRate.TextChanged += h,
							h => textBoxStraightAirPipeEmergencyRate.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(straightAirPipeDisposable);

			straightAirPipe.EmergencyRate
				.BindToErrorProvider(errorProvider, textBoxStraightAirPipeEmergencyRate)
				.AddTo(straightAirPipeDisposable);

			straightAirPipe.EmergencyRateUnit
				.BindTo(
					comboBoxStraightAirPipeEmergencyRateUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.PressureRate)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxStraightAirPipeEmergencyRateUnit.SelectedIndexChanged += h,
							h => comboBoxStraightAirPipeEmergencyRateUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(straightAirPipeDisposable);

			straightAirPipe.ReleaseRate
				.BindTo(
					textBoxStraightAirPipeReleaseRate,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxStraightAirPipeReleaseRate.TextChanged += h,
							h => textBoxStraightAirPipeReleaseRate.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(straightAirPipeDisposable);

			straightAirPipe.ReleaseRate
				.BindToErrorProvider(errorProvider, textBoxStraightAirPipeReleaseRate)
				.AddTo(straightAirPipeDisposable);

			straightAirPipe.ReleaseRateUnit
				.BindTo(
					comboBoxStraightAirPipeReleaseRateUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Unit.PressureRate)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxStraightAirPipeReleaseRateUnit.SelectedIndexChanged += h,
							h => comboBoxStraightAirPipeReleaseRateUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(straightAirPipeDisposable);

			return straightAirPipeDisposable;
		}
	}
}
