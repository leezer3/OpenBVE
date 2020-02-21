using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Sounds;
using TrainEditor2.ViewModels.Sounds;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToSound(SoundViewModel sound)
		{
			CompositeDisposable soundDisposable = new CompositeDisposable();
			CompositeDisposable elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

			WinFormsBinders.BindToTreeView(treeViewSound, sound.TreeItems, sound.SelectedTreeItem).AddTo(soundDisposable);

			sound.SelectedTreeItem
				.BindTo(
					listViewSound,
					x => x.Enabled,
					BindingMode.OneWay,
					x => sound.TreeItems[0].Children.Contains(x)
				)
				.AddTo(soundDisposable);

			WinFormsBinders.BindToListView(listViewSound, sound.ListColumns, sound.ListItems, sound.SelectedListItem).AddTo(soundDisposable);

			sound.SelectedListItem
				.BindTo(
					groupBoxSoundKey,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x != null
				)
				.AddTo(soundDisposable);

			sound.SelectedListItem
				.BindTo(
					groupBoxSoundValue,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x != null
				)
				.AddTo(soundDisposable);

			sound.SelectedRun
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedFlange
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedMotor
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedFrontSwitch
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedRearSwitch
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedBrake
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<BrakeElementViewModel, BrakeKey>(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedCompressor
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<CompressorElementViewModel, CompressorKey>(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedSuspension
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<SuspensionElementViewModel, SuspensionKey>(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedPrimaryHorn
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<PrimaryHornElementViewModel, HornKey>(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedSecondaryHorn
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<SecondaryHornElementViewModel, HornKey>(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedMusicHorn
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<MusicHornElementViewModel, HornKey>(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedDoor
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<DoorElementViewModel, DoorKey>(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedAts
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedBuzzer
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<BuzzerElementViewModel, BuzzerKey>(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedPilotLamp
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<PilotLampElementViewModel, PilotLampKey>(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedBrakeHandle
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<BrakeHandleElementViewModel, BrakeHandleKey>(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedMasterController
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<MasterControllerElementViewModel, MasterControllerKey>(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedReverser
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<ReverserElementViewModel, ReverserKey>(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedBreaker
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<BreakerElementViewModel, BreakerKey>(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedRequestStop
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<RequestStopElementViewModel, RequestStopKey>(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedTouch
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			sound.SelectedOthers
				.Where(x => x != null)
				.Subscribe(x =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<OthersElementViewModel, OthersKey>(x).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			new[]
				{
					sound.UpRun, sound.UpFlange, sound.UpMotor, sound.UpFrontSwitch, sound.UpRearSwitch,
					sound.UpBrake, sound.UpCompressor, sound.UpSuspension, sound.UpPrimaryHorn, sound.UpSecondaryHorn,
					sound.UpMusicHorn, sound.UpDoor, sound.UpAts, sound.UpBuzzer, sound.UpPilotLamp,
					sound.UpBrakeHandle, sound.UpMasterController, sound.UpReverser, sound.UpBreaker, sound.UpRequestStop,
					sound.UpTouch, sound.UpOthers
				}
				.BindToButton(buttonSoundUp)
				.AddTo(soundDisposable);

			new[]
				{
					sound.DownRun, sound.DownFlange, sound.DownMotor, sound.DownFrontSwitch, sound.DownRearSwitch,
					sound.DownBrake, sound.DownCompressor, sound.DownSuspension, sound.DownPrimaryHorn, sound.DownSecondaryHorn,
					sound.DownMusicHorn, sound.DownDoor, sound.DownAts, sound.DownBuzzer, sound.DownPilotLamp,
					sound.DownBrakeHandle, sound.DownMasterController, sound.DownReverser, sound.DownBreaker, sound.DownRequestStop,
					sound.DownTouch, sound.DownOthers
				}
				.BindToButton(buttonSoundDown)
				.AddTo(soundDisposable);

			new[]
				{
					sound.AddRun, sound.AddFlange, sound.AddMotor, sound.AddFrontSwitch, sound.AddRearSwitch,
					sound.AddBrake, sound.AddCompressor, sound.AddSuspension, sound.AddPrimaryHorn, sound.AddSecondaryHorn,
					sound.AddMusicHorn, sound.AddDoor, sound.AddAts, sound.AddBuzzer, sound.AddPilotLamp,
					sound.AddBrakeHandle, sound.AddMasterController, sound.AddReverser, sound.AddBreaker, sound.AddRequestStop,
					sound.AddTouch, sound.AddOthers
				}
				.BindToButton(buttonSoundAdd)
				.AddTo(soundDisposable);

			sound.RemoveSoundElement.BindToButton(buttonSoundRemove).AddTo(soundDisposable);

			return soundDisposable;
		}
	}
}
