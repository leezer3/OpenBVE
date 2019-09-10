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
		private IDisposable BindToCar(CarViewModel y)
		{
			CompositeDisposable carDisposable = new CompositeDisposable();

			CompositeDisposable performanceDisposable = new CompositeDisposable().AddTo(carDisposable);
			CompositeDisposable moveDisposable = new CompositeDisposable().AddTo(carDisposable);
			CompositeDisposable brakeDisposable = new CompositeDisposable().AddTo(carDisposable);
			CompositeDisposable pressureDisposable = new CompositeDisposable().AddTo(carDisposable);
			CompositeDisposable accelerationDisposable = new CompositeDisposable().AddTo(carDisposable);
			CompositeDisposable motorDisposable = new CompositeDisposable().AddTo(carDisposable);

			y.Mass
				.BindTo(
					textBoxMass,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxMass.TextChanged += h,
							h => textBoxMass.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			y.Mass
				.BindToErrorProvider(errorProvider, textBoxMass)
				.AddTo(carDisposable);

			y.Length
				.BindTo(
					textBoxLength,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxLength.TextChanged += h,
							h => textBoxLength.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			y.Length
				.BindToErrorProvider(errorProvider, textBoxLength)
				.AddTo(carDisposable);

			y.Width
				.BindTo(
					textBoxWidth,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxLength.TextChanged += h,
							h => textBoxLength.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			y.Width
				.BindToErrorProvider(errorProvider, textBoxLength)
				.AddTo(carDisposable);

			y.Height
				.BindTo(
					textBoxHeight,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxHeight.TextChanged += h,
							h => textBoxHeight.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			y.Height
				.BindToErrorProvider(errorProvider, textBoxHeight)
				.AddTo(carDisposable);

			y.CenterOfGravityHeight
				.BindTo(
					textBoxCenterOfMassHeight,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxCenterOfMassHeight.TextChanged += h,
							h => textBoxCenterOfMassHeight.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			y.CenterOfGravityHeight
				.BindToErrorProvider(errorProvider, textBoxCenterOfMassHeight)
				.AddTo(carDisposable);

			y.DefinedAxles
				.BindTo(
					checkBoxDefinedAxles,
					z => z.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxDefinedAxles.CheckedChanged += h,
							h => checkBoxDefinedAxles.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			y.DefinedAxles
				.BindTo(
					groupBoxAxles,
					z => z.Enabled
				)
				.AddTo(carDisposable);

			y.FrontAxle
				.BindTo(
					textBoxFrontAxle,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxFrontAxle.TextChanged += h,
							h => textBoxFrontAxle.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			y.FrontAxle
				.BindToErrorProvider(errorProvider, textBoxFrontAxle)
				.AddTo(carDisposable);

			y.RearAxle
				.BindTo(
					textBoxRearAxle,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxRearAxle.TextChanged += h,
							h => textBoxRearAxle.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			y.RearAxle
				.BindToErrorProvider(errorProvider, textBoxRearAxle)
				.AddTo(carDisposable);

			y.ExposedFrontalArea
				.BindTo(
					textBoxExposedFrontalArea,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxExposedFrontalArea.TextChanged += h,
							h => textBoxExposedFrontalArea.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			y.ExposedFrontalArea
				.BindToErrorProvider(errorProvider, textBoxExposedFrontalArea)
				.AddTo(carDisposable);

			y.UnexposedFrontalArea
				.BindTo(
					textBoxUnexposedFrontalArea,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxUnexposedFrontalArea.TextChanged += h,
							h => textBoxUnexposedFrontalArea.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			y.UnexposedFrontalArea
				.BindToErrorProvider(errorProvider, textBoxUnexposedFrontalArea)
				.AddTo(carDisposable);

			y.Performance
				.Subscribe(z =>
				{
					performanceDisposable.Dispose();
					performanceDisposable = new CompositeDisposable().AddTo(carDisposable);

					BindToPerformance(z).AddTo(performanceDisposable);
				})
				.AddTo(carDisposable);

			y.Move
				.Subscribe(z =>
				{
					moveDisposable.Dispose();
					moveDisposable = new CompositeDisposable().AddTo(carDisposable);

					BindToMove(z).AddTo(moveDisposable);
				})
				.AddTo(carDisposable);

			y.Brake
				.Subscribe(z =>
				{
					brakeDisposable.Dispose();
					brakeDisposable = new CompositeDisposable().AddTo(carDisposable);

					BindToBrake(z).AddTo(brakeDisposable);
				})
				.AddTo(carDisposable);

			y.Pressure
				.Subscribe(z =>
				{
					pressureDisposable.Dispose();
					pressureDisposable = new CompositeDisposable().AddTo(carDisposable);

					BindToPressure(z).AddTo(pressureDisposable);
				})
				.AddTo(carDisposable);

			y.Reversed
				.BindTo(
					checkBoxReversed,
					z => z.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxReversed.CheckedChanged += h,
							h => checkBoxReversed.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			y.Object
				.BindTo(
					textBoxObject,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxObject.TextChanged += h,
							h => textBoxObject.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			y.LoadingSway
				.BindTo(
					checkBoxLoadingSway,
					z => z.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxLoadingSway.CheckedChanged += h,
							h => checkBoxLoadingSway.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			MotorCarViewModel motorCar = y as MotorCarViewModel;

			motorCar?.Acceleration
				.Subscribe(z =>
				{
					accelerationDisposable.Dispose();
					accelerationDisposable = new CompositeDisposable().AddTo(carDisposable);

					BindToAcceleration(z).AddTo(accelerationDisposable);
				})
				.AddTo(carDisposable);

			motorCar?.Motor
				.Subscribe(z =>
				{
					motorDisposable.Dispose();
					motorDisposable = new CompositeDisposable().AddTo(carDisposable);

					BindToMotor(z).AddTo(motorDisposable);
				})
				.AddTo(carDisposable);

			return carDisposable;
		}
	}
}
