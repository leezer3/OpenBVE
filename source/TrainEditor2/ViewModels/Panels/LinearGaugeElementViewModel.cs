using System.Globalization;
using System.Reactive.Linq;
using OpenBveApi.Colors;
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
					Color24 result;
					string message;

					Utilities.TryParse(x, out result, out message);

					return message;
				})
				.AddTo(disposable);

			Minimum = linearGauge
				.ToReactivePropertyAsSynchronized(
					x => x.Minimum,
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

			Maximum = linearGauge
				.ToReactivePropertyAsSynchronized(
					x => x.Maximum,
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

			DirectionX = linearGauge
				.ToReactivePropertyAsSynchronized(x => x.DirectionX)
				.AddTo(disposable);

			DirectionY = linearGauge
				.ToReactivePropertyAsSynchronized(x => x.DirectionY)
				.AddTo(disposable);

			Width = linearGauge
				.ToReactivePropertyAsSynchronized(x => x.Width)
				.AddTo(disposable);
		}
	}
}
