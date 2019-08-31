using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
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
using TrainEditor2.ViewModels.Panels;
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

		internal ReactiveProperty<App.TrainFileType> CurrentTrainFileType
		{
			get;
		}

		internal ReactiveProperty<App.PanelFileType> CurrentPanelFileType
		{
			get;
		}

		internal ReactiveProperty<App.SoundFileType> CurrentSoundFileType
		{
			get;
		}

		internal ReactiveProperty<string> TrainDatImportLocation
		{
			get;
		}

		internal ReactiveProperty<string> TrainDatExportLocation
		{
			get;
		}

		internal ReactiveProperty<string> ExtensionsCfgImportLocation
		{
			get;
		}

		internal ReactiveProperty<string> ExtensionsCfgExportLocation
		{
			get;
		}

		internal ReactiveProperty<string> Panel2CfgImportLocation
		{
			get;
		}

		internal ReactiveProperty<string> Panel2CfgExportLocation
		{
			get;
		}

		internal ReactiveProperty<string> PanelXmlImportLocation
		{
			get;
		}

		internal ReactiveProperty<string> PanelXmlExportLocation
		{
			get;
		}

		internal ReactiveProperty<string> TrainFolderImportLocation
		{
			get;
		}

		internal ReactiveProperty<string> SoundCfgImportLocation
		{
			get;
		}

		internal ReactiveProperty<string> SoundCfgExportLocation
		{
			get;
		}

		internal ReactiveProperty<string> SoundXmlImportLocation
		{
			get;
		}

		internal ReactiveProperty<string> SoundXmlExportLocation
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<TrainViewModel> Train
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<PanelViewModel> Panel
		{
			get;
		}

		internal ReadOnlyReactivePropertySlim<SoundViewModel> Sound
		{
			get;
		}

		internal ReactiveProperty<TreeViewItemViewModel> Item
		{
			get;
		}

		internal ReactiveProperty<TreeViewItemViewModel> SelectedItem
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

		internal ReactiveCommand ImportFiles
		{
			get;
		}

		internal ReactiveCommand ExportFiles
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

		internal ReactiveCommand ChangeCarClass
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
			CompositeDisposable itemDisposable = new CompositeDisposable();

			App app = new App();

			CurrentLanguageCode = app
				.ToReactivePropertyAsSynchronized(x => x.CurrentLanguageCode)
				.AddTo(disposable);

			CurrentLanguageCode.Subscribe(_ => app.CreateItem()).AddTo(disposable);

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

			CurrentTrainFileType = app
				.ToReactivePropertyAsSynchronized(x => x.CurrentTrainFileType)
				.AddTo(disposable);

			CurrentPanelFileType = app
				.ToReactivePropertyAsSynchronized(x => x.CurrentPanelFileType)
				.AddTo(disposable);

			CurrentSoundFileType = app
				.ToReactivePropertyAsSynchronized(x => x.CurrentSoundFileType)
				.AddTo(disposable);

			TrainDatImportLocation = app
				.ToReactivePropertyAsSynchronized(
					x => x.TrainDatImportLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && !File.Exists(x) ? @"指定されたファイルは存在しません。" : null)
				.AddTo(disposable);

			TrainDatExportLocation = app
				.ToReactivePropertyAsSynchronized(
					x => x.TrainDatExportLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && x.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ? @"ファイル名に使用できない文字が使われています。" : null)
				.AddTo(disposable);

			ExtensionsCfgImportLocation = app
				.ToReactivePropertyAsSynchronized(
					x => x.ExtensionsCfgImportLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && !File.Exists(x) ? @"指定されたファイルは存在しません。" : null)
				.AddTo(disposable);

			ExtensionsCfgExportLocation = app
				.ToReactivePropertyAsSynchronized(
					x => x.ExtensionsCfgExportLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && x.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ? @"ファイル名に使用できない文字が使われています。" : null)
				.AddTo(disposable);

			Panel2CfgImportLocation = app
				.ToReactivePropertyAsSynchronized(
					x => x.Panel2CfgImportLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && !File.Exists(x) ? @"指定されたファイルは存在しません。" : null)
				.AddTo(disposable);

			Panel2CfgExportLocation = app
				.ToReactivePropertyAsSynchronized(
					x => x.Panel2CfgExportLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && x.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ? @"ファイル名に使用できない文字が使われています。" : null)
				.AddTo(disposable);

			PanelXmlImportLocation = app
				.ToReactivePropertyAsSynchronized(
					x => x.PanelXmlImportLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && !File.Exists(x) ? @"指定されたファイルは存在しません。" : null)
				.AddTo(disposable);

			PanelXmlExportLocation = app
				.ToReactivePropertyAsSynchronized(
					x => x.PanelXmlExportLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && x.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ? @"ファイル名に使用できない文字が使われています。" : null)
				.AddTo(disposable);

			TrainFolderImportLocation = app
				.ToReactivePropertyAsSynchronized(
					x => x.TrainFolderImportLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && !Directory.Exists(x) ? @"指定されたフォルダは存在しません。" : null)
				.AddTo(disposable);

			SoundCfgImportLocation = app
				.ToReactivePropertyAsSynchronized(
					x => x.SoundCfgImportLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && !File.Exists(x) ? @"指定されたファイルは存在しません。" : null)
				.AddTo(disposable);

			SoundCfgExportLocation = app
				.ToReactivePropertyAsSynchronized(
					x => x.SoundCfgExportLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && x.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ? @"ファイル名に使用できない文字が使われています。" : null)
				.AddTo(disposable);

			SoundXmlImportLocation = app
				.ToReactivePropertyAsSynchronized(
					x => x.SoundXmlImportLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && !File.Exists(x) ? @"指定されたファイルは存在しません。" : null)
				.AddTo(disposable);

			SoundXmlExportLocation = app
				.ToReactivePropertyAsSynchronized(
					x => x.SoundXmlExportLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && x.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ? @"ファイル名に使用できない文字が使われています。" : null)
				.AddTo(disposable);

			Train = app
				.ObserveProperty(x => x.Train)
				.Do(_ => Train?.Value.Dispose())
				.Select(x => new TrainViewModel(x, app))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Panel = app
				.ObserveProperty(x => x.Panel)
				.Do(_ => Panel?.Value.Dispose())
				.Select(x => new PanelViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Sound = app
				.ObserveProperty(x => x.Sound)
				.Do(_ => Sound?.Value.Dispose())
				.Select(x => new SoundViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			Item = app
				.ObserveProperty(x => x.Item)
				.Do(_ => Item?.Value.Dispose())
				.Select(x => new TreeViewItemViewModel(x))
				.ToReactiveProperty()
				.AddTo(disposable);

			Item.Subscribe(x =>
				{
					itemDisposable.Dispose();
					itemDisposable = new CompositeDisposable();

					x.PropertyChangedAsObservable()
						.Subscribe(_ => Item.ForceNotify())
						.AddTo(itemDisposable);
				})
				.AddTo(disposable);

			SelectedItem = app
				.ToReactivePropertyAsSynchronized(
					x => x.SelectedItem,
					x => Item.Value.SearchViewModel(x),
					x => x?.Model
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

			ImportFiles = new[]
				{
					TrainDatImportLocation.ObserveHasErrors,
					ExtensionsCfgImportLocation.ObserveHasErrors,
					Panel2CfgImportLocation.ObserveHasErrors,
					PanelXmlImportLocation.ObserveHasErrors,
					TrainFolderImportLocation.ObserveHasErrors,
					SoundCfgImportLocation.ObserveHasErrors,
					SoundXmlImportLocation.ObserveHasErrors
				}
				.CombineLatestValuesAreAllFalse()
				.ToReactiveCommand()
				.WithSubscribe(app.ImportFiles)
				.AddTo(disposable);

			ExportFiles = new[]
				{
					TrainDatExportLocation.ObserveHasErrors,
					ExtensionsCfgExportLocation.ObserveHasErrors,
					Panel2CfgExportLocation.ObserveHasErrors,
					PanelXmlExportLocation.ObserveHasErrors,
					SoundCfgExportLocation.ObserveHasErrors,
					SoundXmlExportLocation.ObserveHasErrors
				}
				.CombineLatestValuesAreAllFalse()
				.ToReactiveCommand()
				.WithSubscribe(app.ExportFiles)
				.AddTo(disposable);

			UpCar = SelectedItem
				.Select(x => Item.Value.Children[1].Children.IndexOf(x) > 0)
				.ToReactiveCommand()
				.WithSubscribe(app.UpCar)
				.AddTo(disposable);

			DownCar = SelectedItem
				.Select(x => Item.Value.Children[1].Children.IndexOf(x))
				.Select(x => x >= 0 && x < Item.Value.Children[1].Children.Count - 1)
				.ToReactiveCommand()
				.WithSubscribe(app.DownCar)
				.AddTo(disposable);

			AddCar = SelectedItem
				.Select(x => x == Item.Value.Children[1] || Item.Value.Children[1].Children.Contains(x))
				.ToReactiveCommand()
				.WithSubscribe(app.AddCar)
				.AddTo(disposable);

			RemoveCar = SelectedItem
				.Select(x => Item.Value.Children[1].Children.Contains(x) && Item.Value.Children[1].Children.Where(y => y != x).Any(y => y.Tag.Value is MotorCar))
				.ToReactiveCommand()
				.WithSubscribe(app.RemoveCar)
				.AddTo(disposable);

			CopyCar = SelectedItem
				.Select(x => Item.Value.Children[1].Children.Contains(x))
				.ToReactiveCommand()
				.WithSubscribe(app.CopyCar)
				.AddTo(disposable);

			UpCoupler = SelectedItem
				.Select(x => Item.Value.Children[2].Children.IndexOf(x) > 0)
				.ToReactiveCommand()
				.WithSubscribe(app.UpCoupler)
				.AddTo(disposable);

			DownCoupler = SelectedItem
				.Select(x => Item.Value.Children[2].Children.IndexOf(x))
				.Select(x => x >= 0 && x < Item.Value.Children[2].Children.Count - 1)
				.ToReactiveCommand()
				.WithSubscribe(app.DownCoupler)
				.AddTo(disposable);

			ChangeCarClass = SelectedItem
				.Select(x => x?.Tag.Value is TrailerCar || Item.Value.Children[1].Children.Count(y => y?.Tag.Value is MotorCar) > 1)
				.ToReactiveCommand()
				.WithSubscribe(() => app.ChangeCarClass(app.Train.Cars.IndexOf((Car)SelectedItem.Value.Tag.Value)))
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

			itemDisposable.AddTo(disposable);
		}
	}
}
