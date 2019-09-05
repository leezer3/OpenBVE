using System;
using System.Reactive.Linq;
using System.Windows.Forms;
using TrainEditor2.ViewModels.Dialogs;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToOpenFileDialog(OpenFileDialogViewModel x)
		{
			return x.IsOpen
				.Where(y => y)
				.Subscribe(_ =>
				{
					using (OpenFileDialog dialog = new OpenFileDialog())
					{
						dialog.Filter = x.Filter.Value;
						dialog.CheckFileExists = x.CheckFileExists.Value;

						switch (dialog.ShowDialog(this))
						{
							case DialogResult.OK:
							case DialogResult.Yes:
								x.FileName.Value = dialog.FileName;
								x.YesCommand.Execute();
								break;
							case DialogResult.No:
								x.NoCommand.Execute();
								break;
							case DialogResult.Cancel:
								x.CancelCommand.Execute();
								break;
						}
					}
				});
		}
	}
}
