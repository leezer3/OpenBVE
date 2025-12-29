using System.Globalization;
using Formats.OpenBve;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;
using TrainManager.Car;
using TrainManager.SafetySystems;

namespace TrainEditor2.ViewModels.Trains
{
	internal class DeviceViewModel : BaseViewModel
	{
		internal ReactiveProperty<AtsModes> Ats
		{
			get;
		}

		internal ReactiveProperty<AtcModes> Atc
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

		internal ReactiveProperty<ReadhesionDeviceType> ReAdhesionDevice
		{
			get;
		}

		internal ReactiveProperty<PassAlarmType> PassAlarm
		{
			get;
		}

		internal ReactiveProperty<DoorMode> DoorOpenMode
		{
			get;
		}

		internal ReactiveProperty<DoorMode> DoorCloseMode
		{
			get;
		}

		internal ReactiveProperty<string> DoorWidth
		{
			get;
		}

		internal ReactiveProperty<string> DoorMaxTolerance
		{
			get;
		}

		internal DeviceViewModel(Device device, Handle handle)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

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
						return Utilities.GetInterfaceString("general_settings", "handle", "brake_notches_error_message");
					}

					return null;
				})
				.AddTo(disposable);

			ReAdhesionDevice = device
				.ToReactivePropertyAsSynchronized(x => x.ReAdhesionDevice)
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

			DoorWidth = device
				.ToReactivePropertyAsSynchronized(
					x => x.DoorWidth,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.NonNegative, out string message);
					return message;
				})
				.AddTo(disposable);

			DoorMaxTolerance = device
				.ToReactivePropertyAsSynchronized(
					x => x.DoorMaxTolerance,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.NonNegative, out string message);
					return message;
				})
				.AddTo(disposable);
		}
	}
}
