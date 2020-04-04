using System.IO;
using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TrainEditor2.Models;
using TrainEditor2.Models.Trains;
using TrainEditor2.ViewModels.Dialogs;
using TrainEditor2.ViewModels.Others;

namespace TrainEditor2.ViewModels
{
	internal abstract class ImportFileViewModel : BaseViewModel
	{
		internal ReadOnlyReactivePropertySlim<OpenFileDialogViewModel> OpenFileDialog
		{
			get;
		}

		public ReactiveCommand Import
		{
			get;
			protected set;
		}

		protected ImportFileViewModel(ImportFile importFile)
		{
			OpenFileDialog = importFile
				.ObserveProperty(x => x.OpenFileDialog)
				.Do(_ => OpenFileDialog?.Value.Dispose())
				.Select(x => new OpenFileDialogViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);
		}
	}

	internal class ImportTrainFileViewModel : ImportFileViewModel
	{
		internal ReactiveProperty<TrainFileType> CurrentTrainFileType
		{
			get;
		}

		internal ReactiveProperty<string> TrainDatLocation
		{
			get;
		}

		internal ReactiveProperty<string> ExtensionsCfgLocation
		{
			get;
		}

		internal ReactiveProperty<string> TrainXmlLocation
		{
			get;
		}

		internal ReactiveCommand SetTrainDatFile
		{
			get;
		}

		internal ReactiveCommand SetExtensionsCfgFile
		{
			get;
		}

		internal ReactiveCommand SetTrainXmlFile
		{
			get;
		}

		internal ImportTrainFileViewModel(ImportTrainFile importFile) : base(importFile)
		{
			CurrentTrainFileType = importFile
				.ToReactivePropertyAsSynchronized(x => x.CurrentTrainFileType)
				.AddTo(disposable);

			TrainDatLocation = importFile
				.ToReactivePropertyAsSynchronized(
					x => x.TrainDatLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && !File.Exists(x) ? @"The specified file does not exist." : null)
				.AddTo(disposable);

			ExtensionsCfgLocation = importFile
				.ToReactivePropertyAsSynchronized(
					x => x.ExtensionsCfgLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && !File.Exists(x) ? @"The specified file does not exist." : null)
				.AddTo(disposable);

			TrainXmlLocation = importFile
				.ToReactivePropertyAsSynchronized(
					x => x.TrainXmlLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && !File.Exists(x) ? @"The specified file does not exist." : null)
				.AddTo(disposable);

			SetTrainDatFile = new ReactiveCommand().WithSubscribe(importFile.SetTrainDatFile).AddTo(disposable);

			SetExtensionsCfgFile = new ReactiveCommand().WithSubscribe(importFile.SetExtensionsCfgFile).AddTo(disposable);

			SetTrainXmlFile = new ReactiveCommand().WithSubscribe(importFile.SetTrainXmlFile).AddTo(disposable);

			Import = new[]
				{
					TrainDatLocation.ObserveHasErrors,
					ExtensionsCfgLocation.ObserveHasErrors,
					TrainXmlLocation.ObserveHasErrors
				}
				.CombineLatestValuesAreAllFalse()
				.ToReactiveCommand()
				.WithSubscribe(importFile.Import)
				.AddTo(disposable);
		}
	}

	internal class ImportPanelFileViewModel : ImportFileViewModel
	{
		internal ReactiveProperty<PanelFileType> CurrentPanelFileType
		{
			get;
		}

		internal ReactiveProperty<string> Panel2CfgLocation
		{
			get;
		}

		internal ReactiveProperty<string> PanelXmlLocation
		{
			get;
		}

		internal ReadOnlyReactiveCollection<ListViewItemViewModel> ImportCarsList
		{
			get;
		}

		internal ReactiveCommand UpdateImportCarsList
		{
			get;
		}

		internal ReactiveCommand SetPanel2CfgFile
		{
			get;
		}

		internal ReactiveCommand SetPanelXmlFile
		{
			get;
		}

		internal ImportPanelFileViewModel(ImportPanelFile importFile) : base(importFile)
		{
			CurrentPanelFileType = importFile
				.ToReactivePropertyAsSynchronized(x => x.CurrentPanelFileType)
				.AddTo(disposable);

			Panel2CfgLocation = importFile
				.ToReactivePropertyAsSynchronized(
					x => x.Panel2CfgLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && !File.Exists(x) ? @"The specified file does not exist." : null)
				.AddTo(disposable);

			PanelXmlLocation = importFile
				.ToReactivePropertyAsSynchronized(
					x => x.PanelXmlLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && !File.Exists(x) ? @"The specified file does not exist." : null)
				.AddTo(disposable);

			ImportCarsList = importFile.ImportCarsList
				.ToReadOnlyReactiveCollection(x => new ListViewItemViewModel(x))
				.AddTo(disposable);

			UpdateImportCarsList = new ReactiveCommand().WithSubscribe(importFile.UpdateImportCarsList).AddTo(disposable);

			SetPanel2CfgFile = new ReactiveCommand().WithSubscribe(importFile.SetPanel2CfgFile).AddTo(disposable);

			SetPanelXmlFile = new ReactiveCommand().WithSubscribe(importFile.SetPanelXmlFile).AddTo(disposable);

			Import = new[]
				{
					Panel2CfgLocation.ObserveHasErrors,
					PanelXmlLocation.ObserveHasErrors
				}
				.CombineLatestValuesAreAllFalse()
				.ToReactiveCommand()
				.WithSubscribe(importFile.Import)
				.AddTo(disposable);
		}
	}

	internal class ImportSoundFileViewModel : ImportFileViewModel
	{
		internal ReadOnlyReactivePropertySlim<OpenFolderDialogViewModel> OpenFolderDialog
		{
			get;
		}

		internal ReactiveProperty<SoundFileType> CurrentSoundFileType
		{
			get;
		}

		internal ReactiveProperty<string> TrainFolderLocation
		{
			get;
		}

		internal ReactiveProperty<string> SoundCfgLocation
		{
			get;
		}

		internal ReactiveProperty<string> SoundXmlLocation
		{
			get;
		}

		internal ReactiveCommand SetTrainFolder
		{
			get;
		}

		internal ReactiveCommand SetSoundCfgFile
		{
			get;
		}

		internal ReactiveCommand SetSoundXmlFile
		{
			get;
		}

		internal ImportSoundFileViewModel(ImportSoundFile importFile) : base(importFile)
		{
			OpenFolderDialog = importFile
				.ObserveProperty(x => x.OpenFolderDialog)
				.Do(_ => OpenFolderDialog?.Value.Dispose())
				.Select(x => new OpenFolderDialogViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);

			CurrentSoundFileType = importFile
				.ToReactivePropertyAsSynchronized(x => x.CurrentSoundFileType)
				.AddTo(disposable);

			TrainFolderLocation = importFile
				.ToReactivePropertyAsSynchronized(
					x => x.TrainFolderLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && !Directory.Exists(x) ? @"The specified folder does not exist." : null)
				.AddTo(disposable);

			SoundCfgLocation = importFile
				.ToReactivePropertyAsSynchronized(
					x => x.SoundCfgLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && !File.Exists(x) ? @"The specified file does not exist." : null)
				.AddTo(disposable);

			SoundXmlLocation = importFile
				.ToReactivePropertyAsSynchronized(
					x => x.SoundXmlLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && !File.Exists(x) ? @"The specified file does not exist." : null)
				.AddTo(disposable);

			SetTrainFolder = new ReactiveCommand().WithSubscribe(importFile.SetTrainFolder).AddTo(disposable);

			SetSoundCfgFile = new ReactiveCommand().WithSubscribe(importFile.SetSoundCfgFile).AddTo(disposable);

			SetSoundXmlFile = new ReactiveCommand().WithSubscribe(importFile.SetSoundXmlFile).AddTo(disposable);

			Import = new[]
				{
					TrainFolderLocation.ObserveHasErrors,
					SoundCfgLocation.ObserveHasErrors,
					SoundXmlLocation.ObserveHasErrors
				}
				.CombineLatestValuesAreAllFalse()
				.ToReactiveCommand()
				.WithSubscribe(importFile.Import)
				.AddTo(disposable);
		}
	}

	internal abstract class ExportFileViewModel : BaseViewModel
	{
		internal ReadOnlyReactivePropertySlim<SaveFileDialogViewModel> SaveFileDialog
		{
			get;
		}

		public ReactiveCommand Export
		{
			get;
			protected set;
		}

		protected ExportFileViewModel(ExportFile exportFile)
		{
			SaveFileDialog = exportFile
				.ObserveProperty(x => x.SaveFileDialog)
				.Do(_ => SaveFileDialog?.Value.Dispose())
				.Select(x => new SaveFileDialogViewModel(x))
				.ToReadOnlyReactivePropertySlim()
				.AddTo(disposable);
		}
	}

	internal class ExportTrainFileViewModel : ExportFileViewModel
	{
		internal ReactiveProperty<TrainFileType> CurrentTrainFileType
		{
			get;
		}

		internal ReactiveProperty<string> TrainDatLocation
		{
			get;
		}

		internal ReactiveProperty<string> ExtensionsCfgLocation
		{
			get;
		}

		internal ReactiveProperty<string> TrainXmlLocation
		{
			get;
		}

		internal ReactiveCommand SetTrainDatFile
		{
			get;
		}

		internal ReactiveCommand SetExtensionsCfgFile
		{
			get;
		}

		internal ReactiveCommand SetTrainXmlFile
		{
			get;
		}

		internal ExportTrainFileViewModel(ExportTrainFile exportFile) : base(exportFile)
		{
			CurrentTrainFileType = exportFile
				.ToReactivePropertyAsSynchronized(x => x.CurrentTrainFileType)
				.AddTo(disposable);

			TrainDatLocation = exportFile
				.ToReactivePropertyAsSynchronized(
					x => x.TrainDatLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && x.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ? @"The specified path contains characters that cannot be used." : null)
				.AddTo(disposable);

			ExtensionsCfgLocation = exportFile
				.ToReactivePropertyAsSynchronized(
					x => x.ExtensionsCfgLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && x.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ? @"The specified path contains characters that cannot be used." : null)
				.AddTo(disposable);

			TrainXmlLocation = exportFile
				.ToReactivePropertyAsSynchronized(
					x => x.TrainXmlLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && x.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ? @"The specified path contains characters that cannot be used." : null)
				.AddTo(disposable);

			SetTrainDatFile = new ReactiveCommand().WithSubscribe(exportFile.SetTrainDatFile).AddTo(disposable);

			SetExtensionsCfgFile = new ReactiveCommand().WithSubscribe(exportFile.SetExtensionsCfgFile).AddTo(disposable);

			SetTrainXmlFile = new ReactiveCommand().WithSubscribe(exportFile.SetTrainXmlFile).AddTo(disposable);

			Export = new[]
				{
					TrainDatLocation.ObserveHasErrors,
					ExtensionsCfgLocation.ObserveHasErrors,
					TrainXmlLocation.ObserveHasErrors
				}
				.CombineLatestValuesAreAllFalse()
				.ToReactiveCommand()
				.WithSubscribe(exportFile.Export)
				.AddTo(disposable);
		}
	}

	internal class ExportPanelFileViewModel : ExportFileViewModel
	{
		internal ReactiveProperty<PanelFileType> CurrentPanelFileType
		{
			get;
		}

		internal ReactiveProperty<string> Panel2CfgLocation
		{
			get;
		}

		internal ReactiveProperty<string> PanelXmlLocation
		{
			get;
		}

		internal ReactiveProperty<int> ExportTrainIndex
		{
			get;
		}

		internal ReactiveCommand SetPanel2CfgFile
		{
			get;
		}

		internal ReactiveCommand SetPanelXmlFile
		{
			get;
		}

		internal ExportPanelFileViewModel(ExportPanelFile exportFile, App app) : base(exportFile)
		{
			CurrentPanelFileType = exportFile
				.ToReactivePropertyAsSynchronized(x => x.CurrentPanelFileType)
				.AddTo(disposable);

			Panel2CfgLocation = exportFile
				.ToReactivePropertyAsSynchronized(
					x => x.Panel2CfgLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && x.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ? @"The specified path contains characters that cannot be used." : null)
				.AddTo(disposable);

			PanelXmlLocation = exportFile
				.ToReactivePropertyAsSynchronized(
					x => x.PanelXmlLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && x.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ? @"The specified path contains characters that cannot be used." : null)
				.AddTo(disposable);

			ExportTrainIndex = exportFile
				.ToReactivePropertyAsSynchronized(
					x => x.ExportTrainIndex,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => x < 0 || x >= app.Train.Cars.Count || !((app.Train.Cars[x] as ControlledMotorCar)?.Cab is EmbeddedCab) && !((app.Train.Cars[x] as ControlledTrailerCar)?.Cab is EmbeddedCab) ? @"The specified car does not have a embedded cab." : null)
				.AddTo(disposable);

			SetPanel2CfgFile = new ReactiveCommand().WithSubscribe(exportFile.SetPanel2CfgFile).AddTo(disposable);

			SetPanelXmlFile = new ReactiveCommand().WithSubscribe(exportFile.SetPanelXmlFile).AddTo(disposable);

			Export = new[]
				{
					Panel2CfgLocation.ObserveHasErrors,
					PanelXmlLocation.ObserveHasErrors,
					ExportTrainIndex.ObserveHasErrors
				}
				.CombineLatestValuesAreAllFalse()
				.ToReactiveCommand()
				.WithSubscribe(exportFile.Export)
				.AddTo(disposable);
		}
	}

	internal class ExportSoundFileViewModel : ExportFileViewModel
	{
		internal ReactiveProperty<SoundFileType> CurrentSoundFileType
		{
			get;
		}

		internal ReactiveProperty<string> SoundCfgLocation
		{
			get;
		}

		internal ReactiveProperty<string> SoundXmlLocation
		{
			get;
		}

		internal ReactiveCommand SetSoundCfgFile
		{
			get;
		}

		internal ReactiveCommand SetSoundXmlFile
		{
			get;
		}

		internal ExportSoundFileViewModel(ExportSoundFile exportFile) : base(exportFile)
		{
			CurrentSoundFileType = exportFile
				.ToReactivePropertyAsSynchronized(x => x.CurrentSoundFileType)
				.AddTo(disposable);

			SoundCfgLocation = exportFile
				.ToReactivePropertyAsSynchronized(
					x => x.SoundCfgLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && x.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ? @"The specified path contains characters that cannot be used." : null)
				.AddTo(disposable);

			SoundXmlLocation = exportFile
				.ToReactivePropertyAsSynchronized(
					x => x.SoundXmlLocation,
					ignoreValidationErrorValue: true
				)
				.SetValidateNotifyError(x => !string.IsNullOrEmpty(x) && x.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ? @"The specified path contains characters that cannot be used." : null)
				.AddTo(disposable);

			SetSoundCfgFile = new ReactiveCommand().WithSubscribe(exportFile.SetSoundCfgFile).AddTo(disposable);

			SetSoundXmlFile = new ReactiveCommand().WithSubscribe(exportFile.SetSoundXmlFile).AddTo(disposable);

			Export = new[]
				{
					SoundCfgLocation.ObserveHasErrors,
					SoundXmlLocation.ObserveHasErrors
				}
				.CombineLatestValuesAreAllFalse()
				.ToReactiveCommand()
				.WithSubscribe(exportFile.Export)
				.AddTo(disposable);
		}
	}
}
