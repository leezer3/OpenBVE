using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.ViewModels.Panels;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToPanel(PanelViewModel panel)
		{
			CompositeDisposable panelDisposable = new CompositeDisposable();
			CompositeDisposable thisDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable screenDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable pilotLampDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable needleDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable digitalNumberDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable digitalGaugeDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable linearGaugeDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable timetableDisposable = new CompositeDisposable().AddTo(panelDisposable);
			CompositeDisposable touchDisposable = new CompositeDisposable().AddTo(panelDisposable);

			WinFormsBinders.BindToTreeView(treeViewPanel, panel.TreeItems, panel.SelectedTreeItem).AddTo(panelDisposable);

			panel.SelectedTreeItem
				.BindTo(
					tabControlPanel,
					x => x.SelectedTab,
					BindingMode.OneWay,
					x => x == panel.TreeItems[0].Children[0] ? tabPageThis : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			panel.SelectedTreeItem
				.BindTo(
					tabPageThis,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == panel.TreeItems[0].Children[0]
				)
				.AddTo(panelDisposable);

			panel.SelectedTreeItem
				.BindTo(
					listViewPanel,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == panel.TreeItems[0].Children[1]
						 || panel.TreeItems[0].Children[1].Children.Any(z => z.Children[0].Children.Contains(x) || x == z.Children[1])
						 || panel.TreeItems[0].Children[2].Children.Contains(x)
				)
				.AddTo(panelDisposable);

			WinFormsBinders.BindToListView(listViewPanel, panel.ListColumns, panel.ListItems, panel.SelectedListItem).AddTo(panelDisposable);

			panel.This
				.Subscribe(x =>
				{
					thisDisposable.Dispose();
					thisDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToThis(x).AddTo(thisDisposable);
				})
				.AddTo(panelDisposable);

			panel.SelectedScreen
				.BindTo(
					tabControlPanel,
					x => x.SelectedTab,
					x => x != null ? tabPageScreen : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			panel.SelectedScreen
				.BindTo(
					tabPageScreen,
					x => x.Enabled,
					x => x != null
				)
				.AddTo(panelDisposable);

			panel.SelectedScreen
				.Where(x => x != null)
				.Subscribe(x =>
				{
					screenDisposable.Dispose();
					screenDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToScreen(x).AddTo(screenDisposable);
				})
				.AddTo(panelDisposable);

			panel.SelectedPilotLamp
				.BindTo(
					tabControlPanel,
					x => x.SelectedTab,
					x => x != null ? tabPagePilotLamp : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			panel.SelectedPilotLamp
				.BindTo(
					tabPagePilotLamp,
					x => x.Enabled,
					x => x != null
				)
				.AddTo(panelDisposable);

			panel.SelectedPilotLamp
				.Where(x => x != null)
				.Subscribe(x =>
				{
					pilotLampDisposable.Dispose();
					pilotLampDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToPilotLamp(x).AddTo(pilotLampDisposable);
				})
				.AddTo(panelDisposable);

			panel.SelectedNeedle
				.BindTo(
					tabControlPanel,
					x => x.SelectedTab,
					x => x != null ? tabPageNeedle : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			panel.SelectedNeedle
				.BindTo(
					tabPageNeedle,
					x => x.Enabled,
					x => x != null
				)
				.AddTo(panelDisposable);

			panel.SelectedNeedle
				.Where(x => x != null)
				.Subscribe(x =>
				{
					needleDisposable.Dispose();
					needleDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToNeedle(x).AddTo(needleDisposable);
				})
				.AddTo(panelDisposable);

			panel.SelectedDigitalNumber
				.BindTo(
					tabControlPanel,
					x => x.SelectedTab,
					x => x != null ? tabPageDigitalNumber : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			panel.SelectedDigitalNumber
				.BindTo(
					tabPageDigitalNumber,
					x => x.Enabled,
					x => x != null
				)
				.AddTo(panelDisposable);

			panel.SelectedDigitalNumber
				.Where(x => x != null)
				.Subscribe(x =>
				{
					digitalNumberDisposable.Dispose();
					digitalNumberDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToDigitalNumber(x).AddTo(digitalNumberDisposable);
				})
				.AddTo(panelDisposable);

			panel.SelectedDigitalGauge
				.BindTo(
					tabControlPanel,
					x => x.SelectedTab,
					x => x != null ? tabPageDigitalGauge : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			panel.SelectedDigitalGauge
				.BindTo(
					tabPageDigitalGauge,
					x => x.Enabled,
					x => x != null
				)
				.AddTo(panelDisposable);

			panel.SelectedDigitalGauge
				.Where(x => x != null)
				.Subscribe(x =>
				{
					digitalGaugeDisposable.Dispose();
					digitalGaugeDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToDigitalGauge(x).AddTo(digitalGaugeDisposable);
				})
				.AddTo(panelDisposable);

			panel.SelectedLinearGauge
				.BindTo(
					tabControlPanel,
					x => x.SelectedTab,
					x => x != null ? tabPageLinearGauge : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			panel.SelectedLinearGauge
				.BindTo(
					tabPageLinearGauge,
					x => x.Enabled,
					x => x != null
				)
				.AddTo(panelDisposable);

			panel.SelectedLinearGauge
				.Where(x => x != null)
				.Subscribe(x =>
				{
					linearGaugeDisposable.Dispose();
					linearGaugeDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToLinearGauge(x).AddTo(linearGaugeDisposable);
				})
				.AddTo(panelDisposable);

			panel.SelectedTimetable
				.BindTo(
					tabControlPanel,
					x => x.SelectedTab,
					x => x != null ? tabPageTimetable : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			panel.SelectedTimetable
				.BindTo(
					tabPageTimetable,
					x => x.Enabled,
					x => x != null
				)
				.AddTo(panelDisposable);

			panel.SelectedTimetable
				.Where(x => x != null)
				.Subscribe(x =>
				{
					timetableDisposable.Dispose();
					timetableDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToTimetable(x).AddTo(timetableDisposable);
				})
				.AddTo(panelDisposable);

			panel.SelectedTouch
				.BindTo(
					tabControlPanel,
					x => x.SelectedTab,
					x => x != null ? tabPageTouch : tabControlPanel.SelectedTab
				)
				.AddTo(panelDisposable);

			panel.SelectedTouch
				.BindTo(
					tabPageTouch,
					x => x.Enabled,
					x => x != null
				)
				.AddTo(panelDisposable);

			panel.SelectedTouch
				.Where(x => x != null)
				.Subscribe(x =>
				{
					touchDisposable.Dispose();
					touchDisposable = new CompositeDisposable().AddTo(panelDisposable);

					BindToTouch(x).AddTo(touchDisposable);
				})
				.AddTo(panelDisposable);

			new[] { panel.UpScreen, panel.UpPilotLamp, panel.UpNeedle, panel.UpDigitalNumber, panel.UpDigitalGauge, panel.UpLinearGauge, panel.UpTimetable, panel.UpTouch }
				.BindToButton(buttonPanelUp)
				.AddTo(panelDisposable);

			new[] { panel.DownScreen, panel.DownPilotLamp, panel.DownNeedle, panel.DownDigitalNumber, panel.DownDigitalGauge, panel.DownLinearGauge, panel.DownTimetable, panel.DownTouch }
				.BindToButton(buttonPanelDown)
				.AddTo(panelDisposable);

			new[] { panel.AddScreen, panel.AddPilotLamp, panel.AddNeedle, panel.AddDigitalNumber, panel.AddDigitalGauge, panel.AddLinearGauge, panel.AddTimetable, panel.AddTouch }
				.BindToButton(buttonPanelAdd)
				.AddTo(panelDisposable);

			new[] { panel.CopyScreen, panel.CopyPilotLamp, panel.CopyNeedle, panel.CopyDigitalNumber, panel.CopyDigitalGauge, panel.CopyLinearGauge, panel.CopyTimetable, panel.CopyTouch }
				.BindToButton(buttonPanelCopy)
				.AddTo(panelDisposable);

			new[] { panel.RemoveScreen, panel.RemovePanelElement, panel.RemoveTouch }
				.BindToButton(buttonPanelRemove)
				.AddTo(panelDisposable);

			return panelDisposable;
		}
	}
}
