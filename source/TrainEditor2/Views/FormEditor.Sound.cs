using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Sounds;
using TrainEditor2.ViewModels.Others;
using TrainEditor2.ViewModels.Sounds;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToSound(SoundViewModel x)
		{
			CompositeDisposable soundDisposable = new CompositeDisposable();
			CompositeDisposable listItemDisposable = new CompositeDisposable().AddTo(soundDisposable);
			CompositeDisposable elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

			listViewSound.Items.Clear();

			x.TreeItem
				.BindTo(
					this,
					y => y.TreeViewSoundTopNode,
					BindingMode.OneWay,
					TreeViewItemViewModelToTreeNode
				)
				.AddTo(soundDisposable);

			x.SelectedTreeItem
				.BindTo(
					treeViewSound,
					y => y.SelectedNode,
					BindingMode.TwoWay,
					y => treeViewSound.Nodes.OfType<TreeNode>().Select(z => SearchTreeNode(y, z)).FirstOrDefault(z => z != null),
					y => (TreeViewItemViewModel)y.Tag,
					Observable.FromEvent<TreeViewEventHandler, TreeViewEventArgs>(
							h => (s, e) => h(e),
							h => treeViewSound.AfterSelect += h,
							h => treeViewSound.AfterSelect -= h
						)
						.ToUnit()
				)
				.AddTo(soundDisposable);

			x.SelectedTreeItem
				.BindTo(
					listViewSound,
					y => y.Enabled,
					BindingMode.OneWay,
					y => x.TreeItem.Value.Children.Contains(y)
				)
				.AddTo(soundDisposable);

			x.ListColumns
				.ObserveAddChanged()
				.Subscribe(y =>
				{
					listViewSound.Columns.Add(ListViewColumnHeaderViewModelToColumnHeader(y));
					listViewSound.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(soundDisposable);

			x.ListColumns
				.ObserveRemoveChanged()
				.Subscribe(y =>
				{
					foreach (ColumnHeader column in listViewSound.Columns.OfType<ColumnHeader>().Where(z => z.Tag == y).ToArray())
					{
						listViewSound.Columns.Remove(column);
					}

					listViewSound.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(soundDisposable);

			x.ListItems
				.ObserveAddChanged()
				.Subscribe(y =>
				{
					listViewSound.Items.Add(ListViewItemViewModelToListViewItem(y));
					listViewSound.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(soundDisposable);

			x.ListItems
				.ObserveRemoveChanged()
				.Subscribe(y =>
				{
					foreach (ListViewItem item in listViewSound.Items.OfType<ListViewItem>().Where(z => z.Tag == y).ToArray())
					{
						listViewSound.Items.Remove(item);
					}

					listViewSound.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(soundDisposable);

			x.SelectedListItem
				.BindTo(
					this,
					y => y.ListViewSoundSelectedItem,
					BindingMode.TwoWay,
					y => listViewSound.Items.OfType<ListViewItem>().FirstOrDefault(z => z.Tag == y),
					y => (ListViewItemViewModel)y?.Tag,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => listViewSound.SelectedIndexChanged += h,
							h => listViewSound.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(soundDisposable);

			x.SelectedListItem
				.Where(y => y != null)
				.Subscribe(y =>
				{
					listItemDisposable.Dispose();
					listItemDisposable = new CompositeDisposable().AddTo(soundDisposable);

					y.Texts
						.ObserveReplaceChanged()
						.Subscribe(_ =>
						{
							UpdateListViewItem(ListViewSoundSelectedItem, x.SelectedListItem.Value);
							listViewSound.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
						})
						.AddTo(listItemDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedListItem
				.BindTo(
					groupBoxSoundKey,
					y => y.Enabled,
					BindingMode.OneWay,
					y => y != null
				)
				.AddTo(soundDisposable);

			x.SelectedListItem
				.BindTo(
					groupBoxSoundValue,
					y => y.Enabled,
					BindingMode.OneWay,
					y => y != null
				)
				.AddTo(soundDisposable);

			x.SelectedRun
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedFlange
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedMotor
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedFrontSwitch
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedRearSwitch
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedBrake
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<BrakeElementViewModel, BrakeKey>(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedCompressor
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<CompressorElementViewModel, CompressorKey>(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedSuspension
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<SuspensionElementViewModel, SuspensionKey>(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedPrimaryHorn
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<PrimaryHornElementViewModel, HornKey>(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedSecondaryHorn
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<SecondaryHornElementViewModel, HornKey>(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedMusicHorn
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<MusicHornElementViewModel, HornKey>(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedDoor
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<DoorElementViewModel, DoorKey>(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedAts
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedBuzzer
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<BuzzerElementViewModel, BuzzerKey>(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedPilotLamp
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<PilotLampElementViewModel, PilotLampKey>(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedBrakeHandle
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<BrakeHandleElementViewModel, BrakeHandleKey>(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedMasterController
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<MasterControllerElementViewModel, MasterControllerKey>(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedReverser
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<ReverserElementViewModel, ReverserKey>(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedBreaker
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<BreakerElementViewModel, BreakerKey>(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedRequestStop
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<RequestStopElementViewModel, RequestStopKey>(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedTouch
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			x.SelectedOthers
				.Where(y => y != null)
				.Subscribe(y =>
				{
					elementDisposable.Dispose();
					elementDisposable = new CompositeDisposable().AddTo(soundDisposable);

					BindToSoundElement<OthersElementViewModel, OthersKey>(y).AddTo(elementDisposable);
				})
				.AddTo(soundDisposable);

			new[]
				{
					x.AddRun, x.AddFlange, x.AddMotor, x.AddFrontSwitch, x.AddRearSwitch,
					x.AddBrake, x.AddCompressor, x.AddSuspension, x.AddPrimaryHorn, x.AddSecondaryHorn,
					x.AddMusicHorn, x.AddDoor, x.AddAts, x.AddBuzzer, x.AddPilotLamp,
					x.AddBrakeHandle, x.AddMasterController, x.AddReverser, x.AddBreaker, x.AddRequestStop,
					x.AddTouch, x.AddOthers
				}
				.BindToButton(buttonSoundAdd)
				.AddTo(soundDisposable);

			new[]
				{
					x.RemoveRun, x.RemoveFlange, x.RemoveMotor, x.RemoveFrontSwitch, x.RemoveRearSwitch,
					x.RemoveBrake, x.RemoveCompressor, x.RemoveSuspension, x.RemovePrimaryHorn, x.RemoveSecondaryHorn,
					x.RemoveMusicHorn, x.RemoveDoor, x.RemoveAts, x.RemoveBuzzer, x.RemovePilotLamp,
					x.RemoveBrakeHandle, x.RemoveMasterController, x.RemoveReverser, x.RemoveBreaker, x.RemoveRequestStop,
					x.RemoveTouch, x.RemoveOthers
				}
				.BindToButton(buttonSoundRemove)
				.AddTo(soundDisposable);

			return soundDisposable;
		}
	}
}
