using System;
using System.Globalization;
using System.Reactive.Linq;
using OpenBveApi.Units;
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

			internal ReactiveProperty<Unit.Length> FrontAxleUnit
			{
				get;
			}

			internal ReactiveProperty<string> RearAxle
			{
				get;
			}

			internal ReactiveProperty<Unit.Length> RearAxleUnit
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
						x => x.Value.ToString(culture),
						x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), bogie.FrontAxle.UnitValue),
						ignoreValidationErrorValue: true
					)
					.AddTo(disposable);

				FrontAxleUnit = bogie
					.ToReactivePropertyAsSynchronized(
						x => x.FrontAxle,
						x => x.UnitValue,
						x => bogie.FrontAxle.ToNewUnit(x)
					)
					.AddTo(disposable);

				RearAxle = bogie
					.ToReactivePropertyAsSynchronized(
						x => x.RearAxle,
						x => x.Value.ToString(culture),
						x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), bogie.RearAxle.UnitValue),
						ignoreValidationErrorValue: true
					)
					.AddTo(disposable);

				RearAxleUnit = bogie
					.ToReactivePropertyAsSynchronized(
						x => x.RearAxle,
						x => x.UnitValue,
						x => bogie.RearAxle.ToNewUnit(x)
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
					})
					.AddTo(disposable);

				FrontAxle.SetValidateNotifyError(x =>
					{
						double front;
						string message;

						if (Utilities.TryParse(x, NumberRange.Any, out front, out message))
						{
							double rear;

							if (DefinedAxles.Value && Utilities.TryParse(RearAxle.Value, NumberRange.Any, out rear) && new Quantity.Length(front, bogie.FrontAxle.UnitValue) <= new Quantity.Length(rear, bogie.RearAxle.UnitValue))
							{
								message = "RearAxle must be less than FrontAxle.";
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

							if (DefinedAxles.Value && Utilities.TryParse(FrontAxle.Value, NumberRange.Any, out front) && new Quantity.Length(rear, bogie.RearAxle.UnitValue) >= new Quantity.Length(front, bogie.FrontAxle.UnitValue))
							{
								message = "RearAxle must be less than FrontAxle.";
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

		internal class DoorViewModel : BaseViewModel
		{
			internal ReactiveProperty<string> Width
			{
				get;
			}

			internal ReactiveProperty<Unit.Length> WidthUnit
			{
				get;
			}

			internal ReactiveProperty<string> MaxTolerance
			{
				get;
			}

			internal ReactiveProperty<Unit.Length> MaxToleranceUnit
			{
				get;
			}

			internal DoorViewModel(Car.Door door)
			{
				CultureInfo culture = CultureInfo.InvariantCulture;

				Width = door
					.ToReactivePropertyAsSynchronized(
						x => x.Width,
						x => x.Value.ToString(culture),
						x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), door.Width.UnitValue),
						ignoreValidationErrorValue: true
					)
					.SetValidateNotifyError(x =>
					{
						double result;
						string message;

						Utilities.TryParse(x, NumberRange.NonNegative, out result, out message);

						return message;
					})
					.AddTo(disposable);

				WidthUnit = door
					.ToReactivePropertyAsSynchronized(
						x => x.Width,
						x => x.UnitValue,
						x => door.Width.ToNewUnit(x)
					)
					.AddTo(disposable);

				MaxTolerance = door
					.ToReactivePropertyAsSynchronized(
						x => x.MaxTolerance,
						x => x.Value.ToString(culture),
						x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), door.MaxTolerance.UnitValue),
						ignoreValidationErrorValue: true
					)
					.SetValidateNotifyError(x =>
					{
						double result;
						string message;

						Utilities.TryParse(x, NumberRange.NonNegative, out result, out message);

						return message;
					})
					.AddTo(disposable);

				MaxToleranceUnit = door
					.ToReactivePropertyAsSynchronized(
						x => x.MaxTolerance,
						x => x.UnitValue,
						x => door.MaxTolerance.ToNewUnit(x)
					)
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

		internal ReactiveProperty<Unit.Mass> MassUnit
		{
			get;
		}

		internal ReactiveProperty<string> Length
		{
			get;
		}

		internal ReactiveProperty<Unit.Length> LengthUnit
		{
			get;
		}

		internal ReactiveProperty<string> Width
		{
			get;
		}

		internal ReactiveProperty<Unit.Length> WidthUnit
		{
			get;
		}

		internal ReactiveProperty<string> Height
		{
			get;
		}

		internal ReactiveProperty<Unit.Length> HeightUnit
		{
			get;
		}

		internal ReactiveProperty<string> CenterOfGravityHeight
		{
			get;
		}

		internal ReactiveProperty<Unit.Length> CenterOfGravityHeightUnit
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

		internal ReactiveProperty<Unit.Length> FrontAxleUnit
		{
			get;
		}

		internal ReactiveProperty<string> RearAxle
		{
			get;
		}

		internal ReactiveProperty<Unit.Length> RearAxleUnit
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

		internal ReadOnlyReactivePropertySlim<JerkViewModel> Jerk
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

		internal ReadOnlyReactivePropertySlim<DoorViewModel> LeftDoor
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<DoorViewModel> RightDoor
		{
			get;
		}

		internal ReactiveProperty<Car.ReAdhesionDevices> ReAdhesionDevice
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
					x => x.Value.ToString(culture),
					x => new Quantity.Mass(double.Parse(x, NumberStyles.Float, culture), car.Mass.UnitValue),
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

			MassUnit = car
				.ToReactivePropertyAsSynchronized(
					x => x.Mass,
					x => x.UnitValue,
					x => car.Mass.ToNewUnit(x)
				)
				.AddTo(disposable);

			Length = car
				.ToReactivePropertyAsSynchronized(
					x => x.Length,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), car.Length.UnitValue),
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

			LengthUnit = car
				.ToReactivePropertyAsSynchronized(
					x => x.Length,
					x => x.UnitValue,
					x => car.Length.ToNewUnit(x)
				)
				.AddTo(disposable);

			Width = car
				.ToReactivePropertyAsSynchronized(
					x => x.Width,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), car.Width.UnitValue),
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

			WidthUnit = car
				.ToReactivePropertyAsSynchronized(
					x => x.Width,
					x => x.UnitValue,
					x => car.Width.ToNewUnit(x)
				)
				.AddTo(disposable);

			Height = car
				.ToReactivePropertyAsSynchronized(
					x => x.Height,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), car.Height.UnitValue),
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

			HeightUnit = car
				.ToReactivePropertyAsSynchronized(
					x => x.Height,
					x => x.UnitValue,
					x => car.Height.ToNewUnit(x)
				)
				.AddTo(disposable);

			CenterOfGravityHeight = car
				.ToReactivePropertyAsSynchronized(
					x => x.CenterOfGravityHeight,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), car.CenterOfGravityHeight.UnitValue),
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

			CenterOfGravityHeightUnit = car
				.ToReactivePropertyAsSynchronized(
					x => x.CenterOfGravityHeight,
					x => x.UnitValue,
					x => car.CenterOfGravityHeight.ToNewUnit(x)
				)
				.AddTo(disposable);

			DefinedAxles = car
				.ToReactivePropertyAsSynchronized(x => x.DefinedAxles)
				.AddTo(disposable);

			FrontAxle = car
				.ToReactivePropertyAsSynchronized(
					x => x.FrontAxle,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), car.FrontAxle.UnitValue),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			FrontAxleUnit = car
				.ToReactivePropertyAsSynchronized(
					x => x.FrontAxle,
					x => x.UnitValue,
					x => car.FrontAxle.ToNewUnit(x)
				)
				.AddTo(disposable);

			RearAxle = car
				.ToReactivePropertyAsSynchronized(
					x => x.RearAxle,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), car.RearAxle.UnitValue),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			RearAxleUnit = car
				.ToReactivePropertyAsSynchronized(
					x => x.RearAxle,
					x => x.UnitValue,
					x => car.RearAxle.ToNewUnit(x)
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
					x => double.Parse(x, NumberStyles.Float, culture),
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
					x => double.Parse(x, NumberStyles.Float, culture),
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

			Jerk = car
				.ObserveProperty(x => x.Jerk)
				.Do(_ => Jerk?.Value.Dispose())
				.Select(x => new JerkViewModel(x))
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

			LeftDoor = car
				.ObserveProperty(x => x.LeftDoor)
				.Do(_ => LeftDoor?.Value.Dispose())
				.Select(x => new DoorViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			RightDoor = car
				.ObserveProperty(x => x.RightDoor)
				.Do(_ => RightDoor?.Value.Dispose())
				.Select(x => new DoorViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			ReAdhesionDevice = car
				.ToReactivePropertyAsSynchronized(x => x.ReAdhesionDevice)
				.AddTo(disposable);

			DefinedAxles.Subscribe(_ =>
				{
					FrontAxle.ForceValidate();
					RearAxle.ForceValidate();
				})
				.AddTo(disposable);

			FrontAxle.SetValidateNotifyError(x =>
				{
					double front;
					string message;

					if (Utilities.TryParse(x, NumberRange.Any, out front, out message))
					{
						double rear;

						if (DefinedAxles.Value && Utilities.TryParse(RearAxle.Value, NumberRange.Any, out rear) && new Quantity.Length(front, car.FrontAxle.UnitValue) <= new Quantity.Length(rear, car.RearAxle.UnitValue))
						{
							message = "RearAxle must be less than FrontAxle.";
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

						if (DefinedAxles.Value && Utilities.TryParse(FrontAxle.Value, NumberRange.Any, out front) && new Quantity.Length(rear, car.RearAxle.UnitValue) >= new Quantity.Length(front, car.FrontAxle.UnitValue))
						{
							message = "RearAxle must be less than FrontAxle.";
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

	internal abstract class MotorCarViewModel : CarViewModel
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

	internal abstract class TrailerCarViewModel : CarViewModel
	{
		internal TrailerCarViewModel(Car car) : base(car)
		{
		}
	}

	internal class ControlledMotorCarViewModel : MotorCarViewModel
	{
		internal ReadOnlyReactivePropertySlim<CabViewModel> Cab
		{
			get;
		}

		internal ControlledMotorCarViewModel(ControlledMotorCar car, Train train) : base(car, train)
		{
			Cab = car
				.ObserveProperty(x => x.Cab)
				.Do(_ => Cab?.Value.Dispose())
				.Select(x =>
				{
					CabViewModel viewModel = null;

					EmbeddedCab embeddedCab = x as EmbeddedCab;
					ExternalCab externalCab = x as ExternalCab;

					if (embeddedCab != null)
					{
						viewModel = new EmbeddedCabViewModel(embeddedCab);
					}

					if (externalCab != null)
					{
						viewModel = new ExternalCabViewModel(externalCab);
					}

					return viewModel;
				})
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);
		}
	}

	internal class UncontrolledMotorCarViewModel : MotorCarViewModel
	{
		internal UncontrolledMotorCarViewModel(MotorCar car, Train train) : base(car, train)
		{
		}
	}

	internal class ControlledTrailerCarViewModel : TrailerCarViewModel
	{
		internal ReadOnlyReactivePropertySlim<CabViewModel> Cab
		{
			get;
		}

		internal ControlledTrailerCarViewModel(ControlledTrailerCar car) : base(car)
		{
			Cab = car
				.ObserveProperty(x => x.Cab)
				.Do(_ => Cab?.Value.Dispose())
				.Select(x =>
				{
					CabViewModel viewModel = null;

					EmbeddedCab embeddedCab = x as EmbeddedCab;
					ExternalCab externalCab = x as ExternalCab;

					if (embeddedCab != null)
					{
						viewModel = new EmbeddedCabViewModel(embeddedCab);
					}

					if (externalCab != null)
					{
						viewModel = new ExternalCabViewModel(externalCab);
					}

					return viewModel;
				})
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);
		}
	}

	internal class UncontrolledTrailerCarViewModel : TrailerCarViewModel
	{
		internal UncontrolledTrailerCarViewModel(Car car) : base(car)
		{
		}
	}
}
