using System;
using System.Reactive.Linq;
using System.Windows.Forms;
using TrainEditor2.ViewModels.Dialogs;

namespace TrainEditor2.Views
{
	public partial class FormEditor
	{
		private IDisposable BindToSaveFileDialog(SaveFileDialogViewModel x)
		{
			return x.IsOpen
				.Where(y => y)
				.Subscribe(_ =>
				{
					using (SaveFileDialog dialog = new SaveFileDialog())
					{
						dialog.Filter = x.Filter.Value;
						dialog.OverwritePrompt = x.OverwritePrompt.Value;

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
