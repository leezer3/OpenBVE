using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToDevice(DeviceViewModel y)
		{
			CompositeDisposable deviceDisposable = new CompositeDisposable();

			y.Ats
				.BindTo(
					comboBoxAts,
					z => z.SelectedIndex,
					BindingMode.TwoWay,
					z => (int)z,
					z => (Device.AtsModes)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxAts.SelectedIndexChanged += h,
							h => comboBoxAts.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			y.Atc
				.BindTo(
					comboBoxAtc,
					z => z.SelectedIndex,
					BindingMode.TwoWay,
					z => (int)z,
					z => (Device.AtcModes)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxAtc.SelectedIndexChanged += h,
							h => comboBoxAtc.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			y.Eb
				.BindTo(
					checkBoxEb,
					z => z.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxEb.CheckedChanged += h,
							h => checkBoxEb.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			y.ConstSpeed
				.BindTo(
					checkBoxConstSpeed,
					z => z.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxConstSpeed.CheckedChanged += h,
							h => checkBoxConstSpeed.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			y.HoldBrake
				.BindTo(
					checkBoxHoldBrake,
					z => z.Checked,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => checkBoxHoldBrake.CheckedChanged += h,
							h => checkBoxHoldBrake.CheckedChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			y.HoldBrake
				.BindToErrorProvider(errorProvider, checkBoxHoldBrake)
				.AddTo(deviceDisposable);

			y.ReAdhesionDevice
				.BindTo(
					comboBoxReAdhesionDevice,
					z => z.SelectedIndex,
					BindingMode.TwoWay,
					z => (int)z,
					z => (Device.ReAdhesionDevices)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxReAdhesionDevice.SelectedIndexChanged += h,
							h => comboBoxReAdhesionDevice.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			y.PassAlarm
				.BindTo(
					comboBoxPassAlarm,
					z => z.SelectedIndex,
					BindingMode.TwoWay,
					z => (int)z,
					z => (Device.PassAlarmModes)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxPassAlarm.SelectedIndexChanged += h,
							h => comboBoxPassAlarm.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			y.DoorOpenMode
				.BindTo(
					comboBoxDoorOpenMode,
					z => z.SelectedIndex,
					BindingMode.TwoWay,
					z => (int)z,
					z => (Device.DoorModes)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxDoorOpenMode.SelectedIndexChanged += h,
							h => comboBoxDoorOpenMode.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			y.DoorCloseMode
				.BindTo(
					comboBoxDoorCloseMode,
					z => z.SelectedIndex,
					BindingMode.TwoWay,
					z => (int)z,
					z => (Device.DoorModes)z,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxDoorCloseMode.SelectedIndexChanged += h,
							h => comboBoxDoorCloseMode.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			y.DoorWidth
				.BindTo(
					textBoxDoorWidth,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDoorWidth.TextChanged += h,
							h => textBoxDoorWidth.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			y.DoorWidth
				.BindToErrorProvider(errorProvider, textBoxDoorWidth)
				.AddTo(deviceDisposable);

			y.DoorMaxTolerance
				.BindTo(
					textBoxDoorMaxTolerance,
					z => z.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxDoorMaxTolerance.TextChanged += h,
							h => textBoxDoorMaxTolerance.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			y.DoorMaxTolerance
				.BindToErrorProvider(errorProvider, textBoxDoorMaxTolerance)
				.AddTo(deviceDisposable);

			return deviceDisposable;
		}
	}
}
