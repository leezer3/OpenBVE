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
		internal ReactiveProperty<TreeViewItemViewModel> TreeItem
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

		internal ReactiveCommand RemoveRun
		{
			get;
		}

		internal ReactiveCommand RemoveFlange
		{
			get;
		}

		internal ReactiveCommand RemoveMotor
		{
			get;
		}

		internal ReactiveCommand RemoveFrontSwitch
		{
			get;
		}

		internal ReactiveCommand RemoveRearSwitch
		{
			get;
		}

		internal ReactiveCommand RemoveBrake
		{
			get;
		}

		internal ReactiveCommand RemoveCompressor
		{
			get;
		}

		internal ReactiveCommand RemoveSuspension
		{
			get;
		}

		internal ReactiveCommand RemovePrimaryHorn
		{
			get;
		}

		internal ReactiveCommand RemoveSecondaryHorn
		{
			get;
		}

		internal ReactiveCommand RemoveMusicHorn
		{
			get;
		}

		internal ReactiveCommand RemoveDoor
		{
			get;
		}

		internal ReactiveCommand RemoveAts
		{
			get;
		}

		internal ReactiveCommand RemoveBuzzer
		{
			get;
		}

		internal ReactiveCommand RemovePilotLamp
		{
			get;
		}

		internal ReactiveCommand RemoveBrakeHandle
		{
			get;
		}

		internal ReactiveCommand RemoveMasterController
		{
			get;
		}

		internal ReactiveCommand RemoveReverser
		{
			get;
		}

		internal ReactiveCommand RemoveBreaker
		{
			get;
		}

		internal ReactiveCommand RemoveRequestStop
		{
			get;
		}

		internal ReactiveCommand RemoveTouch
		{
			get;
		}

		internal ReactiveCommand RemoveOthers
		{
			get;
		}

		internal SoundViewModel(Sound sound)
		{
			CompositeDisposable treeItemDisposable = new CompositeDisposable();
			CompositeDisposable listItemDisposable = new CompositeDisposable();

			TrainManager.RunSounds = sound.SoundElements.OfType<RunElement>().ToList();
			TrainManager.MotorSounds = sound.SoundElements.OfType<MotorElement>().ToList();

			sound.SoundElements
				.ObserveAddChanged()
				.Subscribe(x =>
				{
					RunElement run = x as RunElement;
					MotorElement motor = x as MotorElement;

					if (run != null)
					{
						TrainManager.RunSounds.Add(run);
					}

					if (motor != null)
					{
						TrainManager.MotorSounds.Add(motor);
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
						TrainManager.RunSounds.Remove(run);
					}

					if (motor != null)
					{
						TrainManager.MotorSounds.Remove(motor);
					}
				})
				.AddTo(disposable);

			TreeItem = sound
				.ObserveProperty(x => x.TreeItem)
				.Do(_ => TreeItem?.Value.Dispose())
				.Select(x => new TreeViewItemViewModel(x))
				.ToReactiveProperty()
				.AddTo(disposable);

			TreeItem.Subscribe(x =>
				{
					treeItemDisposable.Dispose();
					treeItemDisposable = new CompositeDisposable();

					x.PropertyChangedAsObservable()
						.ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.None)
						.Subscribe(_ => TreeItem.ForceNotify())
						.AddTo(treeItemDisposable);
				})
				.AddTo(disposable);

			SelectedTreeItem = sound
				.ToReactivePropertyAsSynchronized(
					x => x.SelectedTreeItem,
					x => TreeItem.Value.SearchViewModel(x),
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

			AddRun = SelectedTreeItem
				.Select(x => x == TreeItem.Value.Children[0])
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<RunElement>)
				.AddTo(disposable);

			AddFlange = SelectedTreeItem
				.Select(x => x == TreeItem.Value.Children[1])
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<FlangeElement>)
				.AddTo(disposable);

			AddMotor = SelectedTreeItem
				.Select(x => x == TreeItem.Value.Children[2])
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<MotorElement>)
				.AddTo(disposable);

			AddFrontSwitch = SelectedTreeItem
				.Select(x => x == TreeItem.Value.Children[3])
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<FrontSwitchElement>)
				.AddTo(disposable);

			AddRearSwitch = SelectedTreeItem
				.Select(x => x == TreeItem.Value.Children[4])
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<RearSwitchElement>)
				.AddTo(disposable);

			AddBrake = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItem.Value.Children[5]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(BrakeKey))
							.OfType<BrakeKey>()
							.Except(sound.SoundElements.OfType<BrakeElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<BrakeElement, BrakeKey>)
				.AddTo(disposable);

			AddCompressor = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItem.Value.Children[6]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(CompressorKey))
							.OfType<CompressorKey>()
							.Except(sound.SoundElements.OfType<CompressorElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<CompressorElement, CompressorKey>)
				.AddTo(disposable);

			AddSuspension = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItem.Value.Children[7]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(SuspensionKey))
							.OfType<SuspensionKey>()
							.Except(sound.SoundElements.OfType<SuspensionElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<SuspensionElement, SuspensionKey>)
				.AddTo(disposable);

			AddPrimaryHorn = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItem.Value.Children[8]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(HornKey))
							.OfType<HornKey>()
							.Except(sound.SoundElements.OfType<PrimaryHornElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<PrimaryHornElement, HornKey>)
				.AddTo(disposable);

			AddSecondaryHorn = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItem.Value.Children[9]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(HornKey))
							.OfType<HornKey>()
							.Except(sound.SoundElements.OfType<SecondaryHornElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<SecondaryHornElement, HornKey>)
				.AddTo(disposable);

			AddMusicHorn = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItem.Value.Children[10]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(HornKey))
							.OfType<HornKey>()
							.Except(sound.SoundElements.OfType<MusicHornElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<MusicHornElement, HornKey>)
				.AddTo(disposable);

			AddDoor = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItem.Value.Children[11]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(DoorKey))
							.OfType<DoorKey>()
							.Except(sound.SoundElements.OfType<DoorElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<DoorElement, DoorKey>)
				.AddTo(disposable);

			AddAts = SelectedTreeItem
				.Select(x => x == TreeItem.Value.Children[12])
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<AtsElement>)
				.AddTo(disposable);

			AddBuzzer = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItem.Value.Children[13]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(BuzzerKey))
							.OfType<BuzzerKey>()
							.Except(sound.SoundElements.OfType<BuzzerElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<BuzzerElement, BuzzerKey>)
				.AddTo(disposable);

			AddPilotLamp = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItem.Value.Children[14]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(PilotLampKey))
							.OfType<PilotLampKey>()
							.Except(sound.SoundElements.OfType<PilotLampElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<PilotLampElement, PilotLampKey>)
				.AddTo(disposable);

			AddBrakeHandle = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItem.Value.Children[15]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(BrakeHandleKey))
							.OfType<BrakeHandleKey>()
							.Except(sound.SoundElements.OfType<BrakeHandleElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<BrakeHandleElement, BrakeHandleKey>)
				.AddTo(disposable);

			AddMasterController = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItem.Value.Children[16]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(MasterControllerKey))
							.OfType<MasterControllerKey>()
							.Except(sound.SoundElements.OfType<MasterControllerElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<MasterControllerElement, MasterControllerKey>)
				.AddTo(disposable);

			AddReverser = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItem.Value.Children[17]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(ReverserKey))
							.OfType<ReverserKey>()
							.Except(sound.SoundElements.OfType<ReverserElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<ReverserElement, ReverserKey>)
				.AddTo(disposable);

			AddBreaker = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItem.Value.Children[18]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(BreakerKey))
							.OfType<BreakerKey>()
							.Except(sound.SoundElements.OfType<BreakerElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<BreakerElement, BreakerKey>)
				.AddTo(disposable);

			AddRequestStop = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItem.Value.Children[19]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(RequestStopKey))
							.OfType<RequestStopKey>()
							.Except(sound.SoundElements.OfType<RequestStopElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<RequestStopElement, RequestStopKey>)
				.AddTo(disposable);

			AddTouch = SelectedTreeItem
				.Select(x => x == TreeItem.Value.Children[20])
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<TouchElement>)
				.AddTo(disposable);

			AddOthers = new[]
				{
					SelectedTreeItem.Select(x => x == TreeItem.Value.Children[21]),
					sound.SoundElements
						.CollectionChangedAsObservable()
						.ToReadOnlyReactivePropertySlim()
						.Select(_ => Enum.GetValues(typeof(OthersKey))
							.OfType<OthersKey>()
							.Except(sound.SoundElements.OfType<OthersElement>().Select(y => y.Key))
							.Any()
						)
				}
				.CombineLatestValuesAreAllTrue()
				.ToReactiveCommand()
				.WithSubscribe(sound.AddElement<OthersElement, OthersKey>)
				.AddTo(disposable);

			RemoveRun = SelectedRun
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<RunElement>)
				.AddTo(disposable);

			RemoveFlange = SelectedFlange
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<FlangeElement>)
				.AddTo(disposable);

			RemoveMotor = SelectedMotor
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<MotorElement>)
				.AddTo(disposable);

			RemoveFrontSwitch = SelectedFrontSwitch
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<FrontSwitchElement>)
				.AddTo(disposable);

			RemoveRearSwitch = SelectedRearSwitch
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<RearSwitchElement>)
				.AddTo(disposable);

			RemoveBrake = SelectedBrake
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<BrakeElement>)
				.AddTo(disposable);

			RemoveCompressor = SelectedCompressor
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<CompressorElement>)
				.AddTo(disposable);

			RemoveSuspension = SelectedSuspension
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<SuspensionElement>)
				.AddTo(disposable);

			RemovePrimaryHorn = SelectedPrimaryHorn
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<PrimaryHornElement>)
				.AddTo(disposable);

			RemoveSecondaryHorn = SelectedSecondaryHorn
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<SecondaryHornElement>)
				.AddTo(disposable);

			RemoveMusicHorn = SelectedMusicHorn
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<MusicHornElement>)
				.AddTo(disposable);

			RemoveDoor = SelectedDoor
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<DoorElement>)
				.AddTo(disposable);

			RemoveAts = SelectedAts
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<AtsElement>)
				.AddTo(disposable);

			RemoveBuzzer = SelectedBuzzer
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<BuzzerElement>)
				.AddTo(disposable);

			RemovePilotLamp = SelectedPilotLamp
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<PilotLampElement>)
				.AddTo(disposable);

			RemoveBrakeHandle = SelectedBrakeHandle
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<BrakeHandleElement>)
				.AddTo(disposable);

			RemoveMasterController = SelectedMasterController
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<MasterControllerElement>)
				.AddTo(disposable);

			RemoveReverser = SelectedReverser
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<ReverserElement>)
				.AddTo(disposable);

			RemoveBreaker = SelectedBreaker
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<BreakerElement>)
				.AddTo(disposable);

			RemoveRequestStop = SelectedRequestStop
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<RequestStopElement>)
				.AddTo(disposable);

			RemoveTouch = SelectedTouch
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<TouchElement>)
				.AddTo(disposable);

			RemoveOthers = SelectedOthers
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(sound.RemoveElement<OthersElement>)
				.AddTo(disposable);

			treeItemDisposable.AddTo(disposable);
			listItemDisposable.AddTo(disposable);
		}
	}
}
