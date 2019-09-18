using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models.Panels;
using TrainEditor2.ViewModels.Others;

namespace TrainEditor2.ViewModels.Panels
{
	internal class PanelViewModel : BaseViewModel
	{
		internal ReadOnlyReactivePropertySlim<ThisViewModel> This
		{
			get;
		}

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

		internal ReadOnlyReactivePropertySlim<ScreenViewModel> SelectedScreen
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<PilotLampElementViewModel> SelectedPilotLamp
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<NeedleElementViewModel> SelectedNeedle
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<DigitalNumberElementViewModel> SelectedDigitalNumber
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<DigitalGaugeElementViewModel> SelectedDigitalGauge
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<LinearGaugeElementViewModel> SelectedLinearGauge
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<TimetableElementViewModel> SelectedTimetable
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<TouchElementViewModel> SelectedTouch
		{
			get;
		}

		internal ReactiveCommand AddScreen
		{
			get;
		}

		internal ReactiveCommand AddPilotLamp
		{
			get;
		}

		internal ReactiveCommand AddNeedle
		{
			get;
		}

		internal ReactiveCommand AddDigitalNumber
		{
			get;
		}

		internal ReactiveCommand AddDigitalGauge
		{
			get;
		}

		internal ReactiveCommand AddLinearGauge
		{
			get;
		}

		internal ReactiveCommand AddTimetable
		{
			get;
		}

		internal ReactiveCommand AddTouch
		{
			get;
		}

		internal ReactiveCommand CopyScreen
		{
			get;
		}

		internal ReactiveCommand CopyPilotLamp
		{
			get;
		}

		internal ReactiveCommand CopyNeedle
		{
			get;
		}

		internal ReactiveCommand CopyDigitalNumber
		{
			get;
		}

		internal ReactiveCommand CopyDigitalGauge
		{
			get;
		}

		internal ReactiveCommand CopyLinearGauge
		{
			get;
		}

		internal ReactiveCommand CopyTimetable
		{
			get;
		}

		internal ReactiveCommand CopyTouch
		{
			get;
		}

		internal ReactiveCommand RemoveScreen
		{
			get;
		}

		internal ReactiveCommand RemovePilotLamp
		{
			get;
		}

		internal ReactiveCommand RemoveNeedle
		{
			get;
		}

		internal ReactiveCommand RemoveDigitalNumber
		{
			get;
		}

		internal ReactiveCommand RemoveDigitalGauge
		{
			get;
		}

		internal ReactiveCommand RemoveLinearGauge
		{
			get;
		}

		internal ReactiveCommand RemoveTimetable
		{
			get;
		}

		internal ReactiveCommand RemoveTouch
		{
			get;
		}

		internal PanelViewModel(Panel panel)
		{
			CompositeDisposable treeItemDisposable = new CompositeDisposable();
			CompositeDisposable listItemDisposable = new CompositeDisposable();

			This = panel
				.ObserveProperty(x => x.This)
				.Do(_ => This?.Value.Dispose())
				.Select(x => new ThisViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			TreeItem = panel
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

			SelectedTreeItem = panel
				.ToReactivePropertyAsSynchronized(
					x => x.SelectedTreeItem,
					x => TreeItem.Value.SearchViewModel(x),
					x => x?.Model
				)
				.AddTo(disposable);

			ListColumns = panel.ListColumns
				.ToReadOnlyReactiveCollection(x => new ListViewColumnHeaderViewModel(x))
				.AddTo(disposable);

			ListItems = panel.ListItems
				.ToReadOnlyReactiveCollection(x => new ListViewItemViewModel(x))
				.AddTo(disposable);

			SelectedListItem = panel
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
					panel.CreateListColumns();
					panel.CreateListItems();
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

							CompositeDisposable subjectDisposable = new CompositeDisposable();

							y.PropertyChangedAsObservable()
								.Subscribe(_ => panel.UpdateListItem(x.Model))
								.AddTo(tagDisposable);

							Screen screen = y as Screen;
							PilotLampElement pilotLamp = y as PilotLampElement;
							NeedleElement needle = y as NeedleElement;
							DigitalNumberElement digitalNumber = y as DigitalNumberElement;
							DigitalGaugeElement digitalGauge = y as DigitalGaugeElement;
							LinearGaugeElement linearGauge = y as LinearGaugeElement;

							screen?.ObserveProperty(z => z.Number)
								.Select(_ => TreeItem.Value.Children[1].Children.FirstOrDefault(z => z.Tag.Value == screen))
								.Where(z => z != null)
								.Subscribe(z => panel.RenameScreenTreeItem(z.Model))
								.AddTo(tagDisposable);

							pilotLamp?.ObserveProperty(z => z.Subject)
								.Subscribe(z =>
								{
									subjectDisposable.Dispose();
									subjectDisposable = new CompositeDisposable();

									z.PropertyChangedAsObservable()
										.Subscribe(_ => panel.UpdateListItem(x.Model))
										.AddTo(subjectDisposable);
								})
								.AddTo(tagDisposable);

							needle?.ObserveProperty(z => z.Subject)
								.Subscribe(z =>
								{
									subjectDisposable.Dispose();
									subjectDisposable = new CompositeDisposable();

									z.PropertyChangedAsObservable()
										.Subscribe(_ => panel.UpdateListItem(x.Model))
										.AddTo(subjectDisposable);
								})
								.AddTo(tagDisposable);

							digitalNumber?.ObserveProperty(z => z.Subject)
								.Subscribe(z =>
								{
									subjectDisposable.Dispose();
									subjectDisposable = new CompositeDisposable();

									z.PropertyChangedAsObservable()
										.Subscribe(_ => panel.UpdateListItem(x.Model))
										.AddTo(subjectDisposable);
								})
								.AddTo(tagDisposable);

							digitalGauge?.ObserveProperty(z => z.Subject)
								.Subscribe(z =>
								{
									subjectDisposable.Dispose();
									subjectDisposable = new CompositeDisposable();

									z.PropertyChangedAsObservable()
										.Subscribe(_ => panel.UpdateListItem(x.Model))
										.AddTo(subjectDisposable);
								})
								.AddTo(tagDisposable);

							linearGauge?.ObserveProperty(z => z.Subject)
								.Subscribe(z =>
								{
									subjectDisposable.Dispose();
									subjectDisposable = new CompositeDisposable();

									z.PropertyChangedAsObservable()
										.Subscribe(_ => panel.UpdateListItem(x.Model))
										.AddTo(subjectDisposable);
								})
								.AddTo(tagDisposable);

							subjectDisposable.AddTo(tagDisposable);
						})
						.AddTo(listItemDisposable);

					tagDisposable.AddTo(listItemDisposable);
				})
				.AddTo(disposable);

			SelectedScreen = SelectedListItem
				.Select(x => x?.Tag.Value as Screen)
				.Do(_ => SelectedScreen?.Value?.Dispose())
				.Select(x => x != null ? new ScreenViewModel(x, panel.Screens.Where(y => y != x)) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedPilotLamp = SelectedListItem
				.Select(x => x?.Tag.Value as PilotLampElement)
				.Do(_ => SelectedPilotLamp?.Value?.Dispose())
				.Select(x => x != null ? new PilotLampElementViewModel(x) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedNeedle = SelectedListItem
				.Select(x => x?.Tag.Value as NeedleElement)
				.Do(_ => SelectedNeedle?.Value?.Dispose())
				.Select(x => x != null ? new NeedleElementViewModel(x) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedDigitalNumber = SelectedListItem
				.Select(x => x?.Tag.Value as DigitalNumberElement)
				.Do(_ => SelectedDigitalNumber?.Value?.Dispose())
				.Select(x => x != null ? new DigitalNumberElementViewModel(x) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedDigitalGauge = SelectedListItem
				.Select(x => x?.Tag.Value as DigitalGaugeElement)
				.Do(_ => SelectedDigitalGauge?.Value?.Dispose())
				.Select(x => x != null ? new DigitalGaugeElementViewModel(x) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedLinearGauge = SelectedListItem
				.Select(x => x?.Tag.Value as LinearGaugeElement)
				.Do(_ => SelectedLinearGauge?.Value?.Dispose())
				.Select(x => x != null ? new LinearGaugeElementViewModel(x) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedTimetable = SelectedListItem
				.Select(x => x?.Tag.Value as TimetableElement)
				.Do(_ => SelectedTimetable?.Value?.Dispose())
				.Select(x => x != null ? new TimetableElementViewModel(x) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedTouch = SelectedListItem
				.Select(x => x?.Tag.Value as TouchElement)
				.Do(_ => SelectedTouch?.Value?.Dispose())
				.Select(x => x != null ? new TouchElementViewModel(x) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			AddScreen = SelectedTreeItem
				.Select(x => x == TreeItem.Value.Children[1])
				.ToReactiveCommand()
				.WithSubscribe(panel.AddScreen)
				.AddTo(disposable);

			AddPilotLamp = SelectedTreeItem
				.Select(x => TreeItem.Value.Children[1].Children.Any(y => x == y.Children[0].Children[0])
							 || x == TreeItem.Value.Children[2].Children[0])
				.ToReactiveCommand()
				.WithSubscribe(panel.AddPilotLamp)
				.AddTo(disposable);

			AddNeedle = SelectedTreeItem
				.Select(x => TreeItem.Value.Children[1].Children.Any(y => x == y.Children[0].Children[1])
							 || x == TreeItem.Value.Children[2].Children[1])
				.ToReactiveCommand()
				.WithSubscribe(panel.AddNeedle)
				.AddTo(disposable);

			AddDigitalNumber = SelectedTreeItem
				.Select(x => TreeItem.Value.Children[1].Children.Any(y => x == y.Children[0].Children[2])
							 || x == TreeItem.Value.Children[2].Children[2])
				.ToReactiveCommand()
				.WithSubscribe(panel.AddDigitalNumber)
				.AddTo(disposable);

			AddDigitalGauge = SelectedTreeItem
				.Select(x => TreeItem.Value.Children[1].Children.Any(y => x == y.Children[0].Children[3])
							 || x == TreeItem.Value.Children[2].Children[3])
				.ToReactiveCommand()
				.WithSubscribe(panel.AddDigitalGauge)
				.AddTo(disposable);

			AddLinearGauge = SelectedTreeItem
				.Select(x => TreeItem.Value.Children[1].Children.Any(y => x == y.Children[0].Children[4])
							 || x == TreeItem.Value.Children[2].Children[4])
				.ToReactiveCommand()
				.WithSubscribe(panel.AddLinearGauge)
				.AddTo(disposable);

			AddTimetable = SelectedTreeItem
				.Select(x => TreeItem.Value.Children[1].Children.Any(y => x == y.Children[0].Children[5])
							 || x == TreeItem.Value.Children[2].Children[5])
				.ToReactiveCommand()
				.WithSubscribe(panel.AddTimetable)
				.AddTo(disposable);

			AddTouch = SelectedTreeItem
				.Select(x => TreeItem.Value.Children[1].Children.Any(y => x == y.Children[1]))
				.ToReactiveCommand()
				.WithSubscribe(panel.AddTouch)
				.AddTo(disposable);

			CopyScreen = SelectedScreen
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.CopyScreen)
				.AddTo(disposable);

			CopyPilotLamp = SelectedPilotLamp
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.CopyPilotLamp)
				.AddTo(disposable);

			CopyNeedle = SelectedNeedle
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.CopyNeedle)
				.AddTo(disposable);

			CopyDigitalNumber = SelectedDigitalNumber
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.CopyDigitalNumber)
				.AddTo(disposable);

			CopyDigitalGauge = SelectedDigitalGauge
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.CopyDigitalGauge)
				.AddTo(disposable);

			CopyLinearGauge = SelectedLinearGauge
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.CopyLinearGauge)
				.AddTo(disposable);

			CopyTimetable = SelectedTimetable
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.CopyTimetable)
				.AddTo(disposable);

			CopyTouch = SelectedTouch
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.CopyTouch)
				.AddTo(disposable);

			RemoveScreen = SelectedScreen
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.RemoveScreen)
				.AddTo(disposable);

			RemovePilotLamp = SelectedPilotLamp
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.RemovePilotLamp)
				.AddTo(disposable);

			RemoveNeedle = SelectedNeedle
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.RemoveNeedle)
				.AddTo(disposable);

			RemoveDigitalNumber = SelectedDigitalNumber
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.RemoveDigitalNumber)
				.AddTo(disposable);

			RemoveDigitalGauge = SelectedDigitalGauge
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.RemoveDigitalGauge)
				.AddTo(disposable);

			RemoveLinearGauge = SelectedLinearGauge
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.RemoveLinearGauge)
				.AddTo(disposable);

			RemoveTimetable = SelectedTimetable
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.RemoveTimetable)
				.AddTo(disposable);

			RemoveTouch = SelectedTouch
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.RemoveTouch)
				.AddTo(disposable);

			treeItemDisposable.AddTo(disposable);
			listItemDisposable.AddTo(disposable);
		}
	}
}
