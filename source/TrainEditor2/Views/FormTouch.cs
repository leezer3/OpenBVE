using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using OpenBveApi.Interface;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Others;
using TrainEditor2.ViewModels.Panels;

namespace TrainEditor2.Views
{
	public partial class FormTouch : Form
	{
		private readonly CompositeDisposable disposable;

		private readonly TouchElementViewModel touch;

		private TreeNode TreeViewTouchTopNode
		{
			get
			{
				if (treeViewTouch.Nodes.Count == 0)
				{
					treeViewTouch.Nodes.Add(new TreeNode());
				}

				return treeViewTouch.Nodes[0];
			}
			// ReSharper disable once UnusedMember.Local
			set
			{
				treeViewTouch.Nodes.Clear();
				treeViewTouch.Nodes.Add(value);
				treeViewTouch.ExpandAll();
				treeViewTouch.SelectedNode = treeViewTouch.Nodes
					.OfType<TreeNode>()
					.Select(z => FormEditor.SearchTreeNode(touch.SelectedTreeItem.Value, z))
					.FirstOrDefault(z => z != null);
				touch.SelectedTreeItem.ForceNotify();
			}
		}

		private ListViewItem ListViewTouchSelectedItem
		{
			get
			{
				if (listViewTouch.SelectedItems.Count == 1)
				{
					return listViewTouch.SelectedItems[0];
				}

				return null;
			}
			// ReSharper disable once UnusedMember.Local
			set
			{
				foreach (ListViewItem item in listViewTouch.Items.OfType<ListViewItem>().Where(x => x.Selected))
				{
					item.Selected = false;
				}

				if (value != null)
				{
					value.Selected = true;
				}
			}
		}

		internal FormTouch(TouchElementViewModel touch)
		{
			disposable = new CompositeDisposable();
			CompositeDisposable listItemDisposable = new CompositeDisposable().AddTo(disposable);
			CompositeDisposable entryDisposable = new CompositeDisposable().AddTo(disposable);

			this.touch = touch;

			InitializeComponent();

			touch.TreeItem
				.BindTo(
					this,
					x => x.TreeViewTouchTopNode,
					BindingMode.OneWay,
					FormEditor.TreeViewItemViewModelToTreeNode
				)
				.AddTo(disposable);

			touch.SelectedTreeItem
				.BindTo(
					treeViewTouch,
					x => x.SelectedNode,
					BindingMode.TwoWay,
					x => treeViewTouch.Nodes.OfType<TreeNode>().Select(y => FormEditor.SearchTreeNode(x, y)).FirstOrDefault(y => y != null),
					x => (TreeViewItemViewModel)x.Tag,
					Observable.FromEvent<TreeViewEventHandler, TreeViewEventArgs>(
							h => (s, e) => h(e),
							h => treeViewTouch.AfterSelect += h,
							h => treeViewTouch.AfterSelect -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			touch.SelectedTreeItem
				.BindTo(
					listViewTouch,
					x => x.Enabled,
					BindingMode.OneWay,
					x => touch.TreeItem.Value.Children.Contains(x)
				)
				.AddTo(disposable);

			touch.ListColumns
				.ObserveAddChanged()
				.Subscribe(x =>
				{
					listViewTouch.Columns.Add(FormEditor.ListViewColumnHeaderViewModelToColumnHeader(x));
					listViewTouch.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(disposable);

			touch.ListColumns
				.ObserveRemoveChanged()
				.Subscribe(y =>
				{
					foreach (ColumnHeader column in listViewTouch.Columns.OfType<ColumnHeader>().Where(z => z.Tag == y).ToArray())
					{
						listViewTouch.Columns.Remove(column);
					}

					listViewTouch.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(disposable);

			touch.ListColumns
				.ObserveResetChanged()
				.Subscribe(_ =>
				{
					listViewTouch.Columns.Clear();
					listViewTouch.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(disposable);

			touch.ListItems
				.ObserveAddChanged()
				.Subscribe(x =>
				{
					listViewTouch.Items.Add(FormEditor.ListViewItemViewModelToListViewItem(x));
					listViewTouch.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(disposable);

			touch.ListItems
				.ObserveRemoveChanged()
				.Subscribe(x =>
				{
					foreach (ListViewItem item in listViewTouch.Items.OfType<ListViewItem>().Where(y => y.Tag == x).ToArray())
					{
						listViewTouch.Items.Remove(item);
					}

					listViewTouch.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(disposable);

			touch.ListItems
				.ObserveResetChanged()
				.Subscribe(_ =>
				{
					listViewTouch.Items.Clear();
					listViewTouch.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(disposable);

			touch.SelectedListItem
				.BindTo(
					this,
					x => x.ListViewTouchSelectedItem,
					BindingMode.TwoWay,
					x => listViewTouch.Items.OfType<ListViewItem>().FirstOrDefault(y => y.Tag == x),
					x => (ListViewItemViewModel)x?.Tag,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => listViewTouch.SelectedIndexChanged += h,
							h => listViewTouch.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			touch.SelectedListItem
				.Where(x => x != null)
				.Subscribe(x =>
				{
					listItemDisposable.Dispose();
					listItemDisposable = new CompositeDisposable().AddTo(disposable);

					x.Texts
						.ObserveReplaceChanged()
						.Subscribe(_ =>
						{
							FormEditor.UpdateListViewItem(ListViewTouchSelectedItem, touch.SelectedListItem.Value);
							listViewTouch.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
						})
						.AddTo(listItemDisposable);
				})
				.AddTo(disposable);

			touch.SelectedSoundEntry
				.BindTo(
					groupBoxTouchSound,
					x => x.Enabled,
					x => x != null
				)
				.AddTo(disposable);

			touch.SelectedSoundEntry
				.Where(x => x != null)
				.Subscribe(x =>
				{
					entryDisposable.Dispose();
					entryDisposable = new CompositeDisposable().AddTo(disposable);

					BindToSoundEntry(x).AddTo(entryDisposable);
				})
				.AddTo(disposable);

			touch.SelectedCommandEntry
				.BindTo(
					groupBoxTouchCommand,
					x => x.Enabled,
					x => x != null
				)
				.AddTo(disposable);

			touch.SelectedCommandEntry
				.Where(x => x != null)
				.Subscribe(x =>
				{
					entryDisposable.Dispose();
					entryDisposable = new CompositeDisposable().AddTo(disposable);

					BindToCommandEntry(x).AddTo(entryDisposable);
				})
				.AddTo(disposable);

			comboBoxTouchCommandName.Items
				.AddRange(
					Enum.GetValues(typeof(Translations.Command))
						.OfType<Translations.Command>()
						.Select(c => Translations.CommandInfos.TryGetInfo(c).Name)
						.OfType<object>()
						.ToArray()
				);

			new[] { touch.AddSoundEntry, touch.AddCommandEntry }
				.BindToButton(buttonTouchAdd)
				.AddTo(disposable);

			new[] { touch.CopySoundEntry, touch.CopyCommandEntry }
				.BindToButton(buttonTouchCopy)
				.AddTo(disposable);

			new[] { touch.RemoveSoundEntry, touch.RemoveCommandEntry }
				.BindToButton(buttonTouchRemove)
				.AddTo(disposable);
		}

		private void FormTouch_Load(object sender, EventArgs e)
		{
			Icon = FormEditor.GetIcon();
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			ListViewTouchSelectedItem = null;
			treeViewTouch.SelectedNode = TreeViewTouchTopNode;

			Close();
		}

		private IDisposable BindToSoundEntry(TouchElementViewModel.SoundEntryViewModel entry)
		{
			CompositeDisposable entryDisposable = new CompositeDisposable();

			entry.Index
				.BindTo(
					numericUpDownTouchSoundIndex,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownTouchSoundIndex.ValueChanged += h,
							h => numericUpDownTouchSoundIndex.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(entryDisposable);

			return entryDisposable;
		}

		private IDisposable BindToCommandEntry(TouchElementViewModel.CommandEntryViewModel entry)
		{
			CompositeDisposable entryDisposable = new CompositeDisposable();

			entry.Info
				.BindTo(
					comboBoxTouchCommandName,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x.Command,
					x => Translations.CommandInfos.TryGetInfo((Translations.Command)x),
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxTouchCommandName.SelectedIndexChanged += h,
							h => comboBoxTouchCommandName.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(entryDisposable);

			entry.Option
				.BindTo(
					numericUpDownTouchCommandOption,
					x => x.Value,
					BindingMode.TwoWay,
					null,
					x => (int)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => numericUpDownTouchCommandOption.ValueChanged += h,
							h => numericUpDownTouchCommandOption.ValueChanged -= h
						)
						.ToUnit()
				)
				.AddTo(entryDisposable);

			return entryDisposable;
		}
	}
}
