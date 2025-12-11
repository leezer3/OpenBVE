using System.Globalization;
using System.Reactive.Linq;
using Formats.OpenBve;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Panels;

namespace TrainEditor2.ViewModels.Panels
{
	internal class LinearGaugeElementViewModel : PanelElementViewModel
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

		internal ReactiveProperty<string> Minimum
		{
			get;
		}

		internal ReactiveProperty<string> Maximum
		{
			get;
		}

		internal ReactiveProperty<int> DirectionX
		{
			get;
		}

		internal ReactiveProperty<int> DirectionY
		{
			get;
		}

		internal ReactiveProperty<int> Width
		{
			get;
		}

		internal LinearGaugeElementViewModel(LinearGaugeElement linearGauge) : base(linearGauge)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			Subject = linearGauge
				.ObserveProperty(x => x.Subject)
				.Do(_ => Subject?.Value.Dispose())
				.Select(x => new SubjectViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			DaytimeImage = linearGauge
				.ToReactivePropertyAsSynchronized(x => x.DaytimeImage)
				.AddTo(disposable);

			NighttimeImage = linearGauge
				.ToReactivePropertyAsSynchronized(x => x.NighttimeImage)
				.AddTo(disposable);

			TransparentColor = linearGauge
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

			Minimum = linearGauge
				.ToReactivePropertyAsSynchronized(
					x => x.Minimum,
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

			Maximum = linearGauge
				.ToReactivePropertyAsSynchronized(
					x => x.Maximum,
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

			DirectionX = linearGauge
				.ToReactivePropertyAsSynchronized(
					x => x.Direction,
					x => (int)x.X,
					x => new Vector2(x, linearGauge.Direction.Y))
				.AddTo(disposable);

			DirectionY = linearGauge
				.ToReactivePropertyAsSynchronized(
					x => x.Direction,
					x => (int)x.Y,
					x => new Vector2(linearGauge.Direction.X, x))
				.AddTo(disposable);

			Width = linearGauge
				.ToReactivePropertyAsSynchronized(x => x.Width)
				.AddTo(disposable);
		}
	}
}
