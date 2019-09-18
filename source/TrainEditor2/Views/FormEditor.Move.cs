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
		private IDisposable BindToMove(MoveViewModel z)
		{
			CompositeDisposable moveDisposable = new CompositeDisposable();

			z.JerkPowerUp
				.BindTo(
					textBoxJerkPowerUp,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxJerkPowerUp.TextChanged += h,
							h => textBoxJerkPowerUp.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(moveDisposable);

			z.JerkPowerUp
				.BindToErrorProvider(errorProvider, textBoxJerkPowerUp)
				.AddTo(moveDisposable);

			z.JerkPowerDown
				.BindTo(
					textBoxJerkPowerDown,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxJerkPowerDown.TextChanged += h,
							h => textBoxJerkPowerDown.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(moveDisposable);

			z.JerkPowerDown
				.BindToErrorProvider(errorProvider, textBoxJerkPowerDown)
				.AddTo(moveDisposable);

			z.JerkBrakeUp
				.BindTo(
					textBoxJerkBrakeUp,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxJerkBrakeUp.TextChanged += h,
							h => textBoxJerkBrakeUp.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(moveDisposable);

			z.JerkBrakeUp
				.BindToErrorProvider(errorProvider, textBoxJerkBrakeUp)
				.AddTo(moveDisposable);

			z.JerkBrakeDown
				.BindTo(
					textBoxJerkBrakeDown,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxJerkBrakeDown.TextChanged += h,
							h => textBoxJerkBrakeDown.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(moveDisposable);

			z.JerkBrakeDown
				.BindToErrorProvider(errorProvider, textBoxJerkBrakeDown)
				.AddTo(moveDisposable);

			z.BrakeCylinderUp
				.BindTo(
					textBoxBrakeCylinderUp,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxBrakeCylinderUp.TextChanged += h,
							h => textBoxBrakeCylinderUp.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(moveDisposable);

			z.BrakeCylinderUp
				.BindToErrorProvider(errorProvider, textBoxBrakeCylinderUp)
				.AddTo(moveDisposable);

			z.BrakeCylinderDown
				.BindTo(
					textBoxBrakeCylinderDown,
					w => w.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxBrakeCylinderDown.TextChanged += h,
							h => textBoxBrakeCylinderDown.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(moveDisposable);

			z.BrakeCylinderDown
				.BindToErrorProvider(errorProvider, textBoxBrakeCylinderDown)
				.AddTo(moveDisposable);

			return moveDisposable;
		}
	}
}
