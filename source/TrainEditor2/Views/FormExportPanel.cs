using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using Reactive.Bindings.Binding;
using Reactive.Bindings.Extensions;
using TrainEditor2.Extensions;
using TrainEditor2.Models;
using TrainEditor2.ViewModels;
using TrainEditor2.ViewModels.Trains;

namespace TrainEditor2.Views
{
	public partial class FormExportPanel : Form
	{
		private readonly CompositeDisposable disposable;

		internal FormExportPanel(ExportPanelFileViewModel exportFile, IEnumerable<CarViewModel> cars)
		{
			disposable = new CompositeDisposable();

			InitializeComponent();

			exportFile.SaveFileDialog.BindToSaveFileDialog(this).AddTo(disposable);

			exportFile.CurrentPanelFileType
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

			exportFile.CurrentPanelFileType
				.BindTo(
					groupBoxPanel2Cfg,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == PanelFileType.Panel2Cfg
				)
				.AddTo(disposable);

			exportFile.CurrentPanelFileType
				.BindTo(
					groupBoxPanelXml,
					x => x.Enabled,
					BindingMode.OneWay,
					x => x == PanelFileType.PanelXml
				)
				.AddTo(disposable);

			exportFile.Panel2CfgLocation
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

			exportFile.Panel2CfgLocation
				.BindToErrorProvider(errorProvider, textBoxPanel2CfgFileName)
				.AddTo(disposable);

			exportFile.PanelXmlLocation
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

			exportFile.PanelXmlLocation
				.BindToErrorProvider(errorProvider, textBoxPanelXmlFileName)
				.AddTo(disposable);

			comboBoxTrainIndex.Items.AddRange(Enumerable.Range(0, cars.Count()).OfType<object>().ToArray());

			exportFile.ExportTrainIndex
				.BindTo(
					comboBoxTrainIndex,
					x => x.SelectedIndex,
					BindingMode.TwoWay,
					null,
					null,
					Observable.FromEvent<EventHandler, EventArgs>(
							h => (s, e) => h(e),
							h => comboBoxTrainIndex.SelectedIndexChanged += h,
							h => comboBoxTrainIndex.SelectedIndexChanged -= h
						)
						.ToUnit()
				)
				.AddTo(disposable);

			exportFile.ExportTrainIndex
				.BindToErrorProvider(errorProvider, comboBoxTrainIndex)
				.AddTo(disposable);

			exportFile.SetPanel2CfgFile.BindToButton(buttonPanel2CfgFileNameOpen).AddTo(disposable);

			exportFile.SetPanelXmlFile.BindToButton(buttonPanelXmlFileNameOpen).AddTo(disposable);

			exportFile.Export.BindToButton(buttonOK).AddTo(disposable);
		}

		private void FormExportPanel_Load(object sender, EventArgs e)
		{
			Icon = WinFormsUtilities.GetIcon();
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
