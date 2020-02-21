﻿using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class DeviceViewModel : BaseViewModel
	{
		internal ReactiveProperty<Device.AtsModes> Ats
		{
			get;
		}

		internal ReactiveProperty<Device.AtcModes> Atc
		{
			get;
		}

		internal ReactiveProperty<bool> Eb
		{
			get;
		}

		internal ReactiveProperty<bool> ConstSpeed
		{
			get;
		}

		internal ReactiveProperty<bool> HoldBrake
		{
			get;
		}

		internal ReactiveProperty<Device.PassAlarmModes> PassAlarm
		{
			get;
		}

		internal ReactiveProperty<Device.DoorModes> DoorOpenMode
		{
			get;
		}

		internal ReactiveProperty<Device.DoorModes> DoorCloseMode
		{
			get;
		}

		internal DeviceViewModel(Device device, Handle handle)
		{
			Ats = device
				.ToReactivePropertyAsSynchronized(x => x.Ats)
				.AddTo(disposable);

			Atc = device
				.ToReactivePropertyAsSynchronized(x => x.Atc)
				.AddTo(disposable);

			Eb = device
				.ToReactivePropertyAsSynchronized(x => x.Eb)
				.AddTo(disposable);

			ConstSpeed = device
				.ToReactivePropertyAsSynchronized(x => x.ConstSpeed)
				.AddTo(disposable);

			HoldBrake = device
				.ToReactivePropertyAsSynchronized(x => x.HoldBrake, ignoreValidationErrorValue: true)
				.SetValidateNotifyError(x =>
				{
					if (x && handle.BrakeNotches == 0)
					{
						return "BrakeNotches must be at least 1 if HoldBrake is set.";
					}

					return null;
				})
				.AddTo(disposable);

			PassAlarm = device
				.ToReactivePropertyAsSynchronized(x => x.PassAlarm)
				.AddTo(disposable);

			DoorOpenMode = device
				.ToReactivePropertyAsSynchronized(x => x.DoorOpenMode)
				.AddTo(disposable);

			DoorCloseMode = device
				.ToReactivePropertyAsSynchronized(x => x.DoorCloseMode)
				.AddTo(disposable);
		}
	}
}
