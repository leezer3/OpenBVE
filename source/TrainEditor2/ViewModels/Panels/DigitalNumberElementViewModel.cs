using System.Reactive.Linq;
using OpenBveApi.Colors;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Panels;

namespace TrainEditor2.ViewModels.Panels
{
	internal class DigitalNumberElementViewModel : PanelElementViewModel
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

		internal ReactiveProperty<int> Interval
		{
			get;
		}

		internal DigitalNumberElementViewModel(DigitalNumberElement digitalNumber) : base(digitalNumber)
		{
			Subject = digitalNumber
				.ObserveProperty(x => x.Subject)
				.Do(_ => Subject?.Value.Dispose())
				.Select(x => new SubjectViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			DaytimeImage = digitalNumber
				.ToReactivePropertyAsSynchronized(x => x.DaytimeImage)
				.AddTo(disposable);

			NighttimeImage = digitalNumber
				.ToReactivePropertyAsSynchronized(x => x.NighttimeImage)
				.AddTo(disposable);

			TransparentColor = digitalNumber
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

			Interval = digitalNumber
				.ToReactivePropertyAsSynchronized(x => x.Interval)
				.AddTo(disposable);
		}
	}
}
