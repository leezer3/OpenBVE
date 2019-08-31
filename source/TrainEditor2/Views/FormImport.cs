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
	public partial class FormImport : Form
	{
		private readonly CompositeDisposable disposable;

		internal FormImport(AppViewModel app)
		{
			disposable = new CompositeDisposable();

			InitializeComponent();

			app.CurrentTrainFileType
				.BindTo(
					comboBoxTrainType,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (App.TrainFileType)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxTrainType.SelectedIndexChanged += h,
							h => comboBoxTrainType.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			app.CurrentTrainFileType
				.BindTo(
					groupBoxOldFormat,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == App.TrainFileType.OldFormat
				)
				.AddTo(disposable);

			app.TrainDatImportLocation
				.BindTo(
					textBoxTrainDatFileName,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxTrainDatFileName.TextChanged += h,
							h => textBoxTrainDatFileName.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			app.TrainDatImportLocation
				.BindToErrorProvider(errorProvider, textBoxTrainDatFileName)
				.AddTo(disposable);

			app.ExtensionsCfgImportLocation
				.BindTo(
					textBoxExtensionsCfgFileName,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxExtensionsCfgFileName.TextChanged += h,
							h => textBoxExtensionsCfgFileName.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			app.ExtensionsCfgImportLocation
				.BindToErrorProvider(errorProvider, textBoxExtensionsCfgFileName)
				.AddTo(disposable);

			app.CurrentPanelFileType
				.BindTo(
					comboBoxPanelType,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (App.PanelFileType)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxPanelType.SelectedIndexChanged += h,
							h => comboBoxPanelType.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			app.CurrentPanelFileType
				.BindTo(
					groupBoxPanel2Cfg,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == App.PanelFileType.Panel2Cfg
				)
				.AddTo(disposable);

			app.CurrentPanelFileType
				.BindTo(
					groupBoxPanelXml,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == App.PanelFileType.PanelXml
				)
				.AddTo(disposable);

			app.Panel2CfgImportLocation
				.BindTo(
					textBoxPanel2CfgFileName,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxPanel2CfgFileName.TextChanged += h,
							h => textBoxPanel2CfgFileName.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			app.Panel2CfgImportLocation
				.BindToErrorProvider(errorProvider, textBoxPanel2CfgFileName)
				.AddTo(disposable);

			app.PanelXmlImportLocation
				.BindTo(
					textBoxPanelXmlFileName,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxPanelXmlFileName.TextChanged += h,
							h => textBoxPanelXmlFileName.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			app.PanelXmlImportLocation
				.BindToErrorProvider(errorProvider, textBoxPanelXmlFileName)
				.AddTo(disposable);

			app.CurrentSoundFileType
				.BindTo(
					comboBoxSoundType,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (App.SoundFileType)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxSoundType.SelectedIndexChanged += h,
							h => comboBoxSoundType.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			app.CurrentSoundFileType
				.BindTo(
					groupBoxNoSoundCfg,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == App.SoundFileType.NoSettingFile
				)
				.AddTo(disposable);

			app.CurrentSoundFileType
				.BindTo(
					groupBoxSoundCfg,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == App.SoundFileType.SoundCfg
				)
				.AddTo(disposable);

			app.CurrentSoundFileType
				.BindTo(
					groupBoxSoundXml,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == App.SoundFileType.SoundXml
				)
				.AddTo(disposable);

			app.TrainFolderImportLocation
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

			app.TrainFolderImportLocation
				.BindToErrorProvider(errorProvider, textBoxNoSoundCfgTrainFolder)
				.AddTo(disposable);

			app.SoundCfgImportLocation
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

			app.SoundCfgImportLocation
				.BindToErrorProvider(errorProvider, textBoxSoundCfgFileName)
				.AddTo(disposable);

			app.SoundXmlImportLocation
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

			app.SoundXmlImportLocation
				.BindToErrorProvider(errorProvider, textBoxSoundXmlFileName)
				.AddTo(disposable);

			app.ImportFiles.BindToButton(buttonOK).AddTo(disposable);
		}

		private void FormImport_Load(object sender, EventArgs e)
		{
			Icon = FormEditor.GetIcon();
		}

		private void SetFileName(string filter, TextBox textBox)
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Filter = filter;
				dialog.CheckFileExists = true;

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					textBox.Text = dialog.FileName;
				}
			}
		}

		private void ButtonTrainDatFileNameOpen_Click(object sender, EventArgs e)
		{
			SetFileName(@"train.dat files|train.dat|All files|*", textBoxTrainDatFileName);
		}

		private void ButtonExtensionsCfgFileNameOpen_Click(object sender, EventArgs e)
		{
			SetFileName(@"extensions.cfg files|extensions.cfg|All files|*", textBoxExtensionsCfgFileName);
		}

		private void ButtonPanel2CfgFileNameOpen_Click(object sender, EventArgs e)
		{
			SetFileName(@"panel2.cfg files|panel2.cfg|All files|*", textBoxPanel2CfgFileName);
		}

		private void ButtonPanelXmlFileNameOpen_Click(object sender, EventArgs e)
		{
			SetFileName(@"panel.xml files|panel.xml|All files|*", textBoxPanelXmlFileName);
		}

		private void ButtonNoSoundCfgTrainFolderOpen_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog dialog = new FolderBrowserDialog())
			{
				dialog.Description = @"車両フォルダを指定してください。";
				dialog.ShowNewFolderButton = false;

				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					textBoxNoSoundCfgTrainFolder.Text = dialog.SelectedPath;
				}
			}
		}

		private void ButtonSoundCfgFileNameOpen_Click(object sender, EventArgs e)
		{
			SetFileName(@"sound.cfg files|sound.cfg|All files|*", textBoxSoundCfgFileName);
		}

		private void ButtonSoundXmlFileNameOpen_Click(object sender, EventArgs e)
		{
			SetFileName(@"sound.xml files|sound.xml|All files|*", textBoxSoundXmlFileName);
		}

		private void CheckDragEnter(DragEventArgs e, string extension)
		{
			string fileName = string.Empty;

			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				fileName = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).FirstOrDefault();
			}

			if (string.IsNullOrEmpty(fileName) || !fileName.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
			{
				e.Effect = DragDropEffects.None;
			}
			else
			{
				e.Effect = DragDropEffects.Copy;
			}
		}

		private void GroupBoxTrainDat_DragEnter(object sender, DragEventArgs e)
		{
			CheckDragEnter(e, ".dat");
		}

		private void GroupBoxExtensionsCfg_DragEnter(object sender, DragEventArgs e)
		{
			CheckDragEnter(e, ".cfg");
		}

		private void GroupBoxPanel2Cfg_DragEnter(object sender, DragEventArgs e)
		{
			CheckDragEnter(e, ".cfg");
		}

		private void GroupBoxPanelXml_DragEnter(object sender, DragEventArgs e)
		{
			CheckDragEnter(e, ".xml");
		}

		private void GroupBoxNoSoundCfg_DragEnter(object sender, DragEventArgs e)
		{
			CheckDragEnter(e, string.Empty);
		}

		private void GroupBoxSoundCfg_DragEnter(object sender, DragEventArgs e)
		{
			CheckDragEnter(e, ".cfg");
		}

		private void GroupBoxSoundXml_DragEnter(object sender, DragEventArgs e)
		{
			CheckDragEnter(e, ".xml");
		}

		private void GroupBoxTrainDat_DragDrop(object sender, DragEventArgs e)
		{
			textBoxTrainDatFileName.Text = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).FirstOrDefault();
		}

		private void GroupBoxExtensionsCfg_DragDrop(object sender, DragEventArgs e)
		{
			textBoxExtensionsCfgFileName.Text = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).FirstOrDefault();
		}

		private void GroupBoxPanel2Cfg_DragDrop(object sender, DragEventArgs e)
		{
			textBoxPanel2CfgFileName.Text = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).FirstOrDefault();
		}

		private void GroupBoxPanelXml_DragDrop(object sender, DragEventArgs e)
		{
			textBoxPanelXmlFileName.Text = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).FirstOrDefault();
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
