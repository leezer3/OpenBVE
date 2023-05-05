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
	public partial class FormImportTrain : Form
	{
		private readonly CompositeDisposable disposable;

		internal FormImportTrain(ImportTrainFileViewModel importFile)
		{
			disposable = new CompositeDisposable();

			InitializeComponent();

			importFile.OpenFileDialog.BindToOpenFileDialog(this).AddTo(disposable);

			importFile.CurrentTrainFileType
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

			importFile.CurrentTrainFileType
				.BindTo(
					groupBoxOldFormat,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == TrainFileType.OldFormat
				)
				.AddTo(disposable);

			importFile.CurrentTrainFileType
				.BindTo(
					groupBoxNewFormat,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == TrainFileType.NewFormat
				)
				.AddTo(disposable);

			importFile.TrainDatLocation
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

			importFile.TrainDatLocation
				.BindToErrorProvider(errorProvider, textBoxTrainDatFileName)
				.AddTo(disposable);

			importFile.ExtensionsCfgLocation
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

			importFile.ExtensionsCfgLocation
				.BindToErrorProvider(errorProvider, textBoxExtensionsCfgFileName)
				.AddTo(disposable);

			importFile.TrainXmlLocation
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

			importFile.TrainXmlLocation
				.BindToErrorProvider(errorProvider, textBoxTrainXmlFileName)
				.AddTo(disposable);

			importFile.SetTrainDatFile.BindToButton(buttonTrainDatFileNameOpen).AddTo(disposable);

			importFile.SetExtensionsCfgFile.BindToButton(buttonExtensionsCfgFileNameOpen).AddTo(disposable);

			importFile.SetTrainXmlFile.BindToButton(buttonTrainXmlFileNameOpen).AddTo(disposable);

			importFile.Import.BindToButton(buttonOK).AddTo(disposable);
		}

		private void FormImportTrain_Load(object sender, EventArgs e)
		{
			Icon = WinFormsUtilities.GetIcon();
		}

		private void GroupBoxTrainDat_DragEnter(object sender, DragEventArgs e)
		{
			WinFormsUtilities.CheckDragEnter(e, ".dat");
		}

		private void GroupBoxExtensionsCfg_DragEnter(object sender, DragEventArgs e)
		{
			WinFormsUtilities.CheckDragEnter(e, ".cfg");
		}

		private void GroupBoxTrainXml_DragEnter(object sender, DragEventArgs e)
		{
			WinFormsUtilities.CheckDragEnter(e, ".xml");
		}

		private void GroupBoxTrainDat_DragDrop(object sender, DragEventArgs e)
		{
			textBoxTrainDatFileName.Text = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).FirstOrDefault();
		}

		private void GroupBoxExtensionsCfg_DragDrop(object sender, DragEventArgs e)
		{
			textBoxExtensionsCfgFileName.Text = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).FirstOrDefault();
		}

		private void GroupBoxTrainXml_DragDrop(object sender, DragEventArgs e)
		{
			textBoxTrainXmlFileName.Text = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).FirstOrDefault();
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
