using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Sounds;
using TrainEditor2.Simulation.TrainManager;
using TrainEditor2.ViewModels.Others;

namespace TrainEditor2.ViewModels.Sounds
{
	internal class SoundViewModel : BaseViewModel
	{
		internal ReadOnlyReactiveCollection<TreeViewItemViewModel> TreeItems
		{
			get;
		}

		internal ReactiveProperty<TreeViewItemViewModel> SelectedTreeItem
		{
			get;
		}

		internal ReadOnlyReactiveCollection<ListViewColumnHeaderViewModel> ListColumns
		{
			get;
		}

		internal ReadOnlyReactiveCollection<ListViewItemViewModel> ListItems
		{
			get;
		}

		internal ReactiveProperty<ListViewItemViewModel> SelectedListItem
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<RunElementViewModel> SelectedRun
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<FlangeElementViewModel> SelectedFlange
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<MotorElementViewModel> SelectedMotor
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<FrontSwitchElementViewModel> SelectedFrontSwitch
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<RearSwitchElementViewModel> SelectedRearSwitch
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<BrakeElementViewModel> SelectedBrake
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<CompressorElementViewModel> SelectedCompressor
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<SuspensionElementViewModel> SelectedSuspension
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<PrimaryHornElementViewModel> SelectedPrimaryHorn
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<SecondaryHornElementViewModel> SelectedSecondaryHorn
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<MusicHornElementViewModel> SelectedMusicHorn
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<DoorElementViewModel> SelectedDoor
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<AtsElementViewModel> SelectedAts
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<BuzzerElementViewModel> SelectedBuzzer
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<PilotLampElementViewModel> SelectedPilotLamp
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<BrakeHandleElementViewModel> SelectedBrakeHandle
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<MasterControllerElementViewModel> SelectedMasterController
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<ReverserElementViewModel> SelectedReverser
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<BreakerElementViewModel> SelectedBreaker
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<RequestStopElementViewModel> SelectedRequestStop
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<TouchElementViewModel> SelectedTouch
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<OthersElementViewModel> SelectedOthers
		{
			get;
		}

		internal ReactiveCommand UpRun
		{
			get;
		}

		internal ReactiveCommand UpFlange
		{
			get;
		}

		internal ReactiveCommand UpMotor
		{
			get;
		}

		internal ReactiveCommand UpFrontSwitch
		{
			get;
		}

		internal ReactiveCommand UpRearSwitch
		{
			get;
		}

		internal ReactiveCommand UpBrake
		{
			get;
		}

		internal ReactiveCommand UpCompressor
		{
			get;
		}

		internal ReactiveCommand UpSuspension
		{
			get;
		}

		internal ReactiveCommand UpPrimaryHorn
		{
			get;
		}

		internal ReactiveCommand UpSecondaryHorn
		{
			get;
		}

		internal ReactiveCommand UpMusicHorn
		{
			get;
		}

		internal ReactiveCommand UpDoor
		{
			get;
		}

		internal ReactiveCommand UpAts
		{
			get;
		}

		internal ReactiveCommand UpBuzzer
		{
			get;
		}

		internal ReactiveCommand UpPilotLamp
		{
			get;
		}

		internal ReactiveCommand UpBrakeHandle
		{
			get;
		}

		internal ReactiveCommand UpMasterController
		{
			get;
		}

		internal ReactiveCommand UpReverser
		{
			get;
		}

		internal ReactiveCommand UpBreaker
		{
			get;
		}

		internal ReactiveCommand UpRequestStop
		{
			get;
		}

		internal ReactiveCommand UpTouch
		{
			get;
		}

		internal ReactiveCommand UpOthers
		{
			get;
		}

		internal ReactiveCommand DownRun
		{
			get;
		}

		internal ReactiveCommand DownFlange
		{
			get;
		}

		internal ReactiveCommand DownMotor
		{
			get;
		}

		internal ReactiveCommand DownFrontSwitch
		{
			get;
		}

		internal ReactiveCommand DownRearSwitch
		{
			get;
		}

		internal ReactiveCommand DownBrake
		{
			get;
		}

		internal ReactiveCommand DownCompressor
		{
			get;
		}

		internal ReactiveCommand DownSuspension
		{
			get;
		}

		internal ReactiveCommand DownPrimaryHorn
		{
			get;
		}

		internal ReactiveCommand DownSecondaryHorn
		{
			get;
		}

		internal ReactiveCommand DownMusicHorn
		{
			get;
		}

		internal ReactiveCommand DownDoor
		{
			get;
		}

		internal ReactiveCommand DownAts
		{
			get;
		}

		internal ReactiveCommand DownBuzzer
		{
			get;
		}

		internal ReactiveCommand DownPilotLamp
		{
			get;
		}

		internal ReactiveCommand DownBrakeHandle
		{
			get;
		}

		internal ReactiveCommand DownMasterController
		{
			get;
		}

		internal ReactiveCommand DownReverser
		{
			get;
		}

		internal ReactiveCommand DownBreaker
		{
			get;
		}

		internal ReactiveCommand DownRequestStop
		{
			get;
		}

		internal ReactiveCommand DownTouch
		{
			get;
		}

		internal ReactiveCommand DownOthers
		{
			get;
		}

		internal ReactiveCommand AddRun
		{
			get;
		}

		internal ReactiveCommand AddFlange
		{
			get;
		}

		internal ReactiveCommand AddMotor
		{
			get;
		}

		internal ReactiveCommand AddFrontSwitch
		{
			get;
		}

		internal ReactiveCommand AddRearSwitch
		{
			get;
		}

		internal ReactiveCommand AddBrake
		{
			get;
		}

		internal ReactiveCommand AddCompressor
		{
			get;
		}

		internal ReactiveCommand AddSuspension
		{
			get;
		}

		internal ReactiveCommand AddPrimaryHorn
		{
			get;
		}

		internal ReactiveCommand AddSecondaryHorn
		{
			get;
		}

		internal ReactiveCommand AddMusicHorn
		{
			get;
		}

		internal ReactiveCommand AddDoor
		{
			get;
		}

		internal ReactiveCommand AddAts
		{
			get;
		}

		internal ReactiveCommand AddBuzzer
		{
			get;
		}

		internal ReactiveCommand AddPilotLamp
		{
			get;
		}

		internal ReactiveCommand AddBrakeHandle
		{
			get;
		}

		internal ReactiveCommand AddMasterController
		{
			get;
		}

		internal ReactiveCommand AddReverser
		{
			get;
		}

		internal ReactiveCommand AddBreaker
		{
			get;
		}

		internal ReactiveCommand AddRequestStop
		{
			get;
		}

		internal ReactiveCommand AddTouch
		{
			get;
		}

		internal ReactiveCommand AddOthers
		{
			get;
		}

		internal ReactiveCommand RemoveSoundElement
		{
			get;
		}

		internal SoundViewModel(Sound sound)
		{
			CompositeDisposable listItemDisposable = new CompositeDisposable().AddTo(disposable);

			TrainEditor.RunSounds = sound.SoundElements.OfType<RunElement>().ToList();
			TrainEditor.MotorSounds = sound.SoundElements.OfType<MotorElement>().ToList();

			sound.SoundElements
				.ObserveAddChanged()
				.Subscribe(x =>
				{
					RunElement run = x as RunElement;
					MotorElement motor = x as MotorElement;

					if (run != null)
					{
						TrainEditor.RunSounds.Add(run);
					}

					if (motor != null)
					{
						TrainEditor.MotorSounds.Add(motor);
					}
				})
				.AddTo(disposable);

			sound.SoundElements
				.ObserveRemoveChanged()
				.Subscribe(x =>
				{
					RunElement run = x as RunElement;
					MotorElement motor = x as MotorElement;

					if (run != null)
					{
						TrainEditor.RunSounds.Remove(run);
					}

					if (motor != null)
					{
						TrainEditor.MotorSounds.Remove(motor);
					}
				})
				.AddTo(disposable);

			TreeItems = sound.TreeItems.ToReadOnlyReactiveCollection(x => new TreeViewItemViewModel(x, null)).AddTo(disposable);

			SelectedTreeItem = sound
				.ToReactivePropertyAsSynchronized(
					x => x.SelectedTreeItem,
					x => TreeItems.Select(y => y.SearchViewModel(x)).FirstOrDefault(y => y != null),
					x => x?.Model
				)
				.AddTo(disposable);

			ListColumns = sound.ListColumns
				.ToReadOnlyReactiveCollection(x => new ListViewColumnHeaderViewModel(x))
				.AddTo(disposable);

			ListItems = sound.ListItems
				.ToReadOnlyReactiveCollection(x => new ListViewItemViewModel(x))
				.AddTo(disposable);

			SelectedListItem = sound
				.ToReactivePropertyAsSynchronized(
					x => x.SelectedListItem,
					x => ListItems.FirstOrDefault(y => y.Model == x),
					x => x?.Model
				)
				.AddTo(disposable);

			SelectedTreeItem
				.Subscribe(_ =>
				{
					SelectedListItem.Value = null;
					sound.CreateListColumns();
					sound.CreateListItems();
				})
				.AddTo(disposable);

			SelectedListItem
				.Where(x => x != null)
				.Subscribe(x =>
				{
					listItemDisposable.Dispose();
					listItemDisposable = new CompositeDisposable();

					CompositeDisposable tagDisposable = new CompositeDisposable();

					x.Tag
						.OfType<INotifyPropertyChanged>()
						.Subscribe(y =>
						{
							tagDisposable.Dispose();
							tagDisposable = new CompositeDisposable();

							y.PropertyChangedAsObservable()
								.Subscribe(_ => sound.UpdateListItem(x.Model))
								.AddTo(tagDisposable);
						})
						.AddTo(listItemDisposable);

					tagDisposable.AddTo(listItemDisposable);
				})
				.AddTo(disposable);

			SelectedRun = SelectedListItem
				.Select(x => x?.Tag.Value as RunElement)
				.Do(_ => SelectedRun?.Value?.Dispose())
				.Select(x => x != null ? new RunElementViewModel(x, sound.SoundElements.OfType<RunElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedFlange = SelectedListItem
				.Select(x => x?.Tag.Value as FlangeElement)
				.Do(_ => SelectedFlange?.Value?.Dispose())
				.Select(x => x != null ? new FlangeElementViewModel(x, sound.SoundElements.OfType<FlangeElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedMotor = SelectedListItem
				.Select(x => x?.Tag.Value as MotorElement)
				.Do(_ => SelectedMotor?.Value?.Dispose())
				.Select(x => x != null ? new MotorElementViewModel(x, sound.SoundElements.OfType<MotorElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedFrontSwitch = SelectedListItem
				.Select(x => x?.Tag.Value as FrontSwitchElement)
				.Do(_ => SelectedFrontSwitch?.Value?.Dispose())
				.Select(x => x != null ? new FrontSwitchElementViewModel(x, sound.SoundElements.OfType<FrontSwitchElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedRearSwitch = SelectedListItem
				.Select(x => x?.Tag.Value as RearSwitchElement)
				.Do(_ => SelectedRearSwitch?.Value?.Dispose())
				.Select(x => x != null ? new RearSwitchElementViewModel(x, sound.SoundElements.OfType<RearSwitchElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedBrake = SelectedListItem
				.Select(x => x?.Tag.Value as BrakeElement)
				.Do(_ => SelectedBrake?.Value?.Dispose())
				.Select(x => x != null ? new BrakeElementViewModel(x, sound.SoundElements.OfType<BrakeElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedCompressor = SelectedListItem
				.Select(x => x?.Tag.Value as CompressorElement)
				.Do(_ => SelectedCompressor?.Value?.Dispose())
				.Select(x => x != null ? new CompressorElementViewModel(x, sound.SoundElements.OfType<CompressorElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedSuspension = SelectedListItem
				.Select(x => x?.Tag.Value as SuspensionElement)
				.Do(_ => SelectedSuspension?.Value?.Dispose())
				.Select(x => x != null ? new SuspensionElementViewModel(x, sound.SoundElements.OfType<SuspensionElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedPrimaryHorn = SelectedListItem
				.Select(x => x?.Tag.Value as PrimaryHornElement)
				.Do(_ => SelectedPrimaryHorn?.Value?.Dispose())
				.Select(x => x != null ? new PrimaryHornElementViewModel(x, sound.SoundElements.OfType<PrimaryHornElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedSecondaryHorn = SelectedListItem
				.Select(x => x?.Tag.Value as SecondaryHornElement)
				.Do(_ => SelectedSecondaryHorn?.Value?.Dispose())
				.Select(x => x != null ? new SecondaryHornElementViewModel(x, sound.SoundElements.OfType<SecondaryHornElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedMusicHorn = SelectedListItem
				.Select(x => x?.Tag.Value as MusicHornElement)
				.Do(_ => SelectedMusicHorn?.Value?.Dispose())
				.Select(x => x != null ? new MusicHornElementViewModel(x, sound.SoundElements.OfType<MusicHornElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedDoor = SelectedListItem
				.Select(x => x?.Tag.Value as DoorElement)
				.Do(_ => SelectedDoor?.Value?.Dispose())
				.Select(x => x != null ? new DoorElementViewModel(x, sound.SoundElements.OfType<DoorElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedAts = SelectedListItem
				.Select(x => x?.Tag.Value as AtsElement)
				.Do(_ => SelectedAts?.Value?.Dispose())
				.Select(x => x != null ? new AtsElementViewModel(x, sound.SoundElements.OfType<AtsElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedBuzzer = SelectedListItem
				.Select(x => x?.Tag.Value as BuzzerElement)
				.Do(_ => SelectedBuzzer?.Value?.Dispose())
				.Select(x => x != null ? new BuzzerElementViewModel(x, sound.SoundElements.OfType<BuzzerElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedPilotLamp = SelectedListItem
				.Select(x => x?.Tag.Value as PilotLampElement)
				.Do(_ => SelectedPilotLamp?.Value?.Dispose())
				.Select(x => x != null ? new PilotLampElementViewModel(x, sound.SoundElements.OfType<PilotLampElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedBrakeHandle = SelectedListItem
				.Select(x => x?.Tag.Value as BrakeHandleElement)
				.Do(_ => SelectedBrakeHandle?.Value?.Dispose())
				.Select(x => x != null ? new BrakeHandleElementViewModel(x, sound.SoundElements.OfType<BrakeHandleElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedMasterController = SelectedListItem
				.Select(x => x?.Tag.Value as MasterControllerElement)
				.Do(_ => SelectedMasterController?.Value?.Dispose())
				.Select(x => x != null ? new MasterControllerElementViewModel(x, sound.SoundElements.OfType<MasterControllerElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedReverser = SelectedListItem
				.Select(x => x?.Tag.Value as ReverserElement)
				.Do(_ => SelectedReverser?.Value?.Dispose())
				.Select(x => x != null ? new ReverserElementViewModel(x, sound.SoundElements.OfType<ReverserElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedBreaker = SelectedListItem
				.Select(x => x?.Tag.Value as BreakerElement)
				.Do(_ => SelectedBreaker?.Value?.Dispose())
				.Select(x => x != null ? new BreakerElementViewModel(x, sound.SoundElements.OfType<BreakerElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedRequestStop = SelectedListItem
				.Select(x => x?.Tag.Value as RequestStopElement)
				.Do(_ => SelectedAts?.Value?.Dispose())
				.Select(x => x != null ? new RequestStopElementViewModel(x, sound.SoundElements.OfType<RequestStopElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedTouch = SelectedListItem
				.Select(x => x?.Tag.Value as TouchElement)
				.Do(_ => SelectedTouch?.Value?.Dispose())
				.Select(x => x != null ? new TouchElementViewModel(x, sound.SoundElements.OfType<TouchElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedOthers = SelectedListItem
				.Select(x => x?.Tag.Value as OthersElement)
				.Do(_ => SelectedOthers?.Value?.Dispose())
				.Select(x => x != null ? new OthersElementViewModel(x, sound.SoundElements.OfType<OthersElement>().Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			UpRun = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is RunElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<RunElement>)
				.AddTo(disposable);

			UpFlange = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is FlangeElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<FlangeElement>)
				.AddTo(disposable);

			UpMotor = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is MotorElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<MotorElement>)
				.AddTo(disposable);

			UpFrontSwitch = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is FrontSwitchElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<FrontSwitchElement>)
				.AddTo(disposable);

			UpRearSwitch = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is RearSwitchElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<RearSwitchElement>)
				.AddTo(disposable);

			UpBrake = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is BrakeElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<BrakeElement>)
				.AddTo(disposable);

			UpCompressor = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is CompressorElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<CompressorElement>)
				.AddTo(disposable);

			UpSuspension = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is SuspensionElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<SuspensionElement>)
				.AddTo(disposable);

			UpPrimaryHorn = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is PrimaryHornElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<PrimaryHornElement>)
				.AddTo(disposable);

			UpSecondaryHorn = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is SecondaryHornElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<SecondaryHornElement>)
				.AddTo(disposable);

			UpMusicHorn = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is MusicHornElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<MusicHornElement>)
				.AddTo(disposable);

			UpDoor = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is DoorElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<DoorElement>)
				.AddTo(disposable);

			UpAts = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is AtsElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<AtsElement>)
				.AddTo(disposable);

			UpBuzzer = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is BuzzerElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<BuzzerElement>)
				.AddTo(disposable);

			UpPilotLamp = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is PilotLampElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<PilotLampElement>)
				.AddTo(disposable);

			UpBrakeHandle = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is BrakeHandleElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<BrakeHandleElement>)
				.AddTo(disposable);

			UpMasterController = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is MasterControllerElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<MasterControllerElement>)
				.AddTo(disposable);

			UpReverser = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is ReverserElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<ReverserElement>)
				.AddTo(disposable);

			UpBreaker = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is BreakerElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<BreakerElement>)
				.AddTo(disposable);

			UpRequestStop = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is RequestStopElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<RequestStopElement>)
				.AddTo(disposable);

			UpTouch = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is TouchElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<TouchElement>)
				.AddTo(disposable);

			UpOthers = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is OthersElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.UpElement<OthersElement>)
				.AddTo(disposable);

			DownRun = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is RunElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<RunElement>)
				.AddTo(disposable);

			DownFlange = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is FlangeElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<FlangeElement>)
				.AddTo(disposable);

			DownMotor = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is MotorElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<MotorElement>)
				.AddTo(disposable);

			DownFrontSwitch = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is FrontSwitchElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<FrontSwitchElement>)
				.AddTo(disposable);

			DownRearSwitch = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is RearSwitchElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<RearSwitchElement>)
				.AddTo(disposable);

			DownBrake = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is BrakeElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<BrakeElement>)
				.AddTo(disposable);

			DownCompressor = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is CompressorElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<CompressorElement>)
				.AddTo(disposable);

			DownSuspension = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is SuspensionElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<SuspensionElement>)
				.AddTo(disposable);

			DownPrimaryHorn = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is PrimaryHornElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<PrimaryHornElement>)
				.AddTo(disposable);

			DownSecondaryHorn = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is SecondaryHornElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<SecondaryHornElement>)
				.AddTo(disposable);

			DownMusicHorn = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is MusicHornElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<MusicHornElement>)
				.AddTo(disposable);

			DownDoor = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is DoorElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<DoorElement>)
				.AddTo(disposable);

			DownAts = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is AtsElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<AtsElement>)
				.AddTo(disposable);

			DownBuzzer = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is BuzzerElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<BuzzerElement>)
				.AddTo(disposable);

			DownPilotLamp = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is PilotLampElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<PilotLampElement>)
				.AddTo(disposable);

			DownBrakeHandle = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is BrakeHandleElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<BrakeHandleElement>)
				.AddTo(disposable);

			DownMasterController = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is MasterControllerElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<MasterControllerElement>)
				.AddTo(disposable);

			DownReverser = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is ReverserElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<ReverserElement>)
				.AddTo(disposable);

			DownBreaker = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is BreakerElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<BreakerElement>)
				.AddTo(disposable);

			DownRequestStop = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is RequestStopElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<RequestStopElement>)
				.AddTo(disposable);

			DownTouch = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is TouchElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<TouchElement>)
				.AddTo(disposable);

			DownOthers = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is OthersElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.DownElement<OthersElement>)
				.AddTo(disposable);

			AddRun = SelectedTreeItem
				.Select(x => x == TreeItems[0].Children[0])
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<RunElement>)
				.AddTo(disposable);

			AddFlange = SelectedTreeItem
				.Select(x => x == TreeItems[0].Children[1])
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<FlangeElement>)
				.AddTo(disposable);

			AddMotor = SelectedTreeItem
				.Select(x => x == TreeItems[0].Children[2])
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<MotorElement>)
				.AddTo(disposable);

			AddFrontSwitch = SelectedTreeItem
				.Select(x => x == TreeItems[0].Children[3])
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<FrontSwitchElement>)
				.AddTo(disposable);

			AddRearSwitch = SelectedTreeItem
				.Select(x => x == TreeItems[0].Children[4])
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<RearSwitchElement>)
				.AddTo(disposable);

			AddBrake = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItems[0].Children[5]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SoundKey.Brake))
							.OfType<SoundKey.Brake>()
							.Except(sound.SoundElements.OfType<BrakeElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<BrakeElement, SoundKey.Brake>)
				.AddTo(disposable);

			AddCompressor = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItems[0].Children[6]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SoundKey.Compressor))
							.OfType<SoundKey.Compressor>()
							.Except(sound.SoundElements.OfType<CompressorElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<CompressorElement, SoundKey.Compressor>)
				.AddTo(disposable);

			AddSuspension = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItems[0].Children[7]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SoundKey.Suspension))
							.OfType<SoundKey.Suspension>()
							.Except(sound.SoundElements.OfType<SuspensionElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<SuspensionElement, SoundKey.Suspension>)
				.AddTo(disposable);

			AddPrimaryHorn = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItems[0].Children[8]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SoundKey.Horn))
							.OfType<SoundKey.Horn>()
							.Except(sound.SoundElements.OfType<PrimaryHornElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<PrimaryHornElement, SoundKey.Horn>)
				.AddTo(disposable);

			AddSecondaryHorn = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItems[0].Children[9]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SoundKey.Horn))
							.OfType<SoundKey.Horn>()
							.Except(sound.SoundElements.OfType<SecondaryHornElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<SecondaryHornElement, SoundKey.Horn>)
				.AddTo(disposable);

			AddMusicHorn = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItems[0].Children[10]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SoundKey.Horn))
							.OfType<SoundKey.Horn>()
							.Except(sound.SoundElements.OfType<MusicHornElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<MusicHornElement, SoundKey.Horn>)
				.AddTo(disposable);

			AddDoor = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItems[0].Children[11]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SoundKey.Door))
							.OfType<SoundKey.Door>()
							.Except(sound.SoundElements.OfType<DoorElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<DoorElement, SoundKey.Door>)
				.AddTo(disposable);

			AddAts = SelectedTreeItem
				.Select(x => x == TreeItems[0].Children[12])
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<AtsElement>)
				.AddTo(disposable);

			AddBuzzer = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItems[0].Children[13]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SoundKey.Buzzer))
							.OfType<SoundKey.Buzzer>()
							.Except(sound.SoundElements.OfType<BuzzerElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<BuzzerElement, SoundKey.Buzzer>)
				.AddTo(disposable);

			AddPilotLamp = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItems[0].Children[14]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SoundKey.PilotLamp))
							.OfType<SoundKey.PilotLamp>()
							.Except(sound.SoundElements.OfType<PilotLampElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<PilotLampElement, SoundKey.PilotLamp>)
				.AddTo(disposable);

			AddBrakeHandle = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItems[0].Children[15]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SoundKey.BrakeHandle))
							.OfType<SoundKey.BrakeHandle>()
							.Except(sound.SoundElements.OfType<BrakeHandleElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<BrakeHandleElement, SoundKey.BrakeHandle>)
				.AddTo(disposable);

			AddMasterController = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItems[0].Children[16]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SoundKey.MasterController))
							.OfType<SoundKey.MasterController>()
							.Except(sound.SoundElements.OfType<MasterControllerElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<MasterControllerElement, SoundKey.MasterController>)
				.AddTo(disposable);

			AddReverser = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItems[0].Children[17]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SoundKey.Reverser))
							.OfType<SoundKey.Reverser>()
							.Except(sound.SoundElements.OfType<ReverserElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<ReverserElement, SoundKey.Reverser>)
				.AddTo(disposable);

			AddBreaker = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItems[0].Children[18]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SoundKey.Breaker))
							.OfType<SoundKey.Breaker>()
							.Except(sound.SoundElements.OfType<BreakerElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<BreakerElement, SoundKey.Breaker>)
				.AddTo(disposable);

			AddRequestStop = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItems[0].Children[19]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SoundKey.RequestStop))
							.OfType<SoundKey.RequestStop>()
							.Except(sound.SoundElements.OfType<RequestStopElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<RequestStopElement, SoundKey.RequestStop>)
				.AddTo(disposable);

			AddTouch = SelectedTreeItem
				.Select(x => x == TreeItems[0].Children[20])
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<TouchElement>)
				.AddTo(disposable);

			AddOthers = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItems[0].Children[21]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SoundKey.Others))
							.OfType<SoundKey.Others>()
							.Except(sound.SoundElements.OfType<OthersElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<OthersElement, SoundKey.Others>)
				.AddTo(disposable);

			RemoveSoundElement = SelectedListItem
				.Select(x => x?.Tag.Value is SoundElement)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement)
				.AddTo(disposable);
		}
	}
}
