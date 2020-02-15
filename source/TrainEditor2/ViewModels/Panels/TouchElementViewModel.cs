using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using OpenBveApi.Interface;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models.Panels;
using TrainEditor2.ViewModels.Others;

namespace TrainEditor2.ViewModels.Panels
{
	internal class TouchElementViewModel : BaseViewModel
	{
		internal class SoundEntryViewModel : BaseViewModel
		{
			internal ReactiveProperty<int> Index
			{
				get;
			}

			internal SoundEntryViewModel(TouchElement.SoundEntry entry)
			{
				Index = entry
					.ToReactivePropertyAsSynchronized(x => x.Index)
					.AddTo(disposable);
			}
		}

		internal class CommandEntryViewModel : BaseViewModel
		{
			internal ReactiveProperty<Translations.CommandInfo> Info
			{
				get;
			}

			internal ReactiveProperty<int> Option
			{
				get;
			}

			internal CommandEntryViewModel(TouchElement.CommandEntry entry)
			{
				Info = entry
					.ToReactivePropertyAsSynchronized(x => x.Info)
					.AddTo(disposable);

				Option = entry
					.ToReactivePropertyAsSynchronized(x => x.Option)
					.AddTo(disposable);
			}
		}

		internal ReactiveProperty<string> LocationX
		{
			get;
		}

		internal ReactiveProperty<string> LocationY
		{
			get;
		}

		internal ReactiveProperty<string> SizeX
		{
			get;
		}

		internal ReactiveProperty<string> SizeY
		{
			get;
		}

		internal ReactiveProperty<int> JumpScreen
		{
			get;
		}

		internal ReadOnlyReactiveCollection<SoundEntryViewModel> SoundEntries
		{
			get;
		}

		internal ReadOnlyReactiveCollection<CommandEntryViewModel> CommandEntries
		{
			get;
		}

		internal ReactiveProperty<int> Layer
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

		internal ReadOnlyReactivePropertySlim<SoundEntryViewModel> SelectedSoundEntry
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<CommandEntryViewModel> SelectedCommandEntry
		{
			get;
		}

		internal ReactiveCommand AddSoundEntry
		{
			get;
		}

		internal ReactiveCommand AddCommandEntry
		{
			get;
		}

		internal ReactiveCommand CopySoundEntry
		{
			get;
		}

		internal ReactiveCommand CopyCommandEntry
		{
			get;
		}

		internal ReactiveCommand RemoveSoundEntry
		{
			get;
		}

		internal ReactiveCommand RemoveCommandEntry
		{
			get;
		}

		internal TouchElementViewModel(TouchElement touch)
		{
			CultureInfo culture = CultureInfo.InvariantCulture;

			CompositeDisposable treeItemDisposable = new CompositeDisposable();
			CompositeDisposable listItemDisposable = new CompositeDisposable();

			LocationX = touch
				.ToReactivePropertyAsSynchronized(
					x => x.LocationX,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Any, out result, out message);

					return message;
				})
				.AddTo(disposable);

			LocationY = touch
				.ToReactivePropertyAsSynchronized(
					x => x.LocationY,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.Any, out result, out message);

					return message;
				})
				.AddTo(disposable);

			SizeX = touch
				.ToReactivePropertyAsSynchronized(
					x => x.SizeX,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.NonNegative, out result, out message);

					return message;
				})
				.AddTo(disposable);

			SizeY = touch
				.ToReactivePropertyAsSynchronized(
					x => x.SizeY,
					x => x.ToString(culture),
					x => double.Parse(x, NumberStyles.Float, culture),
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x =>
				{
					double result;
					string message;

					Utilities.TryParse(x, NumberRange.NonNegative, out result, out message);

					return message;
				})
				.AddTo(disposable);

			JumpScreen = touch
				.ToReactivePropertyAsSynchronized(x => x.JumpScreen)
				.AddTo(disposable);

			SoundEntries = touch.SoundEntries
				.ToReadOnlyReactiveCollection(x => new SoundEntryViewModel(x))
				.AddTo(disposable);

			CommandEntries = touch.CommandEntries
				.ToReadOnlyReactiveCollection(x => new CommandEntryViewModel(x))
				.AddTo(disposable);

			Layer = touch
				.ToReactivePropertyAsSynchronized(x => x.Layer)
				.AddTo(disposable);

			TreeItem = touch
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

			SelectedTreeItem = touch
				.ToReactivePropertyAsSynchronized(
					x => x.SelectedTreeItem,
					x => TreeItem.Value.SearchViewModel(x),
					x => x?.Model
				)
				.AddTo(disposable);

			ListColumns = touch.ListColumns
				.ToReadOnlyReactiveCollection(x => new ListViewColumnHeaderViewModel(x))
				.AddTo(disposable);

			ListItems = touch.ListItems
				.ToReadOnlyReactiveCollection(x => new ListViewItemViewModel(x))
				.AddTo(disposable);

			SelectedListItem = touch
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
					touch.CreateListColumns();
					touch.CreateListItems();
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
								.Subscribe(_ => touch.UpdateListItem(x.Model))
								.AddTo(tagDisposable);
						})
						.AddTo(listItemDisposable);

					tagDisposable.AddTo(listItemDisposable);
				})
				.AddTo(disposable);

			SelectedSoundEntry = SelectedListItem
				.Select(x => x?.Tag.Value as TouchElement.SoundEntry)
				.Do(_ => SelectedSoundEntry?.Value?.Dispose())
				.Select(x => x != null ? new SoundEntryViewModel(x) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SelectedCommandEntry = SelectedListItem
				.Select(x => x?.Tag.Value as TouchElement.CommandEntry)
				.Do(_ => SelectedCommandEntry?.Value?.Dispose())
				.Select(x => x != null ? new CommandEntryViewModel(x) : null)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			AddSoundEntry = SelectedTreeItem
				.Select(x => x == TreeItem.Value.Children[0])
				.ToReactiveCommand()
				.WithSubscribe(touch.AddSoundEntry)
				.AddTo(disposable);

			AddCommandEntry = SelectedTreeItem
				.Select(x => x == TreeItem.Value.Children[1])
				.ToReactiveCommand()
				.WithSubscribe(touch.AddCommandEntry)
				.AddTo(disposable);

			CopySoundEntry = SelectedSoundEntry
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(touch.CopySoundEntry)
				.AddTo(disposable);

			CopyCommandEntry = SelectedCommandEntry
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(touch.CopyCommandEntry)
				.AddTo(disposable);

			RemoveSoundEntry = SelectedSoundEntry
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(touch.RemoveSoundEntry)
				.AddTo(disposable);

			RemoveCommandEntry = SelectedCommandEntry
				.Select(x => x != null)
				.ToReactiveCommand()
				.WithSubscribe(touch.RemoveCommandEntry)
				.AddTo(disposable);
		}
	}
}
