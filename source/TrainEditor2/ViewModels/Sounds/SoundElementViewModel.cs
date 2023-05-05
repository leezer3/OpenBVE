using System;
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
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			PositionY = element
				.ToReactivePropertyAsSynchronized(
					x => x.PositionY,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			PositionZ = element
				.ToReactivePropertyAsSynchronized(
					x => x.PositionZ,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			DefinedRadius = element
				.ToReactivePropertyAsSynchronized(x => x.DefinedRadius)
				.AddTo(disposable);

			Radius = element
				.ToReactivePropertyAsSynchronized(
					x => x.Radius,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.AddTo(disposable);

			DefinedRadius.Subscribe(_ => Radius.ForceValidate()).AddTo(disposable);

			Radius.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedRadius.Value)
					{
						double result;
						Utilities.TryParse(x, NumberRange.Any, out result, out message);
					}
					else
					{
						message = string.Empty;
					}

					return message;
				})
				.AddTo(disposable);

			DefinedPosition.Subscribe(_ =>
				{
					PositionX.ForceValidate();
					PositionY.ForceValidate();
					PositionZ.ForceValidate();
				})
				.AddTo(disposable);

			PositionX.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedPosition.Value)
					{
						double result;
						Utilities.TryParse(x, NumberRange.Any, out result, out message);
					}
					else
					{
						message = string.Empty;
					}

					return message;
				})
				.AddTo(disposable);

			PositionY.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedPosition.Value)
					{
						double result;
						Utilities.TryParse(x, NumberRange.Any, out result, out message);
					}
					else
					{
						message = string.Empty;
					}

					return message;
				})
				.AddTo(disposable);

			PositionZ.SetValidateNotifyError(x =>
				{
					string message;

					if (DefinedPosition.Value)
					{
						double result;
						Utilities.TryParse(x, NumberRange.Any, out result, out message);
					}
					else
					{
						message = string.Empty;
					}

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

	internal class BrakeElementViewModel : SoundElementViewModel<SoundKey.Brake>
	{
		internal BrakeElementViewModel(BrakeElement element, IEnumerable<BrakeElement> otherElements) : base(element, otherElements) { }
	}

	internal class CompressorElementViewModel : SoundElementViewModel<SoundKey.Compressor>
	{
		internal CompressorElementViewModel(CompressorElement element, IEnumerable<CompressorElement> otherElements) : base(element, otherElements) { }
	}

	internal class SuspensionElementViewModel : SoundElementViewModel<SoundKey.Suspension>
	{
		internal SuspensionElementViewModel(SuspensionElement element, IEnumerable<SuspensionElement> otherElements) : base(element, otherElements) { }
	}

	internal class PrimaryHornElementViewModel : SoundElementViewModel<SoundKey.Horn>
	{
		internal PrimaryHornElementViewModel(PrimaryHornElement element, IEnumerable<PrimaryHornElement> otherElements) : base(element, otherElements) { }
	}

	internal class SecondaryHornElementViewModel : SoundElementViewModel<SoundKey.Horn>
	{
		internal SecondaryHornElementViewModel(SecondaryHornElement element, IEnumerable<SecondaryHornElement> otherElements) : base(element, otherElements) { }
	}

	internal class MusicHornElementViewModel : SoundElementViewModel<SoundKey.Horn>
	{
		internal MusicHornElementViewModel(MusicHornElement element, IEnumerable<MusicHornElement> otherElements) : base(element, otherElements) { }
	}

	internal class DoorElementViewModel : SoundElementViewModel<SoundKey.Door>
	{
		internal DoorElementViewModel(DoorElement element, IEnumerable<DoorElement> otherElements) : base(element, otherElements) { }
	}

	internal class AtsElementViewModel : SoundElementViewModel<int>
	{
		internal AtsElementViewModel(AtsElement element, IEnumerable<AtsElement> otherElements) : base(element, otherElements) { }
	}

	internal class BuzzerElementViewModel : SoundElementViewModel<SoundKey.Buzzer>
	{
		internal BuzzerElementViewModel(BuzzerElement element, IEnumerable<BuzzerElement> otherElements) : base(element, otherElements) { }
	}

	internal class PilotLampElementViewModel : SoundElementViewModel<SoundKey.PilotLamp>
	{
		internal PilotLampElementViewModel(PilotLampElement element, IEnumerable<PilotLampElement> otherElements) : base(element, otherElements) { }
	}

	internal class BrakeHandleElementViewModel : SoundElementViewModel<SoundKey.BrakeHandle>
	{
		internal BrakeHandleElementViewModel(BrakeHandleElement element, IEnumerable<BrakeHandleElement> otherElements) : base(element, otherElements) { }
	}

	internal class MasterControllerElementViewModel : SoundElementViewModel<SoundKey.MasterController>
	{
		internal MasterControllerElementViewModel(MasterControllerElement element, IEnumerable<MasterControllerElement> otherElements) : base(element, otherElements) { }
	}

	internal class ReverserElementViewModel : SoundElementViewModel<SoundKey.Reverser>
	{
		internal ReverserElementViewModel(ReverserElement element, IEnumerable<ReverserElement> otherElements) : base(element, otherElements) { }
	}

	internal class BreakerElementViewModel : SoundElementViewModel<SoundKey.Breaker>
	{
		internal BreakerElementViewModel(BreakerElement element, IEnumerable<BreakerElement> otherElements) : base(element, otherElements) { }
	}

	internal class RequestStopElementViewModel : SoundElementViewModel<SoundKey.RequestStop>
	{
		internal RequestStopElementViewModel(RequestStopElement element, IEnumerable<RequestStopElement> otherElements) : base(element, otherElements) { }
	}

	internal class TouchElementViewModel : SoundElementViewModel<int>
	{
		internal TouchElementViewModel(TouchElement element, IEnumerable<TouchElement> otherElements) : base(element, otherElements) { }
	}

	internal class OthersElementViewModel : SoundElementViewModel<SoundKey.Others>
	{
		internal OthersElementViewModel(OthersElement element, IEnumerable<OthersElement> otherElements) : base(element, otherElements) { }
	}
}
