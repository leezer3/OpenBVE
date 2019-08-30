using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Sounds;

namespace TrainEditor2.ViewModels.Sounds
{
	internal abstract class SoundElementViewModel : BaseViewModel
	{
		internal ReactiveProperty<object> Key
		{
			get;
		}

		internal ReactiveProperty<string> FilePath
		{
			get;
		}

		internal ReactiveProperty<bool> DefinedPosition
		{
			get;
		}

		internal ReactiveProperty<string> PositionX
		{
			get;
		}

		internal ReactiveProperty<string> PositionY
		{
			get;
		}

		internal ReactiveProperty<string> PositionZ
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

		internal SoundElementViewModel(SoundElement element)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			Key = element
				.ToReactivePropertyAsSynchronized(x => x.Key)
				.AddTo(disposable);

			FilePath = element
				.ToReactivePropertyAsSynchronized(x => x.FilePath)
				.AddTo(disposable);

			DefinedPosition = element
				.ToReactivePropertyAsSynchronized(x => x.DefinedPosition)
				.AddTo(disposable);

			PositionX = element
				.ToReactivePropertyAsSynchronized(
					x => x.PositionX,
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

			PositionY = element
				.ToReactivePropertyAsSynchronized(
					x => x.PositionY,
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

			PositionZ = element
				.ToReactivePropertyAsSynchronized(
					x => x.PositionZ,
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

			DefinedRadius = element
				.ToReactivePropertyAsSynchronized(x => x.DefinedRadius)
				.AddTo(disposable);

			Radius = element
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

					Utilities.TryParse(x, NumberRange.Any, out result, out message);

					return message;
				})
				.AddTo(disposable);
		}
	}

	internal abstract class SoundElementViewModel<T> : SoundElementViewModel
	{
		internal new ReactiveProperty<T> Key
		{
			get;
		}

		internal SoundElementViewModel(SoundElement<T> element, IEnumerable<SoundElement<T>> otherElements) : base(element)
		{
			Key = element
				.ToReactivePropertyAsSynchronized(x => x.Key, ignoreValidationErrorValue: true)
				.SetValidateNotifyError(x =>
				{
					string message = null;

					if (otherElements.Any(y => y.Key.Equals(x)))
					{
						message = "The specified key is already set.";
					}

					return message;
				})
				.AddTo(disposable);
		}
	}

	internal class RunElementViewModel : SoundElementViewModel<int>
	{
		internal RunElementViewModel(RunElement element, IEnumerable<RunElement> otherElements) : base(element, otherElements) { }
	}

	internal class FlangeElementViewModel : SoundElementViewModel<int>
	{
		internal FlangeElementViewModel(FlangeElement element, IEnumerable<FlangeElement> otherElements) : base(element, otherElements) { }
	}

	internal class MotorElementViewModel : SoundElementViewModel<int>
	{
		internal MotorElementViewModel(MotorElement element, IEnumerable<MotorElement> otherElements) : base(element, otherElements) { }
	}

	internal class FrontSwitchElementViewModel : SoundElementViewModel<int>
	{
		internal FrontSwitchElementViewModel(FrontSwitchElement element, IEnumerable<FrontSwitchElement> otherElements) : base(element, otherElements) { }
	}

	internal class RearSwitchElementViewModel : SoundElementViewModel<int>
	{
		internal RearSwitchElementViewModel(RearSwitchElement element, IEnumerable<RearSwitchElement> otherElements) : base(element, otherElements) { }
	}

	internal class BrakeElementViewModel : SoundElementViewModel<BrakeKey>
	{
		internal BrakeElementViewModel(BrakeElement element, IEnumerable<BrakeElement> otherElements) : base(element, otherElements) { }
	}

	internal class CompressorElementViewModel : SoundElementViewModel<CompressorKey>
	{
		internal CompressorElementViewModel(CompressorElement element, IEnumerable<CompressorElement> otherElements) : base(element, otherElements) { }
	}

	internal class SuspensionElementViewModel : SoundElementViewModel<SuspensionKey>
	{
		internal SuspensionElementViewModel(SuspensionElement element, IEnumerable<SuspensionElement> otherElements) : base(element, otherElements) { }
	}

	internal class PrimaryHornElementViewModel : SoundElementViewModel<HornKey>
	{
		internal PrimaryHornElementViewModel(PrimaryHornElement element, IEnumerable<PrimaryHornElement> otherElements) : base(element, otherElements) { }
	}

	internal class SecondaryHornElementViewModel : SoundElementViewModel<HornKey>
	{
		internal SecondaryHornElementViewModel(SecondaryHornElement element, IEnumerable<SecondaryHornElement> otherElements) : base(element, otherElements) { }
	}

	internal class MusicHornElementViewModel : SoundElementViewModel<HornKey>
	{
		internal MusicHornElementViewModel(MusicHornElement element, IEnumerable<MusicHornElement> otherElements) : base(element, otherElements) { }
	}

	internal class DoorElementViewModel : SoundElementViewModel<DoorKey>
	{
		internal DoorElementViewModel(DoorElement element, IEnumerable<DoorElement> otherElements) : base(element, otherElements) { }
	}

	internal class AtsElementViewModel : SoundElementViewModel<int>
	{
		internal AtsElementViewModel(AtsElement element, IEnumerable<AtsElement> otherElements) : base(element, otherElements) { }
	}

	internal class BuzzerElementViewModel : SoundElementViewModel<BuzzerKey>
	{
		internal BuzzerElementViewModel(BuzzerElement element, IEnumerable<BuzzerElement> otherElements) : base(element, otherElements) { }
	}

	internal class PilotLampElementViewModel : SoundElementViewModel<PilotLampKey>
	{
		internal PilotLampElementViewModel(PilotLampElement element, IEnumerable<PilotLampElement> otherElements) : base(element, otherElements) { }
	}

	internal class BrakeHandleElementViewModel : SoundElementViewModel<BrakeHandleKey>
	{
		internal BrakeHandleElementViewModel(BrakeHandleElement element, IEnumerable<BrakeHandleElement> otherElements) : base(element, otherElements) { }
	}

	internal class MasterControllerElementViewModel : SoundElementViewModel<MasterControllerKey>
	{
		internal MasterControllerElementViewModel(MasterControllerElement element, IEnumerable<MasterControllerElement> otherElements) : base(element, otherElements) { }
	}

	internal class ReverserElementViewModel : SoundElementViewModel<ReverserKey>
	{
		internal ReverserElementViewModel(ReverserElement element, IEnumerable<ReverserElement> otherElements) : base(element, otherElements) { }
	}

	internal class BreakerElementViewModel : SoundElementViewModel<BreakerKey>
	{
		internal BreakerElementViewModel(BreakerElement element, IEnumerable<BreakerElement> otherElements) : base(element, otherElements) { }
	}

	internal class RequestStopElementViewModel : SoundElementViewModel<RequestStopKey>
	{
		internal RequestStopElementViewModel(RequestStopElement element, IEnumerable<RequestStopElement> otherElements) : base(element, otherElements) { }
	}

	internal class TouchElementViewModel : SoundElementViewModel<int>
	{
		internal TouchElementViewModel(TouchElement element, IEnumerable<TouchElement> otherElements) : base(element, otherElements) { }
	}

	internal class OthersElementViewModel : SoundElementViewModel<OthersKey>
	{
		internal OthersElementViewModel(OthersElement element, IEnumerable<OthersElement> otherElements) : base(element, otherElements) { }
	}
}
