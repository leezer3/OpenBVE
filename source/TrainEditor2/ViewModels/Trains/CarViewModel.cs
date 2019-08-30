using System;
using System.Globalization;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal abstract class CarViewModel : BaseViewModel
	{
		internal class BogieViewModel : BaseViewModel
		{
			internal ReactiveProperty<bool> DefinedAxles
			{
				get;
			}

			internal ReactiveProperty<string> FrontAxle
			{
				get;
			}

			internal ReactiveProperty<string> RearAxle
			{
				get;
			}

			internal ReactiveProperty<bool> Reversed
			{
				get;
			}

			internal ReactiveProperty<string> Object
			{
				get;
			}

			internal BogieViewModel(Car.Bogie bogie)
			{
				CultureInfo culture = CultureInfo.InvariantCulture;

				DefinedAxles = bogie
					.ToReactivePropertyAsSynchronized(x => x.DefinedAxles)
					.AddTo(disposable);

				FrontAxle = bogie
					.ToReactivePropertyAsSynchronized(
						x => x.FrontAxle,
						x => x.ToString(culture),
						double.Parse,
						ignoreValidationErrorValue: true
					)
					.AddTo(disposable);

				RearAxle = bogie
					.ToReactivePropertyAsSynchronized(
						x => x.RearAxle,
						x => x.ToString(culture),
						double.Parse,
						ignoreValidationErrorValue: true
					)
					.AddTo(disposable);

				Reversed = bogie
					.ToReactivePropertyAsSynchronized(x => x.Reversed)
					.AddTo(disposable);

				Object = bogie
					.ToReactivePropertyAsSynchronized(x => x.Object)
					.AddTo(disposable);

				DefinedAxles.Subscribe(_ =>
					{
						FrontAxle.ForceValidate();
						RearAxle.ForceValidate();
					});

				FrontAxle.SetValidateNotifyError(x =>
					{
						double front;
						string message;

						if (Utilities.TryParse(x, NumberRange.Any, out front, out message))
						{
							double rear;

							if (DefinedAxles.Value && Utilities.TryParse(RearAxle.Value, NumberRange.Any, out rear) && front <= rear)
							{
								message = "RearAxleはFrontAxle未満でなければなりません。";
							}
						}

						return message;
					})
					.Subscribe(_ => RearAxle.ForceValidate())
					.AddTo(disposable);

				FrontAxle.ObserveHasErrors
					.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
					.Where(x => !x)
					.Subscribe(_ => FrontAxle.ForceNotify())
					.AddTo(disposable);

				RearAxle.SetValidateNotifyError(x =>
					{
						double rear;
						string message;

						if (Utilities.TryParse(x, NumberRange.Any, out rear, out message))
						{
							double front;

							if (DefinedAxles.Value && Utilities.TryParse(FrontAxle.Value, NumberRange.Any, out front) && rear >= front)
							{
								message = "RearAxleはFrontAxle未満でなければなりません。";
							}
						}

						return message;
					})
					.Subscribe(_ => FrontAxle.ForceValidate())
					.AddTo(disposable);

				RearAxle.ObserveHasErrors
					.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
					.Where(x => !x)
					.Subscribe(_ => RearAxle.ForceNotify())
					.AddTo(disposable);
			}
		}

		internal Car Model
		{
			get;
		}

		internal ReactiveProperty<string> Mass
		{
			get;
		}

		internal ReactiveProperty<string> Length
		{
			get;
		}

		internal ReactiveProperty<string> Width
		{
			get;
		}

		internal ReactiveProperty<string> Height
		{
			get;
		}

		internal ReactiveProperty<string> CenterOfGravityHeight
		{
			get;
		}

		internal ReactiveProperty<bool> DefinedAxles
		{
			get;
		}

		internal ReactiveProperty<string> FrontAxle
		{
			get;
		}

		internal ReactiveProperty<string> RearAxle
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<BogieViewModel> FrontBogie
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<BogieViewModel> RearBogie
		{
			get;
		}

		internal ReactiveProperty<string> ExposedFrontalArea
		{
			get;
		}

		internal ReactiveProperty<string> UnexposedFrontalArea
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<PerformanceViewModel> Performance
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<DelayViewModel> Delay
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<MoveViewModel> Move
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<BrakeViewModel> Brake
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<PressureViewModel> Pressure
		{
			get;
		}

		internal ReactiveProperty<bool> Reversed
		{
			get;
		}

		internal ReactiveProperty<string> Object
		{
			get;
		}

		internal ReactiveProperty<bool> LoadingSway
		{
			get;
		}

		internal CarViewModel(Car car)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			Model = car;

			Mass = car
				.ToReactivePropertyAsSynchronized(
					x => x.Mass,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Positive, out result, out message);

					return message;
				})
				.AddTo(disposable);

			Length = car
				.ToReactivePropertyAsSynchronized(
					x => x.Length,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Positive, out result, out message);

					return message;
				})
				.AddTo(disposable);

			Width = car
				.ToReactivePropertyAsSynchronized(
					x => x.Width,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Positive, out result, out message);

					return message;
				})
				.AddTo(disposable);

			Height = car
				.ToReactivePropertyAsSynchronized(
					x => x.Height,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Positive, out result, out message);

					return message;
				})
				.AddTo(disposable);

			CenterOfGravityHeight = car
				.ToReactivePropertyAsSynchronized(
					x => x.CenterOfGravityHeight,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Any, out result, out message);

					return message;
				})
				.AddTo(disposable);

			DefinedAxles = car
				.ToReactivePropertyAsSynchronized(x => x.DefinedAxles)
				.AddTo(disposable);

			FrontAxle = car
				.ToReactivePropertyAsSynchronized(
					x => x.FrontAxle,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			RearAxle = car
				.ToReactivePropertyAsSynchronized(
					x => x.RearAxle,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			FrontBogie = car
				.ObserveProperty(x => x.FrontBogie)
				.Do(_ => FrontBogie?.Value.Dispose())
				.Select(x => new BogieViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			RearBogie = car
				.ObserveProperty(x => x.RearBogie)
				.Do(_ => RearBogie?.Value.Dispose())
				.Select(x => new BogieViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			ExposedFrontalArea = car
				.ToReactivePropertyAsSynchronized(
					x => x.ExposedFrontalArea,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Positive, out result, out message);

					return message;
				})
				.AddTo(disposable);

			UnexposedFrontalArea = car
				.ToReactivePropertyAsSynchronized(
					x => x.UnexposedFrontalArea,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Positive, out result, out message);

					return message;
				})
				.AddTo(disposable);

			Performance = car
				.ObserveProperty(x => x.Performance)
				.Do(_ => Performance?.Value.Dispose())
				.Select(x => new PerformanceViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Delay = car
				.ObserveProperty(x => x.Delay)
				.Do(_ => Delay?.Value.Dispose())
				.Select(x => new DelayViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Move = car
				.ObserveProperty(x => x.Move)
				.Do(_ => Move?.Value.Dispose())
				.Select(x => new MoveViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Brake = car
				.ObserveProperty(x => x.Brake)
				.Do(_ => Brake?.Value.Dispose())
				.Select(x => new BrakeViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Pressure = car
				.ObserveProperty(x => x.Pressure)
				.Do(_ => Pressure?.Value.Dispose())
				.Select(x => new PressureViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Reversed = car
				.ToReactivePropertyAsSynchronized(x => x.Reversed)
				.AddTo(disposable);

			Object = car
				.ToReactivePropertyAsSynchronized(x => x.Object)
				.AddTo(disposable);

			LoadingSway = car
				.ToReactivePropertyAsSynchronized(x => x.LoadingSway)
				.AddTo(disposable);

			DefinedAxles.Subscribe(_ =>
				{
					FrontAxle.ForceValidate();
					RearAxle.ForceValidate();
				});

			FrontAxle.SetValidateNotifyError(x =>
				{
					double front;
					string message;

					if (Utilities.TryParse(x, NumberRange.Any, out front, out message))
					{
						double rear;

						if (DefinedAxles.Value && Utilities.TryParse(RearAxle.Value, NumberRange.Any, out rear) && front <= rear)
						{
							message = "RearAxleはFrontAxle未満でなければなりません。";
						}
					}

					return message;
				})
				.Subscribe(_ => RearAxle.ForceValidate())
				.AddTo(disposable);

			FrontAxle.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => FrontAxle.ForceNotify())
				.AddTo(disposable);

			RearAxle.SetValidateNotifyError(x =>
				{
					double rear;
					string message;

					if (Utilities.TryParse(x, NumberRange.Any, out rear, out message))
					{
						double front;

						if (DefinedAxles.Value && Utilities.TryParse(FrontAxle.Value, NumberRange.Any, out front) && rear >= front)
						{
							message = "RearAxleはFrontAxle未満でなければなりません。";
						}
					}

					return message;
				})
				.Subscribe(_ => FrontAxle.ForceValidate())
				.AddTo(disposable);

			RearAxle.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => RearAxle.ForceNotify())
				.AddTo(disposable);
		}
	}

	internal class MotorCarViewModel : CarViewModel
	{
		internal ReadOnlyReactivePropertySlim<AccelerationViewModel> Acceleration
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<MotorViewModel> Motor
		{
			get;
		}

		internal MotorCarViewModel(MotorCar car, Train train) : base(car)
		{
			Acceleration = car
				.ObserveProperty(x => x.Acceleration)
				.Do(_ => Acceleration?.Value.Dispose())
				.Select(x => new AccelerationViewModel(x, train, car))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Motor = car
				.ObserveProperty(x => x.Motor)
				.Do(_ => Motor?.Value.Dispose())
				.Select(x => new MotorViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);
		}
	}

	internal class TrailerCarViewModel : CarViewModel
	{
		internal TrailerCarViewModel(TrailerCar car) : base(car)
		{

		}
	}
}
