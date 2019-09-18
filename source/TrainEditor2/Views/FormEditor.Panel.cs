using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Others;
using TrainEditor2.ViewModels.Panels;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToPanel(PanelViewModel x)
		{
			CompositeDisposable panelDisposable = new CompositeDisposable();
			CompositeDisposable listItemDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable thisDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable screenDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable pilotLampDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable needleDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable digitalNumberDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable digitalGaugeDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable linearGaugeDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable timetableDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable touchDisposable = new CompositeDisposable().AddTo(panelDisposable);

			listViewPanel.Items.Clear();

			x.TreeItem
				.BindTo(
					this,
					y => y.TreeViewPanelTopNode,
					BindingMode.OneWay,
					TreeViewItemViewModelToTreeNode
				)
				.AddTo(panelDisposable);

			x.SelectedTreeItem
				.BindTo(
					treeViewPanel,
					y => y.SelectedNode,
					BindingMode.TwoWay,
					y => treeViewPanel.Nodes.OfType<TreeNode>().Select(z => SearchTreeNode(y, z)).FirstOrDefault(z => z != null),
					y => (TreeViewItemViewModel)y.Tag,
					Observable.FromEvent<TreeViewEventHandler, TreeViewEventArgs>(
							h => (s, e) => h(e),
							h => treeViewPanel.AfterSelect += h,
							h => treeViewPanel.AfterSelect -= h
						)
						.ToUnit()
				)
				.AddTo(panelDisposable);

			x.SelectedTreeItem
				.BindTo(
					tabControlPanel,
					y => y.SelectedTab,
					BindingMode.OneWay,
					y => y == x.TreeItem.Value.Children[0] ? tabPageThis : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			x.SelectedTreeItem
				.BindTo(
					tabPageThis,
					y => y.Enabled,
					BindingMode.OneWay,
					y => y == x.TreeItem.Value.Children[0]
				)
				.AddTo(panelDisposable);

			x.SelectedTreeItem
				.BindTo(
					listViewPanel,
					y => y.Enabled,
					BindingMode.OneWay,
					y => y == x.TreeItem.Value.Children[1]
						 || x.TreeItem.Value.Children[1].Children.Any(z => z.Children[0].Children.Contains(y) || y == z.Children[1])
						 || x.TreeItem.Value.Children[2].Children.Contains(y)
				)
				.AddTo(panelDisposable);

			x.ListColumns
				.ObserveAddChanged()
				.Subscribe(y =>
				{
					listViewPanel.Columns.Add(ListViewColumnHeaderViewModelToColumnHeader(y));
					listViewPanel.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(panelDisposable);

			x.ListColumns
				.ObserveRemoveChanged()
				.Subscribe(y =>
				{
					foreach (ColumnHeader column in listViewPanel.Columns.OfType<ColumnHeader>().Where(z => z.Tag == y).ToArray())
					{
						listViewPanel.Columns.Remove(column);
					}

					listViewPanel.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(panelDisposable);

			x.ListItems
				.ObserveAddChanged()
				.Subscribe(y =>
				{
					listViewPanel.Items.Add(ListViewItemViewModelToListViewItem(y));
					listViewPanel.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(panelDisposable);

			x.ListItems
				.ObserveRemoveChanged()
				.Subscribe(y =>
				{
					foreach (ListViewItem item in listViewPanel.Items.OfType<ListViewItem>().Where(z => z.Tag == y).ToArray())
					{
						listViewPanel.Items.Remove(item);
					}

					listViewPanel.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
				})
				.AddTo(panelDisposable);

			x.SelectedListItem
				.BindTo(
					this,
					y => y.ListViewPanelSelectedItem,
					BindingMode.TwoWay,
					y => listViewPanel.Items.OfType<ListViewItem>().FirstOrDefault(z => z.Tag == y),
					y => (ListViewItemViewModel)y?.Tag,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => listViewPanel.SelectedIndexChanged += h,
							h => listViewPanel.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(panelDisposable);

			x.SelectedListItem
				.Where(y => y != null)
				.Subscribe(y =>
				{
					listItemDisposable.Dispose();
					listItemDisposable = new CompositeDisposable().AddTo(panelDisposable);

					y.Texts
						.ObserveReplaceChanged()
						.Subscribe(_ =>
						{
							UpdateListViewItem(ListViewPanelSelectedItem, x.SelectedListItem.Value);
							listViewPanel.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
						})
						.AddTo(listItemDisposable);
				})
				.AddTo(panelDisposable);

			x.This
				.Subscribe(y =>
				{
					thisDisposable.Dispose();
					thisDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToThis(y).AddTo(thisDisposable);
				})
				.AddTo(panelDisposable);

			x.SelectedScreen
				.BindTo(
					tabControlPanel,
					y => y.SelectedTab,
					y => y != null ? tabPageScreen : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			x.SelectedScreen
				.BindTo(
					tabPageScreen,
					y => y.Enabled,
					y => y != null
				)
				.AddTo(panelDisposable);

			x.SelectedScreen
				.Where(y => y != null)
				.Subscribe(y =>
				{
					screenDisposable.Dispose();
					screenDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToScreen(y).AddTo(screenDisposable);
				})
				.AddTo(panelDisposable);

			x.SelectedPilotLamp
				.BindTo(
					tabControlPanel,
					y => y.SelectedTab,
					y => y != null ? tabPagePilotLamp : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			x.SelectedPilotLamp
				.BindTo(
					tabPagePilotLamp,
					y => y.Enabled,
					y => y != null
				)
				.AddTo(panelDisposable);

			x.SelectedPilotLamp
				.Where(y => y != null)
				.Subscribe(y =>
				{
					pilotLampDisposable.Dispose();
					pilotLampDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToPilotLamp(y).AddTo(pilotLampDisposable);
				})
				.AddTo(panelDisposable);

			x.SelectedNeedle
				.BindTo(
					tabControlPanel,
					y => y.SelectedTab,
					y => y != null ? tabPageNeedle : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			x.SelectedNeedle
				.BindTo(
					tabPageNeedle,
					y => y.Enabled,
					y => y != null
				)
				.AddTo(panelDisposable);

			x.SelectedNeedle
				.Where(y => y != null)
				.Subscribe(y =>
				{
					needleDisposable.Dispose();
					needleDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToNeedle(y).AddTo(needleDisposable);
				})
				.AddTo(panelDisposable);

			x.SelectedDigitalNumber
				.BindTo(
					tabControlPanel,
					y => y.SelectedTab,
					y => y != null ? tabPageDigitalNumber : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			x.SelectedDigitalNumber
				.BindTo(
					tabPageDigitalNumber,
					y => y.Enabled,
					y => y != null
				)
				.AddTo(panelDisposable);

			x.SelectedDigitalNumber
				.Where(y => y != null)
				.Subscribe(y =>
				{
					digitalNumberDisposable.Dispose();
					digitalNumberDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToDigitalNumber(y).AddTo(digitalNumberDisposable);
				})
				.AddTo(panelDisposable);

			x.SelectedDigitalGauge
				.BindTo(
					tabControlPanel,
					y => y.SelectedTab,
					y => y != null ? tabPageDigitalGauge : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			x.SelectedDigitalGauge
				.BindTo(
					tabPageDigitalGauge,
					y => y.Enabled,
					y => y != null
				)
				.AddTo(panelDisposable);

			x.SelectedDigitalGauge
				.Where(y => y != null)
				.Subscribe(y =>
				{
					digitalGaugeDisposable.Dispose();
					digitalGaugeDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToDigitalGauge(y).AddTo(digitalGaugeDisposable);
				})
				.AddTo(panelDisposable);

			x.SelectedLinearGauge
				.BindTo(
					tabControlPanel,
					y => y.SelectedTab,
					y => y != null ? tabPageLinearGauge : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			x.SelectedLinearGauge
				.BindTo(
					tabPageLinearGauge,
					y => y.Enabled,
					y => y != null
				)
				.AddTo(panelDisposable);

			x.SelectedLinearGauge
				.Where(y => y != null)
				.Subscribe(y =>
				{
					linearGaugeDisposable.Dispose();
					linearGaugeDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToLinearGauge(y).AddTo(linearGaugeDisposable);
				})
				.AddTo(panelDisposable);

			x.SelectedTimetable
				.BindTo(
					tabControlPanel,
					y => y.SelectedTab,
					y => y != null ? tabPageTimetable : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			x.SelectedTimetable
				.BindTo(
					tabPageTimetable,
					y => y.Enabled,
					y => y != null
				)
				.AddTo(panelDisposable);

			x.SelectedTimetable
				.Where(y => y != null)
				.Subscribe(y =>
				{
					timetableDisposable.Dispose();
					timetableDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToTimetable(y).AddTo(timetableDisposable);
				})
				.AddTo(panelDisposable);

			x.SelectedTouch
				.BindTo(
					tabControlPanel,
					y => y.SelectedTab,
					y => y != null ? tabPageTouch : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			x.SelectedTouch
				.BindTo(
					tabPageTouch,
					y => y.Enabled,
					y => y != null
				)
				.AddTo(panelDisposable);

			x.SelectedTouch
				.Where(y => y != null)
				.Subscribe(y =>
				{
					touchDisposable.Dispose();
					touchDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToTouch(y).AddTo(touchDisposable);
				})
				.AddTo(panelDisposable);

			new[] { x.AddScreen, x.AddPilotLamp, x.AddNeedle, x.AddDigitalNumber, x.AddDigitalGauge, x.AddLinearGauge, x.AddTimetable, x.AddTouch }
				.BindToButton(buttonPanelAdd)
				.AddTo(panelDisposable);

			new[] { x.CopyScreen, x.CopyPilotLamp, x.CopyNeedle, x.CopyDigitalNumber, x.CopyDigitalGauge, x.CopyLinearGauge, x.CopyTimetable, x.CopyTouch }
				.BindToButton(buttonPanelCopy)
				.AddTo(panelDisposable);

			new[] { x.RemoveScreen, x.RemovePilotLamp, x.RemoveNeedle, x.RemoveDigitalNumber, x.RemoveDigitalGauge, x.RemoveLinearGauge, x.RemoveTimetable, x.RemoveTouch }
				.BindToButton(buttonPanelRemove)
				.AddTo(panelDisposable);

			return panelDisposable;
		}
	}
}
