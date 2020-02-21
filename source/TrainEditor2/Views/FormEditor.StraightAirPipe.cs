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

			return straightAirPipeDisposable;
		}
	}
}
