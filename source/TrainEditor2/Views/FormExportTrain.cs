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
	public partial class FormExportTrain : Form
	{
		private readonly CompositeDisposable disposable;

		internal FormExportTrain(ExportTrainFileViewModel exportFile)
		{
			disposable = new CompositeDisposable();

			InitializeComponent();

			exportFile.SaveFileDialog.BindToSaveFileDialog(this).AddTo(disposable);

			exportFile.CurrentTrainFileType
				.BindTo(
					comboBoxType,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (TrainFileType)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxType.SelectedIndexChanged += h,
							h => comboBoxType.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			exportFile.CurrentTrainFileType
				.BindTo(
					groupBoxOldFormat,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == TrainFileType.OldFormat
				)
				.AddTo(disposable);

			exportFile.CurrentTrainFileType
				.BindTo(
					groupBoxNewFormat,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == TrainFileType.NewFormat
				)
				.AddTo(disposable);

			exportFile.TrainDatLocation
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

			exportFile.TrainDatLocation
				.BindToErrorProvider(errorProvider, textBoxTrainDatFileName)
				.AddTo(disposable);

			exportFile.ExtensionsCfgLocation
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

			exportFile.ExtensionsCfgLocation
				.BindToErrorProvider(errorProvider, textBoxExtensionsCfgFileName)
				.AddTo(disposable);

			exportFile.TrainXmlLocation
				.BindTo(
					textBoxTrainXmlFileName,
					x => x.Text,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => textBoxTrainXmlFileName.TextChanged += h,
							h => textBoxTrainXmlFileName.TextChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			exportFile.TrainXmlLocation
				.BindToErrorProvider(errorProvider, textBoxTrainXmlFileName)
				.AddTo(disposable);

			exportFile.SetTrainDatFile.BindToButton(buttonTrainDatFileNameOpen).AddTo(disposable);

			exportFile.SetExtensionsCfgFile.BindToButton(buttonExtensionsCfgFileNameOpen).AddTo(disposable);

			exportFile.SetTrainXmlFile.BindToButton(buttonTrainXmlFileNameOpen).AddTo(disposable);

			exportFile.Export.BindToButton(buttonOK).AddTo(disposable);
		}

		private void FormExportTrain_Load(object sender, EventArgs e)
		{
			Icon = WinFormsUtilities.GetIcon();
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
