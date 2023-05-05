using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models;
using TrainEditor2.ViewModels;

namespace TrainEditor2.Views
{
	public partial class FormImportSound : Form
	{
		private readonly CompositeDisposable disposable;

		internal FormImportSound(ImportSoundFileViewModel importFile)
		{
			disposable = new CompositeDisposable();

			InitializeComponent();

			importFile.OpenFileDialog.BindToOpenFileDialog(this).AddTo(disposable);

			importFile.OpenFolderDialog.BindToOpenFolderDialog(this).AddTo(disposable);

			importFile.CurrentSoundFileType
				.BindTo(
					comboBoxType,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (SoundFileType)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxType.SelectedIndexChanged += h,
							h => comboBoxType.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			importFile.CurrentSoundFileType
				.BindTo(
					groupBoxNoSoundCfg,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == SoundFileType.NoSettingFile
				)
				.AddTo(disposable);

			importFile.CurrentSoundFileType
				.BindTo(
					groupBoxSoundCfg,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == SoundFileType.SoundCfg
				)
				.AddTo(disposable);

			importFile.CurrentSoundFileType
				.BindTo(
					groupBoxSoundXml,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == SoundFileType.SoundXml
				)
				.AddTo(disposable);

			importFile.TrainFolderLocation
				.BindTo(
					textBoxNoSoundCfgTrainFolder,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxNoSoundCfgTrainFolder.TextChanged += h,
							h => textBoxNoSoundCfgTrainFolder.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			importFile.TrainFolderLocation
				.BindToErrorProvider(errorProvider, textBoxNoSoundCfgTrainFolder)
				.AddTo(disposable);

			importFile.SoundCfgLocation
				.BindTo(
					textBoxSoundCfgFileName,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxSoundCfgFileName.TextChanged += h,
							h => textBoxSoundCfgFileName.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			importFile.SoundCfgLocation
				.BindToErrorProvider(errorProvider, textBoxSoundCfgFileName)
				.AddTo(disposable);

			importFile.SoundXmlLocation
				.BindTo(
					textBoxSoundXmlFileName,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxSoundXmlFileName.TextChanged += h,
							h => textBoxSoundXmlFileName.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			importFile.SoundXmlLocation
				.BindToErrorProvider(errorProvider, textBoxSoundXmlFileName)
				.AddTo(disposable);

			importFile.SetTrainFolder.BindToButton(buttonNoSoundCfgTrainFolderOpen).AddTo(disposable);

			importFile.SetSoundCfgFile.BindToButton(buttonSoundCfgFileNameOpen).AddTo(disposable);

			importFile.SetSoundXmlFile.BindToButton(buttonSoundXmlFileNameOpen).AddTo(disposable);

			importFile.Import.BindToButton(buttonOK).AddTo(disposable);
		}

		private void FormImportSound_Load(object sender, EventArgs e)
		{
			Icon = WinFormsUtilities.GetIcon();
		}

		private void GroupBoxNoSoundCfg_DragEnter(object sender, DragEventArgs e)
		{
			WinFormsUtilities.CheckDragEnter(e, string.Empty);
		}

		private void GroupBoxSoundCfg_DragEnter(object sender, DragEventArgs e)
		{
			WinFormsUtilities.CheckDragEnter(e, ".cfg");
		}

		private void GroupBoxSoundXml_DragEnter(object sender, DragEventArgs e)
		{
			WinFormsUtilities.CheckDragEnter(e, ".xml");
		}

		private void GroupBoxNoSoundCfg_DragDrop(object sender, DragEventArgs e)
		{
			textBoxNoSoundCfgTrainFolder.Text = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).FirstOrDefault();
		}

		private void GroupBoxSoundCfg_DragDrop(object sender, DragEventArgs e)
		{
			textBoxSoundCfgFileName.Text = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).FirstOrDefault();
		}

		private void GroupBoxSoundXml_DragDrop(object sender, DragEventArgs e)
		{
			textBoxSoundXmlFileName.Text = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).FirstOrDefault();
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
