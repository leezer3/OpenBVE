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
	internal class DigitalGaugeElementViewModel : PanelElementViewModel
	{
		internal ReadOnlyReactivePropertySlim<SubjectViewModel> Subject
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

		internal ReactiveProperty<string> Step
		{
			get;
		}

		internal DigitalGaugeElementViewModel(DigitalGaugeElement digitalGauge) : base(digitalGauge)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			Subject = digitalGauge
				.ObserveProperty(x => x.Subject)
				.Do(_ => Subject?.Value.Dispose())
				.Select(x => new SubjectViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Radius = digitalGauge
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

			Color = digitalGauge
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

			InitialAngle = digitalGauge
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

			LastAngle = digitalGauge
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

			Minimum = digitalGauge
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

			Maximum = digitalGauge
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

			Step = digitalGauge
				.ToReactivePropertyAsSynchronized(
					x => x.Step,
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
		}
	}
}
