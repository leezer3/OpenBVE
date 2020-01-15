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

		internal ReactiveCommand UpScreen
		{
			get;
		}

		internal ReactiveCommand UpPilotLamp
		{
			get;
		}

		internal ReactiveCommand UpNeedle
		{
			get;
		}

		internal ReactiveCommand UpDigitalNumber
		{
			get;
		}

		internal ReactiveCommand UpDigitalGauge
		{
			get;
		}

		internal ReactiveCommand UpLinearGauge
		{
			get;
		}

		internal ReactiveCommand UpTimetable
		{
			get;
		}

		internal ReactiveCommand UpTouch
		{
			get;
		}

		internal ReactiveCommand DownScreen
		{
			get;
		}

		internal ReactiveCommand DownPilotLamp
		{
			get;
		}

		internal ReactiveCommand DownNeedle
		{
			get;
		}

		internal ReactiveCommand DownDigitalNumber
		{
			get;
		}

		internal ReactiveCommand DownDigitalGauge
		{
			get;
		}

		internal ReactiveCommand DownLinearGauge
		{
			get;
		}

		internal ReactiveCommand DownTimetable
		{
			get;
		}

		internal ReactiveCommand DownTouch
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
			CompositeDisposable listItemDisposable = new CompositeDisposable().AddTo(disposable);

			This = panel
				.ObserveProperty(x => x.This)
				.Do(_ => This?.Value.Dispose())
				.Select(x => new ThisViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			TreeItems = panel.TreeItems.ToReadOnlyReactiveCollection(x => new TreeViewItemViewModel(x, null)).AddTo(disposable);

			SelectedTreeItem = panel
				.ToReactivePropertyAsSynchronized(
					x => x.SelectedTreeItem,
					x => TreeItems.Select(y => y.SearchViewModel(x)).FirstOrDefault(y => y != null),
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
								.Select(_ => TreeItems[0].Children[1].Children.FirstOrDefault(z => z.Tag.Value == screen))
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

			UpScreen = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is Screen)
				.ToReactiveCommand()
				.WithSubscribe(panel.UpScreen)
				.AddTo(disposable);

			UpPilotLamp = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is PilotLampElement)
				.ToReactiveCommand()
				.WithSubscribe(panel.UpPanelElement<PilotLampElement>)
				.AddTo(disposable);

			UpNeedle = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is NeedleElement)
				.ToReactiveCommand()
				.WithSubscribe(panel.UpPanelElement<NeedleElement>)
				.AddTo(disposable);

			UpDigitalNumber = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is DigitalNumberElement)
				.ToReactiveCommand()
				.WithSubscribe(panel.UpPanelElement<DigitalNumberElement>)
				.AddTo(disposable);

			UpDigitalGauge = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is DigitalGaugeElement)
				.ToReactiveCommand()
				.WithSubscribe(panel.UpPanelElement<DigitalGaugeElement>)
				.AddTo(disposable);

			UpLinearGauge = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is LinearGaugeElement)
				.ToReactiveCommand()
				.WithSubscribe(panel.UpPanelElement<LinearGaugeElement>)
				.AddTo(disposable);

			UpTimetable = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is TimetableElement)
				.ToReactiveCommand()
				.WithSubscribe(panel.UpPanelElement<TimetableElement>)
				.AddTo(disposable);

			UpTouch = SelectedListItem
				.Select(x => ListItems.IndexOf(x) > 0 && x.Tag.Value is TouchElement)
				.ToReactiveCommand()
				.WithSubscribe(panel.UpTouch)
				.AddTo(disposable);

			DownScreen = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is Screen)
				.ToReactiveCommand()
				.WithSubscribe(panel.DownScreen)
				.AddTo(disposable);

			DownPilotLamp = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is PilotLampElement)
				.ToReactiveCommand()
				.WithSubscribe(panel.DownPanelElement<PilotLampElement>)
				.AddTo(disposable);

			DownNeedle = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is NeedleElement)
				.ToReactiveCommand()
				.WithSubscribe(panel.DownPanelElement<NeedleElement>)
				.AddTo(disposable);

			DownDigitalNumber = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is DigitalNumberElement)
				.ToReactiveCommand()
				.WithSubscribe(panel.DownPanelElement<DigitalNumberElement>)
				.AddTo(disposable);

			DownDigitalGauge = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is DigitalGaugeElement)
				.ToReactiveCommand()
				.WithSubscribe(panel.DownPanelElement<DigitalGaugeElement>)
				.AddTo(disposable);

			DownLinearGauge = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is LinearGaugeElement)
				.ToReactiveCommand()
				.WithSubscribe(panel.DownPanelElement<LinearGaugeElement>)
				.AddTo(disposable);

			DownTimetable = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is TimetableElement)
				.ToReactiveCommand()
				.WithSubscribe(panel.DownPanelElement<TimetableElement>)
				.AddTo(disposable);

			DownTouch = SelectedListItem
				.Select(x => ListItems.IndexOf(x) >= 0 && ListItems.IndexOf(x) < ListItems.Count - 1 && x.Tag.Value is TouchElement)
				.ToReactiveCommand()
				.WithSubscribe(panel.DownTouch)
				.AddTo(disposable);

			AddScreen = SelectedTreeItem
				.Select(x => x == TreeItems[0].Children[1])
				.ToReactiveCommand()
				.WithSubscribe(panel.AddScreen)
				.AddTo(disposable);

			AddPilotLamp = SelectedTreeItem
				.Select(x => TreeItems[0].Children[1].Children.Any(y => x == y.Children[0].Children[0])
							 || x == TreeItems[0].Children[2].Children[0])
				.ToReactiveCommand()
				.WithSubscribe(() => panel.AddPanelElement<PilotLampElement>(6))
				.AddTo(disposable);

			AddNeedle = SelectedTreeItem
				.Select(x => TreeItems[0].Children[1].Children.Any(y => x == y.Children[0].Children[1])
							 || x == TreeItems[0].Children[2].Children[1])
				.ToReactiveCommand()
				.WithSubscribe(() => panel.AddPanelElement<NeedleElement>(17))
				.AddTo(disposable);

			AddDigitalNumber = SelectedTreeItem
				.Select(x => TreeItems[0].Children[1].Children.Any(y => x == y.Children[0].Children[2])
							 || x == TreeItems[0].Children[2].Children[2])
				.ToReactiveCommand()
				.WithSubscribe(() => panel.AddPanelElement<DigitalNumberElement>(7))
				.AddTo(disposable);

			AddDigitalGauge = SelectedTreeItem
				.Select(x => TreeItems[0].Children[1].Children.Any(y => x == y.Children[0].Children[3])
							 || x == TreeItems[0].Children[2].Children[3])
				.ToReactiveCommand()
				.WithSubscribe(() => panel.AddPanelElement<DigitalGaugeElement>(10))
				.AddTo(disposable);

			AddLinearGauge = SelectedTreeItem
				.Select(x => TreeItems[0].Children[1].Children.Any(y => x == y.Children[0].Children[4])
							 || x == TreeItems[0].Children[2].Children[4])
				.ToReactiveCommand()
				.WithSubscribe(() => panel.AddPanelElement<LinearGaugeElement>(10))
				.AddTo(disposable);

			AddTimetable = SelectedTreeItem
				.Select(x => TreeItems[0].Children[1].Children.Any(y => x == y.Children[0].Children[5])
							 || x == TreeItems[0].Children[2].Children[5])
				.ToReactiveCommand()
				.WithSubscribe(() => panel.AddPanelElement<TimetableElement>(5))
				.AddTo(disposable);

			AddTouch = SelectedTreeItem
				.Select(x => TreeItems[0].Children[1].Children.Any(y => x == y.Children[1]))
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
				.WithSubscribe(() => panel.CopyPanelElement(6))
				.AddTo(disposable);

			CopyNeedle = SelectedNeedle
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(() => panel.CopyPanelElement(17))
				.AddTo(disposable);

			CopyDigitalNumber = SelectedDigitalNumber
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(() => panel.CopyPanelElement(7))
				.AddTo(disposable);

			CopyDigitalGauge = SelectedDigitalGauge
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(() => panel.CopyPanelElement(10))
				.AddTo(disposable);

			CopyLinearGauge = SelectedLinearGauge
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(() => panel.CopyPanelElement(10))
				.AddTo(disposable);

			CopyTimetable = SelectedTimetable
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(() => panel.CopyPanelElement(5))
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
				.WithSubscribe(panel.RemovePanelElement)
				.AddTo(disposable);

			RemoveNeedle = SelectedNeedle
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.RemovePanelElement)
				.AddTo(disposable);

			RemoveDigitalNumber = SelectedDigitalNumber
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.RemovePanelElement)
				.AddTo(disposable);

			RemoveDigitalGauge = SelectedDigitalGauge
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.RemovePanelElement)
				.AddTo(disposable);

			RemoveLinearGauge = SelectedLinearGauge
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.RemovePanelElement)
				.AddTo(disposable);

			RemoveTimetable = SelectedTimetable
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.RemovePanelElement)
				.AddTo(disposable);

			RemoveTouch = SelectedTouch
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(panel.RemoveTouch)
				.AddTo(disposable);
		}
	}
}
