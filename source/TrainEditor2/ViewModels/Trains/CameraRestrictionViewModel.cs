using System;
using System.Globalization;
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

		internal ReactiveProperty<string> Backwards
		{
			get;
		}

		internal ReactiveProperty<string> Left
		{
			get;
		}

		internal ReactiveProperty<string> Right
		{
			get;
		}

		internal ReactiveProperty<string> Up
		{
			get;
		}

		internal ReactiveProperty<string> Down
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
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			Backwards = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Backwards,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			Left = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Left,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			Right = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Right,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			Up = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Up,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			Down = restriction
				.ToReactivePropertyAsSynchronized(
					x => x.Down,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			DefinedForwards.Subscribe(_ => Forwards.ForceValidate()).AddTo(disposable);

			Forwards.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedForwards.Value)
					{
						double result;
						Utilities.TryParse(x, NumberRange.Any, out result, out message);
					}
					else
					{
						message = string.Empty;
					}

					return message;
				})
				.AddTo(disposable);

			DefinedBackwards.Subscribe(_ => Backwards.ForceValidate()).AddTo(disposable);

			Backwards.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedBackwards.Value)
					{
						double result;
						Utilities.TryParse(x, NumberRange.Any, out result, out message);
					}
					else
					{
						message = string.Empty;
					}

					return message;
				})
				.AddTo(disposable);

			DefinedLeft.Subscribe(_ => Left.ForceValidate()).AddTo(disposable);

			Left.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedLeft.Value)
					{
						double result;
						Utilities.TryParse(x, NumberRange.Any, out result, out message);
					}
					else
					{
						message = string.Empty;
					}

					return message;
				})
				.AddTo(disposable);

			DefinedRight.Subscribe(_ => Right.ForceValidate()).AddTo(disposable);

			Right.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedRight.Value)
					{
						double result;
						Utilities.TryParse(x, NumberRange.Any, out result, out message);
					}
					else
					{
						message = string.Empty;
					}

					return message;
				})
				.AddTo(disposable);

			DefinedUp.Subscribe(_ => Up.ForceValidate()).AddTo(disposable);

			Up.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedUp.Value)
					{
						double result;
						Utilities.TryParse(x, NumberRange.Any, out result, out message);
					}
					else
					{
						message = string.Empty;
					}

					return message;
				})
				.AddTo(disposable);

			DefinedDown.Subscribe(_ => Down.ForceValidate()).AddTo(disposable);

			Down.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedDown.Value)
					{
						double result;
						Utilities.TryParse(x, NumberRange.Any, out result, out message);
					}
					else
					{
						message = string.Empty;
					}

					return message;
				})
				.AddTo(disposable);
		}
	}
}
