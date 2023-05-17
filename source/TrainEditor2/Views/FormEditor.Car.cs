using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using OpenBveApi.World;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;
using TrainEditor2.ViewModels.Trains;
using TrainManager.Car;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToCar(CarViewModel car)
		{
			CompositeDisposable carDisposable = new CompositeDisposable();

			CompositeDisposable performanceDisposable = new CompositeDisposable().AddTo(carDisposable);
			CompositeDisposable brakeDisposable = new CompositeDisposable().AddTo(carDisposable);
			CompositeDisposable pressureDisposable = new CompositeDisposable().AddTo(carDisposable);
			CompositeDisposable accelerationDisposable = new CompositeDisposable().AddTo(carDisposable);
			CompositeDisposable motorDisposable = new CompositeDisposable().AddTo(carDisposable);
			CompositeDisposable cabDisposable = new CompositeDisposable().AddTo(carDisposable);

			car.Mass
				.BindTo(
					textBoxMass,
					x => x.Text,
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

			car.Mass
				.BindToErrorProvider(errorProvider, textBoxMass)
				.AddTo(carDisposable);

			car.MassUnit
				.BindTo(
					comboBoxMassUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (UnitOfWeight)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxMassUnit.SelectedIndexChanged += h,
							h => comboBoxMassUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			car.Length
				.BindTo(
					textBoxLength,
					x => x.Text,
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

			car.Length
				.BindToErrorProvider(errorProvider, textBoxLength)
				.AddTo(carDisposable);

			car.LengthUnit
				.BindTo(
					comboBoxLengthUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (UnitOfLength)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxLengthUnit.SelectedIndexChanged += h,
							h => comboBoxLengthUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			car.Width
				.BindTo(
					textBoxWidth,
					x => x.Text,
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

			car.Width
				.BindToErrorProvider(errorProvider, textBoxLength)
				.AddTo(carDisposable);

			car.WidthUnit
				.BindTo(
					comboBoxWidthUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (UnitOfLength)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxWidthUnit.SelectedIndexChanged += h,
							h => comboBoxWidthUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			car.Height
				.BindTo(
					textBoxHeight,
					x => x.Text,
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

			car.Height
				.BindToErrorProvider(errorProvider, textBoxHeight)
				.AddTo(carDisposable);

			car.HeightUnit
				.BindTo(
					comboBoxHeightUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (UnitOfLength)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxHeightUnit.SelectedIndexChanged += h,
							h => comboBoxHeightUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			car.CenterOfGravityHeight
				.BindTo(
					textBoxCenterOfMassHeight,
					x => x.Text,
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

			car.CenterOfGravityHeight
				.BindToErrorProvider(errorProvider, textBoxCenterOfMassHeight)
				.AddTo(carDisposable);

			car.CenterOfGravityHeightUnit
				.BindTo(
					comboBoxCenterOfMassHeightUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (UnitOfLength)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxCenterOfMassHeightUnit.SelectedIndexChanged += h,
							h => comboBoxCenterOfMassHeightUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			car.DefinedAxles
				.BindTo(
					checkBoxDefinedAxles,
					x => x.Checked,
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

			car.DefinedAxles
				.BindTo(
					groupBoxAxles,
					x => x.Enabled
				)
				.AddTo(carDisposable);

			car.FrontAxle
				.BindTo(
					textBoxFrontAxle,
					x => x.Text,
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

			car.FrontAxle
				.BindToErrorProvider(errorProvider, textBoxFrontAxle)
				.AddTo(carDisposable);

			car.FrontAxleUnit
				.BindTo(
					comboBoxFrontAxleUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (UnitOfLength)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxFrontAxleUnit.SelectedIndexChanged += h,
							h => comboBoxFrontAxleUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			car.RearAxle
				.BindTo(
					textBoxRearAxle,
					x => x.Text,
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

			car.RearAxle
				.BindToErrorProvider(errorProvider, textBoxRearAxle)
				.AddTo(carDisposable);

			car.RearAxleUnit
				.BindTo(
					comboBoxRearAxleUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (UnitOfLength)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxRearAxleUnit.SelectedIndexChanged += h,
							h => comboBoxRearAxleUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			car.ExposedFrontalArea
				.BindTo(
					textBoxExposedFrontalArea,
					x => x.Text,
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

			car.ExposedFrontalArea
				.BindToErrorProvider(errorProvider, textBoxExposedFrontalArea)
				.AddTo(carDisposable);

			car.ExposedFrontalAreaUnit
				.BindTo(
					comboBoxExposedFrontalAreaUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (UnitOfArea)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxExposedFrontalAreaUnit.SelectedIndexChanged += h,
							h => comboBoxExposedFrontalAreaUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			car.UnexposedFrontalArea
				.BindTo(
					textBoxUnexposedFrontalArea,
					x => x.Text,
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

			car.UnexposedFrontalArea
				.BindToErrorProvider(errorProvider, textBoxUnexposedFrontalArea)
				.AddTo(carDisposable);

			car.UnexposedFrontalAreaUnit
				.BindTo(
					comboBoxUnexposedFrontalAreaUnit,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (UnitOfArea)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxUnexposedFrontalAreaUnit.SelectedIndexChanged += h,
							h => comboBoxUnexposedFrontalAreaUnit.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			car.Performance
				.Subscribe(x =>
				{
					performanceDisposable.Dispose();
					performanceDisposable = new CompositeDisposable().AddTo(carDisposable);

					BindToPerformance(x).AddTo(performanceDisposable);
				})
				.AddTo(carDisposable);

			car.Brake
				.Subscribe(x =>
				{
					brakeDisposable.Dispose();
					brakeDisposable = new CompositeDisposable().AddTo(carDisposable);

					BindToBrake(x).AddTo(brakeDisposable);
				})
				.AddTo(carDisposable);

			car.Pressure
				.Subscribe(x =>
				{
					pressureDisposable.Dispose();
					pressureDisposable = new CompositeDisposable().AddTo(carDisposable);

					BindToPressure(x).AddTo(pressureDisposable);
				})
				.AddTo(carDisposable);

			car.Reversed
				.BindTo(
					checkBoxReversed,
					x => x.Checked,
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

			car.Object
				.BindTo(
					textBoxObject,
					x => x.Text,
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

			car.LoadingSway
				.BindTo(
					checkBoxLoadingSway,
					x => x.Checked,
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

			car.ReAdhesionDevice
				.BindTo(
					comboBoxReAdhesionDevice,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (ReadhesionDeviceType)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxReAdhesionDevice.SelectedIndexChanged += h,
							h => comboBoxReAdhesionDevice.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(carDisposable);

			MotorCarViewModel motorCar = car as MotorCarViewModel;

			motorCar?.Acceleration
				.Subscribe(x =>
				{
					accelerationDisposable.Dispose();
					accelerationDisposable = new CompositeDisposable().AddTo(carDisposable);

					BindToAcceleration(x).AddTo(accelerationDisposable);
				})
				.AddTo(carDisposable);

			motorCar?.Motor
				.Subscribe(x =>
				{
					motorDisposable.Dispose();
					motorDisposable = new CompositeDisposable().AddTo(carDisposable);

					BindToMotor(x).AddTo(motorDisposable);
				})
				.AddTo(carDisposable);

			ControlledMotorCarViewModel controlledMotorCar = car as ControlledMotorCarViewModel;
			ControlledTrailerCarViewModel controlledTrailerCar = car as ControlledTrailerCarViewModel;

			controlledMotorCar?.Cab
				.BindTo(
					checkBoxIsEmbeddedCab,
					x => x.Checked,
					x => x is EmbeddedCabViewModel
				)
				.AddTo(carDisposable);

			controlledMotorCar?.Cab
				.BindTo(
					groupBoxExternalCab,
					x => x.Enabled,
					x => x is ExternalCabViewModel
				)
				.AddTo(carDisposable);

			controlledMotorCar?.Cab
				.Subscribe(x =>
				{
					cabDisposable.Dispose();
					cabDisposable = new CompositeDisposable().AddTo(carDisposable);

					BindToCab(x).AddTo(cabDisposable);
				})
				.AddTo(carDisposable);

			controlledTrailerCar?.Cab
				.BindTo(
					checkBoxIsEmbeddedCab,
					x => x.Checked,
					x => x is EmbeddedCabViewModel
				)
				.AddTo(carDisposable);

			controlledTrailerCar?.Cab
				.BindTo(
					groupBoxExternalCab,
					x => x.Enabled,
					x => x is ExternalCabViewModel
				)
				.AddTo(carDisposable);

			controlledTrailerCar?.Cab
				.Subscribe(x =>
				{
					cabDisposable.Dispose();
					cabDisposable = new CompositeDisposable().AddTo(carDisposable);

					BindToCab(x).AddTo(cabDisposable);
				})
				.AddTo(carDisposable);

			return carDisposable;
		}
	}
}
