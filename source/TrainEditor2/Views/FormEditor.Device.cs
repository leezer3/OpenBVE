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
		private IDisposable BindToDevice(DeviceViewModel device)
		{
			CompositeDisposable deviceDisposable = new CompositeDisposable();

			device.Ats
				.BindTo(
					comboBoxAts,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Device.AtsModes)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxAts.SelectedIndexChanged += h,
							h => comboBoxAts.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			device.Atc
				.BindTo(
					comboBoxAtc,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Device.AtcModes)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxAtc.SelectedIndexChanged += h,
							h => comboBoxAtc.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			device.Eb
				.BindTo(
					checkBoxEb,
					x => x.Checked,
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

			device.ConstSpeed
				.BindTo(
					checkBoxConstSpeed,
					x => x.Checked,
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

			device.HoldBrake
				.BindTo(
					checkBoxHoldBrake,
					x => x.Checked,
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

			device.HoldBrake
				.BindToErrorProvider(errorProvider, checkBoxHoldBrake)
				.AddTo(deviceDisposable);

			device.PassAlarm
				.BindTo(
					comboBoxPassAlarm,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Device.PassAlarmModes)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxPassAlarm.SelectedIndexChanged += h,
							h => comboBoxPassAlarm.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			device.DoorOpenMode
				.BindTo(
					comboBoxDoorOpenMode,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Device.DoorModes)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxDoorOpenMode.SelectedIndexChanged += h,
							h => comboBoxDoorOpenMode.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			device.DoorCloseMode
				.BindTo(
					comboBoxDoorCloseMode,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (Device.DoorModes)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxDoorCloseMode.SelectedIndexChanged += h,
							h => comboBoxDoorCloseMode.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(deviceDisposable);

			return deviceDisposable;
		}
	}
}
