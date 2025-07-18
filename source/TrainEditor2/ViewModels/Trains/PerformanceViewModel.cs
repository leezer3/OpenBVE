﻿using System.Globalization;
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

			CoefficientOfStaticFriction = performance
				.ToReactivePropertyAsSynchronized(
					x => x.CoefficientOfStaticFriction,
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

			CoefficientOfRollingResistance = performance
				.ToReactivePropertyAsSynchronized(
					x => x.CoefficientOfRollingResistance,
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

			AerodynamicDragCoefficient = performance
				.ToReactivePropertyAsSynchronized(
					x => x.AerodynamicDragCoefficient,
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
