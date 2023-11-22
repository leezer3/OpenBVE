using System.Globalization;
using OpenBveApi.World;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Trains;

namespace TrainEditor2.ViewModels.Trains
{
	internal class PerformanceViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> Deceleration
		{
			get;
		}

		internal ReactiveProperty<Unit.Acceleration> DecelerationUnit
		{
			get;
		}

		internal ReactiveProperty<string> CoefficientOfStaticFriction
		{
			get;
		}

		internal ReactiveProperty<string> CoefficientOfRollingResistance
		{
			get;
		}

		internal ReactiveProperty<string> AerodynamicDragCoefficient
		{
			get;
		}

		internal PerformanceViewModel(Performance performance)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			Deceleration = performance
				.ToReactivePropertyAsSynchronized(
					x => x.Deceleration,
					x => x.Value.ToString(culture),
					x => new Quantity.Acceleration(double.Parse(x, NumberStyles.Float, culture), performance.Deceleration.UnitValue),
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

			DecelerationUnit = performance
				.ToReactivePropertyAsSynchronized(
					x => x.Deceleration,
					x => x.UnitValue,
					x => performance.Deceleration.ToNewUnit(x)
				)
				.AddTo(disposable);

			CoefficientOfStaticFriction = performance
				.ToReactivePropertyAsSynchronized(
					x => x.CoefficientOfStaticFriction,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
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

			CoefficientOfRollingResistance = performance
				.ToReactivePropertyAsSynchronized(
					x => x.CoefficientOfRollingResistance,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
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

			AerodynamicDragCoefficient = performance
				.ToReactivePropertyAsSynchronized(
					x => x.AerodynamicDragCoefficient,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
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
