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
	public partial class FormImportPanel : Form
	{
		private readonly CompositeDisposable disposable;

		internal FormImportPanel(ImportPanelFileViewModel importFile)
		{
			disposable = new CompositeDisposable();

			InitializeComponent();

			importFile.OpenFileDialog.BindToOpenFileDialog(this).AddTo(disposable);

			importFile.CurrentPanelFileType
				.BindTo(
					comboBoxType,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					x => (int)x,
					x => (PanelFileType)x,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxType.SelectedIndexChanged += h,
							h => comboBoxType.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			importFile.CurrentPanelFileType
				.BindTo(
					groupBoxPanel2Cfg,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == PanelFileType.Panel2Cfg
				)
				.AddTo(disposable);

			importFile.CurrentPanelFileType
				.BindTo(
					groupBoxPanelXml,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == PanelFileType.PanelXml
				)
				.AddTo(disposable);

			importFile.Panel2CfgLocation
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

			importFile.Panel2CfgLocation
				.BindToErrorProvider(errorProvider, textBoxPanel2CfgFileName)
				.AddTo(disposable);

			importFile.PanelXmlLocation
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

			importFile.PanelXmlLocation
				.BindToErrorProvider(errorProvider, textBoxPanelXmlFileName)
				.AddTo(disposable);

			importFile.UpdateImportCarsList.Execute();

			WinFormsBinders.BindToListViewItemCollection(listViewCarsList, importFile.ImportCarsList, listViewCarsList.Items).AddTo(disposable);

			importFile.SetPanel2CfgFile.BindToButton(buttonPanel2CfgFileNameOpen).AddTo(disposable);

			importFile.SetPanelXmlFile.BindToButton(buttonPanelXmlFileNameOpen).AddTo(disposable);

			importFile.Import.BindToButton(buttonOK).AddTo(disposable);
		}

		private void FormImportPanel_Load(object sender, EventArgs e)
		{
			Icon = WinFormsUtilities.GetIcon();
		}

		private void GroupBoxPanel2Cfg_DragEnter(object sender, DragEventArgs e)
		{
			WinFormsUtilities.CheckDragEnter(e, ".cfg");
		}

		private void GroupBoxPanelXml_DragEnter(object sender, DragEventArgs e)
		{
			WinFormsUtilities.CheckDragEnter(e, ".xml");
		}

		private void GroupBoxPanel2Cfg_DragDrop(object sender, DragEventArgs e)
		{
			textBoxPanel2CfgFileName.Text = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).FirstOrDefault();
		}

		private void GroupBoxPanelXml_DragDrop(object sender, DragEventArgs e)
		{
			textBoxPanelXmlFileName.Text = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).FirstOrDefault();
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
