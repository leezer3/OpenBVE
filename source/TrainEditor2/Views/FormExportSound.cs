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
	public partial class FormExportSound : Form
	{
		private readonly CompositeDisposable disposable;

		internal FormExportSound(ExportSoundFileViewModel exportFile)
		{
			disposable = new CompositeDisposable();

			InitializeComponent();

			exportFile.SaveFileDialog.BindToSaveFileDialog(this).AddTo(disposable);

			exportFile.CurrentSoundFileType
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

			exportFile.CurrentSoundFileType
				.BindTo(
					groupBoxSoundCfg,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == SoundFileType.SoundCfg
				)
				.AddTo(disposable);

			exportFile.CurrentSoundFileType
				.BindTo(
					groupBoxSoundXml,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == SoundFileType.SoundXml
				)
				.AddTo(disposable);

			exportFile.SoundCfgLocation
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

			exportFile.SoundCfgLocation
				.BindToErrorProvider(errorProvider, textBoxSoundCfgFileName)
				.AddTo(disposable);

			exportFile.SoundXmlLocation
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

			exportFile.SoundXmlLocation
				.BindToErrorProvider(errorProvider, textBoxSoundXmlFileName)
				.AddTo(disposable);

			exportFile.SetSoundCfgFile.BindToButton(buttonSoundCfgFileNameOpen).AddTo(disposable);

			exportFile.SetSoundXmlFile.BindToButton(buttonSoundXmlFileNameOpen).AddTo(disposable);

			exportFile.Export.BindToButton(buttonOK).AddTo(disposable);
		}

		private void FormExportSound_Load(object sender, EventArgs e)
		{
			Icon = WinFormsUtilities.GetIcon();
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
