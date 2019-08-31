using System;
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
	public partial class FormExport : Form
	{
		private readonly CompositeDisposable disposable;

		internal FormExport(AppViewModel app)
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

			app.TrainDatExportLocation
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

			app.TrainDatExportLocation
				.BindToErrorProvider(errorProvider, textBoxTrainDatFileName)
				.AddTo(disposable);

			app.ExtensionsCfgExportLocation
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

			app.ExtensionsCfgExportLocation
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

			app.Panel2CfgExportLocation
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

			app.Panel2CfgExportLocation
				.BindToErrorProvider(errorProvider, textBoxPanel2CfgFileName)
				.AddTo(disposable);

			app.PanelXmlExportLocation
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

			app.PanelXmlExportLocation
				.BindToErrorProvider(errorProvider, textBoxPanelXmlFileName)
				.AddTo(disposable);

			app.CurrentSoundFileType
				.BindTo(
					comboBoxSoundType,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x - 1,
					x => (App.SoundFileType)(x + 1),
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

			app.SoundCfgExportLocation
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

			app.SoundCfgExportLocation
				.BindToErrorProvider(errorProvider, textBoxSoundCfgFileName)
				.AddTo(disposable);

			app.SoundXmlExportLocation
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

			app.SoundXmlExportLocation
				.BindToErrorProvider(errorProvider, textBoxSoundXmlFileName)
				.AddTo(disposable);

			app.ExportFiles.BindToButton(buttonOK).AddTo(disposable);
		}

		private void FormExport_Load(object sender, EventArgs e)
		{
			Icon = FormEditor.GetIcon();
		}

		private void SetFileName(string filter, TextBox textBox)
		{
			using (SaveFileDialog dialog = new SaveFileDialog())
			{
				dialog.Filter = filter;
				dialog.OverwritePrompt = true;

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

		private void ButtonSoundCfgFileNameOpen_Click(object sender, EventArgs e)
		{
			SetFileName(@"sound.cfg files|sound.cfg|All files|*", textBoxSoundCfgFileName);
		}

		private void ButtonSoundXmlFileNameOpen_Click(object sender, EventArgs e)
		{
			SetFileName(@"sound.xml files|sound.xml|All files|*", textBoxSoundXmlFileName);
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
