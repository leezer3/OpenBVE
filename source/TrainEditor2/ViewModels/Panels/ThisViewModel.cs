using System.Globalization;
using Formats.OpenBve;
using OpenBveApi.Colors;
using OpenBveApi.Math;
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
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.Any, out string message);
					return message;
				})
				.AddTo(disposable);

			Left = _this
				.ToReactivePropertyAsSynchronized(
					x => x.Left,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.Any, out string message);
					return message;
				})
				.AddTo(disposable);

			Right = _this
				.ToReactivePropertyAsSynchronized(
					x => x.Right,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.Any, out string message);
					return message;
				})
				.AddTo(disposable);

			Bottom = _this
				.ToReactivePropertyAsSynchronized(
					x => x.Bottom,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.Any, out string message);
					return message;
				})
				.AddTo(disposable);

			Top = _this
				.ToReactivePropertyAsSynchronized(
					x => x.Top,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.Any, out string message);
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
					Utilities.TryParse(x, out Color24 result, out string message);

					return message;
				})
				.AddTo(disposable);

			CenterX = _this
				.ToReactivePropertyAsSynchronized(
					x => x.Center,
					x => x.X.ToString(culture),
					x => new Vector2(x.Parse(), _this.Center.Y),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.Any, out string message);
					return message;
				})
				.AddTo(disposable);

			CenterY = _this
				.ToReactivePropertyAsSynchronized(
					x => x.Center,
					x => x.Y.ToString(culture),
					x => new Vector2(_this.Center.X, x.Parse()),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.Any, out string message);
					return message;
				})
				.AddTo(disposable);

			OriginX = _this
				.ToReactivePropertyAsSynchronized(
					x => x.Origin,
					x => x.X.ToString(culture),
					x => new Vector2(x.Parse(), _this.Origin.Y),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.Any, out string message);
					return message;
				})
				.AddTo(disposable);

			OriginY = _this
				.ToReactivePropertyAsSynchronized(
					x => x.Origin,
					x => x.Y.ToString(culture),
					x => new Vector2(_this.Origin.X, x.Parse()),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.Any, out string message);
					return message;
				})
				.AddTo(disposable);
		}
	}
}
