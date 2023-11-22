using System;
using System.Globalization;
using System.Reactive.Linq;
using OpenBveApi.World;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class CameraRestrictionViewModel : BaseViewModel
	{
		internal ReactiveProperty<bool> DefinedForwards
		{
			get;
		}

		internal ReactiveProperty<bool> DefinedBackwards
		{
			get;
		}

		internal ReactiveProperty<bool> DefinedLeft
		{
			get;
		}

		internal ReactiveProperty<bool> DefinedRight
		{
			get;
		}

		internal ReactiveProperty<bool> DefinedUp
		{
			get;
		}

		internal ReactiveProperty<bool> DefinedDown
		{
			get;
		}

		internal ReactiveProperty<string> Forwards
		{
			get;
		}

		internal ReactiveProperty<UnitOfLength> ForwardsUnit
		{
			get;
		}

		internal ReactiveProperty<string> Backwards
		{
			get;
		}

		internal ReactiveProperty<UnitOfLength> BackwardsUnit
		{
			get;
		}

		internal ReactiveProperty<string> Left
		{
			get;
		}

		internal ReactiveProperty<UnitOfLength> LeftUnit
		{
			get;
		}

		internal ReactiveProperty<string> Right
		{
			get;
		}

		internal ReactiveProperty<UnitOfLength> RightUnit
		{
			get;
		}

		internal ReactiveProperty<string> Up
		{
			get;
		}

		internal ReactiveProperty<UnitOfLength> UpUnit
		{
			get;
		}

		internal ReactiveProperty<string> Down
		{
			get;
		}

		internal ReactiveProperty<UnitOfLength> DownUnit
		{
			get;
		}

		internal CameraRestrictionViewModel(CameraRestriction restriction)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			DefinedForwards = restriction
				.ToReactivePropertyAsSynchronized(x => x.DefinedForwards)
				.AddTo(disposable);

			DefinedBackwards = restriction
				.ToReactivePropertyAsSynchronized(x => x.DefinedBackwards)
				.AddTo(disposable);

			DefinedLeft = restriction
				.ToReactivePropertyAsSynchronized(x => x.DefinedLeft)
				.AddTo(disposable);

			DefinedRight = restriction
				.ToReactivePropertyAsSynchronized(x => x.DefinedRight)
				.AddTo(disposable);

			DefinedUp = restriction
				.ToReactivePropertyAsSynchronized(x => x.DefinedUp)
				.AddTo(disposable);

			DefinedDown = restriction
				.ToReactivePropertyAsSynchronized(x => x.DefinedDown)
				.AddTo(disposable);

			Forwards = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Forwards,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), restriction.Forwards.UnitValue),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			ForwardsUnit = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Forwards,
					x => x.UnitValue,
					x => restriction.Forwards.ToNewUnit(x)
				)
				.AddTo(disposable);

			Backwards = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Backwards,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), restriction.Backwards.UnitValue),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			BackwardsUnit = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Backwards,
					x => x.UnitValue,
					x => restriction.Backwards.ToNewUnit(x)
				)
				.AddTo(disposable);

			Left = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Left,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), restriction.Left.UnitValue),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			LeftUnit = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Left,
					x => x.UnitValue,
					x => restriction.Left.ToNewUnit(x)
				)
				.AddTo(disposable);

			Right = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Right,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), restriction.Right.UnitValue),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			RightUnit = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Right,
					x => x.UnitValue,
					x => restriction.Right.ToNewUnit(x)
				)
				.AddTo(disposable);

			Up = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Up,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), restriction.Up.UnitValue),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			UpUnit = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Up,
					x => x.UnitValue,
					x => restriction.Up.ToNewUnit(x)
				)
				.AddTo(disposable);

			Down = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Down,
					x => x.Value.ToString(culture),
					x => new Quantity.Length(double.Parse(x, NumberStyles.Float, culture), restriction.Down.UnitValue),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			DownUnit = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Down,
					x => x.UnitValue,
					x => restriction.Down.ToNewUnit(x)
				)
				.AddTo(disposable);

			DefinedForwards.Subscribe(_ =>
				{
					Forwards.ForceValidate();
					Backwards.ForceValidate();
				})
				.AddTo(disposable);

			Forwards.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedForwards.Value)
					{
						double forwards;

						if (Utilities.TryParse(x, NumberRange.Any, out forwards, out message))
						{
							double backwards;

							if (DefinedBackwards.Value && Utilities.TryParse(Backwards.Value, NumberRange.Any, out backwards) && new Quantity.Length(forwards, restriction.Forwards.UnitValue) < new Quantity.Length(backwards, restriction.Backwards.UnitValue))
							{
								message = "Backwards is expected to be less than or equal to Forwards.";
							}
						}
					}
					else
					{
						message = string.Empty;
					}

					return message;
				})
				.Subscribe(_ => Backwards.ForceValidate())
				.AddTo(disposable);

			Forwards.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => Forwards.ForceNotify())
				.AddTo(disposable);

			DefinedBackwards.Subscribe(_ =>
				{
					Forwards.ForceValidate();
					Backwards.ForceValidate();
				})
				.AddTo(disposable);

			Backwards.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedBackwards.Value)
					{
						double backwards;

						if (Utilities.TryParse(x, NumberRange.Any, out backwards, out message))
						{
							double forwards;

							if (DefinedForwards.Value && Utilities.TryParse(Forwards.Value, NumberRange.Any, out forwards) && new Quantity.Length(backwards, restriction.Backwards.UnitValue) > new Quantity.Length(forwards, restriction.Forwards.UnitValue))
							{
								message = "Backwards is expected to be less than or equal to Forwards.";
							}
						}
					}
					else
					{
						message = string.Empty;
					}

					return message;
				})
				.Subscribe(_ => Forwards.ForceValidate())
				.AddTo(disposable);

			Backwards.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => Backwards.ForceNotify())
				.AddTo(disposable);

			DefinedLeft.Subscribe(_ =>
				{
					Left.ForceValidate();
					Right.ForceValidate();
				})
				.AddTo(disposable);

			Left.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedLeft.Value)
					{
						double left;

						if (Utilities.TryParse(x, NumberRange.Any, out left, out message))
						{
							double right;

							if (DefinedRight.Value && Utilities.TryParse(Right.Value, NumberRange.Any, out right) && new Quantity.Length(left, restriction.Left.UnitValue) > new Quantity.Length(right, restriction.Right.UnitValue))
							{
								message = "Left is expected to be less than or equal to Right.";
							}
						}
					}
					else
					{
						message = string.Empty;
					}

					return message;
				})
				.Subscribe(_ => Right.ForceValidate())
				.AddTo(disposable);

			Left.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => Left.ForceNotify())
				.AddTo(disposable);

			DefinedRight.Subscribe(_ =>
				{
					Left.ForceValidate();
					Right.ForceValidate();
				})
				.AddTo(disposable);

			Right.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedRight.Value)
					{
						double right;

						if (Utilities.TryParse(x, NumberRange.Any, out right, out message))
						{
							double left;

							if (DefinedLeft.Value && Utilities.TryParse(Left.Value, NumberRange.Any, out left) && new Quantity.Length(right, restriction.Right.UnitValue) < new Quantity.Length(left, restriction.Left.UnitValue))
							{
								message = "Left is expected to be less than or equal to Right";
							}
						}
					}
					else
					{
						message = string.Empty;
					}

					return message;
				})
				.Subscribe(_ => Left.ForceValidate())
				.AddTo(disposable);

			Right.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => Right.ForceNotify())
				.AddTo(disposable);

			DefinedUp.Subscribe(_ =>
				{
					Up.ForceValidate();
					Down.ForceValidate();
				})
				.AddTo(disposable);

			Up.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedUp.Value)
					{
						double up;

						if (Utilities.TryParse(x, NumberRange.Any, out up, out message))
						{
							double down;

							if (DefinedDown.Value && Utilities.TryParse(Down.Value, NumberRange.Any, out down) && new Quantity.Length(up, restriction.Up.UnitValue) < new Quantity.Length(down, restriction.Down.UnitValue))
							{
								message = "Down is expected to be less than or equal to Up.";
							}
						}
					}
					else
					{
						message = string.Empty;
					}

					return message;
				})
				.Subscribe(_ => Down.ForceValidate())
				.AddTo(disposable);

			Up.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => Up.ForceNotify())
				.AddTo(disposable);

			DefinedDown.Subscribe(_ =>
				{
					Up.ForceValidate();
					Down.ForceValidate();
				})
				.AddTo(disposable);

			Down.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedDown.Value)
					{
						double down;

						if (Utilities.TryParse(x, NumberRange.Any, out down, out message))
						{
							double up;

							if (DefinedUp.Value && Utilities.TryParse(Up.Value, NumberRange.Any, out up) && new Quantity.Length(down, restriction.Down.UnitValue) > new Quantity.Length(up, restriction.Up.UnitValue))
							{
								message = "Down is expected to be less than or equal to Up.";
							}
						}
					}
					else
					{
						message = string.Empty;
					}

					return message;
				})
				.Subscribe(_ => Up.ForceValidate())
				.AddTo(disposable);

			Down.ObserveHasErrors
				.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.DistinctUntilChanged)
				.Where(x => !x)
				.Subscribe(_ => Down.ForceNotify())
				.AddTo(disposable);
		}
	}
}
