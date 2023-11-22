using System;
using System.Linq;
using System.Reactive.Linq;
using OpenBveApi.Interface;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using TrainEditor2.Models;
using TrainEditor2.Models.Trains;
using TrainEditor2.Systems;
using TrainEditor2.ViewModels.Dialogs;
using TrainEditor2.ViewModels.Others;
using TrainEditor2.ViewModels.Sounds;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.ViewModels
{
	internal class AppViewModel : BaseViewModel
	{
		internal ReactiveProperty<string> CurrentLanguageCode
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<string> SaveLocation
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<MessageBoxViewModel> MessageBox
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<OpenFileDialogViewModel> OpenFileDialog
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<SaveFileDialogViewModel> SaveFileDialog
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<ImportTrainFileViewModel> ImportTrainFile
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<ImportPanelFileViewModel> ImportPanelFile
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<ImportSoundFileViewModel> ImportSoundFile
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<ExportTrainFileViewModel> ExportTrainFile
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<ExportPanelFileViewModel> ExportPanelFile
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<ExportSoundFileViewModel> ExportSoundFile
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<TrainViewModel> Train
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<SoundViewModel> Sound
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

		internal BooleanNotifier IsVisibleInfo
		{
			get;
		}

		internal BooleanNotifier IsVisibleWarning
		{
			get;
		}

		internal BooleanNotifier IsVisibleError
		{
			get;
		}

		internal BooleanNotifier IsVisibleCritical
		{
			get;
		}

		internal ReadOnlyReactiveCollection<ListViewItemViewModel> VisibleLogMessages
		{
			get;
		}

		internal ReactiveCommand CreateNewFile
		{
			get;
		}

		internal ReactiveCommand OpenFile
		{
			get;
		}

		internal ReactiveCommand SaveFile
		{
			get;
		}

		internal ReactiveCommand SaveAsFile
		{
			get;
		}

		internal ReactiveCommand OutputLogs
		{
			get;
		}

		internal ReactiveCommand UpCar
		{
			get;
		}

		internal ReactiveCommand DownCar
		{
			get;
		}

		internal ReactiveCommand AddCar
		{
			get;
		}

		internal ReactiveCommand RemoveCar
		{
			get;
		}

		internal ReactiveCommand CopyCar
		{
			get;
		}

		internal ReactiveCommand UpCoupler
		{
			get;
		}

		internal ReactiveCommand DownCoupler
		{
			get;
		}

		internal ReactiveCommand ChangeBaseCarClass
		{
			get;
		}

		internal ReactiveCommand ChangeControlledCarClass
		{
			get;
		}

		internal ReactiveCommand ChangeCabClass
		{
			get;
		}

		internal ReactiveCommand<MessageType> ChangeVisibleLogMessages
		{
			get;
		}

		internal ReactiveCommand ClearLogMessages
		{
			get;
		}

		internal AppViewModel()
		{
			App app = new App();

			CurrentLanguageCode = app
				.ToReactivePropertyAsSynchronized(x => x.CurrentLanguageCode)
				.AddTo(disposable);

			CurrentLanguageCode.Subscribe(_ => app.CreateTreeItem()).AddTo(disposable);

			SaveLocation = app
				.ObserveProperty(x => x.SaveLocation)
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			MessageBox = app
				.ObserveProperty(x => x.MessageBox)
				.Do(_ => MessageBox?.Value.Dispose())
				.Select(x => new MessageBoxViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			OpenFileDialog = app
				.ObserveProperty(x => x.OpenFileDialog)
				.Do(_ => OpenFileDialog?.Value.Dispose())
				.Select(x => new OpenFileDialogViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			SaveFileDialog = app
				.ObserveProperty(x => x.SaveFileDialog)
				.Do(_ => SaveFileDialog?.Value.Dispose())
				.Select(x => new SaveFileDialogViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			ImportTrainFile = app
				.ObserveProperty(x => x.ImportTrainFile)
				.Do(_ => ImportTrainFile?.Value.Dispose())
				.Select(x => new ImportTrainFileViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			ImportPanelFile = app
				.ObserveProperty(x => x.ImportPanelFile)
				.Do(_ => ImportPanelFile?.Value.Dispose())
				.Select(x => new ImportPanelFileViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			ImportSoundFile = app
				.ObserveProperty(x => x.ImportSoundFile)
				.Do(_ => ImportSoundFile?.Value.Dispose())
				.Select(x => new ImportSoundFileViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			ExportTrainFile = app
				.ObserveProperty(x => x.ExportTrainFile)
				.Do(_ => ExportTrainFile?.Value.Dispose())
				.Select(x => new ExportTrainFileViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			ExportPanelFile = app
				.ObserveProperty(x => x.ExportPanelFile)
				.Do(_ => ExportPanelFile?.Value.Dispose())
				.Select(x => new ExportPanelFileViewModel(x, app))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			ExportSoundFile = app
				.ObserveProperty(x => x.ExportSoundFile)
				.Do(_ => ExportSoundFile?.Value.Dispose())
				.Select(x => new ExportSoundFileViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Train = app
				.ObserveProperty(x => x.Train)
				.Do(_ => Train?.Value.Dispose())
				.Select(x => new TrainViewModel(x, app))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Sound = app
				.ObserveProperty(x => x.Sound)
				.Do(_ => Sound?.Value.Dispose())
				.Select(x => new SoundViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			TreeItems = app.TreeItems.ToReadOnlyReactiveCollection(x => new TreeViewItemViewModel(x, null)).AddTo(disposable);

			SelectedTreeItem = app
				.ToReactivePropertyAsSynchronized(
					x => x.SelectedTreeItem,
					x => TreeItems.Select(y => y.SearchViewModel(x)).FirstOrDefault(y => y != null),
					x => x?.Model,
					ReactivePropertyMode.RaiseLatestValueOnSubscribe
				)
				.AddTo(disposable);

			IsVisibleInfo = new BooleanNotifier();

			IsVisibleWarning = new BooleanNotifier();

			IsVisibleError = new BooleanNotifier();

			IsVisibleCritical = new BooleanNotifier();

			VisibleLogMessages = app.VisibleLogMessages
				.ToReadOnlyReactiveCollection(x => new ListViewItemViewModel(x))
				.AddTo(disposable);

			Interface.LogMessages
				.ObserveAddChanged()
				.Where(x =>
				{
					BooleanNotifier notifier;

					switch (x.Type)
					{
						case MessageType.Information:
							notifier = IsVisibleInfo;
							break;
						case MessageType.Warning:
							notifier = IsVisibleWarning;
							break;
						case MessageType.Error:
							notifier = IsVisibleError;
							break;
						case MessageType.Critical:
							notifier = IsVisibleCritical;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					return notifier.Value;
				})
				.Subscribe(app.AddLogMessage)
				.AddTo(disposable);

			Interface.LogMessages
				.ObserveRemoveChanged()
				.Subscribe(app.RemoveLogMessage)
				.AddTo(disposable);

			Interface.LogMessages
				.ObserveResetChanged()
				.Subscribe(_ => app.ResetLogMessages())
				.AddTo(disposable);

			CreateNewFile = new ReactiveCommand()
				.WithSubscribe(app.CreateNewFile)
				.AddTo(disposable);

			OpenFile = new ReactiveCommand()
				.WithSubscribe(app.OpenFile)
				.AddTo(disposable);

			SaveFile = SaveLocation
				.Select(x => !string.IsNullOrEmpty(x))
				.ToReactiveCommand()
				.WithSubscribe(app.SaveFile)
				.AddTo(disposable);

			SaveAsFile = new ReactiveCommand()
				.WithSubscribe(app.SaveAsFile)
				.AddTo(disposable);

			OutputLogs = new ReactiveCommand()
				.WithSubscribe(app.OutputLogs)
				.AddTo(disposable);

			UpCar = SelectedTreeItem
				.Select(x => TreeItems[0].Children[1].Children.IndexOf(x) > 0)
				.ToReactiveCommand()
				.WithSubscribe(app.UpCar)
				.AddTo(disposable);

			DownCar = SelectedTreeItem
				.Select(x => TreeItems[0].Children[1].Children.IndexOf(x))
				.Select(x => x >= 0 && x < TreeItems[0].Children[1].Children.Count - 1)
				.ToReactiveCommand()
				.WithSubscribe(app.DownCar)
				.AddTo(disposable);

			AddCar = SelectedTreeItem
				.Select(x => x == TreeItems[0].Children[1] || TreeItems[0].Children[1].Children.Contains(x))
				.ToReactiveCommand()
				.WithSubscribe(app.AddCar)
				.AddTo(disposable);

			RemoveCar = SelectedTreeItem
				.Select(x =>
					TreeItems[0].Children[1].Children.Contains(x)
					&& TreeItems[0].Children[1].Children.Where(y => y != x).Any(y => y.Tag.Value is MotorCar)
					&& TreeItems[0].Children[1].Children.Where(y => y != x).Any(y => y.Tag.Value is ControlledMotorCar || y.Tag.Value is ControlledTrailerCar)
				)
				.ToReactiveCommand()
				.WithSubscribe(app.RemoveCar)
				.AddTo(disposable);

			CopyCar = SelectedTreeItem
				.Select(x => TreeItems[0].Children[1].Children.Contains(x))
				.ToReactiveCommand()
				.WithSubscribe(app.CopyCar)
				.AddTo(disposable);

			UpCoupler = SelectedTreeItem
				.Select(x => TreeItems[0].Children[2].Children.IndexOf(x) > 0)
				.ToReactiveCommand()
				.WithSubscribe(app.UpCoupler)
				.AddTo(disposable);

			DownCoupler = SelectedTreeItem
				.Select(x => TreeItems[0].Children[2].Children.IndexOf(x))
				.Select(x => x >= 0 && x < TreeItems[0].Children[2].Children.Count - 1)
				.ToReactiveCommand()
				.WithSubscribe(app.DownCoupler)
				.AddTo(disposable);

			ChangeBaseCarClass = SelectedTreeItem
				.Select(x => x?.Tag.Value is TrailerCar || TreeItems[0].Children[1].Children.Count(y => y?.Tag.Value is MotorCar) > 1)
				.ToReactiveCommand()
				.WithSubscribe(app.ChangeBaseCarClass)
				.AddTo(disposable);

			ChangeControlledCarClass = SelectedTreeItem
				.Select(x =>
					x?.Tag.Value is UncontrolledMotorCar
					|| x?.Tag.Value is UncontrolledTrailerCar
					|| TreeItems[0].Children[1].Children.Count(y => y?.Tag.Value is ControlledMotorCar || y?.Tag.Value is ControlledTrailerCar) > 1
				)
				.ToReactiveCommand()
				.WithSubscribe(app.ChangeControlledCarClass)
				.AddTo(disposable);

			ChangeCabClass = SelectedTreeItem
				.Select(x => x?.Tag.Value is ControlledMotorCar || x?.Tag.Value is ControlledTrailerCar)
				.ToReactiveCommand()
				.WithSubscribe(app.ChangeCabClass)
				.AddTo(disposable);

			ChangeVisibleLogMessages = new ReactiveCommand<MessageType>()
				.WithSubscribe(x =>
				{
					BooleanNotifier notifier;

					switch (x)
					{
						case MessageType.Information:
							notifier = IsVisibleInfo;
							break;
						case MessageType.Warning:
							notifier = IsVisibleWarning;
							break;
						case MessageType.Error:
							notifier = IsVisibleError;
							break;
						case MessageType.Critical:
							notifier = IsVisibleCritical;
							break;
						default:
							throw new ArgumentOutOfRangeException(nameof(x), x, null);
					}

					app.ChangeVisibleLogMessages(x, !notifier.Value);

					notifier.SwitchValue();
				})
				.AddTo(disposable);

			ClearLogMessages = new ReactiveCommand()
				.WithSubscribe(Interface.LogMessages.Clear)
				.AddTo(disposable);
		}
	}
}
