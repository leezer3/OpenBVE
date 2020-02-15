using System.Globalization;
using OpenBveApi.Colors;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Panels;

namespace TrainEditor2.ViewModels.Panels
{
	internal class ThisViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> Resolution
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

		internal ReactiveProperty<string> Top
		{
			get;
		}

		internal ReactiveProperty<string> Bottom
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

		internal ReactiveProperty<string> CenterX
		{
			get;
		}

		internal ReactiveProperty<string> CenterY
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

		internal ThisViewModel(This _this)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			Resolution = _this
				.ToReactivePropertyAsSynchronized(
					x => x.Resolution,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
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

			Left = _this
				.ToReactivePropertyAsSynchronized(
					x => x.Left,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
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

			Right = _this
				.ToReactivePropertyAsSynchronized(
					x => x.Right,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
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

			Bottom = _this
				.ToReactivePropertyAsSynchronized(
					x => x.Bottom,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
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

			Top = _this
				.ToReactivePropertyAsSynchronized(
					x => x.Top,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
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

			DaytimeImage = _this
				.ToReactivePropertyAsSynchronized(x => x.DaytimeImage)
				.AddTo(disposable);

			NighttimeImage = _this
				.ToReactivePropertyAsSynchronized(x => x.NighttimeImage)
				.AddTo(disposable);

			TransparentColor = _this
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

			CenterX = _this
				.ToReactivePropertyAsSynchronized(
					x => x.CenterX,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
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

			CenterY = _this
				.ToReactivePropertyAsSynchronized(
					x => x.CenterY,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
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

			OriginX = _this
				.ToReactivePropertyAsSynchronized(
					x => x.OriginX,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
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

			OriginY = _this
				.ToReactivePropertyAsSynchronized(
					x => x.OriginY,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
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
