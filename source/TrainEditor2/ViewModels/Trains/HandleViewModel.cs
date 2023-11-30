using System;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;
using TrainManager.Handles;

namespace TrainEditor2.ViewModels.Trains
{
	internal class HandleViewModel : BaseViewModel
	{
		internal ReactiveProperty<HandleType> HandleType
		{
			get;
		}

		internal ReactiveProperty<int> PowerNotches
		{
			get;
		}

		internal ReactiveProperty<int> BrakeNotches
		{
			get;
		}

		internal ReactiveProperty<int> PowerNotchReduceSteps
		{
			get;
		}

		internal ReactiveProperty<EbHandleBehaviour> HandleBehaviour
		{
			get;
		}

		internal ReactiveProperty<LocoBrakeType> LocoBrake
		{
			get;
		}

		internal ReactiveProperty<int> LocoBrakeNotches
		{
			get;
		}

		internal ReactiveProperty<int> DriverPowerNotches
		{
			get;
		}

		internal ReactiveProperty<int> DriverBrakeNotches
		{
			get;
		}

		internal HandleViewModel(Handle handle, Train train)
		{
			HandleType = handle
				.ToReactivePropertyAsSynchronized(x => x.HandleType)
				.AddTo(disposable);

			PowerNotches = handle
				.ToReactivePropertyAsSynchronized(x => x.PowerNotches, ignoreValidationErrorValue: true)
				.AddTo(disposable);

			PowerNotches.Subscribe(_ => train.ApplyPowerNotchesToCar()).AddTo(disposable);

			BrakeNotches = handle
				.ToReactivePropertyAsSynchronized(x => x.BrakeNotches, ignoreValidationErrorValue: true)
				.SetValidateNotifyError(x =>
				{
					if (x == 0 && train.Device.HoldBrake)
					{
						return Utilities.GetInterfaceString("general_settings", "handle", "brake_notches_error_message");
					}

					return null;
				})
				.AddTo(disposable);

			BrakeNotches.Subscribe(_ => train.ApplyBrakeNotchesToCar()).AddTo(disposable);

			PowerNotchReduceSteps = handle
				.ToReactivePropertyAsSynchronized(x => x.PowerNotchReduceSteps)
				.AddTo(disposable);

			HandleBehaviour = handle
				.ToReactivePropertyAsSynchronized(x => x.HandleBehaviour)
				.AddTo(disposable);

			LocoBrake = handle
				.ToReactivePropertyAsSynchronized(x => x.LocoBrake)
				.AddTo(disposable);

			LocoBrakeNotches = handle
				.ToReactivePropertyAsSynchronized(x => x.LocoBrakeNotches)
				.AddTo(disposable);

			LocoBrakeNotches.Subscribe(_ => train.ApplyLocoBrakeNotchesToCar()).AddTo(disposable);

			DriverPowerNotches = handle
				.ToReactivePropertyAsSynchronized(x => x.DriverPowerNotches, ignoreValidationErrorValue: true)
				.AddTo(disposable);

			DriverBrakeNotches = handle
				.ToReactivePropertyAsSynchronized(x => x.DriverBrakeNotches, ignoreValidationErrorValue: true)
				.AddTo(disposable);

			PowerNotches
				.SetValidateNotifyError(x =>
				{
					if (x < DriverPowerNotches.Value)
					{
						return Utilities.GetInterfaceString("general_settings", "handle", "driver_power_notches_error_message");
					}

					return null;
				})
				.Subscribe(_ => DriverPowerNotches.ForceValidate())
				.AddTo(disposable);

			PowerNotches
				.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => PowerNotches.ForceNotify())
				.AddTo(disposable);

			BrakeNotches
				.SetValidateNotifyError(x =>
				{
					if (x < DriverBrakeNotches.Value)
					{
						return Utilities.GetInterfaceString("general_settings", "handle", "driver_brake_notches_error_message");
					}

					return null;
				})
				.Subscribe(_ => DriverBrakeNotches.ForceValidate())
				.AddTo(disposable);

			BrakeNotches
				.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => BrakeNotches.ForceNotify())
				.AddTo(disposable);

			DriverPowerNotches
				.SetValidateNotifyError(x =>
				{
					if (x > PowerNotches.Value)
					{
						return Utilities.GetInterfaceString("general_settings", "handle", "driver_power_notches_error_message");
					}

					return null;
				})
				.Subscribe(_ => PowerNotches.ForceValidate())
				.AddTo(disposable);

			DriverPowerNotches
				.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => DriverPowerNotches.ForceNotify())
				.AddTo(disposable);

			DriverBrakeNotches
				.SetValidateNotifyError(x =>
				{
					if (x > BrakeNotches.Value)
					{
						return Utilities.GetInterfaceString("general_settings", "handle", "driver_brake_notches_error_message");
					}

					return null;
				})
				.Subscribe(_ => BrakeNotches.ForceValidate())
				.AddTo(disposable);

			DriverBrakeNotches
				.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => DriverBrakeNotches.ForceNotify())
				.AddTo(disposable);
		}
	}
}
