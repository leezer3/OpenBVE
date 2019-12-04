using System.Globalization;
using OpenBveApi.Interface;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Panels;

namespace TrainEditor2.ViewModels.Panels
{
	internal class TouchElementViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> LocationX
		{
			get;
		}

		internal ReactiveProperty<string> LocationY
		{
			get;
		}

		internal ReactiveProperty<string> SizeX
		{
			get;
		}

		internal ReactiveProperty<string> SizeY
		{
			get;
		}

		internal ReactiveProperty<int> JumpScreen
		{
			get;
		}

		internal ReactiveProperty<int> SoundIndex
		{
			get;
		}

		internal ReactiveProperty<Translations.CommandInfo> CommandInfo
		{
			get;
		}

		internal ReactiveProperty<int> CommandOption
		{
			get;
		}

		internal TouchElementViewModel(TouchElement touch)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			LocationX = touch
				.ToReactivePropertyAsSynchronized(
					x => x.LocationX,
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

			LocationY = touch
				.ToReactivePropertyAsSynchronized(
					x => x.LocationY,
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

			SizeX = touch
				.ToReactivePropertyAsSynchronized(
					x => x.SizeX,
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

			SizeY = touch
				.ToReactivePropertyAsSynchronized(
					x => x.SizeY,
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

			JumpScreen = touch
				.ToReactivePropertyAsSynchronized(x => x.JumpScreen)
				.AddTo(disposable);

			SoundIndex = touch
				.ToReactivePropertyAsSynchronized(x => x.SoundIndex)
				.AddTo(disposable);

			CommandInfo = touch
				.ToReactivePropertyAsSynchronized(x => x.CommandInfo)
				.AddTo(disposable);

			CommandOption = touch
				.ToReactivePropertyAsSynchronized(x => x.CommandOption)
				.AddTo(disposable);
		}
	}
}
