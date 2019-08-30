using System.Globalization;
using System.Reactive.Linq;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Panels;

namespace TrainEditor2.ViewModels.Panels
{
	internal class NeedleElementViewModel : PanelElementViewModel
	{
		internal ReadOnlyReactivePropertySlim<SubjectViewModel> Subject
		{
			get;
		}

		internal ReactiveProperty<string> DaytimeImage
		{
			get;
		}

		internal ReactiveProperty<string> NighttimeImage
		{
			get;
		}

		internal ReactiveProperty<string> TransparentColor
		{
			get;
		}

		internal ReactiveProperty<bool> DefinedRadius
		{
			get;
		}

		internal ReactiveProperty<string> Radius
		{
			get;
		}

		internal ReactiveProperty<string> Color
		{
			get;
		}

		internal ReactiveProperty<bool> DefinedOrigin
		{
			get;
		}

		internal ReactiveProperty<string> OriginX
		{
			get;
		}

		internal ReactiveProperty<string> OriginY
		{
			get;
		}

		internal ReactiveProperty<string> InitialAngle
		{
			get;
		}

		internal ReactiveProperty<string> LastAngle
		{
			get;
		}

		internal ReactiveProperty<string> Minimum
		{
			get;
		}

		internal ReactiveProperty<string> Maximum
		{
			get;
		}

		internal ReactiveProperty<bool> DefinedNaturalFreq
		{
			get;
		}

		internal ReactiveProperty<string> NaturalFreq
		{
			get;
		}

		internal ReactiveProperty<bool> DefinedDampingRatio
		{
			get;
		}

		internal ReactiveProperty<string> DampingRatio
		{
			get;
		}

		internal ReactiveProperty<bool> Backstop
		{
			get;
		}

		internal ReactiveProperty<bool> Smoothed
		{
			get;
		}

		internal NeedleElementViewModel(NeedleElement needle) : base(needle)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			Subject = needle
				.ObserveProperty(x => x.Subject)
				.Do(_ => Subject?.Value.Dispose())
				.Select(x => new SubjectViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			DaytimeImage = needle
				.ToReactivePropertyAsSynchronized(x => x.DaytimeImage)
				.AddTo(disposable);

			NighttimeImage = needle
				.ToReactivePropertyAsSynchronized(x => x.NighttimeImage)
				.AddTo(disposable);

			TransparentColor = needle
				.ToReactivePropertyAsSynchronized(
					x => x.TransparentColor,
					x => x.ToString(),
					Color24.ParseHexColor,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Color24 result;
					string message;

					Utilities.TryParse(x, out result, out message);

					return message;
				})
				.AddTo(disposable);

			DefinedRadius = needle
				.ToReactivePropertyAsSynchronized(x => x.DefinedRadius)
				.AddTo(disposable);

			Radius = needle
				.ToReactivePropertyAsSynchronized(
					x => x.Radius,
					x => x.ToString(culture),
					double.Parse,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.NonZero, out result, out message);

					return message;
				})
				.AddTo(disposable);

			Color = needle
				.ToReactivePropertyAsSynchronized(
					x => x.Color,
					x => x.ToString(),
					Color24.ParseHexColor,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Color24 result;
					string message;

					Utilities.TryParse(x, out result, out message);

					return message;
				})
				.AddTo(disposable);

			DefinedOrigin = needle
				.ToReactivePropertyAsSynchronized(x => x.DefinedOrigin)
				.AddTo(disposable);

			OriginX = needle
				.ToReactivePropertyAsSynchronized(
					x => x.OriginX,
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

			OriginY = needle
				.ToReactivePropertyAsSynchronized(
					x => x.OriginY,
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

			InitialAngle = needle
				.ToReactivePropertyAsSynchronized(
					x => x.InitialAngle,
					x => x.ToDegrees().ToString(culture),
					x => double.Parse(x).ToRadians(),
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

			LastAngle = needle
				.ToReactivePropertyAsSynchronized(
					x => x.LastAngle,
					x => x.ToDegrees().ToString(culture),
					x => double.Parse(x).ToRadians(),
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

			Minimum = needle
				.ToReactivePropertyAsSynchronized(
					x => x.Minimum,
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

			Maximum = needle
				.ToReactivePropertyAsSynchronized(
					x => x.Maximum,
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

			DefinedNaturalFreq = needle
				.ToReactivePropertyAsSynchronized(x => x.DefinedNaturalFreq)
				.AddTo(disposable);

			NaturalFreq = needle
				.ToReactivePropertyAsSynchronized(
					x => x.NaturalFreq,
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

			DefinedDampingRatio = needle
				.ToReactivePropertyAsSynchronized(x => x.DefinedDampingRatio)
				.AddTo(disposable);

			DampingRatio = needle
				.ToReactivePropertyAsSynchronized(
					x => x.DampingRatio,
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

			Backstop = needle
				.ToReactivePropertyAsSynchronized(x => x.Backstop)
				.AddTo(disposable);

			Smoothed = needle
				.ToReactivePropertyAsSynchronized(x => x.Smoothed)
				.AddTo(disposable);
		}
	}
}
