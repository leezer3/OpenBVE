using System.Globalization;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class MoveViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> JerkPowerUp
		{
			get;
		}

		internal ReactiveProperty<string> JerkPowerDown
		{
			get;
		}

		internal ReactiveProperty<string> JerkBrakeUp
		{
			get;
		}

		internal ReactiveProperty<string> JerkBrakeDown
		{
			get;
		}

		internal ReactiveProperty<string> BrakeCylinderUp
		{
			get;
		}

		internal ReactiveProperty<string> BrakeCylinderDown
		{
			get;
		}

		internal MoveViewModel(Move move)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			JerkPowerUp = move
				.ToReactivePropertyAsSynchronized(
					x => x.JerkPowerUp,
					x => x.ToString(culture),
					double.Parse,
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

			JerkPowerDown = move
				.ToReactivePropertyAsSynchronized(
					x => x.JerkPowerDown,
					x => x.ToString(culture),
					double.Parse,
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

			JerkBrakeUp = move
				.ToReactivePropertyAsSynchronized(
					x => x.JerkBrakeDown,
					x => x.ToString(culture),
					double.Parse,
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

			JerkBrakeDown = move
				.ToReactivePropertyAsSynchronized(
					x => x.JerkBrakeDown,
					x => x.ToString(culture),
					double.Parse,
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

			BrakeCylinderUp = move
				.ToReactivePropertyAsSynchronized(
					x => x.BrakeCylinderUp,
					x => x.ToString(culture),
					double.Parse,
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

			BrakeCylinderDown = move
				.ToReactivePropertyAsSynchronized(
					x => x.BrakeCylinderDown,
					x => x.ToString(culture),
					double.Parse,
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
		}
	}
}
