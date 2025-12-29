using System.Globalization;
using Formats.OpenBve;
using OpenBveApi.Colors;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Panels;

namespace TrainEditor2.ViewModels.Panels
{
	internal class TimetableElementViewModel : PanelElementViewModel
	{
		internal ReactiveProperty<string> Width
		{
			get;
		}

		internal ReactiveProperty<string> Height
		{
			get;
		}

		internal ReactiveProperty<string> TransparentColor
		{
			get;
		}

		internal TimetableElementViewModel(TimetableElement timetable) : base(timetable)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			Width = timetable
				.ToReactivePropertyAsSynchronized(
					x => x.Width,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.Positive, out string message);
					return message;
				})
				.AddTo(disposable);

			Height = timetable
				.ToReactivePropertyAsSynchronized(
					x => x.Height,
					x => x.ToString(culture),
					x => x.Parse(),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryValidate(x, NumberRange.Positive, out string message);
					return message;
				})
				.AddTo(disposable);

			TransparentColor = timetable
				.ToReactivePropertyAsSynchronized(
					x => x.TransparentColor,
					x => x.ToString(),
					Color24.ParseHexColor,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					Utilities.TryParse(x, out Color24 _, out string message);
					return message;
				})
				.AddTo(disposable);
		}
	}
}
